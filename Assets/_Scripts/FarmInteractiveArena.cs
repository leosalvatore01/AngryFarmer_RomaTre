using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TipoElementoFattoria
{
    Fango,
    BallaPaglia,
    ZuccaEsplosiva,
    CassaMonete,
    CassaCura
}

public enum TipoRicompensaCassa
{
    Monete,
    Cura
}

public readonly struct ZonaProtettaFattoria
{
    public Vector2 Posizione { get; }
    public float Raggio { get; }

    public ZonaProtettaFattoria(Vector2 posizione, float raggio)
    {
        Posizione = posizione;
        Raggio = Mathf.Max(0f, raggio);
    }
}

public readonly struct ElementoFattoriaLayout
{
    public TipoElementoFattoria Tipo { get; }
    public Vector2 Posizione { get; }
    public float Rotazione { get; }
    public bool Specchiato { get; }

    public ElementoFattoriaLayout(
        TipoElementoFattoria tipo,
        Vector2 posizione,
        float rotazione,
        bool specchiato
    )
    {
        Tipo = tipo;
        Posizione = posizione;
        Rotazione = rotazione;
        Specchiato = specchiato;
    }
}

public static class GeneratoreLayoutFattoria
{
    private const float AngoloAureo = 2.39996323f;

    public static List<ElementoFattoriaLayout> Genera(
        FarmInteractiveBalanceSettings configurazione,
        Vector2 centro,
        IReadOnlyList<ZonaProtettaFattoria> zoneProtette
    )
    {
        List<ElementoFattoriaLayout> risultato =
            new List<ElementoFattoriaLayout>();
        if (configurazione == null) return risultato;

        List<TipoElementoFattoria> tipi = CreaListaTipi(configurazione);
        System.Random casualita =
            new System.Random(configurazione.seedArena);
        Mescola(tipi, casualita);

        float raggioMinimo = Mathf.Max(
            2f,
            configurazione.raggioMinimoArena
        );
        float raggioMassimo = Mathf.Max(
            raggioMinimo + 0.5f,
            configurazione.raggioMassimoArena
        );
        float distanzaBase = Mathf.Max(
            0.5f,
            configurazione.distanzaMinimaElementi
        );
        float scalaVerticale = Mathf.Clamp(
            configurazione.scalaVerticaleArena,
            0.4f,
            1f
        );
        float offsetAngolare = Intervallo(
            casualita,
            0f,
            Mathf.PI * 2f
        );

        for (int indice = 0; indice < tipi.Count; indice++)
        {
            bool inserito = false;
            TipoElementoFattoria tipo = tipi[indice];
            for (int tentativo = 0; tentativo < 360; tentativo++)
            {
                float angolo =
                    offsetAngolare +
                    indice * AngoloAureo +
                    tentativo * AngoloAureo * 0.37f +
                    Intervallo(casualita, -0.16f, 0.16f);
                float raggio = Intervallo(
                    casualita,
                    raggioMinimo,
                    raggioMassimo
                );
                Vector2 posizione = centro + new Vector2(
                    Mathf.Cos(angolo) * raggio,
                    Mathf.Sin(angolo) * raggio * scalaVerticale
                );
                if (!PosizioneValida(
                        posizione,
                        tipo,
                        risultato,
                        zoneProtette,
                        configurazione,
                        distanzaBase))
                {
                    continue;
                }

                risultato.Add(CreaElemento(
                    tipo,
                    posizione,
                    casualita
                ));
                inserito = true;
                break;
            }

            if (!inserito)
            {
                for (int tentativo = 0; tentativo < 144; tentativo++)
                {
                    float angolo =
                        offsetAngolare +
                        (tentativo / 144f) * Mathf.PI * 2f;
                    Vector2 posizione = centro + new Vector2(
                        Mathf.Cos(angolo) * raggioMassimo,
                        Mathf.Sin(angolo) *
                        raggioMassimo *
                        scalaVerticale
                    );
                    if (!PosizioneValida(
                            posizione,
                            tipo,
                            risultato,
                            zoneProtette,
                            configurazione,
                            distanzaBase))
                    {
                        continue;
                    }
                    risultato.Add(CreaElemento(
                        tipo,
                        posizione,
                        casualita
                    ));
                    inserito = true;
                    break;
                }
            }
        }

        return risultato;
    }

    private static List<TipoElementoFattoria> CreaListaTipi(
        FarmInteractiveBalanceSettings configurazione
    )
    {
        List<TipoElementoFattoria> tipi =
            new List<TipoElementoFattoria>();
        Aggiungi(
            tipi,
            TipoElementoFattoria.Fango,
            configurazione.numeroZoneFango
        );
        Aggiungi(
            tipi,
            TipoElementoFattoria.BallaPaglia,
            configurazione.numeroBallePaglia
        );
        Aggiungi(
            tipi,
            TipoElementoFattoria.ZuccaEsplosiva,
            configurazione.numeroZuccheEsplosive
        );
        Aggiungi(
            tipi,
            TipoElementoFattoria.CassaMonete,
            configurazione.numeroCasseMonete
        );
        Aggiungi(
            tipi,
            TipoElementoFattoria.CassaCura,
            configurazione.numeroCasseCura
        );
        return tipi;
    }

    private static void Aggiungi(
        List<TipoElementoFattoria> tipi,
        TipoElementoFattoria tipo,
        int quantita
    )
    {
        for (int i = 0; i < Mathf.Max(0, quantita); i++)
        {
            tipi.Add(tipo);
        }
    }

    private static void Mescola<T>(List<T> elementi, System.Random casualita)
    {
        for (int i = elementi.Count - 1; i > 0; i--)
        {
            int j = casualita.Next(i + 1);
            T temporaneo = elementi[i];
            elementi[i] = elementi[j];
            elementi[j] = temporaneo;
        }
    }

    private static bool PosizioneValida(
        Vector2 posizione,
        TipoElementoFattoria tipo,
        List<ElementoFattoriaLayout> esistenti,
        IReadOnlyList<ZonaProtettaFattoria> zoneProtette,
        FarmInteractiveBalanceSettings configurazione,
        float distanzaMinima
    )
    {
        float ingombro = RaggioIngombro(tipo, configurazione);
        for (int i = 0; i < esistenti.Count; i++)
        {
            float separazione = Mathf.Max(
                distanzaMinima,
                ingombro +
                RaggioIngombro(esistenti[i].Tipo, configurazione) +
                0.2f
            );
            if ((esistenti[i].Posizione - posizione).sqrMagnitude <
                separazione * separazione)
            {
                return false;
            }
        }

        if (zoneProtette == null) return true;
        for (int i = 0; i < zoneProtette.Count; i++)
        {
            ZonaProtettaFattoria zona = zoneProtette[i];
            float raggioLibero = zona.Raggio + ingombro;
            if ((zona.Posizione - posizione).sqrMagnitude <
                raggioLibero * raggioLibero)
            {
                return false;
            }
        }
        return true;
    }

    private static float RaggioIngombro(
        TipoElementoFattoria tipo,
        FarmInteractiveBalanceSettings configurazione
    )
    {
        switch (tipo)
        {
            case TipoElementoFattoria.Fango:
                return Mathf.Max(
                    0.4f,
                    configurazione.raggioFango * 1.15f
                );
            case TipoElementoFattoria.BallaPaglia:
                return 0.68f;
            case TipoElementoFattoria.ZuccaEsplosiva:
                return 0.66f;
            default:
                return 0.58f;
        }
    }

    private static ElementoFattoriaLayout CreaElemento(
        TipoElementoFattoria tipo,
        Vector2 posizione,
        System.Random casualita
    )
    {
        float rotazione = tipo == TipoElementoFattoria.Fango
            ? casualita.Next(4) * 90f
            : Intervallo(casualita, -5f, 5f);
        return new ElementoFattoriaLayout(
            tipo,
            posizione,
            rotazione,
            casualita.Next(2) == 0
        );
    }

    private static float Intervallo(
        System.Random casualita,
        float minimo,
        float massimo
    )
    {
        return Mathf.Lerp(
            minimo,
            massimo,
            (float)casualita.NextDouble()
        );
    }
}

[DisallowMultipleComponent]
public sealed class FarmInteractiveArena : MonoBehaviour
{
    private static FarmInteractiveArena instance;

    private readonly List<ElementoFattoriaLayout> layout =
        new List<ElementoFattoriaLayout>();
    private readonly List<GameObject> oggettiCreati =
        new List<GameObject>();

    private FarmInteractiveBalanceSettings configurazione;
    private FarmBackgroundDecorator decoratore;
    private bool costruita;

    public static FarmInteractiveArena Instance => instance;
    public IReadOnlyList<ElementoFattoriaLayout> Layout => layout;
    public int Seed => configurazione != null
        ? configurazione.seedArena
        : 0;
    public int NumeroElementi => layout.Count;
    public int OggettiDistrutti { get; private set; }
    public int ZuccheDetonate { get; private set; }
    public int CasseAperte { get; private set; }

    public static FarmInteractiveArena CreaOTrova()
    {
        // La modalita survival non crea oggetti interattivi nell'arena.
        if (instance != null)
        {
            instance.gameObject.SetActive(false);
            Destroy(instance.gameObject);
            instance = null;
        }
        return null;
    }

    void Awake()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    void Start()
    {
        // Disabilitato nel survival puro.
    }

    public void Costruisci()
    {
        if (costruita || instance != this) return;

        configurazione = GameBalanceConfig.Corrente.Fattoria;
        decoratore = FindFirstObjectByType<FarmBackgroundDecorator>();

        GameObject giocatore = GameObject.FindGameObjectWithTag("Player");
        if (configurazione == null ||
            decoratore == null ||
            giocatore == null)
        {
            return;
        }
        costruita = true;

        Vector2 posizioneGiocatore = giocatore != null
            ? giocatore.transform.position
            : Vector2.zero;

        List<ZonaProtettaFattoria> zoneProtette =
            new List<ZonaProtettaFattoria>
            {
                new ZonaProtettaFattoria(
                    posizioneGiocatore,
                    configurazione.raggioLiberoGiocatore
                )
            };
        Vector2 centro = Vector2.zero;
        int numeroGalline = 0;
        foreach (Gallina gallina in Gallina.Attive)
        {
            if (gallina == null) continue;
            Vector2 posizione = gallina.transform.position;
            centro += posizione;
            numeroGalline++;
            zoneProtette.Add(new ZonaProtettaFattoria(
                posizione,
                configurazione.raggioLiberoGalline
            ));
        }
        if (numeroGalline > 0)
        {
            centro /= numeroGalline;
        }

        List<ElementoFattoriaLayout> generato =
            GeneratoreLayoutFattoria.Genera(
                configurazione,
                centro,
                zoneProtette
            );
        layout.AddRange(generato);
        for (int i = 0; i < layout.Count; i++)
        {
            CreaElemento(layout[i], i);
        }

        if (configurazione.durataSuggerimentoIniziale > 0f)
        {
            FarmInteractiveHint.Crea(
                configurazione.durataSuggerimentoIniziale
            );
        }
    }

    public int Conta(TipoElementoFattoria tipo)
    {
        int totale = 0;
        for (int i = 0; i < layout.Count; i++)
        {
            if (layout[i].Tipo == tipo) totale++;
        }
        return totale;
    }

    internal void NotificaDistruzione()
    {
        OggettiDistrutti++;
    }

    internal void NotificaZuccaDetonata()
    {
        ZuccheDetonate++;
    }

    internal void NotificaCassaAperta()
    {
        CasseAperte++;
    }

    private void CreaElemento(ElementoFattoriaLayout elemento, int indice)
    {
        GameObject oggetto;
        switch (elemento.Tipo)
        {
            case TipoElementoFattoria.Fango:
                oggetto = CreaFango(elemento, indice);
                break;
            case TipoElementoFattoria.BallaPaglia:
                oggetto = CreaBallaPaglia(elemento, indice);
                break;
            case TipoElementoFattoria.ZuccaEsplosiva:
                oggetto = CreaZucca(elemento, indice);
                break;
            case TipoElementoFattoria.CassaCura:
                oggetto = CreaCassa(
                    elemento,
                    indice,
                    TipoRicompensaCassa.Cura
                );
                break;
            default:
                oggetto = CreaCassa(
                    elemento,
                    indice,
                    TipoRicompensaCassa.Monete
                );
                break;
        }

        if (oggetto != null)
        {
            oggetto.transform.SetParent(transform, true);
            oggettiCreati.Add(oggetto);
        }
    }

    private GameObject CreaFango(
        ElementoFattoriaLayout elemento,
        int indice
    )
    {
        GameObject radice = new GameObject("Fango_" + indice);
        radice.transform.position = elemento.Posizione;

        PolygonCollider2D collider =
            radice.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
        ConfiguraColliderFango(
            collider,
            configurazione.raggioFango * 1.15f,
            configurazione.raggioFango * 0.68f,
            elemento.Rotazione
        );

        SpriteRenderer renderer = CreaVisuale(
            radice.transform,
            decoratore != null
                ? decoratore.SpriteFangoInterattivo
                : null,
            new Vector2(1.8f, 1.7f),
            elemento.Rotazione,
            elemento.Specchiato,
            -20,
            new Color(0.48f, 0.33f, 0.18f, 0.9f),
            false
        );
        FarmInteractiveMarker marker = CreaMarcatore(
            radice.transform,
            FarmInteractiveArt.FrecciaFango,
            new Color(0.9f, 0.72f, 0.34f, 0.92f),
            new Vector2(0f, 0.38f)
        );

        FarmMudPatch fango = radice.AddComponent<FarmMudPatch>();
        fango.Configura(
            renderer,
            marker,
            configurazione.velocitaGiocatoreNelFango,
            configurazione.velocitaVolpiNelFango
        );
        return radice;
    }

    private static void ConfiguraColliderFango(
        PolygonCollider2D collider,
        float raggioOrizzontale,
        float raggioVerticale,
        float rotazione
    )
    {
        const int numeroPunti = 16;
        Vector2[] punti = new Vector2[numeroPunti];
        float angoloRotazione = rotazione * Mathf.Deg2Rad;
        float cosenoRotazione = Mathf.Cos(angoloRotazione);
        float senoRotazione = Mathf.Sin(angoloRotazione);
        for (int i = 0; i < numeroPunti; i++)
        {
            float angolo = i * Mathf.PI * 2f / numeroPunti;
            Vector2 punto = new Vector2(
                Mathf.Cos(angolo) * Mathf.Max(0.2f, raggioOrizzontale),
                Mathf.Sin(angolo) * Mathf.Max(0.2f, raggioVerticale)
            );
            punti[i] = new Vector2(
                punto.x * cosenoRotazione - punto.y * senoRotazione,
                punto.x * senoRotazione + punto.y * cosenoRotazione
            );
        }
        collider.points = punti;
    }

    private GameObject CreaBallaPaglia(
        ElementoFattoriaLayout elemento,
        int indice
    )
    {
        GameObject radice = CreaRadiceProp(
            "BallaPaglia_" + indice,
            elemento.Posizione,
            new Vector2(0.95f, 0.7f)
        );
        SpriteRenderer renderer = CreaVisuale(
            radice.transform,
            decoratore != null
                ? decoratore.SpritePagliaInterattiva
                : null,
            new Vector2(1.18f, 1.18f),
            elemento.Rotazione,
            elemento.Specchiato,
            -3,
            Color.white,
            true
        );
        FarmInteractiveMarker marker = CreaMarcatore(
            radice.transform,
            FarmInteractiveArt.Mirino,
            new Color(1f, 0.86f, 0.36f, 1f),
            new Vector2(0f, 0.78f)
        );
        FarmHayBale balla = radice.AddComponent<FarmHayBale>();
        balla.ConfiguraBase(
            renderer,
            radice.GetComponent<Collider2D>(),
            marker,
            configurazione.vitaBallaPaglia,
            this
        );
        return radice;
    }

    private GameObject CreaZucca(
        ElementoFattoriaLayout elemento,
        int indice
    )
    {
        GameObject radice = CreaRadiceProp(
            "ZuccaEsplosiva_" + indice,
            elemento.Posizione,
            new Vector2(1.08f, 0.62f)
        );
        SpriteRenderer renderer = CreaVisuale(
            radice.transform,
            decoratore != null
                ? decoratore.SpriteZuccheInterattive
                : null,
            new Vector2(1.2f, 1.2f),
            elemento.Rotazione,
            elemento.Specchiato,
            -3,
            new Color(1f, 0.93f, 0.82f, 1f),
            true
        );
        FarmInteractiveMarker marker = CreaMarcatore(
            radice.transform,
            FarmInteractiveArt.Pericolo,
            new Color(1f, 0.43f, 0.14f, 1f),
            new Vector2(0f, 0.72f)
        );
        FarmExplosivePumpkin zucca =
            radice.AddComponent<FarmExplosivePumpkin>();
        zucca.ConfiguraBase(
            renderer,
            radice.GetComponent<Collider2D>(),
            marker,
            configurazione.vitaZucca,
            this
        );
        zucca.ConfiguraEsplosione(
            configurazione.raggioEsplosioneZucca,
            configurazione.dannoEsplosioneZucca,
            configurazione.spintaEsplosioneZucca
        );
        return radice;
    }

    private GameObject CreaCassa(
        ElementoFattoriaLayout elemento,
        int indice,
        TipoRicompensaCassa tipoRicompensa
    )
    {
        string nome = tipoRicompensa == TipoRicompensaCassa.Monete
            ? "CassaMonete_"
            : "CassaCura_";
        GameObject radice = CreaRadiceProp(
            nome + indice,
            elemento.Posizione,
            new Vector2(0.84f, 0.72f)
        );
        SpriteRenderer renderer = CreaVisuale(
            radice.transform,
            decoratore != null
                ? decoratore.SpriteCassaInterattiva
                : null,
            new Vector2(1.12f, 1.12f),
            elemento.Rotazione,
            elemento.Specchiato,
            -3,
            tipoRicompensa == TipoRicompensaCassa.Monete
                ? new Color(1f, 0.9f, 0.65f, 1f)
                : new Color(0.78f, 1f, 0.75f, 1f),
            true
        );
        FarmInteractiveMarker marker = CreaMarcatore(
            radice.transform,
            tipoRicompensa == TipoRicompensaCassa.Monete
                ? FarmInteractiveArt.Moneta
                : FarmInteractiveArt.Cuore,
            tipoRicompensa == TipoRicompensaCassa.Monete
                ? new Color(1f, 0.84f, 0.2f, 1f)
                : new Color(1f, 0.96f, 0.96f, 1f),
            new Vector2(0f, 0.8f)
        );
        FarmRewardCrate cassa = radice.AddComponent<FarmRewardCrate>();
        cassa.ConfiguraBase(
            renderer,
            radice.GetComponent<Collider2D>(),
            marker,
            configurazione.vitaCassa,
            this
        );
        cassa.ConfiguraRicompensa(
            tipoRicompensa,
            tipoRicompensa == TipoRicompensaCassa.Monete
                ? configurazione.moneteCassa
                : configurazione.curaCassa
        );
        return radice;
    }

    private static GameObject CreaRadiceProp(
        string nome,
        Vector2 posizione,
        Vector2 dimensioniCollider
    )
    {
        GameObject radice = new GameObject(nome);
        radice.transform.position = posizione;
        BoxCollider2D collider = radice.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = dimensioniCollider;
        return radice;
    }

    private static SpriteRenderer CreaVisuale(
        Transform parent,
        Sprite sprite,
        Vector2 scala,
        float rotazione,
        bool specchiato,
        int ordine,
        Color tinta,
        bool creaContorno
    )
    {
        Vector3 scalaVisuale = new Vector3(
            scala.x * (specchiato ? -1f : 1f),
            scala.y,
            1f
        );
        if (creaContorno)
        {
            GameObject contorno = new GameObject("ContornoInterattivo");
            contorno.transform.SetParent(parent, false);
            contorno.transform.localRotation =
                Quaternion.Euler(0f, 0f, rotazione);
            contorno.transform.localScale = scalaVisuale * 1.12f;
            SpriteRenderer rendererContorno =
                contorno.AddComponent<SpriteRenderer>();
            rendererContorno.sprite = sprite;
            rendererContorno.color =
                new Color(0.12f, 0.055f, 0.025f, 0.78f);
            rendererContorno.sortingOrder = ordine - 1;
        }

        GameObject visuale = new GameObject("VisualeInterattiva");
        visuale.transform.SetParent(parent, false);
        visuale.transform.localRotation =
            Quaternion.Euler(0f, 0f, rotazione);
        visuale.transform.localScale = scalaVisuale;
        SpriteRenderer renderer = visuale.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = tinta;
        renderer.sortingOrder = ordine;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        return renderer;
    }

    private static FarmInteractiveMarker CreaMarcatore(
        Transform parent,
        Sprite sprite,
        Color colore,
        Vector2 posizione
    )
    {
        GameObject oggetto = new GameObject("MarcatoreInterattivo");
        oggetto.transform.SetParent(parent, false);
        SpriteRenderer renderer = oggetto.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = colore;
        renderer.sortingOrder = 12;
        FarmInteractiveMarker marker =
            oggetto.AddComponent<FarmInteractiveMarker>();
        marker.Configura(posizione, colore);
        return marker;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}

[DisallowMultipleComponent]
public sealed class FarmMudPatch : MonoBehaviour
{
    private SpriteRenderer rendererFango;
    private FarmInteractiveMarker marker;
    private float moltiplicatoreGiocatore;
    private float moltiplicatoreVolpi;
    private Color coloreBase;
    private float fase;

    public float MoltiplicatoreGiocatore => moltiplicatoreGiocatore;
    public float MoltiplicatoreVolpi => moltiplicatoreVolpi;

    public void Configura(
        SpriteRenderer renderer,
        FarmInteractiveMarker nuovoMarker,
        float rallentamentoGiocatore,
        float rallentamentoVolpi
    )
    {
        rendererFango = renderer;
        marker = nuovoMarker;
        moltiplicatoreGiocatore = Mathf.Clamp(
            rallentamentoGiocatore,
            0.2f,
            1f
        );
        moltiplicatoreVolpi = Mathf.Clamp(
            rallentamentoVolpi,
            0.2f,
            1f
        );
        coloreBase = rendererFango != null
            ? rendererFango.color
            : Color.white;
        fase = Mathf.Repeat(
            transform.position.x * 0.71f +
            transform.position.y * 0.43f,
            Mathf.PI * 2f
        );
    }

    void Update()
    {
        if (rendererFango == null) return;
        float onda = 0.5f + 0.5f * Mathf.Sin(
            Time.time * 1.8f + fase
        );
        Color colore = coloreBase;
        colore.a = Mathf.Lerp(0.72f, 0.92f, onda);
        rendererFango.color = colore;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        ApplicaA(other);
    }

    public void ApplicaA(Collider2D other)
    {
        if (other == null) return;

        PlayerMovement giocatore =
            other.GetComponentInParent<PlayerMovement>();
        if (giocatore != null)
        {
            giocatore.ApplicaRallentamentoTerreno(
                moltiplicatoreGiocatore,
                0.14f
            );
            if (marker != null) marker.Evidenzia();
            return;
        }

        EnemyAI volpe = other.GetComponentInParent<EnemyAI>();
        if (volpe != null)
        {
            volpe.ApplicaRallentamentoTerreno(
                moltiplicatoreVolpi,
                0.14f
            );
            if (marker != null) marker.Evidenzia();
        }
    }
}

public abstract class FarmInteractiveProp : MonoBehaviour, IDanneggiabile
{
    private SpriteRenderer rendererProp;
    private Collider2D colliderProp;
    private FarmInteractiveMarker marker;
    private Color coloreBase;
    private int vitaMassima;
    private int vitaCorrente;
    private bool distrutto;
    private Coroutine flashRoutine;

    protected FarmInteractiveArena Arena { get; private set; }
    protected SpriteRenderer RendererProp => rendererProp;
    protected FarmInteractiveMarker Marker => marker;

    public int VitaMassima => vitaMassima;
    public int VitaCorrente => vitaCorrente;
    public bool Distrutto => distrutto;

    public void ConfiguraBase(
        SpriteRenderer renderer,
        Collider2D collider,
        FarmInteractiveMarker nuovoMarker,
        int vita,
        FarmInteractiveArena arena
    )
    {
        rendererProp = renderer;
        colliderProp = collider;
        marker = nuovoMarker;
        vitaMassima = Mathf.Max(1, vita);
        vitaCorrente = vitaMassima;
        Arena = arena;
        coloreBase = rendererProp != null
            ? rendererProp.color
            : Color.white;
    }

    public EsitoDanno ProvaSubireDanno(int quantita)
    {
        if (distrutto || quantita <= 0 || !PuoRicevereDanno())
        {
            if (marker != null) marker.Evidenzia();
            return EsitoDanno.NessunDanno;
        }

        int vitaPrecedente = vitaCorrente;
        vitaCorrente = Mathf.Max(0, vitaCorrente - quantita);
        bool eliminato = vitaCorrente <= 0;
        if (eliminato)
        {
            Distruggi();
        }
        else
        {
            bool flashConsentito =
                GameOptionsController.Instance == null ||
                GameOptionsController.Instance.FlashAttivi;
            if (flashConsentito)
            {
                if (flashRoutine != null) StopCoroutine(flashRoutine);
                flashRoutine = StartCoroutine(FlashDanno());
            }
        }
        return new EsitoDanno(
            true,
            eliminato,
            eliminato,
            vitaPrecedente - vitaCorrente
        );
    }

    protected virtual bool PuoRicevereDanno()
    {
        return true;
    }

    protected abstract void QuandoDistrutto();

    protected void CreaScoppio(
        Color colorePrincipale,
        Color coloreSecondario,
        float raggio,
        int particelle
    )
    {
        FarmPixelBurst.Crea(
            transform.position,
            colorePrincipale,
            coloreSecondario,
            raggio,
            particelle
        );
    }

    private void Distruggi()
    {
        if (distrutto) return;
        distrutto = true;
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        if (colliderProp != null) colliderProp.enabled = false;
        if (marker != null) marker.gameObject.SetActive(false);
        Arena?.NotificaDistruzione();
        QuandoDistrutto();
        StartCoroutine(Dissolvi());
    }

    private IEnumerator FlashDanno()
    {
        if (rendererProp == null) yield break;
        rendererProp.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        if (!distrutto && rendererProp != null)
        {
            rendererProp.color = coloreBase;
        }
        flashRoutine = null;
    }

    private IEnumerator Dissolvi()
    {
        float tempo = 0f;
        const float durata = 0.2f;
        Vector3 scalaIniziale = transform.localScale;
        Color coloreIniziale = rendererProp != null
            ? rendererProp.color
            : Color.white;
        while (tempo < durata)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / durata);
            transform.localScale =
                scalaIniziale * Mathf.Lerp(1f, 0.55f, t);
            if (rendererProp != null)
            {
                Color colore = coloreIniziale;
                colore.a = 1f - t;
                rendererProp.color = colore;
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}

public sealed class FarmHayBale : FarmInteractiveProp
{
    protected override void QuandoDistrutto()
    {
        CreaScoppio(
            new Color(1f, 0.76f, 0.2f, 1f),
            new Color(0.65f, 0.31f, 0.08f, 1f),
            0.9f,
            11
        );
    }
}

public sealed class FarmExplosivePumpkin : FarmInteractiveProp
{
    private const int CapacitaEsplosione = 48;
    private static readonly Collider2D[] buffer =
        new Collider2D[CapacitaEsplosione];
    private static readonly HashSet<int> colpiti =
        new HashSet<int>();

    private float raggioEsplosione;
    private int dannoEsplosione;
    private float forzaSpinta;

    public float RaggioEsplosione => raggioEsplosione;
    public int DannoEsplosione => dannoEsplosione;

    public void ConfiguraEsplosione(
        float raggio,
        int danno,
        float spinta
    )
    {
        raggioEsplosione = Mathf.Max(0.5f, raggio);
        dannoEsplosione = Mathf.Max(1, danno);
        forzaSpinta = Mathf.Max(0f, spinta);
    }

    protected override void QuandoDistrutto()
    {
        Arena?.NotificaZuccaDetonata();
        colpiti.Clear();
        int quantita = Physics2D.OverlapCircle(
            transform.position,
            raggioEsplosione,
            ContactFilter2D.noFilter,
            buffer
        );
        for (int i = 0; i < quantita; i++)
        {
            Collider2D collider = buffer[i];
            buffer[i] = null;
            if (collider == null) continue;
            EnemyAI volpe = collider.GetComponentInParent<EnemyAI>();
            if (volpe == null || volpe.IsDead) continue;
            int id = volpe.gameObject.GetInstanceID();
            if (!colpiti.Add(id)) continue;

            Vector2 direzione =
                (Vector2)volpe.transform.position -
                (Vector2)transform.position;
            if (direzione.sqrMagnitude < 0.001f)
            {
                direzione = Vector2.up;
            }
            volpe.ProvaSubireDanno(dannoEsplosione);
            volpe.ApplicaSpinta(direzione.normalized, forzaSpinta);
        }
        colpiti.Clear();

        CreaScoppio(
            new Color(1f, 0.34f, 0.08f, 1f),
            new Color(1f, 0.82f, 0.16f, 1f),
            raggioEsplosione,
            18
        );
        Camera camera = Camera.main;
        CameraFollow follow = camera != null
            ? camera.GetComponent<CameraFollow>()
            : null;
        if (follow != null)
        {
            CombatFeedbackSettings feedback =
                GameBalanceConfig.Corrente.FeedbackCombattimento;
            follow.RichiediVibrazione(
                feedback.intensitaVibrazione * 1.35f,
                feedback.durataVibrazione * 1.25f
            );
        }
    }
}

public sealed class FarmRewardCrate : FarmInteractiveProp
{
    private TipoRicompensaCassa tipoRicompensa;
    private int valoreRicompensa;
    private PlayerHealth saluteGiocatore;
    private float prossimoAvvisoVitaPiena;

    public TipoRicompensaCassa TipoRicompensa => tipoRicompensa;
    public int ValoreRicompensa => valoreRicompensa;

    public void ConfiguraRicompensa(
        TipoRicompensaCassa tipo,
        int valore
    )
    {
        tipoRicompensa = tipo;
        valoreRicompensa = Mathf.Max(0, valore);
        if (tipoRicompensa == TipoRicompensaCassa.Cura)
        {
            saluteGiocatore = FindFirstObjectByType<PlayerHealth>();
        }
    }

    protected override bool PuoRicevereDanno()
    {
        if (tipoRicompensa != TipoRicompensaCassa.Cura) return true;
        if (saluteGiocatore == null)
        {
            saluteGiocatore = FindFirstObjectByType<PlayerHealth>();
        }
        bool puoAprire =
            saluteGiocatore != null && !saluteGiocatore.VitaPiena;
        if (!puoAprire &&
            saluteGiocatore != null &&
            Time.unscaledTime >= prossimoAvvisoVitaPiena)
        {
            prossimoAvvisoVitaPiena = Time.unscaledTime + 0.8f;
            FarmInteractiveHint.MostraVitaPiena();
        }
        return puoAprire;
    }

    protected override void QuandoDistrutto()
    {
        Arena?.NotificaCassaAperta();
        if (tipoRicompensa == TipoRicompensaCassa.Monete)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.AggiungiMonete(valoreRicompensa);
            }
            CreaScoppio(
                new Color(1f, 0.86f, 0.18f, 1f),
                new Color(1f, 0.55f, 0.08f, 1f),
                0.85f,
                12
            );
        }
        else
        {
            if (saluteGiocatore == null)
            {
                saluteGiocatore = FindFirstObjectByType<PlayerHealth>();
            }
            if (saluteGiocatore != null)
            {
                saluteGiocatore.Cura(valoreRicompensa);
            }
            CreaScoppio(
                new Color(0.42f, 1f, 0.46f, 1f),
                new Color(1f, 0.94f, 0.72f, 1f),
                0.85f,
                12
            );
        }
    }
}

public sealed class FarmInteractiveMarker : MonoBehaviour
{
    private Vector3 posizioneBase;
    private Color coloreBase;
    private SpriteRenderer rendererMarker;
    private float fase;
    private float evidenza;

    public void Configura(Vector2 posizione, Color colore)
    {
        posizioneBase = posizione;
        transform.localPosition = posizioneBase;
        coloreBase = colore;
        rendererMarker = GetComponent<SpriteRenderer>();
        fase = Mathf.Repeat(
            transform.parent != null
                ? transform.parent.position.x * 0.37f +
                  transform.parent.position.y * 0.61f
                : 0f,
            Mathf.PI * 2f
        );
    }

    public void Evidenzia()
    {
        evidenza = 1f;
    }

    void Update()
    {
        float onda = 0.5f + 0.5f * Mathf.Sin(
            Time.time * 3.1f + fase
        );
        transform.localPosition =
            posizioneBase + Vector3.up * Mathf.Lerp(0f, 0.08f, onda);
        float scala = Mathf.Lerp(0.92f, 1.08f, onda) +
                      evidenza * 0.18f;
        transform.localScale = Vector3.one * scala;
        if (rendererMarker != null)
        {
            rendererMarker.color = Color.Lerp(
                coloreBase,
                Color.white,
                evidenza * 0.7f
            );
        }
        evidenza = Mathf.MoveTowards(
            evidenza,
            0f,
            Time.deltaTime * 3.5f
        );
    }
}

internal static class FarmPixelBurst
{
    public static void Crea(
        Vector2 posizione,
        Color colorePrincipale,
        Color coloreSecondario,
        float raggio,
        int numeroParticelle
    )
    {
        GameObject radice = new GameObject("ScoppioFattoriaPixel");
        FarmPixelBurstEffect effetto =
            radice.AddComponent<FarmPixelBurstEffect>();
        effetto.Configura(
            posizione,
            colorePrincipale,
            coloreSecondario,
            raggio,
            numeroParticelle
        );
    }
}

internal sealed class FarmPixelBurstEffect : MonoBehaviour
{
    private readonly List<SpriteRenderer> particelle =
        new List<SpriteRenderer>();
    private readonly List<Vector2> direzioni =
        new List<Vector2>();
    private Color coloreA;
    private Color coloreB;
    private float raggio;
    private float tempo;
    private float durata;

    public void Configura(
        Vector2 posizione,
        Color principale,
        Color secondario,
        float nuovoRaggio,
        int numeroParticelle
    )
    {
        transform.position = posizione;
        coloreA = principale;
        coloreB = secondario;
        raggio = Mathf.Max(0.2f, nuovoRaggio);
        durata = Mathf.Lerp(0.24f, 0.36f, Mathf.InverseLerp(0.5f, 2.5f, raggio));

        int quantita = Mathf.Clamp(numeroParticelle, 6, 24);
        for (int i = 0; i < quantita; i++)
        {
            GameObject pixel = new GameObject("Pixel_" + i);
            pixel.transform.SetParent(transform, false);
            SpriteRenderer renderer = pixel.AddComponent<SpriteRenderer>();
            renderer.sprite = FarmInteractiveArt.Pixel;
            renderer.sortingOrder = 18;
            particelle.Add(renderer);

            float angolo = i * (Mathf.PI * 2f / quantita);
            direzioni.Add(new Vector2(
                Mathf.Cos(angolo),
                Mathf.Sin(angolo)
            ));
        }
        Aggiorna(0f);
    }

    void Update()
    {
        tempo += Time.deltaTime;
        float t = Mathf.Clamp01(tempo / Mathf.Max(0.01f, durata));
        Aggiorna(t);
        if (tempo >= durata) Destroy(gameObject);
    }

    private void Aggiorna(float t)
    {
        float distanza = Mathf.Lerp(0.08f, raggio, t);
        float alpha = 1f - t;
        for (int i = 0; i < particelle.Count; i++)
        {
            SpriteRenderer renderer = particelle[i];
            renderer.transform.localPosition =
                direzioni[i] * distanza *
                (i % 2 == 0 ? 1f : 0.72f);
            renderer.transform.localScale =
                Vector3.one * Mathf.Lerp(0.15f, 0.045f, t);
            Color colore = i % 3 == 0 ? coloreB : coloreA;
            colore.a *= alpha;
            renderer.color = colore;
        }
    }
}

internal sealed class FarmInteractiveHint : MonoBehaviour
{
    private const string MessaggioIniziale =
        "FATTORIA INTERATTIVA  |  FANGO: RALLENTA  |  " +
        "ROMPI ZUCCHE E CASSE";

    private CanvasGroup gruppo;
    private TMP_Text testo;
    private float durata;
    private float tempo;

    public static void Crea(float nuovaDurata)
    {
        CreaOAggiorna(MessaggioIniziale, nuovaDurata);
    }

    public static void MostraVitaPiena()
    {
        CreaOAggiorna(
            "VITA PIENA  |  CONSERVA LA CASSA PER DOPO",
            1.6f
        );
    }

    private static void CreaOAggiorna(
        string messaggio,
        float nuovaDurata
    )
    {
        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null)
        {
            return;
        }
        Transform pannelloEsistente =
            interfaccia.transform.Find("SuggerimentoFattoria");
        if (pannelloEsistente != null)
        {
            FarmInteractiveHint hintEsistente =
                pannelloEsistente.GetComponent<FarmInteractiveHint>();
            if (hintEsistente != null)
            {
                hintEsistente.Mostra(messaggio, nuovaDurata);
            }
            return;
        }

        GameObject pannello = new GameObject(
            "SuggerimentoFattoria",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup),
            typeof(FarmInteractiveHint)
        );
        pannello.transform.SetParent(interfaccia.transform, false);
        RectTransform rect = pannello.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 26f);
        rect.sizeDelta = new Vector2(900f, 66f);

        Image immagine = pannello.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(immagine, true, false);
        immagine.color = FarmPixelUI.ColoreCartaFlat;
        immagine.raycastTarget = false;

        GameObject oggettoTesto = new GameObject(
            "Testo",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggettoTesto.transform.SetParent(pannello.transform, false);
        RectTransform testoRect =
            oggettoTesto.GetComponent<RectTransform>();
        testoRect.anchorMin = Vector2.zero;
        testoRect.anchorMax = Vector2.one;
        testoRect.offsetMin = new Vector2(18f, 5f);
        testoRect.offsetMax = new Vector2(-18f, -5f);

        TextMeshProUGUI testo =
            oggettoTesto.GetComponent<TextMeshProUGUI>();
        TMP_Text riferimento =
            GameManager.TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) testo.font = riferimento.font;
        testo.fontSize = 20f;
        testo.fontStyle = FontStyles.Bold;
        testo.alignment = TextAlignmentOptions.Center;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(
            testo,
            FarmPixelUI.TestoChiaroFlat
        );

        FarmInteractiveHint hint =
            pannello.GetComponent<FarmInteractiveHint>();
        hint.gruppo = pannello.GetComponent<CanvasGroup>();
        hint.testo = testo;
        hint.gruppo.alpha = 0f;
        hint.Mostra(messaggio, nuovaDurata);
    }

    private void Mostra(string messaggio, float nuovaDurata)
    {
        if (testo != null)
        {
            testo.text = messaggio;
        }
        durata = Mathf.Max(0.1f, nuovaDurata);
        tempo = 0f;
        if (gruppo != null)
        {
            gruppo.alpha = 0f;
        }
    }

    void Update()
    {
        tempo += Time.deltaTime;
        if (gruppo == null)
        {
            Destroy(gameObject);
            return;
        }

        float ingresso = Mathf.Clamp01(tempo / 0.22f);
        float uscita = Mathf.Clamp01((durata - tempo) / 0.55f);
        gruppo.alpha = Mathf.Min(ingresso, uscita);
        if (tempo >= durata) Destroy(gameObject);
    }
}

internal static class FarmInteractiveArt
{
    private static Sprite mirino;
    private static Sprite pericolo;
    private static Sprite moneta;
    private static Sprite cuore;
    private static Sprite frecciaFango;
    private static Sprite pixel;

    public static Sprite Mirino =>
        mirino ?? (mirino = CreaMirino());
    public static Sprite Pericolo =>
        pericolo ?? (pericolo = CreaPericolo());
    public static Sprite Moneta =>
        moneta ?? (moneta = CreaMoneta());
    public static Sprite Cuore =>
        cuore ?? (cuore = CreaCuore());
    public static Sprite FrecciaFango =>
        frecciaFango ?? (frecciaFango = CreaFrecciaFango());
    public static Sprite Pixel =>
        pixel ?? (pixel = CreaPixel());

    private static Sprite CreaMirino()
    {
        const int lato = 11;
        Color32[] dati = new Color32[lato * lato];
        Color32 colore = new Color32(255, 237, 170, 255);
        for (int i = 1; i <= 3; i++)
        {
            Imposta(dati, lato, 5, 5 + i, colore);
            Imposta(dati, lato, 5, 5 - i, colore);
            Imposta(dati, lato, 5 + i, 5, colore);
            Imposta(dati, lato, 5 - i, 5, colore);
        }
        Imposta(dati, lato, 5, 5, new Color32(86, 43, 20, 255));
        return CreaSprite("MarcatoreMirinoFattoria", dati, lato, 11f);
    }

    private static Sprite CreaPericolo()
    {
        const int lato = 9;
        Color32[] dati = new Color32[lato * lato];
        Color32 scuro = new Color32(84, 35, 17, 255);
        Color32 luce = new Color32(255, 224, 92, 255);
        for (int y = 3; y <= 7; y++)
        {
            Imposta(dati, lato, 4, y, scuro);
            if (y >= 4) Imposta(dati, lato, 4, y, luce);
        }
        Imposta(dati, lato, 4, 1, scuro);
        Imposta(dati, lato, 4, 2, luce);
        return CreaSprite("MarcatorePericoloFattoria", dati, lato, 9f);
    }

    private static Sprite CreaMoneta()
    {
        const int lato = 11;
        Color32[] dati = new Color32[lato * lato];
        Color32 bordo = new Color32(92, 48, 18, 255);
        Color32 oro = new Color32(246, 177, 37, 255);
        Color32 luce = new Color32(255, 225, 93, 255);
        for (int y = 2; y <= 8; y++)
        {
            for (int x = 2; x <= 8; x++)
            {
                int dx = x - 5;
                int dy = y - 5;
                if (dx * dx + dy * dy > 12) continue;
                Imposta(
                    dati,
                    lato,
                    x,
                    y,
                    dx * dx + dy * dy >= 8 ? bordo : oro
                );
            }
        }
        Imposta(dati, lato, 4, 7, luce);
        Imposta(dati, lato, 5, 7, luce);
        return CreaSprite("MarcatoreMonetaFattoria", dati, lato, 11f);
    }

    private static Sprite CreaCuore()
    {
        const int lato = 11;
        Color32[] dati = new Color32[lato * lato];
        Color32 bordo = new Color32(82, 31, 25, 255);
        Color32 rosso = new Color32(227, 64, 50, 255);
        Rettangolo(dati, lato, 2, 5, 8, 7, bordo);
        Rettangolo(dati, lato, 3, 3, 7, 7, rosso);
        Rettangolo(dati, lato, 2, 6, 4, 8, rosso);
        Rettangolo(dati, lato, 6, 6, 8, 8, rosso);
        Imposta(dati, lato, 5, 2, rosso);
        Imposta(dati, lato, 4, 3, rosso);
        Imposta(dati, lato, 6, 3, rosso);
        return CreaSprite("MarcatoreCuoreFattoria", dati, lato, 11f);
    }

    private static Sprite CreaFrecciaFango()
    {
        const int lato = 11;
        Color32[] dati = new Color32[lato * lato];
        Color32 colore = new Color32(246, 203, 82, 255);
        for (int y = 5; y <= 9; y++)
        {
            Imposta(dati, lato, 5, y, colore);
        }
        Imposta(dati, lato, 3, 5, colore);
        Imposta(dati, lato, 4, 4, colore);
        Imposta(dati, lato, 5, 3, colore);
        Imposta(dati, lato, 6, 4, colore);
        Imposta(dati, lato, 7, 5, colore);
        return CreaSprite("MarcatoreFango", dati, lato, 11f);
    }

    private static Sprite CreaPixel()
    {
        Color32[] dati = { new Color32(255, 255, 255, 255) };
        return CreaSprite("PixelEffettoFattoria", dati, 1, 1f);
    }

    private static Sprite CreaSprite(
        string nome,
        Color32[] dati,
        int lato,
        float pixelPerUnita
    )
    {
        Texture2D texture = new Texture2D(
            lato,
            lato,
            TextureFormat.RGBA32,
            false
        );
        texture.name = nome + "_Texture";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels32(dati);
        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, lato, lato),
            new Vector2(0.5f, 0.5f),
            pixelPerUnita,
            0,
            SpriteMeshType.FullRect
        );
        sprite.name = nome;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static void Rettangolo(
        Color32[] dati,
        int lato,
        int xMin,
        int yMin,
        int xMax,
        int yMax,
        Color32 colore
    )
    {
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                Imposta(dati, lato, x, y, colore);
            }
        }
    }

    private static void Imposta(
        Color32[] dati,
        int lato,
        int x,
        int y,
        Color32 colore
    )
    {
        if (x < 0 || x >= lato || y < 0 || y >= lato) return;
        dati[y * lato + x] = colore;
    }
}
