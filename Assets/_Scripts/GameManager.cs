using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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

    public int monete = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;

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

        if (interfaccia.transform.Find("PannelloHUD") == null)
        {
            GameObject pannello = new GameObject(
                "PannelloHUD",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Outline)
            );
            pannello.transform.SetParent(interfaccia.transform, false);
            pannello.transform.SetAsFirstSibling();

            RectTransform rect = pannello.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(18f, -18f);
            rect.sizeDelta = new Vector2(286f, 162f);

            Image immagine = pannello.GetComponent<Image>();
            immagine.color = new Color(0.075f, 0.04f, 0.022f, 0.82f);
            immagine.raycastTarget = false;

            Outline bordo = pannello.GetComponent<Outline>();
            bordo.effectColor = new Color(0.48f, 0.25f, 0.08f, 0.9f);
            bordo.effectDistance = new Vector2(2f, -2f);
            bordo.useGraphicAlpha = true;
        }

        ConfiguraTestoHUD(
            TrovaTestoInterfaccia("OndataText"),
            new Vector2(34f, -38f),
            new Color(1f, 0.77f, 0.32f, 1f)
        );
        ConfiguraTestoHUD(
            TrovaTestoInterfaccia("VitaText"),
            new Vector2(34f, -76f),
            new Color(0.5f, 0.95f, 0.47f, 1f)
        );
        ConfiguraTestoHUD(
            testoMonete,
            new Vector2(34f, -114f),
            new Color(1f, 0.9f, 0.24f, 1f)
        );
        ConfiguraTestoHUD(
            testoUova,
            new Vector2(34f, -146f),
            new Color(1f, 0.94f, 0.76f, 1f)
        );

        if (testoUova != null)
        {
            testoUova.gameObject.SetActive(gallineRimaste > 0);
        }
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
        testo.color = colore;
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
        if (titoloFinePartita != null)
        {
            titoloFinePartita.text = titolo;
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
        }
        Time.timeScale = 0f;
    }

    public void Riprova()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AggiungiMonete(int quantita)
    {
        monete += Mathf.Max(0, quantita);
        AggiornaContatoreMonete();
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
}
