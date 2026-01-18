using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 2f; 
    private Transform target;
    
    // Drops
    public GameObject dentePrefab;
    public GameObject codaPrefab;
    [Range(0, 100)] public float dropChance = 50f; // Probabilità alta per testare

    public static bool isSlowed = false;

    void Start()
    {
        GameObject pollaio = GameObject.Find("Pollaio");
        if (pollaio != null) target = pollaio.transform;
    }

    void Update()
    {
        if (target != null)
        {
            float currentSpeed = isSlowed ? speed / 2 : speed;
            transform.position = Vector2.MoveTowards(transform.position, target.position, currentSpeed * Time.deltaTime);
        }
    }

    public void Die()
    {
        // Logica Drop: lancia una moneta
        if (Random.Range(0f, 100f) < dropChance)
        {
            if (Random.value > 0.5f && dentePrefab != null)
                Instantiate(dentePrefab, transform.position, Quaternion.identity);
            else if (codaPrefab != null)
                Instantiate(codaPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
