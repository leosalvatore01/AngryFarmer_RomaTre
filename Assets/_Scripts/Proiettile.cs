using UnityEngine;

public class Proiettile : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 target;

    void Start()
    {
        target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target.z = 0;

        Vector3 direction = (target - transform.position).normalized;
        GetComponent<Rigidbody2D>().linearVelocity = direction * speed;

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