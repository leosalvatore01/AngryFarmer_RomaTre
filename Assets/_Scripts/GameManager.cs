using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum StatoPartita
{
    Onda = 0,
    Intervallo = 1,
    FinePartita = 2,
    Transizione = 3
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool isGameOver = false;

    [SerializeField] private GameObject gameOverPanel;

    private TMP_Text testoMonete;
    private Image barraVitaHud;
    private PlayerHealth saluteHud;
    private float tempoFeedbackMonete;
    private TMP_Text titoloFinePartita;
    private TMP_Text riepilogoFinePartita;
    private Button pulsanteRiprova;
    private Button pulsanteCambiaDifficolta;
    private GameObject selettoreDifficolta;
    private TMP_Text testoAccessoShopPermanente;
    private ShopInterOndata shopInterOndata;
    private ShopPermanentePrePartita shopPermanente;
    private RunTelemetryController telemetria;
    private float durataPartita;
    private int volpiEliminate;
    private int proiettiliSparati;
    private int proiettiliACentro;
    private int ondateCompletate;
    private int bonusProvvistePermanentiApplicato;
    private int punteggioFinale;
    private bool recordFinaliCalcolati;
    private EsitoRecordPartita recordFinali;
    private Coroutine aperturaPreparazioneRoutine;

    public int monete = 0;
    public int MoneteIniziali { get; private set; }
    public int MoneteGuadagnateDurantePartita { get; private set; }
    public int MoneteRaccolte { get; private set; }
    public int MoneteSpese { get; private set; }
    public int UltimoBonusCompletamento { get; private set; }
    public int GettoniPermanentiGuadagnati { get; private set; }
    public int UltimoBonusPermanente { get; private set; }
    public float DurataPartita => durataPartita;
    public int VolpiEliminate => volpiEliminate;
    public int ProiettiliSparati => proiettiliSparati;
    public int ProiettiliACentro => proiettiliACentro;
    public float Precisione => proiettiliSparati > 0
        ? Mathf.Clamp01(proiettiliACentro / (float)proiettiliSparati)
        : 0f;
    public int OndateCompletate => ondateCompletate;
    public int PunteggioFinale => punteggioFinale;
    public RunTelemetryController Telemetria => telemetria;
    public DifficoltaPartita DifficoltaCorrente { get; private set; }
    public bool DifficoltaConfermata { get; private set; }
    public bool PreparazioneInizialeCompletata { get; private set; }
    public bool PuoAprireShopPermanente =>
        !isGameOver &&
        !PreparazioneInizialeCompletata &&
        !PausaManualeAttiva &&
        StatoCorrente == StatoPartita.Intervallo;
    public StatoPartita StatoCorrente { get; private set; } = StatoPartita.Onda;
    public bool PausaManualeAttiva { get; private set; }
    public bool GameplayAttivo =>
        !MobileAppLifecycle.AppSospesa &&
        !isGameOver &&
        DifficoltaConfermata &&
        StatoCorrente == StatoPartita.Onda &&
        !PausaManualeAttiva;
    public bool PausaInterOndataAttiva =>
        !isGameOver &&
        (StatoCorrente == StatoPartita.Intervallo ||
         StatoCorrente == StatoPartita.Transizione);

    public event Action<int> MoneteCambiate;
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
            MoneteIniziali = monete;
            MoneteGuadagnateDurantePartita = 0;
            MoneteSpese = 0;
            UltimoBonusCompletamento = 0;
            GettoniPermanentiGuadagnati = 0;
            UltimoBonusPermanente = 0;
            durataPartita = 0f;
            volpiEliminate = 0;
            proiettiliSparati = 0;
            proiettiliACentro = 0;
            ondateCompletate = 0;
            bonusProvvistePermanentiApplicato = 0;
            punteggioFinale = 0;
            recordFinaliCalcolati = false;
            DifficoltaCorrente = ProgressionePartita.DifficoltaCorrente;
            DifficoltaConfermata =
                ProgressionePartita.ConsumaRiavvioImmediato();
            telemetria = RunTelemetryController.CreaOTrova(this);
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

        shopInterOndata = ShopInterOndata.CreaOTrova();
        shopPermanente = ShopPermanentePrePartita.CreaOTrova();
        PcOnboardingController.CreaOTrova();
        SincronizzaBonusPermanentiPrimaPartita();

        if (DifficoltaConfermata)
        {
            telemetria?.IniziaSessione(
                DifficoltaCorrente,
                MoneteIniziali
            );
        }

        if (!DifficoltaConfermata)
        {
            MostraSelettoreDifficolta();
        }

        if (DifficoltaConfermata)
        {
            RichiediPreparazioneIniziale();
        }
    }

    void Update()
    {
        tempoFeedbackMonete = Mathf.Max(0f, tempoFeedbackMonete - Time.deltaTime);
        if (testoMonete != null)
            testoMonete.rectTransform.localScale = Vector3.one *
                (1f + Mathf.Sin(Mathf.Clamp01(tempoFeedbackMonete / 0.22f) * Mathf.PI) * 0.08f);
        if (saluteHud == null) saluteHud = FindFirstObjectByType<PlayerHealth>();
        if (barraVitaHud != null && saluteHud != null)
        {
            float rapporto = (float)saluteHud.VitaCorrente / Mathf.Max(1, saluteHud.VitaMassima);
            barraVitaHud.rectTransform.sizeDelta = new Vector2(244f * Mathf.Clamp01(rapporto), 6f);
            barraVitaHud.color = rapporto > 0.5f ? new Color32(151, 201, 111, 255)
                : rapporto > 0.25f ? new Color32(245, 192, 85, 255) : new Color32(239, 99, 76, 255);
        }
        FarmerInputController input = FarmerInputController.CreaOTrova();
        if (ShopPermanentePrePartita.ApertoGlobale)
        {
            return;
        }

        if (!DifficoltaConfermata)
        {
            if (input.SceltaRapidaPremutaQuestoFrame(1))
            {
                ConfermaDifficolta(DifficoltaPartita.Tranquilla);
            }
            else if (input.SceltaRapidaPremutaQuestoFrame(2))
            {
                ConfermaDifficolta(DifficoltaPartita.Normale);
            }
            else if (input.SceltaRapidaPremutaQuestoFrame(3))
            {
                ConfermaDifficolta(DifficoltaPartita.Difficile);
            }
            return;
        }

        if (GameplayAttivo)
        {
            durataPartita += Time.unscaledDeltaTime;
        }

        if (isGameOver && input.RiprovaPremutoQuestoFrame)
        {
            Riprova();
        }
    }

    private void OnApplicationFocus(bool haFocus)
    {
        if (!DevePausarePerPerditaFocus(haFocus, GameplayAttivo)) return;

        ImpostaPausaManuale(true);
        PauseSettingsMenu.CreaOTrova()?.Mostra();
    }

    public static bool DevePausarePerPerditaFocus(
        bool haFocus,
        bool gameplayAttivo
    )
    {
        return !haFocus && gameplayAttivo;
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
        if (canvas != null) canvas.pixelPerfect = true;
        CanvasScaler scaler = interfaccia.GetComponent<CanvasScaler>();
        if (scaler != null) scaler.matchWidthOrHeight = 1f;

        GameObject pannello = CreaPannelloHud(interfaccia.transform, "PannelloHUD",
            new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(300f, 80f));
        FarmPixelUI.AggiungiIcona(pannello.transform, "IconaVita", FarmPixelIcon.Cuore,
            new Vector2(-121f, 12f), new Vector2(26f, 26f));
        FarmPixelUI.AggiungiIcona(pannello.transform, "IconaMonete", FarmPixelIcon.Moneta,
            new Vector2(24f, 12f), new Vector2(24f, 24f));
        ConfiguraTestoHUD(TrovaTestoInterfaccia("VitaText"), new Vector2(0f, 1f),
            new Vector2(72f, -44f), new Vector2(108f, 32f), 26f,
            new Color32(191, 226, 159, 255), TextAlignmentOptions.MidlineLeft);
        ConfiguraTestoHUD(testoMonete, new Vector2(0f, 1f), new Vector2(212f, -44f),
            new Vector2(88f, 32f), 24f, new Color32(255, 214, 117, 255), TextAlignmentOptions.MidlineLeft);

        GameObject fondo = new GameObject("FondoVita", typeof(RectTransform), typeof(Image));
        fondo.transform.SetParent(pannello.transform, false);
        RectTransform rect = fondo.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(28f, -62f);
        rect.sizeDelta = new Vector2(244f, 6f);
        fondo.GetComponent<Image>().color = new Color32(66, 62, 43, 255);
        fondo.GetComponent<Image>().raycastTarget = false;
        GameObject pieno = new GameObject("VitaRimanente", typeof(RectTransform), typeof(Image));
        pieno.transform.SetParent(fondo.transform, false);
        barraVitaHud = pieno.GetComponent<Image>();
        barraVitaHud.raycastTarget = false;
        barraVitaHud.rectTransform.anchorMin = barraVitaHud.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        barraVitaHud.rectTransform.pivot = new Vector2(0f, 0.5f);

        CreaPannelloHud(interfaccia.transform, "PannelloOndataCompatto", new Vector2(0.5f, 1f),
            new Vector2(0f, -16f), new Vector2(260f, 86f));
        ConfiguraTestoHUD(TrovaTestoInterfaccia("OndataText"), new Vector2(0.5f, 1f),
            new Vector2(0f, -38f), new Vector2(240f, 28f), 23f,
            new Color32(255, 231, 182, 255), TextAlignmentOptions.Center);
    }

    private static GameObject CreaPannelloHud(Transform parent, string nome, Vector2 ancora,
        Vector2 posizione, Vector2 dimensioni)
    {
        GameObject pannello = new GameObject(nome, typeof(RectTransform), typeof(Image));
        pannello.transform.SetParent(parent, false);
        pannello.transform.SetAsFirstSibling();
        RectTransform rect = pannello.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = ancora;
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;
        Image sfondo = pannello.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(sfondo, true, false);
        sfondo.color = new Color32(38, 34, 26, 210);
        sfondo.raycastTarget = false;
        Outline bordo = sfondo.GetComponent<Outline>();
        if (bordo != null)
        {
            bordo.effectColor = new Color32(168, 134, 71, 95);
            bordo.effectDistance = Vector2.one;
        }
        return pannello;
    }

    private static void ConfiguraTestoHUD(TMP_Text testo, Vector2 ancora, Vector2 posizione,
        Vector2 dimensioni, float font, Color colore, TextAlignmentOptions allineamento)
    {
        if (testo == null) return;
        RectTransform rect = testo.rectTransform;
        rect.anchorMin = rect.anchorMax = ancora;
        rect.pivot = new Vector2(ancora.x, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;
        testo.fontSize = font;
        testo.fontStyle = FontStyles.Bold;
        testo.alignment = allineamento;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.overflowMode = TextOverflowModes.Ellipsis;
        testo.enableAutoSizing = true;
        testo.fontSizeMin = 14f;
        testo.fontSizeMax = font;
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
            if (MobileExperience.Attiva) rect.sizeDelta = new Vector2(270, 100);

            TMP_Text testoPulsante =
                pulsanteRiprova.GetComponentInChildren<TMP_Text>(true);
            if (testoPulsante != null)
            {
                testoPulsante.text = MobileExperience.Attiva ? "RIPROVA" : "RIPROVA  [R]";
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
        if (MobileExperience.Attiva) cambiaRect.sizeDelta = new Vector2(270, 100);

        pulsanteCambiaDifficolta = cambiaOggetto.GetComponent<Button>();
        TMP_Text testoCambia =
            cambiaOggetto.GetComponentInChildren<TMP_Text>(true);
        if (testoCambia == null)
        {
            testoCambia = CreaTestoPannello(
                cambiaOggetto.transform,
                "TestoCambiaDifficolta",
                "MENU PRINCIPALE",
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
            testoCambia.text = "MENU PRINCIPALE";
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
        pulsanteCambiaDifficolta.onClick.AddListener(TornaAlMenuPrincipale);
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
        AggiornaAccessoShopPermanente();
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
        pannelloRect.sizeDelta = new Vector2(900f, 760f);
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
            new Vector2(0f, 305f),
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
            new Vector2(0f, 255f),
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
            new Vector2(0f, 155f),
            FarmPixelUI.ColorePulsanteNeutroFlat,
            "1"
        );
        CreaPulsanteDifficolta(
            pannello.transform,
            DifficoltaPartita.Normale,
            bilanciamento.Ottieni(DifficoltaPartita.Normale),
            new Vector2(0f, 40f),
            FarmPixelUI.ColorePulsanteVerdeFlat,
            "2"
        );
        CreaPulsanteDifficolta(
            pannello.transform,
            DifficoltaPartita.Difficile,
            bilanciamento.Ottieni(DifficoltaPartita.Difficile),
            new Vector2(0f, -75f),
            FarmPixelUI.ColorePulsanteViolaFlat,
            "3"
        );

        CreaAccessoShopPermanente(
            pannello.transform,
            new Vector2(0f, -184f)
        );

        TMP_Text nota = CreaTestoPannello(
            pannello.transform,
            "NotaDifficolta",
            "Puoi usare i tasti 1, 2 e 3. Il record e separato per difficolta.",
            17f
        );
        ConfiguraRettangoloCentrato(
            nota.rectTransform,
            new Vector2(0f, -305f),
            new Vector2(790f, 46f)
        );
        FarmPixelUI.ApplicaTesto(nota, FarmPixelUI.TestoMetaFlat);
    }

    void CreaAccessoShopPermanente(Transform parent, Vector2 posizione)
    {
        GameObject oggetto = new GameObject(
            "ApriShopPermanente",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        oggetto.transform.SetParent(parent, false);
        ConfiguraRettangoloCentrato(
            oggetto.GetComponent<RectTransform>(),
            posizione,
            new Vector2(700f, 68f)
        );

        Button pulsante = oggetto.GetComponent<Button>();
        FarmPixelUI.ApplicaPulsante(
            pulsante,
            FarmPixelUI.ColorePulsanteOroFlat
        );
        pulsante.onClick.AddListener(ApriShopPermanente);

        FarmPixelUI.AggiungiIcona(
            oggetto.transform,
            "Icona",
            FarmPixelIcon.Bottega,
            new Vector2(-300f, 0f),
            new Vector2(38f, 38f)
        );
        testoAccessoShopPermanente = CreaTestoPannello(
            oggetto.transform,
            "Testo",
            string.Empty,
            21f
        );
        RectTransform testoRect =
            testoAccessoShopPermanente.rectTransform;
        testoRect.anchorMin = Vector2.zero;
        testoRect.anchorMax = Vector2.one;
        testoRect.offsetMin = new Vector2(58f, 6f);
        testoRect.offsetMax = new Vector2(-20f, -6f);
        FarmPixelUI.ApplicaTesto(
            testoAccessoShopPermanente,
            FarmPixelUI.TestoPulsanteFlat
        );
        AggiornaAccessoShopPermanente();
    }

    void AggiornaAccessoShopPermanente()
    {
        if (testoAccessoShopPermanente == null) return;

        testoAccessoShopPermanente.text =
            "MIGLIORAMENTI PERMANENTI  |  " +
            ProgressionePermanente.SaldoGettoni + " GETTONI";
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
        if (DifficoltaConfermata ||
            ShopPermanentePrePartita.ApertoGlobale)
        {
            return;
        }

        DifficoltaCorrente = difficolta;
        ProgressionePartita.ImpostaDifficolta(difficolta);
        DifficoltaConfermata = true;
        telemetria?.IniziaSessione(
            DifficoltaCorrente,
            MoneteIniziali
        );
        if (selettoreDifficolta != null)
        {
            selettoreDifficolta.SetActive(false);
        }
        RichiediPreparazioneIniziale();
        FarmAudioController.RiproduciInterfaccia();
    }

    public void ApriShopPermanente()
    {
        if (!PuoAprireShopPermanente) return;

        if (shopPermanente == null)
        {
            shopPermanente = ShopPermanentePrePartita.CreaOTrova();
        }
        shopPermanente?.Mostra();
    }

    public void SincronizzaBonusPermanentiPrimaPartita()
    {
        if (isGameOver ||
            PreparazioneInizialeCompletata ||
            ondateCompletate > 0 ||
            StatoCorrente != StatoPartita.Intervallo)
        {
            return;
        }

        GameObject giocatore = GameObject.FindGameObjectWithTag("Player");
        if (giocatore != null)
        {
            PlayerHealth salute = giocatore.GetComponent<PlayerHealth>();
            PlayerMovement movimento =
                giocatore.GetComponent<PlayerMovement>();
            PlayerShooting sparo =
                giocatore.GetComponent<PlayerShooting>();

            salute?.ImpostaBonusVitaMassimaPermanente(
                ProgressionePermanente.BonusVitaMassima
            );
            salute?.ImpostaProbabilitaBloccoPermanente(
                ProgressionePermanente.BonusProbabilitaBlocco
            );
            movimento?.ImpostaBonusVelocitaPermanente(
                ProgressionePermanente.BonusVelocitaMovimento
            );
            sparo?.ImpostaBonusDannoPermanente(
                ProgressionePermanente.BonusDanno
            );
            sparo?.ImpostaBonusRiduzioneIntervalloSparoPermanente(
                ProgressionePermanente.BonusRiduzioneIntervalloSparo
            );
        }

        int bonusProvviste = ProgressionePermanente.BonusMoneteIniziali;
        int differenza =
            bonusProvviste - bonusProvvistePermanentiApplicato;
        if (differenza != 0)
        {
            long nuovoSaldo = (long)monete + differenza;
            monete = (int)Math.Max(
                0L,
                Math.Min(int.MaxValue, nuovoSaldo)
            );
            bonusProvvistePermanentiApplicato = bonusProvviste;
            AggiornaContatoreMonete();
            MoneteCambiate?.Invoke(monete);
        }
        MoneteIniziali = monete;
        AggiornaAccessoShopPermanente();
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

    public void GameOverGiocatore()
    {
        if (isGameOver) return;

        Debug.Log("Game over: il contadino e stato sconfitto.");
        FarmAudioController.RiproduciPericolo();
        MostraFinePartita();
    }

    void MostraFinePartita()
    {
        if (isGameOver) return;

        isGameOver = true;
        if (shopInterOndata != null)
        {
            shopInterOndata.Nascondi();
        }
        if (shopPermanente != null)
        {
            shopPermanente.Nascondi();
        }
        if (titoloFinePartita != null)
        {
            titoloFinePartita.text = "CONTADINO SCONFITTO";
        }
        CalcolaRecordFinali();
        AggiornaRiepilogoFinale();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
        }
        ImpostaStatoPartita(StatoPartita.FinePartita);
        telemetria?.ConcludiPartita(true);
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
        TornaAlMenuPrincipale();
    }

    public void TornaAlMenuPrincipale()
    {
        ProgressionePartita.PreparaCambioDifficolta();
        ImpostaPausaManuale(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuIniziale");
    }

    public void AggiungiMonete(int quantita)
    {
        int quantitaValida = Mathf.Max(0, quantita);
        if (quantitaValida == 0) return;

        int saldoPrecedente = monete;
        monete = SommaSaturata(monete, quantitaValida);
        int quantitaAccreditata = monete - saldoPrecedente;
        if (quantitaAccreditata <= 0) return;

        MoneteRaccolte = SommaSaturata(
            MoneteRaccolte,
            quantitaAccreditata
        );
        MoneteGuadagnateDurantePartita = SommaSaturata(
            MoneteGuadagnateDurantePartita,
            quantitaAccreditata
        );
        AggiornaContatoreMonete();
        MoneteCambiate?.Invoke(monete);
        if (GameplayAttivo && (CombatFeedbackController.Instance == null || CombatFeedbackController.Instance.VfxAbilitati))
            tempoFeedbackMonete = 0.22f;
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
            testoMonete.text = monete.ToString();
        }
    }

    public int RegistraCompletamentoOnda(int indiceOnda)
    {
        if (isGameOver)
        {
            UltimoBonusCompletamento = 0;
            UltimoBonusPermanente = 0;
            return 0;
        }

        int ultimaOndaValida = Mathf.Max(0, indiceOnda);
        if (ondateCompletate == int.MaxValue)
        {
            UltimoBonusCompletamento = 0;
            UltimoBonusPermanente = 0;
            return 0;
        }
        int primaOndaNonPremiata = ondateCompletate + 1;
        if (ultimaOndaValida < primaOndaNonPremiata)
        {
            UltimoBonusCompletamento = 0;
            UltimoBonusPermanente = 0;
            return 0;
        }

        int nuoveOndate =
            ultimaOndaValida - primaOndaNonPremiata + 1;
        ondateCompletate = ultimaOndaValida;

        int bonusPerOnda = Mathf.Max(
            0,
            GameBalanceConfig.Corrente.Shop.bonusCompletamentoOnda
        );
        int bonus = ProdottoSaturato(bonusPerOnda, nuoveOndate);
        UltimoBonusCompletamento = bonus;
        if (bonus > 0)
        {
            AggiungiMonete(bonus);
        }

        int ricompensaPermanente =
            CalcolaRicompensePermanentiIntervallo(
            primaOndaNonPremiata,
            ultimaOndaValida
        );
        UltimoBonusPermanente =
            ProgressionePermanente.AggiungiGettoni(
                ricompensaPermanente
            );
        GettoniPermanentiGuadagnati = SommaSaturata(
            GettoniPermanentiGuadagnati,
            UltimoBonusPermanente
        );
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
            "\nGETTONI PERMANENTI  +" +
            GettoniPermanentiGuadagnati +
            "  |  SALDO  " +
            ProgressionePermanente.SaldoGettoni +
            "\nONDATE SUPERATE  " + ondateCompletate +
            "\nPUNTEGGIO  " + punteggioFinale +
            nuovoRecordPunteggio + altriRecord +
            "\nRECORD " + profilo.Nome + "  " +
            recordFinali.MigliorPunteggio + " PT  |  " +
            recordFinali.MassimoVolpi + " VOLPI  |  ONDATA " +
            recordFinali.MassimaOndata +
            "\n------------------------------" +
            "\nBUILD FINALE" +
            "\n" + buildLeggibile;
    }

    private string DescriviAltriRecordFinali()
    {
        string record = string.Empty;
        if (recordFinali.NuovoRecordVolpi) record += "VOLPI ";
        if (recordFinali.NuovoRecordOndata) record += "ONDATA ";
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
            volpiEliminate,
            ondateCompletate,
            MoneteRaccolte,
            Precisione,
            profilo.moltiplicatorePunteggio
        );
        recordFinali = ProgressionePartita.SalvaRecord(
            DifficoltaCorrente,
            punteggioFinale,
            volpiEliminate,
            ondateCompletate
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

    public void IniziaTransizioneBreve()
    {
        if (isGameOver) return;

        shopInterOndata?.Nascondi();
        ImpostaStatoPartita(StatoPartita.Transizione);
    }

    public void ConcludiTransizioneBreve()
    {
        if (isGameOver || StatoCorrente != StatoPartita.Transizione)
        {
            return;
        }

        ImpostaStatoPartita(StatoPartita.Onda);
    }

    public void ContinuaConOndataSuccessiva()
    {
        if (isGameOver ||
            PausaManualeAttiva ||
            ShopPermanentePrePartita.ApertoGlobale ||
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
            !PausaManualeAttiva && !MobileAppLifecycle.AppSospesa
                ? 1f
                : 0f;
    }

    public static int CalcolaRicompensePermanentiIntervallo(
        int primaOnda,
        int ultimaOnda
    )
    {
        if (ultimaOnda < primaOnda || ultimaOnda <= 0) return 0;

        long totaleFinoAllaFine =
            SommaRicompensePermanentiFinoA(ultimaOnda);
        long totalePrima =
            SommaRicompensePermanentiFinoA(primaOnda - 1);
        long differenza = Math.Max(0L, totaleFinoAllaFine - totalePrima);
        return differenza >= int.MaxValue
            ? int.MaxValue
            : (int)differenza;
    }

    private static long SommaRicompensePermanentiFinoA(int onda)
    {
        if (onda <= 0) return 0L;

        long gruppiCompleti = (onda - 1L) / 10L;
        long ondeUltimoGruppo = (onda - 1L) % 10L + 1L;
        double totale =
            10d * gruppiCompleti * (gruppiCompleti + 1d) / 2d +
            ondeUltimoGruppo * (gruppiCompleti + 1d);
        return totale >= long.MaxValue
            ? long.MaxValue
            : (long)totale;
    }

    private static int ProdottoSaturato(int primo, int secondo)
    {
        long prodotto =
            (long)Mathf.Max(0, primo) * Mathf.Max(0, secondo);
        return prodotto >= int.MaxValue
            ? int.MaxValue
            : (int)prodotto;
    }

    private static int SommaSaturata(int primo, int secondo)
    {
        long somma =
            (long)Mathf.Max(0, primo) + Mathf.Max(0, secondo);
        return somma >= int.MaxValue ? int.MaxValue : (int)somma;
    }
}
