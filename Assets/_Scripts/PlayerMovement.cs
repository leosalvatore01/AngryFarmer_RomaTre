using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Statistiche di movimento")]
    [FormerlySerializedAs("speed")]
    [SerializeField, Min(0f)] private float velocitaBase = 8f;

    public float durataBoost = 5f;

    [Header("Fluidità")]
    [Min(0f)] public float accelerazione = 45f;
    [Min(0f)] public float decelerazione = 60f;

    public Vector2 DirezioneMovimento { get; private set; }
    public Vector2 VelocitaAttuale { get; private set; }

    public float VelocitaBase => velocitaBase;
    public float BonusVelocita => bonusVelocita;
    public float VelocitaFinale => Mathf.Max(0f, velocitaBase + bonusVelocita);
    public float VelocitaEffettiva =>
        VelocitaFinale * MoltiplicatoreVelocitaCorrente;
    public float MoltiplicatoreBoostCorrente => moltiplicatoreVelocita;
    public float MoltiplicatoreTerrenoCorrente =>
        tempoRallentamentoTerreno > 0f
            ? moltiplicatoreRallentamentoTerreno
            : 1f;
    public float MoltiplicatoreVelocitaCorrente =>
        MoltiplicatoreBoostCorrente * MoltiplicatoreTerrenoCorrente;
    public bool NelFango => tempoRallentamentoTerreno > 0f;
    public bool StaCamminando => VelocitaAttuale.sqrMagnitude > 0.01f;

    [System.Obsolete("Usa VelocitaFinale.")]
    public float speed => VelocitaFinale;

    private Rigidbody2D corpo;
    private float bonusVelocita;
    private float moltiplicatoreVelocita = 1f;
    private float moltiplicatoreBoost = 2f;
    private float moltiplicatoreInversione = 1.35f;
    private float tempoRallentamentoTerreno;
    private float moltiplicatoreRallentamentoTerreno = 1f;
    private Coroutine boostRoutine;

    void Awake()
    {
        PlayerBalanceSettings configurazione =
            GameBalanceConfig.Corrente.Giocatore;

        velocitaBase = Mathf.Max(0f, configurazione.velocitaMovimento);
        accelerazione = Mathf.Max(0f, configurazione.accelerazione);
        decelerazione = Mathf.Max(0f, configurazione.decelerazione);
        moltiplicatoreInversione = Mathf.Max(
            1f,
            configurazione.moltiplicatoreInversione
        );
        durataBoost = Mathf.Max(0f, configurazione.durataBoostVelocita);
        moltiplicatoreBoost = Mathf.Max(
            1f,
            configurazione.moltiplicatoreBoostVelocita
        );

        corpo = GetComponent<Rigidbody2D>();
        corpo.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        AggiornaRallentamentoTerreno();

        if (GameManager.instance != null &&
            !GameManager.instance.GameplayAttivo)
        {
            DirezioneMovimento = Vector2.zero;
            return;
        }

        DirezioneMovimento = Vector2.ClampMagnitude(new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ), 1f);
    }

    void FixedUpdate()
    {
        Vector2 velocitaDesiderata =
            DirezioneMovimento * VelocitaEffettiva;

        float rapiditaCambio = DirezioneMovimento.sqrMagnitude > 0.001f
            ? accelerazione
            : decelerazione;

        if (Vector2.Dot(VelocitaAttuale, velocitaDesiderata) < 0f)
        {
            rapiditaCambio *= moltiplicatoreInversione;
        }

        VelocitaAttuale = Vector2.MoveTowards(
            VelocitaAttuale,
            velocitaDesiderata,
            rapiditaCambio * Time.fixedDeltaTime
        );

        corpo.MovePosition(
            corpo.position +
            VelocitaAttuale * Time.fixedDeltaTime
        );
    }

    public void AttivaBoostVelocita()
    {
        if (boostRoutine != null)
        {
            StopCoroutine(boostRoutine);
        }

        boostRoutine = StartCoroutine(BoostVelocita());
    }

    public void ImpostaBonusVelocita(float valore)
    {
        bonusVelocita = Mathf.Max(0f, valore);
    }

    public void AggiungiBonusVelocita(float quantita)
    {
        ImpostaBonusVelocita(bonusVelocita + Mathf.Max(0f, quantita));
    }

    public void ApplicaRallentamentoTerreno(
        float moltiplicatore,
        float durata
    )
    {
        float valore = Mathf.Clamp(moltiplicatore, 0.2f, 1f);
        float tempo = Mathf.Max(0.02f, durata);
        if (tempoRallentamentoTerreno <= 0f)
        {
            moltiplicatoreRallentamentoTerreno = valore;
        }
        else
        {
            moltiplicatoreRallentamentoTerreno = Mathf.Min(
                moltiplicatoreRallentamentoTerreno,
                valore
            );
        }
        tempoRallentamentoTerreno = Mathf.Max(
            tempoRallentamentoTerreno,
            tempo
        );
    }

    [System.Obsolete("Usa AggiungiBonusVelocita.")]
    public void AumentaVelocitaBase(float quantita)
    {
        AggiungiBonusVelocita(quantita);
    }

    IEnumerator BoostVelocita()
    {
        moltiplicatoreVelocita = moltiplicatoreBoost;
        yield return new WaitForSeconds(durataBoost);
        moltiplicatoreVelocita = 1f;
        boostRoutine = null;
    }

    private void AggiornaRallentamentoTerreno()
    {
        if (tempoRallentamentoTerreno <= 0f) return;

        tempoRallentamentoTerreno = Mathf.Max(
            0f,
            tempoRallentamentoTerreno - Time.deltaTime
        );
        if (tempoRallentamentoTerreno <= 0f)
        {
            moltiplicatoreRallentamentoTerreno = 1f;
        }
    }

    void OnDisable()
    {
        if (boostRoutine != null)
        {
            StopCoroutine(boostRoutine);
            boostRoutine = null;
        }
        tempoRallentamentoTerreno = 0f;
        moltiplicatoreRallentamentoTerreno = 1f;
        moltiplicatoreVelocita = 1f;
        DirezioneMovimento = Vector2.zero;
        VelocitaAttuale = Vector2.zero;
    }
}
