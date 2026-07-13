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

        IDanneggiabile bersaglio = other.GetComponentInParent<IDanneggiabile>();
        if (bersaglio == null || !bersaglio.ProvaSubireDanno(danno)) return;

        consumato = true;
        Destroy(gameObject);
    }
}
