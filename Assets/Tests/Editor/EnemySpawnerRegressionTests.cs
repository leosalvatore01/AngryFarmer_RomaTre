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
    public void RitmoCapitoli_ApreShopDopoSecondaEQuartaOnda()
    {
        WaveChapterSettings impostazioni =
            GameBalanceConfig.Corrente.Ondate.capitoli;

        for (int onda = 1; onda <= 100; onda++)
        {
            RitmoOndata ritmo = WaveChapterDirector.Calcola(
                onda,
                impostazioni
            );
            bool shopAtteso =
                ritmo.PosizioneNelCapitolo == 2 ||
                ritmo.PosizioneNelCapitolo == 4;

            Assert.That(
                ritmo.ApreShopDopo,
                Is.EqualTo(shopAtteso),
                "Cadenza shop errata dopo l'ondata " + onda + "."
            );
        }
    }

    [Test]
    public void RitmoCapitoli_QuintaEliteEDecimaBoss()
    {
        WaveChapterSettings impostazioni =
            GameBalanceConfig.Corrente.Ondate.capitoli;

        for (int onda = 1; onda <= 50; onda++)
        {
            RitmoOndata ritmo = WaveChapterDirector.Calcola(
                onda,
                impostazioni
            );
            TipoIncontroOndata atteso = onda % 10 == 0
                ? TipoIncontroOndata.Boss
                : onda % 5 == 0
                    ? TipoIncontroOndata.Elite
                    : TipoIncontroOndata.Normale;

            Assert.That(
                ritmo.TipoIncontro,
                Is.EqualTo(atteso),
                "Tipo incontro errato all'ondata " + onda + "."
            );
        }
    }

    [TestCase(1, 1, 1)]
    [TestCase(5, 1, 5)]
    [TestCase(6, 2, 1)]
    [TestCase(10, 2, 5)]
    [TestCase(11, 3, 1)]
    [TestCase(1000, 200, 5)]
    public void RitmoCapitoli_CalcolaCapitoloEPosizioneSenzaFine(
        int onda,
        int capitoloAtteso,
        int posizioneAttesa
    )
    {
        RitmoOndata ritmo = WaveChapterDirector.Calcola(
            onda,
            GameBalanceConfig.Corrente.Ondate.capitoli
        );

        Assert.That(ritmo.NumeroCapitolo, Is.EqualTo(capitoloAtteso));
        Assert.That(
            ritmo.PosizioneNelCapitolo,
            Is.EqualTo(posizioneAttesa)
        );
    }

    [Test]
    public void AnteprimaOndata_EsponeIlRitmoDelCapitolo()
    {
        EnemySpawner spawner = CreaSpawner();
        spawner.ondate = GameBalanceConfig.Corrente.Ondate.ondate;

        AnteprimaOndata elite = spawner.OttieniAnteprima(4);
        AnteprimaOndata boss = spawner.OttieniAnteprima(9);
        AnteprimaOndata avanzata = spawner.OttieniAnteprima(999);

        Assert.That(elite.Elite, Is.True);
        Assert.That(elite.NumeroCapitolo, Is.EqualTo(1));
        Assert.That(boss.Boss, Is.True);
        Assert.That(boss.NumeroCapitolo, Is.EqualTo(2));
        Assert.That(avanzata.NumeroCapitolo, Is.EqualTo(200));
        Assert.That(avanzata.PosizioneNelCapitolo, Is.EqualTo(5));
    }

    [Test]
    public void TransizioneBreve_CongelaERiprendeIlGameplay()
    {
        GameManager precedente = GameManager.instance;
        float scalaPrecedente = Time.timeScale;
        GameObject oggettoManager = new GameObject("GameManager_RitmoTest");
        oggettoManager.SetActive(false);
        GameManager gestore = oggettoManager.AddComponent<GameManager>();

        try
        {
            GameManager.instance = gestore;
            ImpostaCampoPrivato(
                gestore,
                "<DifficoltaConfermata>k__BackingField",
                true
            );

            gestore.IniziaTransizioneBreve();

            Assert.That(
                gestore.StatoCorrente,
                Is.EqualTo(StatoPartita.Transizione)
            );
            Assert.That(gestore.GameplayAttivo, Is.False);
            Assert.That(Time.timeScale, Is.Zero);

            gestore.ConcludiTransizioneBreve();

            Assert.That(
                gestore.StatoCorrente,
                Is.EqualTo(StatoPartita.Onda)
            );
            Assert.That(gestore.GameplayAttivo, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }
        finally
        {
            GameManager.instance = precedente;
            Time.timeScale = scalaPrecedente;
            UnityEngine.Object.DestroyImmediate(oggettoManager);
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
