using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour, IDanneggiabile
{
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
    private bool staInseguendo;
    private float prossimoAttacco;
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

    private int vitaMassima;
    private int vitaCorrente;
    private bool morto;
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
    public bool IsDead => morto;

    void Awake()
    {
        ApplicaBilanciamento();

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

        CombatHitFeedback2D feedbackImpatto =
            gameObject.AddComponent<CombatHitFeedback2D>();
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
        GameObject giocatore = GameObject.FindGameObjectWithTag("Player");

        if (giocatore != null)
        {
            target = giocatore.transform;
            playerHealth = giocatore.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (morto) return;

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

        float rapiditaCambio = velocitaDesiderata.sqrMagnitude > 0.001f
            ? accelerazione
            : decelerazione;

        if (Vector2.Dot(velocitaAttuale, velocitaDesiderata) < 0f)
        {
            rapiditaCambio *= moltiplicatoreInversione;
        }

        velocitaAttuale = Vector2.MoveTowards(
            velocitaAttuale,
            velocitaDesiderata,
            rapiditaCambio * Time.fixedDeltaTime
        );

        corpo.MovePosition(
            corpo.position + velocitaAttuale * Time.fixedDeltaTime
        );
    }

    void CalcolaMovimentoEAttacco()
    {
        velocitaDesiderata = Vector2.zero;

        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            staInseguendo = false;
            return;
        }

        if (target != null && playerHealth != null)
        {
            float distanzaDalGiocatore = Vector2.Distance(transform.position, target.position);

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
                float velocitaCorrente = isSlowed
                    ? speed * moltiplicatoreRallentamento
                    : speed;
                Vector2 direzione =
                    ((Vector2)target.position - corpo.position).normalized;
                velocitaDesiderata = direzione * velocitaCorrente;

                float direzioneOrizzontale = target.position.x - transform.position.x;
                if (spriteRendererVisibile != null && Mathf.Abs(direzioneOrizzontale) > 0.01f)
                {
                    spriteRendererVisibile.flipX = direzioneOrizzontale < 0f;
                }
            }
            else if (distanzaDalGiocatore <= distanzaAttacco &&
                     Time.time >= prossimoAttacco)
            {
                playerHealth.SubisciDanno(danno);
                prossimoAttacco = Time.time + intervalloAttacco;
            }
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
        morto = true;

        velocitaAttuale = Vector2.zero;
        velocitaDesiderata = Vector2.zero;
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
        flashDannoRoutine = null;
        velocitaAttuale = Vector2.zero;
        velocitaDesiderata = Vector2.zero;

        if (spriteRendererVisibile != null)
        {
            spriteRendererVisibile.color = coloreBase;
        }
    }
}
