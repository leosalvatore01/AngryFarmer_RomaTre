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
    public float BonusVelocitaPermanente => bonusVelocitaPermanente;
    public float BonusVelocitaTotale =>
        bonusVelocita + bonusVelocitaPermanente;
    public float VelocitaFinale => Mathf.Max(
        0f,
        velocitaBase + BonusVelocitaTotale
    );
    public float VelocitaEffettiva =>
        VelocitaFinale * MoltiplicatoreVelocitaCorrente;
    public float MoltiplicatoreBoostCorrente => moltiplicatoreVelocita;
    public float MoltiplicatoreTerrenoCorrente =>
        tempoRallentamentoTerreno > 0f
            ? moltiplicatoreRallentamentoTerreno
            : 1f;
    public float MoltiplicatoreVelocitaCorrente =>
        MoltiplicatoreBoostCorrente * MoltiplicatoreTerrenoCorrente;
    public bool BoostVelocitaAttivo => tempoFineBoostVelocita > Time.time;
    public float TempoBoostVelocitaRimasto => BoostVelocitaAttivo
        ? Mathf.Max(0f, tempoFineBoostVelocita - Time.time)
        : 0f;
    public float DurataBoostVelocitaTotale => Mathf.Max(0f, durataBoost);
    public bool NelFango => tempoRallentamentoTerreno > 0f;
    public bool StaCamminando => VelocitaAttuale.sqrMagnitude > 0.01f;
    public bool SchivataAttiva => schivataInCorso &&
        Time.time < schivataAttivaFinoA;
    public bool SchivataPronta => !SchivataAttiva &&
        Time.time >= prossimaSchivataDisponibile;
    public float CooldownSchivata => cooldownSchivata;
    public float CooldownSchivataRimasto => Mathf.Max(
        0f,
        prossimaSchivataDisponibile - Time.time
    );
    public float ProgressoCooldownSchivata => cooldownSchivata > 0f
        ? 1f - Mathf.Clamp01(
            CooldownSchivataRimasto / cooldownSchivata
        )
        : 1f;
    public Vector2 DirezioneUltimaSchivata { get; private set; }

    [System.Obsolete("Usa VelocitaFinale.")]
    public float speed => VelocitaFinale;

    private Rigidbody2D corpo;
    private FarmerInputController input;
    private PlayerHealth salute;
    private PlayerDodgeFeedback feedbackSchivata;
    private float bonusVelocita;
    private float bonusVelocitaPermanente;
    private float moltiplicatoreVelocita = 1f;
    private float moltiplicatoreBoost = 2f;
    private float moltiplicatoreInversione = 1.35f;
    private float tempoRallentamentoTerreno;
    private float moltiplicatoreRallentamentoTerreno = 1f;
    private float tempoFineBoostVelocita;
    private Coroutine boostRoutine;
    private float durataSchivata;
    private float velocitaSchivata;
    private float cooldownSchivata;
    private float durataInvulnerabilitaSchivata;
    private float schivataAttivaFinoA;
    private float prossimaSchivataDisponibile;
    private bool schivataInCorso;
    private Vector2 ultimaDirezioneMovimento = Vector2.down;

    public event System.Action SchivataIniziata;
    public event System.Action SchivataTerminata;

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
        durataSchivata = Mathf.Max(0.05f, configurazione.durataSchivata);
        velocitaSchivata = Mathf.Max(1f, configurazione.velocitaSchivata);
        cooldownSchivata = Mathf.Max(
            durataSchivata,
            configurazione.cooldownSchivata
        );
        durataInvulnerabilitaSchivata = Mathf.Max(
            0.05f,
            configurazione.durataInvulnerabilitaSchivata
        );

        corpo = GetComponent<Rigidbody2D>();
        corpo.interpolation = RigidbodyInterpolation2D.Interpolate;
        input = Application.isPlaying
            ? FarmerInputController.CreaOTrova()
            : FarmerInputController.Instance;
        salute = GetComponent<PlayerHealth>();
        feedbackSchivata = PlayerDodgeFeedback.AggiungiOTrova(gameObject);
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

        if (input == null) input = FarmerInputController.CreaOTrova();
        DirezioneMovimento = input != null
            ? input.Movimento
            : Vector2.zero;
        if (DirezioneMovimento.sqrMagnitude > 0.001f)
        {
            ultimaDirezioneMovimento = DirezioneMovimento.normalized;
        }

        if (input != null && input.SchivataPremutaQuestoFrame)
        {
            ProvaSchivata(DirezioneMovimento);
        }
    }

    void FixedUpdate()
    {
        if (SchivataAttiva)
        {
            VelocitaAttuale =
                DirezioneUltimaSchivata * velocitaSchivata;
            corpo.MovePosition(
                corpo.position + VelocitaAttuale * Time.fixedDeltaTime
            );
            return;
        }

        if (schivataInCorso)
        {
            schivataInCorso = false;
            VelocitaAttuale = Vector2.zero;
            SchivataTerminata?.Invoke();
        }

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

    public bool ProvaSchivata(Vector2 direzioneRichiesta)
    {
        if (!SchivataPronta ||
            (GameManager.instance != null &&
             !GameManager.instance.GameplayAttivo))
        {
            return false;
        }

        Vector2 direzione = direzioneRichiesta;
        if (direzione.sqrMagnitude <= 0.001f)
        {
            PlayerShooting sparo = GetComponent<PlayerShooting>();
            direzione = sparo != null && sparo.HaDirezioneMira
                ? sparo.DirezioneMira
                : ultimaDirezioneMovimento;
        }
        if (direzione.sqrMagnitude <= 0.001f) return false;

        DirezioneUltimaSchivata = direzione.normalized;
        ultimaDirezioneMovimento = DirezioneUltimaSchivata;
        schivataInCorso = true;
        schivataAttivaFinoA = Time.time + durataSchivata;
        prossimaSchivataDisponibile = Time.time + cooldownSchivata;

        if (salute == null) salute = GetComponent<PlayerHealth>();
        salute?.AttivaInvulnerabilitaTemporanea(
            durataInvulnerabilitaSchivata
        );
        if (feedbackSchivata == null)
        {
            feedbackSchivata =
                PlayerDodgeFeedback.AggiungiOTrova(gameObject);
        }
        feedbackSchivata?.Avvia(
            DirezioneUltimaSchivata,
            durataSchivata
        );
        if (Application.isPlaying)
        {
            CombatFeedbackController.CreaOTrova()?.RegistraSchivata(
                transform.position,
                DirezioneUltimaSchivata
            );
        }
        SchivataIniziata?.Invoke();
        return true;
    }

    public void AttivaBoostVelocita()
    {
        if (boostRoutine != null)
        {
            StopCoroutine(boostRoutine);
            boostRoutine = null;
        }

        float durata = DurataBoostVelocitaTotale;
        if (durata <= 0f)
        {
            AzzeraBoostVelocita();
            return;
        }

        tempoFineBoostVelocita = Time.time + durata;
        moltiplicatoreVelocita = moltiplicatoreBoost;
        boostRoutine = StartCoroutine(BoostVelocita(durata));
    }

    public void ImpostaBonusVelocita(float valore)
    {
        bonusVelocita = Mathf.Max(0f, valore);
    }

    public void ImpostaBonusVelocitaPermanente(float valore)
    {
        bonusVelocitaPermanente = Mathf.Max(0f, valore);
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

    IEnumerator BoostVelocita(float durata)
    {
        yield return new WaitForSeconds(durata);
        AzzeraBoostVelocita();
    }

    private void AzzeraBoostVelocita()
    {
        moltiplicatoreVelocita = 1f;
        tempoFineBoostVelocita = 0f;
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
        }
        AzzeraBoostVelocita();
        tempoRallentamentoTerreno = 0f;
        moltiplicatoreRallentamentoTerreno = 1f;
        DirezioneMovimento = Vector2.zero;
        VelocitaAttuale = Vector2.zero;
        schivataInCorso = false;
        schivataAttivaFinoA = 0f;
    }
}
