using UnityEngine;

public class Proiettile : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Nemico"))
        {
            EnemyAI volpe = other.GetComponent<EnemyAI>();

            if (volpe != null)
            {
                volpe.Die();
            }

            Destroy(gameObject);
        }
    }
}