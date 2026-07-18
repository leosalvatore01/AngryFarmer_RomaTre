using System.Collections.Generic;
using UnityEngine;

public class Proiettile : MonoBehaviour
{
    private const int CapacitaQueryFisica = 64;
    private static readonly Collider2D[] bufferEsplosione =
        new Collider2D[CapacitaQueryFisica];
    private static readonly Collider2D[] bufferRimbalzo =
        new Collider2D[CapacitaQueryFisica];
    private static readonly ContactFilter2D filtroQuery =
        ContactFilter2D.noFilter;

    [Min(1)] public int danno = 1;
    [Min(0f)] public float rotazioneVisivaMinima = 100f;
    [Min(0f)] public float rotazioneVisivaMassima = 165f;

    private readonly HashSet<int> bersagliColpiti = new HashSet<int>();
    private int penetrazioniRimaste;
    private int penetrazioniIniziali;
    private int rimbalziRimasti;
    private float raggioRimbalzo;
    private float raggioEsplosione;
    private int dannoEsplosione;
    private float moltiplicatoreRallentamento = 1f;
    private float durataRallentamento;
    private float forzaSpinta;
    private float scalaBuild = 1f;
    private Vector3 scalaPrefab = Vector3.one;
    private Transform grafica;
    private SpriteRenderer rendererGrafica;
    private float velocitaRotazioneVisiva;
    private float durataVita = 3f;
    private bool colpoPotente;
    private bool aspettoPerforante;
    private bool colpoCritico;
    private bool esplosioneUsata;
    private bool consumato;

    public int PenetrazioniRimaste => penetrazioniRimaste;
    public int RimbalziRimasti => rimbalziRimasti;
    public int NumeroBersagliColpiti => bersagliColpiti.Count;
    public bool Consumato => consumato;
    public bool ColpoPotente => colpoPotente;
    public bool AspettoPerforante => aspettoPerforante;
    public bool ColpoCritico => colpoCritico;
    public bool EsplosioneUsata => esplosioneUsata;
    public float RaggioEsplosione => raggioEsplosione;
    public float ScalaBuild => scalaBuild;

    void Awake()
    {
        scalaPrefab = transform.localScale;
        PlayerBalanceSettings bilanciamento =
            GameBalanceConfig.Corrente.Giocatore;
        danno = Mathf.Max(1, bilanciamento.dannoProiettile);
        penetrazioniRimaste = Mathf.Max(
            0,
            bilanciamento.penetrazioneProiettile
        );
        penetrazioniIniziali = penetrazioniRimaste;
        durataVita = Mathf.Max(0.05f, bilanciamento.durataProiettile);

        ConfiguraGraficaRotante();

        float minimo = Mathf.Min(rotazioneVisivaMinima, rotazioneVisivaMassima);
        float massimo = Mathf.Max(rotazioneVisivaMinima, rotazioneVisivaMassima);
        velocitaRotazioneVisiva = Random.Range(minimo, massimo) *
                                  (Random.value < 0.5f ? -1f : 1f);
    }

    public void Inizializza(int nuovoDanno, int penetrazioni)
    {
        Inizializza(
            nuovoDanno,
            penetrazioni,
            false,
            penetrazioni > 0
        );
    }

    public void Inizializza(
        int nuovoDanno,
        int penetrazioni,
        bool potente,
        bool perforante
    )
    {
        ProfiloProiettileBuild profilo = new ProfiloProiettileBuild
        {
            Danno = nuovoDanno,
            Penetrazioni = penetrazioni,
            Scala = 1f,
            MoltiplicatoreVelocita = 1f
        };
        InizializzaBuild(profilo, potente, perforante);
    }

    public void InizializzaBuild(
        ProfiloProiettileBuild profilo,
        bool potente,
        bool perforante
    )
    {
        danno = Mathf.Max(1, profilo.Danno);
        penetrazioniRimaste = Mathf.Max(0, profilo.Penetrazioni);
        penetrazioniIniziali = penetrazioniRimaste;
        rimbalziRimasti = Mathf.Max(0, profilo.Rimbalzi);
        raggioRimbalzo = Mathf.Max(0.5f, profilo.RaggioRimbalzo);
        raggioEsplosione = Mathf.Max(0f, profilo.RaggioEsplosione);
        dannoEsplosione = Mathf.Max(0, profilo.DannoEsplosione);
        moltiplicatoreRallentamento = Mathf.Clamp(
            profilo.MoltiplicatoreRallentamento <= 0f
                ? 1f
                : profilo.MoltiplicatoreRallentamento,
            0.1f,
            1f
        );
        durataRallentamento = Mathf.Max(0f, profilo.DurataRallentamento);
        forzaSpinta = Mathf.Max(0f, profilo.ForzaSpinta);
        scalaBuild = Mathf.Max(0.25f, profilo.Scala);
        transform.localScale = new Vector3(
            scalaPrefab.x * scalaBuild,
            scalaPrefab.y * scalaBuild,
            scalaPrefab.z
        );
        colpoCritico = profilo.Critico;
        colpoPotente = potente || colpoCritico || scalaBuild > 1.01f;
        aspettoPerforante = perforante && penetrazioniIniziali > 0;
        AggiornaAspettoColpo();
    }

    void Start()
    {
        Destroy(gameObject, durataVita);
    }

    void Update()
    {
        if (grafica != null)
        {
            grafica.Rotate(
                0f,
                0f,
                velocitaRotazioneVisiva * Time.deltaTime,
                Space.Self
            );
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (consumato) return;

        IDanneggiabile bersaglio = other.GetComponentInParent<IDanneggiabile>();
        if (bersaglio == null) return;

        Component componenteBersaglio = bersaglio as Component;
        int idBersaglio = OttieniIdBersaglio(
            componenteBersaglio,
            other
        );
        if (!bersagliColpiti.Add(idBersaglio)) return;

        Vector2 posizioneImpatto = other.ClosestPoint(transform.position);
        EsitoDanno esito = bersaglio.ProvaSubireDanno(danno);
        if (!esito.Applicato) return;
        DamageNumberFeedback.Mostra(
            posizioneImpatto,
            esito.DannoApplicato > 0 ? esito.DannoApplicato : danno,
            colpoCritico
        );

        Vector2 direzioneColpo = OttieniDirezioneColpo();
        ApplicaControllo(
            componenteBersaglio,
            direzioneColpo
        );
        AttivaEsplosione(posizioneImpatto);

        bool puoPenetrare =
            esito.Ucciso &&
            esito.ConsentePenetrazioneAllaMorte &&
            penetrazioniRimaste > 0;
        bool haRimbalzato = false;

        if (puoPenetrare)
        {
            penetrazioniRimaste--;
        }
        else
        {
            haRimbalzato = ProvaRimbalzare(posizioneImpatto);
            if (!haRimbalzato)
            {
                consumato = true;
                Destroy(gameObject);
            }
        }

        CombatHitFeedback2D feedback = componenteBersaglio != null
            ? componenteBersaglio.GetComponent<CombatHitFeedback2D>()
            : null;
        SpriteRenderer rendererBersaglio = feedback != null
            ? feedback.RendererSorgente
            : componenteBersaglio != null
                ? componenteBersaglio.GetComponentInChildren<SpriteRenderer>()
                : null;

        if (feedback != null)
        {
            feedback.Riproduci(
                direzioneColpo,
                colpoPotente || colpoCritico,
                puoPenetrare || haRimbalzato
            );
        }

        CombatFeedbackController.CreaOTrova().RegistraImpatto(
            posizioneImpatto,
            direzioneColpo,
            rendererBersaglio,
            colpoPotente || colpoCritico,
            puoPenetrare || haRimbalzato
        );
    }

    private void ApplicaControllo(
        Component componenteBersaglio,
        Vector2 direzioneColpo
    )
    {
        if (componenteBersaglio == null) return;
        EnemyAI volpe = componenteBersaglio.GetComponent<EnemyAI>();
        if (volpe == null) return;

        if (durataRallentamento > 0f &&
            moltiplicatoreRallentamento < 1f)
        {
            volpe.ApplicaRallentamento(
                moltiplicatoreRallentamento,
                durataRallentamento
            );
        }
        if (forzaSpinta > 0f)
        {
            volpe.ApplicaSpinta(direzioneColpo, forzaSpinta);
        }
    }

    private void AttivaEsplosione(Vector2 posizione)
    {
        if (esplosioneUsata ||
            raggioEsplosione <= 0f ||
            dannoEsplosione <= 0)
        {
            return;
        }

        esplosioneUsata = true;
        int numeroColpiti = Physics2D.OverlapCircle(
            posizione,
            raggioEsplosione,
            filtroQuery,
            bufferEsplosione
        );
        for (int i = 0; i < numeroColpiti; i++)
        {
            Collider2D collider = bufferEsplosione[i];
            bufferEsplosione[i] = null;
            if (collider == null) continue;
            IDanneggiabile bersaglio =
                collider.GetComponentInParent<IDanneggiabile>();
            if (bersaglio == null) continue;

            Component componente = bersaglio as Component;
            int id = OttieniIdBersaglio(componente, collider);
            if (!bersagliColpiti.Add(id)) continue;
            EsitoDanno esito = bersaglio.ProvaSubireDanno(dannoEsplosione);
            if (esito.Applicato)
            {
                Vector2 posizioneNumero = componente != null
                    ? componente.transform.position
                    : collider.ClosestPoint(posizione);
                DamageNumberFeedback.Mostra(
                    posizioneNumero,
                    esito.DannoApplicato > 0
                        ? esito.DannoApplicato
                        : dannoEsplosione,
                    colpoCritico
                );
            }
        }
        BuildCombatVfx.CreaEsplosione(
            posizione,
            raggioEsplosione,
            colpoCritico
        );
    }

    private bool ProvaRimbalzare(Vector2 posizioneImpatto)
    {
        if (rimbalziRimasti <= 0) return false;

        int numeroColliders = Physics2D.OverlapCircle(
            posizioneImpatto,
            raggioRimbalzo,
            filtroQuery,
            bufferRimbalzo
        );
        EnemyAI migliore = null;
        float distanzaMigliore = raggioRimbalzo * raggioRimbalzo;
        for (int i = 0; i < numeroColliders; i++)
        {
            Collider2D collider = bufferRimbalzo[i];
            bufferRimbalzo[i] = null;
            if (collider == null) continue;
            EnemyAI volpe = collider.GetComponentInParent<EnemyAI>();
            if (volpe == null || volpe.IsDead) continue;
            int id = volpe.gameObject.GetInstanceID();
            if (bersagliColpiti.Contains(id)) continue;

            float distanza =
                ((Vector2)volpe.transform.position - posizioneImpatto)
                .sqrMagnitude;
            if (distanza > distanzaMigliore) continue;
            distanzaMigliore = distanza;
            migliore = volpe;
        }
        if (migliore == null) return false;

        Vector2 direzione =
            (Vector2)migliore.transform.position - posizioneImpatto;
        if (direzione.sqrMagnitude < 0.0001f) return false;
        direzione.Normalize();

        Rigidbody2D corpo = GetComponent<Rigidbody2D>();
        if (corpo == null) return false;
        float velocita = Mathf.Max(1f, corpo.linearVelocity.magnitude);
        corpo.linearVelocity = direzione * velocita;
        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            Mathf.Atan2(direzione.y, direzione.x) * Mathf.Rad2Deg
        );
        rimbalziRimasti--;
        return true;
    }

    void ConfiguraGraficaRotante()
    {
        SpriteRenderer rendererOriginale = GetComponent<SpriteRenderer>();
        if (rendererOriginale == null)
        {
            SpriteRenderer rendererFiglio =
                GetComponentInChildren<SpriteRenderer>();
            rendererGrafica = rendererFiglio;
            grafica = rendererFiglio != null
                ? rendererFiglio.transform
                : null;
            return;
        }

        GameObject oggettoGrafico = new GameObject("GraficaProiettile");
        oggettoGrafico.layer = gameObject.layer;
        grafica = oggettoGrafico.transform;
        grafica.SetParent(transform, false);

        SpriteRenderer renderer = oggettoGrafico.AddComponent<SpriteRenderer>();
        renderer.sprite = rendererOriginale.sprite;
        renderer.color = rendererOriginale.color;
        renderer.flipX = rendererOriginale.flipX;
        renderer.flipY = rendererOriginale.flipY;
        renderer.drawMode = rendererOriginale.drawMode;
        renderer.size = rendererOriginale.size;
        renderer.maskInteraction = rendererOriginale.maskInteraction;
        renderer.spriteSortPoint = rendererOriginale.spriteSortPoint;
        renderer.sortingLayerID = rendererOriginale.sortingLayerID;
        renderer.sortingOrder = rendererOriginale.sortingOrder;
        renderer.sharedMaterials = rendererOriginale.sharedMaterials;
        renderer.enabled = rendererOriginale.enabled;

        rendererOriginale.enabled = false;
        rendererGrafica = renderer;
    }

    private Vector2 OttieniDirezioneColpo()
    {
        Rigidbody2D corpo = GetComponent<Rigidbody2D>();
        if (corpo != null && corpo.linearVelocity.sqrMagnitude > 0.0001f)
        {
            return corpo.linearVelocity.normalized;
        }

        Vector2 direzione = transform.right;
        return direzione.sqrMagnitude > 0.0001f
            ? direzione.normalized
            : Vector2.right;
    }

    private void AggiornaAspettoColpo()
    {
        if (rendererGrafica == null) return;

        rendererGrafica.transform.localScale = Vector3.one;
        if (raggioEsplosione > 0f)
        {
            rendererGrafica.color = colpoCritico
                ? new Color(1f, 0.95f, 0.38f, 1f)
                : new Color(1f, 0.48f, 0.18f, 1f);
        }
        else if (colpoCritico)
        {
            rendererGrafica.color = new Color(1f, 0.96f, 0.42f, 1f);
            rendererGrafica.transform.localScale *= 1.12f;
        }
        else if (aspettoPerforante)
        {
            rendererGrafica.color = new Color(1f, 0.78f, 0.25f, 1f);
            rendererGrafica.transform.localScale = new Vector3(
                1.18f,
                0.86f,
                1f
            );
        }
        else if (durataRallentamento > 0f)
        {
            rendererGrafica.color = new Color(0.55f, 0.95f, 0.62f, 1f);
        }
        else if (colpoPotente)
        {
            rendererGrafica.color = new Color(1f, 0.9f, 0.62f, 1f);
        }
    }

    private static int OttieniIdBersaglio(
        Component componente,
        Collider2D collider
    )
    {
        return componente != null
            ? componente.gameObject.GetInstanceID()
            : collider.GetInstanceID();
    }

}

internal static class BuildCombatVfx
{
    private static readonly Queue<BuildExplosionBurst> pool =
        new Queue<BuildExplosionBurst>();

    public static void CreaEsplosione(
        Vector2 posizione,
        float raggio,
        bool critico
    )
    {
        BuildExplosionBurst effetto = null;
        while (pool.Count > 0 && effetto == null)
        {
            effetto = pool.Dequeue();
        }
        if (effetto == null)
        {
            GameObject oggetto = new GameObject("EsplosionePatataPixel");
            effetto = oggetto.AddComponent<BuildExplosionBurst>();
        }
        effetto.gameObject.SetActive(true);
        effetto.Attiva(posizione, raggio, critico);
    }

    public static void Rilascia(BuildExplosionBurst effetto)
    {
        if (effetto == null) return;
        effetto.gameObject.SetActive(false);
        pool.Enqueue(effetto);
    }
}

internal sealed class BuildExplosionBurst : MonoBehaviour
{
    private const int NumeroParticelle = 14;
    private static Sprite spritePixel;

    private readonly SpriteRenderer[] particelle =
        new SpriteRenderer[NumeroParticelle];
    private readonly Vector2[] direzioni =
        new Vector2[NumeroParticelle];
    private float raggio;
    private float tempo;
    private bool critico;

    void Awake()
    {
        AssicuraParticelle();
    }

    public void Attiva(Vector2 posizione, float nuovoRaggio, bool nuovoCritico)
    {
        AssicuraParticelle();
        transform.position = posizione;
        raggio = Mathf.Max(0.2f, nuovoRaggio);
        critico = nuovoCritico;
        tempo = 0f;
        Aggiorna(0f);
    }

    private void AssicuraParticelle()
    {
        for (int i = 0; i < NumeroParticelle; i++)
        {
            if (particelle[i] != null) continue;

            GameObject pixel = new GameObject("Scoppio_" + i);
            pixel.transform.SetParent(transform, false);
            SpriteRenderer renderer = pixel.AddComponent<SpriteRenderer>();
            renderer.sprite = OttieniSpritePixel();
            renderer.sortingOrder = 30;
            particelle[i] = renderer;

            float angolo = i * (360f / NumeroParticelle) * Mathf.Deg2Rad;
            direzioni[i] = new Vector2(
                Mathf.Cos(angolo),
                Mathf.Sin(angolo)
            );
        }
    }

    void Update()
    {
        tempo += Time.deltaTime;
        Aggiorna(Mathf.Clamp01(tempo / 0.28f));
        if (tempo >= 0.28f)
        {
            BuildCombatVfx.Rilascia(this);
        }
    }

    private void Aggiorna(float t)
    {
        float distanza = Mathf.Lerp(0.08f, raggio, t);
        float alpha = 1f - t;
        for (int i = 0; i < particelle.Length; i++)
        {
            SpriteRenderer renderer = particelle[i];
            if (renderer == null) continue;
            renderer.transform.localPosition =
                direzioni[i] * distanza *
                (i % 2 == 0 ? 1f : 0.72f);
            renderer.transform.localScale =
                Vector3.one * Mathf.Lerp(0.16f, 0.05f, t);
            Color colore = critico
                ? new Color(1f, 0.94f, 0.32f, alpha)
                : i % 3 == 0
                    ? new Color(1f, 0.34f, 0.12f, alpha)
                    : new Color(1f, 0.72f, 0.2f, alpha);
            renderer.color = colore;
        }
    }

    private static Sprite OttieniSpritePixel()
    {
        if (spritePixel != null) return spritePixel;
        Texture2D texture = Texture2D.whiteTexture;
        spritePixel = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            Mathf.Max(1, texture.width)
        );
        spritePixel.name = "PixelEsplosioneBuild";
        return spritePixel;
    }
}
