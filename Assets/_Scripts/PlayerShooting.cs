using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    [Min(0.12f)]
    public float fireRate = 0.4f;
    public float bulletSpeed = 10f;
    [Min(1)] public int dannoProiettile = 1;
    [Min(0)] public int penetrazioneProiettile = 0;

    [FormerlySerializedAs("durataDoppioSparo")]
    public float durataTriploSparo = 5f;

    private float nextFire = 0f;
    private bool triploSparoAttivo = false;
    private Coroutine triploSparoRoutine;

    void Update()
    {
        if (GameManager.instance != null &&
            !GameManager.instance.GameplayAttivo)
        {
            return;
        }

        if (Input.GetMouseButton(0) && Time.time >= nextFire)
        {
            nextFire = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPos.z = 0;

        if (Vector2.Distance(targetPos, transform.position) < 0.5f) return;

        Vector2 direzione = (targetPos - transform.position).normalized;

        if (triploSparoAttivo)
        {
            CreateBullet(direzione);
            CreateBullet(Quaternion.Euler(0, 0, 10) * direzione);
            CreateBullet(Quaternion.Euler(0, 0, -10) * direzione);
        }
        else
        {
            CreateBullet(direzione);
        }
    }

    void CreateBullet(Vector2 direzione)
    {
        float angolo = Mathf.Atan2(direzione.y, direzione.x) * Mathf.Rad2Deg;
        GameObject proiettile = Instantiate(
            bulletPrefab,
            transform.position,
            Quaternion.Euler(0f, 0f, angolo)
        );

        Proiettile comportamento = proiettile.GetComponent<Proiettile>();
        if (comportamento != null)
        {
            comportamento.Inizializza(
                dannoProiettile,
                penetrazioneProiettile
            );
        }

        Rigidbody2D corpo = proiettile.GetComponent<Rigidbody2D>();
        if (corpo != null)
        {
            corpo.linearVelocity = direzione * bulletSpeed;
        }
    }

    public void AttivaTriploSparo()
    {
        if (triploSparoRoutine != null)
        {
            StopCoroutine(triploSparoRoutine);
        }

        triploSparoRoutine = StartCoroutine(TriploSparoTimer());
    }

    IEnumerator TriploSparoTimer()
    {
        triploSparoAttivo = true;
        yield return new WaitForSeconds(Mathf.Max(0f, durataTriploSparo));
        triploSparoAttivo = false;
        triploSparoRoutine = null;
    }

    public void AumentaDanno(int quantita)
    {
        dannoProiettile = Mathf.Max(1, dannoProiettile + Mathf.Max(0, quantita));
    }

    public void RiduciIntervalloSparo(float quantita)
    {
        fireRate = Mathf.Max(0.12f, fireRate - Mathf.Max(0f, quantita));
    }

    public void AumentaPenetrazione(int quantita)
    {
        penetrazioneProiettile = Mathf.Max(
            0,
            penetrazioneProiettile + Mathf.Max(0, quantita)
        );
    }

    [System.Obsolete("Usa AttivaTriploSparo.")]
    public void AttivaDoppioSparo()
    {
        AttivaTriploSparo();
    }
}
