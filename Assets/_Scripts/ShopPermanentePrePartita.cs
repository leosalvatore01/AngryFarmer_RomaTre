using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ShopPermanentePrePartita : MonoBehaviour
{
    private const int NumeroCarte = 6;

    private static readonly Color32 ColoreVelo =
        FarmPixelUI.ColoreVeloFlat;
    private static readonly Color32 ColorePannello =
        FarmPixelUI.ColorePannelloFlat;
    private static readonly Color32 ColoreCarta =
        FarmPixelUI.ColoreCartaFlat;
    private static readonly Color32 ColoreCartaDisabilitata =
        FarmPixelUI.ColoreCartaDisabilitataFlat;
    private static readonly Color32 ColoreBordo =
        FarmPixelUI.ColoreBordoFlat;
    private static readonly Color32 TestoChiaro =
        FarmPixelUI.TestoChiaroFlat;
    private static readonly Color32 TestoTitolo =
        FarmPixelUI.TestoTitoloFlat;
    private static readonly Color32 TestoMeta =
        FarmPixelUI.TestoMetaFlat;
    private static readonly Color32 TestoIncremento =
        FarmPixelUI.TestoConfrontoFlat;
    private static readonly Color32 TestoPulsante =
        FarmPixelUI.TestoPulsanteFlat;
    private static readonly Color32 TestoErrore =
        FarmPixelUI.TestoErroreFlat;
    private static readonly Color32 ColorePulsanteAttivo =
        FarmPixelUI.ColorePulsanteOroFlat;
    private static readonly Color32 ColorePulsanteChiusura =
        FarmPixelUI.ColorePulsanteNeutroFlat;
    private static readonly Color32 ColorePulsanteDisabilitato =
        new Color32(104, 87, 68, 255);

    private sealed class CartaPotenziamento
    {
        public TipoPotenziamentoPermanente tipo;
        public GameObject radice;
        public Image sfondo;
        public Image icona;
        public TMP_Text titolo;
        public TMP_Text descrizione;
        public TMP_Text stato;
        public TMP_Text incremento;
        public Button pulsante;
        public Image sfondoPulsante;
        public TMP_Text testoPulsante;
        public Image iconaCosto;
    }

    private static ShopPermanentePrePartita istanza;
    private static int frameInputModaleConsumata = -1;

    private readonly List<CartaPotenziamento> carte =
        new List<CartaPotenziamento>(NumeroCarte);

    private TMP_Text testoSaldo;
    private TMP_Text testoMessaggio;
    private bool costruito;
    private bool eventoSottoscritto;
    private bool accessoDaMenu;

    public static bool ApertoGlobale =>
        istanza != null &&
        istanza.isActiveAndEnabled &&
        istanza.gameObject.activeInHierarchy;
    public static bool InputModaleConsumataQuestoFrame =>
        frameInputModaleConsumata == Time.frameCount;

    public static ShopPermanentePrePartita CreaOTrova()
    {
        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null)
        {
            Debug.LogError(
                "L'oggetto Interfaccia non e presente: " +
                "shop permanente non creato."
            );
            return null;
        }

        ShopPermanentePrePartita esistente =
            interfaccia.GetComponentInChildren<ShopPermanentePrePartita>(true);
        if (esistente != null)
        {
            istanza = esistente;
            return esistente;
        }

        GameObject overlay = new GameObject(
            "ShopPermanentePrePartita",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        overlay.transform.SetParent(interfaccia.transform, false);
        return overlay.AddComponent<ShopPermanentePrePartita>();
    }

    public static ShopPermanentePrePartita CreaNelMenu(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError(
                "Lo shop permanente richiede un contenitore UI nel menu."
            );
            return null;
        }

        ShopPermanentePrePartita esistente =
            parent.GetComponentInChildren<ShopPermanentePrePartita>(true);
        if (esistente != null)
        {
            istanza = esistente;
            esistente.accessoDaMenu = true;
            return esistente;
        }

        GameObject overlay = new GameObject(
            "ShopPermanentePrePartita",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        overlay.transform.SetParent(parent, false);
        ShopPermanentePrePartita creato =
            overlay.AddComponent<ShopPermanentePrePartita>();
        creato.accessoDaMenu = true;
        return creato;
    }

    private void Awake()
    {
        if (istanza != null && istanza != this)
        {
            Destroy(gameObject);
            return;
        }

        istanza = this;
        CostruisciInterfaccia();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        SottoscriviEvento();
        if (costruito)
        {
            AggiornaInterfaccia();
        }
    }

    private void OnDisable()
    {
        RimuoviSottoscrizioneEvento();
    }

    private void OnDestroy()
    {
        RimuoviSottoscrizioneEvento();
        if (istanza == this)
        {
            istanza = null;
        }
    }

    private void Update()
    {
        if (!AccessoConsentito())
        {
            Nascondi(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            frameInputModaleConsumata = Time.frameCount;
            Nascondi();
        }
    }

    public void Mostra()
    {
        if (!AccessoConsentito())
        {
            Debug.LogWarning(
                "Lo shop permanente e disponibile solo prima della partita."
            );
            return;
        }

        if (!costruito)
        {
            CostruisciInterfaccia();
        }

        bool eraGiaAperto = gameObject.activeInHierarchy;
        if (testoMessaggio != null)
        {
            testoMessaggio.text =
                "I miglioramenti acquistati valgono per tutte le partite.";
            FarmPixelUI.ApplicaTesto(testoMessaggio, TestoMeta);
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        AggiornaInterfaccia();

        if (!eraGiaAperto)
        {
            FarmAudioController.RiproduciInterfaccia(0.85f);
        }
    }

    public void Nascondi()
    {
        Nascondi(true);
    }

    private void Nascondi(bool riproduciAudio)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        if (riproduciAudio)
        {
            FarmAudioController.RiproduciInterfaccia(0.75f);
        }

        gameObject.SetActive(false);
    }

    private void SottoscriviEvento()
    {
        if (eventoSottoscritto)
        {
            return;
        }

        ProgressionePermanente.StatoCambiato += AggiornaInterfaccia;
        eventoSottoscritto = true;
    }

    private void RimuoviSottoscrizioneEvento()
    {
        if (!eventoSottoscritto)
        {
            return;
        }

        ProgressionePermanente.StatoCambiato -= AggiornaInterfaccia;
        eventoSottoscritto = false;
    }

    private void CostruisciInterfaccia()
    {
        if (costruito)
        {
            return;
        }

        costruito = true;
        ConfiguraOverlay();

        GameObject pannello = CreaPannello(
            "PannelloShopPermanente",
            transform,
            new Vector2(1240f, 990f),
            Vector2.zero,
            false
        );

        FarmPixelUI.AggiungiIcona(
            pannello.transform,
            "IconaBottega",
            FarmPixelIcon.Bottega,
            new Vector2(-530f, 437f),
            new Vector2(58f, 58f)
        );

        CreaTesto(
            "Titolo",
            pannello.transform,
            "MIGLIORAMENTI PERMANENTI",
            new Vector2(-65f, 444f),
            new Vector2(740f, 56f),
            40f,
            TestoTitolo,
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        CreaTesto(
            "Sottotitolo",
            pannello.transform,
            "Completa ondate per guadagnare gettoni. " +
            "I livelli permanenti non hanno un limite massimo.",
            new Vector2(0f, 390f),
            new Vector2(1080f, 38f),
            20f,
            TestoChiaro,
            FontStyles.Normal,
            TextAlignmentOptions.Center
        );

        FarmPixelUI.AggiungiIcona(
            pannello.transform,
            "IconaSaldo",
            FarmPixelIcon.GettonePermanente,
            new Vector2(400f, 443f),
            new Vector2(38f, 38f)
        );
        testoSaldo = CreaTesto(
            "SaldoGettoni",
            pannello.transform,
            "GETTONI  0",
            new Vector2(500f, 443f),
            new Vector2(180f, 42f),
            24f,
            TestoTitolo,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineLeft
        );

        CreaCarte(pannello.transform);

        testoMessaggio = CreaTesto(
            "Messaggio",
            pannello.transform,
            "I miglioramenti acquistati valgono per tutte le partite.",
            new Vector2(0f, -370f),
            new Vector2(1080f, 38f),
            19f,
            TestoMeta,
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        CreaPulsante(
            "Chiudi",
            pannello.transform,
            "INDIETRO",
            new Vector2(0f, -438f),
            new Vector2(360f, 58f),
            ColorePulsanteChiusura,
            Nascondi
        );
    }

    private void ConfiguraOverlay()
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image velo = GetComponent<Image>();
        velo.sprite = null;
        velo.material = null;
        velo.type = Image.Type.Simple;
        velo.color = ColoreVelo;
        velo.raycastTarget = true;
    }

    private void CreaCarte(Transform parent)
    {
        IReadOnlyList<DefinizionePotenziamentoPermanente> catalogo =
            ProgressionePermanente.Catalogo;
        int quantita = Mathf.Min(NumeroCarte, catalogo.Count);

        if (catalogo.Count != NumeroCarte)
        {
            Debug.LogWarning(
                "Lo shop permanente prevede " + NumeroCarte +
                " potenziamenti, ma il catalogo ne contiene " +
                catalogo.Count + "."
            );
        }

        for (int indice = 0; indice < quantita; indice++)
        {
            int colonna = indice % 2;
            int riga = indice / 2;
            float posizioneX = colonna == 0 ? -292f : 292f;
            float posizioneY = 220f - riga * 220f;
            CreaCarta(
                parent,
                catalogo[indice],
                indice,
                new Vector2(posizioneX, posizioneY)
            );
        }
    }

    private void CreaCarta(
        Transform parent,
        DefinizionePotenziamentoPermanente definizione,
        int indice,
        Vector2 posizione
    )
    {
        GameObject radice = CreaPannello(
            "Potenziamento_" + (indice + 1) + "_" + definizione.Tipo,
            parent,
            new Vector2(555f, 205f),
            posizione,
            true
        );
        Image sfondo = radice.GetComponent<Image>();

        Image icona = FarmPixelUI.AggiungiIcona(
            radice.transform,
            "Icona",
            definizione.Icona,
            new Vector2(-229f, 28f),
            new Vector2(68f, 68f)
        );

        TMP_Text titolo = CreaTesto(
            "Titolo",
            radice.transform,
            definizione.Titolo.ToUpperInvariant(),
            new Vector2(23f, 75f),
            new Vector2(438f, 32f),
            23f,
            TestoTitolo,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineLeft
        );

        TMP_Text descrizione = CreaTesto(
            "Descrizione",
            radice.transform,
            definizione.Descrizione,
            new Vector2(24f, 37f),
            new Vector2(438f, 44f),
            17f,
            TestoChiaro,
            FontStyles.Normal,
            TextAlignmentOptions.MidlineLeft
        );
        descrizione.enableAutoSizing = true;
        descrizione.fontSizeMin = 14f;
        descrizione.fontSizeMax = 17f;

        TMP_Text stato = CreaTesto(
            "LivelloAttuale",
            radice.transform,
            string.Empty,
            new Vector2(-103f, -12f),
            new Vector2(300f, 29f),
            17f,
            TestoMeta,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineLeft
        );
        stato.enableAutoSizing = true;
        stato.fontSizeMin = 13f;
        stato.fontSizeMax = 17f;

        TMP_Text incremento = CreaTesto(
            "IncrementoProssimo",
            radice.transform,
            string.Empty,
            new Vector2(149f, -12f),
            new Vector2(198f, 29f),
            16f,
            TestoIncremento,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineRight
        );
        incremento.enableAutoSizing = true;
        incremento.fontSizeMin = 12f;
        incremento.fontSizeMax = 16f;

        TipoPotenziamentoPermanente tipoCatturato = definizione.Tipo;
        Button pulsante = CreaPulsante(
            "Acquista",
            radice.transform,
            string.Empty,
            new Vector2(0f, -70f),
            new Vector2(470f, 48f),
            ColorePulsanteAttivo,
            () => Acquista(tipoCatturato)
        );
        Image sfondoPulsante = pulsante.targetGraphic as Image;
        TMP_Text testoPulsante = pulsante.GetComponentInChildren<TMP_Text>();
        if (testoPulsante != null)
        {
            testoPulsante.rectTransform.anchoredPosition =
                new Vector2(17f, 0f);
            testoPulsante.rectTransform.sizeDelta =
                new Vector2(400f, 38f);
            testoPulsante.enableAutoSizing = true;
            testoPulsante.fontSizeMin = 13f;
            testoPulsante.fontSizeMax = 18f;
        }

        Image iconaCosto = FarmPixelUI.AggiungiIcona(
            pulsante.transform,
            "IconaCosto",
            FarmPixelIcon.GettonePermanente,
            new Vector2(-196f, 0f),
            new Vector2(25f, 25f)
        );

        carte.Add(new CartaPotenziamento
        {
            tipo = definizione.Tipo,
            radice = radice,
            sfondo = sfondo,
            icona = icona,
            titolo = titolo,
            descrizione = descrizione,
            stato = stato,
            incremento = incremento,
            pulsante = pulsante,
            sfondoPulsante = sfondoPulsante,
            testoPulsante = testoPulsante,
            iconaCosto = iconaCosto
        });
    }

    private void Acquista(TipoPotenziamentoPermanente tipo)
    {
        GameManager gameManager = GameManager.instance;
        if (!AccessoConsentito())
        {
            Nascondi(false);
            return;
        }

        string messaggio;
        bool acquistato = ProgressionePermanente.ProvaAcquistare(
            tipo,
            out messaggio
        );

        if (testoMessaggio != null)
        {
            testoMessaggio.text = messaggio;
            FarmPixelUI.ApplicaTesto(
                testoMessaggio,
                acquistato ? TestoIncremento : TestoErrore
            );
        }

        if (acquistato)
        {
            if (gameManager != null)
            {
                gameManager.SincronizzaBonusPermanentiPrimaPartita();
            }
            FarmAudioController.RiproduciAcquisto(0.9f);
        }
        else
        {
            FarmAudioController.RiproduciInterfaccia(0.55f);
        }

        AggiornaInterfaccia();
    }

    private bool AccessoConsentito()
    {
        if (accessoDaMenu)
        {
            return true;
        }

        GameManager gameManager = GameManager.instance;
        return gameManager != null && gameManager.PuoAprireShopPermanente;
    }

    private void AggiornaInterfaccia()
    {
        if (!costruito)
        {
            return;
        }

        int saldo = ProgressionePermanente.SaldoGettoni;
        if (testoSaldo != null)
        {
            testoSaldo.text = "GETTONI  " + saldo;
        }

        for (int indice = 0; indice < carte.Count; indice++)
        {
            CartaPotenziamento carta = carte[indice];
            int livello = ProgressionePermanente.OttieniLivello(carta.tipo);
            int costo = ProgressionePermanente.OttieniCosto(carta.tipo);
            bool acquistabile =
                ProgressionePermanente.PuoAcquistare(carta.tipo);

            carta.stato.text =
                "LV " + livello + "  |  " +
                ProgressionePermanente.DescriviBonus(carta.tipo, livello);
            carta.incremento.text =
                "PROSSIMO  " +
                ProgressionePermanente.DescriviIncrementoProssimo(carta.tipo);

            carta.pulsante.interactable = acquistabile;
            carta.sfondo.color = acquistabile
                ? ColoreCarta
                : ColoreCartaDisabilitata;
            if (carta.sfondoPulsante != null)
            {
                carta.sfondoPulsante.color = acquistabile
                    ? ColorePulsanteAttivo
                    : ColorePulsanteDisabilitato;
            }

            carta.testoPulsante.text = acquistabile
                ? "POTENZIA  |  COSTO " + costo
                : "COSTO " + costo + "  |  MANCANO " +
                    Mathf.Max(0, costo - saldo);
            FarmPixelUI.ApplicaTesto(
                carta.testoPulsante,
                acquistabile ? TestoPulsante : TestoErrore
            );

            Color tintaIcona = acquistabile
                ? Color.white
                : new Color(0.65f, 0.62f, 0.56f, 0.82f);
            carta.icona.color = tintaIcona;
            carta.iconaCosto.color = tintaIcona;
        }
    }

    private static GameObject CreaPannello(
        string nome,
        Transform parent,
        Vector2 dimensioni,
        Vector2 posizione,
        bool incassato
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
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;

        Image immagine = oggetto.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(immagine, incassato, true);

        Outline bordo = oggetto.GetComponent<Outline>();
        bordo.enabled = true;
        bordo.effectColor = ColoreBordo;
        bordo.effectDistance = incassato
            ? new Vector2(1f, 1f)
            : new Vector2(2f, 2f);
        bordo.useGraphicAlpha = true;
        return oggetto;
    }

    private static TMP_Text CreaTesto(
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
        testo.text = contenuto;
        testo.fontSize = dimensioneFont;
        testo.fontStyle = stile;
        testo.alignment = allineamento;
        testo.textWrappingMode = TextWrappingModes.Normal;
        testo.overflowMode = TextOverflowModes.Ellipsis;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(testo, colore);
        return testo;
    }

    private static Button CreaPulsante(
        string nome,
        Transform parent,
        string etichetta,
        Vector2 posizione,
        Vector2 dimensioni,
        Color colore,
        UnityAction azione
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

        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;

        Button pulsante = oggetto.GetComponent<Button>();
        pulsante.targetGraphic = oggetto.GetComponent<Image>();
        pulsante.onClick.AddListener(azione);
        FarmPixelUI.ApplicaPulsante(pulsante, colore);

        ColorBlock colori = pulsante.colors;
        colori.disabledColor = Color.white;
        pulsante.colors = colori;

        TMP_Text testo = CreaTesto(
            "Testo",
            oggetto.transform,
            etichetta,
            Vector2.zero,
            dimensioni - new Vector2(16f, 8f),
            19f,
            TestoPulsante,
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        return pulsante;
    }
}
