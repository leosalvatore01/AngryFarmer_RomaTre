using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public int type; // 0 = Dente, 1 = Coda

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (type == 0) 
                other.GetComponent<PlayerShooting>().ActivateShotgun();
            else 
            {
                StartCoroutine(SlowRoutine());
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;
                return;
            }
            Destroy(gameObject);
        }
    }

    System.Collections.IEnumerator SlowRoutine()
    {
        EnemyAI.isSlowed = true;
        yield return new WaitForSeconds(5f);
        EnemyAI.isSlowed = false;
        Destroy(gameObject);
    }
    void Update()
    {
        transform.Rotate(0f, 0f, 120f * Time.deltaTime);

        float scala = 1f + Mathf.Sin(Time.time * 5f) * 0.12f;
        transform.localScale = Vector3.one * scala;
    }
}
