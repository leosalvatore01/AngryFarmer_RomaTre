using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class UovoRecuperabile : MonoBehaviour
{
    private static readonly HashSet<UovoRecuperabile> attivi =
        new HashSet<UovoRecuperabile>();

    private Gallina gallina;
    private Transform giocatore;
    private Transform grafica;
    private SpriteRenderer alone;
    private TMP_Text testoTempo;
    private float tempoRimasto;
    private float raggioRecupero;
    private float fase;
    private float prossimaRicercaGiocatore;
    private bool risolto;

    public static int RecuperiAttivi => attivi.Count;
    public float TempoRimasto => Mathf.Max(0f, tempoRimasto);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraRegistro()
    {
        attivi.Clear();
    }

    public static UovoRecuperabile Crea(
        Gallina gallinaDaRecuperare,
        Vector3 posizione
    )
    {
        if (gallinaDaRecuperare == null) return null;

        GameObject oggetto = new GameObject("UovoRecuperabile");
        oggetto.transform.position = posizione;
        UovoRecuperabile recupero =
            oggetto.AddComponent<UovoRecuperabile>();
        recupero.Inizializza(gallinaDaRecuperare);
        return recupero;
    }

    private void Awake()
    {
        FarmObjectivesBalanceSettings config =
            GameBalanceConfig.Corrente.ObiettiviFattoria;
        tempoRimasto = Mathf.Max(1f, config.durataRecuperoUovo);
        raggioRecupero = Mathf.Max(0.2f, config.raggioRecuperoUovo);

        CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = raggioRecupero;

        CreaGrafica();
        CercaGiocatore();
    }

    private void OnEnable()
    {
        attivi.Add(this);
    }

    private void Inizializza(Gallina gallinaDaRecuperare)
    {
        gallina = gallinaDaRecuperare;
    }

    private void Update()
    {
        if (risolto) return;

        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            AnnullaSenzaPremio();
            return;
        }

        tempoRimasto -= Time.deltaTime;
        fase += Time.deltaTime * 5.8f;
        AggiornaGrafica();

        if (giocatore == null && Time.time >= prossimaRicercaGiocatore)
        {
            CercaGiocatore();
        }

        if (giocatore != null &&
            Vector2.Distance(transform.position, giocatore.position) <=
            raggioRecupero)
        {
            Raccogli();
            return;
        }

        if (tempoRimasto <= 0f)
        {
            Scade();
        }
    }

    private void CercaGiocatore()
    {
        prossimaRicercaGiocatore = Time.time + 0.35f;
        GameObject oggetto = GameObject.FindGameObjectWithTag("Player");
        giocatore = oggetto != null ? oggetto.transform : null;
    }

    private void OnTriggerEnter2D(Collider2D altro)
    {
        if (risolto || altro == null) return;
        if (altro.CompareTag("Player"))
        {
            Raccogli();
        }
    }

    private void Raccogli()
    {
        if (risolto) return;
        if (gallina == null || !gallina.CompletaRecupero())
        {
            AnnullaSenzaPremio();
            return;
        }

        risolto = true;
        attivi.Remove(this);

        int premio = GameManager.instance != null
            ? GameManager.instance.RegistraUovoRecuperato()
            : 0;
        FarmObjectivesController.Instance?.NotificaUovoRecuperato();

        string messaggio = premio > 0
            ? "+" + premio + " UOVO"
            : "UOVO SALVO";
        if (GameManager.instance != null &&
            GameManager.instance.SerieSalvataggi > 1)
        {
            messaggio += "  SERIE x" +
                         GameManager.instance.SerieSalvataggi;
        }
        CreaTestoEsito(
            messaggio,
            new Color(1f, 0.85f, 0.28f, 1f)
        );
        Destroy(gameObject);
    }

    private void Scade()
    {
        if (risolto) return;
        risolto = true;
        attivi.Remove(this);

        if (gallina != null)
        {
            gallina.ConfermaPerditaDaRecupero();
        }
        FarmObjectivesController.Instance?.NotificaUovoPerso();
        CreaTestoEsito(
            "UOVO PERSO",
            new Color(1f, 0.32f, 0.22f, 1f)
        );
        Destroy(gameObject);
    }

    private void CreaGrafica()
    {
        grafica = new GameObject("GraficaUovo").transform;
        grafica.SetParent(transform, false);

        GameObject oggettoAlone = new GameObject("Alone");
        oggettoAlone.transform.SetParent(grafica, false);
        alone = oggettoAlone.AddComponent<SpriteRenderer>();
        alone.sprite = FarmPixelUI.OttieniIcona(FarmPixelIcon.Uovo);
        alone.color = new Color(1f, 0.76f, 0.15f, 0.35f);
        alone.sortingOrder = 29;
        oggettoAlone.transform.localScale = Vector3.one * 1.18f;

        GameObject oggettoUovo = new GameObject("Uovo");
        oggettoUovo.transform.SetParent(grafica, false);
        SpriteRenderer renderer = oggettoUovo.AddComponent<SpriteRenderer>();
        renderer.sprite = FarmPixelUI.OttieniIcona(FarmPixelIcon.Uovo);
        renderer.color = Color.white;
        renderer.sortingOrder = 30;
        oggettoUovo.transform.localScale = Vector3.one * 0.82f;

        GameObject oggettoTesto = new GameObject("TempoRecupero");
        oggettoTesto.transform.SetParent(grafica, false);
        oggettoTesto.transform.localPosition = new Vector3(0f, 0.88f, 0f);
        oggettoTesto.transform.localScale = Vector3.one * 0.28f;
        testoTempo = oggettoTesto.AddComponent<TextMeshPro>();
        TMP_Text riferimento = GameManager.TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) testoTempo.font = riferimento.font;
        testoTempo.text = "RECUPERA  " + Mathf.CeilToInt(tempoRimasto);
        testoTempo.fontSize = 4f;
        testoTempo.fontStyle = FontStyles.Bold;
        testoTempo.alignment = TextAlignmentOptions.Center;
        testoTempo.color = new Color(1f, 0.9f, 0.47f, 1f);
        Renderer rendererTestoTempo =
            testoTempo.GetComponent<Renderer>();
        if (rendererTestoTempo != null)
        {
            rendererTestoTempo.sortingOrder = 31;
        }
    }

    private void AggiornaGrafica()
    {
        if (grafica != null)
        {
            float salto = Mathf.Sin(fase) * 0.08f;
            grafica.localPosition = Vector3.up * (0.08f + salto);
        }
        if (alone != null)
        {
            Color colore = alone.color;
            colore.a = 0.24f + (Mathf.Sin(fase * 1.3f) + 1f) * 0.12f;
            alone.color = colore;
        }
        if (testoTempo != null)
        {
            testoTempo.text =
                "RECUPERA  " + Mathf.CeilToInt(TempoRimasto);
            testoTempo.color = TempoRimasto <= 2.5f
                ? new Color(1f, 0.32f, 0.2f, 1f)
                : new Color(1f, 0.9f, 0.47f, 1f);
        }
    }

    private void CreaTestoEsito(string messaggio, Color colore)
    {
        GameObject oggetto = new GameObject("EsitoRecuperoUovo");
        oggetto.transform.position = transform.position + Vector3.up * 0.6f;
        oggetto.transform.localScale = Vector3.one * 0.13f;

        TextMeshPro testo = oggetto.AddComponent<TextMeshPro>();
        TMP_Text riferimento = GameManager.TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) testo.font = riferimento.font;
        testo.text = messaggio;
        testo.fontSize = 3.2f;
        testo.fontStyle = FontStyles.Bold;
        testo.alignment = TextAlignmentOptions.Center;
        testo.color = colore;
        Renderer rendererTesto = testo.GetComponent<Renderer>();
        if (rendererTesto != null)
        {
            rendererTesto.sortingOrder = 34;
        }
        oggetto.AddComponent<AutoDistruzioneRealtime>();
    }

    private void AnnullaSenzaPremio()
    {
        if (risolto) return;
        risolto = true;
        attivi.Remove(this);
        if (gallina != null)
        {
            gallina.CompletaRecupero();
        }
        Destroy(gameObject);
    }

    public static void RimuoviTuttiSenzaEsito()
    {
        UovoRecuperabile[] copia =
            new UovoRecuperabile[attivi.Count];
        attivi.CopyTo(copia);
        foreach (UovoRecuperabile recupero in copia)
        {
            if (recupero != null)
            {
                recupero.AnnullaSenzaPremio();
            }
        }
        attivi.Clear();
    }

    private void OnDisable()
    {
        attivi.Remove(this);
        if (!risolto && gallina != null &&
            GameManager.instance != null &&
            !GameManager.instance.isGameOver)
        {
            risolto = true;
            gallina.CompletaRecupero();
        }
    }
}
