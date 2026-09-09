using NUnit.Framework;
using UnityEngine;

public sealed class BalanceCurveRegressionTests
{
    private GameBalanceConfig config;

    [SetUp]
    public void SetUp()
    {
        config = GameBalanceConfig.Corrente;
    }

    [Test]
    public void PrimeOndate_HannoRitmoEdEconomiaControllati()
    {
        int moneteGarantite = 0;
        for (int onda = 1; onda <= 2; onda++)
        {
            PianoMinacciaOndata piano = CreaPiano(onda);
            moneteGarantite += CalcolaMoneteVolpi(piano);
            moneteGarantite += config.Shop.bonusCompletamentoOnda;

            float finestraSpawn = CalcolaFinestraSpawn(onda, piano);
            Assert.That(
                finestraSpawn,
                Is.InRange(3f, 6f),
                "La finestra di spawn dell'ondata " + onda +
                " non accompagna gradualmente il tutorial."
            );
        }

        Assert.That(
            moneteGarantite,
            Is.InRange(8, 10),
            "Il primo shop deve offrire una scelta, non una build completa."
        );
        Assert.That(config.Shop.numeroOfferte, Is.EqualTo(4));
    }

    [Test]
    public void Shop_ApreDueVoltePerCapitoloPrimaDegliScontriSpeciali()
    {
        int aperture = 0;
        for (int onda = 1; onda <= 10; onda++)
        {
            RitmoOndata ritmo = WaveChapterDirector.Calcola(
                onda,
                config.Ondate.capitoli
            );
            if (ritmo.ApreShopDopo) aperture++;
        }

        Assert.That(aperture, Is.EqualTo(4));
        Assert.That(
            WaveChapterDirector.Calcola(9, config.Ondate.capitoli)
                .ApreShopDopo,
            Is.True,
            "Il giocatore deve potersi preparare subito prima del boss."
        );
    }

    [Test]
    public void DropTemporanei_SonoPremiUtiliMaNonCostanti()
    {
        FoxBalanceSettings volpe = config.Volpe;

        Assert.That(volpe.probabilitaDrop, Is.InRange(20f, 25f));
        Assert.That(volpe.uccisioniSenzaDropMassime, Is.InRange(5, 7));
        Assert.That(volpe.ripetizioniMassimeStessoDrop, Is.EqualTo(2));
        Assert.That(volpe.durataDropSullaMappa, Is.InRange(10f, 15f));
    }

    [Test]
    public void PrimoBoss_HaPesoDaBossSenzaFinanziareUnInteroPercorso()
    {
        SpecialEncounterBalanceSettings speciali =
            config.Ondate.IncontriSpeciali;
        int vitaBase = WaveThreatDirector.CalcolaVitaBase(
            10,
            config.Volpe.vitaPrimaOndata,
            config.Volpe.vitaAggiuntivaPerOndata,
            config.Ondate.minaccia
        );
        int vitaAlfa = Mathf.CeilToInt(
            vitaBase * config.VariantiVolpe.alfa.moltiplicatoreVita
        );
        int vitaBoss = SpecialEncounterDirector.CalcolaVitaBoss(
            vitaAlfa,
            1,
            speciali
        );
        int moneteBoss = SpecialEncounterDirector.CalcolaMoneteBoss(
            1,
            speciali
        );

        Assert.That(vitaBoss, Is.InRange(120, 170));
        Assert.That(moneteBoss, Is.InRange(24, 30));
        Assert.That(speciali.dropGarantitiBoss, Is.EqualTo(2));
    }

    [Test]
    public void ProgressionePermanente_PremiaSubitoPoiCresceGradualmente()
    {
        Assert.That(GettoniCumulativi(10), Is.EqualTo(10));
        Assert.That(GettoniCumulativi(20), Is.EqualTo(30));
        Assert.That(GettoniCumulativi(30), Is.EqualTo(60));
        Assert.That(GettoniCumulativi(50), Is.EqualTo(150));

        int costoPrimoLivelloCompleto = 0;
        foreach (DefinizionePotenziamentoPermanente definizione in
                 ProgressionePermanente.Catalogo)
        {
            costoPrimoLivelloCompleto += definizione.CalcolaCosto(0);
        }

        int ondaSbloccoCompleto = 1;
        while (GettoniCumulativi(ondaSbloccoCompleto) <
               costoPrimoLivelloCompleto)
        {
            ondaSbloccoCompleto++;
        }

        Assert.That(
            ondaSbloccoCompleto,
            Is.InRange(20, 30),
            "Tutti i primi livelli permanenti non devono arrivare in una " +
            "breve run introduttiva."
        );
    }

    [Test]
    public void RicompensaRapidaGameManager_SegueGliScaglioniDaDieciOnde()
    {
        Assert.That(
            GameManager.CalcolaRicompensePermanentiIntervallo(1, 10),
            Is.EqualTo(10)
        );
        Assert.That(
            GameManager.CalcolaRicompensePermanentiIntervallo(1, 20),
            Is.EqualTo(30)
        );
        Assert.That(
            GameManager.CalcolaRicompensePermanentiIntervallo(11, 20),
            Is.EqualTo(20)
        );
    }

    private PianoMinacciaOndata CreaPiano(int onda)
    {
        RitmoOndata ritmo = WaveChapterDirector.Calcola(
            onda,
            config.Ondate.capitoli
        );
        return WaveThreatDirector.CreaPiano(
            onda,
            config.Ondate.minaccia,
            ritmo
        );
    }

    private int CalcolaMoneteVolpi(PianoMinacciaOndata piano)
    {
        int totale = 0;
        bool specialeAssegnato = false;
        RitmoOndata ritmo = WaveChapterDirector.Calcola(
            piano.NumeroOndata,
            config.Ondate.capitoli
        );
        foreach (TipoVolpe tipo in piano.Sequenza)
        {
            RuoloIncontroSpeciale ruolo =
                SpecialEncounterDirector.ScegliRuolo(
                    ritmo.TipoIncontro,
                    tipo,
                    specialeAssegnato
                );
            if (ruolo == RuoloIncontroSpeciale.Elite)
            {
                totale += SpecialEncounterDirector.CalcolaMoneteElite(
                    ritmo.NumeroCapitolo,
                    config.Ondate.IncontriSpeciali
                );
                specialeAssegnato = true;
            }
            else if (ruolo == RuoloIncontroSpeciale.Boss)
            {
                totale += SpecialEncounterDirector.CalcolaMoneteBoss(
                    SpecialEncounterDirector.CalcolaApparizioneBoss(
                        piano.NumeroOndata,
                        config.Ondate.capitoli.bossOgniOnde
                    ),
                    config.Ondate.IncontriSpeciali
                );
                specialeAssegnato = true;
            }
            else
            {
                totale += config.VariantiVolpe.Ottieni(tipo)
                    .monetePerEliminazione;
            }
        }

        return totale;
    }

    private float CalcolaFinestraSpawn(
        int onda,
        PianoMinacciaOndata piano
    )
    {
        Wave modello = config.Ondate.ondate[
            Mathf.Clamp(onda - 1, 0, config.Ondate.ondate.Length - 1)
        ];
        int dimensioneGruppo = piano.Archetipo == ArchetipoOndata.Sciame
            ? Mathf.Max(3, modello.dimensioneMassimaGruppo)
            : modello.dimensioneMassimaGruppo;
        int gruppi = Mathf.CeilToInt(
            piano.TotaleVolpi / (float)Mathf.Max(1, dimensioneGruppo)
        );
        int intervalliInterni = Mathf.Max(0, piano.TotaleVolpi - gruppi);
        int intervalliGruppi = Mathf.Max(0, gruppi - 1);
        float moltiplicatore = piano.Archetipo == ArchetipoOndata.Sciame
            ? 0.82f
            : 1f;
        float traNemici = Mathf.Max(
            modello.intervalloTraNemici * moltiplicatore,
            config.Ondate.durataPreavvisoSpawn
        );
        float traGruppi = Mathf.Max(
            modello.intervalloTraGruppi * moltiplicatore,
            config.Ondate.durataPreavvisoSpawn
        );
        return intervalliInterni * traNemici +
               intervalliGruppi * traGruppi;
    }

    private static int GettoniCumulativi(int ondaMassima)
    {
        int totale = 0;
        for (int onda = 1; onda <= ondaMassima; onda++)
        {
            totale += ProgressionePermanente.CalcolaRicompensaOndata(onda);
        }
        return totale;
    }
}
