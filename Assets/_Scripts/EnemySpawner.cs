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
    [HideInInspector] public int indiceSurvival;
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
    private int maialiniDaSpawnare;
    private int totaleNemiciOnda;
    private int gruppoCorrente;
    private int totaleGruppi;
    private ComposizioneVolpi composizioneTotaleOnda;
    private ComposizioneVolpi composizioneDaSpawnare;
    private bool spawnTerminato;
    private bool ondaAttiva;
    private bool avvioRapidoRichiesto;
    private WaveReadabilityController leggibilita;
    private ProfiloDifficolta profiloDifficolta;
    private bool difficoltaApplicata;

    public WaveRuntimeDiagnostics Diagnostica => diagnostica;
    public int IndiceOndaCorrente => currentWaveIndex;
    public int TotaleOndate => int.MaxValue;
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
    public ProfiloDifficolta ProfiloDifficoltaCorrente =>
        profiloDifficolta ??
        GameBalanceConfig.Corrente.Difficolta.Ottieni(
            DifficoltaPartita.Normale
        );
    public bool DifficoltaApplicata => difficoltaApplicata;
    public ProgressoOndata ProgressoCorrente => CreaProgressoCorrente();
    public AnteprimaOndata AnteprimaCorrente =>
        OttieniAnteprima(currentWaveIndex);

    public event System.Action<ProgressoOndata> ProgressoCambiato;

    void Start()
    {
        ApplicaConfigurazioneBilanciamento();
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
            Debug.LogWarning(
                "Nessuna ondata configurata: creo un'ondata survival di fallback.",
                this
            );
            ondate = new[]
            {
                new Wave
                {
                    nomeOndata = "Primo assalto",
                    numeroNemici = 4,
                    intervalloTraNemici = 0.85f,
                    dimensioneMassimaGruppo = 2,
                    intervalloTraGruppi = 1.5f
                }
            };
        }

        // Garantisce che GameManager e galline completino Start prima
        // di fotografare lo stato iniziale del primo obiettivo.
        yield return null;

        yield return new WaitUntil(
            () => GameManager.instance == null ||
                  GameManager.instance.DifficoltaConfermata
        );
        ApplicaDifficoltaAllaCurva();
        VerificaSequenzeConfigurate();
        difficoltaApplicata = true;

        yield return new WaitUntil(
            () => GameManager.instance == null ||
                  (GameManager.instance.PreparazioneInizialeCompletata &&
                   GameManager.instance.StatoCorrente == StatoPartita.Onda)
        );

        while (!PartitaTerminata())
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
            Wave ondataCorrente = OttieniOndata(currentWaveIndex);
            AnteprimaOndata anteprima =
                OttieniAnteprima(currentWaveIndex);

            IniziaStatoOnda(ondataCorrente, anteprima);
            diagnostica.IniziaOndata(
                currentWaveIndex + 1,
                int.MaxValue,
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
            FarmAudioController.RiproduciPericolo(
                currentWaveIndex >= ondate.Length - 1 ? 1f : 0.64f
            );

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

            while (
                minacceAttive.Count > 0 ||
                GameObject.FindGameObjectWithTag("Nemico") != null ||
                false
            )
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
            if (GameManager.instance != null)
            {
                GameManager.instance.RegistraCompletamentoOnda(
                    currentWaveIndex + 1
                );
            }

            if (!PartitaTerminata())
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
                        int.MaxValue,
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

    }

    Wave OttieniOndata(int indice)
    {
        if (ondate == null || ondate.Length == 0 || indice < 0)
        {
            return null;
        }

        if (indice < ondate.Length) return ondate[indice];

        Wave baseFinale = ondate[ondate.Length - 1];
        int numero = indice + 1;
        int extra = indice - ondate.Length + 1;
        int incrementiGruppo =
            numero / 4 - ondate.Length / 4;
        float accelerazione = Mathf.Pow(0.97f, extra);
        return new Wave
        {
            nomeOndata = "Sopravvivenza " + numero,
            numeroNemici = Mathf.Max(
                1,
                baseFinale.numeroNemici + extra * 2
            ),
            indiceSurvival = numero,
            sequenzaVolpi = null,
            intervalloTraNemici = Mathf.Max(
                0.18f,
                baseFinale.intervalloTraNemici * accelerazione
            ),
            dimensioneMassimaGruppo = Mathf.Clamp(
                baseFinale.dimensioneMassimaGruppo + incrementiGruppo,
                1,
                4
            ),
            intervalloTraGruppi = Mathf.Max(
                0.45f,
                baseFinale.intervalloTraGruppi * accelerazione
            ),
            numeroMaialiniBonus = 0,
            vitaMaialinoBonus = 1,
            moneteMaialinoBonus = 0
        };
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
            maialiniDaSpawnare = Mathf.Max(0, maialiniDaSpawnare - 1);
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

    void ApplicaDifficoltaAllaCurva()
    {
        DifficoltaPartita difficolta = GameManager.instance != null
            ? GameManager.instance.DifficoltaCorrente
            : ProgressionePartita.DifficoltaCorrente;
        profiloDifficolta = GameBalanceConfig.Corrente.Difficolta.Ottieni(
            difficolta
        );
        ondate = CreaOndatePerDifficolta(ondate, profiloDifficolta);
        if (ondate == null) return;
        for (int i = 0; i < ondate.Length; i++)
        {
            if (ondate[i] != null) ondate[i].numeroMaialiniBonus = 0;
        }
    }

    public static Wave[] CreaOndatePerDifficolta(
        Wave[] ondateBase,
        ProfiloDifficolta profilo
    )
    {
        if (ondateBase == null) return System.Array.Empty<Wave>();
        ProfiloDifficolta profiloValido = profilo ??
            new ProfiloDifficolta();
        Wave[] risultato = new Wave[ondateBase.Length];
        for (int i = 0; i < ondateBase.Length; i++)
        {
            Wave originale = ondateBase[i];
            if (originale == null) continue;

            int quantitaBase = Mathf.Max(0, originale.numeroNemici);
            int nuovaQuantita = profiloValido.ApplicaQuantita(
                quantitaBase
            );
            TipoVolpe[] sequenzaBase = new TipoVolpe[quantitaBase];
            for (int slot = 0; slot < quantitaBase; slot++)
            {
                sequenzaBase[slot] = OttieniTipoVolpe(originale, slot);
            }

            risultato[i] = new Wave
            {
                nomeOndata = originale.nomeOndata,
                numeroNemici = nuovaQuantita,
                sequenzaVolpi = AdattaSequenza(
                    sequenzaBase,
                    nuovaQuantita
                ),
                intervalloTraNemici = profiloValido.ApplicaIntervallo(
                    originale.intervalloTraNemici
                ),
                dimensioneMassimaGruppo = Mathf.Clamp(
                    originale.dimensioneMassimaGruppo,
                    1,
                    4
                ),
                intervalloTraGruppi = profiloValido.ApplicaIntervallo(
                    originale.intervalloTraGruppi
                ),
                numeroMaialiniBonus = Mathf.Max(
                    0,
                    originale.numeroMaialiniBonus
                ),
                vitaMaialinoBonus = Mathf.Max(
                    1,
                    originale.vitaMaialinoBonus
                ),
                moneteMaialinoBonus = Mathf.Max(
                    0,
                    originale.moneteMaialinoBonus
                )
            };
        }
        return risultato;
    }

    public static TipoVolpe[] AdattaSequenza(
        TipoVolpe[] sequenzaBase,
        int nuovaQuantita
    )
    {
        int quantita = Mathf.Max(0, nuovaQuantita);
        TipoVolpe[] risultato = new TipoVolpe[quantita];
        if (quantita == 0 || sequenzaBase == null ||
            sequenzaBase.Length == 0)
        {
            return risultato;
        }

        int origine = sequenzaBase.Length;
        if (quantita == 1)
        {
            risultato[0] = FoxVariantStyle.Normalizza(
                sequenzaBase[origine - 1]
            );
            return risultato;
        }

        for (int i = 0; i < quantita; i++)
        {
            int indiceOrigine = quantita < origine
                ? Mathf.RoundToInt(i * (origine - 1f) / (quantita - 1f))
                : Mathf.FloorToInt(i * origine / (float)quantita);
            risultato[i] = FoxVariantStyle.Normalizza(
                sequenzaBase[Mathf.Clamp(indiceOrigine, 0, origine - 1)]
            );
        }
        return risultato;
    }

    int CalcolaVitaOnda(int indiceZeroBased)
    {
        int vitaBase = Mathf.Max(
            1,
            vitaPrimaOndata +
            Mathf.Max(0, indiceZeroBased) * vitaAggiuntivaPerOndata
        );
        return ProfiloDifficoltaCorrente.ApplicaVita(vitaBase);
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
        Wave onda = OttieniOndata(indiceZeroBased);
        if (onda == null)
        {
            return default;
        }

        int numeroVolpi = Mathf.Max(0, onda.numeroNemici);
        int numeroGruppi = CalcolaNumeroGruppi(
            numeroVolpi,
            onda.dimensioneMassimaGruppo
        );
        int vitaVolpi = CalcolaVitaOnda(indiceZeroBased);
        ComposizioneVolpi composizione = CalcolaComposizione(onda);

        return new AnteprimaOndata(
            indiceZeroBased + 1,
            indiceZeroBased + 1,
            string.IsNullOrWhiteSpace(onda.nomeOndata)
                ? "Ondata " + (indiceZeroBased + 1)
                : onda.nomeOndata,
            numeroVolpi,
            vitaVolpi,
            composizione,
            0,
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
        maialiniDaSpawnare = 0;
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
        maialiniDaSpawnare = 0;
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
        Wave ondaCorrente = OttieniOndata(currentWaveIndex);
        if (ondaCorrente != null)
        {
            nome = ondaCorrente.nomeOndata;
        }

        int totale = int.MaxValue;
        int indiceVisualizzato = currentWaveIndex + 1;

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
        Wave onda = OttieniOndata(indiceOndaZeroBased);
        if (onda == null)
        {
            return TipoVolpe.Comune;
        }
        return OttieniTipoVolpe(
            onda,
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
            indiceSpawnZeroBased >= Mathf.Max(0, onda.numeroNemici))
        {
            return TipoVolpe.Comune;
        }

        if (onda.sequenzaVolpi == null ||
            indiceSpawnZeroBased >= onda.sequenzaVolpi.Length)
        {
            if (onda.indiceSurvival <= 0) return TipoVolpe.Comune;
            switch ((indiceSpawnZeroBased + onda.indiceSurvival) % 4)
            {
                case 1: return TipoVolpe.Agile;
                case 2: return TipoVolpe.Ladra;
                case 3: return TipoVolpe.Alfa;
                default: return TipoVolpe.Comune;
            }
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
        return
            "ONDATA " + anteprima.Indice +
            "\n" + anteprima.Nome.ToUpperInvariant() +
            "\n" + anteprima.Composizione.FormattaCompatta();
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
                "Ondata   " + (currentWaveIndex + 1);
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
            typeof(Image)
        );
        contenitoreMessaggio.transform.SetParent(canvas, false);
        contenitoreMessaggio.transform.SetSiblingIndex(indice);

        RectTransform pannelloRect =
            contenitoreMessaggio.GetComponent<RectTransform>();
        pannelloRect.anchorMin = new Vector2(0.5f, 0.5f);
        pannelloRect.anchorMax = new Vector2(0.5f, 0.5f);
        pannelloRect.pivot = new Vector2(0.5f, 0.5f);
        pannelloRect.anchoredPosition = Vector2.zero;
        pannelloRect.sizeDelta = new Vector2(760f, 190f);

        Image sfondo = contenitoreMessaggio.GetComponent<Image>();
        FarmPixelUI.ApplicaPannello(sfondo, false, false);

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
        testoRect.offsetMin = new Vector2(98f, 8f);
        testoRect.offsetMax = new Vector2(-98f, -8f);

        FarmPixelUI.AggiungiIcona(
            internoRect,
            "IconaOndataSinistra",
            FarmPixelIcon.Volpe,
            new Vector2(-312f, 0f),
            new Vector2(54f, 54f)
        );
        FarmPixelUI.AggiungiIcona(
            internoRect,
            "IconaOndataDestra",
            FarmPixelIcon.Ondata,
            new Vector2(312f, 0f),
            new Vector2(54f, 54f)
        );

        messaggioOndata.fontSize = 28f;
        messaggioOndata.fontStyle = FontStyles.Bold;
        messaggioOndata.alignment = TextAlignmentOptions.Center;
        FarmPixelUI.ApplicaTesto(
            messaggioOndata,
            new Color(1f, 0.88f, 0.56f, 1f)
        );
        messaggioOndata.textWrappingMode = TextWrappingModes.Normal;
        messaggioOndata.overflowMode = TextOverflowModes.Ellipsis;
        messaggioOndata.maxVisibleLines = 4;
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

        int vitaOndata = CalcolaVitaOnda(currentWaveIndex);
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
