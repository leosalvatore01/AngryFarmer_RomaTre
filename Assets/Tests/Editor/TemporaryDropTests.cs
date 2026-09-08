using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class TemporaryDropTests
{
    private readonly List<GameObject> oggettiCreati = new List<GameObject>();

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
    public void Catalogo_ContieneSetteDropConIconeDedicate()
    {
        IReadOnlyList<DefinizioneDropTemporaneo> catalogo =
            CatalogoDropTemporanei.Tutte;
        HashSet<TipoDropTemporaneo> tipi =
            new HashSet<TipoDropTemporaneo>();
        HashSet<FarmPixelIcon> icone = new HashSet<FarmPixelIcon>();

        for (int i = 0; i < catalogo.Count; i++)
        {
            tipi.Add(catalogo[i].Tipo);
            icone.Add(catalogo[i].Icona);
            Assert.That(catalogo[i].Nome, Is.Not.Empty);
            Assert.That(catalogo[i].Effetto, Is.Not.Empty);
        }

        Assert.That(catalogo.Count, Is.EqualTo(7));
        Assert.That(tipi.Count, Is.EqualTo(7));
        Assert.That(icone.Count, Is.EqualTo(7));
    }

    [Test]
    public void ProtezioneSfortuna_GarantisceIlDropERiparteDaZero()
    {
        FoxBalanceSettings impostazioni = new FoxBalanceSettings
        {
            probabilitaDrop = 0f,
            uccisioniSenzaDropMassime = 4
        };
        TemporaryDropDirector direttore =
            new TemporaryDropDirector(impostazioni, 71);

        Assert.That(direttore.DeveCreareDrop(1f), Is.False);
        Assert.That(direttore.DeveCreareDrop(1f), Is.False);
        Assert.That(direttore.DeveCreareDrop(1f), Is.False);
        Assert.That(direttore.EliminazioniSenzaDrop, Is.EqualTo(3));
        Assert.That(direttore.DeveCreareDrop(1f), Is.True);
        Assert.That(direttore.EliminazioniSenzaDrop, Is.Zero);
    }

    [Test]
    public void SelezioneDrop_NonRipeteOltreIlLimite()
    {
        FoxBalanceSettings impostazioni = new FoxBalanceSettings
        {
            ripetizioniMassimeStessoDrop = 1,
            pesoDropTriploSparo = 1f,
            pesoDropVelocita = 1f,
            pesoDropScudo = 0f,
            pesoDropCura = 0f,
            pesoDropCalamita = 0f,
            pesoDropFuria = 0f,
            pesoDropEsplosione = 0f
        };
        TemporaryDropDirector direttore =
            new TemporaryDropDirector(impostazioni, 12);

        TipoDropTemporaneo primo = direttore.EstraiTipo(false);
        TipoDropTemporaneo secondo = direttore.EstraiTipo(false);

        Assert.That(secondo, Is.Not.EqualTo(primo));
    }

    [Test]
    public void Elite_HannoSempreAlmenoUnDropGarantito()
    {
        SpecialEncounterBalanceSettings impostazioni =
            new SpecialEncounterBalanceSettings();
        impostazioni.dropGarantitiElite = 0;
        impostazioni.Normalizza();

        Assert.That(impostazioni.dropGarantitiElite, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public void Scudo_AssorbeLeCaricheSenzaDanneggiareIlContadino()
    {
        GameObject giocatore = CreaOggetto("ContadinoDropTest");
        PlayerTemporaryEffects effetti =
            giocatore.AddComponent<PlayerTemporaryEffects>();

        effetti.AttivaScudo();

        Assert.That(effetti.ScudoAttivo, Is.True);
        Assert.That(effetti.CaricheScudo, Is.GreaterThanOrEqualTo(1));
        Assert.That(effetti.ProvaAssorbireDanno(), Is.True);
        Assert.That(effetti.CaricheScudo, Is.Zero);
        Assert.That(effetti.ProvaAssorbireDanno(), Is.False);
    }

    [Test]
    public void Furia_AumentaDannoERiduceIntervalloDiSparo()
    {
        GameObject giocatore = CreaOggetto("ContadinoFuriaTest");
        PlayerTemporaryEffects effetti =
            giocatore.AddComponent<PlayerTemporaryEffects>();

        effetti.AttivaFuria();

        Assert.That(effetti.FuriaAttiva, Is.True);
        Assert.That(effetti.MoltiplicatoreDanno, Is.GreaterThan(1f));
        Assert.That(effetti.MoltiplicatoreIntervalloSparo, Is.LessThan(1f));
    }

    [Test]
    public void Cura_NonVieneConsumataQuandoLaVitaEPiena()
    {
        GameObject giocatore = CreaOggetto("ContadinoCuraTest");
        PlayerHealth salute = giocatore.AddComponent<PlayerHealth>();
        Richiama(salute, "Awake");
        Richiama(salute, "Start");
        PlayerTemporaryEffects effetti =
            giocatore.GetComponent<PlayerTemporaryEffects>();

        Assert.That(salute.VitaPiena, Is.True);
        Assert.That(effetti.Applica(TipoDropTemporaneo.Cura), Is.False);
    }

    private GameObject CreaOggetto(string nome)
    {
        GameObject oggetto = new GameObject(nome);
        oggettiCreati.Add(oggetto);
        return oggetto;
    }

    private static void Richiama(object bersaglio, string metodo)
    {
        bersaglio.GetType().GetMethod(
            metodo,
            BindingFlags.Instance | BindingFlags.NonPublic
        )?.Invoke(bersaglio, null);
    }
}
