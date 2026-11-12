using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed class BackgroundStreamingTests
{
    [UnityTest]
    public IEnumerator Esplorazione_NonAccumulaTextureENonAggiungeOstacoli()
    {
        string root = Path.Combine(Path.GetTempPath(), "AngryFarmerBackground_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        SaveService.PreparaRadicePlayModePerTest(root);
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, false);
        yield return new EnterPlayMode();
        SceneManager.LoadScene(MenuInizialeController.NomeScenaGameplay);
        yield return null;
        yield return null;
        Camera camera = Camera.main;
        Assert.That(camera, Is.Not.Null);
        camera.GetComponent<CameraFollow>().enabled = false;
        GameObject dettagli = GameObject.Find("DettagliFattoriaDinamici");
        Assert.That(dettagli, Is.Not.Null);
        // Attraversiamo molte piu' zone di quante possano essere residenti insieme,
        // anche a coordinate negative: una fuga di texture non puo' nascondersi.
        for (int i = 0; i < 18; i++)
        {
            camera.transform.position = new Vector3((i - 9) * 14f, (i % 3 - 1) * 14f, -10f);
            yield return null;
            yield return null;
            Assert.That(dettagli.GetComponentsInChildren<Collider2D>().Length, Is.Zero,
                "Il paesaggio deve restare attraversabile.");
            int textureSentieri = Resources.FindObjectsOfTypeAll<Texture2D>()
                .Count(t => t.name.StartsWith("Sentiero_", StringComparison.Ordinal));
            Assert.That(textureSentieri, Is.LessThanOrEqualTo(81),
                "Le texture delle zone abbandonate devono essere rilasciate.");
        }
        yield return new ExitPlayMode();
    }

    [UnityTearDown]
    public IEnumerator Ripristina()
    {
        if (Application.isPlaying) yield return new ExitPlayMode();
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, false);
        string root = SaveService.RadicePlayModePerTestAttiva;
        SaveService.RimuoviRadicePlayModePerTest();
        if (!string.IsNullOrEmpty(root) && Directory.Exists(root)) Directory.Delete(root, true);
    }
}
