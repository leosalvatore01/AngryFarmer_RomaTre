using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Suggerimento iniziale breve e non bloccante. Compare soltanto quando
/// comincia davvero la prima ondata e si adatta all'ultimo dispositivo usato.
/// </summary>
public sealed class PcOnboardingController : MonoBehaviour
{
    private const float DurataMinima = 1.5f;
    private const float DurataMassima = 8f;

    private GameObject pannello;
    private TMP_Text testoComandi;
    private GameManager gestore;
    private GameOptionsController opzioni;
    private FarmerInputController input;
    private float tempoVisualizzazione;
    private bool visualizzazioneAvviata;

    public static PcOnboardingController Instance { get; private set; }
    public bool Visibile => pannello != null && pannello.activeSelf;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatoStatico()
    {
        Instance = null;
    }

    public static PcOnboardingController CreaOTrova()
    {
        if (Instance != null) return Instance;

        PcOnboardingController esistente =
            FindFirstObjectByType<PcOnboardingController>();
        if (esistente != null)
        {
            Instance = esistente;
            return esistente;
        }

        if (GameManager.instance == null) return null;
        GameObject radice = new GameObject(
            "TutorialControlliPc",
            typeof(RectTransform)
        );
        return radice.AddComponent<PcOnboardingController>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gestore = GameManager.instance;
        opzioni = GameOptionsController.CreaOTrova();
        input = FarmerInputController.CreaOTrova();
        CostruisciInterfaccia();
        input.SchemaCambiato += SchemaCambiato;
    }

    private void OnDestroy()
    {
        if (input != null) input.SchemaCambiato -= SchemaCambiato;
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (gestore == null || opzioni == null || input == null) return;

        bool deveMostrare = DeveMostrare(
            opzioni.TutorialCompletato,
            gestore.GameplayAttivo,
            gestore.PreparazioneInizialeCompletata
        );
        if (!visualizzazioneAvviata)
        {
            if (!deveMostrare) return;
            AvviaVisualizzazione();
        }

        pannello.SetActive(gestore.GameplayAttivo);
        if (!gestore.GameplayAttivo) return;

        tempoVisualizzazione += Time.unscaledDeltaTime;
        bool interazione = input.Movimento.sqrMagnitude > 0.04f ||
            input.FuocoPremuto || input.SchivataPremutaQuestoFrame;
        if (tempoVisualizzazione >= DurataMassima ||
            (tempoVisualizzazione >= DurataMinima && interazione))
        {
            CompletaTutorial();
        }
    }

    public void RichiediNuovaVisualizzazione()
    {
        visualizzazioneAvviata = false;
        tempoVisualizzazione = 0f;
        if (pannello != null) pannello.SetActive(false);
        if (opzioni != null)
        {
            opzioni.ImpostaTutorialCompletato(false);
            opzioni.Salva();
        }
    }

    public static bool DeveMostrare(
        bool tutorialCompletato,
        bool gameplayAttivo,
        bool preparazioneCompletata
    )
    {
        return !tutorialCompletato && gameplayAttivo &&
               preparazioneCompletata;
    }

    private void AvviaVisualizzazione()
    {
        visualizzazioneAvviata = true;
        tempoVisualizzazione = 0f;
        AggiornaTesto(input.SchemaCorrente);
        pannello.SetActive(true);
    }

    private void CompletaTutorial()
    {
        pannello.SetActive(false);
        opzioni.ImpostaTutorialCompletato(true);
        opzioni.Salva();
    }

    private void SchemaCambiato(SchemaInputContadino schema)
    {
        if (visualizzazioneAvviata) AggiornaTesto(schema);
    }

    private void AggiornaTesto(SchemaInputContadino schema)
    {
        if (schema == SchemaInputContadino.Gamepad)
        {
            testoComandi.text =
                "LEVETTA SINISTRA  MUOVITI    •    LEVETTA DESTRA  MIRA    •    RT  SPARA    •    A  SCHIVA    •    MENU  PAUSA";
            return;
        }

        testoComandi.text =
            input.OttieniNomeBinding(ComandoRimappabile.MovimentoSu) +
            input.OttieniNomeBinding(ComandoRimappabile.MovimentoSinistra) +
            input.OttieniNomeBinding(ComandoRimappabile.MovimentoGiu) +
            input.OttieniNomeBinding(ComandoRimappabile.MovimentoDestra) +
            "  MUOVITI    •    MOUSE  MIRA    •    " +
            input.OttieniNomeBinding(ComandoRimappabile.Fuoco) +
            "  SPARA    •    " +
            input.OttieniNomeBinding(ComandoRimappabile.Schivata) +
            "  SCHIVA    •    " +
            input.OttieniNomeBinding(ComandoRimappabile.Pausa) +
            "  PAUSA";
    }

    private void CostruisciInterfaccia()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1800;
        canvas.pixelPerfect = true;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        pannello = new GameObject(
            "SuggerimentoComandi",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        pannello.transform.SetParent(transform, false);
        RectTransform rect = pannello.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 42f);
        rect.sizeDelta = new Vector2(1510f, 78f);
        Image sfondo = pannello.GetComponent<Image>();
        sfondo.color = new Color(0.16f, 0.075f, 0.025f, 0.86f);
        FarmPixelUI.ApplicaPannello(sfondo, true, true);

        GameObject oggettoTesto = new GameObject(
            "Testo",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggettoTesto.transform.SetParent(pannello.transform, false);
        RectTransform testoRect = oggettoTesto.GetComponent<RectTransform>();
        testoRect.anchorMin = Vector2.zero;
        testoRect.anchorMax = Vector2.one;
        testoRect.offsetMin = new Vector2(26f, 10f);
        testoRect.offsetMax = new Vector2(-26f, -10f);
        testoComandi = oggettoTesto.GetComponent<TextMeshProUGUI>();
        TMP_Text riferimento = GameManager.TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) testoComandi.font = riferimento.font;
        testoComandi.fontSize = 20f;
        testoComandi.fontStyle = FontStyles.Bold;
        testoComandi.alignment = TextAlignmentOptions.Center;
        testoComandi.textWrappingMode = TextWrappingModes.NoWrap;
        testoComandi.overflowMode = TextOverflowModes.Ellipsis;
        testoComandi.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(
            testoComandi,
            FarmPixelUI.TestoChiaroFlat
        );
        pannello.SetActive(false);
    }
}
