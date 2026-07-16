using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour, IDanneggiabile
{
    private enum StatoAttaccoAlfa
    {
        Inseguimento,
        Preparazione,
        Scatto,
        Recupero
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

    private Gallina gallinaBersaglio;
    private bool trasportaGallina;
    private bool fugaLadraCompletata;
    private float prossimoControlloGallina;
    private Vector2 direzioneFugaLadra;

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
    private static Sprite[] cacheFrameCorsa;
    private static Sprite[] cacheFrameMorte;

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
    public bool TrasportaGallina => trasportaGallina;
    public Gallina GallinaBersaglio => gallinaBersaglio;
    public Transform BersaglioCorrente =>
        tipo == TipoVolpe.Ladra && gallinaBersaglio != null && !trasportaGallina
            ? gallinaBersaglio.transform
            : target;
    public bool StaPreparandoAttaccoAlfa =>
        statoAlfa == StatoAttaccoAlfa.Preparazione;
    public bool StaScattandoAlfa =>
        statoAlfa == StatoAttaccoAlfa.Scatto;
    public bool RallentataDaBuild => tempoRallentamentoBuild > 0f;
    public float MoltiplicatoreRallentamentoBuild =>
        RallentataDaBuild ? moltiplicatoreRallentamentoBuild : 1f;
    public Vector2 VelocitaSpinta => velocitaSpinta;
    public FoxVariantPresentation PresentazioneVariante =>
        presentazioneVariante;
    public event System.Action<EnemyAI> NonPiuMinaccia;

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

        if (cacheFrameCorsa == null)
        {
            cacheFrameCorsa = Resources.LoadAll<Sprite>("FoxRun");
            System.Array.Sort(cacheFrameCorsa, (a, b) =>
                string.CompareOrdinal(a.name, b.name)
            );
        }
        frameCorsa = cacheFrameCorsa;
        if (spriteRendererVisibile != null && frameCorsa.Length > 0)
        {
            spriteIdle = frameCorsa[0];
            spriteRendererVisibile.sprite = spriteIdle;
        }

        if (cacheFrameMorte == null)
        {
            cacheFrameMorte = Resources.LoadAll<Sprite>("FoxDeath");
            System.Array.Sort(cacheFrameMorte, (a, b) =>
                string.CompareOrdinal(a.name, b.name)
            );
        }
        frameMorte = cacheFrameMorte;

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

        if (tipo == TipoVolpe.Ladra && GestisciLadra()) return;
        if (!ProvaAcquisireGiocatore())
        {
            InterrompiAttaccoAlfa();
            staInseguendo = false;
            return;
        }
        if (tipo == TipoVolpe.Alfa)
        {
            GestisciAlfa();
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
            prossimoAttacco = Time.time + intervalloAttacco;
        }
    }

    bool GestisciLadra()
    {
        if (trasportaGallina)
        {
            float velocitaFuga = speed * varianti.moltiplicatoreFugaLadra;
            ImpostaMovimentoDirezione(direzioneFugaLadra, velocitaFuga);
            if (((Vector2)transform.position).magnitude >=
                varianti.distanzaFugaLadra)
            {
                CompletaFugaLadra();
            }
            return true;
        }

        if (gallinaBersaglio != null &&
            !gallinaBersaglio.PrenotataDa(this))
        {
            gallinaBersaglio = null;
        }

        if (gallinaBersaglio == null && Time.time >= prossimoControlloGallina)
        {
            prossimoControlloGallina =
                Time.time + varianti.intervalloRicercaGallina;
            gallinaBersaglio = TrovaGallinaDaRubare();
        }

        if (gallinaBersaglio == null) return false;

        float distanza = Vector2.Distance(
            transform.position,
            gallinaBersaglio.transform.position
        );
        if (distanza <= varianti.distanzaPrelievoGallina)
        {
            if (gallinaBersaglio.ProvaPrelevare(this))
            {
                trasportaGallina = true;
                staInseguendo = true;
                direzioneFugaLadra = CalcolaDirezioneFuga();
                if (presentazioneVariante != null)
                {
                    presentazioneVariante.ImpostaTrasportoGallina(true);
                    presentazioneVariante.RiproduciPredazione();
                }
            }
            else
            {
                gallinaBersaglio = null;
            }
            return true;
        }

        staInseguendo = true;
        ImpostaMovimentoVerso(
            gallinaBersaglio.transform.position,
            speed,
            false
        );
        return true;
    }

    Gallina TrovaGallinaDaRubare()
    {
        Gallina migliore = null;
        float distanzaMigliore = float.PositiveInfinity;
        foreach (Gallina gallina in Gallina.Attive)
        {
            if (gallina == null || !gallina.Disponibile) continue;
            float distanza = ((Vector2)gallina.transform.position -
                              (Vector2)transform.position).sqrMagnitude;
            if (distanza >= distanzaMigliore) continue;
            migliore = gallina;
            distanzaMigliore = distanza;
        }
        return migliore != null && migliore.ProvaPrenotare(this)
            ? migliore
            : null;
    }

    Vector2 CalcolaDirezioneFuga()
    {
        Vector2 direzione = transform.position;
        if (direzione.sqrMagnitude > 0.25f) return direzione.normalized;

        float angolo = Mathf.Repeat(
            indiceSpawn * 137.5f + 31f,
            360f
        ) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angolo), Mathf.Sin(angolo));
    }

    void CompletaFugaLadra()
    {
        if (morto || fugaLadraCompletata) return;
        Gallina gallinaRubata = gallinaBersaglio;
        if (gallinaRubata == null ||
            !gallinaRubata.ConfermaPerdita(this))
        {
            if (gallinaRubata != null) gallinaRubata.Rilascia(this);
            NotificaGallinaNonDisponibile(gallinaRubata);
            prossimoControlloGallina =
                Time.time + varianti.intervalloRicercaGallina;
            return;
        }

        fugaLadraCompletata = true;
        trasportaGallina = false;
        gallinaBersaglio = null;
        morto = true;
        SegnalaNeutralizzazione();
        Destroy(gameObject);
    }

    public void NotificaGallinaNonDisponibile(Gallina gallina)
    {
        if (gallina != null && gallinaBersaglio != gallina) return;

        gallinaBersaglio = null;
        trasportaGallina = false;
        staInseguendo = false;
        velocitaAttuale = Vector2.zero;
        velocitaDesiderata = Vector2.zero;
        if (presentazioneVariante != null)
        {
            presentazioneVariante.ImpostaTrasportoGallina(false);
        }
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
        InterrompiAttaccoAlfa();
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
        InterrompiAttaccoAlfa();
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
                    playerHealth.SubisciDanno(danno);
                }
                if (timerStatoAlfa <= 0f)
                {
                    statoAlfa = StatoAttaccoAlfa.Recupero;
                    timerStatoAlfa = 0.34f;
                    prossimoAttacco = Time.time + varianti.recuperoScattoAlfa;
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
        float velocitaCorrente = velocita * fattoreRallentamento;
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
        varianti = GameBalanceConfig.Corrente.VariantiVolpe;
        profiloVariante = varianti.Ottieni(tipo);

        speed *= profiloVariante.moltiplicatoreVelocita;
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
        coloreBase = spriteRendererVisibile != null
            ? spriteRendererVisibile.color
            : FoxVariantStyle.ColoreCorpo(tipo);

        statoAlfa = StatoAttaccoAlfa.Inseguimento;
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

        vitaCorrente = Mathf.Max(0, vitaCorrente - quantita);
        AggiornaBarraVita();
        AvviaFlashDanno();

        bool ucciso = vitaCorrente == 0;
        if (ucciso)
        {
            Die();
        }

        return new EsitoDanno(true, ucciso, ucciso);
    }

    void AvviaFlashDanno()
    {
        if (spriteRendererVisibile == null) return;

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
        RilasciaGallinaSeNecessario();
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

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDisable()
    {
        RilasciaGallinaSeNecessario();
        SegnalaNeutralizzazione();
        flashDannoRoutine = null;
        velocitaAttuale = Vector2.zero;
        velocitaDesiderata = Vector2.zero;
        velocitaSpinta = Vector2.zero;
        tempoRallentamentoBuild = 0f;
        moltiplicatoreRallentamentoBuild = 1f;

        if (spriteRendererVisibile != null)
        {
            spriteRendererVisibile.color = coloreBase;
        }
    }

    private void RilasciaGallinaSeNecessario()
    {
        if (!fugaLadraCompletata && gallinaBersaglio != null)
        {
            gallinaBersaglio.Rilascia(this);
        }
        gallinaBersaglio = null;
        trasportaGallina = false;
        if (presentazioneVariante != null)
        {
            presentazioneVariante.ImpostaTrasportoGallina(false);
            presentazioneVariante.ImpostaTelegraphAlfa(false, 0f);
        }
    }

    private void SegnalaNeutralizzazione()
    {
        if (neutralizzazioneSegnalata) return;
        neutralizzazioneSegnalata = true;
        NonPiuMinaccia?.Invoke(this);
    }
}
