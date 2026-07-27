using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class EnemySpawnerRegressionTests
{
    private GameObject oggettoSpawner;
    private GameObject oggettoGiocatore;

    [TearDown]
    public void TearDown()
    {
        if (oggettoSpawner != null)
        {
            UnityEngine.Object.DestroyImmediate(oggettoSpawner);
        }

        if (oggettoGiocatore != null)
        {
            UnityEngine.Object.DestroyImmediate(oggettoGiocatore);
        }
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(3)]
    [TestCase(32)]
    public void PrimaOndata_GeneratoreSurvival_ContieneSoloComuni(
        int numeroNemici
    )
    {
        TipoVolpe[] sequenza =
            EnemySpawner.CreaSequenzaSurvival(1, numeroNemici);

        Assert.That(sequenza, Has.Length.EqualTo(numeroNemici));
        Assert.That(
            sequenza,
            Has.All.EqualTo(TipoVolpe.Comune),
            "La prima ondata deve restare un tutorial composto solo da " +
            "volpi comuni."
        );
    }

    [Test]
    public void PrimaOndata_ConfigurazioneReale_RestaComuneInOgniDifficolta()
    {
        Wave[] riferimento = GameBalanceConfig.Corrente.Ondate.ondate;

        Assert.That(riferimento, Is.Not.Null.And.Not.Empty);

        foreach (DifficoltaPartita difficolta in
                 Enum.GetValues(typeof(DifficoltaPartita)))
        {
            ProfiloDifficolta profilo =
                GameBalanceConfig.Corrente.Difficolta.Ottieni(difficolta);
            Wave[] adattate = EnemySpawner.CreaOndatePerDifficolta(
                riferimento,
                profilo
            );

            Assert.That(adattate, Is.Not.Empty);
            Assert.That(adattate[0], Is.Not.Null);

            ComposizioneVolpi composizione =
                EnemySpawner.CalcolaComposizione(adattate[0]);

            Assert.That(
                composizione.Totale,
                Is.EqualTo(adattate[0].numeroNemici),
                "La composizione deve coprire tutti gli slot della prima onda."
            );
            Assert.That(
                composizione.Comuni,
                Is.EqualTo(composizione.Totale),
                "La difficolta non deve introdurre varianti nella prima onda."
            );

            foreach (TipoVolpe tipo in Enum.GetValues(typeof(TipoVolpe)))
            {
                if (tipo == TipoVolpe.Comune)
                {
                    continue;
                }

                Assert.That(
                    composizione.Ottieni(tipo),
                    Is.Zero,
                    "Trovata una volpe " + tipo +
                    " nella prima onda a difficolta " + difficolta + "."
                );
            }
        }
    }

    [Test]
    public void Survival_ContinuaEScala_OltreLaCurvaConfigurata()
    {
        EnemySpawner spawner = CreaSpawner();
        spawner.vitaPrimaOndata = 2;
        spawner.vitaAggiuntivaPerOndata = 1;
        spawner.ondate = new[]
        {
            new Wave
            {
                nomeOndata = "Baseline",
                numeroNemici = 4,
                sequenzaVolpi = new[]
                {
                    TipoVolpe.Comune,
                    TipoVolpe.Comune,
                    TipoVolpe.Agile,
                    TipoVolpe.Robusta
                },
                intervalloTraNemici = 1f,
                dimensioneMassimaGruppo = 2,
                intervalloTraGruppi = 2f
            }
        };

        int[] indici = { 0, 1, 10, 100, 1000 };
        AnteprimaOndata precedente = default;

        Assert.That(spawner.TotaleOndate, Is.EqualTo(int.MaxValue));

        foreach (int indice in indici)
        {
            AnteprimaOndata anteprima = spawner.OttieniAnteprima(indice);

            Assert.That(anteprima.Valida, Is.True);
            Assert.That(anteprima.Indice, Is.EqualTo(indice + 1));
            Assert.That(
                anteprima.Composizione.Totale,
                Is.EqualTo(anteprima.NumeroVolpi)
            );

            if (indice > 0)
            {
                Assert.That(
                    anteprima.NumeroVolpi,
                    Is.GreaterThan(precedente.NumeroVolpi)
                );
                Assert.That(
                    anteprima.VitaVolpi,
                    Is.GreaterThan(precedente.VitaVolpi)
                );
            }

            for (int slot = 0; slot < anteprima.NumeroVolpi; slot++)
            {
                TipoVolpe tipo = spawner.OttieniTipoConfigurato(indice, slot);
                Assert.That(
                    Enum.IsDefined(typeof(TipoVolpe), tipo),
                    Is.True,
                    "Tipo non valido nell'onda " + anteprima.Indice +
                    ", slot " + slot + "."
                );
            }

            precedente = anteprima;
        }
    }

    [Test]
    public void AnelloSpawn_SegueLaPosizioneCorrenteDelContadino()
    {
        EnemySpawner spawner = CreaSpawner();
        spawner.spawnDistance = 10f;

        oggettoGiocatore = new GameObject("Contadino_Test");
        oggettoGiocatore.transform.position = new Vector3(3f, -2f, 0f);
        ImpostaCampoPrivato(
            spawner,
            "giocatore",
            oggettoGiocatore.transform
        );

        Vector2 direzione = new Vector2(3f, 4f);
        Vector2 primoCentro = oggettoGiocatore.transform.position;
        Vector2 primaPosizione =
            spawner.CalcolaPosizioneSpawnVolpe(direzione);

        Assert.That(
            Vector2.Distance(primoCentro, primaPosizione),
            Is.EqualTo(spawner.spawnDistance).Within(0.0001f)
        );

        oggettoGiocatore.transform.position = new Vector3(-8f, 6f, 0f);
        Vector2 secondoCentro = oggettoGiocatore.transform.position;
        Vector2 secondaPosizione =
            spawner.CalcolaPosizioneSpawnVolpe(direzione);

        Assert.That(spawner.CentroSpawnCorrente, Is.EqualTo(secondoCentro));
        Assert.That(
            Vector2.Distance(secondoCentro, secondaPosizione),
            Is.EqualTo(spawner.spawnDistance).Within(0.0001f)
        );
        Assert.That(
            Vector2.Distance(
                secondaPosizione - primaPosizione,
                secondoCentro - primoCentro
            ),
            Is.LessThanOrEqualTo(0.0001f)
        );
    }

    private EnemySpawner CreaSpawner()
    {
        oggettoSpawner = new GameObject("EnemySpawner_Test");
        return oggettoSpawner.AddComponent<EnemySpawner>();
    }

    private static void ImpostaCampoPrivato(
        object destinazione,
        string nomeCampo,
        object valore
    )
    {
        FieldInfo campo = destinazione.GetType().GetField(
            nomeCampo,
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.That(
            campo,
            Is.Not.Null,
            "Campo di test non trovato: " + nomeCampo
        );
        campo.SetValue(destinazione, valore);
    }
}
