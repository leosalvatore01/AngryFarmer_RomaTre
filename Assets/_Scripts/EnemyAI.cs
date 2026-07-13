using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public float speed = 2.4f;

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

    public int danno = 1;
    public float distanzaAttacco = 0.8f;
    public float intervalloAttacco = 1f;

    private Transform target;
    private PlayerHealth playerHealth;
    private float prossimoAttacco;
    private Transform grafica;
    private SpriteRenderer spriteRendererVisibile;
    private Color coloreBase;
    private float faseMovimento;
    private float offsetVerticale;

    private int vitaMassima;
    private int vitaCorrente;
    private bool morto;
    private Coroutine flashDannoRoutine;

    private Transform barraVita;
    private Transform riempimentoBarra;
    private SpriteRenderer rendererRiempimentoBarra;

    private static Sprite spriteBarra;

    public GameObject dentePrefab;
    public GameObject codaPrefab;
    [Range(0, 100)] public float dropChance = 50f;

    public static bool isSlowed = false;

    public SpriteRenderer RendererVisibile => spriteRendererVisibile;
    public int VitaMassima => vitaMassima;
    public int VitaCorrente => vitaCorrente;

    void Awake()
    {
        spriteRendererVisibile = CreaRendererVisivo(GetComponent<SpriteRenderer>());
        coloreBase = spriteRendererVisibile != null
            ? spriteRendererVisibile.color
            : Color.white;

        vitaMassima = Mathf.Max(1, vitaBase);
        vitaCorrente = vitaMassima;

        CreaBarraVita();
        AggiornaBarraVita();
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
        bool staCamminando = false;

        if (target != null && playerHealth != null)
        {
            float distanzaDalGiocatore = Vector2.Distance(transform.position, target.position);

            if (distanzaDalGiocatore > distanzaAttacco)
            {
                float velocitaCorrente = isSlowed ? speed / 2f : speed;
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    target.position,
                    velocitaCorrente * Time.deltaTime
                );

                float direzioneOrizzontale = target.position.x - transform.position.x;
                if (spriteRendererVisibile != null && Mathf.Abs(direzioneOrizzontale) > 0.01f)
                {
                    spriteRendererVisibile.flipX = direzioneOrizzontale < 0f;
                }

                staCamminando = true;
            }
            else if (Time.time >= prossimoAttacco)
            {
                playerHealth.SubisciDanno(danno);
                prossimoAttacco = Time.time + intervalloAttacco;
            }
        }

        AggiornaMovimentoVisivo(staCamminando);
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
        if (morto || quantita <= 0) return;

        vitaCorrente = Mathf.Max(0, vitaCorrente - quantita);
        AggiornaBarraVita();
        AvviaFlashDanno();

        if (vitaCorrente == 0)
        {
            Die();
        }
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

    void AggiornaMovimentoVisivo(bool staCamminando)
    {
        if (grafica == null) return;

        float frequenza = staCamminando ? frequenzaCamminata : frequenzaIdle;
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

        if (GameManager.instance != null)
        {
            GameManager.instance.AggiungiMonete(1);
        }
        if (Random.Range(0f, 100f) < dropChance)
        {
            if (Random.value > 0.5f && dentePrefab != null)
                Instantiate(dentePrefab, transform.position, Quaternion.identity);
            else if (codaPrefab != null)
                Instantiate(codaPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void OnDisable()
    {
        flashDannoRoutine = null;

        if (spriteRendererVisibile != null)
        {
            spriteRendererVisibile.color = coloreBase;
        }
    }
}
