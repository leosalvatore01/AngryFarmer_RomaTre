using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Feedback di leggibilita legato alla singola scena di gioco.
/// Non usa il pool o i contatori del feedback di combattimento e non altera
/// il flusso casuale globale del gameplay.
/// </summary>
public sealed class WaveReadabilityController : MonoBehaviour
{
    private const int NumeroSettori = 8;
    private const int DimensionePoolTelegraph = 16;
    private const int SegmentiBarra = 10;
    private const int NumeroTipiVolpe = (int)TipoVolpe.Scavatrice + 1;
    private const float DurataSegnale = 1.45f;
    private const float MargineScadenzaPreavviso = 0.35f;
    private const float IntervalloCampionamento = 0.08f;

    private sealed class PreavvisoAttivo
    {
        public int token;
        public int slotId;
        public Vector2 posizione;
        public TipoVolpe tipo;
        public float scadenza;
        public WaveSpawnTelegraph telegraph;
    }

    private sealed class IndicatoreSettore
    {
        public GameObject radice;
        public Image sfondo;
        public RectTransform freccia;
        public Image immagineFreccia;
        public TMP_Text testoQuantita;
        public int quantitaVisualizzata = -1;
        public TipoVolpe tipoVisualizzato = (TipoVolpe)(-1);
        public bool contieneMinacceVisualizzato;
    }

    private readonly Dictionary<long, PreavvisoAttivo> preavvisi =
        new Dictionary<long, PreavvisoAttivo>();
    private readonly List<long> chiaviDaRimuovere = new List<long>();
    private readonly int[] conteggiSettori = new int[NumeroSettori];
    private readonly int[] conteggiTelegraphSettori = new int[NumeroSettori];
    private readonly int[] conteggiMinacceSettori = new int[NumeroSettori];
    private readonly TipoVolpe[] tipiPrioritariSettori =
        new TipoVolpe[NumeroSettori];
    private readonly int[] prioritaTipiSettori = new int[NumeroSettori];
    private readonly int[] conteggiTipiVisualizzati =
        new int[NumeroTipiVolpe];

    private EnemySpawner spawner;
    private AnteprimaOndata anteprimaCorrente;
    private WaveSpawnTelegraph[] poolTelegraph;
    private IndicatoreSettore[] indicatori;
    private int indiceRiutilizzoTelegraph;

    private GameObject radiceInterfaccia;
    private GameObject schedaHud;
    private TMP_Text testoRimasti;
    private TMP_Text testoGruppo;
    private Image[] segmentiBarra;
    private TMP_Text[] chipTipi;
    private Image[] sfondiChipTipi;
    private GameObject pannelloSegnale;
    private CanvasGroup gruppoSegnale;
    private TMP_Text testoSegnale;

    private AudioSource sorgenteAudio;
    private AudioClip clipSegnale;
    private Sprite spritePixelTelegraph;
    private Sprite spriteFreccia;

    private int tokenAttivo;
    private int sogliaUltimiNemici = 2;
    private float volumeSegnale = 0.26f;
    private int rimastiPrecedenti;
    private int rimastiVisualizzati;
    private int totaleVisualizzato;
    private bool ondaInCorso;
    private bool segnaleEmesso;
    private bool attraversamentoSogliaInAttesa;
    private float tempoSegnale;
    private float tempoCampionamento;

    public int PreavvisiRichiesti { get; private set; }
    public int PreavvisiTerminati { get; private set; }
    public int PreavvisiIgnoratiPerToken { get; private set; }
    public int TelegraphAttivati { get; private set; }
    public int SegnaliUltimiNemiciEmessi { get; private set; }
    public int SuoniSegnaleRiprodotti { get; private set; }
    public int MassimoSettoriContemporanei { get; private set; }
    public int PreavvisiAttivi => preavvisi.Count;
    public int SettoriAttivi { get; private set; }
    public int SettoriConMinacceAttive { get; private set; }
    public int PreavvisiFuoriSchermo { get; private set; }
    public int MinacceFuoriSchermo { get; private set; }
    public int RimastiVisualizzati => rimastiVisualizzati;
    public int TotaleVisualizzato => totaleVisualizzato;
    public int SogliaUltimiNemici => sogliaUltimiNemici;
    public float VolumeSegnale => volumeSegnale;
    public bool HudVisibile => schedaHud != null && schedaHud.activeSelf;
    public bool SegnaleVisibile =>
        pannelloSegnale != null && pannelloSegnale.activeSelf;
    public bool OndaInCorso => ondaInCorso;
    public int TokenAttivo => tokenAttivo;
    public int DimensionePool => poolTelegraph != null
        ? poolTelegraph.Length
        : 0;

    public int QuantitaNelSettore(int settore)
    {
        return settore >= 0 && settore < NumeroSettori
            ? conteggiSettori[settore]
            : 0;
    }

    public int QuantitaTipoVisualizzata(TipoVolpe tipo)
    {
        int indice = (int)FoxVariantStyle.Normalizza(tipo);
        return conteggiTipiVisualizzati[indice];
    }

    public TipoVolpe TipoPrioritarioNelSettore(int settore)
    {
        return settore >= 0 && settore < NumeroSettori
            ? tipiPrioritariSettori[settore]
            : TipoVolpe.Comune;
    }

    public int PreavvisiNelSettore(int settore)
    {
        return settore >= 0 && settore < NumeroSettori
            ? conteggiTelegraphSettori[settore]
            : 0;
    }

    public int MinacceNelSettore(int settore)
    {
        return settore >= 0 && settore < NumeroSettori
            ? conteggiMinacceSettori[settore]
            : 0;
    }

    void Awake()
    {
        LeggiConfigurazione();
        CreaPoolTelegraph();
        CreaAudioProcedurale();
        ProvaCostruireInterfaccia();
    }

    void Update()
    {
        if (radiceInterfaccia == null)
        {
            ProvaCostruireInterfaccia();
        }

        if (ondaInCorso && StatoPartitaNascondeInterfaccia())
        {
            TerminaOnda();
            return;
        }

        PulisciPreavvisiScaduti();

        if (ondaInCorso)
        {
            tempoCampionamento -= Time.deltaTime;
            if (tempoCampionamento <= 0f)
            {
                tempoCampionamento = IntervalloCampionamento;
                AggiornaDaSpawner();
            }
        }

        AggiornaVisibilitaGenerale();
        AggiornaIndicatoriBordo();
        AggiornaAnimazioneSegnale();
    }

    /// <summary>
    /// Collega il controller allo spawner. La chiamata e idempotente.
    /// </summary>
    public void Configura(EnemySpawner nuovoSpawner)
    {
        if (spawner == nuovoSpawner)
        {
            if (spawner != null) AggiornaDaSpawner();
            return;
        }

        if (spawner != null)
        {
            spawner.ProgressoCambiato -= ProgressoCambiato;
        }

        spawner = nuovoSpawner;
        if (spawner != null)
        {
            spawner.ProgressoCambiato += ProgressoCambiato;
            AggiornaDaSpawner();
        }
    }

    public void IniziaOnda(AnteprimaOndata anteprima)
    {
        CancellaTuttiPreavvisi(false);
        LeggiConfigurazione();
        ProvaCostruireInterfaccia();

        anteprimaCorrente = anteprima;
        ondaInCorso = anteprima.Valida;
        segnaleEmesso = false;
        attraversamentoSogliaInAttesa = false;
        tempoSegnale = 0f;
        tempoCampionamento = 0f;
        tokenAttivo = 0;

        totaleVisualizzato = Mathf.Max(0, anteprima.NumeroVolpi);
        rimastiVisualizzati = totaleVisualizzato;
        rimastiPrecedenti = rimastiVisualizzati;

        if (spawner != null)
        {
            ProgressoOndata progresso = spawner.ProgressoCorrente;
            if (progresso.Token != 0) tokenAttivo = progresso.Token;
        }

        NascondiSegnale();
        AggiornaTestiHud(
            rimastiVisualizzati,
            totaleVisualizzato,
            0,
            Mathf.Max(0, anteprima.NumeroGruppi)
        );
        AggiornaChipTipi(anteprima.Composizione);
        AggiornaDaSpawner();
        AggiornaVisibilitaGenerale();
    }

    public void PreavvisaSpawn(
        int token,
        int slotId,
        Vector2 posizione,
        float durata,
        TipoVolpe tipo = TipoVolpe.Comune
    )
    {
        PreavvisiRichiesti++;
        if (!ondaInCorso)
        {
            PreavvisiIgnoratiPerToken++;
            return;
        }

        if (tokenAttivo == 0) tokenAttivo = token;
        if (token != tokenAttivo)
        {
            PreavvisiIgnoratiPerToken++;
            return;
        }

        long chiave = CreaChiave(token, slotId);
        PreavvisoAttivo precedente;
        if (preavvisi.TryGetValue(chiave, out precedente))
        {
            DisattivaTelegraphSeCoincide(precedente);
            preavvisi.Remove(chiave);
        }

        float durataSicura = Mathf.Max(0.05f, durata);
        WaveSpawnTelegraph telegraph = OttieniTelegraph();
        if (telegraph != null)
        {
            telegraph.Attiva(
                token,
                slotId,
                posizione,
                durataSicura,
                tipo
            );
            TelegraphAttivati++;
        }

        preavvisi[chiave] = new PreavvisoAttivo
        {
            token = token,
            slotId = slotId,
            posizione = posizione,
            tipo = FoxVariantStyle.Normalizza(tipo),
            scadenza = Time.time + durataSicura + MargineScadenzaPreavviso,
            telegraph = telegraph
        };

        AggiornaIndicatoriBordo();
    }

    public void TerminaPreavviso(int token, int slotId)
    {
        long chiave = CreaChiave(token, slotId);
        PreavvisoAttivo preavviso;
        if (!preavvisi.TryGetValue(chiave, out preavviso)) return;

        DisattivaTelegraphSeCoincide(preavviso);
        preavvisi.Remove(chiave);
        PreavvisiTerminati++;
        AggiornaIndicatoriBordo();
    }

    public void TerminaOnda()
    {
        ondaInCorso = false;
        tokenAttivo = 0;
        attraversamentoSogliaInAttesa = false;
        tempoSegnale = 0f;
        CancellaTuttiPreavvisi(false);
        NascondiSegnale();
        NascondiIndicatori();
        if (schedaHud != null) schedaHud.SetActive(false);
    }

    private void ProgressoCambiato(ProgressoOndata progresso)
    {
        if (!ondaInCorso) return;
        AggiornaDaSpawner();
    }

    private void AggiornaDaSpawner()
    {
        if (!ondaInCorso || spawner == null) return;

        // Leggiamo intenzionalmente sia lo snapshot sia la collezione reale:
        // lo snapshot fornisce gli slot futuri, MinacceOnda esclude i morti
        // che possono restare taggati durante l'animazione di morte.
        ProgressoOndata progresso = spawner.ProgressoCorrente;
        int minacceVive = ContaMinacceVive();
        int rimasti = Mathf.Max(0, progresso.VolpiDaSpawnare) + minacceVive;
        int totale = Mathf.Max(
            Mathf.Max(0, anteprimaCorrente.NumeroVolpi),
            Mathf.Max(0, progresso.VolpiTotali)
        );

        if (progresso.Token != 0)
        {
            if (tokenAttivo == 0) tokenAttivo = progresso.Token;
            if (progresso.Token != tokenAttivo) return;
        }

        ValutaSegnaleUltimiNemici(progresso, rimasti);

        rimastiVisualizzati = rimasti;
        totaleVisualizzato = totale;
        AggiornaTestiHud(
            rimasti,
            totale,
            progresso.GruppoCorrente,
            progresso.TotaleGruppi
        );
        AggiornaChipTipi(progresso.ComposizioneRimasta);
        rimastiPrecedenti = rimasti;
    }

    private int ContaMinacceVive()
    {
        int conteggio = 0;
        if (spawner == null) return conteggio;

        IEnumerable<EnemyAI> minacce = spawner.MinacceOnda;
        if (minacce == null) return conteggio;

        foreach (EnemyAI minaccia in minacce)
        {
            if (minaccia != null && !minaccia.IsDead) conteggio++;
        }
        return conteggio;
    }

    private void ValutaSegnaleUltimiNemici(
        ProgressoOndata progresso,
        int rimasti
    )
    {
        if (segnaleEmesso || rimasti <= 0) return;

        bool attraversamento =
            rimastiPrecedenti > sogliaUltimiNemici &&
            rimasti <= sogliaUltimiNemici;

        if (attraversamento && !progresso.SpawnTerminato)
        {
            attraversamentoSogliaInAttesa = true;
        }

        bool puoMostrare =
            progresso.OndaAttiva &&
            progresso.SpawnTerminato &&
            rimasti <= sogliaUltimiNemici;

        if (puoMostrare &&
            (attraversamento || attraversamentoSogliaInAttesa))
        {
            MostraSegnaleUltimiNemici(rimasti);
        }
    }

    private void MostraSegnaleUltimiNemici(int quantita)
    {
        segnaleEmesso = true;
        attraversamentoSogliaInAttesa = false;
        tempoSegnale = DurataSegnale;
        SegnaliUltimiNemiciEmessi++;

        if (testoSegnale != null)
        {
            testoSegnale.text = quantita == 1
                ? "ULTIMA VOLPE!"
                : "ULTIME " + quantita + " VOLPI!";
        }
        if (pannelloSegnale != null) pannelloSegnale.SetActive(true);
        if (gruppoSegnale != null) gruppoSegnale.alpha = 1f;

        RiproduciSegnaleAudio();
    }

    private void RiproduciSegnaleAudio()
    {
        if (sorgenteAudio == null || clipSegnale == null || volumeSegnale <= 0f)
        {
            return;
        }

        CombatFeedbackController feedback = CombatFeedbackController.Instance;
        if (feedback != null && !feedback.AudioAbilitato) return;

        float volumeEffetti = GameOptionsController.Instance != null
            ? GameOptionsController.Instance.VolumeEffetti
            : 1f;
        sorgenteAudio.volume = volumeSegnale * volumeEffetti;
        sorgenteAudio.Stop();
        sorgenteAudio.clip = clipSegnale;
        sorgenteAudio.Play();
        SuoniSegnaleRiprodotti++;
    }

    private void LeggiConfigurazione()
    {
        GameBalanceConfig configurazione = GameBalanceConfig.Corrente;
        WaveBalanceSettings ritmo = configurazione != null
            ? configurazione.Ondate
            : null;
        if (ritmo == null) return;

        sogliaUltimiNemici = Mathf.Max(1, ritmo.sogliaUltimiNemici);
        volumeSegnale = Mathf.Clamp01(ritmo.volumeSegnaleUltimiNemici);
    }

    private bool StatoPartitaNascondeInterfaccia()
    {
        GameManager gestore = GameManager.instance;
        return gestore != null &&
               (gestore.isGameOver ||
                gestore.StatoCorrente == StatoPartita.Intervallo ||
                gestore.StatoCorrente == StatoPartita.FinePartita);
    }

    private bool InterfacciaOndaVisibile()
    {
        return ondaInCorso && !StatoPartitaNascondeInterfaccia();
    }

    private void AggiornaVisibilitaGenerale()
    {
        bool visibile = InterfacciaOndaVisibile();
        if (schedaHud != null && schedaHud.activeSelf != visibile)
        {
            schedaHud.SetActive(visibile);
        }

        if (!visibile)
        {
            NascondiIndicatori();
            if (pannelloSegnale != null) pannelloSegnale.SetActive(false);
        }
    }

    private void AggiornaTestiHud(
        int rimasti,
        int totale,
        int gruppo,
        int gruppiTotali
    )
    {
        rimasti = Mathf.Max(0, rimasti);
        totale = Mathf.Max(0, totale);

        if (testoRimasti != null)
        {
            testoRimasti.text = "RIMASTE  " + rimasti + " / " + totale;
        }
        if (testoGruppo != null)
        {
            testoGruppo.text = gruppiTotali > 0
                ? "GRUPPO  " + Mathf.Clamp(gruppo, 0, gruppiTotali) +
                  " / " + gruppiTotali
                : string.Empty;
        }

        if (segmentiBarra == null) return;
        float rapporto = totale > 0 ? Mathf.Clamp01((float)rimasti / totale) : 0f;
        int accesi = rimasti > 0
            ? Mathf.Max(1, Mathf.CeilToInt(rapporto * segmentiBarra.Length))
            : 0;
        bool ultimi = rimasti > 0 && rimasti <= sogliaUltimiNemici;
        Color acceso = ultimi
            ? new Color32(229, 76, 42, 255)
            : new Color32(239, 155, 48, 255);
        Color spento = new Color32(76, 42, 27, 210);

        for (int i = 0; i < segmentiBarra.Length; i++)
        {
            if (segmentiBarra[i] != null)
            {
                segmentiBarra[i].color = i < accesi ? acceso : spento;
            }
        }
    }

    private void AggiornaChipTipi(ComposizioneVolpi composizione)
    {
        if (chipTipi == null || sfondiChipTipi == null) return;

        for (int i = 0; i < chipTipi.Length; i++)
        {
            TipoVolpe tipo = (TipoVolpe)i;
            int quantita = composizione.Ottieni(tipo);
            if (conteggiTipiVisualizzati[i] == quantita) continue;
            conteggiTipiVisualizzati[i] = quantita;
            if (chipTipi[i] != null)
            {
                chipTipi[i].SetText(
                    FoxVariantStyle.Abbreviazione(tipo) + "  " + quantita
                );
                chipTipi[i].color = quantita > 0
                    ? new Color32(255, 247, 216, 255)
                    : new Color32(139, 117, 96, 210);
            }
            if (sfondiChipTipi[i] != null)
            {
                Color coloreTipo = FoxVariantStyle.ColoreUi(tipo);
                Color colore = Color.Lerp(
                    FarmPixelUI.ColoreCartaFlat,
                    coloreTipo,
                    quantita > 0 ? 0.28f : 0.08f
                );
                colore.a = quantita > 0 ? 0.64f : 0.36f;
                sfondiChipTipi[i].color = colore;
            }
        }
    }

    private void AggiornaIndicatoriBordo()
    {
        if (indicatori == null) return;
        if (!InterfacciaOndaVisibile())
        {
            NascondiIndicatori();
            return;
        }

        Camera cameraPrincipale = Camera.main;
        if (cameraPrincipale == null)
        {
            NascondiIndicatori();
            return;
        }

        Array.Clear(conteggiSettori, 0, conteggiSettori.Length);
        Array.Clear(
            conteggiTelegraphSettori,
            0,
            conteggiTelegraphSettori.Length
        );
        Array.Clear(
            conteggiMinacceSettori,
            0,
            conteggiMinacceSettori.Length
        );
        Array.Clear(prioritaTipiSettori, 0, prioritaTipiSettori.Length);
        Array.Clear(
            tipiPrioritariSettori,
            0,
            tipiPrioritariSettori.Length
        );
        PreavvisiFuoriSchermo = 0;
        MinacceFuoriSchermo = 0;

        foreach (PreavvisoAttivo preavviso in preavvisi.Values)
        {
            int settore;
            if (!ProvaOttieniSettoreFuoriSchermo(
                    cameraPrincipale,
                    preavviso.posizione,
                    out settore
                ))
            {
                continue;
            }
            conteggiTelegraphSettori[settore]++;
            RegistraTipoSettore(settore, preavviso.tipo);
            PreavvisiFuoriSchermo++;
        }

        if (spawner != null && spawner.MinacceOnda != null)
        {
            foreach (EnemyAI minaccia in spawner.MinacceOnda)
            {
                if (minaccia == null || minaccia.IsDead) continue;

                int settore;
                if (!ProvaOttieniSettoreFuoriSchermo(
                        cameraPrincipale,
                        minaccia.transform.position,
                        out settore
                    ))
                {
                    continue;
                }
                conteggiMinacceSettori[settore]++;
                RegistraTipoSettore(settore, minaccia.Tipo);
                MinacceFuoriSchermo++;
            }
        }

        int settoriAccesi = 0;
        int settoriConMinacce = 0;
        for (int i = 0; i < indicatori.Length; i++)
        {
            IndicatoreSettore indicatore = indicatori[i];
            int preavvisiSettore = conteggiTelegraphSettori[i];
            int minacceSettore = conteggiMinacceSettori[i];
            int quantita = preavvisiSettore + minacceSettore;
            conteggiSettori[i] = quantita;
            bool attivo = quantita > 0;
            if (indicatore.radice.activeSelf != attivo)
            {
                indicatore.radice.SetActive(attivo);
            }
            if (!attivo) continue;

            settoriAccesi++;
            bool contieneMinacce = minacceSettore > 0;
            if (contieneMinacce) settoriConMinacce++;
            TipoVolpe tipoPrioritario = tipiPrioritariSettori[i];
            Color coloreTipo = FoxVariantStyle.ColoreUi(tipoPrioritario);
            bool primoAggiornamento = indicatore.quantitaVisualizzata < 0;
            bool quantitaCambiata =
                indicatore.quantitaVisualizzata != quantita;
            bool tipoCambiato = indicatore.tipoVisualizzato != tipoPrioritario;
            bool statoCambiato =
                indicatore.contieneMinacceVisualizzato != contieneMinacce;

            if (primoAggiornamento || quantitaCambiata || tipoCambiato)
            {
                indicatore.testoQuantita.text =
                    FoxVariantStyle.Abbreviazione(tipoPrioritario) +
                    " x" + quantita;
            }
            if (primoAggiornamento || statoCambiato)
            {
                indicatore.testoQuantita.color = contieneMinacce
                    ? new Color32(255, 239, 194, 255)
                    : new Color32(255, 229, 169, 255);
            }
            if (primoAggiornamento || tipoCambiato || statoCambiato)
            {
                indicatore.immagineFreccia.color = contieneMinacce
                    ? coloreTipo
                    : Color.Lerp(coloreTipo, Color.white, 0.34f);
                Color coloreSfondo = Color.Lerp(
                    FarmPixelUI.ColoreCartaFlat,
                    coloreTipo,
                    contieneMinacce ? 0.32f : 0.18f
                );
                coloreSfondo.a = contieneMinacce ? 0.62f : 0.52f;
                indicatore.sfondo.color = coloreSfondo;
            }

            indicatore.quantitaVisualizzata = quantita;
            indicatore.tipoVisualizzato = tipoPrioritario;
            indicatore.contieneMinacceVisualizzato = contieneMinacce;
        }

        SettoriAttivi = settoriAccesi;
        SettoriConMinacceAttive = settoriConMinacce;
        MassimoSettoriContemporanei = Mathf.Max(
            MassimoSettoriContemporanei,
            SettoriAttivi
        );
    }

    private void RegistraTipoSettore(int settore, TipoVolpe tipo)
    {
        if (settore < 0 || settore >= NumeroSettori) return;
        int priorita = FoxVariantStyle.Priorita(tipo);
        if (priorita <= prioritaTipiSettori[settore]) return;
        prioritaTipiSettori[settore] = priorita;
        tipiPrioritariSettori[settore] =
            FoxVariantStyle.Normalizza(tipo);
    }

    private static bool ProvaOttieniSettoreFuoriSchermo(
        Camera cameraPrincipale,
        Vector2 posizione,
        out int settore
    )
    {
        settore = 0;
        Vector3 viewport = cameraPrincipale.WorldToViewportPoint(posizione);
        bool dentroAreaSicura =
            viewport.z > 0f &&
            viewport.x >= 0.07f && viewport.x <= 0.93f &&
            viewport.y >= 0.09f && viewport.y <= 0.91f;
        if (dentroAreaSicura) return false;

        Vector2 direzione = new Vector2(
            viewport.x - 0.5f,
            viewport.y - 0.5f
        );
        if (viewport.z < 0f) direzione = -direzione;
        if (direzione.sqrMagnitude < 0.0001f)
        {
            direzione = posizione -
                (Vector2)cameraPrincipale.transform.position;
        }

        float angolo = Mathf.Atan2(direzione.y, direzione.x) *
                        Mathf.Rad2Deg;
        settore = Mathf.RoundToInt(angolo / 45f) % NumeroSettori;
        if (settore < 0) settore += NumeroSettori;
        return true;
    }

    private void NascondiIndicatori()
    {
        SettoriAttivi = 0;
        SettoriConMinacceAttive = 0;
        PreavvisiFuoriSchermo = 0;
        MinacceFuoriSchermo = 0;
        Array.Clear(conteggiSettori, 0, conteggiSettori.Length);
        Array.Clear(
            conteggiTelegraphSettori,
            0,
            conteggiTelegraphSettori.Length
        );
        Array.Clear(
            conteggiMinacceSettori,
            0,
            conteggiMinacceSettori.Length
        );
        if (indicatori == null) return;
        for (int i = 0; i < indicatori.Length; i++)
        {
            if (indicatori[i] != null && indicatori[i].radice != null)
            {
                indicatori[i].radice.SetActive(false);
            }
        }
    }

    private void AggiornaAnimazioneSegnale()
    {
        if (tempoSegnale <= 0f)
        {
            NascondiSegnale();
            return;
        }

        if (!InterfacciaOndaVisibile())
        {
            if (pannelloSegnale != null) pannelloSegnale.SetActive(false);
            return;
        }

        tempoSegnale = Mathf.Max(0f, tempoSegnale - Time.deltaTime);
        float trascorso = DurataSegnale - tempoSegnale;
        float ingresso = Mathf.Clamp01(trascorso / 0.16f);
        float uscita = Mathf.Clamp01(tempoSegnale / 0.28f);
        float alpha = Mathf.Min(ingresso, uscita);
        float rimbalzo = Mathf.Sin(trascorso * 22f) *
                          Mathf.Exp(-trascorso * 3.2f);

        if (pannelloSegnale != null)
        {
            pannelloSegnale.SetActive(true);
            pannelloSegnale.transform.localScale =
                Vector3.one * (1f + rimbalzo * 0.075f);
        }
        if (gruppoSegnale != null) gruppoSegnale.alpha = alpha;
    }

    private void NascondiSegnale()
    {
        if (pannelloSegnale != null)
        {
            pannelloSegnale.SetActive(false);
            pannelloSegnale.transform.localScale = Vector3.one;
        }
        if (gruppoSegnale != null) gruppoSegnale.alpha = 0f;
    }

    private void PulisciPreavvisiScaduti()
    {
        if (preavvisi.Count == 0) return;

        chiaviDaRimuovere.Clear();
        foreach (KeyValuePair<long, PreavvisoAttivo> coppia in preavvisi)
        {
            if (Time.time >= coppia.Value.scadenza)
            {
                chiaviDaRimuovere.Add(coppia.Key);
            }
        }

        for (int i = 0; i < chiaviDaRimuovere.Count; i++)
        {
            PreavvisoAttivo preavviso;
            if (!preavvisi.TryGetValue(chiaviDaRimuovere[i], out preavviso))
            {
                continue;
            }
            DisattivaTelegraphSeCoincide(preavviso);
            preavvisi.Remove(chiaviDaRimuovere[i]);
        }
        chiaviDaRimuovere.Clear();
    }

    private void CancellaTuttiPreavvisi(bool contaTerminazioni)
    {
        foreach (PreavvisoAttivo preavviso in preavvisi.Values)
        {
            DisattivaTelegraphSeCoincide(preavviso);
            if (contaTerminazioni) PreavvisiTerminati++;
        }
        preavvisi.Clear();
    }

    private void DisattivaTelegraphSeCoincide(PreavvisoAttivo preavviso)
    {
        if (preavviso == null || preavviso.telegraph == null) return;
        preavviso.telegraph.TerminaSeCoincide(
            preavviso.token,
            preavviso.slotId
        );
    }

    private static long CreaChiave(int token, int slotId)
    {
        return ((long)(uint)token << 32) | (uint)slotId;
    }

    private void CreaPoolTelegraph()
    {
        if (poolTelegraph != null) return;

        spritePixelTelegraph = CreaSpritePixelMondo();
        GameObject contenitore = new GameObject("PoolTelegraphSpawn");
        contenitore.transform.SetParent(transform, false);
        poolTelegraph = new WaveSpawnTelegraph[DimensionePoolTelegraph];

        for (int i = 0; i < poolTelegraph.Length; i++)
        {
            GameObject oggetto = new GameObject("TelegraphSpawn_" + i);
            oggetto.transform.SetParent(contenitore.transform, false);
            WaveSpawnTelegraph telegraph =
                oggetto.AddComponent<WaveSpawnTelegraph>();
            telegraph.Inizializza(spritePixelTelegraph);
            poolTelegraph[i] = telegraph;
            oggetto.SetActive(false);
        }
    }

    private WaveSpawnTelegraph OttieniTelegraph()
    {
        if (poolTelegraph == null || poolTelegraph.Length == 0) return null;

        for (int i = 0; i < poolTelegraph.Length; i++)
        {
            int indice = (indiceRiutilizzoTelegraph + i) % poolTelegraph.Length;
            if (!poolTelegraph[indice].gameObject.activeSelf)
            {
                indiceRiutilizzoTelegraph = (indice + 1) % poolTelegraph.Length;
                return poolTelegraph[indice];
            }
        }

        WaveSpawnTelegraph riutilizzato = poolTelegraph[indiceRiutilizzoTelegraph];
        indiceRiutilizzoTelegraph =
            (indiceRiutilizzoTelegraph + 1) % poolTelegraph.Length;
        riutilizzato.Disattiva();
        return riutilizzato;
    }

    private void CreaAudioProcedurale()
    {
        if (sorgenteAudio == null)
        {
            sorgenteAudio = gameObject.AddComponent<AudioSource>();
            sorgenteAudio.playOnAwake = false;
            sorgenteAudio.loop = false;
            sorgenteAudio.spatialBlend = 0f;
            sorgenteAudio.dopplerLevel = 0f;
            sorgenteAudio.ignoreListenerPause = false;
        }
        if (clipSegnale == null) clipSegnale = CreaClipSegnale();
    }

    private static AudioClip CreaClipSegnale()
    {
        const int frequenzaCampionamento = 22050;
        const float durata = 0.24f;
        int campioni = Mathf.CeilToInt(frequenzaCampionamento * durata);
        float[] dati = new float[campioni];

        for (int i = 0; i < campioni; i++)
        {
            float tempo = (float)i / frequenzaCampionamento;
            bool secondaNota = tempo >= 0.115f;
            float tempoNota = secondaNota ? tempo - 0.115f : tempo;
            float frequenza = secondaNota ? 740f : 555f;
            float inviluppo = Mathf.Clamp01(tempoNota / 0.008f) *
                               Mathf.Clamp01((0.105f - tempoNota) / 0.035f);
            float ondaQuadra = Mathf.Sin(
                tempoNota * frequenza * Mathf.PI * 2f
            ) >= 0f ? 1f : -1f;
            float armonica = Mathf.Sin(
                tempoNota * frequenza * 2f * Mathf.PI * 2f
            );
            dati[i] = (ondaQuadra * 0.16f + armonica * 0.055f) * inviluppo;
        }

        AudioClip clip = AudioClip.Create(
            "SegnaleUltimiNemici",
            campioni,
            1,
            frequenzaCampionamento,
            false
        );
        clip.hideFlags = HideFlags.HideAndDontSave;
        clip.SetData(dati, 0);
        return clip;
    }

    private void ProvaCostruireInterfaccia()
    {
        if (radiceInterfaccia != null) return;

        GameObject interfaccia = GameObject.Find("Interfaccia");
        if (interfaccia == null || interfaccia.GetComponent<Canvas>() == null)
        {
            return;
        }

        GameObject radice = new GameObject(
            "LeggibilitaOndate",
            typeof(RectTransform),
            typeof(CanvasGroup)
        );
        radice.transform.SetParent(interfaccia.transform, false);
        radice.transform.SetAsLastSibling();
        RectTransform radiceRect = radice.GetComponent<RectTransform>();
        radiceRect.anchorMin = Vector2.zero;
        radiceRect.anchorMax = Vector2.one;
        radiceRect.offsetMin = Vector2.zero;
        radiceRect.offsetMax = Vector2.zero;
        CanvasGroup gruppo = radice.GetComponent<CanvasGroup>();
        gruppo.interactable = false;
        gruppo.blocksRaycasts = false;
        gruppo.ignoreParentGroups = false;
        radiceInterfaccia = radice;

        TMP_FontAsset font = null;
        TMP_Text riferimento = GameManager.TrovaTestoInterfaccia("OndataText");
        if (riferimento != null) font = riferimento.font;

        CostruisciSchedaHud(radice.transform, font);
        CostruisciIndicatori(radice.transform, font);
        CostruisciSegnale(radice.transform, font);
        AggiornaVisibilitaGenerale();
    }

    private void CostruisciSchedaHud(Transform parent, TMP_FontAsset font)
    {
        schedaHud = CreaPannello(
            "SchedaMinacceOndata",
            parent,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-12f, -12f),
            new Vector2(360f, 112f),
            false
        );

        FarmPixelUI.AggiungiIcona(
            schedaHud.transform,
            "IconaVolpe",
            FarmPixelIcon.Volpe,
            new Vector2(-151f, 22f),
            new Vector2(38f, 38f)
        );

        CreaTesto(
            "TitoloMinacce",
            schedaHud.transform,
            "VOLPI RIMASTE",
            font,
            new Vector2(24f, 35f),
            new Vector2(272f, 18f),
            15f,
            new Color32(255, 220, 143, 255),
            TextAlignmentOptions.Center,
            FontStyles.Bold
        );
        testoRimasti = CreaTesto(
            "ConteggioMinacce",
            schedaHud.transform,
            "RIMASTE  0 / 0",
            font,
            new Vector2(24f, 14f),
            new Vector2(272f, 24f),
            20f,
            new Color32(255, 181, 67, 255),
            TextAlignmentOptions.Center,
            FontStyles.Bold
        );
        testoGruppo = CreaTesto(
            "ConteggioGruppo",
            schedaHud.transform,
            string.Empty,
            font,
            new Vector2(-122f, -10f),
            new Vector2(90f, 18f),
            13f,
            new Color32(242, 211, 157, 255),
            TextAlignmentOptions.Center,
            FontStyles.Bold
        );

        GameObject fondoBarra = CreaPannello(
            "FondoBarraMinacce",
            schedaHud.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(28f, -10f),
            new Vector2(226f, 16f),
            true
        );

        segmentiBarra = new Image[SegmentiBarra];
        const float larghezza = 18f;
        const float spazio = 2f;
        float totale = SegmentiBarra * larghezza +
                       (SegmentiBarra - 1) * spazio;
        float primoX = -totale * 0.5f + larghezza * 0.5f;
        for (int i = 0; i < segmentiBarra.Length; i++)
        {
            GameObject segmento = new GameObject(
                "SegmentoBarra_" + i,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            segmento.transform.SetParent(fondoBarra.transform, false);
            RectTransform rect = segmento.GetComponent<RectTransform>();
            ImpostaRectCentrato(
                rect,
                new Vector2(primoX + i * (larghezza + spazio), 0f),
                new Vector2(larghezza, 9f)
            );
            Image immagine = segmento.GetComponent<Image>();
            immagine.raycastTarget = false;
            immagine.color = new Color32(76, 42, 27, 210);
            segmentiBarra[i] = immagine;
        }
        CreaChipTipi(schedaHud.transform, font);
        schedaHud.SetActive(false);
    }

    private void CreaChipTipi(Transform parent, TMP_FontAsset font)
    {
        chipTipi = new TMP_Text[NumeroTipiVolpe];
        sfondiChipTipi = new Image[NumeroTipiVolpe];
        const float larghezza = 40f;
        const float spazio = 3f;
        float totale = chipTipi.Length * larghezza +
                       (chipTipi.Length - 1) * spazio;
        float primoX = -totale * 0.5f + larghezza * 0.5f;

        for (int i = 0; i < chipTipi.Length; i++)
        {
            TipoVolpe tipo = (TipoVolpe)i;
            conteggiTipiVisualizzati[i] = -1;
            GameObject pannello = CreaPannello(
                "ChipTipo_" + tipo,
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(primoX + i * (larghezza + spazio), -40f),
                new Vector2(larghezza, 22f),
                true
            );
            Image sfondo = pannello.GetComponent<Image>();
            sfondo.raycastTarget = false;
            sfondiChipTipi[i] = sfondo;

            chipTipi[i] = CreaTesto(
                "TestoChip_" + tipo,
                pannello.transform,
                FoxVariantStyle.Abbreviazione(tipo) + "  0",
                font,
                Vector2.zero,
                new Vector2(larghezza - 2f, 19f),
                12f,
                new Color32(255, 247, 216, 255),
                TextAlignmentOptions.Center,
                FontStyles.Bold
            );
        }
        AggiornaChipTipi(default);
    }

    private void CostruisciIndicatori(Transform parent, TMP_FontAsset font)
    {
        indicatori = new IndicatoreSettore[NumeroSettori];
        Vector2[] ancore =
        {
            new Vector2(0.945f, 0.50f),
            new Vector2(0.70f, 0.78f),
            new Vector2(0.50f, 0.90f),
            new Vector2(0.30f, 0.78f),
            new Vector2(0.055f, 0.50f),
            new Vector2(0.105f, 0.135f),
            new Vector2(0.50f, 0.075f),
            new Vector2(0.895f, 0.135f)
        };
        spriteFreccia = CreaSpriteFreccia();

        for (int i = 0; i < indicatori.Length; i++)
        {
            GameObject pannello = CreaPannello(
                "IndicatoreSpawnSettore_" + i,
                parent,
                ancore[i],
                ancore[i],
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(84f, 36f),
                false
            );

            GameObject freccia = new GameObject(
                "Freccia",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            freccia.transform.SetParent(pannello.transform, false);
            RectTransform frecciaRect = freccia.GetComponent<RectTransform>();
            ImpostaRectCentrato(
                frecciaRect,
                new Vector2(-25f, 0f),
                new Vector2(22f, 22f)
            );
            frecciaRect.localRotation = Quaternion.Euler(0f, 0f, i * 45f);
            Image immagineFreccia = freccia.GetComponent<Image>();
            immagineFreccia.sprite = spriteFreccia;
            immagineFreccia.preserveAspect = true;
            immagineFreccia.raycastTarget = false;
            immagineFreccia.color = new Color32(255, 190, 65, 255);

            TMP_Text quantita = CreaTesto(
                "Quantita",
                pannello.transform,
                "x1",
                font,
                new Vector2(13f, 0f),
                new Vector2(52f, 28f),
                18f,
                new Color32(255, 229, 169, 255),
                TextAlignmentOptions.Center,
                FontStyles.Bold
            );
            quantita.enableAutoSizing = true;
            quantita.fontSizeMin = 12f;
            quantita.fontSizeMax = 18f;

            indicatori[i] = new IndicatoreSettore
            {
                radice = pannello,
                sfondo = pannello.GetComponent<Image>(),
                freccia = frecciaRect,
                immagineFreccia = immagineFreccia,
                testoQuantita = quantita
            };
            pannello.SetActive(false);
        }
    }

    private void CostruisciSegnale(Transform parent, TMP_FontAsset font)
    {
        pannelloSegnale = CreaPannello(
            "SegnaleUltimiNemici",
            parent,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -82f),
            new Vector2(300f, 50f),
            false
        );
        gruppoSegnale = pannelloSegnale.AddComponent<CanvasGroup>();
        gruppoSegnale.interactable = false;
        gruppoSegnale.blocksRaycasts = false;

        FarmPixelUI.AggiungiIcona(
            pannelloSegnale.transform,
            "VolpeSinistra",
            FarmPixelIcon.Volpe,
            new Vector2(-126f, 0f),
            new Vector2(28f, 28f)
        );
        FarmPixelUI.AggiungiIcona(
            pannelloSegnale.transform,
            "VolpeDestra",
            FarmPixelIcon.Volpe,
            new Vector2(126f, 0f),
            new Vector2(28f, 28f)
        );
        testoSegnale = CreaTesto(
            "TestoSegnale",
            pannelloSegnale.transform,
            "ULTIME VOLPI!",
            font,
            Vector2.zero,
            new Vector2(230f, 36f),
            22f,
            new Color32(255, 182, 63, 255),
            TextAlignmentOptions.Center,
            FontStyles.Bold
        );
        NascondiSegnale();
    }

    private static GameObject CreaPannello(
        string nome,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 posizione,
        Vector2 dimensioni,
        bool incassato
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        oggetto.transform.SetParent(parent, false);
        RectTransform rect = oggetto.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;

        Image immagine = oggetto.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(immagine, incassato, false);
        Color colore = immagine.color;
        colore.a = incassato ? 0.48f : 0.66f;
        immagine.color = colore;

        Outline bordo = immagine.GetComponent<Outline>();
        if (bordo != null)
        {
            Color coloreBordo = bordo.effectColor;
            coloreBordo.a = incassato ? 0.34f : 0.48f;
            bordo.effectColor = coloreBordo;
            bordo.effectDistance = new Vector2(1f, 1f);
        }
        return oggetto;
    }

    private static TMP_Text CreaTesto(
        string nome,
        Transform parent,
        string contenuto,
        TMP_FontAsset font,
        Vector2 posizione,
        Vector2 dimensioni,
        float dimensioneFont,
        Color colore,
        TextAlignmentOptions allineamento,
        FontStyles stile
    )
    {
        GameObject oggetto = new GameObject(
            nome,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        oggetto.transform.SetParent(parent, false);
        RectTransform rect = oggetto.GetComponent<RectTransform>();
        ImpostaRectCentrato(rect, posizione, dimensioni);

        TextMeshProUGUI testo = oggetto.GetComponent<TextMeshProUGUI>();
        if (font != null) testo.font = font;
        testo.text = contenuto;
        testo.fontSize = dimensioneFont;
        testo.fontStyle = stile;
        testo.alignment = allineamento;
        testo.textWrappingMode = TextWrappingModes.NoWrap;
        testo.overflowMode = TextOverflowModes.Ellipsis;
        testo.raycastTarget = false;
        FarmPixelUI.ApplicaTesto(testo, colore);
        return testo;
    }

    private static void ImpostaRectCentrato(
        RectTransform rect,
        Vector2 posizione,
        Vector2 dimensioni
    )
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posizione;
        rect.sizeDelta = dimensioni;
    }

    private static Sprite CreaSpritePixelMondo()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.name = "PixelTelegraphSpawn";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            8f
        );
        sprite.name = "PixelTelegraphSpawn";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static Sprite CreaSpriteFreccia()
    {
        const int larghezza = 9;
        const int altezza = 7;
        Color32 trasparente = new Color32(0, 0, 0, 0);
        Color32 chiaro = new Color32(255, 255, 255, 255);
        Color32[] pixel = new Color32[larghezza * altezza];
        for (int i = 0; i < pixel.Length; i++) pixel[i] = trasparente;

        for (int x = 1; x <= 6; x++) pixel[3 * larghezza + x] = chiaro;
        pixel[2 * larghezza + 5] = chiaro;
        pixel[2 * larghezza + 6] = chiaro;
        pixel[1 * larghezza + 6] = chiaro;
        pixel[1 * larghezza + 7] = chiaro;
        pixel[0 * larghezza + 7] = chiaro;
        pixel[4 * larghezza + 5] = chiaro;
        pixel[4 * larghezza + 6] = chiaro;
        pixel[5 * larghezza + 6] = chiaro;
        pixel[5 * larghezza + 7] = chiaro;
        pixel[6 * larghezza + 7] = chiaro;

        Texture2D texture = new Texture2D(
            larghezza,
            altezza,
            TextureFormat.RGBA32,
            false
        );
        texture.name = "UI_FrecciaSpawn";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels32(pixel);
        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, larghezza, altezza),
            new Vector2(0.5f, 0.5f),
            8f
        );
        sprite.name = "UI_FrecciaSpawn";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    void OnDisable()
    {
        TerminaOnda();
    }

    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.ProgressoCambiato -= ProgressoCambiato;
        }
        if (clipSegnale != null)
        {
            Destroy(clipSegnale);
            clipSegnale = null;
        }
        if (radiceInterfaccia != null)
        {
            Destroy(radiceInterfaccia);
            radiceInterfaccia = null;
        }
        DistruggiSpriteRuntime(ref spritePixelTelegraph);
        DistruggiSpriteRuntime(ref spriteFreccia);
    }

    private static void DistruggiSpriteRuntime(ref Sprite sprite)
    {
        if (sprite == null) return;

        Texture2D texture = sprite.texture;
        Destroy(sprite);
        if (texture != null) Destroy(texture);
        sprite = null;
    }
}

/// <summary>
/// Singolo elemento del pool di polvere. Usa solo tempo scalato e geometria
/// deterministica, cosi pausa e preavviso restano sincronizzati.
/// </summary>
internal sealed class WaveSpawnTelegraph : MonoBehaviour
{
    private const int NumeroParticelle = 12;

    private SpriteRenderer[] particelle;
    private int token;
    private int slotId;
    private TipoVolpe tipo;
    private float durata;
    private float tempo;

    public int Token => token;
    public int SlotId => slotId;
    public TipoVolpe Tipo => tipo;
    public bool Attivo => gameObject.activeSelf;

    public void Inizializza(Sprite spritePixel)
    {
        if (particelle != null) return;
        particelle = new SpriteRenderer[NumeroParticelle];

        for (int i = 0; i < particelle.Length; i++)
        {
            GameObject pixel = new GameObject("ZollaPixel_" + i);
            pixel.transform.SetParent(transform, false);
            SpriteRenderer renderer = pixel.AddComponent<SpriteRenderer>();
            renderer.sprite = spritePixel;
            renderer.sortingOrder = -1;
            renderer.color = ColoreBase(i, 1f, TipoVolpe.Comune);
            particelle[i] = renderer;
        }
    }

    public void Attiva(
        int nuovoToken,
        int nuovoSlotId,
        Vector2 posizione,
        float nuovaDurata,
        TipoVolpe nuovoTipo
    )
    {
        token = nuovoToken;
        slotId = nuovoSlotId;
        tipo = FoxVariantStyle.Normalizza(nuovoTipo);
        durata = Mathf.Max(0.05f, nuovaDurata);
        tempo = 0f;
        transform.position = new Vector3(posizione.x, posizione.y, 0f);
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
        AggiornaAspetto(0f);
    }

    void Update()
    {
        tempo += Time.deltaTime;
        float t = Mathf.Clamp01(tempo / durata);
        AggiornaAspetto(t);
        if (tempo >= durata) Disattiva();
    }

    private void AggiornaAspetto(float t)
    {
        if (particelle == null) return;

        float curva = t * t * (3f - 2f * t);
        float alpha = Mathf.Sin(Mathf.PI * t);
        float raggioEsterno = Mathf.Lerp(0.64f, 0.22f, curva);

        for (int i = 0; i < particelle.Length; i++)
        {
            SpriteRenderer renderer = particelle[i];
            if (renderer == null) continue;

            bool esterna = i < 8;
            float angolo = esterna
                ? i * 45f + Mathf.Sin(t * Mathf.PI) * (i % 2 == 0 ? 7f : -7f)
                : 45f + (i - 8) * 90f;
            float radianti = angolo * Mathf.Deg2Rad;
            float raggio = esterna
                ? raggioEsterno
                : Mathf.Lerp(0.18f, 0.34f, curva);
            renderer.transform.localPosition = new Vector3(
                Mathf.Cos(radianti) * raggio,
                Mathf.Sin(radianti) * raggio * 0.58f,
                0f
            );

            float scala = esterna
                ? Mathf.Lerp(1.3f, 0.82f, curva)
                : Mathf.Lerp(0.72f, 1.05f, curva);
            renderer.transform.localScale = Vector3.one * scala;
            renderer.color = ColoreBase(i, alpha, tipo);
        }
    }

    private static Color ColoreBase(
        int indice,
        float alpha,
        TipoVolpe tipo
    )
    {
        Color colore;
        switch (indice % 4)
        {
            case 0:
                colore = new Color32(116, 70, 34, 255);
                break;
            case 1:
                colore = new Color32(177, 111, 47, 255);
                break;
            case 2:
                colore = new Color32(222, 164, 73, 255);
                break;
            default:
                colore = new Color32(145, 88, 39, 255);
                break;
        }
        if (indice % 4 == 0)
        {
            colore = Color.Lerp(
                colore,
                FoxVariantStyle.ColoreUi(tipo),
                tipo == TipoVolpe.Comune ? 0.18f : 0.68f
            );
        }
        colore.a = Mathf.Clamp01(alpha);
        return colore;
    }

    public void TerminaSeCoincide(int tokenAtteso, int slotAtteso)
    {
        if (token == tokenAtteso && slotId == slotAtteso) Disattiva();
    }

    public void Disattiva()
    {
        tempo = 0f;
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }
}
