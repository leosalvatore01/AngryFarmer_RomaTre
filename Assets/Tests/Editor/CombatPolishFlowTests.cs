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

public sealed class CombatPolishFlowTests
{
    [UnityTest]
    public IEnumerator ImpattiRicompenseEPausa_NonAlteranoEconomiaOCasualita()
    {
        string root = Path.Combine(Path.GetTempPath(), "AngryFarmerCombat_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        SaveService.PreparaRadicePlayModePerTest(root);
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, false);
        yield return new EnterPlayMode();
        yield return EseguiFlussoCombattimento();
        yield return new ExitPlayMode();
    }

    private static IEnumerator EseguiFlussoCombattimento()
    {
        SceneManager.LoadScene(MenuInizialeController.NomeScenaMenu);
        yield return Attendi(() => Object.FindFirstObjectByType<MenuInizialeController>() != null);
        GameOptionsController.Instance.ImpostaTutorialCompletato(true);
        Object.FindFirstObjectByType<MenuInizialeController>().AvviaPartita(DifficoltaPartita.Normale);
        yield return Attendi(() => Object.FindFirstObjectByType<ShopInterOndata>(FindObjectsInactive.Include)?.PreparazioneInizialeAttiva == true);
        var shop = Object.FindFirstObjectByType<ShopInterOndata>(FindObjectsInactive.Include);
        while (shop.ScelteGratuiteRimaste > 0)
        {
            yield return Attendi(() => Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b => b.name == "Acquista" && b.IsInteractable()));
            Premi("Acquista");
            yield return null;
        }
        Premi("OndataSuccessiva");
        yield return Attendi(() => GameManager.instance != null && GameManager.instance.GameplayAttivo);
        yield return Attendi(() => Object.FindFirstObjectByType<EnemySpawner>().ProgressoCorrente.GruppoCorrente > 0 &&
            GameObject.Find("PannelloMessaggioOndata") == null);
        // Isoliamo il bersaglio di prova: le ondate ordinarie sono gia' coperte
        // dai test del flusso e non devono interferire con il saldo verificato.
        Object.FindFirstObjectByType<EnemySpawner>().StopAllCoroutines();
        GameManager manager = GameManager.instance;
        PlayerShooting giocatore = Object.FindFirstObjectByType<PlayerShooting>();
        Assert.That(giocatore != null, Is.True, "Giocatore assente nella scena " + SceneManager.GetActiveScene().name);
        Vector3 posizioneGiocatore = giocatore.transform.position;
        CombatFeedbackController feedback = CombatFeedbackController.CreaOTrova();
        Assert.That(feedback != null, Is.True, "Controller feedback non disponibile.");
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Volpe.prefab");
        Assert.That(prefab != null, Is.True, "Prefab volpe assente.");
        GameObject istanza = Object.Instantiate(prefab, posizioneGiocatore + new Vector3(1.7f, 0, 0), Quaternion.identity);
        Assert.That(istanza != null, Is.True, "Il bersaglio deve restare attivo dopo Awake.");
        EnemyAI volpe = istanza.GetComponent<EnemyAI>();
        Assert.That(volpe != null, Is.True, "EnemyAI non presente nel prefab istanziato.");
        yield return null;
        volpe.InizializzaVariante(TipoVolpe.Comune, 2, 0, false);
        volpe.ImpostaControlloEsterno(true);
        int impattiPrima = feedback.ImpattiRegistrati;
        Spara(giocatore.transform.position, false);
        yield return Attendi(() => volpe.VitaCorrente == 1);
        Assert.That(feedback.ImpattiRegistrati, Is.GreaterThan(impattiPrima));
        int saldoPrima = manager.monete;
        int ricompensa = volpe.MonetePerEliminazione;
        Spara(giocatore.transform.position, true);
        yield return Attendi(() => volpe.IsDead);
        Assert.That(manager.monete, Is.EqualTo(saldoPrima + ricompensa), "La ricompensa e' immediata e non dipende dall'animazione.");
        var premio = Object.FindObjectsByType<RewardBurstFeedback>(FindObjectsSortMode.None).First();
        manager.ImpostaPausaManuale(true);
        float progresso = premio.Progresso;
        float finePausa = Time.realtimeSinceStartup + 0.1f;
        while (Time.realtimeSinceStartup < finePausa) yield return null;
        Assert.That(premio.Progresso, Is.EqualTo(progresso).Within(0.00001f));
        MobileVisualCapture.Salva("combattimento-impatto-16-9");
        manager.ImpostaPausaManuale(false);
        for (int i = 0; i < 14; i++)
        {
            MobileVisualCapture.Salva("combattimento-frame-" + i.ToString("D2"), 1280, 720);
            float prossimoFrame = Time.time + 0.04f;
            while (Time.time < prossimoFrame) yield return null;
        }
        Assert.That(manager.monete, Is.EqualTo(saldoPrima + ricompensa), "Il volo delle monete non deve accreditarle una seconda volta.");

        UnityEngine.Random.State stato = UnityEngine.Random.state;
        UnityEngine.Random.InitState(9183);
        float atteso = UnityEngine.Random.value;
        UnityEngine.Random.InitState(9183);
        for (int i = 0; i < 100; i++) feedback.RegistraEliminazione(giocatore.transform.position + Vector3.right, null, 3);
        Assert.That(UnityEngine.Random.value, Is.EqualTo(atteso), "Gli effetti cosmetici non devono cambiare spawn e drop.");
        UnityEngine.Random.state = stato;
        Assert.That(Object.FindObjectsByType<RewardBurstFeedback>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length,
            Is.EqualTo(feedback.CapacitaRicompense), "Il pool deve restare limitato anche dopo molte eliminazioni.");
        Assert.That(manager.monete, Is.EqualTo(saldoPrima + ricompensa));
        GameOptionsController.Instance.ImpostaFlashAttivi(false);
        feedback.RegistraImpatto(giocatore.transform.position + Vector3.right, Vector2.right, null, true, false, true);
        yield return null;
        Assert.That(Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
            .Any(r => r.name == "ScintillaImpatto" && r.enabled), Is.False);
        feedback.VfxAbilitati = false;
        yield return null;
        Assert.That(Object.FindObjectsByType<RewardBurstFeedback>(FindObjectsSortMode.None).Length, Is.Zero);
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, true);
        GameOptionsController.Instance.ImpostaQualitaMobileRidotta(true);
        feedback.VfxAbilitati = true;
        feedback.RegistraEliminazione(giocatore.transform.position + Vector3.right, null, 3);
        var premioLeggero = Object.FindObjectsByType<RewardBurstFeedback>(FindObjectsSortMode.None).Single();
        Assert.That(premioLeggero.GetComponentsInChildren<SpriteRenderer>()
            .Count(r => r.name.StartsWith("MonetaCosmetica_", StringComparison.Ordinal) && r.enabled), Is.EqualTo(1));
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, false);
    }

    private static void Spara(Vector3 posizione, bool critico)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Proiettile.prefab");
        var patata = Object.Instantiate(prefab, posizione + Vector3.right * 0.5f, Quaternion.identity).GetComponent<Proiettile>();
        patata.InizializzaBuild(new ProfiloProiettileBuild { Danno = 1, Scala = 1, MoltiplicatoreVelocita = 1, Critico = critico }, critico, false);
        patata.GetComponent<Rigidbody2D>().linearVelocity = Vector2.right * 9f;
        Object.FindFirstObjectByType<PlayerVisualController>().RiproduciFeedbackSparo(Vector2.right, critico);
    }

    private static void Premi(string nome) => Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
        .First(b => b.name == nome && b.isActiveAndEnabled && b.IsInteractable()).onClick.Invoke();

    private static IEnumerator Attendi(Func<bool> condizione)
    {
        float limite = Time.realtimeSinceStartup + 12f;
        while (!condizione() && Time.realtimeSinceStartup < limite) yield return null;
        Assert.That(condizione(), Is.True, "Timeout nel flusso del combattimento.");
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
