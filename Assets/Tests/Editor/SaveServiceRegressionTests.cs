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
                    migliorePercentualeGalline = 140,
                    migliorTempoVittoria = -4f,
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
        Assert.That(normalizzato.revisione, Is.Zero);
        Assert.That(normalizzato.ultimoSalvataggioUtcTicks, Is.Zero);

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
        Assert.That(primo.migliorTempoVittoria, Is.Zero);
        Assert.That(primo.massimaOndata, Is.Zero);
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
}
