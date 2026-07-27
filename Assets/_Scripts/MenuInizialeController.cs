using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Costruisce e gestisce la schermata iniziale del gioco. La scena che lo
/// ospita deve rimanere leggera: canvas, EventSystem e pannelli vengono creati
/// a runtime, in continuita' con le altre interfacce della fattoria.
/// </summary>
[DisallowMultipleComponent]
public sealed class MenuInizialeController : MonoBehaviour
{
    public const string NomeScenaMenu = "MenuIniziale";
    public const string NomeScenaGameplay = "SampleScene";

    private static readonly Color32 ColoreSfondo =
        new Color32(54, 91, 48, 255);
    private static readonly Color32 ColoreSfondoSecondario =
        new Color32(41, 69, 37, 255);
    private readonly List<Button> pulsantiPrincipali =
        new List<Button>(5);

    private Canvas canvas;
    private GameObject pannelloPrincipale;
    private GameObject pannelloDifficolta;
    private GameObject pannelloProfilo;
    private GameObject pannelloConfermaUscita;
    private TMP_Text testoPulsanteProfilo;
    private TMP_Text testoPulsanteShop;
    private TMP_Text testoRiepilogo;
    private TMP_Text testoNomeProfilo;
    private TMP_Text testoIdProfilo;
    private TMP_Text testoStatisticheProfilo;
    private TMP_Text testoMessaggioProfilo;
    private TMP_Text testoStatoCaricamento;
    private TMP_InputField campoNomeProfilo;
    private bool interfacciaCostruita;
    private bool caricamentoInCorso;

    public static MenuInizialeController Instance { get; private set; }
    public static bool Attivo =>
        Instance != null &&
        Instance.isActiveAndEnabled &&
        Instance.gameObject.activeInHierarchy;

    public bool InterfacciaCostruita => interfacciaCostruita;
    public bool SelettoreDifficoltaAperto =>
        pannelloDifficolta != null && pannelloDifficolta.activeSelf;
    public bool ProfiloAperto =>
        pannelloProfilo != null && pannelloProfilo.activeSelf;
    public bool ConfermaUscitaAperta =>
        pannelloConfermaUscita != null &&
        pannelloConfermaUscita.activeSelf;
    public string NomeProfiloVisualizzato =>
        SalvataggioGiocatore.NomeProfilo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        AssicuraEventSystem();
        AssicuraAudioListener();
        CostruisciInterfaccia();
        AggiornaDatiVisibili();
    }

    private void OnEnable()
    {
        ProgressionePermanente.StatoCambiato += AggiornaDatiVisibili;
    }

    private void OnDisable()
    {
        ProgressionePermanente.StatoCambiato -= AggiornaDatiVisibili;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationFocus(bool haFocus)
    {
        if (haFocus)
        {
            AggiornaDatiVisibili();
        }
    }

    private void Update()
    {
        if (caricamentoInCorso ||
            ShopPermanentePrePartita.ApertoGlobale ||
            (PauseSettingsMenu.Instance != null &&
             PauseSettingsMenu.Instance.Aperto))
        {
            return;
        }

        if (SelettoreDifficoltaAperto)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) ||
                Input.GetKeyDown(KeyCode.Keypad1))
            {
                AvviaPartita(DifficoltaPartita.Tranquilla);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) ||
                     Input.GetKeyDown(KeyCode.Keypad2))
            {
                AvviaPartita(DifficoltaPartita.Normale);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) ||
                     Input.GetKeyDown(KeyCode.Keypad3))
            {
                AvviaPartita(DifficoltaPartita.Difficile);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                TornaAlPannelloPrincipale();
            }
            return;
        }

        if (ProfiloAperto)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TornaAlPannelloPrincipale();
            }
            return;
        }

        if (ConfermaUscitaAperta)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TornaAlPannelloPrincipale();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ApriSelettoreDifficolta();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            RichiediUscita();
        }
    }

    public void ApriSelettoreDifficolta()
    {
        if (caricamentoInCorso || pannelloDifficolta == null)
        {
            return;
        }

        NascondiPannelliLocali();
        pannelloDifficolta.SetActive(true);
        pannelloDifficolta.transform.SetAsLastSibling();
        FarmAudioController.RiproduciInterfaccia();
    }

    public void AvviaPartita(DifficoltaPartita difficolta)
    {
        if (caricamentoInCorso)
        {
            return;
        }

        caricamentoInCorso = true;
        ProgressionePartita.ImpostaDifficolta(difficolta);
        ProgressionePartita.PreparaAvvioDaMenu();

        if (testoStatoCaricamento != null)
        {
            ProfiloDifficolta profilo =
                GameBalanceConfig.Corrente.Difficolta.Ottieni(difficolta);
            testoStatoCaricamento.text =
                "PREPARAZIONE PARTITA  ·  " + profilo.Nome;
            testoStatoCaricamento.gameObject.SetActive(true);
        }

        for (int indice = 0; indice < pulsantiPrincipali.Count; indice++)
        {
            pulsantiPrincipali[indice].interactable = false;
        }

        FarmAudioController.RiproduciInterfaccia();
        Time.timeScale = 1f;
        SceneManager.LoadScene(NomeScenaGameplay, LoadSceneMode.Single);
    }

    public void ApriShopPermanente()
    {
        if (caricamentoInCorso)
        {
            return;
        }

        ShopPermanentePrePartita shop =
            ShopPermanentePrePartita.CreaNelMenu(canvas.transform);
        if (shop == null)
        {
            Debug.LogError(
                "Impossibile aprire lo shop permanente dal menu iniziale."
            );
            return;
        }

        shop.Mostra();
    }

    public void ApriOpzioni()
    {
        if (caricamentoInCorso)
        {
            return;
        }

        PauseSettingsMenu opzioni = PauseSettingsMenu.CreaOTrova();
        opzioni?.Mostra();
    }

    public void ApriProfilo()
    {
        if (caricamentoInCorso || pannelloProfilo == null)
        {
            return;
        }

        NascondiPannelliLocali();
        AggiornaDatiProfilo();
        pannelloProfilo.SetActive(true);
        pannelloProfilo.transform.SetAsLastSibling();
        FarmAudioController.RiproduciInterfaccia();
    }

    public void SalvaNomeProfilo()
    {
        if (campoNomeProfilo == null)
        {
            return;
        }

        string nome = campoNomeProfilo.text != null
            ? campoNomeProfilo.text.Trim()
            : string.Empty;
        if (string.IsNullOrEmpty(nome))
        {
            ImpostaMessaggioProfilo(
                "INSERISCI UN NOME PER IL PROFILO.",
                FarmPixelUI.TestoErroreFlat
            );
            return;
        }

        SalvataggioGiocatore.ImpostaNomeProfilo(nome);
        AggiornaDatiVisibili();
        AggiornaDatiProfilo();
        ImpostaMessaggioProfilo(
            "NOME SALVATO SU QUESTO DISPOSITIVO.",
            FarmPixelUI.TestoConfrontoFlat
        );
        FarmAudioController.RiproduciAcquisto(0.75f);
    }

    public void TornaAlPannelloPrincipale()
    {
        NascondiPannelliLocali();
        AggiornaDatiVisibili();
        FarmAudioController.RiproduciInterfaccia(0.75f);
    }

    public void RichiediUscita()
    {
        if (caricamentoInCorso || pannelloConfermaUscita == null)
        {
            return;
        }

        NascondiPannelliLocali();
        pannelloConfermaUscita.SetActive(true);
        pannelloConfermaUscita.transform.SetAsLastSibling();
        FarmAudioController.RiproduciInterfaccia();
    }

    public void ConfermaUscita()
    {
        GameOptionsController.Instance?.Salva();
        FarmAudioController.RiproduciInterfaccia(0.65f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CostruisciInterfaccia()
    {
        if (interfacciaCostruita)
        {
            return;
        }

        interfacciaCostruita = true;
        canvas = CreaCanvas();
        CreaSfondo(canvas.transform);
        CreaPannelloPrincipale(canvas.transform);
        CreaPannelloDifficolta(canvas.transform);
        CreaPannelloProfilo(canvas.transform);
        CreaPannelloConfermaUscita(canvas.transform);

        pannelloDifficolta.SetActive(false);
        pannelloProfilo.SetActive(false);
        pannelloConfermaUscita.SetActive(false);
    }

    private Canvas CreaCanvas()
    {
        GameObject oggetto = new GameObject(
            "MenuCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );
        oggetto.transform.SetParent(transform, false);

        Canvas nuovoCanvas = oggetto.GetComponent<Canvas>();
        nuovoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        nuovoCanvas.pixelPerfect = true;

        CanvasScaler scaler = oggetto.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return nuovoCanvas;
    }

    private void CreaSfondo(Transform parent)
    {
        Image baseSfondo = CreaImmagineStesa(
            "SfondoFattoria",
            parent,
            ColoreSfondo
        );
        baseSfondo.transform.SetAsFirstSibling();

        CreaImmagine(
            "FasciaSuperiore",
            parent,
            new Vector2(0f, 500f),
            new Vector2(1920f, 160f),
            ColoreSfondoSecondario
        );
        CreaImmagine(
            "FasciaInferiore",
            parent,
            new Vector2(0f, -500f),
            new Vector2(1920f, 160f),
            ColoreSfondoSecondario
        );

        Image volpe = FarmPixelUI.AggiungiIcona(
            parent,
            "DecorazioneVolpe",
            FarmPixelIcon.Volpe,
            new Vector2(-670f, -20f),
            new Vector2(230f, 230f)
        );
        volpe.color = new Color(1f, 1f, 1f, 0.25f);

        Image contadino = FarmPixelUI.AggiungiIcona(
            parent,
            "DecorazioneContadino",
            FarmPixelIcon.Ondata,
            new Vector2(670f, 20f),
            new Vector2(230f, 230f)
        );
        contadino.color = new Color(1f, 1f, 1f, 0.25f);

        CreaTesto(
            "MottoSfondo",
            parent,
            "ONDATE INFINITE  ·  UN SOLO CONTADINO",
            new Vector2(0f, -494f),
            new Vector2(1000f, 42f),
            19f,
            new Color32(226, 210, 169, 210),
            FontStyles.Bold
        );
    }

    private void CreaPannelloPrincipale(Transform parent)
    {
        pannelloPrincipale = CreaPannello(
            "PannelloPrincipale",
            parent,
            new Vector2(820f, 900f),
            Vector2.zero
        );

        CreaTesto(
            "Titolo",
            pannelloPrincipale.transform,
            "ANGRY FARMER",
            new Vector2(0f, 365f),
            new Vector2(700f, 86f),
            58f,
            FarmPixelUI.TestoTitoloFlat,
            FontStyles.Bold
        );
        CreaTesto(
            "Sottotitolo",
            pannelloPrincipale.transform,
            "SURVIVAL DELLA FATTORIA",
            new Vector2(0f, 305f),
            new Vector2(680f, 40f),
            22f,
            FarmPixelUI.TestoChiaroFlat,
            FontStyles.Bold
        );

        GameObject schedaProfilo = CreaPannello(
            "SchedaProfilo",
            pannelloPrincipale.transform,
            new Vector2(650f, 78f),
            new Vector2(0f, 236f),
            true
        );
        FarmPixelUI.AggiungiIcona(
            schedaProfilo.transform,
            "IconaGettone",
            FarmPixelIcon.GettonePermanente,
            new Vector2(-273f, 0f),
            new Vector2(34f, 34f)
        );
        testoRiepilogo = CreaTesto(
            "Riepilogo",
            schedaProfilo.transform,
            string.Empty,
            new Vector2(17f, 0f),
            new Vector2(540f, 56f),
            19f,
            FarmPixelUI.TestoMetaFlat,
            FontStyles.Bold
        );

        CreaPulsantePrincipale(
            "Gioca",
            "GIOCA",
            new Vector2(0f, 130f),
            FarmPixelUI.ColorePulsanteVerdeFlat,
            FarmPixelIcon.Ondata,
            ApriSelettoreDifficolta,
            out _
        );
        CreaPulsantePrincipale(
            "ShopPermanente",
            "MIGLIORAMENTI PERMANENTI",
            new Vector2(0f, 32f),
            FarmPixelUI.ColorePulsanteOroFlat,
            FarmPixelIcon.Bottega,
            ApriShopPermanente,
            out testoPulsanteShop
        );
        CreaPulsantePrincipale(
            "Opzioni",
            "OPZIONI",
            new Vector2(0f, -66f),
            FarmPixelUI.ColorePulsanteNeutroFlat,
            FarmPixelIcon.Resistenza,
            ApriOpzioni,
            out _
        );
        CreaPulsantePrincipale(
            "Profilo",
            "PROFILO OSPITE",
            new Vector2(0f, -164f),
            FarmPixelUI.ColorePulsanteViolaFlat,
            FarmPixelIcon.Cuore,
            ApriProfilo,
            out testoPulsanteProfilo
        );
        CreaPulsantePrincipale(
            "Esci",
            "ESCI",
            new Vector2(0f, -262f),
            FarmPixelUI.ColorePulsanteNeutroFlat,
            null,
            RichiediUscita,
            out _
        );

        CreaTesto(
            "NotaSalvataggio",
            pannelloPrincipale.transform,
            "PROFILO OSPITE  ·  SALVATAGGIO LOCALE ATTIVO",
            new Vector2(0f, -345f),
            new Vector2(680f, 34f),
            16f,
            FarmPixelUI.TestoMetaFlat,
            FontStyles.Bold
        );
        CreaTesto(
            "Comandi",
            pannelloPrincipale.transform,
            "INVIO: GIOCA  ·  ESC: ESCI",
            new Vector2(0f, -390f),
            new Vector2(680f, 34f),
            15f,
            FarmPixelUI.TestoChiaroFlat,
            FontStyles.Normal
        );

        testoStatoCaricamento = CreaTesto(
            "StatoCaricamento",
            parent,
            "PREPARAZIONE PARTITA",
            new Vector2(0f, -455f),
            new Vector2(820f, 42f),
            20f,
            FarmPixelUI.TestoTitoloFlat,
            FontStyles.Bold
        );
        testoStatoCaricamento.gameObject.SetActive(false);
    }

    private void CreaPannelloDifficolta(Transform parent)
    {
        pannelloDifficolta = CreaOverlay("SelettoreDifficolta", parent);
        GameObject pannello = CreaPannello(
            "PannelloScelta",
            pannelloDifficolta.transform,
            new Vector2(900f, 820f),
            Vector2.zero
        );

        CreaTesto(
            "Titolo",
            pannello.transform,
            "SCEGLI LA DIFFICOLTA",
            new Vector2(0f, 340f),
            new Vector2(760f, 62f),
            38f,
            FarmPixelUI.TestoTitoloFlat,
            FontStyles.Bold
        );
        CreaTesto(
            "Sottotitolo",
            pannello.transform,
            "La scelta modifica vita, velocita e ritmo delle volpi.",
            new Vector2(0f, 288f),
            new Vector2(760f, 40f),
            19f,
            FarmPixelUI.TestoChiaroFlat,
            FontStyles.Normal
        );

        BilanciamentoDifficolta difficolta =
            GameBalanceConfig.Corrente.Difficolta;
        CreaPulsanteDifficolta(
            pannello.transform,
            DifficoltaPartita.Tranquilla,
            difficolta.Ottieni(DifficoltaPartita.Tranquilla),
            new Vector2(0f, 174f),
            FarmPixelUI.ColorePulsanteNeutroFlat,
            "1"
        );
        CreaPulsanteDifficolta(
            pannello.transform,
            DifficoltaPartita.Normale,
            difficolta.Ottieni(DifficoltaPartita.Normale),
            new Vector2(0f, 48f),
            FarmPixelUI.ColorePulsanteVerdeFlat,
            "2"
        );
        CreaPulsanteDifficolta(
            pannello.transform,
            DifficoltaPartita.Difficile,
            difficolta.Ottieni(DifficoltaPartita.Difficile),
            new Vector2(0f, -78f),
            FarmPixelUI.ColorePulsanteViolaFlat,
            "3"
        );

        CreaTesto(
            "Nota",
            pannello.transform,
            "Lo shop iniziale gratuito si aprira prima dell'ondata 1.",
            new Vector2(0f, -198f),
            new Vector2(760f, 42f),
            18f,
            FarmPixelUI.TestoMetaFlat,
            FontStyles.Bold
        );
        CreaPulsante(
            "Indietro",
            pannello.transform,
            "INDIETRO  [ESC]",
            new Vector2(0f, -286f),
            new Vector2(360f, 62f),
            FarmPixelUI.ColorePulsanteNeutroFlat,
            null,
            TornaAlPannelloPrincipale,
            out _
        );
    }

    private void CreaPannelloProfilo(Transform parent)
    {
        pannelloProfilo = CreaOverlay("ProfiloOspite", parent);
        GameObject pannello = CreaPannello(
            "PannelloProfilo",
            pannelloProfilo.transform,
            new Vector2(850f, 780f),
            Vector2.zero
        );

        FarmPixelUI.AggiungiIcona(
            pannello.transform,
            "IconaProfilo",
            FarmPixelIcon.Cuore,
            new Vector2(-310f, 308f),
            new Vector2(54f, 54f)
        );
        CreaTesto(
            "Titolo",
            pannello.transform,
            "PROFILO OSPITE LOCALE",
            new Vector2(15f, 312f),
            new Vector2(630f, 58f),
            35f,
            FarmPixelUI.TestoTitoloFlat,
            FontStyles.Bold
        );

        testoNomeProfilo = CreaTesto(
            "NomeProfilo",
            pannello.transform,
            "OSPITE",
            new Vector2(0f, 228f),
            new Vector2(700f, 58f),
            31f,
            FarmPixelUI.TestoChiaroFlat,
            FontStyles.Bold
        );
        CreaTesto(
            "Descrizione",
            pannello.transform,
            "Puoi giocare senza account. I progressi restano salvati " +
            "su questo dispositivo.",
            new Vector2(0f, 168f),
            new Vector2(700f, 58f),
            18f,
            FarmPixelUI.TestoMetaFlat,
            FontStyles.Normal
        );

        campoNomeProfilo = CreaCampoNome(
            pannello.transform,
            new Vector2(0f, 78f),
            new Vector2(650f, 66f)
        );
        campoNomeProfilo.onSubmit.AddListener(_ => SalvaNomeProfilo());

        testoIdProfilo = CreaTesto(
            "IdProfilo",
            pannello.transform,
            "ID LOCALE  --------",
            new Vector2(0f, 12f),
            new Vector2(650f, 34f),
            16f,
            FarmPixelUI.TestoMetaFlat,
            FontStyles.Bold
        );
        testoStatisticheProfilo = CreaTesto(
            "Statistiche",
            pannello.transform,
            string.Empty,
            new Vector2(0f, -78f),
            new Vector2(700f, 94f),
            20f,
            FarmPixelUI.TestoChiaroFlat,
            FontStyles.Bold
        );
        testoMessaggioProfilo = CreaTesto(
            "Messaggio",
            pannello.transform,
            "Il collegamento a un account online rimane opzionale.",
            new Vector2(0f, -160f),
            new Vector2(700f, 42f),
            17f,
            FarmPixelUI.TestoMetaFlat,
            FontStyles.Bold
        );

        CreaPulsante(
            "SalvaNome",
            pannello.transform,
            "SALVA NOME",
            new Vector2(-185f, -248f),
            new Vector2(320f, 62f),
            FarmPixelUI.ColorePulsanteVerdeFlat,
            FarmPixelIcon.Cuore,
            SalvaNomeProfilo,
            out _
        );
        CreaPulsante(
            "Indietro",
            pannello.transform,
            "INDIETRO",
            new Vector2(185f, -248f),
            new Vector2(320f, 62f),
            FarmPixelUI.ColorePulsanteNeutroFlat,
            null,
            TornaAlPannelloPrincipale,
            out _
        );
        CreaTesto(
            "NotaAccount",
            pannello.transform,
            "Nessuna registrazione e richiesta per iniziare a giocare.",
            new Vector2(0f, -326f),
            new Vector2(700f, 34f),
            15f,
            FarmPixelUI.TestoMetaFlat,
            FontStyles.Normal
        );
    }

    private void CreaPannelloConfermaUscita(Transform parent)
    {
        pannelloConfermaUscita = CreaOverlay(
            "ConfermaUscita",
            parent
        );
        GameObject pannello = CreaPannello(
            "PannelloConferma",
            pannelloConfermaUscita.transform,
            new Vector2(650f, 360f),
            Vector2.zero
        );

        CreaTesto(
            "Titolo",
            pannello.transform,
            "VUOI USCIRE?",
            new Vector2(0f, 100f),
            new Vector2(520f, 60f),
            34f,
            FarmPixelUI.TestoTitoloFlat,
            FontStyles.Bold
        );
        CreaTesto(
            "Nota",
            pannello.transform,
            "I progressi locali sono gia stati salvati.",
            new Vector2(0f, 40f),
            new Vector2(520f, 40f),
            17f,
            FarmPixelUI.TestoChiaroFlat,
            FontStyles.Normal
        );
        CreaPulsante(
            "Annulla",
            pannello.transform,
            "ANNULLA",
            new Vector2(-150f, -74f),
            new Vector2(250f, 62f),
            FarmPixelUI.ColorePulsanteVerdeFlat,
            null,
            TornaAlPannelloPrincipale,
            out _
        );
        CreaPulsante(
            "Esci",
            pannello.transform,
            "ESCI",
            new Vector2(150f, -74f),
            new Vector2(250f, 62f),
            FarmPixelUI.ColorePulsanteNeutroFlat,
            null,
            ConfermaUscita,
            out _
        );
    }

    private void CreaPulsantePrincipale(
        string nome,
        string etichetta,
        Vector2 posizione,
        Color colore,
        FarmPixelIcon? icona,
        UnityEngine.Events.UnityAction azione,
        out TMP_Text testo
    )
    {
        Button pulsante = CreaPulsante(
            nome,
            pannelloPrincipale.transform,
            etichetta,
            posizione,
            new Vector2(650f, 76f),
            colore,
            icona,
            azione,
            out testo
        );
        pulsantiPrincipali.Add(pulsante);
    }

    private void CreaPulsanteDifficolta(
        Transform parent,
        DifficoltaPartita difficolta,
        ProfiloDifficolta profilo,
        Vector2 posizione,
        Color colore,
        string tasto
    )
    {
        string nomeProfilo = profilo != null
            ? profilo.Nome
            : difficolta.ToString().ToUpperInvariant();
        string descrizione = profilo != null
            ? profilo.descrizione
            : string.Empty;
        TMP_Text testo;
        CreaPulsante(
            "Difficolta_" + difficolta,
            parent,
            "[" + tasto + "]  " + nomeProfilo + "\n" + descrizione,
            posizione,
            new Vector2(720f, 100f),
            colore,
            FarmPixelIcon.Volpe,
            () => AvviaPartita(difficolta),
            out testo
        );
        testo.fontSize = 19f;
        testo.lineSpacing = -6f;
    }

    private Button CreaPulsante(
        string nome,
        Transform parent,
        string etichetta,
        Vector2 posizione,
        Vector2 dimensione,
        Color colore,
        FarmPixelIcon? icona,
        UnityEngine.Events.UnityAction azione,
        out TMP_Text testo
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        oggetto.transform.SetParent(parent, false);
        ImpostaRectCentrato(
            oggetto.GetComponent<RectTransform>(),
            posizione,
            dimensione
        );

        Button pulsante = oggetto.GetComponent<Button>();
        FarmPixelUI.ApplicaPulsante(pulsante, colore);
        pulsante.onClick.AddListener(azione);

        float margineSinistro = 18f;
        if (icona.HasValue)
        {
            FarmPixelUI.AggiungiIcona(
                oggetto.transform,
                "Icona",
                icona.Value,
                new Vector2(-dimensione.x * 0.42f, 0f),
                new Vector2(38f, 38f)
            );
            margineSinistro = 72f;
        }

        testo = CreaTesto(
            "Testo",
            oggetto.transform,
            etichetta,
            Vector2.zero,
            dimensione,
            22f,
            FarmPixelUI.TestoPulsanteFlat,
            FontStyles.Bold
        );
        RectTransform testoRect = testo.rectTransform;
        testoRect.anchorMin = Vector2.zero;
        testoRect.anchorMax = Vector2.one;
        testoRect.offsetMin = new Vector2(margineSinistro, 6f);
        testoRect.offsetMax = new Vector2(-18f, -6f);
        testoRect.anchoredPosition = Vector2.zero;
        testoRect.sizeDelta = Vector2.zero;
        return pulsante;
    }

    private TMP_InputField CreaCampoNome(
        Transform parent,
        Vector2 posizione,
        Vector2 dimensione
    )
    {
        GameObject radice = new GameObject(
            "CampoNomeProfilo",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(TMP_InputField)
        );
        radice.transform.SetParent(parent, false);
        ImpostaRectCentrato(
            radice.GetComponent<RectTransform>(),
            posizione,
            dimensione
        );
        FarmPixelUI.ApplicaPannello(
            radice.GetComponent<Image>(),
            true,
            true
        );

        GameObject area = new GameObject(
            "AreaTesto",
            typeof(RectTransform),
            typeof(RectMask2D)
        );
        area.transform.SetParent(radice.transform, false);
        RectTransform areaRect = area.GetComponent<RectTransform>();
        areaRect.anchorMin = Vector2.zero;
        areaRect.anchorMax = Vector2.one;
        areaRect.offsetMin = new Vector2(22f, 8f);
        areaRect.offsetMax = new Vector2(-22f, -8f);

        TMP_Text segnaposto = CreaTesto(
            "Segnaposto",
            area.transform,
            "NOME PROFILO",
            Vector2.zero,
            Vector2.zero,
            21f,
            new Color32(196, 176, 145, 170),
            FontStyles.Normal
        );
        ConfiguraTestoCampo(segnaposto.rectTransform);

        TMP_Text testo = CreaTesto(
            "Testo",
            area.transform,
            string.Empty,
            Vector2.zero,
            Vector2.zero,
            22f,
            FarmPixelUI.TestoChiaroFlat,
            FontStyles.Bold
        );
        ConfiguraTestoCampo(testo.rectTransform);

        TMP_InputField campo = radice.GetComponent<TMP_InputField>();
        campo.targetGraphic = radice.GetComponent<Image>();
        campo.textViewport = areaRect;
        campo.textComponent = testo;
        campo.placeholder = segnaposto;
        campo.lineType = TMP_InputField.LineType.SingleLine;
        campo.contentType = TMP_InputField.ContentType.Standard;
        campo.characterLimit = 18;
        campo.caretColor = FarmPixelUI.TestoTitoloFlat;
        campo.selectionColor = new Color(0.75f, 0.55f, 0.2f, 0.55f);
        return campo;
    }

    private static void ConfiguraTestoCampo(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private GameObject CreaOverlay(string nome, Transform parent)
    {
        GameObject overlay = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        overlay.transform.SetParent(parent, false);
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image immagine = overlay.GetComponent<Image>();
        immagine.color = FarmPixelUI.ColoreVeloFlat;
        immagine.raycastTarget = true;
        return overlay;
    }

    private GameObject CreaPannello(
        string nome,
        Transform parent,
        Vector2 dimensione,
        Vector2 posizione,
        bool incassato = false
    )
    {
        GameObject pannello = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        pannello.transform.SetParent(parent, false);
        ImpostaRectCentrato(
            pannello.GetComponent<RectTransform>(),
            posizione,
            dimensione
        );
        FarmPixelUI.ApplicaPannello(
            pannello.GetComponent<Image>(),
            incassato,
            true
        );
        return pannello;
    }

    private static Image CreaImmagineStesa(
        string nome,
        Transform parent,
        Color colore
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        oggetto.transform.SetParent(parent, false);
        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image immagine = oggetto.GetComponent<Image>();
        immagine.color = colore;
        immagine.raycastTarget = false;
        return immagine;
    }

    private static Image CreaImmagine(
        string nome,
        Transform parent,
        Vector2 posizione,
        Vector2 dimensione,
        Color colore
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        oggetto.transform.SetParent(parent, false);
        ImpostaRectCentrato(
            oggetto.GetComponent<RectTransform>(),
            posizione,
            dimensione
        );
        Image immagine = oggetto.GetComponent<Image>();
        immagine.color = colore;
        immagine.raycastTarget = false;
        return immagine;
    }

    private static TMP_Text CreaTesto(
        string nome,
        Transform parent,
        string contenuto,
        Vector2 posizione,
        Vector2 dimensione,
        float grandezza,
        Color colore,
        FontStyles stile
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggetto.transform.SetParent(parent, false);
        ImpostaRectCentrato(
            oggetto.GetComponent<RectTransform>(),
            posizione,
            dimensione
        );

        TextMeshProUGUI testo = oggetto.GetComponent<TextMeshProUGUI>();
        testo.text = contenuto;
        testo.fontSize = grandezza;
        testo.fontStyle = stile;
        testo.alignment = TextAlignmentOptions.Center;
        testo.textWrappingMode = TextWrappingModes.Normal;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(testo, colore);
        return testo;
    }

    private static void ImpostaRectCentrato(
        RectTransform rect,
        Vector2 posizione,
        Vector2 dimensione
    )
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensione;
    }

    private static void AssicuraEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject oggetto = new GameObject(
            "EventSystem",
            typeof(EventSystem)
        );
        InputSystemUIInputModule modulo =
            oggetto.AddComponent<InputSystemUIInputModule>();
        modulo.AssignDefaultActions();
    }

    private void AssicuraAudioListener()
    {
        if (FindFirstObjectByType<AudioListener>() != null)
        {
            return;
        }

        GameObject ascolto = new GameObject(
            "AudioListenerMenu",
            typeof(AudioListener)
        );
        ascolto.transform.SetParent(transform, false);
    }

    private void NascondiPannelliLocali()
    {
        if (pannelloDifficolta != null)
        {
            pannelloDifficolta.SetActive(false);
        }
        if (pannelloProfilo != null)
        {
            pannelloProfilo.SetActive(false);
        }
        if (pannelloConfermaUscita != null)
        {
            pannelloConfermaUscita.SetActive(false);
        }
    }

    private void AggiornaDatiVisibili()
    {
        if (!interfacciaCostruita)
        {
            return;
        }

        string nome = NomeProfiloSicuro();
        int gettoni = ProgressionePermanente.SaldoGettoni;
        int miglioreOndata = Mathf.Max(
            0,
            SalvataggioGiocatore.MiglioreOndata
        );

        if (testoPulsanteProfilo != null)
        {
            testoPulsanteProfilo.text = "PROFILO  ·  " + nome;
        }
        if (testoPulsanteShop != null)
        {
            testoPulsanteShop.text =
                "MIGLIORAMENTI  ·  " + gettoni + " GETTONI";
        }
        if (testoRiepilogo != null)
        {
            testoRiepilogo.text =
                nome + "  ·  " + gettoni + " GETTONI  ·  RECORD ONDATA " +
                miglioreOndata;
        }

        if (ProfiloAperto)
        {
            AggiornaDatiProfilo();
        }
    }

    private void AggiornaDatiProfilo()
    {
        if (testoNomeProfilo == null)
        {
            return;
        }

        string nome = NomeProfiloSicuro();
        testoNomeProfilo.text = nome;
        testoIdProfilo.text =
            "ID LOCALE  " + SalvataggioGiocatore.IdProfiloBreve;
        testoStatisticheProfilo.text =
            "MIGLIORE ONDATA  " +
            Mathf.Max(0, SalvataggioGiocatore.MiglioreOndata) +
            "\nGETTONI  " + ProgressionePermanente.SaldoGettoni +
            "  ·  LIVELLI PERMANENTI  " +
            ProgressionePermanente.TotaleLivelliAcquistati;
        campoNomeProfilo.SetTextWithoutNotify(
            SalvataggioGiocatore.NomeProfilo
        );
        ImpostaMessaggioProfilo(
            "Il collegamento a un account online rimane opzionale.",
            FarmPixelUI.TestoMetaFlat
        );
    }

    private static string NomeProfiloSicuro()
    {
        string nome = SalvataggioGiocatore.NomeProfilo;
        return string.IsNullOrWhiteSpace(nome)
            ? "OSPITE"
            : nome.Trim().ToUpperInvariant();
    }

    private void ImpostaMessaggioProfilo(string messaggio, Color colore)
    {
        if (testoMessaggioProfilo == null)
        {
            return;
        }

        testoMessaggioProfilo.text = messaggio;
        FarmPixelUI.ApplicaTesto(testoMessaggioProfilo, colore);
    }
}
