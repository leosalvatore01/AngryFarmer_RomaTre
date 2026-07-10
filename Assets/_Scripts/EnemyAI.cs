using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 2f;

    public int danno = 1;
    public float distanzaAttacco = 0.8f;
    public float intervalloAttacco = 1f;

    private Transform target;
    private PlayerHealth playerHealth;
    private float prossimoAttacco;

    public GameObject dentePrefab;
    public GameObject codaPrefab;
    [Range(0, 100)] public float dropChance = 50f;

    public static bool isSlowed = false;

    void Start()
    {
        GameObject giocatore = GameObject.FindGameObjectWithTag("Player");

        if (giocatore != null)
        {
            target = giocatore.transform;
            playerHealth = giocatore.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (target == null || playerHealth == null) return;

        float distanzaDalGiocatore = Vector2.Distance(transform.position, target.position);

        if (distanzaDalGiocatore > distanzaAttacco)
        {
            float velocitaCorrente = isSlowed ? speed / 2f : speed;
            transform.position = Vector2.MoveTowards(
                transform.position,
                target.position,
                velocitaCorrente * Time.deltaTime
            );
        }
        else if (Time.time >= prossimoAttacco)
        {
            playerHealth.SubisciDanno(danno);
            prossimoAttacco = Time.time + intervalloAttacco;
        }
    }

    public void Die()
    {
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