using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int gallineRimaste = 0;
    public bool isGameOver = false;

    [SerializeField] private GameObject gameOverPanel;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;

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
    }

    public void GallinaMorta()
    {
        if (isGameOver) return;

        gallineRimaste--;
        if (gallineRimaste <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("GAME OVER! Le volpi hanno vinto.");
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Riprova()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Vittoria()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("VITTORIA! Hai difeso il pollaio da tutte le ondate!");
        Time.timeScale = 0f;
    }
}