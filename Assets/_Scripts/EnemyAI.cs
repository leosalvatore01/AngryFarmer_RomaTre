using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour, IDanneggiabile
{
    private const float IntervalloControlloProiettili = 0.04f;
    private const float DurataMiraFango = 0.68f;
    private const float DurataPreparazioneScavo = 0.72f;

    private enum StatoAttaccoAlfa
    {
        Inseguimento,
        Preparazione,
        Scatto,
        Recupero
    }

    private enum StatoUlulatrice
    {
        Inseguimento,
        Canalizzazione,
        Recupero
    }

    private enum StatoSputafango
    {
        Movimento,
        Mira
    }

    private enum StatoScavatrice
    {
        Inseguimento,
        Preparazione,
        Sotterranea,
        Emersione
    }

    public float speed = 2.4f;

    [Header("Fluidita inseguimento")]
    [Min(0f)] public float accelerazione = 12f;
    [Min(0f)] public float decelerazione = 18f;
    [Min(0f)] public float distanzaRipresaInseguimento = 0.95f;
    [Min(1f)] public float frameCorsaAlSecondo = 7f;

    [Header("Movimento visivo")]
    [Min(0f)] public float ampiezzaIdle = 0.02f;
    [Min(0f)] public float frequenzaIdle = 1.4f;
    [Min(0f)] public float ampiezzaCamminata = 0.09f;
    [Min(0f)] public float frequenzaCamminata = 2.7f;
    [Min(0f)] public float velocitaTransizione = 12f;

    [Header("Vita e feedback")]
    [Min(1)] public int vitaBase = 2;
    [Min(0.01f)] public float durataFlashDanno = 0.14f;
    public Vector2 posizioneBarraVita = new Vector2(0f, 0.58f);
    [Min(0.01f)] public float larghezzaBarraVita = 0.78f;
    [Min(0.01f)] public float altezzaBarraVita = 0.09f;

    [Header("Morte")]
    [Min(1f)] public float frameMorteAlSecondo = 8f;
    [Min(0.01f)] public float durataDissolvenza = 0.2f;

    public int danno = 1;
    public float distanzaAttacco = 0.8f;
    public float intervalloAttacco = 1f;

    private Transform target;
    private PlayerHealth playerHealth;
    private Rigidbody2D corpo;
    private Collider2D colliderFisico;
    private Vector2 velocitaAttuale;
    private Vector2 velocitaDesiderata;
    private Vector2 velocitaSpinta;
    private bool staInseguendo;
    private float prossimoAttacco;
    private float prossimaRicercaGiocatore;
    private Transform grafica;
    private SpriteRenderer spriteRendererVisibile;
    private SpriteRenderer ombraRenderer;
    private Sprite spriteIdle;
    private Sprite[] frameCorsa;
    private Sprite[] frameMorte;
    private float timerAnimazione;
    private Color coloreBase;
    private float faseMovimento;
    private float offsetVerticale;
    private float moltiplicatoreInversione = 1.3f;
    private float moltiplicatoreRallentamento = 0.5f;
    private int monetePerEliminazione = 1;
    private float probabilitaDenteSulDrop = 0.5f;
    private TipoVolpe tipo = TipoVolpe.Comune;
    private FoxVariantStats profiloVariante;
    private FoxVariantsBalanceSettings varianti;
    private FoxVariantPresentation presentazioneVariante;
    private CombatHitFeedback2D feedbackImpatto;
    private Vector3 scalaPrefab = Vector3.one;
    private int indiceSpawn;
    private float faseSerpentina;
    private float tempoRallentamentoBuild;
    private float moltiplicatoreRallentamentoBuild = 1f;
    private float tempoRallentamentoTerreno;
    private float moltiplicatoreRallentamentoTerreno = 1f;

    private float prossimoControlloProiettili;
    private float prossimaSchivata;
    private float timerSchivata;
    private Vector2 direzioneSchivata;

    private StatoUlulatrice statoUlulatrice;
    private float timerUlulatrice;
    private float prossimoUlulato;
    private float buffUlulatoFino;
    private float moltiplicatoreVelocitaUlulato = 1f;
    private float moltiplicatoreCadenzaUlulato = 1f;

    private StatoSputafango statoSputafango;
    private float timerSputafango;
    private float prossimoSputoFango;
    private Vector2 bersaglioSputoFango;

    private StatoScavatrice statoScavatrice;
    private float timerScavatrice;
    private float prossimoScavo;
    private float prossimaTracciaScavo;
    private Vector2 partenzaScavo;
    private Vector2 destinazioneScavo;

    private StatoAttaccoAlfa statoAlfa;
    private float timerStatoAlfa;
    private Vector2 direzioneScattoAlfa;
    private bool scattoAlfaHaColpito;

    private int vitaMassima;
    private int vitaCorrente;
    private bool morto;
    private bool neutralizzazioneSegnalata;
    private Coroutine flashDannoRoutine;

    private Transform barraVita;
    private Transform riempimentoBarra;
    private SpriteRenderer rendererRiempimentoBarra;

    private static Sprite spriteBarra;
    private static readonly Dictionary<TipoVolpe, Sprite[]> cacheFrameCorsa =
        new Dictionary<TipoVolpe, Sprite[]>();
    private static readonly Dictionary<TipoVolpe, Sprite[]> cacheFrameMorte =
        new Dictionary<TipoVolpe, Sprite[]>();
    private static readonly HashSet<EnemyAI> volpiAttive =
        new HashSet<EnemyAI>();
    private static readonly Collider2D[] bufferProiettili = new Collider2D[32];
    private static readonly ContactFilter2D filtroProiettili =
        ContactFilter2D.noFilter;

    public GameObject dentePrefab;
    public GameObject codaPrefab;
    [Range(0, 100)] public float dropChance = 50f;

    public static bool isSlowed = false;

    public SpriteRenderer RendererVisibile => spriteRendererVisibile;
    public int VitaMassima => vitaMassima;
    public int VitaCorrente => vitaCorrente;
    public int MonetePerEliminazione => monetePerEliminazione;
    public bool IsDead => morto;
    public TipoVolpe Tipo => tipo;
    public string NomeTipo => FoxVariantStyle.Nome(tipo);
    public Transform BersaglioCorrente => target;
    public bool StaPreparandoAttaccoAlfa =>
        statoAlfa == StatoAttaccoAlfa.Preparazione;
    public bool StaScattandoAlfa =>
        statoAlfa == StatoAttaccoAlfa.Scatto;
    public bool StaSchivando => timerSchivata > 0f;
    public bool StaCanalizzandoUlulato =>
        statoUlulatrice == StatoUlulatrice.Canalizzazione;
    public bool StaMirandoFango => statoSputafango == StatoSputafango.Mira;
    public bool StaScavando => statoScavatrice == StatoScavatrice.Sotterranea;
    public bool BuffUlulatoAttivo => Time.time < buffUlulatoFino;
    public bool RallentataDaBuild => tempoRallentamentoBuild > 0f;
    public float MoltiplicatoreRallentamentoBuild =>
        RallentataDaBuild ? moltiplicatoreRallentamentoBuild : 1f;
    public bool RallentataDaTerreno => tempoRallentamentoTerreno > 0f;
    public float MoltiplicatoreRallentamentoTerreno =>
        RallentataDaTerreno ? moltiplicatoreRallentamentoTerreno : 1f;
    public Vector2 VelocitaSpinta => velocitaSpinta;
    public FoxVariantPresentation PresentazioneVariante =>
        presentazioneVariante;
    public event System.Action<EnemyAI> NonPiuMinaccia;

    private float DistanzaAttivazioneSchivata => varianti != null
        ? varianti.distanzaAttivazioneSchivata
        : 2.1f;
    private float DurataSchivataCorrente => varianti != null
        ? varianti.durataSchivata
        : 0.24f;
    private float RecuperoSchivataCorrente => varianti != null
        ? varianti.recuperoSchivata
        : 1.65f;
    private float MoltiplicatoreVelocitaSchivata => varianti != null
        ? varianti.moltiplicatoreVelocitaSchivata
        : 2.8f;
    private float RaggioUlulatoCorrente => varianti != null
        ? varianti.raggioUlulato
        : 4.6f;
    private float RecuperoUlulatoCorrente => varianti != null
        ? varianti.recuperoUlulato
        : 6.5f;
    private float DurataCanalizzazioneUlulatoCorrente => varianti != null
        ? varianti.durataPreparazioneUlulato
        : 0.85f;
    private float DurataBuffUlulatoCorrente => varianti != null
        ? varianti.durataRallentamentoUlulato
        : 2.5f;
    private float MoltiplicatoreVelocitaBuffUlulato => varianti != null
        ? 2f - varianti.moltiplicatoreRallentamentoUlulato
        : 1.28f;
    private float MoltiplicatoreCadenzaBuffUlulato => varianti != null
        ? 1f + (1f - varianti.moltiplicatoreRallentamentoUlulato) * 0.6f
        : 1.17f;
    private float DistanzaTiroFangoCorrente => varianti != null
        ? varianti.distanzaTiroFango
        : 5.5f;
    private float RecuperoSputoFangoCorrente => varianti != null
        ? varianti.recuperoTiroFango
        : 3.4f;
    private float VelocitaProiettileFangoCorrente => varianti != null
        ? varianti.velocitaProiettileFango
        : 6.2f;
    private int DannoFangoCorrente => varianti != null
        ? varianti.dannoFango
        : 1;
    private float DurataFangoCorrente => varianti != null
        ? varianti.durataPozzaFango
        : 4.5f;
    private float MoltiplicatoreRallentamentoFangoCorrente => varianti != null
        ? varianti.moltiplicatoreRallentamentoFango
        : 0.58f;
    private float DistanzaInizioScavoCorrente => varianti != null
        ? varianti.distanzaInizioScavo
        : 5f;
    private float DurataScavoCorrente => varianti != null
        ? varianti.durataScavo
        : 0.8f;
    private float DurataEmersioneCorrente => varianti != null
        ? varianti.durataEmersione
        : 0.35f;
    private float RecuperoScavoCorrente => varianti != null
        ? varianti.recuperoScavo
        : 5.2f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatoRuntime()
    {
        volpiAttive.Clear();
        cacheFrameCorsa.Clear();
        cacheFrameMorte.Clear();
        spriteBarra = null;
    }

    void Awake()
    {
        ApplicaBilanciamento();
        scalaPrefab = transform.localScale;

        corpo = GetComponent<Rigidbody2D>();
        corpo.interpolation = RigidbodyInterpolation2D.Interpolate;
        colliderFisico = GetComponent<Collider2D>();

        spriteRendererVisibile = CreaRendererVisivo(GetComponent<SpriteRenderer>());
        spriteIdle = spriteRendererVisibile != null
            ? spriteRendererVisibile.sprite
            : null;
        coloreBase = spriteRendererVisibile != null
            ? spriteRendererVisibile.color
            : Color.white;

        feedbackImpatto = gameObject.AddComponent<CombatHitFeedback2D>();
        feedbackImpatto.Configura(grafica, spriteRendererVisibile);

        CaricaGraficaVariante(TipoVolpe.Comune);

        ombraRenderer = OmbraDinamica2D.Crea(
            transform,
            spriteRendererVisibile,
            new Vector2(0f, -0.36f),
            new Vector2(0.74f, 0.24f)
        );

        vitaMassima = Mathf.Max(1, vitaBase);
        vitaCorrente = vitaMassima;

        CreaBarraVita();
        AggiornaBarraVita();
    }

    void OnEnable()
    {
        volpiAttive.Add(this);
    }

    void ApplicaBilanciamento()
    {
        FoxBalanceSettings bilanciamento = GameBalanceConfig.Corrente.Volpe;

        speed = Mathf.Max(0f, bilanciamento.velocita);
        accelerazione = Mathf.Max(0f, bilanciamento.accelerazione);
        decelerazione = Mathf.Max(0f, bilanciamento.decelerazione);
        moltiplicatoreInversione = Mathf.Max(
            1f,
            bilanciamento.moltiplicatoreInversione
        );
        distanzaRipresaInseguimento = Mathf.Max(
            0f,
            bilanciamento.distanzaRipresaInseguimento
        );
        distanzaAttacco = Mathf.Max(0f, bilanciamento.distanzaAttacco);
        danno = Mathf.Max(0, bilanciamento.danno);
        intervalloAttacco = Mathf.Max(
            0.01f,
            bilanciamento.intervalloAttacco
        );
        moltiplicatoreRallentamento = Mathf.Clamp(
            bilanciamento.moltiplicatoreRallentamento,
            0.05f,
            1f
        );
        vitaBase = Mathf.Max(1, bilanciamento.vitaPrimaOndata);
        monetePerEliminazione = Mathf.Max(
            0,
            bilanciamento.monetePerEliminazione
        );
        dropChance = Mathf.Clamp(bilanciamento.probabilitaDrop, 0f, 100f);
        probabilitaDenteSulDrop = Mathf.Clamp01(
            bilanciamento.probabilitaDenteSulDrop
        );
    }

    void Start()
    {
        ProvaAcquisireGiocatore(true);
    }

    void Update()
    {
        if (morto) return;

        if (tempoRallentamentoBuild > 0f)
        {
            tempoRallentamentoBuild = Mathf.Max(
                0f,
                tempoRallentamentoBuild - Time.deltaTime
            );
            if (tempoRallentamentoBuild <= 0f)
            {
                moltiplicatoreRallentamentoBuild = 1f;
            }
        }
        if (tempoRallentamentoTerreno > 0f)
        {
            tempoRallentamentoTerreno = Mathf.Max(
                0f,
                tempoRallentamentoTerreno - Time.deltaTime
            );
            if (tempoRallentamentoTerreno <= 0f)
            {
                moltiplicatoreRallentamentoTerreno = 1f;
            }
        }

        CalcolaMovimentoEAttacco();

        bool staCamminando = velocitaAttuale.sqrMagnitude > 0.01f;
        float fattoreCadenza = Mathf.Clamp(
            velocitaAttuale.magnitude / Mathf.Max(0.01f, speed),
            0.35f,
            1.5f
        );

        AggiornaAnimazioneCorsa(staCamminando, fattoreCadenza);
        AggiornaMovimentoVisivo(staCamminando, fattoreCadenza);
    }

    void FixedUpdate()
    {
        if (morto || corpo == null) return;
        if (GameManager.instance != null &&
            !GameManager.instance.GameplayAttivo)
        {
            velocitaAttuale = Vector2.zero;
            velocitaDesiderata = Vector2.zero;
            corpo.linearVelocity = Vector2.zero;
            return;
        }

        if (tipo == TipoVolpe.Scavatrice &&
            statoScavatrice == StatoScavatrice.Sotterranea)
        {
            float progresso = 1f - timerScavatrice / DurataScavoCorrente;
            progresso = Mathf.Clamp01(progresso);
            progresso = progresso * progresso * (3f - 2f * progresso);
            corpo.MovePosition(Vector2.Lerp(
                partenzaScavo,
                destinazioneScavo,
                progresso
            ));
            corpo.linearVelocity = Vector2.zero;
            velocitaAttuale = Vector2.zero;
            velocitaDesiderata = Vector2.zero;
            velocitaSpinta = Vector2.zero;
            return;
        }

        float rapiditaCambio = velocitaDesiderata.sqrMagnitude > 0.001f
            ? accelerazione
            : decelerazione;

        if (Vector2.Dot(velocitaAttuale, velocitaDesiderata) < 0f)
        {
            rapiditaCambio *= moltiplicatoreInversione;
        }
        if (tipo == TipoVolpe.Alfa &&
            statoAlfa == StatoAttaccoAlfa.Scatto)
        {
            rapiditaCambio *= 8f;
        }

        velocitaAttuale = Vector2.MoveTowards(
            velocitaAttuale,
            velocitaDesiderata,
            rapiditaCambio * Time.fixedDeltaTime
        );

        corpo.MovePosition(
            corpo.position +
            (velocitaAttuale + velocitaSpinta) * Time.fixedDeltaTime
        );
        velocitaSpinta = Vector2.MoveTowards(
            velocitaSpinta,
            Vector2.zero,
            7.5f * Time.fixedDeltaTime
        );
    }

    void CalcolaMovimentoEAttacco()
    {
        velocitaDesiderata = Vector2.zero;

        if (GameManager.instance != null &&
            !GameManager.instance.GameplayAttivo)
        {
            SospendiComportamento();
            return;
        }

        if (!ProvaAcquisireGiocatore())
        {
            InterrompiAbilitaSpeciali();
            staInseguendo = false;
            return;
        }

        if (tipo == TipoVolpe.Schivatrice && GestisciSchivatrice())
        {
            return;
        }

        switch (tipo)
        {
            case TipoVolpe.Alfa:
                GestisciAlfa();
                return;
            case TipoVolpe.Ululatrice:
                GestisciUlulatrice();
                return;
            case TipoVolpe.Sputafango:
                GestisciSputafango();
                return;
            case TipoVolpe.Scavatrice:
                GestisciScavatrice();
                return;
        }

        GestisciInseguimentoGiocatore(tipo == TipoVolpe.Agile);
    }

    void GestisciInseguimentoGiocatore(bool usaSerpentina)
    {
        if (target == null || playerHealth == null) return;

        float distanzaDalGiocatore = Vector2.Distance(
            transform.position,
            target.position
        );

        if (staInseguendo && distanzaDalGiocatore <= distanzaAttacco)
        {
            staInseguendo = false;
        }
        else if (!staInseguendo &&
                 distanzaDalGiocatore > distanzaRipresaInseguimento)
        {
            staInseguendo = true;
        }

        if (staInseguendo)
        {
            ImpostaMovimentoVerso(target.position, speed, usaSerpentina);
        }
        else if (distanzaDalGiocatore <= distanzaAttacco &&
                 Time.time >= prossimoAttacco)
        {
            playerHealth.SubisciDanno(danno);
            prossimoAttacco = Time.time + IntervalloAttaccoEffettivo(
                intervalloAttacco
            );
        }
    }

    bool GestisciSchivatrice()
    {
        if (timerSchivata > 0f)
        {
            timerSchivata = Mathf.Max(0f, timerSchivata - Time.deltaTime);
            staInseguendo = true;
            ImpostaMovimentoDirezione(
                direzioneSchivata,
                speed * MoltiplicatoreVelocitaSchivata
            );
            return true;
        }

        if (Time.time < prossimaSchivata ||
            Time.time < prossimoControlloProiettili)
        {
            return false;
        }

        prossimoControlloProiettili =
            Time.time + IntervalloControlloProiettili;
        Vector2 velocitaProiettile;
        if (!TrovaProiettileInArrivo(out velocitaProiettile)) return false;

        Vector2 laterale = new Vector2(
            -velocitaProiettile.y,
            velocitaProiettile.x
        ).normalized;
        if (laterale.sqrMagnitude < 0.001f) laterale = Vector2.up;

        Vector2 posizione = corpo != null
            ? corpo.position
            : (Vector2)transform.position;
        Vector2 riferimento = target != null
            ? (Vector2)target.position
            : Vector2.zero;
        float distanzaPositiva = (posizione + laterale - riferimento).sqrMagnitude;
        float distanzaNegativa = (posizione - laterale - riferimento).sqrMagnitude;
        if (Mathf.Abs(distanzaPositiva - distanzaNegativa) < 0.001f)
        {
            if ((indiceSpawn & 1) != 0) laterale = -laterale;
        }
        else if (distanzaNegativa > distanzaPositiva)
        {
            laterale = -laterale;
        }

        direzioneSchivata = laterale;
        timerSchivata = DurataSchivataCorrente;
        prossimaSchivata = Time.time + RecuperoSchivataCorrente;
        velocitaSpinta = laterale * Mathf.Clamp(
            speed * MoltiplicatoreVelocitaSchivata,
            4.2f,
            7.5f
        );
        if (presentazioneVariante != null)
        {
            presentazioneVariante.RiproduciAbilita();
        }
        FoxAbilityVfx.CreaAnello(
            posizione,
            new Color32(88, 228, 220, 230),
            0.18f,
            0.72f,
            DurataSchivataCorrente,
            transform
        );
        ImpostaMovimentoDirezione(
            direzioneSchivata,
            speed * MoltiplicatoreVelocitaSchivata
        );
        return true;
    }

    bool TrovaProiettileInArrivo(out Vector2 velocitaScelta)
    {
        velocitaScelta = Vector2.zero;
        Vector2 posizione = corpo != null
            ? corpo.position
            : (Vector2)transform.position;
        int quantita = Physics2D.OverlapCircle(
            posizione,
            DistanzaAttivazioneSchivata,
            filtroProiettili,
            bufferProiettili
        );
        float tempoMigliore = float.PositiveInfinity;

        for (int i = 0; i < quantita; i++)
        {
            Collider2D collider = bufferProiettili[i];
            bufferProiettili[i] = null;
            if (collider == null) continue;
            Proiettile proiettile = collider.GetComponentInParent<Proiettile>();
            if (proiettile == null || proiettile.Consumato) continue;
            Rigidbody2D corpoProiettile = proiettile.GetComponent<Rigidbody2D>();
            if (corpoProiettile == null) continue;

            Vector2 velocita = corpoProiettile.linearVelocity;
            float velocitaQuadrata = velocita.sqrMagnitude;
            if (velocitaQuadrata < 0.25f) continue;
            Vector2 versoVolpe = posizione - corpoProiettile.position;
            float tempoImpatto = Vector2.Dot(versoVolpe, velocita) /
                                  velocitaQuadrata;
            if (tempoImpatto < 0f || tempoImpatto > 0.38f) continue;

            Vector2 distanzaMinima = versoVolpe - velocita * tempoImpatto;
            float raggioSicurezza = 0.64f * Mathf.Max(
                transform.lossyScale.x,
                transform.lossyScale.y
            );
            if (distanzaMinima.sqrMagnitude >
                raggioSicurezza * raggioSicurezza)
            {
                continue;
            }
            if (tempoImpatto >= tempoMigliore) continue;
            tempoMigliore = tempoImpatto;
            velocitaScelta = velocita;
        }
        return tempoMigliore < float.PositiveInfinity;
    }

    void GestisciUlulatrice()
    {
        if (target == null || playerHealth == null) return;

        if (statoUlulatrice == StatoUlulatrice.Canalizzazione)
        {
            timerUlulatrice -= Time.deltaTime;
            staInseguendo = false;
            velocitaDesiderata = Vector2.zero;
            if (grafica != null)
            {
                float impulso = 1f + 0.08f * Mathf.Sin(Time.time * 20f);
                grafica.localScale = Vector3.one * impulso;
            }
            if (presentazioneVariante != null)
            {
                float progresso = 1f - timerUlulatrice /
                    DurataCanalizzazioneUlulatoCorrente;
                presentazioneVariante.ImpostaTelegraphAbilita(
                    true,
                    progresso
                );
            }
            if (timerUlulatrice <= 0f)
            {
                EmettiUlulato();
                statoUlulatrice = StatoUlulatrice.Recupero;
                timerUlulatrice = 0.38f;
                if (grafica != null) grafica.localScale = Vector3.one;
                if (presentazioneVariante != null)
                {
                    presentazioneVariante.ImpostaTelegraphAbilita(false, 0f);
                    presentazioneVariante.RiproduciAbilita();
                }
            }
            return;
        }

        if (statoUlulatrice == StatoUlulatrice.Recupero)
        {
            timerUlulatrice -= Time.deltaTime;
            staInseguendo = false;
            velocitaDesiderata = Vector2.zero;
            if (timerUlulatrice <= 0f)
            {
                statoUlulatrice = StatoUlulatrice.Inseguimento;
            }
            return;
        }

        if (Time.time >= prossimoUlulato && ContaCompagneNelRaggio() >= 2)
        {
            statoUlulatrice = StatoUlulatrice.Canalizzazione;
            timerUlulatrice = DurataCanalizzazioneUlulatoCorrente;
            prossimoUlulato = Time.time + RecuperoUlulatoCorrente;
            velocitaAttuale = Vector2.zero;
            velocitaDesiderata = Vector2.zero;
            if (presentazioneVariante != null)
            {
                presentazioneVariante.ImpostaTelegraphAbilita(true, 0f);
            }
            FoxAbilityVfx.CreaAnello(
                transform.position,
                new Color32(192, 112, 255, 235),
                0.35f,
                RaggioUlulatoCorrente,
                DurataCanalizzazioneUlulatoCorrente,
                transform
            );
            return;
        }

        GestisciInseguimentoGiocatore(false);
    }

    int ContaCompagneNelRaggio()
    {
        int quantita = 0;
        float raggioQuadrato = RaggioUlulatoCorrente * RaggioUlulatoCorrente;
        Vector2 centro = transform.position;
        foreach (EnemyAI volpe in volpiAttive)
        {
            if (volpe == null || volpe.morto || !volpe.isActiveAndEnabled)
            {
                continue;
            }
            if (((Vector2)volpe.transform.position - centro).sqrMagnitude <=
                raggioQuadrato)
            {
                quantita++;
            }
        }
        return quantita;
    }

    void EmettiUlulato()
    {
        float raggioQuadrato = RaggioUlulatoCorrente * RaggioUlulatoCorrente;
        Vector2 centro = transform.position;
        foreach (EnemyAI volpe in volpiAttive)
        {
            if (volpe == null || volpe.morto || !volpe.isActiveAndEnabled)
            {
                continue;
            }
            if (((Vector2)volpe.transform.position - centro).sqrMagnitude >
                raggioQuadrato)
            {
                continue;
            }
            volpe.ApplicaBuffUlulato(
                DurataBuffUlulatoCorrente,
                MoltiplicatoreVelocitaBuffUlulato,
                MoltiplicatoreCadenzaBuffUlulato
            );
        }
        FoxAbilityVfx.CreaAnello(
            centro,
            new Color32(233, 180, 255, 245),
            0.45f,
            RaggioUlulatoCorrente,
            0.42f,
            null
        );
    }

    public void ApplicaBuffUlulato(
        float durata,
        float moltiplicatoreVelocita,
        float moltiplicatoreCadenza
    )
    {
        if (morto || durata <= 0f) return;
        buffUlulatoFino = Mathf.Max(buffUlulatoFino, Time.time + durata);
        moltiplicatoreVelocitaUlulato = Mathf.Max(
            moltiplicatoreVelocitaUlulato,
            Mathf.Clamp(moltiplicatoreVelocita, 1f, 1.6f)
        );
        moltiplicatoreCadenzaUlulato = Mathf.Max(
            moltiplicatoreCadenzaUlulato,
            Mathf.Clamp(moltiplicatoreCadenza, 1f, 1.7f)
        );
        FoxAbilityVfx.CreaAnello(
            transform.position,
            new Color32(203, 130, 255, 190),
            0.2f,
            0.75f,
            0.34f,
            transform
        );
    }

    void GestisciSputafango()
    {
        if (target == null || playerHealth == null) return;

        if (statoSputafango == StatoSputafango.Mira)
        {
            timerSputafango -= Time.deltaTime;
            staInseguendo = false;
            velocitaDesiderata = Vector2.zero;
            if (presentazioneVariante != null)
            {
                float progresso = 1f - timerSputafango / DurataMiraFango;
                presentazioneVariante.ImpostaTelegraphAbilita(
                    true,
                    progresso
                );
            }
            if (timerSputafango <= 0f)
            {
                SputaFango();
                statoSputafango = StatoSputafango.Movimento;
                if (presentazioneVariante != null)
                {
                    presentazioneVariante.ImpostaTelegraphAbilita(false, 0f);
                    presentazioneVariante.RiproduciAbilita();
                }
                prossimoSputoFango = Time.time +
                    IntervalloAttaccoEffettivo(RecuperoSputoFangoCorrente);
            }
            return;
        }

        Vector2 posizione = corpo != null
            ? corpo.position
            : (Vector2)transform.position;
        Vector2 versoGiocatore = (Vector2)target.position - posizione;
        float distanza = versoGiocatore.magnitude;
        Vector2 direzione = distanza > 0.001f
            ? versoGiocatore / distanza
            : Vector2.right;

        if (Time.time >= prossimoSputoFango &&
            distanza >= 2.1f &&
            distanza <= DistanzaTiroFangoCorrente + 0.9f)
        {
            statoSputafango = StatoSputafango.Mira;
            timerSputafango = DurataMiraFango;
            bersaglioSputoFango = target.position;
            velocitaAttuale = Vector2.zero;
            velocitaDesiderata = Vector2.zero;
            if (presentazioneVariante != null)
            {
                presentazioneVariante.ImpostaTelegraphAbilita(true, 0f);
            }
            FoxAbilityVfx.CreaLinea(
                transform,
                bersaglioSputoFango,
                new Color32(144, 94, 48, 230),
                DurataMiraFango
            );
            return;
        }

        staInseguendo = true;
        if (distanza > 5.15f)
        {
            ImpostaMovimentoDirezione(direzione, speed);
        }
        else if (distanza < 3.25f)
        {
            ImpostaMovimentoDirezione(-direzione, speed * 1.12f);
        }
        else
        {
            Vector2 tangente = new Vector2(-direzione.y, direzione.x);
            if ((indiceSpawn & 1) != 0) tangente = -tangente;
            float correzione = Mathf.Clamp((distanza - 4.15f) * 0.32f, -0.3f, 0.3f);
            ImpostaMovimentoDirezione(
                (tangente + direzione * correzione).normalized,
                speed * 0.78f
            );
        }
    }

    void SputaFango()
    {
        Vector2 origine = transform.position;
        Vector2 direzione = bersaglioSputoFango - origine;
        if (direzione.sqrMagnitude < 0.001f) direzione = Vector2.right;
        direzione.Normalize();
        FoxMudProjectile.Crea(
            origine + direzione * 0.46f,
            direzione,
            VelocitaProiettileFangoCorrente,
            DannoFangoCorrente,
            MoltiplicatoreRallentamentoFangoCorrente,
            Mathf.Min(2.2f, DurataFangoCorrente)
        );
    }

    void GestisciScavatrice()
    {
        if (target == null || playerHealth == null) return;

        switch (statoScavatrice)
        {
            case StatoScavatrice.Preparazione:
                timerScavatrice -= Time.deltaTime;
                staInseguendo = false;
                velocitaDesiderata = Vector2.zero;
                if (grafica != null)
                {
                    float impulso = 1f + 0.06f * Mathf.Sin(Time.time * 24f);
                    grafica.localScale = new Vector3(impulso, 1f / impulso, 1f);
                }
                if (presentazioneVariante != null)
                {
                    float progresso = 1f - timerScavatrice /
                        DurataPreparazioneScavo;
                    presentazioneVariante.ImpostaTelegraphAbilita(
                        true,
                        progresso
                    );
                }
                if (timerScavatrice <= 0f) EntraSottoterra();
                return;

            case StatoScavatrice.Sotterranea:
                timerScavatrice -= Time.deltaTime;
                staInseguendo = false;
                velocitaDesiderata = Vector2.zero;
                if (Time.time >= prossimaTracciaScavo)
                {
                    prossimaTracciaScavo = Time.time + 0.11f;
                    FoxAbilityVfx.CreaTracciaScavo(transform.position);
                }
                if (timerScavatrice <= 0f) EmergeDalTerreno();
                return;

            case StatoScavatrice.Emersione:
                timerScavatrice -= Time.deltaTime;
                staInseguendo = false;
                velocitaDesiderata = Vector2.zero;
                if (timerScavatrice <= 0f)
                {
                    statoScavatrice = StatoScavatrice.Inseguimento;
                    prossimoScavo = Time.time + RecuperoScavoCorrente;
                }
                return;
        }

        float distanza = Vector2.Distance(transform.position, target.position);
        if (Time.time >= prossimoScavo &&
            distanza >= DistanzaInizioScavoCorrente)
        {
            PreparaScavo();
            return;
        }
        GestisciInseguimentoGiocatore(false);
    }

    void PreparaScavo()
    {
        Vector2 posizione = corpo != null
            ? corpo.position
            : (Vector2)transform.position;
        Vector2 centro = target != null
            ? (Vector2)target.position
            : Vector2.zero;
        Vector2 esterna = posizione - centro;
        if (esterna.sqrMagnitude < 0.01f)
        {
            float angolo = Mathf.Repeat(indiceSpawn * 137.5f, 360f) *
                            Mathf.Deg2Rad;
            esterna = new Vector2(Mathf.Cos(angolo), Mathf.Sin(angolo));
        }
        esterna.Normalize();
        Vector2 laterale = new Vector2(-esterna.y, esterna.x) *
                           ((indiceSpawn & 1) == 0 ? 0.42f : -0.42f);
        partenzaScavo = posizione;
        destinazioneScavo = centro + esterna * 1.72f + laterale;
        statoScavatrice = StatoScavatrice.Preparazione;
        timerScavatrice = DurataPreparazioneScavo;
        velocitaAttuale = Vector2.zero;
        velocitaDesiderata = Vector2.zero;
        if (presentazioneVariante != null)
        {
            presentazioneVariante.ImpostaTelegraphAbilita(true, 0f);
        }
        FoxAbilityVfx.CreaAnello(
            posizione,
            new Color32(174, 112, 50, 235),
            0.2f,
            1.05f,
            DurataPreparazioneScavo,
            transform
        );
    }

    void EntraSottoterra()
    {
        statoScavatrice = StatoScavatrice.Sotterranea;
        timerScavatrice = DurataScavoCorrente;
        prossimaTracciaScavo = 0f;
        velocitaAttuale = Vector2.zero;
        velocitaDesiderata = Vector2.zero;
        velocitaSpinta = Vector2.zero;
        if (presentazioneVariante != null)
        {
            presentazioneVariante.ImpostaTelegraphAbilita(false, 0f);
            presentazioneVariante.RiproduciAbilita();
        }
        ImpostaScavatriceVisibile(false);
        FoxAbilityVfx.CreaTracciaScavo(transform.position);
    }

    void EmergeDalTerreno()
    {
        if (corpo != null) corpo.position = destinazioneScavo;
        transform.position = destinazioneScavo;
        statoScavatrice = StatoScavatrice.Emersione;
        timerScavatrice = DurataEmersioneCorrente;
        ImpostaScavatriceVisibile(true);
        if (grafica != null) grafica.localScale = Vector3.one;
        FoxAbilityVfx.CreaAnello(
            destinazioneScavo,
            new Color32(214, 145, 67, 245),
            0.18f,
            1.2f,
            DurataEmersioneCorrente,
            null
        );
    }

    void ImpostaScavatriceVisibile(bool visibile)
    {
        if (colliderFisico != null) colliderFisico.enabled = visibile;
        if (spriteRendererVisibile != null) spriteRendererVisibile.enabled = visibile;
        if (ombraRenderer != null) ombraRenderer.enabled = visibile;
        if (barraVita != null) barraVita.gameObject.SetActive(visibile && !morto);
    }

    float IntervalloAttaccoEffettivo(float intervalloBase)
    {
        float cadenza = BuffUlulatoAttivo
            ? moltiplicatoreCadenzaUlulato
            : 1f;
        return Mathf.Max(0.05f, intervalloBase / Mathf.Max(1f, cadenza));
    }

    private bool ProvaAcquisireGiocatore(bool forzaRicerca = false)
    {
        if (target != null && playerHealth != null &&
            target.gameObject.activeInHierarchy)
        {
            return true;
        }

        // Un'Alfa non deve terminare una carica preparata contro un Player
        // precedente dopo che il bersaglio e stato rimpiazzato.
        InterrompiAbilitaSpeciali();
        target = null;
        playerHealth = null;
        if (!forzaRicerca && Time.time < prossimaRicercaGiocatore)
        {
            return false;
        }

        prossimaRicercaGiocatore = Time.time + 0.5f;
        GameObject giocatore = GameObject.FindGameObjectWithTag("Player");
        if (giocatore == null) return false;

        PlayerHealth salute = giocatore.GetComponent<PlayerHealth>();
        if (salute == null) return false;
        target = giocatore.transform;
        playerHealth = salute;
        return true;
    }

    private void SospendiComportamento()
    {
        staInseguendo = false;
        velocitaAttuale = Vector2.zero;
        velocitaDesiderata = Vector2.zero;
        velocitaSpinta = Vector2.zero;
        if (corpo != null) corpo.linearVelocity = Vector2.zero;
        InterrompiAbilitaSpeciali();
    }

    private void InterrompiAbilitaSpeciali()
    {
        timerSchivata = 0f;
        InterrompiAttaccoAlfa();
        statoUlulatrice = StatoUlulatrice.Inseguimento;
        timerUlulatrice = 0f;
        statoSputafango = StatoSputafango.Movimento;
        timerSputafango = 0f;
        if (statoScavatrice != StatoScavatrice.Inseguimento)
        {
            statoScavatrice = StatoScavatrice.Inseguimento;
            timerScavatrice = 0f;
            ImpostaScavatriceVisibile(true);
        }
        if (grafica != null) grafica.localScale = Vector3.one;
        if (presentazioneVariante != null)
        {
            presentazioneVariante.ImpostaTelegraphAbilita(false, 0f);
        }
    }

    private void InterrompiAttaccoAlfa()
    {
        if (tipo != TipoVolpe.Alfa) return;
        statoAlfa = StatoAttaccoAlfa.Inseguimento;
        timerStatoAlfa = 0f;
        scattoAlfaHaColpito = false;
        if (presentazioneVariante != null)
        {
            presentazioneVariante.ImpostaTelegraphAlfa(false, 0f);
        }
    }

    void GestisciAlfa()
    {
        if (target == null || playerHealth == null) return;

        float distanza = Vector2.Distance(transform.position, target.position);
        switch (statoAlfa)
        {
            case StatoAttaccoAlfa.Preparazione:
                timerStatoAlfa -= Time.deltaTime;
                staInseguendo = false;
                if (presentazioneVariante != null)
                {
                    float progresso = 1f - timerStatoAlfa /
                        Mathf.Max(0.01f, varianti.durataPreparazioneAlfa);
                    presentazioneVariante.ImpostaTelegraphAlfa(
                        true,
                        progresso
                    );
                }
                if (timerStatoAlfa <= 0f)
                {
                    statoAlfa = StatoAttaccoAlfa.Scatto;
                    timerStatoAlfa = varianti.durataScattoAlfa;
                    scattoAlfaHaColpito = false;
                    if (presentazioneVariante != null)
                    {
                        presentazioneVariante.ImpostaTelegraphAlfa(false, 0f);
                        presentazioneVariante.RiproduciScattoAlfa();
                    }
                }
                return;

            case StatoAttaccoAlfa.Scatto:
                timerStatoAlfa -= Time.deltaTime;
                staInseguendo = true;
                ImpostaMovimentoDirezione(
                    direzioneScattoAlfa,
                    speed * varianti.moltiplicatoreScattoAlfa
                );
                if (!scattoAlfaHaColpito &&
                    distanza <= distanzaAttacco * 1.18f)
                {
                    scattoAlfaHaColpito = true;
                    playerHealth.ProvaSubireDanno(danno);
                }
                if (timerStatoAlfa <= 0f)
                {
                    statoAlfa = StatoAttaccoAlfa.Recupero;
                    timerStatoAlfa = 0.34f;
                    prossimoAttacco = Time.time +
                        IntervalloAttaccoEffettivo(
                            varianti.recuperoScattoAlfa
                        );
                }
                return;

            case StatoAttaccoAlfa.Recupero:
                timerStatoAlfa -= Time.deltaTime;
                staInseguendo = false;
                if (timerStatoAlfa <= 0f)
                {
                    statoAlfa = StatoAttaccoAlfa.Inseguimento;
                }
                return;
        }

        if (distanza <= varianti.distanzaPreparazioneAlfa &&
            Time.time >= prossimoAttacco)
        {
            direzioneScattoAlfa =
                ((Vector2)target.position - (Vector2)transform.position)
                .normalized;
            if (direzioneScattoAlfa.sqrMagnitude < 0.001f)
            {
                direzioneScattoAlfa = Vector2.right;
            }
            statoAlfa = StatoAttaccoAlfa.Preparazione;
            timerStatoAlfa = varianti.durataPreparazioneAlfa;
            velocitaAttuale = Vector2.zero;
            staInseguendo = false;
            if (presentazioneVariante != null)
            {
                presentazioneVariante.ImpostaTelegraphAlfa(true, 0f);
                presentazioneVariante.RiproduciCaricaAlfa();
            }
            return;
        }

        staInseguendo = true;
        ImpostaMovimentoVerso(target.position, speed, false);
    }

    void ImpostaMovimentoVerso(
        Vector2 posizione,
        float velocita,
        bool usaSerpentina
    )
    {
        Vector2 direzione = posizione - corpo.position;
        if (direzione.sqrMagnitude < 0.0001f) return;
        direzione.Normalize();

        if (usaSerpentina && profiloVariante != null)
        {
            Vector2 laterale = new Vector2(-direzione.y, direzione.x);
            float onda = Mathf.Sin(
                Time.time * profiloVariante.frequenzaSerpentina *
                Mathf.PI * 2f + faseSerpentina
            );
            direzione = (direzione +
                laterale * onda * profiloVariante.ampiezzaSerpentina)
                .normalized;
        }
        ImpostaMovimentoDirezione(direzione, velocita);
    }

    void ImpostaMovimentoDirezione(Vector2 direzione, float velocita)
    {
        float fattoreRallentamento = 1f;
        if (isSlowed)
        {
            fattoreRallentamento = Mathf.Min(
                fattoreRallentamento,
                moltiplicatoreRallentamento
            );
        }
        if (tempoRallentamentoBuild > 0f)
        {
            fattoreRallentamento = Mathf.Min(
                fattoreRallentamento,
                moltiplicatoreRallentamentoBuild
            );
        }
        if (tempoRallentamentoTerreno > 0f)
        {
            fattoreRallentamento = Mathf.Min(
                fattoreRallentamento,
                moltiplicatoreRallentamentoTerreno
            );
        }
        float buffVelocita = BuffUlulatoAttivo
            ? moltiplicatoreVelocitaUlulato
            : 1f;
        float velocitaCorrente =
            velocita * fattoreRallentamento * buffVelocita;
        Vector2 direzioneNormalizzata = direzione.sqrMagnitude > 0.0001f
            ? direzione.normalized
            : Vector2.zero;
        velocitaDesiderata = direzioneNormalizzata * velocitaCorrente;

        if (spriteRendererVisibile != null &&
            Mathf.Abs(direzioneNormalizzata.x) > 0.01f)
        {
            spriteRendererVisibile.flipX = direzioneNormalizzata.x < 0f;
        }
    }

    void CaricaGraficaVariante(TipoVolpe tipoRichiesto)
    {
        TipoVolpe tipoValido = FoxVariantStyle.Normalizza(tipoRichiesto);
        frameCorsa = OttieniFrameCorsa(tipoValido);
        frameMorte = OttieniFrameMorte(tipoValido);
        timerAnimazione = 0f;

        if (frameCorsa != null && frameCorsa.Length > 0)
        {
            spriteIdle = frameCorsa[0];
            if (spriteRendererVisibile != null)
            {
                spriteRendererVisibile.sprite = spriteIdle;
            }
        }
        if (spriteRendererVisibile != null)
        {
            spriteRendererVisibile.color = Color.white;
        }
        coloreBase = Color.white;
    }

    static Sprite[] OttieniFrameCorsa(TipoVolpe tipo)
    {
        Sprite[] frame;
        if (cacheFrameCorsa.TryGetValue(tipo, out frame)) return frame;

        frame = CaricaFrame("Foxes/" + tipo + "/Run");
        if ((frame == null || frame.Length == 0) && tipo != TipoVolpe.Comune)
        {
            frame = CaricaFrame("Foxes/" + TipoVolpe.Comune + "/Run");
        }
        if (frame == null || frame.Length == 0)
        {
            // Compatibilita con il set grafico storico durante la migrazione.
            frame = CaricaFrame("FoxRun");
        }
        if (frame == null) frame = System.Array.Empty<Sprite>();
        cacheFrameCorsa[tipo] = frame;
        return frame;
    }

    static Sprite[] OttieniFrameMorte(TipoVolpe tipo)
    {
        Sprite[] frame;
        if (cacheFrameMorte.TryGetValue(tipo, out frame)) return frame;

        frame = CaricaFrame("Foxes/" + tipo + "/Death");
        if ((frame == null || frame.Length == 0) && tipo == TipoVolpe.Comune)
        {
            frame = CaricaFrame("FoxDeath");
        }
        // Una variante senza Death dedicata mantiene la propria silhouette:
        // AnimaMorte applica rotazione e dissolvenza al frame di corsa attuale.
        if (frame == null) frame = System.Array.Empty<Sprite>();
        cacheFrameMorte[tipo] = frame;
        return frame;
    }

    static Sprite[] CaricaFrame(string percorso)
    {
        Sprite[] frame = Resources.LoadAll<Sprite>(percorso);
        if (frame != null && frame.Length > 1)
        {
            System.Array.Sort(frame, (a, b) =>
                string.CompareOrdinal(a.name, b.name)
            );
        }
        return frame;
    }

    void AggiornaAnimazioneCorsa(bool staCamminando, float fattoreCadenza)
    {
        if (spriteRendererVisibile == null) return;

        if (!staCamminando || frameCorsa == null || frameCorsa.Length == 0)
        {
            timerAnimazione = 0f;
            if (spriteIdle != null)
            {
                spriteRendererVisibile.sprite = spriteIdle;
            }
            return;
        }

        timerAnimazione += Time.deltaTime * fattoreCadenza;
        int indice = Mathf.FloorToInt(timerAnimazione * frameCorsaAlSecondo) %
                     frameCorsa.Length;
        spriteRendererVisibile.sprite = frameCorsa[indice];
    }

    public void InizializzaVariante(
        TipoVolpe nuovoTipo,
        int vitaBaseOndata,
        int nuovoIndiceSpawn,
        bool riproduciVerso = true
    )
    {
        if (morto) return;

        ApplicaBilanciamento();
        tipo = FoxVariantStyle.Normalizza(nuovoTipo);
        indiceSpawn = Mathf.Max(0, nuovoIndiceSpawn);
        faseSerpentina = indiceSpawn * 1.618034f;
        CaricaGraficaVariante(tipo);
        varianti = GameBalanceConfig.Corrente.VariantiVolpe;
        profiloVariante = varianti.Ottieni(tipo);

        speed *= profiloVariante.moltiplicatoreVelocita;
        speed *= GameBalanceConfig.Corrente.Difficolta.Ottieni(
            ProgressionePartita.DifficoltaCorrente
        ).moltiplicatoreVelocita;
        accelerazione *= profiloVariante.moltiplicatoreAccelerazione;
        decelerazione *= profiloVariante.moltiplicatoreDecelerazione;
        intervalloAttacco *=
            profiloVariante.moltiplicatoreIntervalloAttacco;

        if (tipo != TipoVolpe.Comune)
        {
            monetePerEliminazione = Mathf.Max(
                0,
                profiloVariante.monetePerEliminazione
            );
        }

        float scala = Mathf.Max(0.1f, profiloVariante.scala);
        transform.localScale = new Vector3(
            scalaPrefab.x * scala,
            scalaPrefab.y * scala,
            scalaPrefab.z
        );

        float vitaCalcolata = Mathf.Max(1, vitaBaseOndata) *
            profiloVariante.moltiplicatoreVita;
        int vitaVariante = Mathf.Max(
            1,
            tipo == TipoVolpe.Agile
                ? Mathf.RoundToInt(vitaCalcolata)
                : Mathf.CeilToInt(vitaCalcolata)
        );
        InizializzaVita(vitaVariante);

        if (feedbackImpatto != null)
        {
            feedbackImpatto.ConfiguraMoltiplicatoreRinculo(
                profiloVariante.moltiplicatoreRinculo
            );
        }

        if (presentazioneVariante == null)
        {
            presentazioneVariante = GetComponent<FoxVariantPresentation>();
        }
        if (presentazioneVariante == null)
        {
            presentazioneVariante = gameObject.AddComponent<
                FoxVariantPresentation
            >();
        }
        presentazioneVariante.Configura(
            tipo,
            spriteRendererVisibile,
            grafica,
            riproduciVerso
        );
        if (spriteRendererVisibile != null)
        {
            // I tipi hanno silhouette e palette proprie: nessuna tinta runtime
            // deve alterare i pixel dedicati caricati da Resources/Foxes.
            spriteRendererVisibile.color = Color.white;
        }
        coloreBase = Color.white;

        statoAlfa = StatoAttaccoAlfa.Inseguimento;
        statoUlulatrice = StatoUlulatrice.Inseguimento;
        statoSputafango = StatoSputafango.Movimento;
        statoScavatrice = StatoScavatrice.Inseguimento;
        timerSchivata = 0f;
        prossimaSchivata = Time.time + 0.45f;
        prossimoControlloProiettili = Time.time +
            (indiceSpawn % 4) * 0.01f;
        prossimoUlulato = Time.time + 2.6f + (indiceSpawn % 4) * 0.55f;
        prossimoSputoFango = Time.time + 1.25f +
            (indiceSpawn % 3) * 0.28f;
        prossimoScavo = Time.time + 2.2f + (indiceSpawn % 4) * 0.42f;
        buffUlulatoFino = 0f;
        moltiplicatoreVelocitaUlulato = 1f;
        moltiplicatoreCadenzaUlulato = 1f;
        prossimoAttacco = tipo == TipoVolpe.Alfa
            ? Time.time + 0.35f
            : Time.time;
        gameObject.name = "Volpe_" + FoxVariantStyle.Nome(tipo);
    }

    public void InizializzaVita(int nuovaVitaMassima)
    {
        if (morto) return;

        vitaMassima = Mathf.Max(1, nuovaVitaMassima);
        vitaCorrente = vitaMassima;
        AggiornaBarraVita();
    }

    public void SubisciDanno(int quantita)
    {
        ProvaSubireDanno(quantita);
    }

    public void ApplicaRallentamento(
        float moltiplicatore,
        float durata
    )
    {
        if (morto || durata <= 0f || moltiplicatore >= 1f) return;

        float valore = Mathf.Clamp(moltiplicatore, 0.1f, 0.95f);
        if (tempoRallentamentoBuild <= 0f)
        {
            moltiplicatoreRallentamentoBuild = valore;
        }
        else
        {
            moltiplicatoreRallentamentoBuild = Mathf.Min(
                moltiplicatoreRallentamentoBuild,
                valore
            );
        }
        tempoRallentamentoBuild = Mathf.Max(
            tempoRallentamentoBuild,
            durata
        );
    }

    public void ApplicaRallentamentoTerreno(
        float moltiplicatore,
        float durata
    )
    {
        if (morto) return;

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

    public void ApplicaSpinta(Vector2 direzione, float forza)
    {
        if (morto || forza <= 0f || direzione.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float resistenza = profiloVariante != null
            ? Mathf.Clamp(profiloVariante.moltiplicatoreRinculo, 0f, 2f)
            : 1f;
        velocitaSpinta +=
            direzione.normalized * forza * resistenza;
        velocitaSpinta = Vector2.ClampMagnitude(velocitaSpinta, 5.5f);
    }

    public EsitoDanno ProvaSubireDanno(int quantita)
    {
        if (morto || quantita <= 0) return EsitoDanno.NessunDanno;

        int vitaPrecedente = vitaCorrente;
        vitaCorrente = Mathf.Max(0, vitaCorrente - quantita);
        AggiornaBarraVita();
        AvviaFlashDanno();

        bool ucciso = vitaCorrente == 0;
        if (ucciso)
        {
            Die();
        }

        return new EsitoDanno(
            true,
            ucciso,
            ucciso,
            vitaPrecedente - vitaCorrente
        );
    }

    void AvviaFlashDanno()
    {
        if (spriteRendererVisibile == null) return;
        if (GameOptionsController.Instance != null &&
            !GameOptionsController.Instance.FlashAttivi)
        {
            return;
        }

        if (flashDannoRoutine != null)
        {
            StopCoroutine(flashDannoRoutine);
        }

        Color rossoDanno = new Color(1f, 0.15f, 0.15f, coloreBase.a);
        spriteRendererVisibile.color = Color.Lerp(coloreBase, rossoDanno, 0.75f);
        flashDannoRoutine = StartCoroutine(RipristinaColoreDopoFlash());
    }

    IEnumerator RipristinaColoreDopoFlash()
    {
        yield return new WaitForSeconds(Mathf.Max(0.01f, durataFlashDanno));

        if (spriteRendererVisibile != null)
        {
            spriteRendererVisibile.color = coloreBase;
        }

        flashDannoRoutine = null;
    }

    void CreaBarraVita()
    {
        GameObject contenitore = new GameObject("BarraVita");
        contenitore.layer = gameObject.layer;

        barraVita = contenitore.transform;
        barraVita.SetParent(transform, false);
        barraVita.localPosition = posizioneBarraVita;

        int sortingLayer = spriteRendererVisibile != null
            ? spriteRendererVisibile.sortingLayerID
            : 0;
        int ordineBase = spriteRendererVisibile != null
            ? spriteRendererVisibile.sortingOrder + 20
            : 20;

        SpriteRenderer sfondo = CreaElementoBarra(
            "Sfondo",
            new Color(0.12f, 0.05f, 0.04f, 0.95f),
            sortingLayer,
            ordineBase
        );
        sfondo.transform.localScale = new Vector3(
            larghezzaBarraVita,
            altezzaBarraVita,
            1f
        );

        rendererRiempimentoBarra = CreaElementoBarra(
            "Riempimento",
            Color.green,
            sortingLayer,
            ordineBase + 1
        );
        riempimentoBarra = rendererRiempimentoBarra.transform;
    }

    SpriteRenderer CreaElementoBarra(
        string nome,
        Color colore,
        int sortingLayer,
        int sortingOrder
    )
    {
        GameObject elemento = new GameObject(nome);
        elemento.layer = gameObject.layer;
        elemento.transform.SetParent(barraVita, false);

        SpriteRenderer renderer = elemento.AddComponent<SpriteRenderer>();
        renderer.sprite = OttieniSpriteBarra();
        renderer.color = colore;
        renderer.sortingLayerID = sortingLayer;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    static Sprite OttieniSpriteBarra()
    {
        if (spriteBarra != null) return spriteBarra;

        Texture2D texture = Texture2D.whiteTexture;
        spriteBarra = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            Mathf.Max(1, texture.width),
            0,
            SpriteMeshType.FullRect
        );
        spriteBarra.name = "SpriteBarraVita";
        return spriteBarra;
    }

    void AggiornaBarraVita()
    {
        if (riempimentoBarra == null || rendererRiempimentoBarra == null) return;

        float rapporto = vitaMassima > 0
            ? Mathf.Clamp01((float)vitaCorrente / vitaMassima)
            : 0f;

        float larghezzaRiempimento = larghezzaBarraVita * rapporto;
        riempimentoBarra.localScale = new Vector3(
            larghezzaRiempimento,
            altezzaBarraVita * 0.62f,
            1f
        );
        riempimentoBarra.localPosition = new Vector3(
            (larghezzaRiempimento - larghezzaBarraVita) * 0.5f,
            0f,
            0f
        );

        Color coloreVitaBassa = new Color(0.92f, 0.12f, 0.08f, 1f);
        Color coloreVitaPiena = new Color(0.18f, 0.88f, 0.28f, 1f);
        rendererRiempimentoBarra.color = Color.Lerp(
            coloreVitaBassa,
            coloreVitaPiena,
            rapporto
        );
    }

    SpriteRenderer CreaRendererVisivo(SpriteRenderer rendererOriginale)
    {
        if (rendererOriginale == null) return null;

        GameObject oggettoGrafico = new GameObject("Grafica");
        oggettoGrafico.layer = gameObject.layer;

        grafica = oggettoGrafico.transform;
        grafica.SetParent(transform, false);

        SpriteRenderer nuovoRenderer = oggettoGrafico.AddComponent<SpriteRenderer>();
        nuovoRenderer.sprite = rendererOriginale.sprite;
        nuovoRenderer.color = rendererOriginale.color;
        nuovoRenderer.flipX = rendererOriginale.flipX;
        nuovoRenderer.flipY = rendererOriginale.flipY;
        nuovoRenderer.drawMode = rendererOriginale.drawMode;
        nuovoRenderer.size = rendererOriginale.size;
        nuovoRenderer.maskInteraction = rendererOriginale.maskInteraction;
        nuovoRenderer.spriteSortPoint = rendererOriginale.spriteSortPoint;
        nuovoRenderer.sortingLayerID = rendererOriginale.sortingLayerID;
        nuovoRenderer.sortingOrder = rendererOriginale.sortingOrder;
        nuovoRenderer.sharedMaterials = rendererOriginale.sharedMaterials;
        nuovoRenderer.enabled = rendererOriginale.enabled;

        rendererOriginale.enabled = false;
        return nuovoRenderer;
    }

    void AggiornaMovimentoVisivo(bool staCamminando, float fattoreCadenza)
    {
        if (grafica == null) return;

        float frequenza = staCamminando
            ? frequenzaCamminata * fattoreCadenza
            : frequenzaIdle;
        float ampiezza = staCamminando ? ampiezzaCamminata : ampiezzaIdle;

        faseMovimento = Mathf.Repeat(
            faseMovimento + Time.deltaTime * frequenza * Mathf.PI * 2f,
            Mathf.PI * 2f
        );

        float onda = staCamminando
            ? (1f - Mathf.Cos(faseMovimento)) * 0.5f
            : Mathf.Sin(faseMovimento);

        float offsetDesiderato = onda * ampiezza;
        float transizione = 1f - Mathf.Exp(-velocitaTransizione * Time.deltaTime);
        offsetVerticale = Mathf.Lerp(offsetVerticale, offsetDesiderato, transizione);

        grafica.localPosition = Vector3.up * offsetVerticale;
    }

    public void Die()
    {
        if (morto) return;
        InterrompiAbilitaSpeciali();
        morto = true;
        SegnalaNeutralizzazione();

        velocitaAttuale = Vector2.zero;
        velocitaDesiderata = Vector2.zero;
        velocitaSpinta = Vector2.zero;
        staInseguendo = false;

        if (colliderFisico != null)
        {
            colliderFisico.enabled = false;
        }
        if (corpo != null)
        {
            corpo.linearVelocity = Vector2.zero;
            corpo.angularVelocity = 0f;
        }
        if (barraVita != null)
        {
            barraVita.gameObject.SetActive(false);
        }
        if (flashDannoRoutine != null)
        {
            StopCoroutine(flashDannoRoutine);
            flashDannoRoutine = null;
        }
        if (presentazioneVariante != null)
        {
            presentazioneVariante.NascondiPerMorte();
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.RegistraVolpeEliminata(tipo);
            GameManager.instance.AggiungiMonete(monetePerEliminazione);
        }
        if (Random.Range(0f, 100f) < dropChance)
        {
            if (Random.value > 1f - probabilitaDenteSulDrop &&
                dentePrefab != null)
                Instantiate(dentePrefab, transform.position, Quaternion.identity);
            else if (codaPrefab != null)
                Instantiate(codaPrefab, transform.position, Quaternion.identity);
        }

        StartCoroutine(AnimaMorte());
    }

    IEnumerator AnimaMorte()
    {
        bool usaMorteAlternativa =
            tipo != TipoVolpe.Comune &&
            (frameMorte == null || frameMorte.Length == 0);
        bool ripristinaColoreDopoPrimoFrame =
            spriteRendererVisibile != null &&
            spriteRendererVisibile.color != coloreBase;

        if (grafica != null)
        {
            grafica.localPosition = Vector3.zero;
            grafica.localRotation = Quaternion.identity;
        }

        if (spriteRendererVisibile != null &&
            frameMorte != null &&
            frameMorte.Length > 0)
        {
            int numeroFrame = Mathf.Min(4, frameMorte.Length);
            float durataFrame = 1f / Mathf.Max(1f, frameMorteAlSecondo);

            for (int i = 0; i < numeroFrame; i++)
            {
                spriteRendererVisibile.sprite = frameMorte[i];
                yield return new WaitForSeconds(durataFrame);

                if (i == 0 && ripristinaColoreDopoPrimoFrame)
                {
                    spriteRendererVisibile.color = coloreBase;
                    ripristinaColoreDopoPrimoFrame = false;
                }
            }
        }

        if (ripristinaColoreDopoPrimoFrame &&
            spriteRendererVisibile != null)
        {
            spriteRendererVisibile.color = coloreBase;
        }

        float tempo = 0f;
        float durata = Mathf.Max(0.01f, durataDissolvenza);
        Color coloreIniziale = spriteRendererVisibile != null
            ? spriteRendererVisibile.color
            : coloreBase;
        Color coloreOmbraIniziale = ombraRenderer != null
            ? ombraRenderer.color
            : Color.clear;

        while (tempo < durata)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / durata);

            if (spriteRendererVisibile != null)
            {
                Color colore = coloreIniziale;
                colore.a = Mathf.Lerp(coloreIniziale.a, 0f, t);
                spriteRendererVisibile.color = colore;
            }
            if (ombraRenderer != null)
            {
                Color colore = coloreOmbraIniziale;
                colore.a = Mathf.Lerp(coloreOmbraIniziale.a, 0f, t);
                ombraRenderer.color = colore;
            }
            if (usaMorteAlternativa && grafica != null)
            {
                float verso = (indiceSpawn & 1) == 0 ? 1f : -1f;
                grafica.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Lerp(0f, 24f * verso, t)
                );
                grafica.localScale = new Vector3(
                    Mathf.Lerp(1f, 0.82f, t),
                    Mathf.Lerp(1f, 0.68f, t),
                    1f
                );
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDisable()
    {
        volpiAttive.Remove(this);
        InterrompiAbilitaSpeciali();
        SegnalaNeutralizzazione();
        flashDannoRoutine = null;
        velocitaAttuale = Vector2.zero;
        velocitaDesiderata = Vector2.zero;
        velocitaSpinta = Vector2.zero;
        tempoRallentamentoBuild = 0f;
        moltiplicatoreRallentamentoBuild = 1f;
        tempoRallentamentoTerreno = 0f;
        moltiplicatoreRallentamentoTerreno = 1f;

        if (spriteRendererVisibile != null)
        {
            spriteRendererVisibile.color = coloreBase;
        }
    }

    private void SegnalaNeutralizzazione()
    {
        if (neutralizzazioneSegnalata) return;
        neutralizzazioneSegnalata = true;
        NonPiuMinaccia?.Invoke(this);
    }
}
