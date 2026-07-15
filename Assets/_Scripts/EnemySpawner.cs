using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class Wave
{
    public string nomeOndata;
    [Min(0)] public int numeroNemici;
    [Tooltip("Ordine deterministico dei tipi. Se assente, usa volpi comuni.")]
    public TipoVolpe[] sequenzaVolpi;
    [Header("Ritmo gruppi")]
    [Min(0.05f)] public float intervalloTraNemici = 1f;
    [Range(1, 4)] public int dimensioneMassimaGruppo = 2;
    [Min(0.05f)] public float intervalloTraGruppi = 2f;

    [Header("Bonus")]
    [Min(0)] public int numeroMaialiniBonus;
    [Min(1)] public int vitaMaialinoBonus = 1;
    [Min(0)] public int moneteMaialinoBonus = 3;
}

public readonly struct AnteprimaOndata
{
    public int Indice { get; }
    public int Totale { get; }
    public string Nome { get; }
    public int NumeroVolpi { get; }
    public int VitaVolpi { get; }
    public int VitaBaseVolpi => VitaVolpi;
    public ComposizioneVolpi Composizione { get; }
    public int NumeroMaialini { get; }
    public int VitaMaialino { get; }
    public int MoneteMaialino { get; }
    public int NumeroGruppi { get; }
    public bool Valida => Indice > 0 && Totale >= Indice;

    public AnteprimaOndata(
        int indice,
        int totale,
        string nome,
        int numeroVolpi,
        int vitaVolpi,
        ComposizioneVolpi composizione,
        int numeroMaialini,
        int vitaMaialino,
        int moneteMaialino,
        int numeroGruppi
    )
    {
        Indice = indice;
        Totale = totale;
        Nome = nome;
        NumeroVolpi = numeroVolpi;
        VitaVolpi = vitaVolpi;
        Composizione = composizione;
        NumeroMaialini = numeroMaialini;
        VitaMaialino = vitaMaialino;
        MoneteMaialino = moneteMaialino;
        NumeroGruppi = numeroGruppi;
    }
}

public readonly struct ProgressoOndata
{
    public int Token { get; }
    public int Indice { get; }
    public int TotaleOndate { get; }
    public string Nome { get; }
    public int VolpiTotali { get; }
    public int VolpiDaSpawnare { get; }
    public int MinacceAttive { get; }
    public int VolpiRimaste => VolpiDaSpawnare + MinacceAttive;
    public ComposizioneVolpi ComposizioneTotale { get; }
    public ComposizioneVolpi ComposizioneRimasta { get; }
    public int GruppoCorrente { get; }
    public int TotaleGruppi { get; }
    public bool SpawnTerminato { get; }
    public bool OndaAttiva { get; }

    public ProgressoOndata(
        int token,
        int indice,
        int totaleOndate,
        string nome,
        int volpiTotali,
        int volpiDaSpawnare,
        int minacceAttive,
        ComposizioneVolpi composizioneTotale,
        ComposizioneVolpi composizioneRimasta,
        int gruppoCorrente,
        int totaleGruppi,
        bool spawnTerminato,
        bool ondaAttiva
    )
    {
        Token = token;
        Indice = indice;
        TotaleOndate = totaleOndate;
        Nome = nome;
        VolpiTotali = volpiTotali;
        VolpiDaSpawnare = volpiDaSpawnare;
        MinacceAttive = minacceAttive;
        ComposizioneTotale = composizioneTotale;
        ComposizioneRimasta = composizioneRimasta;
        GruppoCorrente = gruppoCorrente;
        TotaleGruppi = totaleGruppi;
        SpawnTerminato = spawnTerminato;
        OndaAttiva = ondaAttiva;
    }
}

public class EnemySpawner : MonoBehaviour
{
    public GameObject foxPrefab;
    public GameObject pigPrefab;
    public float spawnDistance = 10f;
    public float distanzaSpawnMaialino = 5.2f;

    [Header("Vita nemici")]
    [Min(1)] public int vitaPrimaOndata = 2;
    [Min(0)] public int vitaAggiuntivaPerOndata = 1;

    public Wave[] ondate;

    [Header("Diagnostica sviluppo")]
    [SerializeField] private WaveRuntimeDiagnostics diagnostica;

    private readonly HashSet<EnemyAI> minacceAttive =
        new HashSet<EnemyAI>();

    private int currentWaveIndex = 0;
    private int tokenOndata;
    private TMP_Text testoOndata;
    private TMP_Text messaggioOndata;
    private GameObject contenitoreMessaggio;
    private Transform giocatore;
    private float durataBannerOndata = 0.85f;
    private float intervalloControlloFineOndata = 0.2f;
    private float durataMinimaDistribuzioneMaialini = 1f;
    private float durataPreavvisoSpawn = 0.5f;
    private int nemiciDaSpawnare;
    private int totaleNemiciOnda;
    private int gruppoCorrente;
    private int totaleGruppi;
    private ComposizioneVolpi composizioneTotaleOnda;
    private ComposizioneVolpi composizioneDaSpawnare;
    private bool spawnTerminato;
    private bool ondaAttiva;
    private bool avvioRapidoRichiesto;
    private WaveReadabilityController leggibilita;

    public WaveRuntimeDiagnostics Diagnostica => diagnostica;
    public int IndiceOndaCorrente => currentWaveIndex;
    public int TotaleOndate => ondate != null ? ondate.Length : 0;
    public int NemiciDaSpawnare => nemiciDaSpawnare;
    public int MinacceAttive => minacceAttive.Count;
    public int NemiciRimasti => nemiciDaSpawnare + minacceAttive.Count;
    public int TotaleNemiciOnda => totaleNemiciOnda;
    public ComposizioneVolpi ComposizioneTotaleOnda =>
        composizioneTotaleOnda;
    public ComposizioneVolpi ComposizioneRimasta =>
        composizioneDaSpawnare + ContaComposizioneAttiva();
    public bool SpawnTerminato => spawnTerminato;
    public bool OndaAttiva => ondaAttiva;
    public IEnumerable<EnemyAI> MinacceOnda => minacceAttive;
    public WaveReadabilityController Leggibilita => leggibilita;
    public ProgressoOndata ProgressoCorrente => CreaProgressoCorrente();
    public AnteprimaOndata AnteprimaCorrente =>
        OttieniAnteprima(currentWaveIndex);

    public event System.Action<ProgressoOndata> ProgressoCambiato;

    void Start()
    {
        ApplicaConfigurazioneBilanciamento();
        VerificaSequenzeConfigurate();
        ConfiguraDiagnostica();

        testoOndata = GameManager.TrovaTestoInterfaccia("OndataText");
        messaggioOndata =
            GameManager.TrovaTestoInterfaccia("MessaggioOndataText");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            giocatore = player.transform;
        }

        ConfiguraPannelloMessaggio();
        NascondiMessaggio();
        ConfiguraLeggibilita();
        StartCoroutine(GestoreOndate());
    }

    IEnumerator GestoreOndate()
    {
        if (ondate == null || ondate.Length == 0)
        {
            yield break;
        }

        while (currentWaveIndex < ondate.Length)
        {
            if (PartitaTerminata())
            {
                diagnostica.TerminaOndata(
                    EsitoDiagnosticaOndata.Sconfitta
                );
                PulisciEntitaTraOndate();
                yield break;
            }

            int tokenCorrente = ++tokenOndata;
            Wave ondataCorrente = ondate[currentWaveIndex];
            AnteprimaOndata anteprima =
                OttieniAnteprima(currentWaveIndex);

            IniziaStatoOnda(ondataCorrente, anteprima);
            diagnostica.IniziaOndata(
                currentWaveIndex + 1,
                ondate.Length,
                ondataCorrente.nomeOndata,
                ondataCorrente.numeroNemici,
                ondataCorrente.numeroMaialiniBonus
            );
            AggiornaContatoreOndata();
            if (leggibilita != null)
            {
                leggibilita.IniziaOnda(anteprima);
            }
            NotificaProgresso();

            MostraMessaggio(CreaTestoBanner(anteprima));
            float durataBanner = avvioRapidoRichiesto
                ? Mathf.Min(0.18f, durataBannerOndata)
                : durataBannerOndata;
            avvioRapidoRichiesto = false;
            yield return AttendiConToken(durataBanner, tokenCorrente);

            if (!TokenValido(tokenCorrente))
            {
                InterrompiStatoOnda();
                PulisciEntitaTraOndate();
                yield break;
            }

            NascondiMessaggio();
            diagnostica.AvviaCombattimento();

            if (ondataCorrente.numeroMaialiniBonus > 0)
            {
                StartCoroutine(
                    SpawnMaialiniGraduali(ondataCorrente, tokenCorrente)
                );
            }

            int indiceSpawn = 0;
            for (int gruppo = 0; gruppo < totaleGruppi; gruppo++)
            {
                gruppoCorrente = gruppo + 1;
                NotificaProgresso();

                int dimensioneGruppo = CalcolaDimensioneGruppo(
                    ondataCorrente.numeroNemici,
                    totaleGruppi,
                    gruppo
                );

                for (int membro = 0; membro < dimensioneGruppo; membro++)
                {
                    if (!TokenValido(tokenCorrente))
                    {
                        diagnostica.TerminaOndata(
                            EsitoDiagnosticaOndata.Sconfitta
                        );
                        InterrompiStatoOnda();
                        PulisciEntitaTraOndate();
                        yield break;
                    }

                    Vector2 posizioneSpawn =
                        DirezioneCasuale() * spawnDistance;
                    int slotSpawn = indiceSpawn + 1;
                    TipoVolpe tipoSpawn = OttieniTipoVolpe(
                        ondataCorrente,
                        indiceSpawn
                    );

                    if (leggibilita != null)
                    {
                        leggibilita.PreavvisaSpawn(
                            tokenCorrente,
                            slotSpawn,
                            posizioneSpawn,
                            durataPreavvisoSpawn,
                            tipoSpawn
                        );
                    }

                    yield return AttendiConToken(
                        durataPreavvisoSpawn,
                        tokenCorrente
                    );

                    if (leggibilita != null)
                    {
                        leggibilita.TerminaPreavviso(
                            tokenCorrente,
                            slotSpawn
                        );
                    }

                    if (!TokenValido(tokenCorrente))
                    {
                        diagnostica.TerminaOndata(
                            EsitoDiagnosticaOndata.Sconfitta
                        );
                        InterrompiStatoOnda();
                        PulisciEntitaTraOndate();
                        yield break;
                    }

                    bool riproduciVerso =
                        tipoSpawn != TipoVolpe.Comune || membro == 0;
                    EnemyAI nemico = SpawnEnemy(
                        posizioneSpawn,
                        tipoSpawn,
                        indiceSpawn,
                        riproduciVerso
                    );
                    nemiciDaSpawnare = Mathf.Max(0, nemiciDaSpawnare - 1);
                    composizioneDaSpawnare =
                        composizioneDaSpawnare.Rimuovi(tipoSpawn);
                    if (nemico != null)
                    {
                        RegistraMinaccia(nemico);
                    }
                    diagnostica.RegistraSpawnNemico(nemico != null);
                    indiceSpawn++;
                    NotificaProgresso();

                    bool ultimoSpawn =
                        indiceSpawn >= ondataCorrente.numeroNemici;
                    if (ultimoSpawn) continue;

                    bool ultimoNelGruppo =
                        membro >= dimensioneGruppo - 1;
                    float intervalloFinoAlProssimo = ultimoNelGruppo
                        ? ondataCorrente.intervalloTraGruppi
                        : ondataCorrente.intervalloTraNemici;
                    float attesaPrimaDelProssimoPreavviso = Mathf.Max(
                        0f,
                        intervalloFinoAlProssimo - durataPreavvisoSpawn
                    );
                    yield return AttendiConToken(
                        attesaPrimaDelProssimoPreavviso,
                        tokenCorrente
                    );
                }
            }

            nemiciDaSpawnare = 0;
            composizioneDaSpawnare = default;
            spawnTerminato = true;
            NotificaProgresso();
            diagnostica.SegnaFineSpawn();

            while (GameObject.FindGameObjectWithTag("Nemico") != null)
            {
                diagnostica.CampionaNemiciVivi();
                if (PartitaTerminata())
                {
                    diagnostica.TerminaOndata(
                        EsitoDiagnosticaOndata.Sconfitta
                    );
                    InterrompiStatoOnda();
                    PulisciEntitaTraOndate();
                    yield break;
                }

                yield return AttendiConToken(
                    intervalloControlloFineOndata,
                    tokenCorrente
                );
            }

            if (PartitaTerminata())
            {
                diagnostica.TerminaOndata(
                    EsitoDiagnosticaOndata.Sconfitta
                );
                InterrompiStatoOnda();
                PulisciEntitaTraOndate();
                yield break;
            }

            diagnostica.TerminaOndata(
                EsitoDiagnosticaOndata.Completata
            );
            ConcludiStatoOnda();

            if (currentWaveIndex < ondate.Length - 1)
            {
                tokenOndata++;
                PulisciEntitaTraOndate();

                // Lascia completare le distruzioni prima di fermare il tempo.
                yield return null;

                if (PartitaTerminata()) yield break;
                PulisciEntitaTraOndate();

                if (GameManager.instance != null)
                {
                    GameManager.instance.IniziaIntervallo(
                        currentWaveIndex + 1,
                        ondate.Length,
                        OttieniAnteprima(currentWaveIndex + 1)
                    );

                    while (!PartitaTerminata() &&
                           GameManager.instance.StatoCorrente ==
                           StatoPartita.Intervallo)
                    {
                        yield return null;
                    }
                }
            }

            currentWaveIndex++;
        }

        tokenOndata++;
        PulisciEntitaTraOndate();
        NascondiMessaggio();

        if (GameManager.instance != null)
        {
            GameManager.instance.Vittoria();
        }
    }

    IEnumerator SpawnMaialiniGraduali(Wave ondata, int tokenCorrente)
    {
        int quantita = Mathf.Max(0, ondata.numeroMaialiniBonus);
        float durataSpawnVolpi = Mathf.Max(
            durataMinimaDistribuzioneMaialini,
            CalcolaDurataSpawnVolpi(ondata)
        );
        float tempoPrecedente = 0f;

        for (int i = 0; i < quantita; i++)
        {
            float frazione = (i + 1f) / (quantita + 1f);
            float tempoSpawn = durataSpawnVolpi * frazione;
            yield return new WaitForSeconds(tempoSpawn - tempoPrecedente);
            tempoPrecedente = tempoSpawn;

            if (tokenCorrente != tokenOndata || PartitaTerminata())
            {
                yield break;
            }

            diagnostica.RegistraSpawnMaialino(SpawnMaialino(ondata));
        }
    }

    void ApplicaConfigurazioneBilanciamento()
    {
        GameBalanceConfig configurazione = GameBalanceConfig.Corrente;
        if (configurazione == null) return;

        FoxBalanceSettings volpe = configurazione.Volpe;
        if (volpe != null)
        {
            vitaPrimaOndata = Mathf.Max(1, volpe.vitaPrimaOndata);
            vitaAggiuntivaPerOndata = Mathf.Max(
                0,
                volpe.vitaAggiuntivaPerOndata
            );
        }

        WaveBalanceSettings ritmo = configurazione.Ondate;
        if (ritmo == null) return;

        spawnDistance = Mathf.Max(0f, ritmo.distanzaSpawnVolpi);
        distanzaSpawnMaialino = Mathf.Max(
            0f,
            ritmo.distanzaSpawnMaialini
        );
        durataBannerOndata = Mathf.Max(0f, ritmo.durataBannerOndata);
        intervalloControlloFineOndata = Mathf.Max(
            0.02f,
            ritmo.intervalloControlloFineOndata
        );
        durataMinimaDistribuzioneMaialini = Mathf.Max(
            0.05f,
            ritmo.durataMinimaDistribuzioneMaialini
        );
        durataPreavvisoSpawn = Mathf.Clamp(
            ritmo.durataPreavvisoSpawn,
            0.15f,
            1f
        );

        if (ritmo.ondate != null && ritmo.ondate.Length > 0)
        {
            ondate = ritmo.ondate;
        }
    }

    void ConfiguraDiagnostica()
    {
        if (diagnostica == null)
        {
            diagnostica = GetComponent<WaveRuntimeDiagnostics>();
        }
        if (diagnostica == null)
        {
            diagnostica = gameObject.AddComponent<WaveRuntimeDiagnostics>();
        }
    }

    void VerificaSequenzeConfigurate()
    {
        if (ondate == null) return;
        for (int i = 0; i < ondate.Length; i++)
        {
            Wave onda = ondate[i];
            if (onda == null || onda.sequenzaVolpi == null) continue;
            if (onda.sequenzaVolpi.Length == Mathf.Max(0, onda.numeroNemici))
            {
                continue;
            }
            Debug.LogWarning(
                "La sequenza tipi dell'onda " + (i + 1) +
                " non coincide con numeroNemici: gli slot mancanti " +
                "saranno volpi comuni e quelli extra verranno ignorati.",
                this
            );
        }
    }

    void ConfiguraLeggibilita()
    {
        if (leggibilita == null)
        {
            leggibilita = GetComponent<WaveReadabilityController>();
        }
        if (leggibilita == null)
        {
            leggibilita = gameObject.AddComponent<
                WaveReadabilityController
            >();
        }
        leggibilita.Configura(this);
    }

    public AnteprimaOndata OttieniAnteprima(int indiceZeroBased)
    {
        if (ondate == null ||
            indiceZeroBased < 0 ||
            indiceZeroBased >= ondate.Length ||
            ondate[indiceZeroBased] == null)
        {
            return default;
        }

        Wave onda = ondate[indiceZeroBased];
        int numeroVolpi = Mathf.Max(0, onda.numeroNemici);
        int numeroGruppi = CalcolaNumeroGruppi(
            numeroVolpi,
            onda.dimensioneMassimaGruppo
        );
        int vitaVolpi = Mathf.Max(
            1,
            vitaPrimaOndata +
            indiceZeroBased * vitaAggiuntivaPerOndata
        );
        ComposizioneVolpi composizione = CalcolaComposizione(onda);

        return new AnteprimaOndata(
            indiceZeroBased + 1,
            ondate.Length,
            string.IsNullOrWhiteSpace(onda.nomeOndata)
                ? "Ondata " + (indiceZeroBased + 1)
                : onda.nomeOndata,
            numeroVolpi,
            vitaVolpi,
            composizione,
            Mathf.Max(0, onda.numeroMaialiniBonus),
            Mathf.Max(1, onda.vitaMaialinoBonus),
            Mathf.Max(0, onda.moneteMaialinoBonus),
            numeroGruppi
        );
    }

    public void RichiediAvvioRapido()
    {
        avvioRapidoRichiesto = true;
    }

    void IniziaStatoOnda(Wave onda, AnteprimaOndata anteprima)
    {
        SganciaMinacce();
        totaleNemiciOnda = anteprima.NumeroVolpi;
        nemiciDaSpawnare = anteprima.NumeroVolpi;
        composizioneTotaleOnda = anteprima.Composizione;
        composizioneDaSpawnare = anteprima.Composizione;
        gruppoCorrente = 0;
        totaleGruppi = anteprima.NumeroGruppi;
        spawnTerminato = totaleNemiciOnda == 0;
        ondaAttiva = true;
    }

    void ConcludiStatoOnda()
    {
        bool statoCambiato =
            ondaAttiva ||
            nemiciDaSpawnare > 0 ||
            minacceAttive.Count > 0;
        ondaAttiva = false;
        nemiciDaSpawnare = 0;
        composizioneDaSpawnare = default;
        spawnTerminato = true;
        gruppoCorrente = totaleGruppi;
        SganciaMinacce();
        if (statoCambiato) NotificaProgresso();

        if (leggibilita != null)
        {
            leggibilita.TerminaOnda();
        }
    }

    void InterrompiStatoOnda()
    {
        ConcludiStatoOnda();
        NascondiMessaggio();
    }

    void RegistraMinaccia(EnemyAI nemico)
    {
        if (nemico == null || !minacceAttive.Add(nemico)) return;
        nemico.NonPiuMinaccia += NemicoNonPiuMinaccia;
    }

    void NemicoNonPiuMinaccia(EnemyAI nemico)
    {
        if (!minacceAttive.Remove(nemico)) return;
        if (nemico != null)
        {
            nemico.NonPiuMinaccia -= NemicoNonPiuMinaccia;
        }
        NotificaProgresso();
    }

    void SganciaMinacce()
    {
        foreach (EnemyAI nemico in minacceAttive)
        {
            if (nemico != null)
            {
                nemico.NonPiuMinaccia -= NemicoNonPiuMinaccia;
            }
        }
        minacceAttive.Clear();
    }

    ComposizioneVolpi ContaComposizioneAttiva()
    {
        ComposizioneVolpi composizione = default;
        foreach (EnemyAI nemico in minacceAttive)
        {
            if (nemico != null && !nemico.IsDead)
            {
                composizione = composizione.Aggiungi(nemico.Tipo);
            }
        }
        return composizione;
    }

    ProgressoOndata CreaProgressoCorrente()
    {
        string nome = string.Empty;
        if (ondate != null &&
            currentWaveIndex >= 0 &&
            currentWaveIndex < ondate.Length &&
            ondate[currentWaveIndex] != null)
        {
            nome = ondate[currentWaveIndex].nomeOndata;
        }

        int totale = ondate != null ? ondate.Length : 0;
        int indiceVisualizzato = totale > 0
            ? Mathf.Clamp(currentWaveIndex + 1, 1, totale)
            : 0;

        return new ProgressoOndata(
            tokenOndata,
            indiceVisualizzato,
            totale,
            nome,
            totaleNemiciOnda,
            nemiciDaSpawnare,
            minacceAttive.Count,
            composizioneTotaleOnda,
            ComposizioneRimasta,
            gruppoCorrente,
            totaleGruppi,
            spawnTerminato,
            ondaAttiva
        );
    }

    void NotificaProgresso()
    {
        ProgressoCambiato?.Invoke(CreaProgressoCorrente());
    }

    public TipoVolpe OttieniTipoConfigurato(
        int indiceOndaZeroBased,
        int indiceSpawnZeroBased
    )
    {
        if (ondate == null ||
            indiceOndaZeroBased < 0 ||
            indiceOndaZeroBased >= ondate.Length)
        {
            return TipoVolpe.Comune;
        }
        return OttieniTipoVolpe(
            ondate[indiceOndaZeroBased],
            indiceSpawnZeroBased
        );
    }

    public static TipoVolpe OttieniTipoVolpe(
        Wave onda,
        int indiceSpawnZeroBased
    )
    {
        if (onda == null ||
            indiceSpawnZeroBased < 0 ||
            indiceSpawnZeroBased >= Mathf.Max(0, onda.numeroNemici) ||
            onda.sequenzaVolpi == null ||
            indiceSpawnZeroBased >= onda.sequenzaVolpi.Length)
        {
            return TipoVolpe.Comune;
        }
        return FoxVariantStyle.Normalizza(
            onda.sequenzaVolpi[indiceSpawnZeroBased]
        );
    }

    public static ComposizioneVolpi CalcolaComposizione(Wave onda)
    {
        ComposizioneVolpi composizione = default;
        int totale = onda != null ? Mathf.Max(0, onda.numeroNemici) : 0;
        for (int i = 0; i < totale; i++)
        {
            composizione = composizione.Aggiungi(
                OttieniTipoVolpe(onda, i)
            );
        }
        return composizione;
    }

    string CreaTestoBanner(AnteprimaOndata anteprima)
    {
        string bonus = anteprima.NumeroMaialini > 0
            ? anteprima.NumeroMaialini + " MAIALINI BONUS"
            : "NESSUN MAIALINO BONUS";

        return
            "ONDATA " + anteprima.Indice + " / " + anteprima.Totale +
            "\n" + anteprima.Nome.ToUpperInvariant() +
            "\n" + anteprima.Composizione.FormattaCompatta() +
            "\n" + bonus;
    }

    IEnumerator AttendiConToken(float durata, int tokenCorrente)
    {
        float tempoRimasto = Mathf.Max(0f, durata);
        while (tempoRimasto > 0f && TokenValido(tokenCorrente))
        {
            tempoRimasto -= Time.deltaTime;
            yield return null;
        }
    }

    bool TokenValido(int tokenCorrente)
    {
        return tokenCorrente == tokenOndata &&
               isActiveAndEnabled &&
               !PartitaTerminata();
    }

    static int CalcolaNumeroGruppi(int numeroNemici, int massimoGruppo)
    {
        int quantita = Mathf.Max(0, numeroNemici);
        if (quantita == 0) return 0;

        return Mathf.CeilToInt(
            quantita / (float)Mathf.Clamp(massimoGruppo, 1, 4)
        );
    }

    static int CalcolaDimensioneGruppo(
        int numeroNemici,
        int numeroGruppi,
        int indiceGruppo
    )
    {
        if (numeroNemici <= 0 || numeroGruppi <= 0) return 0;

        int baseGruppo = numeroNemici / numeroGruppi;
        int eccedenza = numeroNemici % numeroGruppi;
        return baseGruppo + (indiceGruppo < eccedenza ? 1 : 0);
    }

    float CalcolaDurataSpawnVolpi(Wave onda)
    {
        if (onda == null || onda.numeroNemici <= 1) return 0f;

        int gruppi = CalcolaNumeroGruppi(
            onda.numeroNemici,
            onda.dimensioneMassimaGruppo
        );
        int intervalliInterni = Mathf.Max(0, onda.numeroNemici - gruppi);
        int intervalliTraGruppi = Mathf.Max(0, gruppi - 1);
        return
            intervalliInterni * Mathf.Max(
                onda.intervalloTraNemici,
                durataPreavvisoSpawn
            ) +
            intervalliTraGruppi * Mathf.Max(
                onda.intervalloTraGruppi,
                durataPreavvisoSpawn
            );
    }

    void AggiornaContatoreOndata()
    {
        if (testoOndata != null)
        {
            testoOndata.text =
                "Ondata   " + (currentWaveIndex + 1) + " / " + ondate.Length;
        }
    }

    void ConfiguraPannelloMessaggio()
    {
        if (messaggioOndata == null) return;

        RectTransform testoRect = messaggioOndata.rectTransform;
        Transform canvas = testoRect.parent;
        int indice = testoRect.GetSiblingIndex();

        contenitoreMessaggio = new GameObject(
            "PannelloMessaggioOndata",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Shadow)
        );
        contenitoreMessaggio.transform.SetParent(canvas, false);
        contenitoreMessaggio.transform.SetSiblingIndex(indice);

        RectTransform pannelloRect =
            contenitoreMessaggio.GetComponent<RectTransform>();
        pannelloRect.anchorMin = new Vector2(0.5f, 0.5f);
        pannelloRect.anchorMax = new Vector2(0.5f, 0.5f);
        pannelloRect.pivot = new Vector2(0.5f, 0.5f);
        pannelloRect.anchoredPosition = Vector2.zero;
        pannelloRect.sizeDelta = new Vector2(690f, 174f);

        Image sfondo = contenitoreMessaggio.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(sfondo, false, false);

        Shadow ombra = contenitoreMessaggio.GetComponent<Shadow>();
        ombra.effectColor = new Color(0.08f, 0.035f, 0.018f, 0.88f);
        ombra.effectDistance = new Vector2(5f, -5f);
        ombra.useGraphicAlpha = true;

        GameObject interno = new GameObject(
            "CorniceMessaggio",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        interno.transform.SetParent(pannelloRect, false);
        RectTransform internoRect = interno.GetComponent<RectTransform>();
        internoRect.anchorMin = Vector2.zero;
        internoRect.anchorMax = Vector2.one;
        internoRect.offsetMin = new Vector2(13f, 13f);
        internoRect.offsetMax = new Vector2(-13f, -13f);
        FarmPixelUI.ApplicaPannello(
            interno.GetComponent<Image>(),
            true,
            false
        );

        testoRect.SetParent(internoRect, false);
        testoRect.anchorMin = Vector2.zero;
        testoRect.anchorMax = Vector2.one;
        testoRect.pivot = new Vector2(0.5f, 0.5f);
        testoRect.anchoredPosition = Vector2.zero;
        testoRect.offsetMin = new Vector2(86f, 8f);
        testoRect.offsetMax = new Vector2(-86f, -8f);

        FarmPixelUI.AggiungiIcona(
            internoRect,
            "IconaOndataSinistra",
            FarmPixelIcon.Volpe,
            new Vector2(-282f, 0f),
            new Vector2(48f, 48f)
        );
        FarmPixelUI.AggiungiIcona(
            internoRect,
            "IconaOndataDestra",
            FarmPixelIcon.Ondata,
            new Vector2(282f, 0f),
            new Vector2(48f, 48f)
        );

        messaggioOndata.fontSize = 25f;
        messaggioOndata.fontStyle = FontStyles.Bold;
        messaggioOndata.alignment = TextAlignmentOptions.Center;
        FarmPixelUI.ApplicaTesto(
            messaggioOndata,
            new Color(1f, 0.88f, 0.56f, 1f)
        );
        messaggioOndata.textWrappingMode = TextWrappingModes.Normal;
        messaggioOndata.raycastTarget = false;
        messaggioOndata.gameObject.SetActive(true);
    }

    void MostraMessaggio(string testo)
    {
        if (messaggioOndata == null) return;

        if (contenitoreMessaggio != null)
        {
            contenitoreMessaggio.SetActive(true);
        }
        else
        {
            messaggioOndata.gameObject.SetActive(true);
        }

        messaggioOndata.text = testo;
    }

    void NascondiMessaggio()
    {
        if (contenitoreMessaggio != null)
        {
            contenitoreMessaggio.SetActive(false);
        }
        else if (messaggioOndata != null)
        {
            messaggioOndata.gameObject.SetActive(false);
        }
    }

    EnemyAI SpawnEnemy(
        Vector2 spawnPos,
        TipoVolpe tipo,
        int indiceSpawn,
        bool riproduciVerso
    )
    {
        if (foxPrefab == null) return null;

        GameObject nuovaVolpe =
            Instantiate(foxPrefab, spawnPos, Quaternion.identity);

        EnemyAI nemico = nuovaVolpe.GetComponent<EnemyAI>();
        if (nemico == null)
        {
            Debug.LogError(
                "Il prefab della volpe non contiene EnemyAI.",
                nuovaVolpe
            );
            Destroy(nuovaVolpe);
            return null;
        }

        int vitaOndata = Mathf.Max(
            1,
            vitaPrimaOndata + currentWaveIndex * vitaAggiuntivaPerOndata
        );
        nemico.InizializzaVariante(
            tipo,
            vitaOndata,
            indiceSpawn,
            riproduciVerso
        );
        return nemico;
    }

    bool SpawnMaialino(Wave ondata)
    {
        if (pigPrefab == null) return false;

        Vector2 centro = giocatore != null
            ? (Vector2)giocatore.position
            : Vector2.zero;
        Vector2 spawnPos =
            centro + DirezioneCasuale() * distanzaSpawnMaialino;

        GameObject nuovoMaialino =
            Instantiate(pigPrefab, spawnPos, Quaternion.identity);
        MaialinoBonus bonus = nuovoMaialino.GetComponent<MaialinoBonus>();

        if (bonus == null)
        {
            Debug.LogError(
                "Il prefab del maialino non contiene MaialinoBonus.",
                nuovoMaialino
            );
            Destroy(nuovoMaialino);
            return false;
        }

        bonus.Inizializza(
            ondata.vitaMaialinoBonus,
            ondata.moneteMaialinoBonus
        );
        return true;
    }

    bool PartitaTerminata()
    {
        return GameManager.instance != null && GameManager.instance.isGameOver;
    }

    static void PulisciEntitaTraOndate()
    {
        Proiettile[] proiettili = FindObjectsByType<Proiettile>(
            FindObjectsSortMode.None
        );
        foreach (Proiettile proiettile in proiettili)
        {
            if (proiettile != null)
            {
                Destroy(proiettile.gameObject);
            }
        }

        MaialinoBonus.RimuoviTuttiSenzaPremio();
    }

    static Vector2 DirezioneCasuale()
    {
        Vector2 direzione = Random.insideUnitCircle;
        return direzione.sqrMagnitude > 0.001f
            ? direzione.normalized
            : Vector2.right;
    }

    void OnDisable()
    {
        if (diagnostica != null)
        {
            diagnostica.TerminaOndata(
                EsitoDiagnosticaOndata.Interrotta
            );
        }
        tokenOndata++;
        InterrompiStatoOnda();
    }
}
