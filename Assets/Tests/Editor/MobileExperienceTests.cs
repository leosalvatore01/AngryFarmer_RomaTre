using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class MobileExperienceTests
{
    [TestCase(1920, 1080)]
    [TestCase(2160, 1080)]
    [TestCase(2400, 1080)]
    [TestCase(1280, 800)]
    public void SafeArea_AncoraOgniBordoDentroIlNotch(int larghezza, int altezza)
    {
        Rect safe = new Rect(80, 24, larghezza - 140, altezza - 24);
        Rect normalizzata = MobileSafeAreaCanvas.Normalizza(safe, larghezza, altezza);
        Vector2 min = MobileSafeAreaCanvas.MappaAncora(Vector2.zero, normalizzata);
        Vector2 max = MobileSafeAreaCanvas.MappaAncora(Vector2.one, normalizzata);
        Assert.That(min.x * larghezza, Is.EqualTo(safe.xMin).Within(.01));
        Assert.That(min.y * altezza, Is.EqualTo(safe.yMin).Within(.01));
        Assert.That(max.x * larghezza, Is.EqualTo(safe.xMax).Within(.01));
        Assert.That(max.y * altezza, Is.EqualTo(safe.yMax).Within(.01));
        Assert.That(MobileSafeAreaCanvas.MappaAncora(Vector2.one * .5f, normalizzata),
            Is.EqualTo(normalizzata.center));
    }

    [Test]
    public void Joystick_DeadzoneEMovimentoDiagonaleNonAumentanoLaVelocita()
    {
        Assert.That(MobileJoystick.Normalizza(new Vector2(1, 1), 100), Is.EqualTo(Vector2.zero));
        Assert.That(MobileJoystick.Normalizza(new Vector2(500, 500), 100).magnitude,
            Is.EqualTo(1).Within(.001));
        Assert.That(MobileJoystick.Normalizza(new Vector2(50, 0), 100).x, Is.EqualTo(.5f));
    }

    [Test]
    public void Mira_IgnoraPuntiFuoriSchermoODietroLaCamera()
    {
        var oggetto = new GameObject("CameraTestMobile", typeof(Camera));
        try
        {
            var camera = oggetto.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.transform.position = new Vector3(0, 0, -10);
            Assert.That(MobileAutoAim.Visibile(camera, Vector3.zero), Is.True);
            Assert.That(MobileAutoAim.Visibile(camera, new Vector3(100, 0, 0)), Is.False);
            Assert.That(MobileAutoAim.Visibile(camera, new Vector3(0, 0, -20)), Is.False);
            Assert.That(new MobileAutoAim().ProvaDirezione(null, Vector3.zero, out _), Is.False);
        }
        finally { Object.DestroyImmediate(oggetto); }
    }

    [Test]
    public void CartaTouch_DisabilitataNonAcquistaEAbilitataAcquistaUnaVolta()
    {
        bool anteprima = MobileExperience.Attiva;
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, true);
        var oggetto = new GameObject("CartaTest", typeof(RectTransform), typeof(Button), typeof(MobileCardPurchase));
        try
        {
            Button pulsante = oggetto.GetComponent<Button>();
            MobileCardPurchase carta = oggetto.GetComponent<MobileCardPurchase>();
            carta.Pulsante = pulsante;
            int acquisti = 0;
            pulsante.onClick.AddListener(() => acquisti++);
            var evento = new PointerEventData(null);
            pulsante.interactable = false;
            carta.OnPointerClick(evento);
            Assert.That(acquisti, Is.Zero);
            pulsante.interactable = true;
            carta.OnPointerClick(evento);
            Assert.That(acquisti, Is.EqualTo(1));
        }
        finally
        {
            Object.DestroyImmediate(oggetto);
            SessionState.SetBool(MobileExperience.ChiaveAnteprima, anteprima);
        }
    }

    [Test]
    public void Aptica_PreferenzaSalvataSeparatamenteDallaCamera()
    {
        string root = Path.Combine(Path.GetTempPath(), "AngryFarmerMobileOptions_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using (SaveService.IsolaDatiPerTest(root))
            {
                Assert.That(SaveService.Dispositivo.apticaAttiva, Is.True);
                SaveService.ModificaDispositivo(d => { d.apticaAttiva = false; d.vibrazioneAttiva = true; }, true);
                SaveService.RicaricaDatiPerTest();
                Assert.That(SaveService.Dispositivo.apticaAttiva, Is.False);
                Assert.That(SaveService.Dispositivo.vibrazioneAttiva, Is.True);
            }
        }
        finally { Directory.Delete(root, true); }
    }

    [UnityTest]
    public IEnumerator FlussoTouch_MenuShopMovimentoSchivataEPausa()
    {
        string root = Path.Combine(Path.GetTempPath(), "AngryFarmerMobileFlow_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        SaveService.PreparaRadicePlayModePerTest(root);
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, true);
        yield return new EnterPlayMode();
        yield return EseguiFlussoTouch();
        yield return new ExitPlayMode();
    }

    private static IEnumerator EseguiFlussoTouch()
    {
        SceneManager.LoadScene(MenuInizialeController.NomeScenaMenu);
        yield return Attendi(() => Object.FindFirstObjectByType<MenuInizialeController>() != null);
        yield return null;
        // Il batch non ha una finestra Game View in primo piano: simuliamo
        // esplicitamente l'app attiva prima di inviare tocchi.
        MobileAppLifecycle.Instance.NotificaPausa(false);
        MobileAppLifecycle.Instance.NotificaFocus(true);
        MobileVisualCapture.Salva("menu-16-9");
        yield return null;
        yield return Tocca("ShopPermanente");
        yield return null;
        Assert.That(ShopPermanentePrePartita.ApertoGlobale, Is.True);
        MobileVisualCapture.Salva("permanenti-20-9", 2400, 1080);
        yield return null;
        yield return Tocca("Chiudi");
        yield return null;
        yield return Tocca("Gioca");
        yield return null;
        yield return Tocca("Difficolta_Normale");
        yield return Attendi(() => SceneManager.GetActiveScene().name == MenuInizialeController.NomeScenaGameplay &&
            GameManager.instance != null &&
            Object.FindFirstObjectByType<ShopInterOndata>(FindObjectsInactive.Include) != null);
        yield return null;
        var shop = Object.FindFirstObjectByType<ShopInterOndata>(FindObjectsInactive.Include);
        yield return Attendi(() => shop.PreparazioneInizialeAttiva);
        Assert.That(MobileExperience.GameplayTouchAttivo, Is.False);
        MobileVisualCapture.Salva("shop-16-9");
        yield return null;
        int scelteIniziali = shop.ScelteGratuiteRimaste;
        for (int i = 0; i < scelteIniziali; i++)
        {
            yield return Attendi(() => Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .Any(b => b.name == "Acquista" && b.IsInteractable()));
            yield return Tocca("Acquista");
            yield return null;
            yield return null;
        }
        Assert.That(shop.ScelteGratuiteRimaste, Is.Zero);
        yield return Tocca("OndataSuccessiva");
        yield return Attendi(() => MobileExperience.GameplayTouchAttivo);
        yield return null;
        var giocatore = Object.FindFirstObjectByType<PlayerShooting>();
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Volpe.prefab");
        var vicina = Object.Instantiate(prefab, giocatore.transform.position + Vector3.up * 2, Quaternion.identity).GetComponent<EnemyAI>();
        var lontana = Object.Instantiate(prefab, giocatore.transform.position + Vector3.right * 4, Quaternion.identity).GetComponent<EnemyAI>();
        var mira = new MobileAutoAim();
        Assert.That(mira.ProvaDirezione(Camera.main, giocatore.transform.position, out Vector2 direzione), Is.True);
        Assert.That(Vector2.Dot(direzione, Vector2.up), Is.GreaterThan(.99f));
        vicina.SubisciDanno(int.MaxValue);
        Assert.That(MobileAutoAim.BersaglioValido(vicina, Camera.main), Is.False);
        Assert.That(mira.ProvaDirezione(Camera.main, giocatore.transform.position, out direzione), Is.True);
        Assert.That(Vector2.Dot(direzione, Vector2.right), Is.GreaterThan(.99f));
        yield return Attendi(() => giocatore.UltimoNumeroProiettiliCreati > 0);
        var input = FarmerInputController.Instance;
        MobileVisualCapture.Salva("gameplay-20-9", 2400, 1080);
        var stick = Object.FindFirstObjectByType<MobileJoystick>();
        Assert.That(stick, Is.Not.Null);
        Vector2 centro = RectTransformUtility.WorldToScreenPoint(null, stick.transform.position);
        var primo = new PointerEventData(EventSystem.current) { pointerId = 11, position = centro + Vector2.right * 100 };
        var secondo = new PointerEventData(EventSystem.current) { pointerId = 12, position = centro + Vector2.up * 100 };
        stick.OnPointerDown(primo);
        Assert.That(input.Movimento.x, Is.GreaterThan(.1));
        stick.OnPointerDown(secondo);
        stick.OnPointerUp(secondo);
        Assert.That(stick.PuntatoreAttivo, Is.EqualTo(11));
        input.RichiediSchivataTouch();
        stick.OnPointerUp(primo);
        Assert.That(input.ConsumaSchivata(), Is.True);
        Assert.That(input.ConsumaSchivata(), Is.False);
        stick.OnPointerDown(primo);
        yield return Tocca("SchivataTouch");
        yield return null;
        yield return null;
        Assert.That(Object.FindFirstObjectByType<PlayerMovement>().SchivataPronta, Is.False);
        yield return Tocca("ApriPausaOpzioni");
        yield return Attendi(() => !stick.gameObject.activeInHierarchy);
        Assert.That(input.Movimento, Is.EqualTo(Vector2.zero));
        Assert.That(stick.PuntatoreAttivo, Is.Null);
        MobileVisualCapture.Salva("pausa-18-9", 2160, 1080);
        Assert.That(stick.gameObject.activeInHierarchy, Is.False);
        input.RichiediSchivataTouch();
        Assert.That(input.ConsumaSchivata(), Is.False);
        var aptica = Object.FindObjectsByType<Toggle>(FindObjectsSortMode.None).Single(t => t.name == "Aptica");
        aptica.isOn = false;
        Assert.That(GameOptionsController.Instance.ApticaAttiva, Is.False);
        int fpsPc = SaveService.Dispositivo.limiteFps;
        yield return Tocca("FpsMobile");
        Assert.That(GameOptionsController.Instance.FpsMobile, Is.EqualTo(30));
        Assert.That(Application.targetFrameRate, Is.EqualTo(30));
        yield return Tocca("QualitaMobile");
        Assert.That(MobilePerformance.ModalitaLeggera, Is.True);
        Assert.That(SaveService.Dispositivo.limiteFps, Is.EqualTo(fpsPc));
        DamageNumberFeedback.Mostra(Vector2.zero, 1);
        int saltatiPrima = DamageNumberFeedback.Instance.NumeriSaltati;
        for (int i = 0; i < 30; i++) DamageNumberFeedback.Mostra(Vector2.zero, 1);
        Assert.That(DamageNumberFeedback.Instance.NumeriSaltati, Is.GreaterThan(saltatiPrima));
        int mostratiPrima = DamageNumberFeedback.Instance.NumeriMostrati;
        DamageNumberFeedback.MostraGiocatore(Vector2.zero, 1);
        Assert.That(DamageNumberFeedback.Instance.NumeriMostrati, Is.EqualTo(mostratiPrima + 1));
        yield return Tocca("Chiudi");
        yield return Attendi(() => stick.gameObject.activeInHierarchy);
        Assert.That(input.Movimento, Is.EqualTo(Vector2.zero));
        Assert.That(Object.FindObjectsByType<MobileControls>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        var lifecycle = MobileAppLifecycle.Instance;
        Assert.That(lifecycle, Is.Not.Null);
        stick.OnPointerDown(primo);
        input.RichiediSchivataTouch();
        lifecycle.NotificaFocus(false);
        lifecycle.NotificaPausa(true);
        Assert.That(lifecycle.Sospesa, Is.True);
        Assert.That(lifecycle.UltimoSalvataggioRiuscito, Is.True);
        Assert.That(AudioListener.pause, Is.True);
        Assert.That(Time.timeScale, Is.Zero);
        Assert.That(input.Movimento, Is.EqualTo(Vector2.zero));
        Assert.That(input.ConsumaSchivata(), Is.False);
        DeviceSettingsData salvato = JsonUtility.FromJson<DeviceSettingsData>(
            File.ReadAllText(SaveService.PercorsoImpostazioniDispositivo));
        Assert.That(salvato.fpsMobile, Is.EqualTo(30));
        Assert.That(salvato.qualitaMobileRidotta, Is.True);
        lifecycle.NotificaPausa(false);
        Assert.That(lifecycle.Sospesa, Is.True, "Non riprendere finche' manca il focus.");
        lifecycle.NotificaFocus(true);
        yield return null;
        Assert.That(lifecycle.Sospesa, Is.False);
        Assert.That(AudioListener.pause, Is.False);
        Assert.That(Time.timeScale, Is.Zero, "La ripresa deve richiedere una conferma.");
        Assert.That(MobileExperience.GameplayTouchAttivo, Is.False);
        MobileVisualCapture.Salva("ripresa-18-9", 2160, 1080);
        yield return Tocca("Chiudi");
        yield return null;
        Assert.That(MobileExperience.GameplayTouchAttivo, Is.True);
        Assert.That(Time.timeScale, Is.EqualTo(1));
        Assert.That(input.Movimento, Is.EqualTo(Vector2.zero));
        GameManager.instance.GameOverGiocatore();
        yield return null;
        Assert.That(MobileExperience.GameplayTouchAttivo, Is.False);
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

    private static IEnumerator Tocca(string nome)
    {
        Canvas.ForceUpdateCanvases();
        Button pulsante = Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
            .FirstOrDefault(b => b.name == nome && b.isActiveAndEnabled && b.IsInteractable());
        Assert.That(pulsante, Is.Not.Null, "Pulsante touch mancante: " + nome);
        var rect = (RectTransform)pulsante.transform;
        var evento = new PointerEventData(EventSystem.current)
        {
            position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
        };
        var risultati = new System.Collections.Generic.List<RaycastResult>();
        float limite = Time.realtimeSinceStartup + 2f;
        // I test batch possono avanzare prima del render: attendiamo la UI reale,
        // anche a 30 FPS, senza bypassare il raycast o invocare direttamente onClick.
        do
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            evento.position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            risultati.Clear();
            EventSystem.current.RaycastAll(evento, risultati);
            if (risultati.Count > 0 && risultati[0].gameObject.GetComponentInParent<Button>() == pulsante) break;
        } while (Time.realtimeSinceStartup < limite);
        Assert.That(risultati.Count, Is.GreaterThan(0), "Bottone non raggiungibile dal touch: " + nome);
        Assert.That(risultati[0].gameObject.GetComponentInParent<Button>(), Is.SameAs(pulsante),
            "Un altro elemento copre il bottone: " + nome + " -> " + risultati[0].gameObject.name);
        ExecuteEvents.Execute(pulsante.gameObject, evento, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(pulsante.gameObject, evento, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(pulsante.gameObject, evento, ExecuteEvents.pointerClickHandler);
    }

    private static IEnumerator Attendi(Func<bool> condizione,
        [System.Runtime.CompilerServices.CallerLineNumber] int linea = 0)
    {
        float limite = Time.realtimeSinceStartup + 12f;
        while (!condizione() && Time.realtimeSinceStartup < limite) yield return null;
        Assert.That(condizione(), Is.True, "Timeout nel flusso mobile, linea " + linea +
            "; sospesa=" + MobileAppLifecycle.AppSospesa + "; stato=" + GameManager.instance?.StatoCorrente +
            "; pausa=" + GameManager.instance?.PausaManualeAttiva);
    }
}
