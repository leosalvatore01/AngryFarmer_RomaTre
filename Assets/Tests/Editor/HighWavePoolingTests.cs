using NUnit.Framework;
using System.Reflection;
using UnityEngine;

public sealed class GameplayObjectPoolTests
{
    private GameObject prefab;

    [TearDown]
    public void TearDown()
    {
        GameplayObjectPool.AzzeraPerTest();
        if (prefab != null) Object.DestroyImmediate(prefab);
    }

    [Test]
    public void RilascioESpawn_RiutilizzanoLoStessoOggettoAzzerato()
    {
        prefab = new GameObject("PrefabPoolTest");

        GameObject primo = GameplayObjectPool.Spawn(
            prefab,
            new Vector3(2f, 3f, 0f),
            Quaternion.identity,
            2,
            "test"
        );
        int idPrimo = primo.GetInstanceID();
        primo.transform.localScale = Vector3.one * 7f;
        primo.transform.rotation = Quaternion.Euler(0f, 0f, 33f);

        Assert.That(GameplayObjectPool.Rilascia(primo), Is.True);
        Assert.That(primo.activeSelf, Is.False);

        GameObject secondo = GameplayObjectPool.Spawn(
            prefab,
            Vector3.zero,
            Quaternion.identity,
            2,
            "test"
        );
        GameplayPoolSnapshot diagnostica = GameplayObjectPool.Diagnostica;

        Assert.That(secondo.GetInstanceID(), Is.EqualTo(idPrimo));
        Assert.That(secondo.activeSelf, Is.True);
        Assert.That(secondo.transform.localScale, Is.EqualTo(Vector3.one));
        Assert.That(secondo.transform.rotation, Is.EqualTo(Quaternion.identity));
        Assert.That(diagnostica.OggettiCreati, Is.EqualTo(1));
        Assert.That(diagnostica.Riutilizzi, Is.EqualTo(1));
        Assert.That(diagnostica.OggettiInUso, Is.EqualTo(1));

        GameplayObjectPool.Rilascia(secondo);
    }

    [Test]
    public void PoolConLimite_NonAccumulaOggettiDisponibiliSenzaFine()
    {
        prefab = new GameObject("PrefabPoolLimitTest");
        GameObject[] oggetti = new GameObject[6];
        for (int i = 0; i < oggetti.Length; i++)
        {
            oggetti[i] = GameplayObjectPool.Spawn(
                prefab,
                Vector3.zero,
                Quaternion.identity,
                2,
                "limite"
            );
        }
        for (int i = 0; i < oggetti.Length; i++)
            GameplayObjectPool.Rilascia(oggetti[i]);

        GameplayPoolSnapshot diagnostica = GameplayObjectPool.Diagnostica;
        Assert.That(diagnostica.OggettiDisponibili, Is.EqualTo(2));
        Assert.That(diagnostica.OggettiInUso, Is.Zero);
        Assert.That(diagnostica.DistruzioniPerLimite, Is.EqualTo(4));
    }
}

public sealed class HighWaveStabilityTests
{
    private GameObject oggettoSpawner;

    [TearDown]
    public void TearDown()
    {
        if (oggettoSpawner != null) Object.DestroyImmediate(oggettoSpawner);
    }

    [TestCase(25)]
    [TestCase(50)]
    [TestCase(100)]
    public void OndataAvanzata_RestaEntroBudgetDiFrameMemoriaEdEntita(
        int numeroOndata
    )
    {
        oggettoSpawner = new GameObject("SpawnerStabilitaTest");
        EnemySpawner spawner = oggettoSpawner.AddComponent<EnemySpawner>();
        typeof(EnemySpawner).GetMethod(
            "ApplicaConfigurazioneBilanciamento",
            BindingFlags.Instance | BindingFlags.NonPublic
        ).Invoke(spawner, null);
        AnteprimaOndata onda = spawner.OttieniAnteprima(numeroOndata - 1);
        PoolingBalanceSettings pooling = GameBalanceConfig.Corrente.Pooling;
        HighWaveStabilityReport report =
            HighWaveStabilityAnalyzer.Analizza(onda, pooling);

        int capacitaMassima = pooling.volpiComuni + pooling.volpiElite +
            pooling.boss + pooling.proiettiliContadino + pooling.drop +
            pooling.maialini + pooling.proiettiliNemici + pooling.anelliVfx +
            pooling.lineeVfx + pooling.impulsiSecondari +
            pooling.particelleRicompensa + 28;

        Assert.That(onda.Valida, Is.True);
        Assert.That(report.NumeroOndata, Is.EqualTo(numeroOndata));
        Assert.That(
            report.PiccoVolpi,
            Is.LessThanOrEqualTo(onda.LimiteNemiciContemporanei)
        );
        Assert.That(
            report.EntitaMassimeStimate,
            Is.LessThanOrEqualTo(capacitaMassima)
        );
        Assert.That(
            report.MemoriaPoolStimataMb,
            Is.LessThanOrEqualTo(report.BudgetMemoriaMb)
        );
        Assert.That(
            report.CostoFrameStimatoMs,
            Is.LessThanOrEqualTo(report.BudgetFrameMs)
        );
        Assert.That(
            report.Stabile,
            Is.True,
            "L'ondata " + numeroOndata +
            " supera almeno un budget di stabilita."
        );
    }
}
