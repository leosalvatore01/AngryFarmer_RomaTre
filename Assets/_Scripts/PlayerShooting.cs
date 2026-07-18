using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;

    [Header("Statistiche di sparo")]
    [FormerlySerializedAs("fireRate")]
    [SerializeField, Min(0.01f)] private float intervalloSparoBase = 0.4f;

    [FormerlySerializedAs("bulletSpeed")]
    [SerializeField, Min(0f)] private float velocitaProiettileBase = 10f;

    [FormerlySerializedAs("dannoProiettile")]
    [SerializeField, Min(1)] private int dannoBase = 1;

    [FormerlySerializedAs("penetrazioneProiettile")]
    [SerializeField, Min(0)] private int penetrazioneBase;

    [Header("Mira")]
    [Min(0f)] public float distanzaMinimaMira = 0.2f;
    [Min(0f)] public float distanzaUscitaProiettile = 0.44f;

    [FormerlySerializedAs("durataDoppioSparo")]
    public float durataTriploSparo = 5f;

    public float IntervalloSparoBase => intervalloSparoBase;
    public float BonusRiduzioneIntervalloSparo =>
        bonusRiduzioneIntervalloSparo;
    public float IntervalloSparoFinale => Mathf.Max(
        intervalloSparoMinimo,
        intervalloSparoBase - bonusRiduzioneIntervalloSparo
    );

    public float VelocitaProiettileBase => velocitaProiettileBase;
    public float BonusVelocitaProiettile => bonusVelocitaProiettile;
    public float VelocitaProiettileFinale => Mathf.Max(
        0f,
        velocitaProiettileBase + bonusVelocitaProiettile
    );

    public int DannoBase => dannoBase;
    public int BonusDanno => bonusDanno;
    public int DannoFinale => Mathf.Max(1, dannoBase + bonusDanno);

    public int PenetrazioneBase => penetrazioneBase;
    public int BonusPenetrazione => bonusPenetrazione;
    public int PenetrazioneFinale => Mathf.Max(
        0,
        penetrazioneBase + bonusPenetrazione
    );

    public float IntervalloSparoMinimo => intervalloSparoMinimo;
    public Vector2 DirezioneMira { get; private set; } = Vector2.down;
    public bool HaDirezioneMira { get; private set; }

    [System.Obsolete("Usa IntervalloSparoFinale.")]
    public float fireRate => IntervalloSparoFinale;
    [System.Obsolete("Usa VelocitaProiettileFinale.")]
    public float bulletSpeed => VelocitaProiettileFinale;
    [System.Obsolete("Usa DannoFinale.")]
    public int dannoProiettile => DannoFinale;
    [System.Obsolete("Usa PenetrazioneFinale.")]
    public int penetrazioneProiettile => PenetrazioneFinale;

    private float bonusRiduzioneIntervalloSparo;
    private float bonusVelocitaProiettile;
    private int bonusDanno;
    private int bonusPenetrazione;
    private float intervalloSparoMinimo = 0.12f;
    private float angoloLateraleTriploSparo = 10f;
    private float nextFire;
    private bool triploSparoAttivo;
    private Coroutine triploSparoRoutine;
    private Camera cameraPrincipale;
    private PlayerVisualController controllerVisivo;
    private PlayerUpgrades potenziamenti;
    private System.Random casualitaBuild;
    private bool attendiRilascioMouse;
    private int colpiContatiPerRaffica;
    private int segnoColpoAggiuntivo = 1;

    public bool InAttesaRilascioMouse => attendiRilascioMouse;
    public int UltimoNumeroProiettiliCreati { get; private set; }
    public int CriticiGenerati { get; private set; }

    void Awake()
    {
        PlayerBalanceSettings configurazione =
            GameBalanceConfig.Corrente.Giocatore;

        intervalloSparoMinimo = Mathf.Max(
            0.01f,
            configurazione.intervalloSparoMinimo
        );
        intervalloSparoBase = Mathf.Max(
            intervalloSparoMinimo,
            configurazione.intervalloSparo
        );
        velocitaProiettileBase = Mathf.Max(
            0f,
            configurazione.velocitaProiettile
        );
        dannoBase = Mathf.Max(1, configurazione.dannoProiettile);
        penetrazioneBase = Mathf.Max(0, configurazione.penetrazioneProiettile);
        distanzaMinimaMira = Mathf.Max(0f, configurazione.distanzaMinimaMira);
        distanzaUscitaProiettile = Mathf.Max(
            0f,
            configurazione.distanzaUscitaProiettile
        );
        durataTriploSparo = Mathf.Max(
            0f,
            configurazione.durataTriploSparo
        );
        angoloLateraleTriploSparo = Mathf.Clamp(
            configurazione.angoloLateraleTriploSparo,
            0f,
            45f
        );

        cameraPrincipale = Camera.main;
        controllerVisivo = GetComponent<PlayerVisualController>();
        potenziamenti = GetComponent<PlayerUpgrades>();
        if (potenziamenti == null)
        {
            potenziamenti = gameObject.AddComponent<PlayerUpgrades>();
        }
        casualitaBuild = new System.Random(
            unchecked(Environment.TickCount ^ GetInstanceID() * 397)
        );
    }

    void OnEnable()
    {
        attendiRilascioMouse = Input.GetMouseButton(0);
    }

    void Update()
    {
        bool miraValida = AggiornaDirezioneMira();

        if (GameManager.instance != null &&
            !GameManager.instance.GameplayAttivo)
        {
            attendiRilascioMouse = Input.GetMouseButton(0);
            return;
        }

        if (attendiRilascioMouse)
        {
            if (!Input.GetMouseButton(0))
            {
                attendiRilascioMouse = false;
            }
            return;
        }

        if (miraValida &&
            Input.GetMouseButton(0) &&
            Time.time >= nextFire)
        {
            if (Shoot(DirezioneMira))
            {
                nextFire = Time.time + IntervalloSparoFinale;
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

        if (potenziamenti == null)
        {
            potenziamenti = GetComponent<PlayerUpgrades>();
        }

        colpiContatiPerRaffica++;
        bool rafficaRaccolto =
            potenziamenti != null &&
            potenziamenti.HaRafficaRaccolto &&
            colpiContatiPerRaffica %
            potenziamenti.ColpiPerRafficaRaccolto == 0;
        bool sparaVentaglio = triploSparoAttivo || rafficaRaccolto;
        bool potente =
            bonusDanno > 0 ||
            sparaVentaglio ||
            (potenziamenti != null &&
             (potenziamenti.OttieniLivello(
                  TipoPotenziamento.PatataGigante
              ) > 0 ||
              potenziamenti.OttieniLivello(
                  TipoPotenziamento.PatataEsplosiva
              ) > 0));
        bool perforante = PenetrazioneFinale > 0;
        UltimoNumeroProiettiliCreati = 0;

        if (sparaVentaglio)
        {
            CreateBullet(direzione, potente, perforante);
            CreateBullet(
                Quaternion.Euler(0f, 0f, angoloLateraleTriploSparo) * direzione,
                potente,
                perforante
            );
            CreateBullet(
                Quaternion.Euler(0f, 0f, -angoloLateraleTriploSparo) * direzione,
                potente,
                perforante
            );
        }
        else
        {
            CreateBullet(direzione, potente, perforante);
            if (potenziamenti != null &&
                EstraiProbabilita(
                    potenziamenti.ProbabilitaColpoAggiuntivo
                ))
            {
                float angolo =
                    potenziamenti.AngoloColpoAggiuntivo *
                    segnoColpoAggiuntivo;
                segnoColpoAggiuntivo *= -1;
                CreateBullet(
                    Quaternion.Euler(0f, 0f, angolo) * direzione,
                    true,
                    perforante
                );
            }
        }

        if (controllerVisivo != null)
        {
            controllerVisivo.RiproduciFeedbackSparo(
                direzione,
                potente,
                perforante
            );
        }

        CombatFeedbackController.CreaOTrova().RegistraSparo(
            (Vector2)transform.position +
            direzione * distanzaUscitaProiettile,
            direzione,
            potente,
            perforante
        );
        return true;
    }

    void CreateBullet(
        Vector2 direzione,
        bool potente,
        bool perforante
    )
    {
        bool critico =
            potenziamenti != null &&
            EstraiProbabilita(potenziamenti.ProbabilitaCritico);
        ProfiloProiettileBuild profilo =
            potenziamenti != null
                ? potenziamenti.CreaProfiloProiettile(critico)
                : new ProfiloProiettileBuild
                {
                    Danno = DannoFinale,
                    Penetrazioni = PenetrazioneFinale,
                    Scala = 1f,
                    MoltiplicatoreVelocita = 1f
                };
        if (critico) CriticiGenerati++;

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
            comportamento.InizializzaBuild(
                profilo,
                potente || critico || profilo.Esplosivo,
                perforante
            );
        }

        Rigidbody2D corpo = proiettile.GetComponent<Rigidbody2D>();
        if (corpo != null)
        {
            corpo.linearVelocity =
                direzione *
                VelocitaProiettileFinale *
                Mathf.Max(0.1f, profilo.MoltiplicatoreVelocita);
        }
        UltimoNumeroProiettiliCreati++;
    }

    private bool EstraiProbabilita(float probabilita)
    {
        if (probabilita <= 0f) return false;
        if (probabilita >= 1f) return true;
        if (casualitaBuild == null)
        {
            casualitaBuild = new System.Random(
                unchecked(Environment.TickCount ^ GetInstanceID() * 397)
            );
        }
        return casualitaBuild.NextDouble() < probabilita;
    }

    public void ImpostaSeedBuildPerTest(int seed)
    {
        casualitaBuild = new System.Random(seed);
        colpiContatiPerRaffica = 0;
        segnoColpoAggiuntivo = 1;
        CriticiGenerati = 0;
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
        yield return new WaitForSeconds(durataTriploSparo);
        triploSparoAttivo = false;
        triploSparoRoutine = null;
    }

    public void ImpostaBonusDanno(int valore)
    {
        bonusDanno = Mathf.Max(0, valore);
    }

    public void ImpostaBonusRiduzioneIntervalloSparo(float valore)
    {
        bonusRiduzioneIntervalloSparo = Mathf.Max(0f, valore);
    }

    public void ImpostaBonusPenetrazione(int valore)
    {
        bonusPenetrazione = Mathf.Max(0, valore);
    }

    public void ImpostaBonusVelocitaProiettile(float valore)
    {
        bonusVelocitaProiettile = Mathf.Max(0f, valore);
    }

    void OnApplicationFocus(bool attiva)
    {
        if (!attiva)
        {
            attendiRilascioMouse = true;
        }
    }

    [System.Obsolete("Usa ImpostaBonusDanno.")]
    public void AumentaDanno(int quantita)
    {
        ImpostaBonusDanno(bonusDanno + Mathf.Max(0, quantita));
    }

    [System.Obsolete("Usa ImpostaBonusRiduzioneIntervalloSparo.")]
    public void RiduciIntervalloSparo(float quantita)
    {
        ImpostaBonusRiduzioneIntervalloSparo(
            bonusRiduzioneIntervalloSparo + Mathf.Max(0f, quantita)
        );
    }

    [System.Obsolete("Usa ImpostaBonusPenetrazione.")]
    public void AumentaPenetrazione(int quantita)
    {
        ImpostaBonusPenetrazione(
            bonusPenetrazione + Mathf.Max(0, quantita)
        );
    }

    [System.Obsolete("Usa AttivaTriploSparo.")]
    public void AttivaDoppioSparo()
    {
        AttivaTriploSparo();
    }
}
