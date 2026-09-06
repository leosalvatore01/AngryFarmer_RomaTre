using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class BuildEvolutionTests
{
    private readonly List<GameObject> oggettiCreati =
        new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        for (int i = oggettiCreati.Count - 1; i >= 0; i--)
        {
            if (oggettiCreati[i] != null)
                Object.DestroyImmediate(oggettiCreati[i]);
        }
        oggettiCreati.Clear();
    }

    [Test]
    public void Catalogo_ContieneQuattroEvoluzioniDistinteASogliaTreESei()
    {
        IReadOnlyList<DefinizioneEvoluzioneBuild> evoluzioni =
            CatalogoEvoluzioniBuild.Tutte;
        HashSet<PercorsoBuild> percorsi = new HashSet<PercorsoBuild>();
        HashSet<FarmPixelIcon> icone = new HashSet<FarmPixelIcon>();
        ShopBalanceSettings configurazione =
            GameBalanceConfig.Corrente.Shop;

        Assert.That(evoluzioni.Count, Is.EqualTo(4));
        Assert.That(
            CatalogoEvoluzioniBuild.SogliaPrimoStadio(configurazione),
            Is.EqualTo(3)
        );
        Assert.That(
            CatalogoEvoluzioniBuild.SogliaSecondoStadio(configurazione),
            Is.EqualTo(6)
        );
        for (int i = 0; i < evoluzioni.Count; i++)
        {
            Assert.That(percorsi.Add(evoluzioni[i].Percorso), Is.True);
            Assert.That(icone.Add(evoluzioni[i].Icona), Is.True);
            Assert.That(evoluzioni[i].NomePrimoStadio, Is.Not.Empty);
            Assert.That(evoluzioni[i].NomeSecondoStadio, Is.Not.Empty);
        }
    }

    [TestCase(TipoPotenziamento.Cadenza, PercorsoBuild.Raffica)]
    [TestCase(TipoPotenziamento.Danno, PercorsoBuild.Artiglieria)]
    [TestCase(TipoPotenziamento.Critico, PercorsoBuild.Perforazione)]
    [TestCase(TipoPotenziamento.Spinta, PercorsoBuild.Controllo)]
    public void Percorso_SbloccaEvoluzioniAlTerzoESestoPunto(
        TipoPotenziamento potenziamento,
        PercorsoBuild percorso
    )
    {
        PlayerUpgrades upgrades = CreaGiocatore();

        Applica(upgrades, potenziamento, 2);
        Assert.That(upgrades.OttieniLivelloEvoluzione(percorso), Is.Zero);
        StringAssert.Contains(
            "SBLOCCA EVOLUZIONE",
            upgrades.OttieniBonusProssimoLivello(potenziamento)
        );

        Assert.That(
            upgrades.ProvaApplicareGratis(
                potenziamento,
                out string messaggioSblocco
            ),
            Is.True
        );
        Assert.That(upgrades.OttieniLivelloEvoluzione(percorso), Is.EqualTo(1));
        Assert.That(upgrades.OttieniNomeEvoluzione(percorso), Is.Not.Empty);
        StringAssert.Contains(
            "EVOLUZIONE SBLOCCATA",
            messaggioSblocco
        );

        Applica(upgrades, potenziamento, 3);
        Assert.That(upgrades.OttieniLivelloEvoluzione(percorso), Is.EqualTo(2));
        StringAssert.Contains("E2", upgrades.DescriviBuildCompatta());
    }

    [Test]
    public void Raffica_EvolveDaDueATreFrammenti()
    {
        PlayerUpgrades upgrades = CreaGiocatore();

        Applica(upgrades, TipoPotenziamento.Cadenza, 3);
        ProfiloProiettileBuild primo = upgrades.CreaProfiloProiettile(false);
        Assert.That(primo.NumeroFrammenti, Is.EqualTo(2));
        Assert.That(primo.SiDivide, Is.True);

        Applica(upgrades, TipoPotenziamento.Cadenza, 3);
        ProfiloProiettileBuild secondo = upgrades.CreaProfiloProiettile(false);
        Assert.That(secondo.NumeroFrammenti, Is.EqualTo(3));
        Assert.That(
            secondo.MoltiplicatoreDannoFrammento,
            Is.GreaterThan(primo.MoltiplicatoreDannoFrammento)
        );
    }

    [Test]
    public void Artiglieria_AggiungeEsplosioniSecondarie()
    {
        PlayerUpgrades upgrades = CreaGiocatore();

        Applica(upgrades, TipoPotenziamento.Danno, 3);
        ProfiloProiettileBuild primo = upgrades.CreaProfiloProiettile(false);
        Assert.That(primo.RaggioEsplosione, Is.GreaterThan(0f));
        Assert.That(primo.NumeroEsplosioniSecondarie, Is.EqualTo(1));

        Applica(upgrades, TipoPotenziamento.Danno, 3);
        ProfiloProiettileBuild secondo = upgrades.CreaProfiloProiettile(false);
        Assert.That(secondo.NumeroEsplosioniSecondarie, Is.EqualTo(2));
        Assert.That(secondo.HaEsplosioniSecondarie, Is.True);
    }

    [Test]
    public void Perforazione_AttraversaSempreEPoiRimbalza()
    {
        PlayerUpgrades upgrades = CreaGiocatore();

        Applica(upgrades, TipoPotenziamento.Critico, 3);
        ProfiloProiettileBuild primo = upgrades.CreaProfiloProiettile(false);
        Assert.That(primo.PenetrazioneIncondizionata, Is.True);
        Assert.That(primo.Penetrazioni, Is.GreaterThanOrEqualTo(1));

        Applica(upgrades, TipoPotenziamento.Critico, 3);
        ProfiloProiettileBuild secondo = upgrades.CreaProfiloProiettile(false);
        Assert.That(secondo.Rimbalzi, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public void Controllo_PropagaUnRallentamentoSemprePiuAmpio()
    {
        PlayerUpgrades upgrades = CreaGiocatore();

        Applica(upgrades, TipoPotenziamento.Spinta, 3);
        ProfiloProiettileBuild primo = upgrades.CreaProfiloProiettile(false);
        Assert.That(primo.Rallentante, Is.True);
        Assert.That(primo.PropagaRallentamento, Is.True);

        Applica(upgrades, TipoPotenziamento.Spinta, 3);
        ProfiloProiettileBuild secondo = upgrades.CreaProfiloProiettile(false);
        Assert.That(
            secondo.RaggioPropagazioneRallentamento,
            Is.GreaterThan(primo.RaggioPropagazioneRallentamento)
        );
        Assert.That(
            secondo.IntensitaPropagazioneRallentamento,
            Is.GreaterThan(primo.IntensitaPropagazioneRallentamento)
        );
    }

    [Test]
    public void CrescitaDanno_RestaInfinitaConRendimentoDecrescente()
    {
        PlayerUpgrades upgrades = CreaGiocatore();
        PlayerShooting sparo = upgrades.GetComponent<PlayerShooting>();
        int dannoBase = sparo.DannoFinale;

        Applica(upgrades, TipoPotenziamento.Danno, 6);
        int dannoAlSei = sparo.DannoFinale;
        Applica(upgrades, TipoPotenziamento.Danno, 6);
        int dannoAlDodici = sparo.DannoFinale;
        Applica(upgrades, TipoPotenziamento.Danno, 48);
        int dannoAlSessanta = sparo.DannoFinale;

        Assert.That(dannoAlSei - dannoBase, Is.GreaterThan(0));
        Assert.That(
            dannoAlDodici - dannoAlSei,
            Is.LessThan(dannoAlSei - dannoBase)
        );
        Assert.That(dannoAlDodici, Is.GreaterThan(dannoAlSei));
        Assert.That(dannoAlSessanta, Is.GreaterThan(dannoAlDodici));
        Assert.That(upgrades.OttieniLivello(TipoPotenziamento.Danno),
            Is.EqualTo(60));
        Assert.That(upgrades.OttieniLivelloMassimo(
            TipoPotenziamento.Danno), Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void CrescitaVita_RestaInfinitaConRendimentoDecrescente()
    {
        PlayerUpgrades upgrades = CreaGiocatore();
        PlayerHealth salute = upgrades.GetComponent<PlayerHealth>();
        int vitaBase = salute.VitaMassimaFinale;

        Applica(upgrades, TipoPotenziamento.SaluteMassima, 6);
        int vitaAlSei = salute.VitaMassimaFinale;
        Applica(upgrades, TipoPotenziamento.SaluteMassima, 6);
        int vitaAlDodici = salute.VitaMassimaFinale;
        Applica(upgrades, TipoPotenziamento.SaluteMassima, 48);
        int vitaAlSessanta = salute.VitaMassimaFinale;

        Assert.That(vitaAlSei - vitaBase, Is.GreaterThan(0));
        Assert.That(
            vitaAlDodici - vitaAlSei,
            Is.LessThan(vitaAlSei - vitaBase)
        );
        Assert.That(vitaAlDodici, Is.GreaterThan(vitaAlSei));
        Assert.That(vitaAlSessanta, Is.GreaterThan(vitaAlDodici));
        Assert.That(
            upgrades.OttieniLivello(TipoPotenziamento.SaluteMassima),
            Is.EqualTo(60)
        );
    }

    private PlayerUpgrades CreaGiocatore()
    {
        GameObject oggetto = new GameObject("Player_Evoluzioni_Test");
        oggettiCreati.Add(oggetto);
        Rigidbody2D corpo = oggetto.AddComponent<Rigidbody2D>();
        corpo.bodyType = RigidbodyType2D.Kinematic;
        oggetto.AddComponent<PlayerMovement>();
        oggetto.AddComponent<PlayerHealth>();
        oggetto.AddComponent<PlayerShooting>();
        PlayerUpgrades upgrades = oggetto.GetComponent<PlayerUpgrades>();
        if (upgrades == null) upgrades = oggetto.AddComponent<PlayerUpgrades>();
        typeof(PlayerUpgrades).GetMethod(
            "Awake",
            BindingFlags.Instance | BindingFlags.NonPublic
        ).Invoke(upgrades, null);
        return upgrades;
    }

    private static void Applica(
        PlayerUpgrades upgrades,
        TipoPotenziamento tipo,
        int quantita
    )
    {
        for (int i = 0; i < quantita; i++)
        {
            Assert.That(
                upgrades.ProvaApplicareGratis(tipo, out string messaggio),
                Is.True,
                messaggio
            );
        }
    }
}
