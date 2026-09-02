using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class SurvivalLegacyRemovalTests
{
    private const string PercorsoGameplay =
        "Assets/Scenes/SampleScene.unity";

    [Test]
    public void Punteggio_UsaSoloIndicatoriDelSurvival()
    {
        int punteggio = ProgressionePartita.CalcolaPunteggio(
            volpiEliminate: 3,
            ondateCompletate: 2,
            moneteRaccolte: 4,
            precisione: 0.5f,
            moltiplicatoreDifficolta: 1f
        );

        Assert.That(punteggio, Is.EqualTo(940));
    }

    [Test]
    public void API_Runtime_NonEsponePiuSistemiLegacy()
    {
        string[] metodiGameManagerVietati =
        {
            "Vittoria",
            "GameOver",
            "RegistraGallina",
            "GallinaMorta",
            "AggiungiUova",
            "RegistraUovoRecuperato",
            "RegistraEsitoObiettivo"
        };
        BindingFlags tuttiIMetodi =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static;

        foreach (string nome in metodiGameManagerVietati)
        {
            Assert.That(
                typeof(GameManager).GetMethod(nome, tuttiIMetodi),
                Is.Null,
                "API legacy ancora raggiungibile: " + nome
            );
        }

        Assert.That(
            typeof(GameBalanceConfig).GetProperty("ObiettiviFattoria"),
            Is.Null
        );
        Assert.That(
            typeof(GameBalanceConfig).GetProperty("Fattoria"),
            Is.Null
        );
        Assert.That(
            Enum.GetNames(typeof(FarmPixelIcon)),
            Does.Not.Contain("Uovo")
        );
        Assert.That(
            Enum.GetNames(typeof(FarmPixelIcon)),
            Does.Not.Contain("Gallina")
        );

        string[] tipiLegacy =
        {
            "Gallina",
            "UovoRecuperabile",
            "FarmObjectivesController",
            "FarmInteractiveArena"
        };
        foreach (string nome in tipiLegacy)
        {
            Assert.That(
                typeof(GameManager).Assembly.GetType(nome),
                Is.Null,
                "Tipo legacy ancora compilato: " + nome
            );
        }

        Assert.That(
            typeof(DatiRecordDifficolta).GetField(
                "migliorePercentualeGalline"
            ),
            Is.Null
        );
        Assert.That(
            typeof(DatiRecordDifficolta).GetField("migliorTempoVittoria"),
            Is.Null
        );
    }

    [Test]
    public void ScenaGameplay_NonContieneResiduiDiArenaOUova()
    {
        string percorsoAssoluto = Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            PercorsoGameplay
        );
        string yaml = File.ReadAllText(percorsoAssoluto);
        string[] residuiVietati =
        {
            "gallineRimaste:",
            "m_Name: Gallina",
            "m_Name: Pollaio",
            "m_Name: Uovo",
            "m_Name: FattoriaInterattiva",
            "m_Name: ObiettiviFattoria",
            "m_Name: SfondoFattoria",
            "5b38d647e6b85ff4e9de96edf26b1dc0"
        };

        foreach (string residuo in residuiVietati)
        {
            Assert.That(
                yaml,
                Does.Not.Contain(residuo),
                "Residuo legacy nella scena: " + residuo
            );
        }
    }
}
