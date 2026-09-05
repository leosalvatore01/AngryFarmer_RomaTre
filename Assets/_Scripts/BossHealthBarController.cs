using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra dedicata al boss, separata dall'HUD delle normali volpi e costruita
/// a runtime per non richiedere modifiche manuali alla scena.
/// </summary>
public sealed class BossHealthBarController : MonoBehaviour
{
    private static BossHealthBarController instance;

    private BossFoxController boss;
    private CanvasGroup gruppo;
    private Image riempimento;
    private TMP_Text titolo;
    private TMP_Text fase;

    public BossFoxController BossCorrente => boss;
    public float PercentualeVita => boss != null
        ? boss.PercentualeVita
        : 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatoRuntime()
    {
        instance = null;
    }

    public static BossHealthBarController Mostra(BossFoxController nuovoBoss)
    {
        if (nuovoBoss == null) return null;
        if (instance == null)
        {
            GameObject oggetto = new GameObject(
                "HUD_Boss",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(BossHealthBarController)
            );
            instance = oggetto.GetComponent<BossHealthBarController>();
        }

        instance.boss = nuovoBoss;
        instance.AggiornaAspetto();
        return instance;
    }

    public static void Nascondi(BossFoxController bossDaNascondere)
    {
        if (instance == null || instance.boss != bossDaNascondere) return;
        instance.boss = null;
        if (instance.gruppo != null) instance.gruppo.alpha = 0f;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1900;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CostruisciInterfaccia();
    }

    void Update()
    {
        if (boss == null || boss.IsDead)
        {
            boss = null;
            if (gruppo != null) gruppo.alpha = 0f;
            return;
        }

        AggiornaAspetto();
    }

    private void AggiornaAspetto()
    {
        if (boss == null || gruppo == null) return;

        bool gameplayVisibile = GameManager.instance == null ||
            GameManager.instance.GameplayAttivo;
        gruppo.alpha = gameplayVisibile ? 1f : 0f;

        float percentuale = Mathf.Clamp01(boss.PercentualeVita);
        riempimento.fillAmount = percentuale;
        Color colore = boss.FaseCorrente >= 2
            ? new Color32(220, 61, 45, 255)
            : new Color32(231, 151, 42, 255);
        riempimento.color = colore;
        titolo.text = boss.NomeVisualizzato.ToUpperInvariant();
        fase.text = "FASE " + boss.FaseCorrente +
            "  •  " + boss.Nemico.VitaCorrente +
            " / " + boss.Nemico.VitaMassima;
    }

    private void CostruisciInterfaccia()
    {
        GameObject pannello = new GameObject(
            "PannelloBoss",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup)
        );
        pannello.transform.SetParent(transform, false);
        RectTransform rect = pannello.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -18f);
        rect.sizeDelta = new Vector2(500f, 58f);

        Image sfondo = pannello.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(sfondo, true, false);
        sfondo.color = new Color32(47, 25, 18, 210);
        sfondo.raycastTarget = false;

        gruppo = pannello.GetComponent<CanvasGroup>();
        gruppo.interactable = false;
        gruppo.blocksRaycasts = false;
        gruppo.alpha = 0f;

        titolo = CreaTesto(
            pannello.transform,
            "NomeBoss",
            17f,
            TextAlignmentOptions.MidlineLeft
        );
        ImpostaRect(
            titolo.rectTransform,
            new Vector2(15f, -5f),
            new Vector2(300f, 20f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f)
        );
        titolo.color = FarmPixelUI.TestoTitoloFlat;

        fase = CreaTesto(
            pannello.transform,
            "FaseBoss",
            12f,
            TextAlignmentOptions.MidlineRight
        );
        ImpostaRect(
            fase.rectTransform,
            new Vector2(-15f, -6f),
            new Vector2(185f, 20f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f)
        );
        fase.color = FarmPixelUI.TestoMetaFlat;

        GameObject fondoBarra = CreaImmagine(
            pannello.transform,
            "FondoBarra",
            new Color32(38, 19, 16, 245)
        );
        ImpostaRect(
            fondoBarra.GetComponent<RectTransform>(),
            new Vector2(0f, 8f),
            new Vector2(-28f, 14f),
            Vector2.zero,
            Vector2.one
        );

        GameObject barra = CreaImmagine(
            fondoBarra.transform,
            "VitaBoss",
            new Color32(231, 151, 42, 255)
        );
        RectTransform rectBarra = barra.GetComponent<RectTransform>();
        rectBarra.anchorMin = Vector2.zero;
        rectBarra.anchorMax = Vector2.one;
        rectBarra.offsetMin = new Vector2(3f, 3f);
        rectBarra.offsetMax = new Vector2(-3f, -3f);
        riempimento = barra.GetComponent<Image>();
        riempimento.type = Image.Type.Filled;
        riempimento.fillMethod = Image.FillMethod.Horizontal;
        riempimento.fillOrigin = 0;
        riempimento.fillAmount = 1f;
    }

    private static TMP_Text CreaTesto(
        Transform parent,
        string nome,
        float dimensione,
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
        TMP_Text testo = oggetto.GetComponent<TMP_Text>();
        testo.fontSize = dimensione;
        testo.fontStyle = FontStyles.Bold;
        testo.alignment = allineamento;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.overflowMode = TextOverflowModes.Overflow;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(testo, FarmPixelUI.TestoChiaroFlat);
        return testo;
    }

    private static GameObject CreaImmagine(
        Transform parent,
        string nome,
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
        Image immagine = oggetto.GetComponent<Image>();
        immagine.sprite = FoxAbilityVfx.Pixel;
        immagine.color = colore;
        immagine.raycastTarget = false;
        return oggetto;
    }

    private static void ImpostaRect(
        RectTransform rect,
        Vector2 posizione,
        Vector2 dimensioni,
        Vector2 ancoraMin,
        Vector2 ancoraMax
    )
    {
        rect.anchorMin = ancoraMin;
        rect.anchorMax = ancoraMax;
        rect.pivot = new Vector2(
            ancoraMin.x == 1f ? 1f : 0f,
            ancoraMin.y == 1f ? 1f : 0f
        );
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;
    }
}
