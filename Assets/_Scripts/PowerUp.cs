using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public int type; // 0 = Dente, 1 = Coda

    private Vector3 scalaBase;

    void Start()
    {
        scalaBase = transform.localScale;
    }

    void Update()
    {
        transform.Rotate(0f, 0f, 120f * Time.deltaTime);

        float pulsazione = 1f + Mathf.Sin(Time.time * 5f) * 0.12f;
        transform.localScale = scalaBase * pulsazione;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (type == 0)
        {
            PlayerShooting sparo = other.GetComponent<PlayerShooting>();

            if (sparo != null)
            {
                sparo.AttivaDoppioSparo();
            }
        }
        else if (type == 1)
        {
            PlayerMovement movimento = other.GetComponent<PlayerMovement>();

            if (movimento != null)
            {
                movimento.AttivaBoostVelocita();
            }
        }

        Destroy(gameObject);
    }
}