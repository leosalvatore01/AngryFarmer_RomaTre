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
}
