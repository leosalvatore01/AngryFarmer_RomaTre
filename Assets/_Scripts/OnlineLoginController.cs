using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Schermata pronta per un futuro provider. L'accesso online resta disattivato.</summary>
public sealed class OnlineLoginController : MonoBehaviour
{
    public static OnlineLoginController Instance { get; private set; }
    public static bool BloccaInputMenu => Instance != null &&
        (Instance.gameObject.activeSelf || Time.frameCount == Instance.frameChiusura);
    private MenuInizialeController menu;
    private CanvasGroup gruppoMenu;
    private TMP_InputField email, password, conferma;
    private TMP_Text titolo, messaggio, testoInvia, testoMostra, testoRecupero;
    private GameObject rigaPassword, rigaConferma;
    private Button invia, ospite, accediTab, registraTab, recupero;
    private ILoginProvider provider;
    private System.Func<EsitoAccesso, bool> attivaProfiloAccount;
    private bool registrazione, recupera, occupato, passwordVisibile;
    private int frameChiusura = -1;
    public bool Aperto => gameObject.activeSelf;

    public static void Mostra(MenuInizialeController proprietario)
    {
        if (Instance == null)
        {
            GameObject radice = new GameObject("AccessoIniziale", typeof(RectTransform));
            radice.transform.SetParent(proprietario.transform, false);
            Instance = radice.AddComponent<OnlineLoginController>();
            Instance.menu = proprietario;
            Canvas menuCanvas = proprietario.GetComponentInChildren<Canvas>();
            Instance.gruppoMenu = menuCanvas.GetComponent<CanvasGroup>();
            if (Instance.gruppoMenu == null)
                Instance.gruppoMenu = menuCanvas.gameObject.AddComponent<CanvasGroup>();
            Instance.Costruisci();
        }
        Instance.gameObject.SetActive(true);
        Instance.gruppoMenu.interactable = false;
        Instance.gruppoMenu.blocksRaycasts = false;
        Instance.gruppoMenu.alpha = 0f;
        Instance.email.SetTextWithoutNotify("");
        Instance.PulisciPassword();
        Instance.ImpostaModalita(false, false);
        Instance.ImpostaMessaggio("Accesso online non attivo.\nGioca come ospite per entrare.", false);
        if (!MobileExperience.Attiva) Instance.email.Select();
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    public void ContinuaComeOspite()
    {
        if (occupato) return;
        LoginSession.ContinuaComeOspite();
        Chiudi();
    }

    private void Chiudi()
    {
        PulisciPassword();
        gruppoMenu.alpha = 1f;
        gruppoMenu.interactable = true;
        gruppoMenu.blocksRaycasts = true;
        frameChiusura = Time.frameCount;
        gameObject.SetActive(false);
        menu.TornaAlPannelloPrincipale();
        menu.AggiornaProfiloDopoAccesso();
        FarmAudioController.RiproduciInterfaccia();
    }

    private void ImpostaModalita(bool crea, bool reset)
    {
        if (occupato) return;
        registrazione = crea;
        recupera = reset;
        titolo.text = reset ? "RECUPERA PASSWORD" : crea ? "CREA IL TUO ACCOUNT" : "BENTORNATO IN FATTORIA";
        rigaPassword.SetActive(!reset);
        rigaConferma.SetActive(crea && !reset);
        invia.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, crea ? -190f : -95f);
        recupero.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, crea ? -260f : -170f);
        testoInvia.text = reset ? "INVIA LINK DI RECUPERO" : crea ? "CREA ACCOUNT" : "ACCEDI";
        testoRecupero.text = reset || crea ? "TORNA ALL'ACCESSO" : "PASSWORD DIMENTICATA?";
        messaggio.rectTransform.anchoredPosition = new Vector2(0f, crea ? -316f : -270f);
        messaggio.rectTransform.sizeDelta = new Vector2(610f, crea ? 60f : 130f);
        accediTab.GetComponent<Image>().color = !crea ? FarmPixelUI.ColorePulsanteOroFlat : FarmPixelUI.ColorePulsanteNeutroFlat;
        registraTab.GetComponent<Image>().color = crea ? FarmPixelUI.ColorePulsanteOroFlat : FarmPixelUI.ColorePulsanteNeutroFlat;
        PulisciPassword();
        ImpostaMessaggio(crea ? "Almeno 8 caratteri. Scegli una password personale." : "", false);
    }

    private void PulisciPassword()
    {
        password?.SetTextWithoutNotify("");
        conferma?.SetTextWithoutNotify("");
        passwordVisibile = false;
        if (password != null) password.contentType = TMP_InputField.ContentType.Password;
        if (testoMostra != null) testoMostra.text = "MOSTRA";
    }

    public void Invia()
    {
        if (occupato) return;
        OperazioneAccesso operazione = recupera ? OperazioneAccesso.RecuperaPassword :
            registrazione ? OperazioneAccesso.CreaAccount : OperazioneAccesso.Accedi;
        string errore = ValidazioneAccesso.Errore(operazione, email.text, password.text, conferma.text);
        if (!string.IsNullOrEmpty(errore)) { ImpostaMessaggio(errore, true); return; }
        if (provider == null || !provider.Disponibile)
        {
            PulisciPassword();
            ImpostaMessaggio("Accesso online non attivo. Per ora puoi giocare come ospite.", false);
            return;
        }
        StartCoroutine(Esegui());
    }

    private IEnumerator Esegui()
    {
        occupato = true;
        ImpostaControlli(false);
        ImpostaMessaggio(recupera ? "INVIO IN CORSO..." : "CONNESSIONE IN CORSO...", false);
        EsitoAccesso esito = null;
        OperazioneAccesso operazione = recupera ? OperazioneAccesso.RecuperaPassword :
            registrazione ? OperazioneAccesso.CreaAccount : OperazioneAccesso.Accedi;
        yield return provider.Esegui(operazione, email.text.Trim(), password.text, valore => esito = valore);
        occupato = false;
        ImpostaControlli(true);
        PulisciPassword();
        if (esito == null) { ImpostaMessaggio("Operazione non riuscita. Riprova.", true); yield break; }
        if (!esito.Riuscito || recupera) { ImpostaMessaggio(esito.Messaggio, !esito.Riuscito); yield break; }
        // Il futuro collegamento deve anche selezionare il profilo corretto prima di entrare.
        if (attivaProfiloAccount == null || !attivaProfiloAccount(esito))
        { ImpostaMessaggio("Accesso verificato, ma il profilo locale non si apre. Riprova.", true); yield break; }
        Chiudi();
    }

    private void ImpostaControlli(bool attivi)
    {
        foreach (Selectable controllo in GetComponentsInChildren<Selectable>(true)) controllo.interactable = attivi;
    }

    private void ImpostaMessaggio(string testo, bool errore)
    {
        messaggio.text = testo;
        messaggio.color = errore ? new Color32(255, 177, 148, 255) : new Color32(216, 222, 179, 255);
    }

    public void ConfiguraProvider(ILoginProvider valore, System.Func<EsitoAccesso, bool> attivaProfilo)
    {
        if (occupato) return;
        provider = valore ?? new LoginDisattivatoProvider();
        attivaProfiloAccount = attivaProfilo;
    }

    private void Costruisci()
    {
        provider = new LoginDisattivatoProvider();
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2800;
        canvas.pixelPerfect = true;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        gameObject.AddComponent<GraphicRaycaster>();
        gameObject.AddComponent<MobileSafeAreaCanvas>();
        GameObject sfondo = Pannello("SfondoAccesso", transform, Vector2.zero, Vector2.zero, new Color32(36, 54, 37, 255));
        RectTransform pieno = sfondo.GetComponent<RectTransform>();
        pieno.anchorMin = Vector2.zero; pieno.anchorMax = Vector2.one;
        pieno.offsetMin = pieno.offsetMax = Vector2.zero;
        GameObject cornice = Pannello("CorniceAccesso", transform, Vector2.zero, new Vector2(1480, 920), new Color32(48, 67, 43, 255));
        Testo("Logo", cornice.transform, "ANGRY\nFARMER", new Vector2(-420, 280), new Vector2(520, 190), 72, FarmPixelUI.TestoTitoloFlat);
        Testo("Invito", cornice.transform, "LA FATTORIA TI ASPETTA", new Vector2(-420, 130), new Vector2(550, 56), 25, FarmPixelUI.TestoChiaroFlat);
        Sprite[] farmer = Resources.LoadAll<Sprite>("FarmerWalk-v1");
        GameObject ritratto = Pannello("RitrattoContadino", cornice.transform, new Vector2(-420, -65), new Vector2(265, 285), Color.white);
        Image immagine = ritratto.GetComponent<Image>();
        if (farmer.Length > 0) immagine.sprite = farmer[0];
        immagine.preserveAspect = true; immagine.raycastTarget = false;
        Testo("MottoAccesso", cornice.transform, "UN CONTADINO.\nINFINITE ONDATE.", new Vector2(-420, -300), new Vector2(500, 100), 27, new Color32(187, 205, 159, 255));
        Testo("StatoOnline", cornice.transform, "ACCOUNT ONLINE · PROSSIMAMENTE", new Vector2(-420, -395), new Vector2(550, 36), 17, new Color32(187, 205, 159, 255));

        GameObject scheda = Pannello("SchedaAccesso", cornice.transform, new Vector2(325, 0), new Vector2(720, 860), new Color32(35, 34, 26, 255));
        titolo = Testo("TitoloAccesso", scheda.transform, "", new Vector2(0, 350), new Vector2(650, 60), 28, FarmPixelUI.TestoTitoloFlat);
        accediTab = Pulsante("TabAccedi", scheda.transform, "ACCEDI", new Vector2(-155, 267), new Vector2(295, 58), () => ImpostaModalita(false, false));
        registraTab = Pulsante("TabRegistrati", scheda.transform, "CREA ACCOUNT", new Vector2(155, 267), new Vector2(295, 58), () => ImpostaModalita(true, false));
        email = Campo(scheda.transform, "EmailAccesso", "EMAIL", new Vector2(0, 145), false);
        password = Campo(scheda.transform, "PasswordAccesso", "PASSWORD", new Vector2(0, 30), true);
        rigaPassword = password.transform.parent.gameObject;
        conferma = Campo(scheda.transform, "ConfermaPasswordAccesso", "RIPETI PASSWORD", new Vector2(0, -85), true);
        rigaConferma = conferma.transform.parent.gameObject;
        Button mostra = Pulsante("MostraPassword", password.transform, "MOSTRA", new Vector2(238, 0), new Vector2(105, 42), () =>
        {
            passwordVisibile = !passwordVisibile;
            password.contentType = passwordVisibile ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            password.ForceLabelUpdate();
            testoMostra.text = passwordVisibile ? "NASCONDI" : "MOSTRA";
        });
        testoMostra = mostra.GetComponentInChildren<TMP_Text>();
        invia = Pulsante("InviaAccesso", scheda.transform, "ACCEDI", new Vector2(0, -95), new Vector2(610, 76), Invia);
        invia.GetComponent<Image>().color = FarmPixelUI.ColorePulsanteVerdeFlat;
        testoInvia = invia.GetComponentInChildren<TMP_Text>();
        recupero = Pulsante("RecuperaPassword", scheda.transform, "PASSWORD DIMENTICATA?", new Vector2(0, -170), new Vector2(610, 48), () =>
            ImpostaModalita(false, !recupera && !registrazione));
        recupero.GetComponent<Image>().color = new Color32(53, 52, 37, 255);
        testoRecupero = recupero.GetComponentInChildren<TMP_Text>();
        testoRecupero.color = FarmPixelUI.TestoChiaroFlat;
        messaggio = Testo("MessaggioAccesso", scheda.transform, "", new Vector2(0, -300), new Vector2(610, 106), 21, FarmPixelUI.TestoChiaroFlat);
        messaggio.textWrappingMode = TextWrappingModes.Normal;
        ospite = Pulsante("ContinuaOspite", scheda.transform, "GIOCA COME OSPITE", new Vector2(0, -380), new Vector2(610, 54), ContinuaComeOspite);
        ospite.GetComponent<Image>().color = new Color32(66, 68, 45, 255);
        ospite.GetComponentInChildren<TMP_Text>().color = FarmPixelUI.TestoChiaroFlat;
        email.onSubmit.AddListener(_ =>
        {
            if (recupera) Invia();
            else { password.Select(); password.ActivateInputField(); }
        });
        password.onSubmit.AddListener(_ =>
        {
            if (!registrazione) Invia();
            else { conferma.Select(); conferma.ActivateInputField(); }
        });
        conferma.onSubmit.AddListener(_ => Invia());
    }

    private static GameObject Pannello(string nome, Transform parent, Vector2 posizione, Vector2 dimensioni, Color colore)
    {
        GameObject obj = new GameObject(nome, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f;
        rect.anchoredPosition = posizione; rect.sizeDelta = dimensioni;
        obj.GetComponent<Image>().color = colore;
        return obj;
    }

    private static TMP_Text Testo(string nome, Transform parent, string contenuto, Vector2 posizione, Vector2 dimensioni, float font, Color colore)
    {
        GameObject obj = new GameObject(nome, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f;
        rect.anchoredPosition = posizione; rect.sizeDelta = dimensioni;
        TMP_Text testo = obj.GetComponent<TMP_Text>();
        FarmPixelUI.ApplicaTesto(testo, colore);
        testo.text = contenuto; testo.richText = false; testo.fontSize = font;
        testo.enableAutoSizing = true; testo.fontSizeMin = font * 0.7f; testo.fontSizeMax = font;
        testo.alignment = TextAlignmentOptions.Center;
        testo.raycastTarget = false;
        return testo;
    }

    private static Button Pulsante(string nome, Transform parent, string etichetta, Vector2 posizione, Vector2 dimensioni, UnityEngine.Events.UnityAction azione)
    {
        GameObject obj = Pannello(nome, parent, posizione, dimensioni, FarmPixelUI.ColorePulsanteNeutroFlat);
        Button button = obj.AddComponent<Button>();
        button.targetGraphic = obj.GetComponent<Image>();
        FarmPixelUI.ApplicaPulsante(button, FarmPixelUI.ColorePulsanteNeutroFlat);
        button.onClick.AddListener(azione);
        Testo("Etichetta", obj.transform, etichetta, Vector2.zero, dimensioni - new Vector2(16, 8), 22, FarmPixelUI.TestoPulsanteFlat);
        return button;
    }

    private static TMP_InputField Campo(Transform parent, string nome, string etichetta, Vector2 posizione, bool segreto)
    {
        GameObject riga = new GameObject("Riga_" + nome, typeof(RectTransform));
        riga.transform.SetParent(parent, false);
        RectTransform rigaRect = riga.GetComponent<RectTransform>();
        rigaRect.anchorMin = rigaRect.anchorMax = Vector2.one * 0.5f;
        rigaRect.anchoredPosition = posizione;
        rigaRect.sizeDelta = new Vector2(610, 76);
        TMP_Text label = Testo("Etichetta_" + nome, riga.transform, etichetta, new Vector2(0, 57), new Vector2(610, 28), 18, new Color32(217, 222, 185, 255));
        label.alignment = TextAlignmentOptions.MidlineLeft;
        GameObject obj = Pannello(nome, riga.transform, Vector2.zero, new Vector2(610, 76), new Color32(66, 70, 48, 255));
        TMP_InputField input = obj.AddComponent<TMP_InputField>();
        GameObject viewport = new GameObject("TestoVisibile", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(obj.transform, false);
        RectTransform rect = viewport.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(18, 8); rect.offsetMax = new Vector2(segreto && nome == "PasswordAccesso" ? -124 : -18, -8);
        TMP_Text value = Testo("Valore", viewport.transform, "", Vector2.zero, Vector2.zero, 24, FarmPixelUI.TestoChiaroFlat);
        value.rectTransform.anchorMin = Vector2.zero; value.rectTransform.anchorMax = Vector2.one;
        value.rectTransform.offsetMin = value.rectTransform.offsetMax = Vector2.zero;
        value.alignment = TextAlignmentOptions.MidlineLeft;
        TMP_Text placeholder = Testo("Segnaposto", viewport.transform, segreto ? "La tua password" : "nome@esempio.it", Vector2.zero, Vector2.zero, 22, new Color32(183, 191, 154, 180));
        placeholder.rectTransform.anchorMin = Vector2.zero; placeholder.rectTransform.anchorMax = Vector2.one;
        placeholder.rectTransform.offsetMin = placeholder.rectTransform.offsetMax = Vector2.zero;
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        input.textComponent = value; input.placeholder = placeholder; input.textViewport = rect;
        input.targetGraphic = obj.GetComponent<Image>();
        input.contentType = segreto ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.EmailAddress;
        input.characterLimit = segreto ? 256 : 254;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.caretColor = FarmPixelUI.TestoTitoloFlat;
        return input;
    }
}
