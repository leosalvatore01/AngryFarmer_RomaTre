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
    public int monete = 0;
    private TMP_Text testoMonete;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;

        testoUova = GameObject.Find("UovaText")?.GetComponent<TMP_Text>();
        AggiornaContatoreUova();
        testoMonete = GameObject.Find("MoneteText")?.GetComponent<TMP_Text>();
        AggiornaContatoreMonete();

        Button pulsanteRiprova = gameOverPanel.GetComponentInChildren<Button>(true);
        if (pulsanteRiprova != null)
        {
            pulsanteRiprova.onClick.AddListener(Riprova);
        }

        gameOverPanel.SetActive(false);
    }

    public void RegistraGallina()
    {
        gallineRimaste++;
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
            testoUova.text = "UOVA: " + gallineRimaste;
        }
    }

    void GameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Riprova()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOverGiocatore()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("GAME OVER! Il contadino è stato sconfitto.");
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    public void AggiungiMonete(int quantita)
    {
        monete += quantita;
        AggiornaContatoreMonete();
    }

    void AggiornaContatoreMonete()
    {
        if (testoMonete != null)
        {
            testoMonete.text = "MONETE: " + monete;
        }
    }
    public void Vittoria()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;
    }
}