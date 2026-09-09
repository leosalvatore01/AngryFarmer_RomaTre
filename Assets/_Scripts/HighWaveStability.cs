using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public readonly struct HighWaveStabilityReport
{
    public int NumeroOndata { get; }
    public int VolpiTotali { get; }
    public int PiccoVolpi { get; }
    public int EntitaMassimeStimate { get; }
    public float MemoriaPoolStimataMb { get; }
    public float BudgetMemoriaMb { get; }
    public float CostoFrameStimatoMs { get; }
    public float BudgetFrameMs { get; }
    public bool Stabile { get; }

    public HighWaveStabilityReport(
        int numeroOndata,
        int volpiTotali,
        int piccoVolpi,
        int entitaMassimeStimate,
        float memoriaPoolStimataMb,
        float budgetMemoriaMb,
        float costoFrameStimatoMs,
        float budgetFrameMs,
        bool stabile
    )
    {
        NumeroOndata = numeroOndata;
        VolpiTotali = volpiTotali;
        PiccoVolpi = piccoVolpi;
        EntitaMassimeStimate = entitaMassimeStimate;
        MemoriaPoolStimataMb = memoriaPoolStimataMb;
        BudgetMemoriaMb = budgetMemoriaMb;
        CostoFrameStimatoMs = costoFrameStimatoMs;
        BudgetFrameMs = budgetFrameMs;
        Stabile = stabile;
    }
}

/// <summary>
/// Simula il carico massimo contemporaneo di un'ondata usando gli stessi
/// limiti impiegati dallo spawner e dai pool. Non pretende di sostituire il
/// profiler su dispositivo: impedisce pero che una modifica renda illimitato
/// il numero di entita o superi i budget dichiarati per le ondate avanzate.
/// </summary>
public static class HighWaveStabilityAnalyzer
{
    private const int LimiteNumeriDanno = 28;

    public static HighWaveStabilityReport Analizza(
        AnteprimaOndata onda,
        PoolingBalanceSettings pooling
    )
    {
        pooling = pooling ?? new PoolingBalanceSettings();

        int piccoVolpi = Mathf.Min(
            Mathf.Max(0, onda.NumeroVolpi),
            Mathf.Max(1, onda.LimiteNemiciContemporanei)
        );
        int proiettiliContadino = Mathf.Min(
            pooling.proiettiliContadino,
            Mathf.Max(12, piccoVolpi * 3)
        );
        int sputafangoContemporanee = onda.NumeroVolpi > 0
            ? Mathf.CeilToInt(
                piccoVolpi *
                (onda.Composizione.Sputafango /
                 (float)onda.NumeroVolpi)
            )
            : 0;
        int proiettiliNemici = Mathf.Min(
            pooling.proiettiliNemici,
            sputafangoContemporanee * 2 + (onda.Boss ? 12 : 0)
        );
        int drop = Mathf.Min(
            pooling.drop,
            Mathf.CeilToInt(piccoVolpi * 0.45f) + onda.NumeroMaialini
        );
        int maialini = Mathf.Min(pooling.maialini, onda.NumeroMaialini);
        int anelli = Mathf.Min(
            pooling.anelliVfx,
            proiettiliNemici * 2 + Mathf.CeilToInt(piccoVolpi * 1.4f)
        );
        int linee = Mathf.Min(
            pooling.lineeVfx,
            Mathf.Max(onda.Boss ? 3 : 0, sputafangoContemporanee)
        );
        int impulsi = Mathf.Min(
            pooling.impulsiSecondari,
            Mathf.Max(2, piccoVolpi / 3)
        );
        int numeriDanno = Mathf.Min(
            LimiteNumeriDanno,
            Mathf.Max(8, piccoVolpi + proiettiliContadino / 4)
        );
        int particelleRicompensa = Mathf.Min(
            pooling.particelleRicompensa,
            maialini * 11
        );

        int entita = piccoVolpi + proiettiliContadino + proiettiliNemici +
            drop + maialini + anelli + linee + impulsi + numeriDanno +
            particelleRicompensa;

        // Pesi prudenziali comprensivi di componenti e figli grafici.
        float memoriaMb = (
            piccoVolpi * 160f +
            proiettiliContadino * 18f +
            proiettiliNemici * 22f +
            drop * 14f +
            maialini * 110f +
            anelli * 72f +
            linee * 42f +
            impulsi * 10f +
            numeriDanno * 18f +
            particelleRicompensa * 14f
        ) / 1024f;

        float budgetFrameMs = 1000f / Mathf.Max(1, pooling.fpsObiettivo);
        float costoFrameMs = 2.6f +
            piccoVolpi * 0.19f +
            proiettiliContadino * 0.022f +
            proiettiliNemici * 0.045f +
            anelli * 0.014f +
            (drop + maialini + linee + impulsi + numeriDanno) * 0.009f;

        int capacitaVolpi = pooling.volpiComuni +
            (onda.Elite ? pooling.volpiElite : 0) +
            (onda.Boss ? pooling.boss : 0);
        bool stabile = onda.Valida &&
            piccoVolpi <= capacitaVolpi &&
            memoriaMb <= pooling.budgetMemoriaPoolMb &&
            costoFrameMs <= budgetFrameMs;

        return new HighWaveStabilityReport(
            onda.Indice,
            onda.NumeroVolpi,
            piccoVolpi,
            entita,
            memoriaMb,
            pooling.budgetMemoriaPoolMb,
            costoFrameMs,
            budgetFrameMs,
            stabile
        );
    }
}

/// <summary>
/// Raccoglie durante una partita FPS, memoria ed entita attive e stampa un
/// checkpoint di sviluppo alle ondate 25, 50 e 100.
/// </summary>
public sealed class HighWavePerformanceMonitor : MonoBehaviour
{
    private static readonly int[] checkpoint = { 25, 50, 100 };
    private readonly HashSet<int> registrati = new HashSet<int>();
    private EnemySpawner spawner;
    private int spawnerId;
    private float fpsMedio = 60f;
    private int piccoEntita;
    private long piccoMemoriaByte;

    public static HighWavePerformanceMonitor Instance { get; private set; }
    public float FpsMedio => fpsMedio;
    public int PiccoEntita => piccoEntita;
    public float PiccoMemoriaMb => piccoMemoriaByte / (1024f * 1024f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreaMonitor()
    {
        if (Instance != null) return;
        GameObject oggetto = new GameObject("HighWavePerformanceMonitor");
        oggetto.AddComponent<HighWavePerformanceMonitor>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        float delta = Time.unscaledDeltaTime;
        if (delta > 0.0001f)
        {
            float fpsIstantanei = 1f / delta;
            fpsMedio = Mathf.Lerp(fpsMedio, fpsIstantanei, 0.04f);
        }

        GameplayPoolSnapshot pool = GameplayObjectPool.Diagnostica;
        int numeri = DamageNumberFeedback.Instance != null
            ? DamageNumberFeedback.Instance.PopupAttivi
            : 0;
        piccoEntita = Mathf.Max(piccoEntita, pool.OggettiInUso + numeri);
        piccoMemoriaByte = System.Math.Max(
            piccoMemoriaByte,
            Profiler.GetTotalAllocatedMemoryLong()
        );

        if (spawner == null)
        {
            spawner = FindFirstObjectByType<EnemySpawner>();
            int nuovoId = spawner != null ? spawner.GetInstanceID() : 0;
            if (nuovoId != spawnerId)
            {
                spawnerId = nuovoId;
                registrati.Clear();
                piccoEntita = 0;
                piccoMemoriaByte = 0;
            }
        }
        if (spawner == null) return;

        int ondaCorrente = spawner.IndiceOndaCorrente + 1;
        for (int i = 0; i < checkpoint.Length; i++)
        {
            int numero = checkpoint[i];
            if (ondaCorrente != numero || !registrati.Add(numero)) continue;
            RegistraCheckpoint(numero);
        }
    }

    private void RegistraCheckpoint(int numeroOndata)
    {
        AnteprimaOndata onda = spawner.OttieniAnteprima(numeroOndata - 1);
        HighWaveStabilityReport report =
            HighWaveStabilityAnalyzer.Analizza(
                onda,
                GameBalanceConfig.Corrente.Pooling
            );
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            "[Stabilita ondata " + numeroOndata + "] FPS medi " +
            fpsMedio.ToString("0.0") + ", memoria massima " +
            PiccoMemoriaMb.ToString("0.0") + " MB, entita attive max " +
            piccoEntita + ", simulazione " +
            report.CostoFrameStimatoMs.ToString("0.0") + " ms / " +
            report.MemoriaPoolStimataMb.ToString("0.0") + " MB."
        );
#endif
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
