using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ShopInterOndata : MonoBehaviour
{
    private sealed class RigaShop
    {
        public Button pulsante;
        public TMP_Text testoPulsante;
        public TMP_Text testoDescrizione;
        public TMP_Text testoStato;
    }

    private static readonly TipoPotenziamento[] tipi =
    {
        TipoPotenziamento.Movimento,
        TipoPotenziamento.Resistenza,
        TipoPotenziamento.SaluteMassima,
        TipoPotenziamento.Cura,
        TipoPotenziamento.Danno,
        TipoPotenziamento.Cadenza,
        TipoPotenziamento.Penetrazione
    };

    private readonly Dictionary<TipoPotenziamento, RigaShop> righe =
        new Dictionary<TipoPotenziamento, RigaShop>();

    private GameObject pannelloRiepilogo;
    private GameObject pannelloBottega;
    private TMP_Text testoRiepilogo;
    private TMP_Text testoMoneteRiepilogo;
    private TMP_Text testoMoneteBottega;
    private TMP_Text testoMessaggioBottega;
    private TMP_FontAsset fontInterfaccia;
    private PlayerUpgrades potenziamenti;
    private bool costruito;

    public static ShopInterOndata CreaOTrova()
    {
        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null)
        {
            Debug.LogError(
                "L'oggetto Interfaccia non è presente: shop non creato."
            );
            return null;
        }

        ShopInterOndata esistente =
            interfaccia.GetComponentInChildren<ShopInterOndata>(true);
        if (esistente != null) return esistente;

        GameObject overlay = new GameObject(
            "IntervalloTraOndate",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        overlay.transform.SetParent(interfaccia.transform, false);
        return overlay.AddComponent<ShopInterOndata>();
    }

    void Awake()
    {
        CostruisciInterfaccia();
    }

    void OnEnable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.MoneteCambiate += MoneteAggiornate;
        }
        AggiornaInterfaccia();
    }

    void OnDisable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.MoneteCambiate -= MoneteAggiornate;
        }
    }

    public void Mostra(int ondaCompletata, int totaleOndate)
    {
        if (!costruito) CostruisciInterfaccia();

        PreparaPotenziamentiGiocatore();
        testoRiepilogo.text =
            "Ondata " + ondaCompletata + " di " + totaleOndate +
            " superata\nLa fattoria ha un momento per riorganizzarsi.";
        testoMessaggioBottega.text =
            "Scegli con cura: i potenziamenti durano per tutta la partita.";

        pannelloRiepilogo.SetActive(true);
        pannelloBottega.SetActive(false);
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        AggiornaInterfaccia();
    }

    public void Nascondi()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    void ApriBottega()
    {
        PreparaPotenziamentiGiocatore();
        pannelloRiepilogo.SetActive(false);
        pannelloBottega.SetActive(true);
        testoMessaggioBottega.text =
            "Scegli con cura: i potenziamenti durano per tutta la partita.";
        AggiornaInterfaccia();
    }

    void TornaAlRiepilogo()
    {
        pannelloBottega.SetActive(false);
        pannelloRiepilogo.SetActive(true);
        AggiornaInterfaccia();
    }

    void AvviaOndataSuccessiva()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ContinuaConOndataSuccessiva();
        }
    }

    void Acquista(TipoPotenziamento tipo)
    {
        if (potenziamenti == null)
        {
            testoMessaggioBottega.text =
                "Contadino non trovato: acquisto non disponibile.";
            return;
        }

        string messaggio;
        bool acquistato = potenziamenti.ProvaAcquistare(tipo, out messaggio);
        testoMessaggioBottega.text = acquistato
            ? messaggio + "  " + potenziamenti.OttieniTitolo(tipo)
            : messaggio;
        AggiornaInterfaccia();
    }

    void PreparaPotenziamentiGiocatore()
    {
        if (potenziamenti != null) return;

        GameObject giocatore = GameObject.FindGameObjectWithTag("Player");
        if (giocatore == null) return;

        potenziamenti = giocatore.GetComponent<PlayerUpgrades>();
        if (potenziamenti == null)
        {
            potenziamenti = giocatore.AddComponent<PlayerUpgrades>();
        }
    }

    void MoneteAggiornate(int nuovaQuantita)
    {
        AggiornaInterfaccia();
    }

    void AggiornaInterfaccia()
    {
        int monete = GameManager.instance != null
            ? GameManager.instance.monete
            : 0;
        string testoMonete = "MONETE  " + monete;

        if (testoMoneteRiepilogo != null)
        {
            testoMoneteRiepilogo.text = testoMonete;
        }
        if (testoMoneteBottega != null)
        {
            testoMoneteBottega.text = testoMonete;
        }

        foreach (TipoPotenziamento tipo in tipi)
        {
            RigaShop riga;
            if (!righe.TryGetValue(tipo, out riga)) continue;

            if (potenziamenti == null)
            {
                riga.pulsante.interactable = false;
                riga.testoPulsante.text = "NON DISPONIBILE";
                riga.testoStato.text = string.Empty;
                continue;
            }

            bool disponibile = potenziamenti.PuoAcquistare(tipo);
            int costo = potenziamenti.OttieniCosto(tipo);
            riga.pulsante.interactable = disponibile && monete >= costo;
            riga.testoDescrizione.text =
                potenziamenti.OttieniDescrizione(tipo);
            riga.testoStato.text = potenziamenti.OttieniStato(tipo);

            if (!disponibile)
            {
                riga.testoPulsante.text =
                    tipo == TipoPotenziamento.Cura ? "SALUTE PIENA" : "MAX";
            }
            else
            {
                riga.testoPulsante.text = costo + " MONETE";
            }
        }
    }

    void CostruisciInterfaccia()
    {
        if (costruito) return;
        costruito = true;

        TMP_Text testoHUD = GameManager.TrovaTestoInterfaccia("OndataText");
        if (testoHUD != null)
        {
            fontInterfaccia = testoHUD.font;
        }

        RectTransform rootRect = GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image velo = GetComponent<Image>();
        velo.color = new Color(0.025f, 0.014f, 0.009f, 0.86f);
        velo.raycastTarget = true;

        pannelloRiepilogo = CreaPannello(
            "RiepilogoOndata",
            transform,
            new Vector2(820f, 520f),
            new Color(0.13f, 0.072f, 0.035f, 0.98f)
        );
        CostruisciRiepilogo(pannelloRiepilogo.transform);

        pannelloBottega = CreaPannello(
            "Bottega",
            transform,
            new Vector2(1180f, 900f),
            new Color(0.105f, 0.057f, 0.027f, 0.99f)
        );
        CostruisciBottega(pannelloBottega.transform);

        pannelloRiepilogo.SetActive(true);
        pannelloBottega.SetActive(false);
        gameObject.SetActive(false);
    }

    void CostruisciRiepilogo(Transform parent)
    {
        CreaTesto(
            "Titolo",
            parent,
            "ONDATA COMPLETATA",
            new Vector2(0f, 182f),
            new Vector2(730f, 64f),
            42f,
            new Color(1f, 0.76f, 0.25f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        testoRiepilogo = CreaTesto(
            "Riepilogo",
            parent,
            string.Empty,
            new Vector2(0f, 82f),
            new Vector2(700f, 100f),
            25f,
            new Color(1f, 0.91f, 0.71f, 1f),
            FontStyles.Normal,
            TextAlignmentOptions.Center
        );

        testoMoneteRiepilogo = CreaTesto(
            "Monete",
            parent,
            "MONETE  0",
            new Vector2(0f, -5f),
            new Vector2(400f, 50f),
            29f,
            new Color(1f, 0.86f, 0.22f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        CreaTesto(
            "Suggerimento",
            parent,
            "Puoi ripartire subito oppure spendere le monete nella bottega.",
            new Vector2(0f, -70f),
            new Vector2(690f, 54f),
            20f,
            new Color(0.86f, 0.78f, 0.65f, 1f),
            FontStyles.Normal,
            TextAlignmentOptions.Center
        );

        CreaPulsante(
            "ApriBottega",
            parent,
            "APRI LA BOTTEGA",
            new Vector2(-190f, -166f),
            new Vector2(330f, 64f),
            new Color(0.7f, 0.35f, 0.08f, 1f),
            ApriBottega
        );
        CreaPulsante(
            "OndataSuccessiva",
            parent,
            "ONDATA SUCCESSIVA",
            new Vector2(190f, -166f),
            new Vector2(330f, 64f),
            new Color(0.24f, 0.55f, 0.2f, 1f),
            AvviaOndataSuccessiva
        );
    }

    void CostruisciBottega(Transform parent)
    {
        CreaTesto(
            "Titolo",
            parent,
            "BOTTEGA DELLA FATTORIA",
            new Vector2(0f, 397f),
            new Vector2(760f, 54f),
            37f,
            new Color(1f, 0.76f, 0.25f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        testoMoneteBottega = CreaTesto(
            "Monete",
            parent,
            "MONETE  0",
            new Vector2(430f, 397f),
            new Vector2(250f, 48f),
            25f,
            new Color(1f, 0.86f, 0.22f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        for (int i = 0; i < tipi.Length; i++)
        {
            CreaRigaBottega(parent, tipi[i], 305f - i * 84f);
        }

        testoMessaggioBottega = CreaTesto(
            "Messaggio",
            parent,
            string.Empty,
            new Vector2(0f, -302f),
            new Vector2(950f, 44f),
            18f,
            new Color(1f, 0.84f, 0.51f, 1f),
            FontStyles.Italic,
            TextAlignmentOptions.Center
        );

        CreaPulsante(
            "Indietro",
            parent,
            "INDIETRO",
            new Vector2(-205f, -377f),
            new Vector2(330f, 58f),
            new Color(0.39f, 0.24f, 0.13f, 1f),
            TornaAlRiepilogo
        );
        CreaPulsante(
            "OndataSuccessiva",
            parent,
            "ONDATA SUCCESSIVA",
            new Vector2(205f, -377f),
            new Vector2(330f, 58f),
            new Color(0.24f, 0.55f, 0.2f, 1f),
            AvviaOndataSuccessiva
        );
    }

    void CreaRigaBottega(
        Transform parent,
        TipoPotenziamento tipo,
        float posizioneY
    )
    {
        GameObject riga = CreaPannello(
            "Riga_" + tipo,
            parent,
            new Vector2(1060f, 72f),
            new Color(0.23f, 0.13f, 0.065f, 0.92f),
            new Vector2(0f, posizioneY),
            false
        );

        string titolo = TitoloPredefinito(tipo);
        string descrizione = DescrizionePredefinita(tipo);

        CreaTesto(
            "Nome",
            riga.transform,
            titolo,
            new Vector2(-360f, 12f),
            new Vector2(300f, 30f),
            19f,
            new Color(1f, 0.78f, 0.32f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.MidlineLeft
        );
        TMP_Text descrizioneTesto = CreaTesto(
            "Descrizione",
            riga.transform,
            descrizione,
            new Vector2(-255f, -18f),
            new Vector2(510f, 27f),
            16f,
            new Color(0.91f, 0.84f, 0.72f, 1f),
            FontStyles.Normal,
            TextAlignmentOptions.MidlineLeft
        );

        TMP_Text stato = CreaTesto(
            "Stato",
            riga.transform,
            string.Empty,
            new Vector2(172f, 0f),
            new Vector2(210f, 36f),
            16f,
            new Color(0.74f, 0.89f, 0.62f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        TipoPotenziamento tipoCatturato = tipo;
        Button bottone = CreaPulsante(
            "Acquista",
            riga.transform,
            "--",
            new Vector2(414f, 0f),
            new Vector2(190f, 48f),
            new Color(0.68f, 0.33f, 0.075f, 1f),
            () => Acquista(tipoCatturato)
        );

        righe[tipo] = new RigaShop
        {
            pulsante = bottone,
            testoPulsante = bottone.GetComponentInChildren<TMP_Text>(),
            testoDescrizione = descrizioneTesto,
            testoStato = stato
        };
    }

    GameObject CreaPannello(
        string nome,
        Transform parent,
        Vector2 dimensioni,
        Color colore,
        Vector2? posizione = null,
        bool bordoMarcato = true
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline)
        );
        oggetto.transform.SetParent(parent, false);

        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione ?? Vector2.zero;
        rect.sizeDelta = dimensioni;

        Image immagine = oggetto.GetComponent<Image>();
        immagine.color = colore;
        immagine.raycastTarget = true;

        Outline bordo = oggetto.GetComponent<Outline>();
        bordo.effectColor = bordoMarcato
            ? new Color(0.75f, 0.39f, 0.11f, 0.96f)
            : new Color(0.42f, 0.24f, 0.1f, 0.75f);
        bordo.effectDistance = bordoMarcato
            ? new Vector2(3f, -3f)
            : new Vector2(1f, -1f);
        bordo.useGraphicAlpha = true;
        return oggetto;
    }

    TMP_Text CreaTesto(
        string nome,
        Transform parent,
        string contenuto,
        Vector2 posizione,
        Vector2 dimensioni,
        float dimensioneFont,
        Color colore,
        FontStyles stile,
        TextAlignmentOptions allineamento
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggetto.transform.SetParent(parent, false);

        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;

        TextMeshProUGUI testo = oggetto.GetComponent<TextMeshProUGUI>();
        if (fontInterfaccia != null) testo.font = fontInterfaccia;
        testo.text = contenuto;
        testo.fontSize = dimensioneFont;
        testo.fontStyle = stile;
        testo.alignment = allineamento;
        testo.color = colore;
        testo.textWrappingMode = TextWrappingModes.Normal;
        testo.overflowMode = TextOverflowModes.Ellipsis;
        testo.raycastTarget = false;
        return testo;
    }

    Button CreaPulsante(
        string nome,
        Transform parent,
        string etichetta,
        Vector2 posizione,
        Vector2 dimensioni,
        Color colore,
        UnityEngine.Events.UnityAction azione
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(Outline)
        );
        oggetto.transform.SetParent(parent, false);

        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;

        Image immagine = oggetto.GetComponent<Image>();
        immagine.color = colore;

        Outline bordo = oggetto.GetComponent<Outline>();
        bordo.effectColor = new Color(0.13f, 0.06f, 0.02f, 0.9f);
        bordo.effectDistance = new Vector2(2f, -2f);

        Button pulsante = oggetto.GetComponent<Button>();
        pulsante.targetGraphic = immagine;
        ColorBlock colori = pulsante.colors;
        colori.normalColor = Color.white;
        colori.highlightedColor = new Color(1.14f, 1.08f, 0.91f, 1f);
        colori.pressedColor = new Color(0.76f, 0.76f, 0.76f, 1f);
        colori.selectedColor = Color.white;
        colori.disabledColor = new Color(0.38f, 0.38f, 0.38f, 0.72f);
        colori.colorMultiplier = 1f;
        pulsante.colors = colori;
        pulsante.onClick.AddListener(azione);

        TMP_Text testo = CreaTesto(
            "Testo",
            oggetto.transform,
            etichetta,
            Vector2.zero,
            dimensioni - new Vector2(16f, 10f),
            18f,
            new Color(1f, 0.94f, 0.77f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        return pulsante;
    }

    static string TitoloPredefinito(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento: return "PASSO PIÙ RAPIDO";
            case TipoPotenziamento.Resistenza: return "GIACCA RINFORZATA";
            case TipoPotenziamento.SaluteMassima: return "SALUTE BONUS";
            case TipoPotenziamento.Cura: return "RIMEDIO DELLA NONNA";
            case TipoPotenziamento.Danno: return "PATATE PIÙ DURE";
            case TipoPotenziamento.Cadenza: return "CARICATORE RAPIDO";
            case TipoPotenziamento.Penetrazione: return "PATATA PERFORANTE";
            default: return "POTENZIAMENTO";
        }
    }

    static string DescrizionePredefinita(TipoPotenziamento tipo)
    {
        switch (tipo)
        {
            case TipoPotenziamento.Movimento:
                return "+0,5 velocità di movimento";
            case TipoPotenziamento.Resistenza:
                return "Blocca automaticamente colpi a intervalli regolari";
            case TipoPotenziamento.SaluteMassima:
                return "+1 salute massima e cura 1";
            case TipoPotenziamento.Cura:
                return "Recupera subito 2 salute";
            case TipoPotenziamento.Danno:
                return "+1 danno per ogni patata";
            case TipoPotenziamento.Cadenza:
                return "Riduce di 0,04 s il tempo tra i colpi";
            case TipoPotenziamento.Penetrazione:
                return "+1 volpe attraversata sul colpo finale";
            default:
                return string.Empty;
        }
    }
}
