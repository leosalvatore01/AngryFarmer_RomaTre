using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu di pausa e accessibilita' creato interamente a runtime. Rimane
/// disponibile durante l'onda, nello shop e nella schermata finale.
/// </summary>
public sealed class PauseSettingsMenu : MonoBehaviour
{
    private static readonly Color Crema =
        FarmPixelUI.TestoChiaroFlat;
    private static readonly Color Oro =
        FarmPixelUI.TestoTitoloFlat;
    private static readonly Color Verde =
        FarmPixelUI.TestoConfrontoFlat;
    private static readonly Color RossoMorbido =
        FarmPixelUI.TestoErroreFlat;
    private static readonly Color TestoSecondario =
        FarmPixelUI.TestoMetaFlat;

    private Canvas canvas;
    private GameObject pannelloOverlay;
    private Button pulsanteApri;
    private Button pulsanteChiudi;
    private TMP_Text testoTitolo;
    private TMP_Text testoPulsanteChiudi;
    private TMP_Text testoValoreMusica;
    private TMP_Text testoValoreEffetti;
    private TMP_Text testoValoreMirino;
    private TMP_Text testoStatoVibrazione;
    private TMP_Text testoStatoFlash;
    private TMP_Text testoStatoNumeriDanno;
    private Slider sliderMusica;
    private Slider sliderEffetti;
    private Slider sliderMirino;
    private Toggle toggleVibrazione;
    private Toggle toggleFlash;
    private Toggle toggleNumeriDanno;
    private TMP_FontAsset fontInterfaccia;
    private GameOptionsController opzioni;
    private bool costruito;

    public static PauseSettingsMenu Instance { get; private set; }
    public bool Aperto =>
        pannelloOverlay != null && pannelloOverlay.activeSelf;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatoStatico()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreaDopoCaricamentoScena()
    {
        CreaOTrova();
    }

    public static PauseSettingsMenu CreaOTrova()
    {
        if (Instance != null) return Instance;

        PauseSettingsMenu esistente =
            FindFirstObjectByType<PauseSettingsMenu>();
        if (esistente != null)
        {
            Instance = esistente;
            return esistente;
        }

        GameObject radice = new GameObject(
            "MenuPausaOpzioni",
            typeof(RectTransform)
        );
        return radice.AddComponent<PauseSettingsMenu>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        opzioni = GameOptionsController.CreaOTrova();
        CostruisciInterfaccia();
        opzioni.ImpostazioniCambiate += AggiornaControlli;
        AggiornaControlli();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += ScenaCaricata;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= ScenaCaricata;
    }

    private void OnDestroy()
    {
        if (opzioni != null)
        {
            opzioni.ImpostazioniCambiate -= AggiornaControlli;
        }

        if (Instance == this)
        {
            GameManager gestore = GameManager.instance;
            if (Aperto && gestore != null && gestore.PausaManualeAttiva)
            {
                gestore.ImpostaPausaManuale(false);
            }
            Instance = null;
        }
    }

    private void Update()
    {
        AggiornaVisibilitaPulsanteApri();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AlternaMenu();
        }
    }

    private void AggiornaVisibilitaPulsanteApri()
    {
        if (pulsanteApri == null || Aperto) return;
        GameManager gestore = GameManager.instance;
        bool visibile = gestore == null || gestore.DifficoltaConfermata;
        if (pulsanteApri.gameObject.activeSelf != visibile)
        {
            pulsanteApri.gameObject.SetActive(visibile);
        }
    }

    public void AlternaMenu()
    {
        GameManager gestore = GameManager.instance;
        if (!Aperto && gestore != null &&
            !gestore.DifficoltaConfermata)
        {
            return;
        }
        if (Aperto) Nascondi();
        else Mostra();
    }

    public void Mostra()
    {
        GameManager gestoreSelezione = GameManager.instance;
        if (gestoreSelezione != null &&
            !gestoreSelezione.DifficoltaConfermata)
        {
            return;
        }
        if (!costruito || Aperto) return;

        pannelloOverlay.SetActive(true);
        pulsanteApri.gameObject.SetActive(false);
        FarmAudioController.RiproduciInterfaccia();

        GameManager gestore = GameManager.instance;
        if (gestore != null && !gestore.PausaManualeAttiva)
        {
            gestore.ImpostaPausaManuale(true);
        }

        AggiornaControlli();
        AggiornaTestiStatoPartita();
    }

    public void Nascondi()
    {
        if (!costruito || !Aperto) return;

        pannelloOverlay.SetActive(false);
        pulsanteApri.gameObject.SetActive(true);
        FarmAudioController.RiproduciInterfaccia();
        opzioni.Salva();

        GameManager gestore = GameManager.instance;
        if (gestore != null && gestore.PausaManualeAttiva)
        {
            gestore.ImpostaPausaManuale(false);
        }

    }

    private void ScenaCaricata(Scene scena, LoadSceneMode modalita)
    {
        if (!costruito) return;

        pannelloOverlay.SetActive(false);
        pulsanteApri.gameObject.SetActive(true);
    }

    private void CostruisciInterfaccia()
    {
        if (costruito) return;
        costruito = true;

        TMP_Text testoHUD = GameManager.TrovaTestoInterfaccia("OndataText");
        if (testoHUD != null) fontInterfaccia = testoHUD.font;

        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 2400;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
        gameObject.AddComponent<GraphicRaycaster>();

        pulsanteApri = CreaPulsante(
            "ApriPausaOpzioni",
            transform,
            "PAUSA [ESC]",
            new Vector2(-22f, 22f),
            new Vector2(250f, 58f),
            FarmPixelUI.ColorePulsanteNeutroFlat,
            Mostra,
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f)
        );

        pannelloOverlay = CreaImmagine(
            "OverlayPausa",
            transform,
            Vector2.zero,
            Vector2.zero,
            FarmPixelUI.ColoreVeloFlat,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f)
        ).gameObject;
        RectTransform overlayRect =
            pannelloOverlay.GetComponent<RectTransform>();
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        GameObject pannello = CreaImmagine(
            "PannelloOpzioni",
            pannelloOverlay.transform,
            Vector2.zero,
            new Vector2(780f, 820f),
            Color.white
        ).gameObject;
        FarmPixelUI.ApplicaPannello(
            pannello.GetComponent<Image>(),
            false,
            true
        );

        FarmPixelUI.AggiungiIcona(
            pannello.transform,
            "IconaBottegaSinistra",
            FarmPixelIcon.Bottega,
            new Vector2(-320f, 350f),
            new Vector2(46f, 46f)
        );
        FarmPixelUI.AggiungiIcona(
            pannello.transform,
            "IconaBottegaDestra",
            FarmPixelIcon.Bottega,
            new Vector2(320f, 350f),
            new Vector2(46f, 46f)
        );

        testoTitolo = CreaTesto(
            "Titolo",
            pannello.transform,
            "PAUSA NEL FIENILE",
            new Vector2(0f, 354f),
            new Vector2(550f, 46f),
            31f,
            Oro,
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        CreaTesto(
            "Sottotitolo",
            pannello.transform,
            "SUONI, COMFORT E LEGGIBILITÀ",
            new Vector2(0f, 310f),
            new Vector2(620f, 30f),
            22f,
            Crema,
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        sliderMusica = CreaSlider(
            pannello.transform,
            "VolumeMusica",
            "MUSICA",
            new Vector2(0f, 238f),
            0f,
            1f,
            false,
            out testoValoreMusica
        );
        sliderEffetti = CreaSlider(
            pannello.transform,
            "VolumeEffetti",
            "EFFETTI SONORI",
            new Vector2(0f, 144f),
            0f,
            1f,
            false,
            out testoValoreEffetti
        );
        sliderMirino = CreaSlider(
            pannello.transform,
            "DimensioneMirino",
            "DIMENSIONE MIRINO",
            new Vector2(0f, 50f),
            GameOptionsController.DimensioneMirinoMinima,
            GameOptionsController.DimensioneMirinoMassima,
            true,
            out testoValoreMirino
        );

        toggleVibrazione = CreaToggle(
            pannello.transform,
            "Vibrazione",
            "VIBRAZIONE CAMERA",
            new Vector2(0f, -56f),
            out testoStatoVibrazione
        );
        toggleFlash = CreaToggle(
            pannello.transform,
            "Flash",
            "FLASH DI IMPATTO",
            new Vector2(0f, -128f),
            out testoStatoFlash
        );
        toggleNumeriDanno = CreaToggle(
            pannello.transform,
            "NumeriDanno",
            "NUMERI DEL DANNO",
            new Vector2(0f, -200f),
            out testoStatoNumeriDanno
        );

        CreaTesto(
            "SuggerimentoComandi",
            pannello.transform,
            "ESC: APRI O CHIUDI  -  MOUSE: REGOLA LE OPZIONI",
            new Vector2(0f, -276f),
            new Vector2(670f, 32f),
            20f,
            Crema,
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );

        CreaPulsante(
            "Ripristina",
            pannello.transform,
            "PREDEFINITI",
            new Vector2(-176f, -346f),
            new Vector2(260f, 58f),
            FarmPixelUI.ColorePulsanteNeutroFlat,
            opzioni.RipristinaPredefiniti
        );
        pulsanteChiudi = CreaPulsante(
            "Chiudi",
            pannello.transform,
            "RIPRENDI",
            new Vector2(176f, -346f),
            new Vector2(330f, 58f),
            FarmPixelUI.ColorePulsanteVerdeFlat,
            Nascondi
        );
        testoPulsanteChiudi =
            pulsanteChiudi.GetComponentInChildren<TMP_Text>();

        sliderMusica.onValueChanged.AddListener(
            opzioni.ImpostaVolumeMusica
        );
        sliderEffetti.onValueChanged.AddListener(
            opzioni.ImpostaVolumeEffetti
        );
        sliderMirino.onValueChanged.AddListener(
            opzioni.ImpostaDimensioneMirino
        );
        toggleVibrazione.onValueChanged.AddListener(
            opzioni.ImpostaVibrazioneAttiva
        );
        toggleFlash.onValueChanged.AddListener(
            opzioni.ImpostaFlashAttivi
        );
        toggleNumeriDanno.onValueChanged.AddListener(
            opzioni.ImpostaNumeriDannoAttivi
        );

        pannelloOverlay.SetActive(false);
    }

    private void AggiornaControlli()
    {
        if (!costruito || opzioni == null) return;

        sliderMusica.SetValueWithoutNotify(opzioni.VolumeMusica);
        sliderEffetti.SetValueWithoutNotify(opzioni.VolumeEffetti);
        sliderMirino.SetValueWithoutNotify(opzioni.DimensioneMirino);
        toggleVibrazione.SetIsOnWithoutNotify(opzioni.VibrazioneAttiva);
        toggleFlash.SetIsOnWithoutNotify(opzioni.FlashAttivi);
        toggleNumeriDanno.SetIsOnWithoutNotify(
            opzioni.NumeriDannoAttivi
        );

        testoValoreMusica.text =
            Mathf.RoundToInt(opzioni.VolumeMusica * 100f) + "%";
        testoValoreEffetti.text =
            Mathf.RoundToInt(opzioni.VolumeEffetti * 100f) + "%";
        testoValoreMirino.text =
            Mathf.RoundToInt(opzioni.DimensioneMirino) + " px";
        AggiornaStatoToggle(
            testoStatoVibrazione,
            opzioni.VibrazioneAttiva
        );
        AggiornaStatoToggle(testoStatoFlash, opzioni.FlashAttivi);
        AggiornaStatoToggle(
            testoStatoNumeriDanno,
            opzioni.NumeriDannoAttivi
        );
    }

    private void AggiornaTestiStatoPartita()
    {
        GameManager gestore = GameManager.instance;
        bool duranteOnda = gestore != null &&
            gestore.StatoCorrente == StatoPartita.Onda &&
            !gestore.isGameOver;

        testoTitolo.text = duranteOnda
            ? "PAUSA NEL FIENILE"
            : "OPZIONI DELLA FATTORIA";
        testoPulsanteChiudi.text = duranteOnda ? "RIPRENDI" : "CHIUDI";
    }

    private static void AggiornaStatoToggle(TMP_Text testo, bool attivo)
    {
        testo.text = attivo ? "ATTIVO" : "DISATTIVO";
        testo.color = attivo ? Verde : RossoMorbido;
    }

    private Slider CreaSlider(
        Transform parent,
        string nome,
        string etichetta,
        Vector2 posizione,
        float minimo,
        float massimo,
        bool valoriInteri,
        out TMP_Text testoValore
    )
    {
        GameObject radice = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(Slider)
        );
        radice.transform.SetParent(parent, false);
        RectTransform rect = radice.GetComponent<RectTransform>();
        ImpostaRectCentrato(rect, posizione, new Vector2(650f, 82f));

        CreaTesto(
            "Etichetta",
            radice.transform,
            etichetta,
            new Vector2(-180f, 23f),
            new Vector2(350f, 28f),
            20f,
            Crema,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineLeft
        );
        testoValore = CreaTesto(
            "Valore",
            radice.transform,
            "--",
            new Vector2(255f, 23f),
            new Vector2(120f, 28f),
            20f,
            Oro,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineRight
        );

        Image sfondo = CreaImmagine(
            "Binario",
            radice.transform,
            new Vector2(0f, -18f),
            new Vector2(620f, 28f),
            new Color(0.35f, 0.18f, 0.075f, 1f)
        );
        FarmPixelUI.ApplicaPannello(sfondo, true, true);

        GameObject areaRiempimento = new GameObject(
            "AreaRiempimento",
            typeof(RectTransform)
        );
        areaRiempimento.transform.SetParent(sfondo.transform, false);
        RectTransform areaRect =
            areaRiempimento.GetComponent<RectTransform>();
        areaRect.anchorMin = new Vector2(0f, 0.5f);
        areaRect.anchorMax = new Vector2(1f, 0.5f);
        areaRect.offsetMin = new Vector2(10f, -6f);
        areaRect.offsetMax = new Vector2(-10f, 6f);

        Image riempimento = CreaImmagine(
            "Riempimento",
            areaRiempimento.transform,
            Vector2.zero,
            Vector2.zero,
            Oro,
            Vector2.zero,
            Vector2.one,
            new Vector2(0f, 0.5f)
        );
        RectTransform riempimentoRect = riempimento.rectTransform;
        riempimentoRect.offsetMin = Vector2.zero;
        riempimentoRect.offsetMax = Vector2.zero;

        GameObject areaManiglia = new GameObject(
            "AreaManiglia",
            typeof(RectTransform)
        );
        areaManiglia.transform.SetParent(sfondo.transform, false);
        RectTransform areaManigliaRect =
            areaManiglia.GetComponent<RectTransform>();
        areaManigliaRect.anchorMin = Vector2.zero;
        areaManigliaRect.anchorMax = Vector2.one;
        areaManigliaRect.offsetMin = new Vector2(10f, 0f);
        areaManigliaRect.offsetMax = new Vector2(-10f, 0f);

        Image maniglia = CreaImmagine(
            "Maniglia",
            areaManiglia.transform,
            Vector2.zero,
            new Vector2(28f, 42f),
            FarmPixelUI.ColorePulsanteOroFlat
        );
        maniglia.sprite = null;
        maniglia.type = Image.Type.Simple;

        Slider slider = radice.GetComponent<Slider>();
        slider.minValue = minimo;
        slider.maxValue = massimo;
        slider.wholeNumbers = valoriInteri;
        slider.direction = Slider.Direction.LeftToRight;
        slider.fillRect = riempimentoRect;
        slider.handleRect = maniglia.rectTransform;
        slider.targetGraphic = maniglia;
        slider.transition = Selectable.Transition.ColorTint;

        ColorBlock colori = slider.colors;
        colori.normalColor = Color.white;
        colori.highlightedColor = new Color(1f, 0.86f, 0.52f, 1f);
        colori.selectedColor = new Color(1f, 0.80f, 0.35f, 1f);
        colori.pressedColor = new Color(0.78f, 0.58f, 0.28f, 1f);
        slider.colors = colori;
        return slider;
    }

    private Toggle CreaToggle(
        Transform parent,
        string nome,
        string etichetta,
        Vector2 posizione,
        out TMP_Text testoStato
    )
    {
        GameObject radice = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Toggle)
        );
        radice.transform.SetParent(parent, false);
        RectTransform rect = radice.GetComponent<RectTransform>();
        ImpostaRectCentrato(rect, posizione, new Vector2(650f, 58f));

        Image sfondo = radice.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(sfondo, true, true);

        Image casella = CreaImmagine(
            "Casella",
            radice.transform,
            new Vector2(-284f, 0f),
            new Vector2(38f, 38f),
            new Color(0.34f, 0.17f, 0.07f, 1f)
        );
        FarmPixelUI.ApplicaPannello(casella, true, false);
        Image spunta = CreaImmagine(
            "Spunta",
            casella.transform,
            Vector2.zero,
            new Vector2(22f, 22f),
            Verde
        );
        spunta.raycastTarget = false;

        CreaTesto(
            "Etichetta",
            radice.transform,
            etichetta,
            new Vector2(-78f, 0f),
            new Vector2(360f, 34f),
            20f,
            Crema,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineLeft
        );
        testoStato = CreaTesto(
            "Stato",
            radice.transform,
            "ATTIVO",
            new Vector2(230f, 0f),
            new Vector2(150f, 34f),
            18f,
            Verde,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineRight
        );

        Toggle toggle = radice.GetComponent<Toggle>();
        toggle.targetGraphic = sfondo;
        toggle.graphic = spunta;
        toggle.toggleTransition = Toggle.ToggleTransition.None;
        toggle.transition = Selectable.Transition.ColorTint;
        ColorBlock colori = toggle.colors;
        colori.normalColor = Color.white;
        colori.highlightedColor = new Color(1.08f, 1.02f, 0.89f, 1f);
        colori.selectedColor = new Color(1.05f, 0.96f, 0.77f, 1f);
        colori.pressedColor = new Color(0.72f, 0.64f, 0.53f, 1f);
        toggle.colors = colori;
        return toggle;
    }

    private Button CreaPulsante(
        string nome,
        Transform parent,
        string etichetta,
        Vector2 posizione,
        Vector2 dimensioni,
        Color tinta,
        UnityEngine.Events.UnityAction azione,
        Vector2? ancoraMinima = null,
        Vector2? ancoraMassima = null,
        Vector2? perno = null
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
        rect.anchorMin = ancoraMinima ?? new Vector2(0.5f, 0.5f);
        rect.anchorMax = ancoraMassima ?? new Vector2(0.5f, 0.5f);
        rect.pivot = perno ?? new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;

        Image immagine = oggetto.GetComponent<Image>();
        Button pulsante = oggetto.GetComponent<Button>();
        pulsante.targetGraphic = immagine;
        FarmPixelUI.ApplicaPulsante(pulsante, tinta);
        pulsante.onClick.AddListener(azione);

        CreaTesto(
            "Testo",
            oggetto.transform,
            etichetta,
            Vector2.zero,
            dimensioni - new Vector2(18f, 10f),
            20f,
            FarmPixelUI.TestoPulsanteFlat,
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        return pulsante;
    }

    private TMP_Text CreaTesto(
        string nome,
        Transform parent,
        string contenuto,
        Vector2 posizione,
        Vector2 dimensioni,
        float dimensione,
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
        ImpostaRectCentrato(rect, posizione, dimensioni);

        TextMeshProUGUI testo = oggetto.GetComponent<TextMeshProUGUI>();
        if (fontInterfaccia != null) testo.font = fontInterfaccia;
        testo.text = contenuto;
        testo.fontSize = dimensione;
        testo.fontStyle = stile;
        testo.alignment = allineamento;
        testo.color = colore;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.overflowMode = TextOverflowModes.Ellipsis;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(testo, colore);
        return testo;
    }

    private Image CreaImmagine(
        string nome,
        Transform parent,
        Vector2 posizione,
        Vector2 dimensioni,
        Color colore,
        Vector2? ancoraMinima = null,
        Vector2? ancoraMassima = null,
        Vector2? perno = null
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
        rect.anchorMin = ancoraMinima ?? new Vector2(0.5f, 0.5f);
        rect.anchorMax = ancoraMassima ?? new Vector2(0.5f, 0.5f);
        rect.pivot = perno ?? new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;

        Image immagine = oggetto.GetComponent<Image>();
        immagine.color = colore;
        return immagine;
    }

    private static void ImpostaRectCentrato(
        RectTransform rect,
        Vector2 posizione,
        Vector2 dimensioni
    )
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;
    }
}
