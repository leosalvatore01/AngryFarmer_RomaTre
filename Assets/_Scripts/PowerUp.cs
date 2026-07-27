using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public int type; // 0 = Dente, 1 = Coda

    private Vector3 scalaBase;
    public float DurataDespawn { get; private set; }

    void Start()
    {
        scalaBase = transform.localScale;
        DurataDespawn = Mathf.Max(
            0.1f,
            GameBalanceConfig.Corrente.Volpe.durataDropSullaMappa
        );
        Destroy(gameObject, DurataDespawn);
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
                sparo.AttivaTriploSparo();
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
