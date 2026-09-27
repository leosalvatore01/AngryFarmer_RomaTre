using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class SaveServiceRegressionTests
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false);
    private string cartellaTemporanea;
    private IDisposable ambienteDati;

    [SetUp]
    public void SetUp()
    {
        cartellaTemporanea = Path.Combine(
            Path.GetTempPath(),
            "AngryFarmerRegressionTests_" +
            Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(cartellaTemporanea);
        ambienteDati = SaveService.IsolaDatiPerTest(cartellaTemporanea);
    }

    [TearDown]
    public void TearDown()
    {
        ambienteDati?.Dispose();
        ambienteDati = null;

        if (Directory.Exists(cartellaTemporanea))
        {
            Directory.Delete(cartellaTemporanea, true);
        }
    }

    [Test]
    public void Profilo_SalvataggioAtomico_CreaBackupERecuperaCorruzione()
    {
        ScriviFixtureValide("Profilo precedente");

        Assert.That(
            SaveService.Profilo.nomeProfilo,
            Is.EqualTo("Profilo precedente")
        );

        bool salvato = SaveService.ModificaProfilo(
            dati => dati.nomeProfilo = "Profilo aggiornato"
        );

        Assert.That(salvato, Is.True);
        Assert.That(File.Exists(SaveService.PercorsoProfiloOspite), Is.True);
        Assert.That(
            File.Exists(SaveService.PercorsoProfiloOspite + ".bak"),
            Is.True
        );
        Assert.That(
            File.Exists(SaveService.PercorsoProfiloOspite + ".tmp"),
            Is.False
        );

        SaveData corrente = LeggiJson<SaveData>(
            SaveService.PercorsoProfiloOspite
        );
        SaveData backup = LeggiJson<SaveData>(
            SaveService.PercorsoProfiloOspite + ".bak"
        );
        Assert.That(corrente.nomeProfilo, Is.EqualTo("Profilo aggiornato"));
        Assert.That(backup.nomeProfilo, Is.EqualTo("Profilo precedente"));

        File.WriteAllText(
            SaveService.PercorsoProfiloOspite,
            "{ json non valido",
            Utf8
        );
        SaveService.RicaricaDatiPerTest();

        Assert.That(
            SaveService.Profilo.nomeProfilo,
            Is.EqualTo("Profilo precedente"),
            "Un file principale corrotto deve essere recuperato dal backup."
        );
    }

    [Test]
    public void Profilo_VersioneLegacy_VieneNormalizzataSenzaPlayerPrefs()
    {
        SaveData legacy = new SaveData
        {
            versioneSchema = 0,
            idProfilo = string.Empty,
            nomeProfilo = "   ",
            revisione = -10,
            ultimoSalvataggioUtcTicks = -20,
            migrazioneLegacyVersione =
                SaveService.VersioneMigrazioneLegacy,
            difficoltaPreferita = 99,
            miglioreOndataAssoluta = -7,
            progressionePermanente = new DatiProgressionePermanente
            {
                saldoGettoni = -5,
                totaleGettoniGuadagnati = -3,
                totaleGettoniSpesi = -2,
                livelli = new[] { -4, 2 }
            },
            recordDifficolta = new[]
            {
                new DatiRecordDifficolta
                {
                    difficolta = 99,
                    migliorPunteggio = -100,
                    massimoVolpi = -2,
                    massimaOndata = -1
                }
            }
        };

        ScriviFixture(legacy, CreaDispositivoValido());

        SaveData normalizzato = SaveService.Profilo;

        Assert.That(
            normalizzato.versioneSchema,
            Is.EqualTo(SaveService.VersioneSchemaCorrente)
        );
        Assert.That(
            normalizzato.idProfilo,
            Is.EqualTo(SaveService.IdProfiloOspite)
        );
        Assert.That(normalizzato.nomeProfilo, Is.EqualTo("Contadino"));
        Assert.That(
            normalizzato.difficoltaPreferita,
            Is.EqualTo((int)DifficoltaPartita.Difficile)
        );
        Assert.That(normalizzato.miglioreOndataAssoluta, Is.Zero);
        Assert.That(
            normalizzato.revisione,
            Is.GreaterThan(0),
            "La normalizzazione di schema deve essere salvata subito."
        );
        Assert.That(
            normalizzato.ultimoSalvataggioUtcTicks,
            Is.GreaterThan(0)
        );

        DatiProgressionePermanente meta =
            normalizzato.progressionePermanente;
        Assert.That(meta.saldoGettoni, Is.Zero);
        Assert.That(meta.totaleGettoniGuadagnati, Is.Zero);
        Assert.That(meta.totaleGettoniSpesi, Is.Zero);
        Assert.That(meta.livelli, Has.Length.EqualTo(6));
        Assert.That(meta.livelli[0], Is.Zero);
        Assert.That(meta.livelli[1], Is.EqualTo(2));

        Assert.That(
            normalizzato.recordDifficolta,
            Has.Length.EqualTo(3)
        );
        for (int i = 0; i < normalizzato.recordDifficolta.Length; i++)
        {
            Assert.That(normalizzato.recordDifficolta[i], Is.Not.Null);
            Assert.That(
                normalizzato.recordDifficolta[i].difficolta,
                Is.EqualTo(i)
            );
        }

        DatiRecordDifficolta primo = normalizzato.recordDifficolta[0];
        Assert.That(primo.migliorPunteggio, Is.Zero);
        Assert.That(primo.massimoVolpi, Is.Zero);
        Assert.That(primo.massimaOndata, Is.Zero);
    }

    [Test]
    public void Profilo_SchemaUno_RimuoveRecordLegacySenzaPerdereProgressi()
    {
        ScriviFixture(
            CreaProfiloValido("Placeholder"),
            CreaDispositivoValido()
        );
        const string profiloVersioneUno =
            "{\"versioneSchema\":1,\"idProfilo\":\"guest\"," +
            "\"nomeProfilo\":\"Veterano\",\"migrazioneLegacyVersione\":1," +
            "\"miglioreOndataAssoluta\":7,\"recordDifficolta\":[{" +
            "\"difficolta\":0,\"migliorPunteggio\":1234," +
            "\"massimoVolpi\":12,\"migliorePercentualeGalline\":100," +
            "\"migliorTempoVittoria\":45.5,\"massimaOndata\":7},null,null]}";
        File.WriteAllText(
            SaveService.PercorsoProfiloOspite,
            profiloVersioneUno,
            Utf8
        );

        SaveData migrato = SaveService.Profilo;

        Assert.That(
            migrato.versioneSchema,
            Is.EqualTo(SaveService.VersioneSchemaCorrente)
        );
        Assert.That(migrato.nomeProfilo, Is.EqualTo("Veterano"));
        Assert.That(migrato.miglioreOndataAssoluta, Is.EqualTo(7));
        Assert.That(
            migrato.recordDifficolta[0].migliorPunteggio,
            Is.Zero,
            "Il vecchio punteggio includeva obiettivi non più presenti."
        );
        Assert.That(migrato.recordDifficolta[0].massimoVolpi, Is.EqualTo(12));
        Assert.That(migrato.recordDifficolta[0].massimaOndata, Is.EqualTo(7));

        string jsonMigrato = File.ReadAllText(
            SaveService.PercorsoProfiloOspite,
            Utf8
        );
        SaveData persistito = JsonUtility.FromJson<SaveData>(jsonMigrato);
        Assert.That(
            persistito.versioneSchema,
            Is.EqualTo(SaveService.VersioneSchemaCorrente),
            "La migrazione deve essere persistita già al primo caricamento."
        );
        Assert.That(
            persistito.recordDifficolta[0].migliorPunteggio,
            Is.Zero
        );
        Assert.That(
            jsonMigrato,
            Does.Not.Contain("migliorePercentualeGalline")
        );
        Assert.That(jsonMigrato, Does.Not.Contain("migliorTempoVittoria"));
    }

    [Test]
    public void SchemaDue_AggiungeOpzioniPcSenzaCancellareIRecordSurvival()
    {
        SaveData profilo = CreaProfiloValido("Veterano PC");
        profilo.versioneSchema = 2;
        profilo.recordDifficolta[0] = new DatiRecordDifficolta
        {
            difficolta = 0,
            migliorPunteggio = 4321,
            massimoVolpi = 38,
            massimaOndata = 14
        };
        DeviceSettingsData dispositivo = CreaDispositivoValido();
        dispositivo.versioneSchema = 2;
        dispositivo.vSyncAttivo = false;
        dispositivo.limiteFps = 0;
        ScriviFixture(profilo, dispositivo);

        Assert.That(
            SaveService.Profilo.recordDifficolta[0].migliorPunteggio,
            Is.EqualTo(4321)
        );
        Assert.That(SaveService.Dispositivo.vSyncAttivo, Is.True);
        Assert.That(SaveService.Dispositivo.limiteFps, Is.EqualTo(60));
        Assert.That(
            SaveService.Dispositivo.versioneSchema,
            Is.EqualTo(SaveService.VersioneSchemaCorrente)
        );
    }

    [Test]
    public void ImportLegacy_IgnoraPunteggioPreSurvivalMaConservaVolpi()
    {
        SaveData profilo = CreaProfiloValido("Veterano PlayerPrefs");
        profilo.migrazioneLegacyVersione = 0;
        ScriviFixture(profilo, CreaDispositivoValido());

        const string prefisso = "AngryFarmer.Blocco8.Record.0";
        string chiavePunti = prefisso + ".Punti";
        string chiaveVolpi = prefisso + ".Volpi";
        bool puntiEsistevano = PlayerPrefs.HasKey(chiavePunti);
        bool volpiEsistevano = PlayerPrefs.HasKey(chiaveVolpi);
        int puntiPrecedenti = PlayerPrefs.GetInt(chiavePunti, 0);
        int volpiPrecedenti = PlayerPrefs.GetInt(chiaveVolpi, 0);
        PlayerPrefs.SetInt(chiavePunti, 999999);
        PlayerPrefs.SetInt(chiaveVolpi, 37);
        try
        {
            SaveData migrato = SaveService.Profilo;

            Assert.That(
                migrato.recordDifficolta[0].migliorPunteggio,
                Is.Zero
            );
            Assert.That(
                migrato.recordDifficolta[0].massimoVolpi,
                Is.EqualTo(37)
            );
        }
        finally
        {
            RipristinaPlayerPref(
                chiavePunti,
                puntiEsistevano,
                puntiPrecedenti
            );
            RipristinaPlayerPref(
                chiaveVolpi,
                volpiEsistevano,
                volpiPrecedenti
            );
        }
    }

    [Test]
    public void Profilo_SchemaFuturo_RestaInSolaLettura()
    {
        SaveData futuro = CreaProfiloValido("Profilo futuro");
        futuro.versioneSchema = SaveService.VersioneSchemaCorrente + 1;
        ScriviFixture(futuro, CreaDispositivoValido());
        string contenutoOriginale = File.ReadAllText(
            SaveService.PercorsoProfiloOspite,
            Utf8
        );

        LogAssert.Expect(
            LogType.Error,
            new System.Text.RegularExpressions.Regex(
                "Save profilo v.*Modalita sola lettura"
            )
        );
        Assert.That(
            SaveService.Profilo.nomeProfilo,
            Is.EqualTo("Profilo futuro")
        );

        LogAssert.Expect(
            LogType.Error,
            new System.Text.RegularExpressions.Regex(
                "versione piu recente.*non puo essere modificato"
            )
        );
        bool modificato = SaveService.ModificaProfilo(
            dati => dati.nomeProfilo = "Non deve cambiare"
        );

        Assert.That(modificato, Is.False);
        Assert.That(
            File.ReadAllText(SaveService.PercorsoProfiloOspite, Utf8),
            Is.EqualTo(contenutoOriginale)
        );
    }

    [Test]
    public void ResetProfilo_CancellaProgressiMaConservaIlDispositivo()
    {
        ScriviFixtureValide("Veterano");
        Assert.That(
            SaveService.ModificaProfilo(dati =>
            {
                dati.miglioreOndataAssoluta = 42;
                dati.progressionePermanente.saldoGettoni = 91;
                dati.progressionePermanente.livelli[2] = 7;
                dati.recordDifficolta[1].massimaOndata = 42;
            }),
            Is.True
        );
        Assert.That(
            SaveService.ModificaDispositivo(dati =>
            {
                dati.volumeMusica = 0.23f;
                dati.limiteFps = 120;
                dati.tutorialCompletato = true;
            }, true),
            Is.True
        );

        Assert.That(SaveService.AzzeraProfiloOspite(), Is.True);

        SaveData profilo = SaveService.Profilo;
        Assert.That(profilo.nomeProfilo, Is.EqualTo("Contadino"));
        Assert.That(profilo.miglioreOndataAssoluta, Is.Zero);
        Assert.That(profilo.progressionePermanente.saldoGettoni, Is.Zero);
        Assert.That(profilo.progressionePermanente.livelli, Is.All.Zero);
        Assert.That(
            profilo.recordDifficolta,
            Has.All.Matches<DatiRecordDifficolta>(r =>
                r.migliorPunteggio == 0 &&
                r.massimoVolpi == 0 &&
                r.massimaOndata == 0)
        );

        Assert.That(SaveService.Dispositivo.volumeMusica, Is.EqualTo(0.23f));
        Assert.That(SaveService.Dispositivo.limiteFps, Is.EqualTo(120));
        Assert.That(SaveService.Dispositivo.tutorialCompletato, Is.True);

        SaveData persistito = LeggiJson<SaveData>(
            SaveService.PercorsoProfiloOspite
        );
        Assert.That(persistito.nomeProfilo, Is.EqualTo("Contadino"));
        Assert.That(persistito.miglioreOndataAssoluta, Is.Zero);
    }

    private void ScriviFixtureValide(string nomeProfilo)
    {
        ScriviFixture(
            CreaProfiloValido(nomeProfilo),
            CreaDispositivoValido()
        );
    }

    private void ScriviFixture(
        SaveData profilo,
        DeviceSettingsData dispositivo
    )
    {
        ScriviJson(SaveService.PercorsoProfiloOspite, profilo);
        ScriviJson(
            SaveService.PercorsoImpostazioniDispositivo,
            dispositivo
        );
    }

    private static SaveData CreaProfiloValido(string nome)
    {
        return new SaveData
        {
            nomeProfilo = nome,
            migrazioneLegacyVersione =
                SaveService.VersioneMigrazioneLegacy
        };
    }

    private static DeviceSettingsData CreaDispositivoValido()
    {
        return new DeviceSettingsData
        {
            migrazioneLegacyVersione =
                SaveService.VersioneMigrazioneLegacy
        };
    }

    private static void ScriviJson<T>(string percorso, T dati)
    {
        string cartella = Path.GetDirectoryName(percorso);
        if (!string.IsNullOrEmpty(cartella))
        {
            Directory.CreateDirectory(cartella);
        }

        File.WriteAllText(
            percorso,
            JsonUtility.ToJson(dati, true),
            Utf8
        );
    }

    private static T LeggiJson<T>(string percorso)
    {
        return JsonUtility.FromJson<T>(File.ReadAllText(percorso, Utf8));
    }

    private static void RipristinaPlayerPref(
        string chiave,
        bool esisteva,
        int valore
    )
    {
        if (esisteva)
        {
            PlayerPrefs.SetInt(chiave, valore);
        }
        else
        {
            PlayerPrefs.DeleteKey(chiave);
        }
    }
}
