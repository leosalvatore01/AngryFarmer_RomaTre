using System;
using System.Collections.Generic;

public enum TipoAttaccoNemico
{
    Sconosciuto = 0,
    ContattoVolpe = 1,
    ScattoAlfa = 2,
    ProiettileFango = 3,
    CaricaBoss = 4,
    SchiantoBoss = 5,
    RafficaBoss = 6
}

public enum SorgentePowerUpTelemetry
{
    PreparazioneGratuita = 0,
    Shop = 1,
    DropTemporaneo = 2
}

public readonly struct EventoDannoGiocatore
{
    public int DannoEffettivo { get; }
    public int VitaRimasta { get; }
    public bool Fatale { get; }
    public TipoAttaccoNemico TipoAttacco { get; }
    public bool HaTipoVolpe { get; }
    public TipoVolpe TipoVolpe { get; }

    public EventoDannoGiocatore(
        int dannoEffettivo,
        int vitaRimasta,
        bool fatale,
        TipoAttaccoNemico tipoAttacco,
        bool haTipoVolpe,
        TipoVolpe tipoVolpe
    )
    {
        DannoEffettivo = Math.Max(0, dannoEffettivo);
        VitaRimasta = Math.Max(0, vitaRimasta);
        Fatale = fatale;
        TipoAttacco = tipoAttacco;
        HaTipoVolpe = haTipoVolpe;
        TipoVolpe = tipoVolpe;
    }

    public static EventoDannoGiocatore Sconosciuto(
        int dannoEffettivo,
        int vitaRimasta,
        bool fatale
    )
    {
        return new EventoDannoGiocatore(
            dannoEffettivo,
            vitaRimasta,
            fatale,
            TipoAttaccoNemico.Sconosciuto,
            false,
            TipoVolpe.Comune
        );
    }
}

[Serializable]
public sealed class RunTelemetryWaveData
{
    public int indice;
    public string nome;
    public string esito;
    public float inizioGameplaySecondi;
    public float durataTotaleSecondi;
    public float durataBannerSecondi;
    public float durataSpawnSecondi;
    public float durataCombattimentoSecondi;
    public int nemiciPrevisti;
    public int tentativiSpawn;
    public int nemiciSpawnati;
    public int nemiciViviFinali;
    public int piccoNemiciVivi;
    public int maialiniPrevisti;
    public int maialiniSpawnati;
}

[Serializable]
public sealed class RunTelemetryPowerUpData
{
    public float tempoGameplaySecondi;
    public int dopoOndata;
    public string id;
    public string nome;
    public string sorgente;
    public int costo;
    public int livelloDopo;
}

[Serializable]
public sealed class RunTelemetryData
{
    public const int VersioneSchemaCorrente = 1;

    public int versioneSchema = VersioneSchemaCorrente;
    public string idPartita;
    public string avvioUtc;
    public string fineUtc;
    public string versioneGioco;
    public string piattaforma;
    public string difficolta;
    public bool conclusa;
    public bool morteContadino;
    public string causaMorte;
    public string tipoVolpeMorte;

    public float durataGameplaySecondi;
    public int ondataRaggiunta;
    public int ondateCompletate;
    public int danniSubiti;
    public int volpiEliminate;
    public int proiettiliSparati;
    public int proiettiliACentro;
    public float precisione;
    public int punteggioFinale;

    public int moneteIniziali;
    public int moneteGuadagnate;
    public int moneteSpese;
    public int moneteNonSpese;
    public int gettoniPermanentiGuadagnati;
    public int rerollEseguiti;
    public int costoTotaleReroll;

    public float fpsMedio;
    public int fotogrammiCampionati;
    public float secondiFpsCampionati;
    public int piccoNemiciAttivi;

    public List<RunTelemetryWaveData> ondate =
        new List<RunTelemetryWaveData>();
    public List<RunTelemetryPowerUpData> powerUp =
        new List<RunTelemetryPowerUpData>();
}

public readonly struct RunTelemetryFinalSnapshot
{
    public float DurataGameplaySecondi { get; }
    public int OndateCompletate { get; }
    public int VolpiEliminate { get; }
    public int ProiettiliSparati { get; }
    public int ProiettiliACentro { get; }
    public int PunteggioFinale { get; }
    public int MoneteIniziali { get; }
    public int MoneteGuadagnate { get; }
    public int MoneteSpese { get; }
    public int MoneteNonSpese { get; }
    public int GettoniPermanentiGuadagnati { get; }

    public RunTelemetryFinalSnapshot(
        float durataGameplaySecondi,
        int ondateCompletate,
        int volpiEliminate,
        int proiettiliSparati,
        int proiettiliACentro,
        int punteggioFinale,
        int moneteIniziali,
        int moneteGuadagnate,
        int moneteSpese,
        int moneteNonSpese,
        int gettoniPermanentiGuadagnati
    )
    {
        DurataGameplaySecondi = Math.Max(0f, durataGameplaySecondi);
        OndateCompletate = Math.Max(0, ondateCompletate);
        VolpiEliminate = Math.Max(0, volpiEliminate);
        ProiettiliSparati = Math.Max(0, proiettiliSparati);
        ProiettiliACentro = Math.Max(0, proiettiliACentro);
        PunteggioFinale = Math.Max(0, punteggioFinale);
        MoneteIniziali = Math.Max(0, moneteIniziali);
        MoneteGuadagnate = Math.Max(0, moneteGuadagnate);
        MoneteSpese = Math.Max(0, moneteSpese);
        MoneteNonSpese = Math.Max(0, moneteNonSpese);
        GettoniPermanentiGuadagnati = Math.Max(
            0,
            gettoniPermanentiGuadagnati
        );
    }
}
