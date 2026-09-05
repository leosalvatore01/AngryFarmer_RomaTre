using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyAI), typeof(Rigidbody2D))]
public sealed class BossFoxController : MonoBehaviour
{
    private const string PercorsoSpriteBoss =
        "Foxes/Boss/IlReDelBranco";

    private EnemyAI nemico;
    private Rigidbody2D corpo;
    private BoxCollider2D colliderBoss;
    private SpriteRenderer rendererBoss;
    private PlayerHealth giocatore;
    private SpecialEncounterBalanceSettings impostazioni;
    private Vector2 velocita;
    private Vector3 posizioneGraficaBase;
    private float prossimoAttacco;
    private float prossimoDannoContatto;
    private float prossimoImpulsoFaseDue;
    private int indiceAttacco;
    private bool attaccoInCorso;
    private bool transizioneFaseEseguita;
    private bool fineGestita;

    public EnemyAI Nemico => nemico;
    public string NomeVisualizzato => "Il Re del Branco";
    public int NumeroOndata { get; private set; }
    public int Apparizione { get; private set; }
    public int FaseCorrente { get; private set; } = 1;
    public bool IsDead => nemico == null || nemico.IsDead;
    public float PercentualeVita => nemico != null && nemico.VitaMassima > 0
        ? Mathf.Clamp01(nemico.VitaCorrente / (float)nemico.VitaMassima)
        : 0f;
    public bool AttaccoInCorso => attaccoInCorso;

    public static BossFoxController Configura(
        EnemyAI nemico,
        int numeroOndata
    )
    {
        if (nemico == null) return null;

        BossFoxController boss = nemico.GetComponent<BossFoxController>();
        if (boss == null)
        {
            boss = nemico.gameObject.AddComponent<BossFoxController>();
        }
        boss.Inizializza(numeroOndata);
        return boss;
    }

    void Awake()
    {
        nemico = GetComponent<EnemyAI>();
        corpo = GetComponent<Rigidbody2D>();
        colliderBoss = GetComponent<BoxCollider2D>();
    }

    private void Inizializza(int numeroOndata)
    {
        NumeroOndata = Mathf.Max(1, numeroOndata);
        WaveBalanceSettings ondate = GameBalanceConfig.Corrente.Ondate;
        impostazioni = ondate.IncontriSpeciali;
        int cadenzaBoss = ondate.capitoli != null
            ? ondate.capitoli.bossOgniOnde
            : 10;
        Apparizione = SpecialEncounterDirector.CalcolaApparizioneBoss(
            NumeroOndata,
            cadenzaBoss
        );

        nemico.ImpostaControlloEsterno(true);
        Sprite spriteBoss = CaricaSpriteBoss();
        if (spriteBoss != null)
        {
            nemico.ImpostaGraficaDedicata(spriteBoss);
        }
        else
        {
            Debug.LogError(
                "Sprite del boss non trovato in Resources/" +
                PercorsoSpriteBoss + ".",
                this
            );
        }

        rendererBoss = nemico.RendererVisibile;
        if (rendererBoss != null)
        {
            posizioneGraficaBase = rendererBoss.transform.localPosition;
        }
        nemico.NascondiBarraVitaLocale();
        nemico.InizializzaVita(
            SpecialEncounterDirector.CalcolaVitaBoss(
                nemico.VitaMassima,
                Apparizione,
                impostazioni
            )
        );
        nemico.ConfiguraRicompensaSpeciale(
            SpecialEncounterDirector.CalcolaMoneteBoss(
                Apparizione,
                impostazioni
            ),
            impostazioni.dropGarantitiBoss
        );

        transform.localScale *= impostazioni.moltiplicatoreScalaBoss;
        if (colliderBoss != null)
        {
            colliderBoss.size = new Vector2(1.42f, 0.92f);
            colliderBoss.offset = new Vector2(0.04f, -0.08f);
        }

        FaseCorrente = 1;
        transizioneFaseEseguita = false;
        fineGestita = false;
        indiceAttacco = Mathf.Max(0, Apparizione - 1) % 3;
        prossimoAttacco = Time.time + 1.35f;
        prossimoImpulsoFaseDue = float.PositiveInfinity;
        gameObject.name = "BOSS_Il_Re_del_Branco_" + Apparizione;

        FoxAbilityVfx.CreaAnello(
            transform.position,
            new Color32(255, 177, 45, 240),
            0.3f,
            2.1f,
            1.05f,
            transform
        );
        BossHealthBarController.Mostra(this);
    }

    void Update()
    {
        if (nemico == null || nemico.IsDead)
        {
            GestisciFine();
            return;
        }
        if (!GameplayDisponibile())
        {
            velocita = Vector2.zero;
            return;
        }

        if (giocatore == null || giocatore.VitaCorrente <= 0)
        {
            giocatore = FindFirstObjectByType<PlayerHealth>();
            if (giocatore == null || giocatore.VitaCorrente <= 0)
            {
                velocita = Vector2.zero;
                return;
            }
        }

        if (!transizioneFaseEseguita &&
            PercentualeVita <= impostazioni.sogliaSecondaFase &&
            !attaccoInCorso)
        {
            StartCoroutine(EseguiTransizioneSecondaFase());
        }
        else if (!attaccoInCorso && Time.time >= prossimoAttacco)
        {
            int attacco = indiceAttacco % 3;
            indiceAttacco++;
            StartCoroutine(EseguiAttacco(attacco));
        }

        if (FaseCorrente == 2 && Time.time >= prossimoImpulsoFaseDue)
        {
            prossimoImpulsoFaseDue = Time.time + 1.15f;
            FoxAbilityVfx.CreaAnello(
                transform.position,
                new Color32(231, 66, 42, 180),
                0.52f,
                1.25f,
                0.65f,
                transform
            );
        }

        AggiornaAspetto();
        ProvaDannoContatto();
    }

    void FixedUpdate()
    {
        if (IsDead || corpo == null || !GameplayDisponibile()) return;
        if (attaccoInCorso || giocatore == null)
        {
            velocita = Vector2.zero;
            corpo.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 versoGiocatore =
            (Vector2)giocatore.transform.position - corpo.position;
        Vector2 direzione = versoGiocatore.sqrMagnitude > 0.001f
            ? versoGiocatore.normalized
            : Vector2.zero;
        float velocitaMassima = impostazioni.velocitaInseguimento *
            (FaseCorrente == 2 ? impostazioni.velocitaSecondaFase : 1f);
        velocita = Vector2.MoveTowards(
            velocita,
            direzione * velocitaMassima,
            7f * Time.fixedDeltaTime
        );
        corpo.MovePosition(
            corpo.position + velocita * Time.fixedDeltaTime
        );
        corpo.linearVelocity = Vector2.zero;
    }

    private IEnumerator EseguiTransizioneSecondaFase()
    {
        attaccoInCorso = true;
        velocita = Vector2.zero;
        FaseCorrente = 2;
        transizioneFaseEseguita = true;

        float durata = impostazioni.durataTransizioneFase;
        FoxAbilityVfx.CreaAnello(
            transform.position,
            new Color32(255, 84, 39, 245),
            0.35f,
            2.8f,
            durata,
            transform
        );
        FoxAbilityVfx.CreaAnello(
            transform.position,
            new Color32(255, 200, 70, 225),
            0.25f,
            1.9f,
            durata * 0.72f,
            transform
        );
        if (nemico.PresentazioneVariante != null)
        {
            nemico.PresentazioneVariante.RiproduciAbilita();
        }

        yield return AttendiGameplay(durata);
        attaccoInCorso = false;
        prossimoImpulsoFaseDue = Time.time;
        prossimoAttacco = Time.time + 0.45f;
    }

    private IEnumerator EseguiAttacco(int attacco)
    {
        attaccoInCorso = true;
        velocita = Vector2.zero;
        if (nemico.PresentazioneVariante != null)
        {
            nemico.PresentazioneVariante.RiproduciAbilita();
        }

        switch (attacco)
        {
            case 0:
                yield return EseguiCarica();
                break;
            case 1:
                yield return EseguiSchianto();
                break;
            default:
                yield return EseguiRaffica();
                break;
        }

        attaccoInCorso = false;
        float moltiplicatore = FaseCorrente == 2
            ? impostazioni.intervalloSecondaFase
            : 1f;
        prossimoAttacco = Time.time +
            impostazioni.intervalloAttacchi * moltiplicatore;
    }

    private IEnumerator EseguiCarica()
    {
        Vector2 direzione = DirezioneVersoGiocatore();
        float durataPreavviso = impostazioni.durataPreavvisoCarica *
            (FaseCorrente == 2 ? 0.78f : 1f);
        Vector2 destinazione = (Vector2)transform.position +
            direzione * impostazioni.velocitaCarica *
            impostazioni.durataCarica;

        FoxAbilityVfx.CreaLinea(
            transform,
            destinazione,
            new Color32(255, 67, 48, 235),
            durataPreavviso
        );
        FoxAbilityVfx.CreaAnello(
            transform.position,
            new Color32(255, 174, 45, 220),
            0.35f,
            1.15f,
            durataPreavviso,
            transform
        );
        yield return AttendiGameplay(durataPreavviso);
        if (!BersaglioValido()) yield break;

        bool haColpito = false;
        float tempo = 0f;
        float durata = impostazioni.durataCarica *
            (FaseCorrente == 2 ? 1.14f : 1f);
        while (tempo < durata && !IsDead && BersaglioValido())
        {
            if (GameplayDisponibile())
            {
                float delta = Time.deltaTime;
                tempo += delta;
                corpo.position = corpo.position +
                    direzione * impostazioni.velocitaCarica * delta;
                if (!haColpito && DistanzaDalGiocatore() <= 1.15f)
                {
                    haColpito = DanneggiaGiocatore(
                        TipoAttaccoNemico.CaricaBoss
                    );
                }
            }
            yield return null;
        }

        FoxAbilityVfx.CreaAnello(
            transform.position,
            new Color32(238, 91, 42, 225),
            0.2f,
            1.3f,
            0.35f,
            null
        );
    }

    private IEnumerator EseguiSchianto()
    {
        float durata = impostazioni.durataPreavvisoSchianto *
            (FaseCorrente == 2 ? 0.8f : 1f);
        float raggio = impostazioni.raggioSchianto *
            (FaseCorrente == 2 ? 1.18f : 1f);
        FoxAbilityVfx.CreaAnello(
            transform.position,
            new Color32(255, 190, 54, 245),
            0.25f,
            raggio,
            durata,
            null
        );
        yield return AttendiGameplay(durata);
        if (!BersaglioValido()) yield break;

        if (DistanzaDalGiocatore() <= raggio)
        {
            DanneggiaGiocatore(TipoAttaccoNemico.SchiantoBoss);
        }
        FoxAbilityVfx.CreaAnello(
            transform.position,
            new Color32(221, 65, 38, 235),
            raggio * 0.45f,
            raggio * 1.08f,
            0.32f,
            null
        );

        if (FaseCorrente == 2)
        {
            yield return AttendiGameplay(0.36f);
            float secondoRaggio = raggio * 1.28f;
            FoxAbilityVfx.CreaAnello(
                transform.position,
                new Color32(255, 104, 42, 225),
                raggio * 0.85f,
                secondoRaggio,
                0.34f,
                null
            );
            if (DistanzaDalGiocatore() <= secondoRaggio)
            {
                DanneggiaGiocatore(TipoAttaccoNemico.SchiantoBoss);
            }
        }
    }

    private IEnumerator EseguiRaffica()
    {
        Vector2 direzioneBase = DirezioneVersoGiocatore();
        float durata = impostazioni.durataPreavvisoRaffica *
            (FaseCorrente == 2 ? 0.82f : 1f);
        Vector2 mira = (Vector2)transform.position + direzioneBase * 5.6f;
        FoxAbilityVfx.CreaLinea(
            transform,
            mira,
            new Color32(255, 151, 46, 235),
            durata
        );
        FoxAbilityVfx.CreaAnello(
            transform.position,
            new Color32(255, 96, 43, 215),
            0.25f,
            1.1f,
            durata,
            transform
        );
        yield return AttendiGameplay(durata);
        if (!BersaglioValido()) yield break;

        int quantita = FaseCorrente == 2
            ? impostazioni.proiettiliSecondaFase
            : impostazioni.proiettiliPrimaFase;
        float apertura = impostazioni.aperturaRaffica;
        for (int i = 0; i < quantita; i++)
        {
            float t = quantita <= 1 ? 0.5f : i / (float)(quantita - 1);
            float angolo = Mathf.Lerp(-apertura * 0.5f, apertura * 0.5f, t);
            Vector2 direzione = Quaternion.Euler(0f, 0f, angolo) *
                direzioneBase;
            BossFireProjectile.Crea(
                this,
                (Vector2)transform.position + direzione * 0.85f,
                direzione,
                impostazioni.velocitaProiettili *
                    (FaseCorrente == 2 ? 1.12f : 1f),
                DannoCorrente()
            );
        }
    }

    private IEnumerator AttendiGameplay(float durata)
    {
        float trascorso = 0f;
        while (trascorso < durata && !IsDead)
        {
            if (giocatore != null && giocatore.VitaCorrente <= 0)
            {
                yield break;
            }
            if (GameplayDisponibile()) trascorso += Time.deltaTime;
            yield return null;
        }
    }

    private void AggiornaAspetto()
    {
        if (rendererBoss == null) return;

        Vector2 direzione = DirezioneVersoGiocatore();
        if (Mathf.Abs(direzione.x) > 0.05f)
        {
            rendererBoss.flipX = direzione.x < 0f;
        }
        float ampiezza = attaccoInCorso ? 0.035f : 0.07f;
        rendererBoss.transform.localPosition = posizioneGraficaBase +
            Vector3.up * (Mathf.Sin(Time.time * 3.2f) * ampiezza);
    }

    private void ProvaDannoContatto()
    {
        if (giocatore == null || Time.time < prossimoDannoContatto) return;
        if (DistanzaDalGiocatore() > 1.05f) return;

        prossimoDannoContatto = Time.time + 0.9f;
        giocatore.ProvaSubireDanno(
            Mathf.Max(1, nemico.danno),
            TipoAttaccoNemico.ContattoVolpe,
            TipoVolpe.Alfa
        );
    }

    private bool DanneggiaGiocatore(TipoAttaccoNemico tipoAttacco)
    {
        return giocatore != null && giocatore.ProvaSubireDanno(
            DannoCorrente(),
            tipoAttacco,
            TipoVolpe.Alfa
        );
    }

    private int DannoCorrente()
    {
        return FaseCorrente == 2
            ? impostazioni.dannoSecondaFase
            : impostazioni.dannoPrimaFase;
    }

    private Vector2 DirezioneVersoGiocatore()
    {
        if (giocatore == null) return Vector2.right;
        Vector2 direzione =
            (Vector2)giocatore.transform.position - (Vector2)transform.position;
        return direzione.sqrMagnitude > 0.001f
            ? direzione.normalized
            : Vector2.right;
    }

    private float DistanzaDalGiocatore()
    {
        return giocatore != null
            ? Vector2.Distance(transform.position, giocatore.transform.position)
            : float.PositiveInfinity;
    }

    private bool GameplayDisponibile()
    {
        return GameManager.instance == null ||
            GameManager.instance.GameplayAttivo;
    }

    private bool BersaglioValido()
    {
        return giocatore != null && giocatore.VitaCorrente > 0 &&
            (GameManager.instance == null || !GameManager.instance.isGameOver);
    }

    private void GestisciFine()
    {
        if (fineGestita) return;
        fineGestita = true;
        velocita = Vector2.zero;
        StopAllCoroutines();
        BossHealthBarController.Nascondi(this);
    }

    private static Sprite CaricaSpriteBoss()
    {
        Sprite sprite = Resources.Load<Sprite>(PercorsoSpriteBoss);
        if (sprite != null) return sprite;

        Sprite[] spriteDisponibili =
            Resources.LoadAll<Sprite>(PercorsoSpriteBoss);
        return spriteDisponibili != null && spriteDisponibili.Length > 0
            ? spriteDisponibili[0]
            : null;
    }

    void OnDisable()
    {
        GestisciFine();
    }
}

internal sealed class BossFireProjectile : MonoBehaviour
{
    private static Sprite spriteProiettile;
    private BossFoxController proprietario;
    private Rigidbody2D corpo;
    private Vector2 direzione;
    private float velocita;
    private int danno;
    private float durata = 4.5f;
    private float prossimaTraccia;
    private bool consumato;

    public static BossFireProjectile Crea(
        BossFoxController proprietario,
        Vector2 posizione,
        Vector2 direzione,
        float velocita,
        int danno
    )
    {
        GameObject oggetto = new GameObject("ProiettileBoss");
        oggetto.transform.position = posizione;
        oggetto.transform.localScale = Vector3.one * 0.62f;

        SpriteRenderer renderer = oggetto.AddComponent<SpriteRenderer>();
        renderer.sprite = OttieniSprite();
        renderer.sortingOrder = 35;

        CircleCollider2D collider = oggetto.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.42f;

        Rigidbody2D corpo = oggetto.AddComponent<Rigidbody2D>();
        corpo.bodyType = RigidbodyType2D.Kinematic;
        corpo.gravityScale = 0f;
        corpo.interpolation = RigidbodyInterpolation2D.Interpolate;
        corpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BossFireProjectile proiettile =
            oggetto.AddComponent<BossFireProjectile>();
        proiettile.proprietario = proprietario;
        proiettile.corpo = corpo;
        proiettile.direzione = direzione.sqrMagnitude > 0.001f
            ? direzione.normalized
            : Vector2.right;
        proiettile.velocita = Mathf.Max(1f, velocita);
        proiettile.danno = Mathf.Max(1, danno);
        return proiettile;
    }

    void FixedUpdate()
    {
        if (consumato || corpo == null) return;
        corpo.MovePosition(
            corpo.position + direzione * velocita * Time.fixedDeltaTime
        );
    }

    void Update()
    {
        if (consumato) return;
        if (proprietario == null || proprietario.IsDead)
        {
            Destroy(gameObject);
            return;
        }
        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            Destroy(gameObject);
            return;
        }

        durata -= Time.deltaTime;
        transform.Rotate(0f, 0f, 280f * Time.deltaTime);
        if (Time.time >= prossimaTraccia)
        {
            prossimaTraccia = Time.time + 0.11f;
            FoxAbilityVfx.CreaAnello(
                transform.position,
                new Color32(255, 100, 38, 150),
                0.03f,
                0.24f,
                0.2f,
                null
            );
        }
        if (durata <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (consumato) return;
        PlayerHealth salute = other.GetComponentInParent<PlayerHealth>();
        if (salute == null) return;

        consumato = true;
        salute.ProvaSubireDanno(
            danno,
            TipoAttaccoNemico.RafficaBoss,
            TipoVolpe.Alfa
        );
        FoxAbilityVfx.CreaAnello(
            transform.position,
            new Color32(255, 81, 35, 240),
            0.12f,
            0.9f,
            0.35f,
            null
        );
        Destroy(gameObject);
    }

    private static Sprite OttieniSprite()
    {
        if (spriteProiettile != null) return spriteProiettile;

        const int lato = 13;
        Color32[] pixel = new Color32[lato * lato];
        Color32 bordo = new Color32(81, 28, 17, 255);
        Color32 fuoco = new Color32(231, 76, 31, 255);
        Color32 luce = new Color32(255, 195, 62, 255);
        for (int y = 1; y < lato - 1; y++)
        {
            for (int x = 1; x < lato - 1; x++)
            {
                float dx = x - 6f;
                float dy = y - 6f;
                float distanza = dx * dx + dy * dy;
                if (distanza <= 28f) pixel[y * lato + x] = bordo;
                if (distanza <= 19f) pixel[y * lato + x] = fuoco;
                if (distanza <= 7f) pixel[y * lato + x] = luce;
            }
        }
        pixel[11 * lato + 5] = fuoco;
        pixel[12 * lato + 6] = bordo;
        pixel[2 * lato + 9] = luce;

        Texture2D texture = new Texture2D(
            lato,
            lato,
            TextureFormat.RGBA32,
            false
        );
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels32(pixel);
        texture.Apply();
        texture.name = "ProiettileBossTexture";
        texture.hideFlags = HideFlags.HideAndDontSave;

        spriteProiettile = Sprite.Create(
            texture,
            new Rect(0f, 0f, lato, lato),
            new Vector2(0.5f, 0.5f),
            13f,
            0,
            SpriteMeshType.FullRect
        );
        spriteProiettile.name = "ProiettileBoss";
        spriteProiettile.hideFlags = HideFlags.HideAndDontSave;
        return spriteProiettile;
    }
}
