using UnityEngine;

public class Proiettile : MonoBehaviour
{
    [Min(1)] public int danno = 1;

    private bool consumato;

    void Start()
    {
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (consumato) return;

        EnemyAI volpe = other.GetComponentInParent<EnemyAI>();
        if (volpe == null) return;

        consumato = true;
        volpe.SubisciDanno(danno);
        Destroy(gameObject);
    }
}
