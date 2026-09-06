using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Indicatore minimale della schivata: mostra comando, stato e ricarica senza
/// occupare la zona centrale del combattimento.
/// </summary>
[DisallowMultipleComponent]
public sealed class DodgeHudController : MonoBehaviour
{
    private const float LarghezzaBarra = 76f;

    private CanvasGroup gruppo;
    private Image icona;
    private Image riempimento;
    private TMP_Text testoStato;
    private TMP_Text testoComando;
    private PlayerMovement movimento;
    private FarmerInputController input;
    private float prossimoCollegamento;

    public bool Visibile => gruppo != null && gruppo.alpha > 0f;
    public float Riempimento => movimento != null
        ? movimento.ProgressoCooldownSchivata
        : 0f;
    public string StatoVisualizzato => testoStato != null
        ? testoStato.text
        : string.Empty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreaDopoCaricamentoScena()
    {
        SceneManager.sceneLoaded -= ScenaCaricata;
        SceneManager.sceneLoaded += ScenaCaricata;
        CreaOTrova();
    }

    private static void ScenaCaricata(Scene scena, LoadSceneMode modalita)
    {
        CreaOTrova();
    }

    public static DodgeHudController CreaOTrova()
    {
        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null || interfaccia.GetComponent<Canvas>() == null)
            return null;

        DodgeHudController esistente =
            interfaccia.GetComponentInChildren<DodgeHudController>(true);
        if (esistente != null) return esistente;

        GameObject radice = new GameObject(
            "SchivataHUD",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup)
        );
        radice.transform.SetParent(interfaccia.transform, false);
        radice.transform.SetAsLastSibling();
        return radice.AddComponent<DodgeHudController>();
    }

    private void Awake()
    {
        CostruisciInterfaccia();
        input = FarmerInputController.CreaOTrova();
        CollegaGiocatore();
        AggiornaInterfaccia();
    }

    private void Update()
    {
        if (movimento == null && Time.unscaledTime >= prossimoCollegamento)
        {
            prossimoCollegamento = Time.unscaledTime + 0.25f;
            CollegaGiocatore();
        }
        if (input == null) input = FarmerInputController.CreaOTrova();
        AggiornaInterfaccia();
    }

    public void AggiornaSubitoPerTest()
    {
        CollegaGiocatore();
        AggiornaInterfaccia();
    }

    private void CollegaGiocatore()
    {
        GameObject giocatore = GameObject.FindGameObjectWithTag("Player");
        movimento = giocatore != null
            ? giocatore.GetComponent<PlayerMovement>()
            : null;
    }

    private void CostruisciInterfaccia()
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(18f, 18f);
        rect.sizeDelta = new Vector2(132f, 43f);

        Image sfondo = GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(sfondo, true, false);
        sfondo.color = new Color32(35, 29, 25, 150);
        sfondo.raycastTarget = false;

        gruppo = GetComponent<CanvasGroup>();
        gruppo.interactable = false;
        gruppo.blocksRaycasts = false;

        GameObject oggettoIcona = new GameObject(
            "IconaSchivata",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        oggettoIcona.transform.SetParent(transform, false);
        RectTransform rectIcona = oggettoIcona.GetComponent<RectTransform>();
        rectIcona.anchorMin = new Vector2(0f, 0.5f);
        rectIcona.anchorMax = new Vector2(0f, 0.5f);
        rectIcona.pivot = new Vector2(0f, 0.5f);
        rectIcona.anchoredPosition = new Vector2(6f, 0f);
        rectIcona.sizeDelta = new Vector2(30f, 30f);
        icona = oggettoIcona.GetComponent<Image>();
        icona.sprite = FarmPixelUI.OttieniIcona(FarmPixelIcon.Schivata);
        icona.preserveAspect = true;
        icona.raycastTarget = false;

        testoStato = CreaTesto(
            "Stato",
            "PRONTA",
            11f,
            new Color32(182, 239, 164, 255),
            new Vector2(39f, 20f),
            new Vector2(87f, 16f)
        );
        testoComando = CreaTesto(
            "Comando",
            "SPAZIO / SHIFT",
            7f,
            new Color32(222, 205, 174, 230),
            new Vector2(39f, 7f),
            new Vector2(87f, 11f)
        );

        GameObject fondo = CreaBarra(
            "FondoCooldown",
            transform,
            new Color32(72, 58, 47, 220)
        );
        RectTransform rectFondo = fondo.GetComponent<RectTransform>();
        rectFondo.anchorMin = new Vector2(0f, 0f);
        rectFondo.anchorMax = new Vector2(0f, 0f);
        rectFondo.pivot = new Vector2(0f, 0.5f);
        rectFondo.anchoredPosition = new Vector2(43f, 4f);
        rectFondo.sizeDelta = new Vector2(LarghezzaBarra, 3f);

        GameObject pieno = CreaBarra(
            "Cooldown",
            fondo.transform,
            new Color32(75, 222, 232, 255)
        );
        riempimento = pieno.GetComponent<Image>();
        RectTransform rectPieno = pieno.GetComponent<RectTransform>();
        rectPieno.anchorMin = new Vector2(0f, 0.5f);
        rectPieno.anchorMax = new Vector2(0f, 0.5f);
        rectPieno.pivot = new Vector2(0f, 0.5f);
        rectPieno.anchoredPosition = Vector2.zero;
        rectPieno.sizeDelta = new Vector2(LarghezzaBarra, 3f);
    }

    private void AggiornaInterfaccia()
    {
        GameManager gestore = GameManager.instance;
        bool visibile = movimento != null && gestore != null &&
            gestore.DifficoltaConfermata &&
            gestore.StatoCorrente == StatoPartita.Onda &&
            !gestore.isGameOver;
        gruppo.alpha = visibile ? 1f : 0f;
        if (!visibile) return;

        float progresso = movimento.ProgressoCooldownSchivata;
        RectTransform rectBarra = riempimento.rectTransform;
        rectBarra.sizeDelta = new Vector2(
            LarghezzaBarra * progresso,
            rectBarra.sizeDelta.y
        );

        if (movimento.SchivataAttiva)
        {
            testoStato.text = "SCHIVATA!";
            testoStato.color = new Color32(105, 235, 242, 255);
            icona.color = new Color32(180, 255, 247, 255);
        }
        else if (movimento.SchivataPronta)
        {
            testoStato.text = "PRONTA";
            testoStato.color = new Color32(182, 239, 164, 255);
            icona.color = Color.white;
        }
        else
        {
            testoStato.text = movimento.CooldownSchivataRimasto
                .ToString("0.0")
                .Replace('.', ',') + "s";
            testoStato.color = new Color32(207, 193, 169, 255);
            icona.color = new Color32(145, 138, 126, 220);
        }

        testoComando.text = input != null &&
            input.SchemaCorrente == SchemaInputContadino.Gamepad
            ? "PULSANTE A"
            : "SPAZIO / SHIFT";
    }

    private TMP_Text CreaTesto(
        string nome,
        string contenuto,
        float dimensione,
        Color colore,
        Vector2 posizione,
        Vector2 dimensioni
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggetto.transform.SetParent(transform, false);
        TMP_Text testo = oggetto.GetComponent<TMP_Text>();
        FarmPixelUI.ApplicaTesto(testo, colore);
        testo.text = contenuto;
        testo.fontSize = dimensione;
        testo.alignment = TextAlignmentOptions.MidlineLeft;
        testo.raycastTarget = false;
        RectTransform rect = testo.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;
        return testo;
    }

    private static GameObject CreaBarra(
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
        Image immagine = oggetto.GetComponent<Image>();
        immagine.color = colore;
        immagine.raycastTarget = false;
        return oggetto;
    }
}
