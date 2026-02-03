using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int gallineRimaste = 0;
    public bool isGameOver = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
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
        Time.timeScale = 0; 
    }

    public void Vittoria()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("VITTORIA! Hai difeso il pollaio da tutte le ondate!");
        // Qui in futuro faremo apparire i fuochi d artificio
        Time.timeScale = 0;
    }
}
