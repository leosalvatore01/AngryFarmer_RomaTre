using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Overlay touch persistente, disattivato completamente durante tutti i menu.</summary>
public sealed class MobileControls : MonoBehaviour
{
    private GameObject comandi;
    private MobileJoystick joystick;
    private Button schivata;
    private Image progresso;
    private TMP_Text testoSchivata;
    private PlayerMovement movimento;
    private float prossimoControlloCanvas;
    private Sprite disco;

    private void Awake()
    {
        disco = CreaDisco();
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1700;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        gameObject.AddComponent<GraphicRaycaster>();
        gameObject.AddComponent<MobileSafeAreaCanvas>();
        comandi = new GameObject("ComandiTouch", typeof(RectTransform));
        RectTransform root = (RectTransform)comandi.transform;
        root.SetParent(transform, false);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.sizeDelta = Vector2.zero;

        Image baseStick = Immagine("Joystick", root, new Vector2(0, 0), new Vector2(142, 138),
            new Vector2(210, 210), new Color(.12f, .10f, .06f, .28f));
        joystick = baseStick.gameObject.AddComponent<MobileJoystick>();
        baseStick.sprite = disco;
        Image manopola = Immagine("Manopola", baseStick.transform, Vector2.one * .5f,
            Vector2.zero, new Vector2(76, 76), new Color(1f, .80f, .37f, .65f));
        manopola.raycastTarget = false;
        manopola.sprite = disco;
        joystick.Manopola = manopola.rectTransform;

        Image fondo = Immagine("SchivataTouch", root, new Vector2(1, 0), new Vector2(-127, 120),
            new Vector2(160, 160), new Color(.26f, .18f, .08f, .7f));
        schivata = fondo.gameObject.AddComponent<Button>();
        schivata.targetGraphic = fondo;
        fondo.sprite = disco;
        fondo.gameObject.AddComponent<MobileDodgeButton>();
        Image icona = Immagine("Icona", fondo.transform, Vector2.one * .5f,
            new Vector2(0, 15), new Vector2(72, 72), Color.white);
        icona.sprite = FarmPixelUI.OttieniIcona(FarmPixelIcon.Schivata);
        icona.preserveAspect = true;
        icona.raycastTarget = false;
        GameObject testo = new GameObject("Disponibilita", typeof(RectTransform), typeof(TextMeshProUGUI));
        testo.transform.SetParent(fondo.transform, false);
        testoSchivata = testo.GetComponent<TMP_Text>();
        testoSchivata.rectTransform.anchoredPosition = new Vector2(0, -43);
        testoSchivata.rectTransform.sizeDelta = new Vector2(148, 32);
        testoSchivata.fontSize = 21;
        if (FarmPixelUI.FontInterfaccia != null) testoSchivata.font = FarmPixelUI.FontInterfaccia;
        testoSchivata.alignment = TextAlignmentOptions.Center;
        testoSchivata.raycastTarget = false;
        progresso = Immagine("Ricarica", fondo.transform, new Vector2(.5f, 0), new Vector2(0, 10),
            new Vector2(125, 5), new Color(1f, .8f, .3f, .9f));
        progresso.raycastTarget = false;
        comandi.SetActive(false);
    }

    private void Update()
    {
        bool attivo = MobileExperience.GameplayTouchAttivo;
        if (comandi.activeSelf != attivo)
        {
            joystick.Azzera();
            if (!attivo && FarmerInputController.Instance != null)
                FarmerInputController.Instance.AzzeraTouch();
            comandi.SetActive(attivo);
        }
        if (attivo)
        {
            if (movimento == null)
            {
                GameObject giocatore = GameObject.FindGameObjectWithTag("Player");
                if (giocatore != null) movimento = giocatore.GetComponent<PlayerMovement>();
            }
            bool pronta = movimento != null && movimento.SchivataPronta;
            schivata.interactable = pronta;
            testoSchivata.text = pronta ? "SCHIVA" : movimento != null
                ? movimento.CooldownSchivataRimasto.ToString("0.0") + "s" : "--";
            progresso.rectTransform.localScale = new Vector3(movimento != null ? movimento.ProgressoCooldownSchivata : 0, 1, 1);
        }
        if (Time.unscaledTime < prossimoControlloCanvas) return;
        prossimoControlloCanvas = Time.unscaledTime + .5f;
        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas.renderMode == RenderMode.WorldSpace || !canvas.isRootCanvas ||
                canvas.GetComponent<MobileSafeAreaCanvas>() != null) continue;
            canvas.gameObject.AddComponent<MobileSafeAreaCanvas>();
        }
    }

    private void OnApplicationFocus(bool focus) { if (!focus) joystick.Azzera(); }
    private void OnApplicationPause(bool pausa) { if (pausa) joystick.Azzera(); }

    private void OnDestroy()
    {
        if (disco == null) return;
        Destroy(disco.texture);
        Destroy(disco);
    }

    private static Sprite CreaDisco()
    {
        const int lato = 64;
        var texture = new Texture2D(lato, lato, TextureFormat.RGBA32, false);
        texture.name = "DiscoControlliTouch";
        texture.filterMode = FilterMode.Bilinear;
        Color32[] pixel = new Color32[lato * lato];
        for (int y = 0; y < lato; y++)
            for (int x = 0; x < lato; x++)
            {
                float distanza = new Vector2(x - 31.5f, y - 31.5f).magnitude;
                pixel[y * lato + x] = new Color(1, 1, 1, Mathf.Clamp01(31.5f - distanza));
            }
        texture.SetPixels32(pixel);
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0, 0, lato, lato), Vector2.one * .5f);
    }

    private static Image Immagine(string nome, Transform parent, Vector2 ancora, Vector2 posizione, Vector2 dimensioni, Color colore)
    {
        var oggetto = new GameObject(nome, typeof(RectTransform), typeof(Image));
        var immagine = oggetto.GetComponent<Image>();
        immagine.rectTransform.SetParent(parent, false);
        immagine.rectTransform.anchorMin = immagine.rectTransform.anchorMax = ancora;
        immagine.rectTransform.anchoredPosition = posizione;
        immagine.rectTransform.sizeDelta = dimensioni;
        immagine.color = colore;
        return immagine;
    }
}
