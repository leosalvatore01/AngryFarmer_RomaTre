using System;
using System.Collections.Generic;
using NUnit.Framework;

public sealed class WaveThreatDirectorTests
{
    private ThreatBudgetSettings impostazioni;
    private WaveChapterSettings capitoli;

    [SetUp]
    public void SetUp()
    {
        impostazioni = GameBalanceConfig.Corrente.Ondate.minaccia;
        capitoli = GameBalanceConfig.Corrente.Ondate.capitoli;
    }

    [Test]
    public void PrimaOndata_SpendeIlBudgetSoloInVolpiComuni()
    {
        PianoMinacciaOndata piano = CreaPiano(1);

        Assert.That(piano.Archetipo, Is.EqualTo(
            ArchetipoOndata.Addestramento
        ));
        Assert.That(piano.Composizione.Comuni, Is.EqualTo(piano.TotaleVolpi));
        Assert.That(piano.Composizione.Totale, Is.EqualTo(3));
        Assert.That(piano.BudgetUsato, Is.EqualTo(piano.BudgetTotale));
    }

    [TestCase(2, TipoVolpe.Agile)]
    [TestCase(3, TipoVolpe.Robusta)]
    [TestCase(4, TipoVolpe.Schivatrice)]
    [TestCase(5, TipoVolpe.Alfa)]
    [TestCase(6, TipoVolpe.Ululatrice)]
    [TestCase(8, TipoVolpe.Sputafango)]
    [TestCase(10, TipoVolpe.Scavatrice)]
    public void NuoveVarianti_CompaionoNellaLoroOndataDiIntroduzione(
        int onda,
        TipoVolpe tipo
    )
    {
        PianoMinacciaOndata piano = CreaPiano(onda);

        Assert.That(
            piano.Composizione.Ottieni(tipo),
            Is.GreaterThanOrEqualTo(1),
            "La volpe " + tipo + " non viene introdotta all'ondata " + onda
        );
    }

    [Test]
    public void PrimeDuecentoOndate_RispettanoBudgetSblocchiECombinazioni()
    {
        FoxThreatCosts costi = impostazioni.costi;
        for (int onda = 1; onda <= 200; onda++)
        {
            PianoMinacciaOndata piano = CreaPiano(onda);
            int costoCalcolato = 0;

            Assert.That(piano.TotaleVolpi, Is.GreaterThan(0));
            Assert.That(
                piano.TotaleVolpi,
                Is.LessThanOrEqualTo(impostazioni.limiteVolpiPerOndata)
            );
            Assert.That(piano.BudgetUsato, Is.LessThanOrEqualTo(
                piano.BudgetTotale
            ));

            foreach (TipoVolpe tipo in piano.Sequenza)
            {
                costoCalcolato += costi.Ottieni(tipo);
                Assert.That(
                    onda,
                    Is.GreaterThanOrEqualTo(
                        WaveThreatDirector.OndaSblocco(tipo)
                    ),
                    tipo + " compare troppo presto nell'ondata " + onda
                );
                Assert.That(
                    WaveThreatDirector.TipoConsentito(
                        piano.Archetipo,
                        tipo
                    ),
                    Is.True,
                    tipo + " non appartiene all'archetipo " +
                    piano.Archetipo + " dell'ondata " + onda
                );
            }

            Assert.That(costoCalcolato, Is.EqualTo(piano.BudgetUsato));
            Assert.That(piano.Composizione.Totale, Is.EqualTo(
                piano.TotaleVolpi
            ));
        }
    }

    [Test]
    public void RotazioneTattica_UsaTuttiGliArchetipiRichiesti()
    {
        HashSet<ArchetipoOndata> trovati = new HashSet<ArchetipoOndata>();
        for (int onda = 1; onda <= 20; onda++)
        {
            trovati.Add(CreaPiano(onda).Archetipo);
        }

        Assert.That(trovati, Does.Contain(ArchetipoOndata.Sciame));
        Assert.That(trovati, Does.Contain(ArchetipoOndata.Assedio));
        Assert.That(trovati, Does.Contain(ArchetipoOndata.Distanza));
        Assert.That(trovati, Does.Contain(ArchetipoOndata.Elite));
        Assert.That(trovati, Does.Contain(ArchetipoOndata.Boss));
    }

    [Test]
    public void LimiteContemporaneo_CresceMaNonSuperaMaiIlMassimo()
    {
        int precedente = 0;
        int[] ondate = { 1, 10, 25, 50, 100, 1000, 1000000 };
        foreach (int onda in ondate)
        {
            PianoMinacciaOndata piano = CreaPiano(onda);
            Assert.That(
                piano.LimiteContemporaneo,
                Is.GreaterThanOrEqualTo(precedente)
            );
            Assert.That(
                piano.LimiteContemporaneo,
                Is.LessThanOrEqualTo(
                    impostazioni.limiteContemporaneoMassimo
                )
            );
            precedente = piano.LimiteContemporaneo;
        }
    }

    [Test]
    public void LimiteContemporaneo_BloccaLoSpawnFincheLoSlotNonSiLibera()
    {
        Assert.That(EnemySpawner.SlotMinacciaDisponibile(5, 6), Is.True);
        Assert.That(EnemySpawner.SlotMinacciaDisponibile(6, 6), Is.False);
        Assert.That(EnemySpawner.SlotMinacciaDisponibile(8, 6), Is.False);
        Assert.That(EnemySpawner.SlotMinacciaDisponibile(0, 6), Is.True);
    }

    [Test]
    public void ProgressioneAvanzata_RestaInfinitaMaContenuta()
    {
        PianoMinacciaOndata ondaCento = CreaPiano(100);
        PianoMinacciaOndata ondaMille = CreaPiano(1000);
        PianoMinacciaOndata ondaMilione = CreaPiano(1000000);

        Assert.That(
            ondaMille.BudgetTotale,
            Is.GreaterThan(ondaCento.BudgetTotale)
        );
        Assert.That(
            ondaMilione.BudgetTotale,
            Is.GreaterThan(ondaMille.BudgetTotale)
        );
        Assert.That(
            ondaMilione.TotaleVolpi,
            Is.LessThanOrEqualTo(impostazioni.limiteVolpiPerOndata)
        );

        int vita100 = WaveThreatDirector.CalcolaVitaBase(
            100,
            2,
            1,
            impostazioni
        );
        int vita1000 = WaveThreatDirector.CalcolaVitaBase(
            1000,
            2,
            1,
            impostazioni
        );
        int vitaLineare1000 = 2 + 999;

        Assert.That(vita1000, Is.GreaterThan(vita100));
        Assert.That(vita1000, Is.LessThan(vitaLineare1000 / 10));
    }

    [Test]
    public void DistanzeSpawn_RestanoNellAnelloConfigurato()
    {
        for (int onda = 1; onda <= 30; onda++)
        {
            PianoMinacciaOndata piano = CreaPiano(onda);
            Assert.That(
                piano.DistanzaSpawnMinima,
                Is.GreaterThanOrEqualTo(
                    impostazioni.distanzaMinimaDalContadino
                )
            );
            Assert.That(
                piano.DistanzaSpawnMassima,
                Is.LessThanOrEqualTo(
                    impostazioni.distanzaMassimaDalContadino
                )
            );
            Assert.That(
                piano.DistanzaSpawnMassima,
                Is.GreaterThanOrEqualTo(piano.DistanzaSpawnMinima)
            );
        }
    }

    [Test]
    public void StessaOndata_ProduceSempreLaStessaComposizione()
    {
        PianoMinacciaOndata prima = CreaPiano(137);
        PianoMinacciaOndata seconda = CreaPiano(137);

        Assert.That(seconda.Archetipo, Is.EqualTo(prima.Archetipo));
        Assert.That(seconda.BudgetTotale, Is.EqualTo(prima.BudgetTotale));
        Assert.That(seconda.Sequenza, Is.EqualTo(prima.Sequenza));
    }

    private PianoMinacciaOndata CreaPiano(int onda)
    {
        RitmoOndata ritmo = WaveChapterDirector.Calcola(onda, capitoli);
        return WaveThreatDirector.CreaPiano(
            onda,
            impostazioni,
            ritmo
        );
    }
}
