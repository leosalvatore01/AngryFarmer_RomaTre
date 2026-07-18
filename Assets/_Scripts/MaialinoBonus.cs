using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MaialinoBonus : MonoBehaviour, IDanneggiabile
{
    [Header("Passeggio e fuga")]
    [Min(0f)] public float velocitaPasseggio = 1.45f;
    [Min(0f)] public float velocitaFuga = 3.1f;
    [Min(0f)] public float accelerazione = 10f;
    [Min(0f)] public float decelerazione = 14f;
    [Min(0f)] public float raggioFuga = 2.6f;
    [Min(1f)] public float durataSullaMappa = 14f;

    [Header("Ricompensa")]
    [Min(1)] public int vitaBase = 1;
    [Min(0)] public int moneteBase = 3;
    [Min(0.01f)] public float durataFlashDanno = 0.1f;

    [Header("Barra vita")]
    public Vector2 posizioneBarraVita = new Vector2(0f, 0.56f);
    [Min(0.01f)] public float larghezzaBarraVita = 0.74f;
    [Min(0.01f)] public float altezzaBarraVita = 0.1f;
    [Min(0f)] public float spessoreBordoBarra = 0.025f;

    [Header("Animazione")]
    [Min(1f)] public float frameAlSecondo = 5.5f;
    [Min(0f)] public float ampiezzaSaltello = 0.055f;
    [Min(0f)] public float frequenzaSaltello = 2.5f;

    private static readonly HashSet<MaialinoBonus> attivi =
        new HashSet<MaialinoBonus>();
    private static Sprite spriteFrammento;
    private static Sprite spriteMoneta;
    private static Sprite spriteBarraVita;
    private static Sprite[] cacheFrameTrotto;

    private Rigidbody2D corpo;
    private Collider2D colliderFisico;
    private Transform giocatore;
    private Transform grafica;
    private SpriteRenderer spriteRendererVisibile;
    private Sprite[] frameTrotto;
    private Color coloreBase;
    private Vector2 direzionePasseggio;
    private Vector2 velocitaDesiderata;
    private Vector2 velocitaAttuale;
    private float prossimoCambioDirezione;
    private float tempoScadenza;
    private float timerAnimazione;
    private float faseSaltello;
    private float moltiplicatoreInversione = 1.25f;
    private float cambioDirezioneMinimo = 1.4f;
    private float cambioDirezioneMassimo = 2.8f;
    private float ritardoDirezioneDopoFuga = 0.5f;
    private int vitaMassima;
    private int vitaCorrente;
    private int moneteRicompensa;
    private int uovaRicompensa;
    private bool morto;
    private bool ricompensaAssegnata;
    private Coroutine flashRoutine;
    private Transform barraVita;
    private Transform riempimentoBarra;
    private SpriteRenderer rendererRiempimentoBarra;

    public int VitaMassima => vitaMassima;
    public int VitaCorrente => vitaCorrente;
    public static int NumeroAttivi => attivi.Count;

    void Awake()
    {
        ApplicaBilanciamento();

        corpo = GetComponent<Rigidbody2D>();
        corpo.bodyType = RigidbodyType2D.Kinematic;
        corpo.gravityScale = 0f;
        corpo.interpolation = RigidbodyInterpolation2D.Interpolate;

        colliderFisico = GetComponent<Collider2D>();
        colliderFisico.isTrigger = true;

        spriteRendererVisibile =
            CreaRendererVisivo(GetComponent<SpriteRenderer>());
        coloreBase = spriteRendererVisibile != null
            ? spriteRendererVisibile.color
            : Color.white;

        CombatHitFeedback2D feedbackImpatto =
            gameObject.AddComponent<CombatHitFeedback2D>();
        feedbackImpatto.Configura(grafica, spriteRendererVisibile);

        if (cacheFrameTrotto == null)
        {
            cacheFrameTrotto = Resources.LoadAll<Sprite>("PigRun");
            System.Array.Sort(cacheFrameTrotto, (a, b) =>
                string.CompareOrdinal(a.name, b.name)
            );
        }
        frameTrotto = cacheFrameTrotto;
        if (spriteRendererVisibile != null && frameTrotto.Length > 0)
        {
            spriteRendererVisibile.sprite = frameTrotto[0];
        }

        OmbraDinamica2D.Crea(
            transform,
            spriteRendererVisibile,
            new Vector2(0f, -0.34f),
            new Vector2(0.72f, 0.23f)
        );

        vitaMassima = Mathf.Max(1, vitaBase);
        vitaCorrente = vitaMassima;
        moneteRicompensa = Mathf.Max(0, moneteBase);
        uovaRicompensa = Mathf.Max(
            0,
            GameBalanceConfig.Corrente.ObiettiviFattoria.uovaPerMaialino
        );

        CreaBarraVita();
        AggiornaBarraVita();
    }

    void ApplicaBilanciamento()
    {
        PigBalanceSettings bilanciamento = GameBalanceConfig.Corrente.Maialino;

        velocitaPasseggio = Mathf.Max(0f, bilanciamento.velocitaPasseggio);
        velocitaFuga = Mathf.Max(0f, bilanciamento.velocitaFuga);
        accelerazione = Mathf.Max(0f, bilanciamento.accelerazione);
        decelerazione = Mathf.Max(0f, bilanciamento.decelerazione);
        moltiplicatoreInversione = Mathf.Max(
            1f,
            bilanciamento.moltiplicatoreInversione
        );
        raggioFuga = Mathf.Max(0f, bilanciamento.raggioFuga);
        durataSullaMappa = Mathf.Max(0.1f, bilanciamento.durataSullaMappa);
        vitaBase = Mathf.Max(1, bilanciamento.vitaBase);
        moneteBase = Mathf.Max(0, bilanciamento.moneteBase);
        cambioDirezioneMinimo = Mathf.Max(
            0.05f,
            bilanciamento.cambioDirezioneMinimo
        );
        cambioDirezioneMassimo = Mathf.Max(
            cambioDirezioneMinimo,
            bilanciamento.cambioDirezioneMassimo
        );
        ritardoDirezioneDopoFuga = Mathf.Max(
            0f,
            bilanciamento.ritardoDirezioneDopoFuga
        );
    }

    void OnEnable()
    {
        attivi.Add(this);
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            giocatore = player.transform;
        }

        ScegliNuovaDirezione();
        tempoScadenza = Time.time + durataSullaMappa;
    }

    public void Inizializza(int vita, int monete)
    {
        if (morto) return;

        vitaMassima = Mathf.Max(1, vita);
        vitaCorrente = vitaMassima;
        moneteRicompensa = Mathf.Max(0, monete);
        AggiornaBarraVita();
    }

    void Update()
    {
        if (morto) return;

        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            RimuoviSenzaPremio();
            return;
        }

        if (Time.time >= tempoScadenza)
        {
            RimuoviSenzaPremio();
            return;
        }

        CalcolaMovimento();
        AggiornaAnimazione();
        AggiornaSaltello();
    }

    void FixedUpdate()
    {
        if (morto || corpo == null) return;

        float rapidita = velocitaDesiderata.sqrMagnitude > 0.001f
            ? accelerazione
            : decelerazione;
        if (Vector2.Dot(velocitaAttuale, velocitaDesiderata) < 0f)
        {
            rapidita *= moltiplicatoreInversione;
        }

        velocitaAttuale = Vector2.MoveTowards(
            velocitaAttuale,
            velocitaDesiderata,
            rapidita * Time.fixedDeltaTime
        );
        corpo.MovePosition(
            corpo.position + velocitaAttuale * Time.fixedDeltaTime
        );
    }

    void CalcolaMovimento()
    {
        bool staFuggendo = false;
        if (giocatore != null)
        {
            Vector2 dalGiocatore = corpo.position - (Vector2)giocatore.position;
            if (dalGiocatore.sqrMagnitude < raggioFuga * raggioFuga)
            {
                direzionePasseggio = dalGiocatore.sqrMagnitude > 0.001f
                    ? dalGiocatore.normalized
                    : Random.insideUnitCircle.normalized;
                staFuggendo = true;
                prossimoCambioDirezione =
                    Time.time + ritardoDirezioneDopoFuga;
            }
        }

        if (!staFuggendo && Time.time >= prossimoCambioDirezione)
        {
            ScegliNuovaDirezione();
        }

        float velocita = staFuggendo ? velocitaFuga : velocitaPasseggio;
        velocitaDesiderata = direzionePasseggio * velocita;

        if (spriteRendererVisibile != null &&
            Mathf.Abs(direzionePasseggio.x) > 0.01f)
        {
            spriteRendererVisibile.flipX = direzionePasseggio.x < 0f;
        }
    }

    void ScegliNuovaDirezione()
    {
        direzionePasseggio = Random.insideUnitCircle.normalized;
        if (direzionePasseggio.sqrMagnitude < 0.01f)
        {
            direzionePasseggio = Vector2.right;
        }
        prossimoCambioDirezione = Time.time + Random.Range(
            cambioDirezioneMinimo,
            cambioDirezioneMassimo
        );
    }

    void AggiornaAnimazione()
    {
        if (spriteRendererVisibile == null || frameTrotto.Length == 0) return;

        float cadenza = Mathf.Clamp(
            velocitaAttuale.magnitude / Mathf.Max(0.01f, velocitaPasseggio),
            0.45f,
            1.65f
        );
        timerAnimazione += Time.deltaTime * cadenza;
        int indice = Mathf.FloorToInt(timerAnimazione * frameAlSecondo) %
                     frameTrotto.Length;
        spriteRendererVisibile.sprite = frameTrotto[indice];
    }

    void AggiornaSaltello()
    {
        if (grafica == null) return;

        float cadenza = Mathf.Clamp(
            velocitaAttuale.magnitude / Mathf.Max(0.01f, velocitaPasseggio),
            0.5f,
            1.6f
        );
        faseSaltello = Mathf.Repeat(
            faseSaltello + Time.deltaTime * frequenzaSaltello *
            cadenza * Mathf.PI * 2f,
            Mathf.PI * 2f
        );
        float salto = (1f - Mathf.Cos(faseSaltello)) * 0.5f;
        grafica.localPosition = Vector3.up * salto * ampiezzaSaltello;
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
            StartCoroutine(RompiSalvadanaio());
        }

        return new EsitoDanno(
            true,
            ucciso,
            false,
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

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        spriteRendererVisibile.color =
            new Color(1f, 0.28f, 0.28f, coloreBase.a);
        flashRoutine = StartCoroutine(RipristinaColore());
    }

    IEnumerator RipristinaColore()
    {
        yield return new WaitForSeconds(durataFlashDanno);
        if (spriteRendererVisibile != null)
        {
            spriteRendererVisibile.color = coloreBase;
        }
        flashRoutine = null;
    }

    IEnumerator RompiSalvadanaio()
    {
        if (morto) yield break;

        morto = true;
        colliderFisico.enabled = false;
        velocitaDesiderata = Vector2.zero;
        velocitaAttuale = Vector2.zero;
        NascondiBarraVita();

        if (!ricompensaAssegnata)
        {
            ricompensaAssegnata = true;
            if (GameManager.instance != null)
            {
                GameManager.instance.AggiungiMonete(moneteRicompensa);
                GameManager.instance.AggiungiUova(uovaRicompensa);
            }
            FarmObjectivesController.Instance?.NotificaMaialinoCatturato();
        }

        CreaEsplosioneRicompense();
        TextMeshPro testoBonus = CreaTestoBonus();

        Vector3 scalaBase = grafica != null
            ? grafica.localScale
            : Vector3.one;
        yield return AnimaScala(
            scalaBase,
            Vector3.Scale(scalaBase, new Vector3(1.22f, 0.72f, 1f)),
            0.07f
        );
        yield return AnimaScala(
            grafica.localScale,
            Vector3.Scale(scalaBase, new Vector3(0.78f, 1.24f, 1f)),
            0.07f
        );

        float tempo = 0f;
        const float durataRottura = 0.18f;
        Vector3 scalaIniziale = grafica != null
            ? grafica.localScale
            : Vector3.one;

        while (tempo < durataRottura)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / durataRottura);
            float curva = t * t;

            if (grafica != null)
            {
                grafica.localScale = Vector3.Lerp(
                    scalaIniziale,
                    Vector3.zero,
                    curva
                );
                grafica.localRotation = Quaternion.Euler(0f, 0f, t * 16f);
            }
            if (testoBonus != null)
            {
                testoBonus.transform.position +=
                    Vector3.up * Time.deltaTime * 0.9f;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator AnimaScala(Vector3 da, Vector3 a, float durata)
    {
        if (grafica == null) yield break;

        float tempo = 0f;
        while (tempo < durata)
        {
            tempo += Time.deltaTime;
            grafica.localScale = Vector3.Lerp(
                da,
                a,
                Mathf.Clamp01(tempo / durata)
            );
            yield return null;
        }
    }

    void CreaEsplosioneRicompense()
    {
        for (int i = 0; i < 7; i++)
        {
            CreaParticella(
                "CoccioRosa",
                OttieniSpriteFrammento(),
                new Color(1f, 0.35f, 0.48f, 1f),
                Random.Range(0.08f, 0.14f)
            );
        }

        for (int i = 0; i < 4; i++)
        {
            CreaParticella(
                "MonetaVFX",
                OttieniSpriteMoneta(),
                new Color(1f, 0.76f, 0.08f, 1f),
                Random.Range(0.13f, 0.18f)
            );
        }

        for (int i = 0; i < Mathf.Min(2, uovaRicompensa); i++)
        {
            CreaParticella(
                "UovoBonusVFX",
                FarmPixelUI.OttieniIcona(FarmPixelIcon.Uovo),
                new Color(1f, 0.92f, 0.67f, 1f),
                Random.Range(0.1f, 0.14f)
            );
        }
    }

    void CreaParticella(
        string nome,
        Sprite sprite,
        Color colore,
        float scala
    )
    {
        GameObject particella = new GameObject(nome);
        particella.transform.position =
            transform.position + (Vector3)(Random.insideUnitCircle * 0.18f);
        particella.transform.localScale = Vector3.one * scala;

        SpriteRenderer renderer = particella.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = colore;
        renderer.sortingLayerID = spriteRendererVisibile.sortingLayerID;
        renderer.sortingOrder = spriteRendererVisibile.sortingOrder + 5;

        Rigidbody2D fisica = particella.AddComponent<Rigidbody2D>();
        fisica.gravityScale = 1.25f;
        fisica.linearVelocity = new Vector2(
            Random.Range(-1.7f, 1.7f),
            Random.Range(1.5f, 2.8f)
        );
        fisica.angularVelocity = Random.Range(-320f, 320f);

        particella.AddComponent<AutoDistruzioneRealtime>();
    }

    TextMeshPro CreaTestoBonus()
    {
        GameObject oggettoTesto = new GameObject("TestoBonusMonete");
        oggettoTesto.transform.position = transform.position + Vector3.up * 0.48f;
        oggettoTesto.transform.localScale = Vector3.one * 0.16f;

        TextMeshPro testo = oggettoTesto.AddComponent<TextMeshPro>();
        testo.text =
            "+" + moneteRicompensa + " MONETE" +
            (uovaRicompensa > 0
                ? "  +" + uovaRicompensa + " UOVO"
                : string.Empty);
        testo.fontSize = 3.2f;
        testo.fontStyle = FontStyles.Bold;
        testo.alignment = TextAlignmentOptions.Center;
        testo.color = new Color(1f, 0.82f, 0.16f, 1f);
        testo.renderer.sortingLayerID = spriteRendererVisibile.sortingLayerID;
        testo.renderer.sortingOrder = spriteRendererVisibile.sortingOrder + 8;

        oggettoTesto.AddComponent<AutoDistruzioneRealtime>();
        return testo;
    }

    void RimuoviSenzaPremio()
    {
        if (morto) return;
        morto = true;
        colliderFisico.enabled = false;
        velocitaDesiderata = Vector2.zero;
        velocitaAttuale = Vector2.zero;
        NascondiBarraVita();
        StartCoroutine(SvanisciSenzaPremio());
    }

    IEnumerator SvanisciSenzaPremio()
    {
        if (grafica == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector3 scalaIniziale = grafica.localScale;
        float tempo = 0f;
        const float durata = 0.18f;
        while (tempo < durata)
        {
            tempo += Time.deltaTime;
            grafica.localScale = Vector3.Lerp(
                scalaIniziale,
                Vector3.zero,
                Mathf.Clamp01(tempo / durata)
            );
            yield return null;
        }
        Destroy(gameObject);
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

        float bordo = Mathf.Clamp(
            spessoreBordoBarra,
            0f,
            Mathf.Min(larghezzaBarraVita, altezzaBarraVita) * 0.45f
        );
        float larghezzaInterna = Mathf.Max(0.01f, larghezzaBarraVita - bordo * 2f);
        float altezzaInterna = Mathf.Max(0.01f, altezzaBarraVita - bordo * 2f);

        SpriteRenderer bordoRenderer = CreaElementoBarra(
            "Bordo",
            new Color(0.12f, 0.045f, 0.04f, 0.98f),
            sortingLayer,
            ordineBase
        );
        bordoRenderer.transform.localScale = new Vector3(
            larghezzaBarraVita,
            altezzaBarraVita,
            1f
        );

        SpriteRenderer sfondoRenderer = CreaElementoBarra(
            "Sfondo",
            new Color(0.24f, 0.1f, 0.09f, 0.95f),
            sortingLayer,
            ordineBase + 1
        );
        sfondoRenderer.transform.localScale = new Vector3(
            larghezzaInterna,
            altezzaInterna,
            1f
        );

        rendererRiempimentoBarra = CreaElementoBarra(
            "Riempimento",
            Color.green,
            sortingLayer,
            ordineBase + 2
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
        renderer.sprite = OttieniSpriteBarraVita();
        renderer.color = colore;
        renderer.sortingLayerID = sortingLayer;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    static Sprite OttieniSpriteBarraVita()
    {
        if (spriteBarraVita != null) return spriteBarraVita;

        Texture2D texture = Texture2D.whiteTexture;
        spriteBarraVita = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            Mathf.Max(1, texture.width),
            0,
            SpriteMeshType.FullRect
        );
        spriteBarraVita.name = "SpriteBarraVitaMaialino";
        return spriteBarraVita;
    }

    void AggiornaBarraVita()
    {
        if (riempimentoBarra == null || rendererRiempimentoBarra == null) return;

        float rapporto = vitaMassima > 0
            ? Mathf.Clamp01((float)vitaCorrente / vitaMassima)
            : 0f;
        float bordo = Mathf.Clamp(
            spessoreBordoBarra,
            0f,
            Mathf.Min(larghezzaBarraVita, altezzaBarraVita) * 0.45f
        );
        float larghezzaInterna = Mathf.Max(0.01f, larghezzaBarraVita - bordo * 2f);
        float altezzaInterna = Mathf.Max(0.01f, altezzaBarraVita - bordo * 2f);
        float larghezzaRiempimento = larghezzaInterna * rapporto;

        riempimentoBarra.localScale = new Vector3(
            larghezzaRiempimento,
            altezzaInterna,
            1f
        );
        riempimentoBarra.localPosition = new Vector3(
            (larghezzaRiempimento - larghezzaInterna) * 0.5f,
            0f,
            0f
        );

        Color rosso = new Color(0.92f, 0.12f, 0.08f, 1f);
        Color giallo = new Color(1f, 0.72f, 0.08f, 1f);
        Color verde = new Color(0.18f, 0.88f, 0.28f, 1f);
        rendererRiempimentoBarra.color = rapporto <= 0.5f
            ? Color.Lerp(rosso, giallo, rapporto * 2f)
            : Color.Lerp(giallo, verde, (rapporto - 0.5f) * 2f);
    }

    void NascondiBarraVita()
    {
        if (barraVita != null)
        {
            barraVita.gameObject.SetActive(false);
        }
    }

    SpriteRenderer CreaRendererVisivo(SpriteRenderer rendererOriginale)
    {
        GameObject oggettoGrafico = new GameObject("Grafica");
        oggettoGrafico.layer = gameObject.layer;
        grafica = oggettoGrafico.transform;
        grafica.SetParent(transform, false);

        SpriteRenderer nuovoRenderer =
            oggettoGrafico.AddComponent<SpriteRenderer>();
        nuovoRenderer.sprite = rendererOriginale.sprite;
        nuovoRenderer.color = rendererOriginale.color;
        nuovoRenderer.sortingLayerID = rendererOriginale.sortingLayerID;
        nuovoRenderer.sortingOrder = rendererOriginale.sortingOrder;
        nuovoRenderer.sharedMaterials = rendererOriginale.sharedMaterials;
        rendererOriginale.enabled = false;
        return nuovoRenderer;
    }

    static Sprite OttieniSpriteFrammento()
    {
        if (spriteFrammento != null) return spriteFrammento;
        spriteFrammento = CreaSpritePixel(false, "SpriteCoccio");
        return spriteFrammento;
    }

    static Sprite OttieniSpriteMoneta()
    {
        if (spriteMoneta != null) return spriteMoneta;
        spriteMoneta = CreaSpritePixel(true, "SpriteMonetaVFX");
        return spriteMoneta;
    }

    static Sprite CreaSpritePixel(bool rotondo, string nome)
    {
        const int dimensione = 8;
        Texture2D texture = new Texture2D(
            dimensione,
            dimensione,
            TextureFormat.RGBA32,
            false
        );
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixel = new Color[dimensione * dimensione];
        for (int y = 0; y < dimensione; y++)
        {
            for (int x = 0; x < dimensione; x++)
            {
                bool visibile = !rotondo ||
                    ((x - 3.5f) * (x - 3.5f) + (y - 3.5f) * (y - 3.5f) < 13f);
                pixel[y * dimensione + x] = visibile
                    ? Color.white
                    : Color.clear;
            }
        }
        texture.SetPixels(pixel);
        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, dimensione, dimensione),
            new Vector2(0.5f, 0.5f),
            dimensione,
            0,
            SpriteMeshType.FullRect
        );
        sprite.name = nome;
        return sprite;
    }

    public static void RimuoviTuttiSenzaPremio()
    {
        MaialinoBonus[] copia = new MaialinoBonus[attivi.Count];
        attivi.CopyTo(copia);
        foreach (MaialinoBonus maialino in copia)
        {
            if (maialino != null)
            {
                Destroy(maialino.gameObject);
            }
        }
        attivi.Clear();
    }

    void OnDisable()
    {
        attivi.Remove(this);
        if (spriteRendererVisibile != null)
        {
            spriteRendererVisibile.color = coloreBase;
        }
    }
}

public class AutoDistruzioneRealtime : MonoBehaviour
{
    [Min(0.01f)] public float durata = 0.75f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(durata);
        Destroy(gameObject);
    }
}
