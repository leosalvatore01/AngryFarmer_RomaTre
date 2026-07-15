using System.Collections.Generic;
using UnityEngine;

public class Proiettile : MonoBehaviour
{
    [Min(1)] public int danno = 1;
    [Min(0f)] public float rotazioneVisivaMinima = 100f;
    [Min(0f)] public float rotazioneVisivaMassima = 165f;

    private readonly HashSet<int> bersagliColpiti = new HashSet<int>();
    private int penetrazioniRimaste;
    private int penetrazioniIniziali;
    private Transform grafica;
    private SpriteRenderer rendererGrafica;
    private float velocitaRotazioneVisiva;
    private float durataVita = 3f;
    private bool colpoPotente;
    private bool aspettoPerforante;

    private bool consumato;

    public int PenetrazioniRimaste => penetrazioniRimaste;
    public int NumeroBersagliColpiti => bersagliColpiti.Count;
    public bool Consumato => consumato;
    public bool ColpoPotente => colpoPotente;
    public bool AspettoPerforante => aspettoPerforante;

    void Awake()
    {
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
        danno = Mathf.Max(1, nuovoDanno);
        penetrazioniRimaste = Mathf.Max(0, penetrazioni);
        penetrazioniIniziali = penetrazioniRimaste;
        colpoPotente = potente;
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
        int idBersaglio = componenteBersaglio != null
            ? componenteBersaglio.gameObject.GetInstanceID()
            : other.GetInstanceID();

        if (!bersagliColpiti.Add(idBersaglio)) return;

        Vector2 posizioneImpatto = other.ClosestPoint(transform.position);
        EsitoDanno esito = bersaglio.ProvaSubireDanno(danno);
        if (!esito.Applicato) return;

        bool puoPenetrare =
            esito.Ucciso &&
            esito.ConsentePenetrazioneAllaMorte &&
            penetrazioniRimaste > 0;

        if (puoPenetrare)
        {
            penetrazioniRimaste--;
        }
        else
        {
            consumato = true;
            Destroy(gameObject);
        }

        Vector2 direzioneColpo = OttieniDirezioneColpo();
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
                colpoPotente,
                puoPenetrare
            );
        }

        CombatFeedbackController.CreaOTrova().RegistraImpatto(
            posizioneImpatto,
            direzioneColpo,
            rendererBersaglio,
            colpoPotente,
            puoPenetrare
        );

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

        if (aspettoPerforante)
        {
            rendererGrafica.color = new Color(1f, 0.78f, 0.25f, 1f);
            rendererGrafica.transform.localScale = new Vector3(
                1.18f,
                0.86f,
                1f
            );
        }
        else if (colpoPotente)
        {
            rendererGrafica.color = new Color(1f, 0.9f, 0.62f, 1f);
            rendererGrafica.transform.localScale = Vector3.one * 1.12f;
        }
    }
}
