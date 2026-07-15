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

    [Header("Mira")]
    [Min(0f)] public float distanzaMinimaMira = 0.2f;
    [Min(0f)] public float distanzaUscitaProiettile = 0.44f;

    [FormerlySerializedAs("durataDoppioSparo")]
    public float durataTriploSparo = 5f;

    private float nextFire = 0f;
    private bool triploSparoAttivo = false;
    private Coroutine triploSparoRoutine;
    private Camera cameraPrincipale;
    private PlayerVisualController controllerVisivo;

    public Vector2 DirezioneMira { get; private set; } = Vector2.down;
    public bool HaDirezioneMira { get; private set; }

    void Awake()
    {
        cameraPrincipale = Camera.main;
        controllerVisivo = GetComponent<PlayerVisualController>();
    }

    void Update()
    {
        bool miraValida = AggiornaDirezioneMira();

        if (GameManager.instance != null &&
            !GameManager.instance.GameplayAttivo)
        {
            return;
        }

        if (miraValida &&
            Input.GetMouseButton(0) &&
            Time.time >= nextFire)
        {
            if (Shoot(DirezioneMira))
            {
                nextFire = Time.time + fireRate;
            }
        }
    }

    public bool ProvaOttieniDirezioneMira(out Vector2 direzione)
    {
        bool valida = AggiornaDirezioneMira();
        direzione = DirezioneMira;
        return valida;
    }

    bool AggiornaDirezioneMira()
    {
        if (cameraPrincipale == null)
        {
            cameraPrincipale = Camera.main;
        }

        if (cameraPrincipale == null)
        {
            HaDirezioneMira = false;
            return false;
        }

        Vector3 posizioneSchermo = Input.mousePosition;
        posizioneSchermo.z = Mathf.Abs(
            transform.position.z - cameraPrincipale.transform.position.z
        );

        Vector3 posizioneMondo =
            cameraPrincipale.ScreenToWorldPoint(posizioneSchermo);
        Vector2 scarto =
            (Vector2)posizioneMondo - (Vector2)transform.position;

        float distanzaMinima = Mathf.Max(0f, distanzaMinimaMira);
        if (scarto.sqrMagnitude < distanzaMinima * distanzaMinima)
        {
            HaDirezioneMira = false;
            return false;
        }

        DirezioneMira = scarto.normalized;
        HaDirezioneMira = true;
        return true;
    }

    bool Shoot(Vector2 direzione)
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Prefab del proiettile non assegnato.", this);
            return false;
        }

        direzione.Normalize();

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

        if (controllerVisivo != null)
        {
            controllerVisivo.RiproduciFeedbackSparo(direzione);
        }
        return true;
    }

    void CreateBullet(Vector2 direzione)
    {
        float angolo = Mathf.Atan2(direzione.y, direzione.x) * Mathf.Rad2Deg;
        Vector3 posizioneUscita = transform.position +
            (Vector3)(direzione.normalized * distanzaUscitaProiettile);
        GameObject proiettile = Instantiate(
            bulletPrefab,
            posizioneUscita,
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
