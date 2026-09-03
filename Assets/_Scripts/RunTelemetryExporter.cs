using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public readonly struct RunTelemetryExportResult
{
    public bool Riuscito { get; }
    public string Cartella { get; }
    public string PercorsoJson { get; }
    public string Errore { get; }

    public RunTelemetryExportResult(
        bool riuscito,
        string cartella,
        string percorsoJson,
        string errore
    )
    {
        Riuscito = riuscito;
        Cartella = cartella ?? string.Empty;
        PercorsoJson = percorsoJson ?? string.Empty;
        Errore = errore ?? string.Empty;
    }
}

/// <summary>
/// Esporta ogni run in JSON e mantiene tre CSV append-only, comodi da
/// analizzare con un foglio di calcolo. Gli errori restano non bloccanti.
/// </summary>
public static class RunTelemetryExporter
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false);

    public static string CartellaPredefinita
    {
        get
        {
#if UNITY_EDITOR
            const string chiaveTest =
                "AngryFarmer.Tests.PlayModeSaveRoot";
            string radiceTest = UnityEditor.SessionState.GetString(
                chiaveTest,
                string.Empty
            );
            if (!string.IsNullOrWhiteSpace(radiceTest))
            {
                return Path.Combine(radiceTest, "Telemetry");
            }
#endif
            return Path.Combine(
                Application.persistentDataPath,
                "Telemetry"
            );
        }
    }

    public static RunTelemetryExportResult Esporta(
        RunTelemetryData dati,
        string cartellaOverride = null
    )
    {
        string cartella = string.Empty;
        string percorsoJson = string.Empty;
        try
        {
            cartella = string.IsNullOrWhiteSpace(cartellaOverride)
                ? CartellaPredefinita
                : Path.GetFullPath(cartellaOverride);
            if (dati == null)
            {
                return new RunTelemetryExportResult(
                    false,
                    cartella,
                    string.Empty,
                    "Dati telemetria assenti."
                );
            }

            Directory.CreateDirectory(cartella);
            percorsoJson = Path.Combine(
                cartella,
                CreaNomeJson(dati)
            );
            ScriviJsonAtomico(percorsoJson, dati);

            AggiungiCsv(
                Path.Combine(cartella, "partite.csv"),
                HeaderPartite,
                new[] { CreaRigaPartita(dati) }
            );
            AggiungiCsv(
                Path.Combine(cartella, "ondate.csv"),
                HeaderOndate,
                CreaRigheOndate(dati)
            );
            AggiungiCsv(
                Path.Combine(cartella, "powerup.csv"),
                HeaderPowerUp,
                CreaRighePowerUp(dati)
            );

            return new RunTelemetryExportResult(
                true,
                cartella,
                percorsoJson,
                string.Empty
            );
        }
        catch (Exception eccezione)
        {
            return new RunTelemetryExportResult(
                false,
                cartella,
                percorsoJson,
                eccezione.Message
            );
        }
    }

    public static string FormattaCampoCsv(string valore)
    {
        string testo = valore ?? string.Empty;
        bool richiedeVirgolette =
            testo.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
        if (!richiedeVirgolette) return testo;
        return "\"" + testo.Replace("\"", "\"\"") + "\"";
    }

    private static readonly string[] HeaderPartite =
    {
        "versione_schema",
        "id_partita",
        "avvio_utc",
        "fine_utc",
        "versione_gioco",
        "piattaforma",
        "difficolta",
        "morte_contadino",
        "causa_morte",
        "tipo_volpe_morte",
        "durata_gameplay_s",
        "ondata_raggiunta",
        "ondate_completate",
        "danni_subiti",
        "volpi_eliminate",
        "proiettili_sparati",
        "proiettili_a_centro",
        "precisione",
        "punteggio",
        "monete_iniziali",
        "monete_guadagnate",
        "monete_spese",
        "monete_non_spese",
        "gettoni_permanenti",
        "reroll",
        "costo_reroll",
        "fps_medio",
        "frame_campionati",
        "secondi_fps",
        "picco_nemici",
        "numero_powerup"
    };

    private static readonly string[] HeaderOndate =
    {
        "id_partita",
        "indice",
        "nome",
        "esito",
        "inizio_gameplay_s",
        "durata_totale_s",
        "durata_banner_s",
        "durata_spawn_s",
        "durata_combattimento_s",
        "nemici_previsti",
        "tentativi_spawn",
        "nemici_spawnati",
        "nemici_vivi_finali",
        "picco_nemici",
        "maialini_previsti",
        "maialini_spawnati"
    };

    private static readonly string[] HeaderPowerUp =
    {
        "id_partita",
        "tempo_gameplay_s",
        "dopo_ondata",
        "id_powerup",
        "nome",
        "sorgente",
        "costo",
        "livello_dopo"
    };

    private static string[] CreaRigaPartita(RunTelemetryData dati)
    {
        return new[]
        {
            dati.versioneSchema.ToString(CultureInfo.InvariantCulture),
            dati.idPartita,
            dati.avvioUtc,
            dati.fineUtc,
            dati.versioneGioco,
            dati.piattaforma,
            dati.difficolta,
            dati.morteContadino ? "true" : "false",
            dati.causaMorte,
            dati.tipoVolpeMorte,
            F(dati.durataGameplaySecondi),
            I(dati.ondataRaggiunta),
            I(dati.ondateCompletate),
            I(dati.danniSubiti),
            I(dati.volpiEliminate),
            I(dati.proiettiliSparati),
            I(dati.proiettiliACentro),
            F(dati.precisione),
            I(dati.punteggioFinale),
            I(dati.moneteIniziali),
            I(dati.moneteGuadagnate),
            I(dati.moneteSpese),
            I(dati.moneteNonSpese),
            I(dati.gettoniPermanentiGuadagnati),
            I(dati.rerollEseguiti),
            I(dati.costoTotaleReroll),
            F(dati.fpsMedio),
            I(dati.fotogrammiCampionati),
            F(dati.secondiFpsCampionati),
            I(dati.piccoNemiciAttivi),
            I(dati.powerUp != null ? dati.powerUp.Count : 0)
        };
    }

    private static IEnumerable<string[]> CreaRigheOndate(
        RunTelemetryData dati
    )
    {
        if (dati.ondate == null) yield break;

        foreach (RunTelemetryWaveData onda in dati.ondate)
        {
            if (onda == null) continue;
            yield return new[]
            {
                dati.idPartita,
                I(onda.indice),
                onda.nome,
                onda.esito,
                F(onda.inizioGameplaySecondi),
                F(onda.durataTotaleSecondi),
                F(onda.durataBannerSecondi),
                F(onda.durataSpawnSecondi),
                F(onda.durataCombattimentoSecondi),
                I(onda.nemiciPrevisti),
                I(onda.tentativiSpawn),
                I(onda.nemiciSpawnati),
                I(onda.nemiciViviFinali),
                I(onda.piccoNemiciVivi),
                I(onda.maialiniPrevisti),
                I(onda.maialiniSpawnati)
            };
        }
    }

    private static IEnumerable<string[]> CreaRighePowerUp(
        RunTelemetryData dati
    )
    {
        if (dati.powerUp == null) yield break;

        foreach (RunTelemetryPowerUpData powerUp in dati.powerUp)
        {
            if (powerUp == null) continue;
            yield return new[]
            {
                dati.idPartita,
                F(powerUp.tempoGameplaySecondi),
                I(powerUp.dopoOndata),
                powerUp.id,
                powerUp.nome,
                powerUp.sorgente,
                I(powerUp.costo),
                I(powerUp.livelloDopo)
            };
        }
    }

    private static void AggiungiCsv(
        string percorso,
        IReadOnlyList<string> header,
        IEnumerable<string[]> righe
    )
    {
        bool scriviHeader = !File.Exists(percorso) ||
                            new FileInfo(percorso).Length == 0;
        StringBuilder contenuto = new StringBuilder();
        if (scriviHeader)
        {
            contenuto.AppendLine(CreaRigaCsv(header));
        }

        foreach (string[] riga in righe ?? Enumerable.Empty<string[]>())
        {
            contenuto.AppendLine(CreaRigaCsv(riga));
        }

        if (contenuto.Length > 0)
        {
            File.AppendAllText(percorso, contenuto.ToString(), Utf8);
        }
    }

    private static string CreaRigaCsv(IEnumerable<string> campi)
    {
        return string.Join(",", campi.Select(FormattaCampoCsv));
    }

    private static void ScriviJsonAtomico(
        string percorso,
        RunTelemetryData dati
    )
    {
        string temporaneo = percorso + ".tmp";
        try
        {
            File.WriteAllText(
                temporaneo,
                JsonUtility.ToJson(dati, true),
                Utf8
            );
            if (File.Exists(percorso)) File.Delete(percorso);
            File.Move(temporaneo, percorso);
        }
        finally
        {
            if (File.Exists(temporaneo)) File.Delete(temporaneo);
        }
    }

    private static string CreaNomeJson(RunTelemetryData dati)
    {
        DateTime timestamp;
        if (!DateTime.TryParse(
                dati.avvioUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out timestamp
            ))
        {
            timestamp = DateTime.UtcNow;
        }

        string idPulito = new string(
            (dati.idPartita ?? string.Empty)
            .Where(char.IsLetterOrDigit)
            .Take(16)
            .ToArray()
        );
        if (string.IsNullOrEmpty(idPulito)) idPulito = "senzaid";
        return "run_" + timestamp.ToUniversalTime().ToString(
                   "yyyyMMdd_HHmmss",
                   CultureInfo.InvariantCulture
               ) + "_" + idPulito + ".json";
    }

    private static string I(int valore)
    {
        return valore.ToString(CultureInfo.InvariantCulture);
    }

    private static string F(float valore)
    {
        return valore.ToString("0.######", CultureInfo.InvariantCulture);
    }
}
