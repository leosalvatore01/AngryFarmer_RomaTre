using System;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class RunTelemetryTests
{
    [Test]
    public void Frame_CalcolaDurataEFpsPonderato()
    {
        RunTelemetrySession sessione = CreaSessione("fps");

        sessione.RegistraFrame(0.02f);
        sessione.RegistraFrame(0.04f);
        sessione.RegistraFrame(0.50f, false);
        sessione.RegistraFrame(0f);
        sessione.RegistraFrame(float.NaN);
        sessione.Finalizza(
            DateTime.UtcNow,
            true,
            CreaRiepilogo(durata: 0.56f)
        );

        Assert.That(
            sessione.Dati.durataGameplaySecondi,
            Is.EqualTo(0.56f).Within(0.0001f)
        );
        Assert.That(sessione.Dati.fotogrammiCampionati, Is.EqualTo(2));
        Assert.That(
            sessione.Dati.fpsMedio,
            Is.EqualTo(2f / 0.06f).Within(0.001f)
        );
    }

    [Test]
    public void Lifecycle_RegistraOndatePiccoDannoECausaMorte()
    {
        RunTelemetrySession sessione = CreaSessione("lifecycle");

        sessione.RegistraProgressoOnda(10, 1, "Primo assalto", 4, 4, 0);
        sessione.RegistraFrame(1f);
        sessione.RegistraProgressoOnda(10, 1, "Primo assalto", 4, 0, 3);
        sessione.RegistraRiepilogoOnda(CreaRiepilogoOndaCompletata());

        sessione.RegistraProgressoOnda(20, 2, "Fango", 7, 7, 0);
        sessione.RegistraFrame(2f);
        sessione.RegistraProgressoOnda(20, 2, "Fango", 7, 2, 5);
        sessione.RegistraDanno(new EventoDannoGiocatore(
            3,
            0,
            true,
            TipoAttaccoNemico.ProiettileFango,
            true,
            TipoVolpe.Sputafango
        ));
        sessione.Finalizza(
            DateTime.UtcNow,
            true,
            CreaRiepilogo(durata: 3f, ondate: 1)
        );

        Assert.That(sessione.Dati.ondataRaggiunta, Is.EqualTo(2));
        Assert.That(sessione.Dati.ondateCompletate, Is.EqualTo(1));
        Assert.That(sessione.Dati.ondate, Has.Count.EqualTo(2));
        Assert.That(sessione.Dati.ondate[0].esito, Is.EqualTo("Completata"));
        Assert.That(sessione.Dati.ondate[1].esito, Is.EqualTo("Sconfitta"));
        Assert.That(
            sessione.Dati.ondate[1].durataTotaleSecondi,
            Is.EqualTo(2f).Within(0.001f)
        );
        Assert.That(sessione.Dati.piccoNemiciAttivi, Is.EqualTo(5));
        Assert.That(sessione.Dati.danniSubiti, Is.EqualTo(3));
        Assert.That(
            sessione.Dati.causaMorte,
            Is.EqualTo(TipoAttaccoNemico.ProiettileFango.ToString())
        );
        Assert.That(
            sessione.Dati.tipoVolpeMorte,
            Is.EqualTo(TipoVolpe.Sputafango.ToString())
        );
    }

    [Test]
    public void EconomiaEScelte_SeparaInizialiGuadagniRerollEOrigini()
    {
        RunTelemetrySession sessione = CreaSessione("economia", 8);

        sessione.RegistraPowerUp(
            "Danno",
            "Patate più dure",
            SorgentePowerUpTelemetry.PreparazioneGratuita,
            0,
            1,
            0
        );
        sessione.RegistraReroll(3);
        sessione.RegistraPowerUp(
            "Critico",
            "Centro perfetto",
            SorgentePowerUpTelemetry.Shop,
            5,
            2,
            3
        );
        sessione.RegistraPowerUp(
            "Coda",
            "Velocità temporanea",
            SorgentePowerUpTelemetry.DropTemporaneo,
            0,
            0,
            4
        );
        sessione.Finalizza(
            DateTime.UtcNow,
            true,
            new RunTelemetryFinalSnapshot(
                30f,
                4,
                20,
                40,
                30,
                900,
                8,
                17,
                8,
                17,
                4
            )
        );

        Assert.That(sessione.Dati.moneteIniziali, Is.EqualTo(8));
        Assert.That(sessione.Dati.moneteGuadagnate, Is.EqualTo(17));
        Assert.That(sessione.Dati.moneteSpese, Is.EqualTo(8));
        Assert.That(sessione.Dati.moneteNonSpese, Is.EqualTo(17));
        Assert.That(sessione.Dati.rerollEseguiti, Is.EqualTo(1));
        Assert.That(sessione.Dati.costoTotaleReroll, Is.EqualTo(3));
        Assert.That(sessione.Dati.powerUp, Has.Count.EqualTo(3));
        Assert.That(
            sessione.Dati.powerUp.Select(x => x.sorgente),
            Is.EqualTo(new[]
            {
                "PreparazioneGratuita",
                "Shop",
                "DropTemporaneo"
            })
        );
    }

    [Test]
    public void Finalizzazione_EIdempotenteENonDuplicaLaRun()
    {
        RunTelemetrySession sessione = CreaSessione("idempotente");
        sessione.RegistraProgressoOnda(1, 1, "Uno", 4, 4, 0);

        bool prima = sessione.Finalizza(
            DateTime.UtcNow,
            true,
            CreaRiepilogo()
        );
        bool seconda = sessione.Finalizza(
            DateTime.UtcNow.AddSeconds(1),
            true,
            CreaRiepilogo()
        );

        Assert.That(prima, Is.True);
        Assert.That(seconda, Is.False);
        Assert.That(sessione.Dati.ondate, Has.Count.EqualTo(1));
        Assert.That(sessione.Dati.ondate[0].esito, Is.EqualTo("Sconfitta"));
    }

    [Test]
    public void Diagnostica_CreaRiepilogoAncheConOverlayDisattivo()
    {
        GameObject oggetto = new GameObject("DiagnosticaTest");
        try
        {
            WaveRuntimeDiagnostics diagnostica =
                oggetto.AddComponent<WaveRuntimeDiagnostics>();
            diagnostica.ConfiguraOutput(false, false, false);
            RiepilogoDiagnosticaOndata ricevuto = default;
            diagnostica.RiepilogoCreato += valore => ricevuto = valore;

            diagnostica.IniziaOndata(1, int.MaxValue, "Test", 4, 1);
            diagnostica.AvviaCombattimento();
            diagnostica.RegistraSpawnNemico(true);
            diagnostica.CampionaNemiciVivi(3);
            diagnostica.RegistraSpawnMaialino(true);
            diagnostica.SegnaFineSpawn();
            RiepilogoDiagnosticaOndata restituito =
                diagnostica.TerminaOndata(
                    EsitoDiagnosticaOndata.Completata
                );

            Assert.That(restituito.Valido, Is.True);
            Assert.That(ricevuto.Valido, Is.True);
            Assert.That(ricevuto.NemiciSpawnati, Is.EqualTo(1));
            Assert.That(ricevuto.PiccoNemiciVivi, Is.EqualTo(3));
            Assert.That(ricevuto.MaialiniSpawnati, Is.EqualTo(1));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(oggetto);
        }
    }

    [Test]
    public void Exporter_ScriveJsonECsvInvariantConEscaping()
    {
        string cartella = CreaCartellaTemporanea();
        CultureInfo culturaPrecedente = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("it-IT");
            RunTelemetrySession sessione = CreaSessione("export-uno");
            sessione.RegistraProgressoOnda(
                1,
                1,
                "Onda, \"forte\"\nnotte",
                4,
                4,
                0
            );
            sessione.RegistraFrame(1.5f);
            sessione.RegistraPowerUp(
                "Critico",
                "Centro, perfetto",
                SorgentePowerUpTelemetry.Shop,
                3,
                1,
                0
            );
            sessione.Finalizza(
                DateTime.UtcNow,
                true,
                CreaRiepilogo(durata: 1.5f)
            );

            RunTelemetryExportResult esito =
                RunTelemetryExporter.Esporta(sessione.Dati, cartella);

            Assert.That(esito.Riuscito, Is.True, esito.Errore);
            Assert.That(File.Exists(esito.PercorsoJson), Is.True);
            Assert.That(File.Exists(Path.Combine(cartella, "partite.csv")), Is.True);
            Assert.That(File.Exists(Path.Combine(cartella, "ondate.csv")), Is.True);
            Assert.That(File.Exists(Path.Combine(cartella, "powerup.csv")), Is.True);

            RunTelemetryData json = JsonUtility.FromJson<RunTelemetryData>(
                File.ReadAllText(esito.PercorsoJson)
            );
            Assert.That(json.versioneSchema, Is.EqualTo(1));
            Assert.That(json.idPartita, Is.EqualTo("export-uno"));

            string csvPartita = File.ReadAllText(
                Path.Combine(cartella, "partite.csv")
            );
            string csvOndate = File.ReadAllText(
                Path.Combine(cartella, "ondate.csv")
            );
            Assert.That(csvPartita, Does.Contain("1.5"));
            Assert.That(csvPartita, Does.Not.Contain("1,5"));
            Assert.That(
                csvOndate,
                Does.Contain("\"Onda, \"\"forte\"\"\nnotte\"")
            );
        }
        finally
        {
            CultureInfo.CurrentCulture = culturaPrecedente;
            Directory.Delete(cartella, true);
        }
    }

    [Test]
    public void Exporter_AppendeSenzaDuplicareGliHeader()
    {
        string cartella = CreaCartellaTemporanea();
        try
        {
            RunTelemetrySession prima = CreaSessione("append-uno");
            prima.Finalizza(DateTime.UtcNow, true, CreaRiepilogo());
            RunTelemetrySession seconda = CreaSessione("append-due");
            seconda.Finalizza(DateTime.UtcNow, true, CreaRiepilogo());

            Assert.That(
                RunTelemetryExporter.Esporta(prima.Dati, cartella).Riuscito,
                Is.True
            );
            Assert.That(
                RunTelemetryExporter.Esporta(seconda.Dati, cartella).Riuscito,
                Is.True
            );

            string[] righe = File.ReadAllLines(
                Path.Combine(cartella, "partite.csv")
            );
            Assert.That(
                righe.Count(x => x.StartsWith("versione_schema,")),
                Is.EqualTo(1)
            );
            Assert.That(righe, Has.Length.EqualTo(3));
            Assert.That(
                Directory.GetFiles(cartella, "run_*.json"),
                Has.Length.EqualTo(2)
            );
        }
        finally
        {
            Directory.Delete(cartella, true);
        }
    }

    [Test]
    public void Exporter_PercorsoNonValidoRestituisceErroreSenzaEccezioni()
    {
        RunTelemetrySession sessione = CreaSessione("percorso-non-valido");
        sessione.Finalizza(DateTime.UtcNow, true, CreaRiepilogo());

        RunTelemetryExportResult esito = default;
        Assert.DoesNotThrow(() =>
        {
            esito = RunTelemetryExporter.Esporta(sessione.Dati, "\0");
        });

        Assert.That(esito.Riuscito, Is.False);
        Assert.That(esito.Errore, Is.Not.Empty);
    }

    private static RunTelemetrySession CreaSessione(
        string id,
        int moneteIniziali = 4
    )
    {
        return new RunTelemetrySession(
            id,
            new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc),
            "Normale",
            moneteIniziali,
            "test",
            "Editor"
        );
    }

    private static RunTelemetryFinalSnapshot CreaRiepilogo(
        float durata = 0f,
        int ondate = 0
    )
    {
        return new RunTelemetryFinalSnapshot(
            durata,
            ondate,
            0,
            0,
            0,
            0,
            4,
            0,
            0,
            4,
            0
        );
    }

    private static RiepilogoDiagnosticaOndata
        CreaRiepilogoOndaCompletata()
    {
        return new RiepilogoDiagnosticaOndata(
            1,
            int.MaxValue,
            "Primo assalto",
            EsitoDiagnosticaOndata.Completata,
            4,
            4,
            4,
            0,
            3,
            0,
            0,
            0,
            0.2f,
            0.3f,
            0.8f,
            1f
        );
    }

    private static string CreaCartellaTemporanea()
    {
        string percorso = Path.Combine(
            Path.GetTempPath(),
            "AngryFarmerTelemetryTests_" + Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(percorso);
        return percorso;
    }
}
