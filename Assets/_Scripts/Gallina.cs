using UnityEngine;

public class Gallina : MonoBehaviour
{
    void Start()
    {
        // Appena inizia il gioco, dice al Manager: "Esisto anch io!"
        if (GameManager.instance != null) 
            GameManager.instance.RegistraGallina();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Se mi tocca un Nemico (Volpe)
        if (other.CompareTag("Nemico"))
        {
            // La volpe mangia la gallina (sparisce la volpe perché è sazia)
            Destroy(other.gameObject);
            
            // Avvisa il manager che una gallina è morta
            if (GameManager.instance != null)
                GameManager.instance.GallinaMorta();

            // Distruggi la gallina
            Destroy(gameObject);
        }
    }
}
