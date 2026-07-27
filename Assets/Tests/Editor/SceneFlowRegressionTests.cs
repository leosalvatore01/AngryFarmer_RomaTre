using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class SceneFlowRegressionTests
{
    private const string PercorsoMenu = "Assets/Scenes/MenuIniziale.unity";
    private const string PercorsoGameplay = "Assets/Scenes/SampleScene.unity";
    private string cartellaPlayMode;

    [Test]
    public void ConfigurazioneFlussoScene_HaSceneEControllerRichiesti()
    {
        EditorBuildSettingsScene[] abilitate = EditorBuildSettings.scenes
            .Where(scena => scena.enabled)
            .ToArray();

        Assert.That(abilitate, Has.Length.GreaterThanOrEqualTo(2));
        Assert.That(
            NormalizzaPercorso(abilitate[0].path),
            Is.EqualTo(PercorsoMenu)
        );
        Assert.That(
            NormalizzaPercorso(abilitate[1].path),
            Is.EqualTo(PercorsoGameplay)
        );
        Assert.That(
            MenuInizialeController.NomeScenaMenu,
            Is.EqualTo(Path.GetFileNameWithoutExtension(PercorsoMenu))
        );
        Assert.That(
            MenuInizialeController.NomeScenaGameplay,
            Is.EqualTo(Path.GetFileNameWithoutExtension(PercorsoGameplay))
        );

        VerificaScenaMenu();
        VerificaScenaGameplay();
    }

    [UnityTest]
    public IEnumerator FlussoReale_MenuPartitaGameOverMenu_Completa()
    {
        cartellaPlayMode = Path.Combine(
            Path.GetTempPath(),
            "AngryFarmerPlayModeTests_" +
            Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(cartellaPlayMode);
        SaveService.PreparaRadicePlayModePerTest(cartellaPlayMode);

        yield return new EnterPlayMode();

        SceneManager.LoadScene(MenuInizialeController.NomeScenaMenu);
        yield return AttendiScena(MenuInizialeController.NomeScenaMenu);
        yield return null;

        MenuInizialeController menu =
            UnityEngine.Object.FindFirstObjectByType<
                MenuInizialeController
            >();
        Assert.That(menu, Is.Not.Null);
        Assert.That(menu.InterfacciaCostruita, Is.True);

        menu.AvviaPartita(DifficoltaPartita.Normale);
        yield return AttendiScena(
            MenuInizialeController.NomeScenaGameplay
        );
        yield return null;

        GameManager manager =
            UnityEngine.Object.FindFirstObjectByType<GameManager>();
        Assert.That(manager, Is.Not.Null);
        Assert.That(
            UnityEngine.Object.FindFirstObjectByType<PlayerHealth>(),
            Is.Not.Null
        );

        manager.GameOverGiocatore();
        yield return null;

        Assert.That(manager.isGameOver, Is.True);
        Assert.That(
            manager.StatoCorrente,
            Is.EqualTo(StatoPartita.FinePartita)
        );

        manager.TornaAlMenuPrincipale();
        yield return AttendiScena(MenuInizialeController.NomeScenaMenu);
        yield return null;

        Assert.That(
            UnityEngine.Object.FindFirstObjectByType<
                MenuInizialeController
            >(),
            Is.Not.Null
        );
        Assert.That(
            UnityEngine.Object.FindFirstObjectByType<GameManager>(),
            Is.Null
        );

        yield return new ExitPlayMode();
    }

    [UnityTearDown]
    public IEnumerator RipristinaAmbientePlayMode()
    {
        if (Application.isPlaying)
        {
            yield return new ExitPlayMode();
        }

        string radiceTemporanea =
            SaveService.RadicePlayModePerTestAttiva;
        SaveService.RimuoviRadicePlayModePerTest();
        if (!string.IsNullOrWhiteSpace(radiceTemporanea) &&
            Directory.Exists(radiceTemporanea))
        {
            Directory.Delete(radiceTemporanea, true);
        }
        cartellaPlayMode = null;
    }

    private static void VerificaScenaMenu()
    {
        EseguiConScena(PercorsoMenu, scena =>
        {
            MenuInizialeController controller =
                TrovaComponente<MenuInizialeController>(scena);

            Assert.That(
                controller,
                Is.Not.Null,
                "La scena menu deve contenere MenuInizialeController."
            );
            Assert.That(
                typeof(MenuInizialeController).GetMethod("AvviaPartita"),
                Is.Not.Null
            );
        });
    }

    private static void VerificaScenaGameplay()
    {
        EseguiConScena(PercorsoGameplay, scena =>
        {
            GameManager manager = TrovaComponente<GameManager>(scena);
            EnemySpawner spawner = TrovaComponente<EnemySpawner>(scena);
            PlayerHealth contadino = TrovaComponente<PlayerHealth>(scena);

            Assert.That(manager, Is.Not.Null);
            Assert.That(spawner, Is.Not.Null);
            Assert.That(
                contadino,
                Is.Not.Null,
                "La scena gameplay deve contenere il contadino."
            );
            Assert.That(
                contadino.CompareTag("Player"),
                Is.True,
                "Il contadino deve mantenere il tag Player."
            );
            Assert.That(
                typeof(GameManager).GetMethod("GameOverGiocatore"),
                Is.Not.Null
            );
            Assert.That(
                typeof(GameManager).GetMethod("TornaAlMenuPrincipale"),
                Is.Not.Null
            );
        });
    }

    private static void EseguiConScena(
        string percorso,
        Action<Scene> verifica
    )
    {
        Scene scena = SceneManager.GetSceneByPath(percorso);
        bool apertaDalTest = !scena.IsValid() || !scena.isLoaded;

        if (apertaDalTest)
        {
            scena = EditorSceneManager.OpenScene(
                percorso,
                OpenSceneMode.Additive
            );
        }

        try
        {
            verifica(scena);
        }
        finally
        {
            if (apertaDalTest && scena.IsValid() && scena.isLoaded)
            {
                EditorSceneManager.CloseScene(scena, true);
            }
        }
    }

    private static T TrovaComponente<T>(Scene scena)
        where T : Component
    {
        foreach (GameObject radice in scena.GetRootGameObjects())
        {
            T componente = radice.GetComponentInChildren<T>(true);
            if (componente != null)
            {
                return componente;
            }
        }

        return null;
    }

    private static string NormalizzaPercorso(string percorso)
    {
        return percorso.Replace('\\', '/');
    }

    private static IEnumerator AttendiScena(string nomeScena)
    {
        const float timeout = 10f;
        float inizio = Time.realtimeSinceStartup;

        while (SceneManager.GetActiveScene().name != nomeScena &&
               Time.realtimeSinceStartup - inizio < timeout)
        {
            yield return null;
        }

        Assert.That(
            SceneManager.GetActiveScene().name,
            Is.EqualTo(nomeScena),
            "Timeout durante il caricamento della scena " + nomeScena + "."
        );
    }
}
