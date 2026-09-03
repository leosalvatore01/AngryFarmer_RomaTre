using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        yield return AttendiCondizione(
            () => GameManager.instance != null &&
                  UnityEngine.Object.FindFirstObjectByType<EnemySpawner>() !=
                      null &&
                  UnityEngine.Object.FindFirstObjectByType<ShopInterOndata>(
                      FindObjectsInactive.Include
                  ) != null,
            "inizializzazione dei sistemi della scena"
        );

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

        string cartellaTelemetria = Path.Combine(
            SaveService.RadicePlayModePerTestAttiva,
            "Telemetry"
        );
        string[] runEsportate = Directory.GetFiles(
            cartellaTelemetria,
            "run_*.json"
        );
        Assert.That(runEsportate, Has.Length.EqualTo(1));
        RunTelemetryData telemetria = JsonUtility.FromJson<RunTelemetryData>(
            File.ReadAllText(runEsportate[0])
        );
        Assert.That(telemetria.conclusa, Is.True);
        Assert.That(telemetria.morteContadino, Is.True);
        Assert.That(telemetria.difficolta, Is.EqualTo("Normale"));
        Assert.That(
            File.Exists(Path.Combine(cartellaTelemetria, "partite.csv")),
            Is.True
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

    [UnityTest]
    public IEnumerator RitmoReale_CapitoloFermaLaRunSoloAgliShopPrevisti()
    {
        cartellaPlayMode = Path.Combine(
            Path.GetTempPath(),
            "AngryFarmerChapterTests_" + Guid.NewGuid().ToString("N")
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
        menu.AvviaPartita(DifficoltaPartita.Normale);

        yield return AttendiScena(
            MenuInizialeController.NomeScenaGameplay
        );
        yield return AttendiCondizione(
            () => GameManager.instance != null &&
                  UnityEngine.Object.FindFirstObjectByType<EnemySpawner>() !=
                      null &&
                  UnityEngine.Object.FindFirstObjectByType<ShopInterOndata>(
                      FindObjectsInactive.Include
                  ) != null,
            "inizializzazione dei sistemi del test capitoli"
        );

        GameManager manager = GameManager.instance;
        EnemySpawner spawner =
            UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
        ShopInterOndata shop =
            UnityEngine.Object.FindFirstObjectByType<ShopInterOndata>(
                FindObjectsInactive.Include
            );
        Assert.That(manager, Is.Not.Null);
        Assert.That(spawner, Is.Not.Null);
        Assert.That(shop, Is.Not.Null);

        yield return AttendiDifficoltaApplicata(spawner);

        spawner.ondate = CreaOndateIstantanee(12);
        ImpostaCampoPrivato(spawner, "durataBannerOndata", 0f);
        ImpostaCampoPrivato(spawner, "durataTransizioneBreve", 0.1f);
        ImpostaCampoPrivato(shop, "scelteGratuiteRimaste", 0);

        List<StatoPartita> cambiStato = new List<StatoPartita>();
        manager.StatoPartitaCambiato += cambiStato.Add;

        manager.ContinuaConOndataSuccessiva();
        yield return AttendiShopDopoOndata(manager, 2);
        Assert.That(
            cambiStato.Count(x => x == StatoPartita.Transizione),
            Is.EqualTo(1)
        );

        manager.ContinuaConOndataSuccessiva();
        yield return AttendiShopDopoOndata(manager, 4);
        Assert.That(
            cambiStato.Count(x => x == StatoPartita.Transizione),
            Is.EqualTo(2)
        );

        manager.ContinuaConOndataSuccessiva();
        yield return AttendiShopDopoOndata(manager, 7);
        Assert.That(
            cambiStato.Count(x => x == StatoPartita.Transizione),
            Is.EqualTo(4),
            "Dopo le ondate 5 e 6 la run deve ripartire da sola."
        );
        Assert.That(spawner.OttieniAnteprima(4).Elite, Is.True);
        Assert.That(spawner.OttieniAnteprima(9).Boss, Is.True);

        manager.StatoPartitaCambiato -= cambiStato.Add;
        manager.GameOverGiocatore();
        yield return null;
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

    private static IEnumerator AttendiCondizione(
        Func<bool> condizione,
        string descrizione
    )
    {
        const float timeout = 10f;
        float inizio = Time.realtimeSinceStartup;
        while (!condizione() &&
               Time.realtimeSinceStartup - inizio < timeout)
        {
            yield return null;
        }

        Assert.That(
            condizione(),
            Is.True,
            "Timeout durante " + descrizione + "."
        );
    }

    private static IEnumerator AttendiDifficoltaApplicata(
        EnemySpawner spawner
    )
    {
        const float timeout = 10f;
        float inizio = Time.realtimeSinceStartup;
        while (spawner != null &&
               !spawner.DifficoltaApplicata &&
               Time.realtimeSinceStartup - inizio < timeout)
        {
            yield return null;
        }

        Assert.That(spawner, Is.Not.Null);
        Assert.That(
            spawner.DifficoltaApplicata,
            Is.True,
            "Timeout durante l'applicazione della difficolta."
        );
    }

    private static IEnumerator AttendiShopDopoOndata(
        GameManager manager,
        int ondaCompletata
    )
    {
        const float timeout = 10f;
        float inizio = Time.realtimeSinceStartup;
        while (manager != null &&
               (manager.OndateCompletate < ondaCompletata ||
                manager.StatoCorrente != StatoPartita.Intervallo) &&
               Time.realtimeSinceStartup - inizio < timeout)
        {
            yield return null;
        }

        Assert.That(manager, Is.Not.Null);
        Assert.That(
            manager.OndateCompletate,
            Is.GreaterThanOrEqualTo(ondaCompletata),
            "Timeout in attesa dell'ondata " + ondaCompletata + "."
        );
        Assert.That(
            manager.StatoCorrente,
            Is.EqualTo(StatoPartita.Intervallo),
            "Lo shop non si e aperto dopo l'ondata " +
            ondaCompletata + "."
        );
    }

    private static Wave[] CreaOndateIstantanee(int quantita)
    {
        Wave[] risultato = new Wave[Mathf.Max(1, quantita)];
        for (int i = 0; i < risultato.Length; i++)
        {
            risultato[i] = new Wave
            {
                nomeOndata = "Test " + (i + 1),
                numeroNemici = 0,
                sequenzaVolpi = Array.Empty<TipoVolpe>(),
                intervalloTraNemici = 0.05f,
                dimensioneMassimaGruppo = 1,
                intervalloTraGruppi = 0.05f,
                numeroMaialiniBonus = 0
            };
        }
        return risultato;
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
        Assert.That(campo, Is.Not.Null, "Campo non trovato: " + nomeCampo);
        campo.SetValue(destinazione, valore);
    }
}
