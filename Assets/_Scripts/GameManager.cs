using System;
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
    private TMP_Text testoMonete;
    private TMP_Text titoloFinePartita;
    private Button pulsanteRiprova;
    private ShopInterOndata shopInterOndata;
    private GameObject schedaUovaHud;

    public int monete = 0;
    public StatoPartita StatoCorrente { get; private set; } = StatoPartita.Onda;
    public bool GameplayAttivo =>
        !isGameOver && StatoCorrente == StatoPartita.Onda;
    public bool PausaInterOndataAttiva =>
        !isGameOver && StatoCorrente == StatoPartita.Intervallo;

    public event Action<int> MoneteCambiate;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ImpostaStatoPartita(StatoPartita.Onda);

        testoUova = TrovaTestoInterfaccia("UovaText");
        testoMonete = TrovaTestoInterfaccia("MoneteText");

        ConfiguraHUD();
        AggiornaContatoreUova();
        AggiornaContatoreMonete();

        ConfiguraPannelloFinale();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        shopInterOndata = ShopInterOndata.CreaOTrova();
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

        Transform pannelloEsistente = interfaccia.transform.Find("PannelloHUD");
        GameObject pannello;
        if (pannelloEsistente == null)
        {
            pannello = new GameObject(
                "PannelloHUD",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Shadow)
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
        rect.anchoredPosition = new Vector2(18f, -18f);
        rect.sizeDelta = new Vector2(356f, 224f);

        Image immagine = pannello.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(immagine, false, false);

        Shadow ombra = pannello.GetComponent<Shadow>();
        if (ombra == null) ombra = pannello.AddComponent<Shadow>();
        ombra.effectColor = new Color(0.09f, 0.04f, 0.018f, 0.9f);
        ombra.effectDistance = new Vector2(5f, -5f);
        ombra.useGraphicAlpha = true;

        CreaTitoloHUD(pannello.transform);
        CreaSchedaHUD(
            pannello.transform,
            "SchedaOndata",
            FarmPixelIcon.Ondata,
            -67f
        );
        CreaSchedaHUD(
            pannello.transform,
            "SchedaVita",
            FarmPixelIcon.Cuore,
            -108f
        );
        CreaSchedaHUD(
            pannello.transform,
            "SchedaMonete",
            FarmPixelIcon.Moneta,
            -149f
        );
        schedaUovaHud = CreaSchedaHUD(
            pannello.transform,
            "SchedaUova",
            FarmPixelIcon.Uovo,
            -190f
        );

        if (schedaUovaHud != null)
        {
            schedaUovaHud.SetActive(gallineRimaste > 0);
        }

        ConfiguraTestoHUD(
            TrovaTestoInterfaccia("OndataText"),
            new Vector2(78f, -85f),
            new Color(1f, 0.77f, 0.32f, 1f)
        );
        ConfiguraTestoHUD(
            TrovaTestoInterfaccia("VitaText"),
            new Vector2(78f, -126f),
            new Color(0.5f, 0.95f, 0.47f, 1f)
        );
        ConfiguraTestoHUD(
            testoMonete,
            new Vector2(78f, -167f),
            new Color(1f, 0.9f, 0.24f, 1f)
        );
        ConfiguraTestoHUD(
            testoUova,
            new Vector2(78f, -208f),
            new Color(1f, 0.94f, 0.76f, 1f)
        );

        if (testoUova != null)
        {
            testoUova.gameObject.SetActive(gallineRimaste > 0);
        }
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
        rect.anchoredPosition = new Vector2(0f, -26f);
        rect.sizeDelta = new Vector2(300f, 32f);

        TextMeshProUGUI titolo = oggetto.GetComponent<TextMeshProUGUI>();
        TMP_Text riferimento = TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) titolo.font = riferimento.font;
        titolo.text = "FATTORIA";
        titolo.fontSize = 21f;
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
        rect.anchoredPosition = new Vector2(14f, posizioneY);
        rect.sizeDelta = new Vector2(328f, 37f);

        Image sfondo = scheda.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(sfondo, true, false);

        FarmPixelUI.AggiungiIcona(
            scheda.transform,
            "Icona",
            icona,
            new Vector2(-136f, 0f),
            new Vector2(31f, 31f)
        );
        return scheda;
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
        rect.sizeDelta = new Vector2(230f, 32f);

        testo.fontSize = 22f;
        testo.fontStyle = FontStyles.Bold;
        testo.alignment = TextAlignmentOptions.MidlineLeft;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.overflowMode = TextOverflowModes.Overflow;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(testo, colore);
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
            sfondo.color = new Color(0.035f, 0.018f, 0.012f, 0.72f);
        }

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
            rect.anchoredPosition = new Vector2(0f, 65f);
            rect.sizeDelta = new Vector2(620f, 90f);

            titoloFinePartita.fontSize = 50f;
            titoloFinePartita.fontStyle = FontStyles.Bold;
            titoloFinePartita.alignment = TextAlignmentOptions.Center;
            titoloFinePartita.color = new Color(1f, 0.83f, 0.34f, 1f);
            titoloFinePartita.raycastTarget = false;
        }

        pulsanteRiprova = gameOverPanel.GetComponentInChildren<Button>(true);
        if (pulsanteRiprova != null)
        {
            RectTransform rect = pulsanteRiprova.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -42f);
            rect.sizeDelta = new Vector2(220f, 54f);

            TMP_Text testoPulsante =
                pulsanteRiprova.GetComponentInChildren<TMP_Text>(true);
            if (testoPulsante != null)
            {
                testoPulsante.text = "RIPROVA";
                testoPulsante.fontSize = 25f;
                testoPulsante.fontStyle = FontStyles.Bold;
            }

            pulsanteRiprova.onClick.AddListener(Riprova);
        }
    }

    public void RegistraGallina()
    {
        gallineRimaste++;

        if (testoUova == null)
        {
            testoUova = TrovaTestoInterfaccia("UovaText");
        }
        if (testoUova != null)
        {
            testoUova.gameObject.SetActive(true);
        }
        if (schedaUovaHud != null)
        {
            schedaUovaHud.SetActive(true);
        }

        AggiornaContatoreUova();
    }

    public void GallinaMorta()
    {
        if (isGameOver) return;

        gallineRimaste--;
        AggiornaContatoreUova();

        if (gallineRimaste <= 0)
        {
            GameOver();
        }
    }

    void AggiornaContatoreUova()
    {
        if (testoUova != null)
        {
            testoUova.text = "Uova protette   " + gallineRimaste;
        }
    }

    void GameOver()
    {
        MostraFinePartita("FATTORIA PERDUTA");
    }

    public void GameOverGiocatore()
    {
        if (isGameOver) return;

        Debug.Log("Game over: il contadino e stato sconfitto.");
        MostraFinePartita("CONTADINO SCONFITTO");
    }

    void MostraFinePartita(string titolo)
    {
        if (isGameOver) return;

        isGameOver = true;
        if (shopInterOndata != null)
        {
            shopInterOndata.Nascondi();
        }
        if (titoloFinePartita != null)
        {
            titoloFinePartita.text = titolo;
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
        }
        ImpostaStatoPartita(StatoPartita.FinePartita);
    }

    public void Riprova()
    {
        ImpostaStatoPartita(StatoPartita.Onda);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AggiungiMonete(int quantita)
    {
        int quantitaValida = Mathf.Max(0, quantita);
        if (quantitaValida == 0) return;

        monete += quantitaValida;
        AggiornaContatoreMonete();
        MoneteCambiate?.Invoke(monete);
    }

    public bool ProvaSpendiMonete(int costo)
    {
        if (costo < 0 || monete < costo) return false;

        monete -= costo;
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
        MostraFinePartita("FATTORIA SALVA!");
    }

    public void IniziaIntervallo(int ondaCompletata, int totaleOndate)
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
        shopInterOndata.Mostra(ondaCompletata, totaleOndate);
    }

    public void ContinuaConOndataSuccessiva()
    {
        if (isGameOver || StatoCorrente != StatoPartita.Intervallo) return;

        if (shopInterOndata != null)
        {
            shopInterOndata.Nascondi();
        }
        ImpostaStatoPartita(StatoPartita.Onda);
    }

    void ImpostaStatoPartita(StatoPartita nuovoStato)
    {
        StatoCorrente = nuovoStato;
        Time.timeScale = nuovoStato == StatoPartita.Onda ? 1f : 0f;
    }
}
