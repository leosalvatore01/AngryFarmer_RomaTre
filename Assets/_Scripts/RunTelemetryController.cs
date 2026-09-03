using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RunTelemetryController : MonoBehaviour
{
    public static RunTelemetryController Corrente { get; private set; }

    private GameManager gestore;
    private EnemySpawner spawner;
    private WaveRuntimeDiagnostics diagnostica;
    private RunTelemetrySession sessione;
    private bool chiusuraInCorso;

    public RunTelemetrySession Sessione => sessione;
    public RunTelemetryData Dati => sessione != null
        ? sessione.Dati
        : null;
    public RunTelemetryExportResult UltimaEsportazione { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AzzeraStatoRuntime()
    {
        Corrente = null;
    }

    public static RunTelemetryController CreaOTrova(GameManager proprietario)
    {
        if (proprietario == null) return null;

        RunTelemetryController esistente =
            proprietario.GetComponent<RunTelemetryController>();
        return esistente != null
            ? esistente
            : proprietario.gameObject.AddComponent<RunTelemetryController>();
    }

    void Awake()
    {
        gestore = GetComponent<GameManager>();
        if (Corrente != null && Corrente != this)
        {
            Destroy(this);
            return;
        }
        Corrente = this;
    }

    void Start()
    {
        AgganciaSpawner();
    }

    void Update()
    {
        if (spawner == null || diagnostica == null)
        {
            AgganciaSpawner();
        }

        if (sessione == null || sessione.Finalizzata || gestore == null ||
            !gestore.GameplayAttivo)
        {
            return;
        }

        sessione.RegistraFrame(
            Time.unscaledDeltaTime,
            Application.isFocused
        );
    }

    public void IniziaSessione(
        DifficoltaPartita difficolta,
        int moneteIniziali
    )
    {
        if (sessione != null) return;

        sessione = new RunTelemetrySession(
            Guid.NewGuid().ToString("N"),
            DateTime.UtcNow,
            difficolta.ToString(),
            moneteIniziali,
            Application.version,
            Application.platform.ToString()
        );
        AgganciaSpawner();
    }

    public void RegistraDanno(EventoDannoGiocatore evento)
    {
        sessione?.RegistraDanno(evento);
    }

    public void RegistraReroll(int costo)
    {
        sessione?.RegistraReroll(costo);
    }

    public void RegistraPowerUp(
        TipoPotenziamento tipo,
        string nome,
        SorgentePowerUpTelemetry sorgente,
        int costo,
        int livelloDopo
    )
    {
        if (sessione == null) return;

        int dopoOndata = gestore != null
            ? gestore.OndateCompletate
            : sessione.Dati.ondateCompletate;
        sessione.RegistraPowerUp(
            tipo.ToString(),
            nome,
            sorgente,
            costo,
            livelloDopo,
            dopoOndata
        );
    }

    public void RegistraDropTemporaneo(string id, string nome)
    {
        if (sessione == null) return;

        int onda = spawner != null && spawner.OndaAttiva
            ? spawner.IndiceOndaCorrente + 1
            : sessione.Dati.ondataRaggiunta;
        sessione.RegistraPowerUp(
            id,
            nome,
            SorgentePowerUpTelemetry.DropTemporaneo,
            0,
            0,
            onda
        );
    }

    public void ConcludiPartita(bool morteContadino)
    {
        if (chiusuraInCorso || gestore == null) return;

        if (sessione == null)
        {
            IniziaSessione(
                gestore.DifficoltaCorrente,
                gestore.MoneteIniziali
            );
        }
        if (sessione == null || sessione.Finalizzata) return;

        chiusuraInCorso = true;
        AggiornaUltimoProgresso();

        // Chiude subito la diagnostica dell'ondata corrente: l'evento
        // RiepilogoCreato aggiorna la sessione prima dell'esportazione e
        // conserva così spawn, picco e tempi reali anche nell'ondata fatale.
        if (diagnostica != null && diagnostica.OndaInCorso)
        {
            diagnostica.TerminaOndata(
                morteContadino
                    ? EsitoDiagnosticaOndata.Sconfitta
                    : EsitoDiagnosticaOndata.Interrotta
            );
        }

        RunTelemetryFinalSnapshot riepilogo =
            new RunTelemetryFinalSnapshot(
                gestore.DurataPartita,
                gestore.OndateCompletate,
                gestore.VolpiEliminate,
                gestore.ProiettiliSparati,
                gestore.ProiettiliACentro,
                gestore.PunteggioFinale,
                gestore.MoneteIniziali,
                gestore.MoneteGuadagnateDurantePartita,
                gestore.MoneteSpese,
                gestore.monete,
                gestore.GettoniPermanentiGuadagnati
            );

        if (sessione.Finalizza(DateTime.UtcNow, morteContadino, riepilogo))
        {
            UltimaEsportazione = RunTelemetryExporter.Esporta(sessione.Dati);
            if (UltimaEsportazione.Riuscito)
            {
                Debug.Log(
                    "[Telemetry] Partita esportata in " +
                    UltimaEsportazione.PercorsoJson,
                    this
                );
            }
            else
            {
                Debug.LogWarning(
                    "[Telemetry] Esportazione non riuscita: " +
                    UltimaEsportazione.Errore,
                    this
                );
            }
        }
        chiusuraInCorso = false;
    }

    private void AgganciaSpawner()
    {
        EnemySpawner nuovoSpawner = FindFirstObjectByType<EnemySpawner>();
        if (nuovoSpawner != spawner)
        {
            SganciaSpawner();
            spawner = nuovoSpawner;
            if (spawner != null)
            {
                spawner.ProgressoCambiato += ProgressoOndaCambiato;
            }
        }

        WaveRuntimeDiagnostics nuovaDiagnostica = spawner != null
            ? spawner.Diagnostica
            : null;
        if (nuovaDiagnostica == diagnostica) return;

        if (diagnostica != null)
        {
            diagnostica.RiepilogoCreato -= RiepilogoOndaCreato;
        }
        diagnostica = nuovaDiagnostica;
        if (diagnostica != null)
        {
            diagnostica.RiepilogoCreato += RiepilogoOndaCreato;
        }
    }

    private void SganciaSpawner()
    {
        if (spawner != null)
        {
            spawner.ProgressoCambiato -= ProgressoOndaCambiato;
        }
        if (diagnostica != null)
        {
            diagnostica.RiepilogoCreato -= RiepilogoOndaCreato;
        }
        spawner = null;
        diagnostica = null;
    }

    private void ProgressoOndaCambiato(ProgressoOndata progresso)
    {
        if (sessione == null || sessione.Finalizzata ||
            !progresso.OndaAttiva)
        {
            return;
        }

        sessione.RegistraProgressoOnda(
            progresso.Token,
            progresso.Indice,
            progresso.Nome,
            progresso.VolpiTotali,
            progresso.VolpiDaSpawnare,
            progresso.MinacceAttive
        );
    }

    private void RiepilogoOndaCreato(
        RiepilogoDiagnosticaOndata riepilogo
    )
    {
        sessione?.RegistraRiepilogoOnda(riepilogo);
    }

    private void AggiornaUltimoProgresso()
    {
        if (spawner == null || sessione == null || !spawner.OndaAttiva)
        {
            return;
        }
        ProgressoOndaCambiato(spawner.ProgressoCorrente);
    }

    void OnDestroy()
    {
        SganciaSpawner();
        if (sessione != null && !sessione.Finalizzata && gestore != null)
        {
            ConcludiPartita(false);
        }
        if (Corrente == this) Corrente = null;
    }
}
