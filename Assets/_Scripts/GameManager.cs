using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum StatoPartita
{
    Onda,
    Intervallo,
    FinePartita
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int gallineRimaste = 0;
    public bool isGameOver = false;

    [SerializeField] private GameObject gameOverPanel;

    private TMP_Text testoUova;
    private TMP_Text testoUovaSalvate;
    private TMP_Text testoMonete;
    private TMP_Text titoloFinePartita;
    private TMP_Text riepilogoFinePartita;
    private Button pulsanteRiprova;
    private Button pulsanteCambiaDifficolta;
    private GameObject selettoreDifficolta;
    private ShopInterOndata shopInterOndata;
    private GameObject schedaUovaHud;
    private GameObject schedaUovaSalvateHud;
    private int gallineTotali;
    private int uovaInizioOnda;
    private float durataPartita;
    private int volpiEliminate;
    private int proiettiliSparati;
    private int proiettiliACentro;
    private int ondateCompletate;
    private int punteggioFinale;
    private bool ultimaPartitaVinta;
    private bool recordFinaliCalcolati;
    private EsitoRecordPartita recordFinali;
    private Coroutine aperturaPreparazioneRoutine;

    public int monete = 0;
    public int MoneteRaccolte { get; private set; }
    public int MoneteSpese { get; private set; }
    public int UltimoBonusCompletamento { get; private set; }
    public int GallineTotali => gallineTotali;
    public int GallineAlSicuro =>
        Mathf.Min(gallineTotali, Gallina.ContaAlSicuro());
    public int UovaSalvate { get; private set; }
    public int SerieSalvataggi { get; private set; }
    public int MiglioreSerieSalvataggi { get; private set; }
    public int UltimoBonusSerie { get; private set; }
    public int UovaUltimaOnda { get; private set; }
    public int ObiettiviCompletati { get; private set; }
    public int ObiettiviFalliti { get; private set; }
    public float DurataPartita => durataPartita;
    public int VolpiEliminate => volpiEliminate;
    public int ProiettiliSparati => proiettiliSparati;
    public int ProiettiliACentro => proiettiliACentro;
    public float Precisione => proiettiliSparati > 0
        ? Mathf.Clamp01(proiettiliACentro / (float)proiettiliSparati)
        : 0f;
    public int OndateCompletate => ondateCompletate;
    public int PunteggioFinale => punteggioFinale;
    public DifficoltaPartita DifficoltaCorrente { get; private set; }
    public bool DifficoltaConfermata { get; private set; }
    public bool PreparazioneInizialeCompletata { get; private set; }
    public string UltimoObiettivo { get; private set; } = string.Empty;
    public bool UltimoObiettivoValutato { get; private set; }
    public bool UltimoObiettivoCompletato { get; private set; }
    public int UovaUltimoObiettivo { get; private set; }
    public StatoPartita StatoCorrente { get; private set; } = StatoPartita.Onda;
    public bool PausaManualeAttiva { get; private set; }
    public bool GameplayAttivo =>
        !isGameOver &&
        DifficoltaConfermata &&
        StatoCorrente == StatoPartita.Onda &&
        !PausaManualeAttiva;
    public bool PausaInterOndataAttiva =>
        !isGameOver && StatoCorrente == StatoPartita.Intervallo;

    public event Action<int> MoneteCambiate;
    public event Action<int> UovaCambiate;
    public event Action<int, int> GallineCambiate;
    public event Action<StatoPartita> StatoPartitaCambiato;
    public event Action<bool> PausaManualeCambiata;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            monete = Mathf.Max(
                0,
                GameBalanceConfig.Corrente.Shop.moneteIniziali
            );
            MoneteRaccolte = monete;
            MoneteSpese = 0;
            UltimoBonusCompletamento = 0;
            UovaSalvate = 0;
            SerieSalvataggi = 0;
            MiglioreSerieSalvataggi = 0;
            UltimoBonusSerie = 0;
            UovaUltimaOnda = 0;
            ObiettiviCompletati = 0;
            ObiettiviFalliti = 0;
            durataPartita = 0f;
            volpiEliminate = 0;
            proiettiliSparati = 0;
            proiettiliACentro = 0;
            ondateCompletate = 0;
            punteggioFinale = 0;
            recordFinaliCalcolati = false;
            DifficoltaCorrente = ProgressionePartita.DifficoltaCorrente;
            DifficoltaConfermata =
                ProgressionePartita.ConsumaRiavvioImmediato();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ImpostaStatoPartita(StatoPartita.Intervallo);

        testoMonete = TrovaTestoInterfaccia("MoneteText");

        ConfiguraHUD();
        AggiornaContatoreMonete();

        ConfiguraPannelloFinale();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (!DifficoltaConfermata)
        {
            MostraSelettoreDifficolta();
        }

        shopInterOndata = ShopInterOndata.CreaOTrova();
        // Survival puro: nessun blocco interattivo o obiettivo-uovo.
        foreach (Gallina gallina in FindObjectsByType<Gallina>(FindObjectsSortMode.None))
        {
            if (gallina != null) Destroy(gallina.gameObject);
        }
        gallineRimaste = 0;

        if (DifficoltaConfermata)
        {
            RichiediPreparazioneIniziale();
        }
    }

    void Update()
    {
        if (!DifficoltaConfermata)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) ||
                Input.GetKeyDown(KeyCode.Keypad1))
            {
                ConfermaDifficolta(DifficoltaPartita.Tranquilla);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) ||
                     Input.GetKeyDown(KeyCode.Keypad2))
            {
                ConfermaDifficolta(DifficoltaPartita.Normale);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) ||
                     Input.GetKeyDown(KeyCode.Keypad3))
            {
                ConfermaDifficolta(DifficoltaPartita.Difficile);
            }
            return;
        }

        if (GameplayAttivo)
        {
            durataPartita += Time.unscaledDeltaTime;
        }

        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            Riprova();
        }
    }

    public static TMP_Text TrovaTestoInterfaccia(string nome)
    {
        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null) return null;

        Transform elemento = interfaccia.transform.Find(nome);
        return elemento != null ? elemento.GetComponent<TMP_Text>() : null;
    }

    void ConfiguraHUD()
    {
        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null) return;

        Canvas canvas = interfaccia.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.pixelPerfect = true;
        }
        CanvasScaler scaler = interfaccia.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.matchWidthOrHeight = 1f;
        }

        Transform pannelloEsistente = interfaccia.transform.Find("PannelloHUD");
        GameObject pannello;
        if (pannelloEsistente == null)
        {
            pannello = new GameObject(
                "PannelloHUD",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            pannello.transform.SetParent(interfaccia.transform, false);
            pannello.transform.SetAsFirstSibling();
        }
        else
        {
            pannello = pannelloEsistente.gameObject;
            pannello.transform.SetAsFirstSibling();
        }

        RectTransform rect = pannello.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(12f, -12f);
        rect.sizeDelta = new Vector2(286f, 124f);

        Image immagine = pannello.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(immagine, false, false);
        ImpostaTrasparenzaPannello(immagine, 0.70f, 0.58f);

        CreaTitoloHUD(pannello.transform);
        CreaSchedaHUD(
            pannello.transform,
            "SchedaOndata",
            FarmPixelIcon.Ondata,
            -44f
        );
        CreaSchedaHUD(
            pannello.transform,
            "SchedaVita",
            FarmPixelIcon.Cuore,
            -75f
        );
        CreaSchedaHUD(
            pannello.transform,
            "SchedaMonete",
            FarmPixelIcon.Moneta,
            -106f
        );
        schedaUovaHud = null;
        schedaUovaSalvateHud = null;

        ConfiguraTestoHUD(
            TrovaTestoInterfaccia("OndataText"),
            new Vector2(52f, -56f),
            new Color(1f, 0.77f, 0.32f, 1f)
        );
        ConfiguraTestoHUD(
            TrovaTestoInterfaccia("VitaText"),
            new Vector2(52f, -87f),
            new Color(0.5f, 0.95f, 0.47f, 1f)
        );
        ConfiguraTestoHUD(
            testoMonete,
            new Vector2(52f, -118f),
            new Color(1f, 0.9f, 0.24f, 1f)
        );
    }

    void CreaTitoloHUD(Transform parent)
    {
        if (parent.Find("TitoloFattoria") != null) return;

        GameObject oggetto = new GameObject(
            "TitoloFattoria",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggetto.transform.SetParent(parent, false);

        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -16f);
        rect.sizeDelta = new Vector2(258f, 22f);

        TextMeshProUGUI titolo = oggetto.GetComponent<TextMeshProUGUI>();
        TMP_Text riferimento = TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) titolo.font = riferimento.font;
        titolo.text = "SOPRAVVIVENZA";
        titolo.fontSize = 17f;
        titolo.fontStyle = FontStyles.Bold;
        titolo.alignment = TextAlignmentOptions.Center;
        titolo.textWrappingMode = TextWrappingModes.NoWrap;
        titolo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(
            titolo,
            new Color(1f, 0.88f, 0.55f, 1f)
        );
    }

    static GameObject CreaSchedaHUD(
        Transform parent,
        string nome,
        FarmPixelIcon icona,
        float posizioneY
    )
    {
        Transform esistente = parent.Find(nome);
        if (esistente != null) return esistente.gameObject;

        GameObject scheda = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        scheda.transform.SetParent(parent, false);

        RectTransform rect = scheda.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(11f, posizioneY);
        rect.sizeDelta = new Vector2(264f, 28f);

        Image sfondo = scheda.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(sfondo, true, false);
        ImpostaTrasparenzaPannello(sfondo, 0.58f, 0.44f);

        FarmPixelUI.AggiungiIcona(
            scheda.transform,
            "Icona",
            icona,
            new Vector2(-111f, 0f),
            new Vector2(22f, 22f)
        );
        return scheda;
    }

    static void ImpostaTrasparenzaPannello(
        Image immagine,
        float alphaSfondo,
        float alphaBordo
    )
    {
        if (immagine == null) return;

        Color colore = immagine.color;
        colore.a = Mathf.Clamp01(alphaSfondo);
        immagine.color = colore;

        Outline bordo = immagine.GetComponent<Outline>();
        if (bordo == null) return;
        Color coloreBordo = bordo.effectColor;
        coloreBordo.a = Mathf.Clamp01(alphaBordo);
        bordo.effectColor = coloreBordo;
        bordo.effectDistance = new Vector2(1f, 1f);
    }

    static void ConfiguraTestoHUD(
        TMP_Text testo,
        Vector2 posizione,
        Color colore
    )
    {
        if (testo == null) return;

        RectTransform rect = testo.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = new Vector2(230f, 26f);

        testo.fontSize = 20f;
        testo.fontStyle = FontStyles.Bold;
        testo.alignment = TextAlignmentOptions.MidlineLeft;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.overflowMode = TextOverflowModes.Overflow;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(testo, colore);
    }

    static TMP_Text CreaTestoHUDRuntime(Transform parent, string nome)
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggetto.transform.SetParent(parent, false);

        TextMeshProUGUI testo = oggetto.GetComponent<TextMeshProUGUI>();
        TMP_Text riferimento = TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) testo.font = riferimento.font;
        testo.raycastTarget = false;
        return testo;
    }

    void ConfiguraPannelloFinale()
    {
        if (gameOverPanel == null) return;

        RectTransform pannelloRect = gameOverPanel.GetComponent<RectTransform>();
        if (pannelloRect != null)
        {
            pannelloRect.anchorMin = Vector2.zero;
            pannelloRect.anchorMax = Vector2.one;
            pannelloRect.offsetMin = Vector2.zero;
            pannelloRect.offsetMax = Vector2.zero;
        }

        Image sfondo = gameOverPanel.GetComponent<Image>();
        if (sfondo != null)
        {
            sfondo.color = FarmPixelUI.ColoreVeloFlat;
        }

        Transform pannelloRiepilogo =
            gameOverPanel.transform.Find("PannelloRiepilogoFinale");
        GameObject pannelloRiepilogoOggetto;
        if (pannelloRiepilogo == null)
        {
            pannelloRiepilogoOggetto = new GameObject(
                "PannelloRiepilogoFinale",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            pannelloRiepilogoOggetto.transform.SetParent(
                gameOverPanel.transform,
                false
            );
        }
        else
        {
            pannelloRiepilogoOggetto = pannelloRiepilogo.gameObject;
        }

        RectTransform pannelloRiepilogoRect =
            pannelloRiepilogoOggetto.GetComponent<RectTransform>();
        pannelloRiepilogoRect.anchorMin = new Vector2(0.5f, 0.5f);
        pannelloRiepilogoRect.anchorMax = new Vector2(0.5f, 0.5f);
        pannelloRiepilogoRect.pivot = new Vector2(0.5f, 0.5f);
        pannelloRiepilogoRect.anchoredPosition = Vector2.zero;
        pannelloRiepilogoRect.sizeDelta = new Vector2(980f, 720f);

        FarmPixelUI.ApplicaPannello(
            pannelloRiepilogoOggetto.GetComponent<Image>(),
            false,
            false
        );
        pannelloRiepilogoOggetto.transform.SetAsFirstSibling();

        Transform titoloTransform = gameOverPanel.transform.Find("Text");
        titoloFinePartita = titoloTransform != null
            ? titoloTransform.GetComponent<TMP_Text>()
            : gameOverPanel.GetComponentInChildren<TMP_Text>(true);

        if (titoloFinePartita != null)
        {
            RectTransform rect = titoloFinePartita.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 290f);
            rect.sizeDelta = new Vector2(620f, 90f);

            titoloFinePartita.fontSize = 50f;
            titoloFinePartita.fontStyle = FontStyles.Bold;
            titoloFinePartita.alignment = TextAlignmentOptions.Center;
            titoloFinePartita.color = FarmPixelUI.TestoTitoloFlat;
            titoloFinePartita.raycastTarget = false;
            FarmPixelUI.ApplicaTesto(
                titoloFinePartita,
                titoloFinePartita.color
            );
        }

        Transform riepilogoEsistente =
            gameOverPanel.transform.Find("RiepilogoPartita");
        if (riepilogoEsistente == null)
        {
            GameObject oggetto = new GameObject(
                "RiepilogoPartita",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );
            oggetto.transform.SetParent(gameOverPanel.transform, false);
            riepilogoFinePartita =
                oggetto.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            riepilogoFinePartita =
                riepilogoEsistente.GetComponent<TMP_Text>();
        }

        if (riepilogoFinePartita != null)
        {
            RectTransform rect = riepilogoFinePartita.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 12f);
            rect.sizeDelta = new Vector2(900f, 480f);

            if (titoloFinePartita != null)
            {
                riepilogoFinePartita.font = titoloFinePartita.font;
            }
            riepilogoFinePartita.fontSize = 19f;
            riepilogoFinePartita.fontStyle = FontStyles.Bold;
            riepilogoFinePartita.alignment =
                TextAlignmentOptions.Center;
            riepilogoFinePartita.color =
                FarmPixelUI.TestoChiaroFlat;
            riepilogoFinePartita.textWrappingMode =
                TextWrappingModes.Normal;
            riepilogoFinePartita.raycastTarget = false;
            FarmPixelUI.ApplicaTesto(
                riepilogoFinePartita,
                riepilogoFinePartita.color
            );
        }

        pulsanteRiprova = gameOverPanel.GetComponentInChildren<Button>(true);
        if (pulsanteRiprova != null)
        {
            RectTransform rect = pulsanteRiprova.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(150f, -310f);
            rect.sizeDelta = new Vector2(270f, 58f);

            TMP_Text testoPulsante =
                pulsanteRiprova.GetComponentInChildren<TMP_Text>(true);
            if (testoPulsante != null)
            {
                testoPulsante.text = "RIPROVA  [R]";
                testoPulsante.fontSize = 23f;
                testoPulsante.fontStyle = FontStyles.Bold;
                FarmPixelUI.ApplicaTesto(
                    testoPulsante,
                    FarmPixelUI.TestoPulsanteFlat
                );
            }

            FarmPixelUI.ApplicaPulsante(
                pulsanteRiprova,
                FarmPixelUI.ColorePulsanteOroFlat
            );
            pulsanteRiprova.onClick.AddListener(Riprova);
        }

        Transform cambiaEsistente =
            gameOverPanel.transform.Find("PulsanteCambiaDifficolta");
        GameObject cambiaOggetto;
        if (cambiaEsistente == null)
        {
            cambiaOggetto = new GameObject(
                "PulsanteCambiaDifficolta",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            );
            cambiaOggetto.transform.SetParent(gameOverPanel.transform, false);
        }
        else
        {
            cambiaOggetto = cambiaEsistente.gameObject;
        }

        RectTransform cambiaRect = cambiaOggetto.GetComponent<RectTransform>();
        cambiaRect.anchorMin = new Vector2(0.5f, 0.5f);
        cambiaRect.anchorMax = new Vector2(0.5f, 0.5f);
        cambiaRect.pivot = new Vector2(0.5f, 0.5f);
        cambiaRect.anchoredPosition = new Vector2(-150f, -310f);
        cambiaRect.sizeDelta = new Vector2(270f, 58f);

        pulsanteCambiaDifficolta = cambiaOggetto.GetComponent<Button>();
        TMP_Text testoCambia =
            cambiaOggetto.GetComponentInChildren<TMP_Text>(true);
        if (testoCambia == null)
        {
            testoCambia = CreaTestoPannello(
                cambiaOggetto.transform,
                "TestoCambiaDifficolta",
                "CAMBIA DIFFICOLTA",
                19f
            );
            RectTransform testoRect = testoCambia.rectTransform;
            testoRect.anchorMin = Vector2.zero;
            testoRect.anchorMax = Vector2.one;
            testoRect.offsetMin = new Vector2(10f, 4f);
            testoRect.offsetMax = new Vector2(-10f, -4f);
        }
        else
        {
            testoCambia.text = "CAMBIA DIFFICOLTA";
        }
        FarmPixelUI.ApplicaTesto(
            testoCambia,
            FarmPixelUI.TestoPulsanteFlat
        );
        FarmPixelUI.ApplicaPulsante(
            pulsanteCambiaDifficolta,
            FarmPixelUI.ColorePulsanteNeutroFlat
        );
        pulsanteCambiaDifficolta.onClick.RemoveAllListeners();
        pulsanteCambiaDifficolta.onClick.AddListener(CambiaDifficolta);
    }

    void MostraSelettoreDifficolta()
    {
        if (selettoreDifficolta == null)
        {
            CostruisciSelettoreDifficolta();
        }
        if (selettoreDifficolta == null) return;

        selettoreDifficolta.SetActive(true);
        selettoreDifficolta.transform.SetAsLastSibling();
        AggiornaScalaTemporale();
    }

    void CostruisciSelettoreDifficolta()
    {
        Transform parent = gameOverPanel != null
            ? gameOverPanel.transform.parent
            : null;
        if (parent == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null) parent = canvas.transform;
        }
        if (parent == null)
        {
            Debug.LogError(
                "Impossibile creare il selettore della difficolta: " +
                "Canvas non trovato."
            );
            return;
        }

        selettoreDifficolta = new GameObject(
            "SelettoreDifficolta",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        selettoreDifficolta.transform.SetParent(parent, false);
        RectTransform overlayRect =
            selettoreDifficolta.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        Image overlay = selettoreDifficolta.GetComponent<Image>();
        overlay.sprite = null;
        overlay.type = Image.Type.Simple;
        overlay.color = FarmPixelUI.ColoreVeloFlat;
        overlay.raycastTarget = true;

        GameObject pannello = new GameObject(
            "PannelloDifficolta",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        pannello.transform.SetParent(selettoreDifficolta.transform, false);
        RectTransform pannelloRect = pannello.GetComponent<RectTransform>();
        pannelloRect.anchorMin = new Vector2(0.5f, 0.5f);
        pannelloRect.anchorMax = new Vector2(0.5f, 0.5f);
        pannelloRect.pivot = new Vector2(0.5f, 0.5f);
        pannelloRect.anchoredPosition = Vector2.zero;
        pannelloRect.sizeDelta = new Vector2(900f, 650f);
        FarmPixelUI.ApplicaPannello(
            pannello.GetComponent<Image>(),
            false,
            false
        );

        TMP_Text titolo = CreaTestoPannello(
            pannello.transform,
            "TitoloDifficolta",
            "SCEGLI LA DIFFICOLTA",
            38f
        );
        ConfiguraRettangoloCentrato(
            titolo.rectTransform,
            new Vector2(0f, 255f),
            new Vector2(760f, 70f)
        );
        FarmPixelUI.ApplicaTesto(titolo, FarmPixelUI.TestoTitoloFlat);

        TMP_Text sottotitolo = CreaTestoPannello(
            pannello.transform,
            "SottotitoloDifficolta",
            "La scelta cambia vita, velocita e ritmo delle volpi.",
            20f
        );
        ConfiguraRettangoloCentrato(
            sottotitolo.rectTransform,
            new Vector2(0f, 205f),
            new Vector2(760f, 42f)
        );
        FarmPixelUI.ApplicaTesto(
            sottotitolo,
            FarmPixelUI.TestoChiaroFlat
        );

        BilanciamentoDifficolta bilanciamento =
            GameBalanceConfig.Corrente.Difficolta;
        CreaPulsanteDifficolta(
            pannello.transform,
            DifficoltaPartita.Tranquilla,
            bilanciamento.Ottieni(DifficoltaPartita.Tranquilla),
            new Vector2(0f, 105f),
            FarmPixelUI.ColorePulsanteNeutroFlat,
            "1"
        );
        CreaPulsanteDifficolta(
            pannello.transform,
            DifficoltaPartita.Normale,
            bilanciamento.Ottieni(DifficoltaPartita.Normale),
            new Vector2(0f, -10f),
            FarmPixelUI.ColorePulsanteVerdeFlat,
            "2"
        );
        CreaPulsanteDifficolta(
            pannello.transform,
            DifficoltaPartita.Difficile,
            bilanciamento.Ottieni(DifficoltaPartita.Difficile),
            new Vector2(0f, -125f),
            FarmPixelUI.ColorePulsanteViolaFlat,
            "3"
        );

        TMP_Text nota = CreaTestoPannello(
            pannello.transform,
            "NotaDifficolta",
            "Puoi usare i tasti 1, 2 e 3. Il record e separato per difficolta.",
            17f
        );
        ConfiguraRettangoloCentrato(
            nota.rectTransform,
            new Vector2(0f, -250f),
            new Vector2(790f, 46f)
        );
        FarmPixelUI.ApplicaTesto(nota, FarmPixelUI.TestoMetaFlat);
    }

    void CreaPulsanteDifficolta(
        Transform parent,
        DifficoltaPartita difficolta,
        ProfiloDifficolta profilo,
        Vector2 posizione,
        Color colore,
        string tasto
    )
    {
        GameObject oggetto = new GameObject(
            "Difficolta_" + difficolta,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        oggetto.transform.SetParent(parent, false);
        RectTransform rect = oggetto.GetComponent<RectTransform>();
        ConfiguraRettangoloCentrato(
            rect,
            posizione,
            new Vector2(700f, 92f)
        );

        Button pulsante = oggetto.GetComponent<Button>();
        FarmPixelUI.ApplicaPulsante(pulsante, colore);
        pulsante.onClick.AddListener(() => ConfermaDifficolta(difficolta));

        string descrizione = profilo != null
            ? profilo.descrizione
            : string.Empty;
        string nome = profilo != null
            ? profilo.Nome
            : difficolta.ToString().ToUpperInvariant();
        TMP_Text testo = CreaTestoPannello(
            oggetto.transform,
            "Testo",
            "[" + tasto + "]  " + nome + "\n" + descrizione,
            20f
        );
        testo.lineSpacing = -5f;
        RectTransform testoRect = testo.rectTransform;
        testoRect.anchorMin = Vector2.zero;
        testoRect.anchorMax = Vector2.one;
        testoRect.offsetMin = new Vector2(18f, 7f);
        testoRect.offsetMax = new Vector2(-18f, -7f);
        FarmPixelUI.ApplicaTesto(
            testo,
            FarmPixelUI.TestoPulsanteFlat
        );
    }

    TMP_Text CreaTestoPannello(
        Transform parent,
        string nome,
        string contenuto,
        float dimensione
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggetto.transform.SetParent(parent, false);
        TMP_Text testo = oggetto.GetComponent<TextMeshProUGUI>();
        testo.text = contenuto;
        testo.fontSize = dimensione;
        testo.fontStyle = FontStyles.Bold;
        testo.alignment = TextAlignmentOptions.Center;
        testo.textWrappingMode = TextWrappingModes.Normal;
        testo.raycastTarget = false;
        if (titoloFinePartita != null)
        {
            testo.font = titoloFinePartita.font;
        }
        FarmPixelUI.ApplicaTesto(testo, FarmPixelUI.TestoChiaroFlat);
        return testo;
    }

    static void ConfiguraRettangoloCentrato(
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

    public void ConfermaDifficolta(DifficoltaPartita difficolta)
    {
        if (DifficoltaConfermata) return;

        DifficoltaCorrente = difficolta;
        ProgressionePartita.ImpostaDifficolta(difficolta);
        DifficoltaConfermata = true;
        if (selettoreDifficolta != null)
        {
            selettoreDifficolta.SetActive(false);
        }
        RichiediPreparazioneIniziale();
        FarmAudioController.RiproduciInterfaccia();
    }

    void RichiediPreparazioneIniziale()
    {
        if (PreparazioneInizialeCompletata || isGameOver) return;

        ImpostaStatoPartita(StatoPartita.Intervallo);
        if (aperturaPreparazioneRoutine != null)
        {
            StopCoroutine(aperturaPreparazioneRoutine);
        }
        aperturaPreparazioneRoutine = StartCoroutine(
            ApriPreparazioneInizialeQuandoPronta()
        );
    }

    IEnumerator ApriPreparazioneInizialeQuandoPronta()
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            yield return new WaitUntil(
                () => spawner == null || spawner.DifficoltaApplicata
            );
        }
        else
        {
            yield return null;
        }

        if (isGameOver || PreparazioneInizialeCompletata)
        {
            aperturaPreparazioneRoutine = null;
            yield break;
        }

        if (shopInterOndata == null)
        {
            shopInterOndata = ShopInterOndata.CreaOTrova();
        }
        if (shopInterOndata == null)
        {
            Debug.LogError("Impossibile aprire la preparazione iniziale.");
            PreparazioneInizialeCompletata = true;
            ImpostaStatoPartita(StatoPartita.Onda);
            aperturaPreparazioneRoutine = null;
            yield break;
        }

        AnteprimaOndata primaOnda = spawner != null
            ? spawner.OttieniAnteprima(0)
            : default;
        shopInterOndata.MostraPreparazioneIniziale(primaOnda);
        aperturaPreparazioneRoutine = null;
    }

    public void RegistraProiettileSparato()
    {
        if (isGameOver || !DifficoltaConfermata) return;
        proiettiliSparati++;
    }

    public void RegistraProiettileACentro()
    {
        if (isGameOver || !DifficoltaConfermata) return;
        proiettiliACentro = Mathf.Min(
            proiettiliSparati,
            proiettiliACentro + 1
        );
    }

    public void RegistraVolpeEliminata(TipoVolpe tipo)
    {
        if (isGameOver || !DifficoltaConfermata) return;
        volpiEliminate++;
    }

    public void RegistraGallina()
    {
        // Le galline della vecchia scena non partecipano al survival.
    }

    public void GallinaMorta()
    {
        if (isGameOver) return;

        gallineRimaste = Mathf.Max(0, gallineRimaste - 1);
        SerieSalvataggi = 0;
        UltimoBonusSerie = 0;
        AggiornaContatoreUova();
        AggiornaContatoreUovaSalvate();
        GallineCambiate?.Invoke(GallineAlSicuro, gallineTotali);

        // Le galline non sono una condizione di sconfitta nel survival.
    }

    void AggiornaContatoreUova()
    {
        if (testoUova != null)
        {
            testoUova.text =
                "Galline   " + GallineAlSicuro + " / " + gallineTotali;
        }
    }

    public void NotificaStatoGallineCambiato()
    {
        AggiornaContatoreUova();
        GallineCambiate?.Invoke(GallineAlSicuro, gallineTotali);
    }

    public void AggiungiUova(int quantita)
    {
        int quantitaValida = Mathf.Max(0, quantita);
        if (quantitaValida == 0) return;

        UovaSalvate += quantitaValida;
        AggiornaContatoreUovaSalvate();
        UovaCambiate?.Invoke(UovaSalvate);
    }

    public int RegistraUovoRecuperato()
    {
        FarmObjectivesBalanceSettings config =
            GameBalanceConfig.Corrente.ObiettiviFattoria;
        SerieSalvataggi++;
        MiglioreSerieSalvataggi = Mathf.Max(
            MiglioreSerieSalvataggi,
            SerieSalvataggi
        );

        int livelloBonus = Mathf.Min(
            config.bonusMassimoSerie,
            SerieSalvataggi / Mathf.Max(1, config.salvataggiPerBonusSerie)
        );
        UltimoBonusSerie =
            livelloBonus * config.uovaBonusSeriePerLivello;
        int premio = Mathf.Max(0, config.uovaPerRecupero) +
                     UltimoBonusSerie;
        AggiungiUova(premio);
        AggiornaContatoreUovaSalvate();
        return premio;
    }

    public void PreparaNuovaOnda()
    {
        uovaInizioOnda = UovaSalvate;
        UovaUltimaOnda = 0;
        UltimoBonusSerie = 0;
        UltimoObiettivo = string.Empty;
        UltimoObiettivoValutato = false;
        UltimoObiettivoCompletato = false;
        UovaUltimoObiettivo = 0;
    }

    public void RegistraEsitoObiettivo(
        string nome,
        bool completato,
        int premioUova
    )
    {
        UltimoObiettivo = nome ?? string.Empty;
        UltimoObiettivoValutato = true;
        UltimoObiettivoCompletato = completato;
        UovaUltimoObiettivo = completato
            ? Mathf.Max(0, premioUova)
            : 0;

        if (completato)
        {
            ObiettiviCompletati++;
            AggiungiUova(UovaUltimoObiettivo);
        }
        else
        {
            ObiettiviFalliti++;
        }
    }

    public void ConcludiRegistroOnda()
    {
        UovaUltimaOnda = Mathf.Max(0, UovaSalvate - uovaInizioOnda);
    }

    void AggiornaContatoreUovaSalvate()
    {
        if (testoUovaSalvate != null)
        {
            testoUovaSalvate.text =
                "Uova   " + UovaSalvate +
                "   Serie x" + SerieSalvataggi;
        }
    }

    void GameOver()
    {
        FarmAudioController.RiproduciPericolo();
        MostraFinePartita("FATTORIA PERDUTA");
    }

    public void GameOverGiocatore()
    {
        if (isGameOver) return;

        Debug.Log("Game over: il contadino e stato sconfitto.");
        FarmAudioController.RiproduciPericolo();
        MostraFinePartita("CONTADINO SCONFITTO");
    }

    void MostraFinePartita(string titolo)
    {
        if (isGameOver) return;

        isGameOver = true;
        ultimaPartitaVinta = titolo == "FATTORIA SALVA!";
        if (shopInterOndata != null)
        {
            shopInterOndata.Nascondi();
        }
        if (titoloFinePartita != null)
        {
            titoloFinePartita.text = titolo;
        }
        CalcolaRecordFinali();
        AggiornaRiepilogoFinale();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
        }
        ImpostaStatoPartita(StatoPartita.FinePartita);
    }

    public void Riprova()
    {
        ProgressionePartita.ImpostaDifficolta(DifficoltaCorrente);
        ProgressionePartita.PreparaRiavvioImmediato();
        ImpostaPausaManuale(false);
        ImpostaStatoPartita(StatoPartita.Onda);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CambiaDifficolta()
    {
        ProgressionePartita.PreparaCambioDifficolta();
        ImpostaPausaManuale(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AggiungiMonete(int quantita)
    {
        int quantitaValida = Mathf.Max(0, quantita);
        if (quantitaValida == 0) return;

        monete += quantitaValida;
        MoneteRaccolte += quantitaValida;
        AggiornaContatoreMonete();
        MoneteCambiate?.Invoke(monete);
        FarmAudioController.RiproduciMoneta();
    }

    public bool ProvaSpendiMonete(int costo)
    {
        if (costo < 0 || monete < costo) return false;

        monete -= costo;
        MoneteSpese += costo;
        AggiornaContatoreMonete();
        MoneteCambiate?.Invoke(monete);
        return true;
    }

    void AggiornaContatoreMonete()
    {
        if (testoMonete != null)
        {
            testoMonete.text = "Monete   " + monete;
        }
    }

    public void Vittoria()
    {
        if (isGameOver) return;
        FarmAudioController.RiproduciSuccesso();
        MostraFinePartita("FATTORIA SALVA!");
    }

    public int RegistraCompletamentoOnda(int indiceOnda)
    {
        if (isGameOver)
        {
            UltimoBonusCompletamento = 0;
            return 0;
        }

        ondateCompletate = Mathf.Max(ondateCompletate, indiceOnda);

        int bonus = Mathf.Max(
            0,
            GameBalanceConfig.Corrente.Shop.bonusCompletamentoOnda
        );
        UltimoBonusCompletamento = bonus;
        if (bonus > 0)
        {
            AggiungiMonete(bonus);
        }
        return bonus;
    }

    private void AggiornaRiepilogoFinale()
    {
        if (riepilogoFinePartita == null) return;

        PlayerUpgrades potenziamenti =
            FindFirstObjectByType<PlayerUpgrades>();
        string build = potenziamenti != null
            ? potenziamenti.DescriviBuildCompatta()
            : "NESSUNA BUILD";
        string buildLeggibile = build
            .Replace("  |  ", "\n")
            .Replace("  •  ", "\n");
        ProfiloDifficolta profilo =
            GameBalanceConfig.Corrente.Difficolta.Ottieni(
                DifficoltaCorrente
            );
        string migliorTempo = recordFinali.MigliorTempoVittoria > 0f
            ? ProgressionePartita.FormattaTempo(
                recordFinali.MigliorTempoVittoria
            )
            : "--:--";
        string nuovoRecordPunteggio = recordFinali.NuovoPunteggio
            ? "  NUOVO RECORD!"
            : string.Empty;
        string altriRecord = DescriviAltriRecordFinali();

        riepilogoFinePartita.text =
            "DIFFICOLTA  " + profilo.Nome +
            "  |  TEMPO  " +
            ProgressionePartita.FormattaTempo(durataPartita) +
            "\nVOLPI ELIMINATE  " + volpiEliminate +
            "  |  PRECISIONE  " +
            Mathf.RoundToInt(Precisione * 100f) + "%  (" +
            proiettiliACentro + " / " + proiettiliSparati + ")" +
            "\nMONETE RACCOLTE  " + MoneteRaccolte +
            "  |  SPESE  " + MoneteSpese +
            "  |  RIMASTE  " + monete +
            "\nONDATE SUPERATE  " + ondateCompletate +
            "\nPUNTEGGIO  " + punteggioFinale +
            nuovoRecordPunteggio + altriRecord +
            "\nRECORD " + profilo.Nome + "  " +
            recordFinali.MigliorPunteggio + " PT  |  " +
            recordFinali.MassimoVolpi + " VOLPI" +
            "\n------------------------------" +
            "\nBUILD FINALE" +
            "\n" + buildLeggibile;
    }

    private string DescriviAltriRecordFinali()
    {
        string record = string.Empty;
        if (recordFinali.NuovoRecordVolpi) record += "VOLPI ";
        if (recordFinali.NuovoRecordGalline) record += "GALLINE ";
        if (recordFinali.NuovoRecordTempo) record += "TEMPO ";
        return string.IsNullOrEmpty(record)
            ? string.Empty
            : "\nNUOVI RECORD  " + record.TrimEnd();
    }

    void CalcolaRecordFinali()
    {
        if (recordFinaliCalcolati) return;

        ProfiloDifficolta profilo =
            GameBalanceConfig.Corrente.Difficolta.Ottieni(
                DifficoltaCorrente
            );
        punteggioFinale = ProgressionePartita.CalcolaPunteggio(
            ultimaPartitaVinta,
            volpiEliminate,
            ondateCompletate,
            MoneteRaccolte,
            GallineAlSicuro,
            UovaSalvate,
            ObiettiviCompletati,
            Precisione,
            profilo.moltiplicatorePunteggio
        );
        recordFinali = ProgressionePartita.SalvaRecord(
            DifficoltaCorrente,
            ultimaPartitaVinta,
            punteggioFinale,
            durataPartita,
            volpiEliminate,
            GallineAlSicuro,
            gallineTotali
        );
        recordFinaliCalcolati = true;
    }

    public void IniziaIntervallo(int ondaCompletata, int totaleOndate)
    {
        IniziaIntervallo(
            ondaCompletata,
            totaleOndate,
            default
        );
    }

    public void IniziaIntervallo(
        int ondaCompletata,
        int totaleOndate,
        AnteprimaOndata prossimaOnda
    )
    {
        if (isGameOver) return;

        if (shopInterOndata == null)
        {
            shopInterOndata = ShopInterOndata.CreaOTrova();
        }

        if (shopInterOndata == null)
        {
            Debug.LogError(
                "Impossibile creare la schermata tra le ondate."
            );
            ImpostaStatoPartita(StatoPartita.Onda);
            return;
        }

        ImpostaStatoPartita(StatoPartita.Intervallo);
        shopInterOndata.Mostra(
            ondaCompletata,
            totaleOndate,
            prossimaOnda
        );
    }

    public void ContinuaConOndataSuccessiva()
    {
        if (isGameOver ||
            PausaManualeAttiva ||
            StatoCorrente != StatoPartita.Intervallo)
        {
            return;
        }

        if (shopInterOndata != null &&
            shopInterOndata.PreparazioneInizialeAttiva &&
            shopInterOndata.ScelteGratuiteRimaste > 0)
        {
            return;
        }

        if (!PreparazioneInizialeCompletata)
        {
            PreparazioneInizialeCompletata = true;
        }

        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.RichiediAvvioRapido();
        }

        if (shopInterOndata != null)
        {
            shopInterOndata.Nascondi();
        }
        ImpostaStatoPartita(StatoPartita.Onda);
    }

    public void ImpostaPausaManuale(bool attiva)
    {
        if (PausaManualeAttiva == attiva)
        {
            AggiornaScalaTemporale();
            return;
        }

        PausaManualeAttiva = attiva;
        AggiornaScalaTemporale();
        PausaManualeCambiata?.Invoke(attiva);
    }

    void ImpostaStatoPartita(StatoPartita nuovoStato)
    {
        bool cambiato = StatoCorrente != nuovoStato;
        StatoCorrente = nuovoStato;
        AggiornaScalaTemporale();
        if (cambiato)
        {
            StatoPartitaCambiato?.Invoke(nuovoStato);
        }
    }

    void AggiornaScalaTemporale()
    {
        Time.timeScale =
            DifficoltaConfermata &&
            StatoCorrente == StatoPartita.Onda &&
            !PausaManualeAttiva
                ? 1f
                : 0f;
    }
}
