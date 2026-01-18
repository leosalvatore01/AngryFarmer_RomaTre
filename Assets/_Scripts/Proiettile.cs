using UnityEngine;

public class Proiettile : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 target;

    void Start()
    {
        // Appena nasce, calcola dove si trova il mouse e punta li
        target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target.z = 0;
        
        // Calcola la direzione e ruota il proiettile
        Vector3 direction = (target - transform.position).normalized;
        GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
        
        // Distruggi il proiettile dopo 3 secondi per pulire la memoria
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Se tocca qualcosa con il tag "Nemico"
        if (other.CompareTag("Nemico"))
        {
            Destroy(other.gameObject); // Distruggi la volpe
            Destroy(gameObject);       // Distruggi il proiettile
        }
    }
}
