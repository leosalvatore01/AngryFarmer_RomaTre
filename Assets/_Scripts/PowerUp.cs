using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour, IPoolableGameplayObject
{
    // Mantenuto come int per non rompere i prefab Dente (0) e Coda (1).
    public int type;

    private static readonly List<PowerUp> dropAttivi = new List<PowerUp>();
    private Vector3 scalaBase;
    private SpriteRenderer rendererDrop;
    private Collider2D colliderRaccolta;
    private float scadeA;
    private bool raccolto;

    public TipoDropTemporaneo Tipo => CatalogoDropTemporanei.Normalizza(type);
    public float DurataDespawn { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraRegistro()
    {
        dropAttivi.Clear();
    }

    void OnEnable()
    {
        if (!dropAttivi.Contains(this)) dropAttivi.Add(this);
    }

    void Awake()
    {
        scalaBase = transform.localScale;
        rendererDrop = GetComponentInChildren<SpriteRenderer>();
        colliderRaccolta = GetComponent<Collider2D>();
    }

    void OnDisable()
    {
        dropAttivi.Remove(this);
    }

    void Start()
    {
        ReimpostaScadenza();
        AggiornaAspetto();
    }

    void Update()
    {
        if (scadeA > 0f && Time.time >= scadeA)
        {
            GameplayObjectPool.RilasciaODistruggi(gameObject);
            return;
        }
        transform.Rotate(0f, 0f, 120f * Time.deltaTime);
        float pulsazione = 1f + Mathf.Sin(Time.time * 5f) * 0.12f;
        transform.localScale = scalaBase * pulsazione;
    }

    public void Configura(TipoDropTemporaneo nuovoTipo)
    {
        type = (int)nuovoTipo;
        gameObject.name = "Drop_" + nuovoTipo;
        AggiornaAspetto();
    }

    private void AggiornaAspetto()
    {
        if (rendererDrop == null)
            rendererDrop = GetComponentInChildren<SpriteRenderer>();
        if (rendererDrop == null) return;

        DefinizioneDropTemporaneo definizione =
            CatalogoDropTemporanei.Ottieni(Tipo);
        rendererDrop.sprite = FarmPixelUI.OttieniIcona(definizione.Icona);
        rendererDrop.color = Color.white;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (raccolto || !other.CompareTag("Player")) return;

        PlayerTemporaryEffects effetti =
            PlayerTemporaryEffects.AggiungiOTrova(other.gameObject);
        if (effetti == null || !effetti.Applica(Tipo)) return;
        raccolto = true;

        DefinizioneDropTemporaneo definizione =
            CatalogoDropTemporanei.Ottieni(Tipo);
        RunTelemetryController.Corrente?.RegistraDropTemporaneo(
            definizione.Nome,
            definizione.Effetto
        );
        CombatFeedbackController.CreaOTrova()?.RegistraRicompensa(
            transform.position,
            definizione.Colore,
            definizione.Istantaneo
        );
        GameplayObjectPool.RilasciaODistruggi(gameObject);
    }

    public void PreparaUscitaDalPool()
    {
        raccolto = false;
        transform.localScale = scalaBase;
        transform.localRotation = Quaternion.identity;
        if (colliderRaccolta != null) colliderRaccolta.enabled = true;
        ReimpostaScadenza();
        AggiornaAspetto();
    }

    public void PreparaRientroNelPool()
    {
        raccolto = true;
        scadeA = 0f;
    }

    private void ReimpostaScadenza()
    {
        DurataDespawn = Mathf.Max(
            0.1f,
            GameBalanceConfig.Corrente.Volpe.durataDropSullaMappa
        );
        scadeA = Time.time + DurataDespawn;
    }

    public static int AttraiDropVerso(
        Vector2 centro,
        float raggio,
        float velocita,
        float deltaTime
    )
    {
        int attratti = 0;
        float raggioQuadrato = raggio * raggio;
        for (int i = dropAttivi.Count - 1; i >= 0; i--)
        {
            PowerUp drop = dropAttivi[i];
            if (drop == null)
            {
                dropAttivi.RemoveAt(i);
                continue;
            }

            Vector2 posizione = drop.transform.position;
            if ((posizione - centro).sqrMagnitude > raggioQuadrato) continue;
            drop.transform.position = Vector2.MoveTowards(
                posizione,
                centro,
                velocita * Mathf.Max(0f, deltaTime)
            );
            attratti++;
        }
        return attratti;
    }
}
