using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class BuildArchetypeTests
{
    [Test]
    public void Catalogo_ContieneQuattroIdentitaDistinte()
    {
        IReadOnlyList<DefinizioneArchetipoBuild> archetipi =
            CatalogoArchetipiBuild.Tutte;
        HashSet<ArchetipoBuild> tipi = new HashSet<ArchetipoBuild>();
        HashSet<TipoPotenziamento> bonus =
            new HashSet<TipoPotenziamento>();
        HashSet<FarmPixelIcon> icone = new HashSet<FarmPixelIcon>();

        Assert.That(archetipi.Count, Is.EqualTo(4));
        for (int i = 0; i < archetipi.Count; i++)
        {
            DefinizioneArchetipoBuild archetipo = archetipi[i];
            Assert.That(tipi.Add(archetipo.Tipo), Is.True);
            Assert.That(bonus.Add(archetipo.PotenziamentoIniziale), Is.True);
            Assert.That(icone.Add(archetipo.Icona), Is.True);
            Assert.That(archetipo.StileGioco, Is.Not.Empty);
            Assert.That(archetipo.EtichettaSinergie, Is.Not.Empty);
        }
    }

    [Test]
    public void PesiShop_OrientanoMaNonEscludonoNessunaCarta()
    {
        ShopBalanceSettings configurazione =
            GameBalanceConfig.Corrente.Shop;

        foreach (DefinizioneArchetipoBuild archetipo in
                 CatalogoArchetipiBuild.Tutte)
        {
            float pesoMinimo = float.MaxValue;
            float pesoMassimo = float.MinValue;
            foreach (DefinizionePotenziamentoBuild offerta in
                     CatalogoPotenziamentiBuild.Tutte)
            {
                float peso = CatalogoArchetipiBuild.MoltiplicatoreOfferta(
                    archetipo.Tipo,
                    offerta,
                    configurazione
                );
                Assert.That(peso, Is.GreaterThan(0f));
                pesoMinimo = Mathf.Min(pesoMinimo, peso);
                pesoMassimo = Mathf.Max(pesoMassimo, peso);
            }

            Assert.That(pesoMinimo, Is.EqualTo(1f).Within(0.001f));
            Assert.That(pesoMassimo, Is.GreaterThan(1f));
        }
    }

    [Test]
    public void SceltaArchetipo_RestaFissaPerLaPartita()
    {
        GameObject oggetto = new GameObject("PlayerUpgrades_Archetipo_Test");
        try
        {
            PlayerUpgrades potenziamenti =
                oggetto.AddComponent<PlayerUpgrades>();

            Assert.That(
                potenziamenti.ProvaImpostaArchetipoIniziale(
                    ArchetipoBuild.Artigliere
                ),
                Is.True
            );
            Assert.That(
                potenziamenti.ProvaImpostaArchetipoIniziale(
                    ArchetipoBuild.Tiratore
                ),
                Is.False
            );
            Assert.That(
                potenziamenti.ArchetipoIniziale,
                Is.EqualTo(ArchetipoBuild.Artigliere)
            );
            StringAssert.Contains(
                "ARTIGLIERE",
                potenziamenti.DescriviBuildCompatta()
            );
        }
        finally
        {
            Object.DestroyImmediate(oggetto);
        }
    }

    [Test]
    public void GeneratoreConArchetipo_PuoProporreTuttiIPercorsi()
    {
        GameObject oggetto = new GameObject("Generatore_Archetipo_Test");
        try
        {
            Rigidbody2D corpo = oggetto.AddComponent<Rigidbody2D>();
            corpo.bodyType = RigidbodyType2D.Kinematic;
            oggetto.AddComponent<PlayerMovement>();
            oggetto.AddComponent<PlayerHealth>();
            oggetto.AddComponent<PlayerShooting>();
            PlayerUpgrades potenziamenti =
                oggetto.GetComponent<PlayerUpgrades>();
            if (potenziamenti == null)
                potenziamenti = oggetto.AddComponent<PlayerUpgrades>();
            typeof(PlayerUpgrades).GetMethod(
                "Awake",
                BindingFlags.Instance | BindingFlags.NonPublic
            ).Invoke(potenziamenti, null);
            GeneratoreOfferteBuild generatore =
                new GeneratoreOfferteBuild(73031);
            HashSet<PercorsoBuild> percorsi =
                new HashSet<PercorsoBuild>();

            for (int i = 0; i < 250; i++)
            {
                List<TipoPotenziamento> offerte = generatore.Genera(
                    potenziamenti,
                    50,
                    1000,
                    4,
                    null,
                    PercorsoBuild.Raffica,
                    true,
                    ArchetipoBuild.Tiratore
                );
                for (int j = 0; j < offerte.Count; j++)
                {
                    DefinizionePotenziamentoBuild definizione =
                        CatalogoPotenziamentiBuild.Ottieni(offerte[j]);
                    if (definizione.Percorso != PercorsoBuild.Utilita)
                        percorsi.Add(definizione.Percorso);
                }
            }

            Assert.That(percorsi, Does.Contain(PercorsoBuild.Raffica));
            Assert.That(percorsi, Does.Contain(PercorsoBuild.Artiglieria));
            Assert.That(percorsi, Does.Contain(PercorsoBuild.Perforazione));
            Assert.That(percorsi, Does.Contain(PercorsoBuild.Controllo));
        }
        finally
        {
            Object.DestroyImmediate(oggetto);
        }
    }
}
