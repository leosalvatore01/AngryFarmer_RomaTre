using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireRate = 0.4f;
    private float nextFire = 0f;
    
    // Variabili Shotgun (Dente)
    public bool isShotgunActive = false;
    public float shotgunDuration = 5f;

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

        // --- PROTEZIONE ANTI-BLOCCO ---
        // Se il mouse è a meno di 0.5 unità dal contadino (cioè ci clicchi sopra), non fa nulla.
        if (Vector2.Distance(targetPos, transform.position) < 0.5f) return;
        // -----------------------------

        Vector2 direction = (targetPos - transform.position).normalized;

        CreateBullet(direction);

        if (isShotgunActive)
        {
            Vector2 left = Quaternion.Euler(0, 0, 15) * direction;
            Vector2 right = Quaternion.Euler(0, 0, -15) * direction;
            CreateBullet(left);
            CreateBullet(right);
        }
    }

    void CreateBullet(Vector2 dir)
    {
        GameObject b = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        b.GetComponent<Rigidbody2D>().linearVelocity = dir * 10f;
    }

    public void ActivateShotgun()
    {
        StartCoroutine(ShotgunTimer());
    }

    IEnumerator ShotgunTimer()
    {
        isShotgunActive = true;
        yield return new WaitForSeconds(shotgunDuration);
        isShotgunActive = false;
    }
}
