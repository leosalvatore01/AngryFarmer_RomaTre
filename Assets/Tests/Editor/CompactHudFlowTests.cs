using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class CompactHudFlowTests
{
    [UnityTest]
    public IEnumerator Pc_EffettiBossEPausa_NonCopronoLeInformazioniEssenziali()
    {
        string root = Path.Combine(Path.GetTempPath(), "AngryFarmerHud_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        SaveService.PreparaRadicePlayModePerTest(root);
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, false);
        yield return new EnterPlayMode();
        SceneManager.LoadScene(MenuInizialeController.NomeScenaMenu);
        yield return Attendi(() => Object.FindFirstObjectByType<MenuInizialeController>() != null);
        GameOptionsController.Instance.ImpostaTutorialCompletato(true);
        Object.FindFirstObjectByType<MenuInizialeController>().AvviaPartita(DifficoltaPartita.Normale);
        yield return Attendi(() => Object.FindFirstObjectByType<ShopInterOndata>(FindObjectsInactive.Include)?.PreparazioneInizialeAttiva == true);
        var shop = Object.FindFirstObjectByType<ShopInterOndata>(FindObjectsInactive.Include);
        while (shop.ScelteGratuiteRimaste > 0)
        {
            yield return Attendi(() => Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .Any(b => b.name == "Acquista" && b.IsInteractable()));
            Premi("Acquista");
            yield return null;
        }
        Premi("OndataSuccessiva");
        yield return Attendi(() => GameManager.instance.GameplayAttivo);
        // Aspettiamo lo spawn reale, non un tempo fisso: la coroutine puo'
        // iniziare dopo il frame che chiude lo shop.
        yield return Attendi(() => Object.FindFirstObjectByType<EnemySpawner>().ProgressoCorrente.GruppoCorrente > 0 &&
            GameObject.Find("PannelloMessaggioOndata") == null);
        yield return null;
        var hud = PowerUpHudController.CreaOTrova();
        hud.AggiornaSubitoPerTest();
        Assert.That(hud.NumeroSlotVisibili, Is.Zero, "La build non deve riempire l'HUD durante il combattimento.");
        MobileVisualCapture.Salva("hud-pc-16-9");
        var player = Object.FindFirstObjectByType<PlayerShooting>();
        player.AttivaTriploSparo();
        Object.FindFirstObjectByType<PlayerMovement>().AttivaBoostVelocita();
        hud.AggiornaSubitoPerTest();
        Assert.That(hud.NumeroEffettiTemporaneiAttivi, Is.EqualTo(2));
        Assert.That(hud.NumeroSlotVisibili, Is.EqualTo(2));
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Volpe.prefab");
        EnemyAI volpe = Object.Instantiate(prefab, player.transform.position + new Vector3(3, 1, 0), Quaternion.identity).GetComponent<EnemyAI>();
        BossFoxController.Configura(volpe, 10);
        yield return null;
        yield return null;
        var onda = GameObject.Find("PannelloOndataCompatto").GetComponent<RectTransform>();
        var boss = GameObject.Find("PannelloBoss").GetComponent<RectTransform>();
        Assert.That(Rettangolo(onda).Overlaps(Rettangolo(boss)), Is.False, "La barra boss non deve coprire il contatore ondata.");
        Rect vitaBoss = Rettangolo(boss.Find("FondoBarra").GetComponent<RectTransform>());
        Assert.That(Rettangolo(boss).Contains(vitaBoss.min) && Rettangolo(boss).Contains(vitaBoss.max), Is.True,
            "Il riempimento della vita deve restare dentro la scheda del boss.");
        MobileVisualCapture.Salva("hud-boss-pc-16-9");
        MobileVisualCapture.Salva("hud-boss-pc-4-3", 1440, 1080);
        PauseSettingsMenu.Instance.Mostra();
        Premi("SchedaBuild");
        yield return null;
        Assert.That(Time.timeScale, Is.Zero);
        MobileVisualCapture.Salva("build-pausa-pc-16-9");
        PauseSettingsMenu.Instance.Nascondi();
        yield return null;
        Assert.That(Object.FindObjectsByType<PowerUpHudController>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        Assert.That(GameManager.instance.GameplayAttivo, Is.True);
        yield return new ExitPlayMode();
    }

    private static void Premi(string nome) => Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
        .First(b => b.name == nome && b.isActiveAndEnabled && b.IsInteractable()).onClick.Invoke();

    private static Rect Rettangolo(RectTransform rect)
    {
        Vector3[] angoli = new Vector3[4];
        rect.GetWorldCorners(angoli);
        return Rect.MinMaxRect(angoli[0].x, angoli[0].y, angoli[2].x, angoli[2].y);
    }

    private static IEnumerator Attendi(Func<bool> condizione)
    {
        float limite = Time.realtimeSinceStartup + 12f;
        while (!condizione() && Time.realtimeSinceStartup < limite) yield return null;
        Assert.That(condizione(), Is.True, "Timeout nel flusso HUD.");
    }

    [UnityTearDown]
    public IEnumerator Ripristina()
    {
        if (Application.isPlaying) yield return new ExitPlayMode();
        string root = SaveService.RadicePlayModePerTestAttiva;
        SaveService.RimuoviRadicePlayModePerTest();
        if (!string.IsNullOrEmpty(root) && Directory.Exists(root)) Directory.Delete(root, true);
    }
}
