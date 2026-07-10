using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireRate = 0.4f;
    public float bulletSpeed = 10f;
    public float durataDoppioSparo = 5f;

    private float nextFire = 0f;
    private bool doppioSparoAttivo = false;
    private Coroutine doppioSparoRoutine;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time > nextFire)
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

        if (doppioSparoAttivo)
        {
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
        GameObject proiettile = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        proiettile.GetComponent<Rigidbody2D>().linearVelocity = direzione * bulletSpeed;
    }

    public void AttivaDoppioSparo()
    {
        if (doppioSparoRoutine != null)
        {
            StopCoroutine(doppioSparoRoutine);
        }

        doppioSparoRoutine = StartCoroutine(DoppioSparoTimer());
    }

    IEnumerator DoppioSparoTimer()
    {
        doppioSparoAttivo = true;
        yield return new WaitForSeconds(durataDoppioSparo);
        doppioSparoAttivo = false;
        doppioSparoRoutine = null;
    }
}