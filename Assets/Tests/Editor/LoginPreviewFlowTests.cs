using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class LoginPreviewFlowTests
{
    private sealed class ProviderNonDisponibile : ILoginProvider
    {
        public int Chiamate;
        public bool Disponibile => false;
        public IEnumerator Esegui(OperazioneAccesso op, string email, string password, Action<EsitoAccesso> fine)
        {
            Chiamate++;
            fine(new EsitoAccesso());
            yield break;
        }
    }

    [UnityTest]
    public IEnumerator AnteprimaAccesso_NonContattaProviderENonCambiaProgressi()
    {
        string root = Path.Combine(Path.GetTempPath(), "AngryFarmerLogin_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        SaveService.PreparaRadicePlayModePerTest(root);
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, false);
        yield return new EnterPlayMode();
        yield return EseguiFlusso();
        yield return new ExitPlayMode();
    }

    private static IEnumerator EseguiFlusso()
    {
        SceneManager.LoadScene(MenuInizialeController.NomeScenaMenu);
        float limite = Time.realtimeSinceStartup + 12;
        while (OnlineLoginController.Instance == null && Time.realtimeSinceStartup < limite) yield return null;
        var login = OnlineLoginController.Instance;
        Assert.That(login != null && login.Aperto, Is.True);
        var provider = new ProviderNonDisponibile();
        login.ConfiguraProvider(provider, _ => throw new Exception("La selezione account non deve avvenire nell'anteprima."));
        SaveService.ModificaProfilo(d => d.progressionePermanente.saldoGettoni = 37);
        yield return null;
        MobileVisualCapture.Salva("login-pc-16-9");
        Input("EmailAccesso").text = "non-una-email";
        Input("PasswordAccesso").text = "Patata!123";
        yield return Tocca("InviaAccesso");
        Assert.That(Stato().ToLowerInvariant(), Does.Contain("email valido"));
        Input("EmailAccesso").text = "contadino@example.test";
        yield return Tocca("MostraPassword");
        Assert.That(Input("PasswordAccesso").contentType, Is.EqualTo(TMP_InputField.ContentType.Standard));
        yield return Tocca("InviaAccesso");
        Assert.That(login.Aperto, Is.True);
        Assert.That(Stato(), Does.Contain("non attivo"));
        Assert.That(Input("PasswordAccesso").text, Is.Empty);
        Assert.That(provider.Chiamate, Is.Zero);
        yield return Tocca("TabRegistrati");
        Input("PasswordAccesso").text = "Patata!123";
        Input("ConfermaPasswordAccesso").text = "Diversa!123";
        yield return Tocca("InviaAccesso");
        Assert.That(Stato(), Does.Contain("non coincidono"));
        Input("ConfermaPasswordAccesso").text = "Patata!123";
        yield return Tocca("InviaAccesso");
        Assert.That(provider.Chiamate, Is.Zero);
        MobileVisualCapture.Salva("registrazione-pc-16-9");
        yield return Tocca("RecuperaPassword");
        yield return Tocca("RecuperaPassword");
        yield return Tocca("InviaAccesso");
        Assert.That(Stato(), Does.Contain("non attivo"));
        Assert.That(provider.Chiamate, Is.Zero);
        MobileVisualCapture.Salva("recupero-password-pc-16-9");
        yield return Tocca("TabAccedi");
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, true);
        yield return null;
        MobileVisualCapture.Salva("login-mobile-20-9", 2400, 1080);
        MobileVisualCapture.Salva("login-tablet-4-3", 1440, 1080);
        SessionState.SetBool(MobileExperience.ChiaveAnteprima, false);
        yield return Tocca("ContinuaOspite");
        yield return null;
        Assert.That(login.Aperto, Is.False);
        Assert.That(LoginSession.AccessoLocaleScelto, Is.True);
        Assert.That(SaveService.Profilo.idProfilo, Is.EqualTo(SaveService.IdProfiloOspite));
        Assert.That(SaveService.Profilo.progressionePermanente.saldoGettoni, Is.EqualTo(37));
        foreach (string file in Directory.GetFiles(SaveService.RadicePlayModePerTestAttiva, "*.json", SearchOption.AllDirectories))
            Assert.That(File.ReadAllText(file), Does.Not.Contain("Patata!123"));
        yield return Tocca("CambiaAccount");
        yield return null;
        Assert.That(login.Aperto, Is.True);
        Assert.That(Input("EmailAccesso").text, Is.Empty);
        Assert.That(Input("PasswordAccesso").contentType, Is.EqualTo(TMP_InputField.ContentType.Password));
        yield return Tocca("ContinuaOspite");
        yield return null;
        yield return Tocca("Gioca");
        Assert.That(MenuInizialeController.Instance.SelettoreDifficoltaAperto, Is.True);
    }

    private static TMP_InputField Input(string nome) => GameObject.Find(nome).GetComponent<TMP_InputField>();
    private static string Stato() => GameObject.Find("MessaggioAccesso").GetComponent<TMP_Text>().text;

    private static IEnumerator Tocca(string nome)
    {
        Button pulsante = Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
            .First(b => b.name == nome && b.isActiveAndEnabled && b.IsInteractable());
        var rect = (RectTransform)pulsante.transform;
        var evento = new PointerEventData(EventSystem.current);
        var risultati = new System.Collections.Generic.List<RaycastResult>();
        float limite = Time.realtimeSinceStartup + 2f;
        do
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            evento.position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            risultati.Clear();
            EventSystem.current.RaycastAll(evento, risultati);
            if (risultati.Count > 0 && risultati[0].gameObject.GetComponentInParent<Button>() == pulsante) break;
        } while (Time.realtimeSinceStartup < limite);
        Assert.That(risultati.Count > 0 && risultati[0].gameObject.GetComponentInParent<Button>() == pulsante,
            Is.True, "Pulsante non raggiungibile: " + nome);
        ExecuteEvents.Execute(pulsante.gameObject, evento, ExecuteEvents.pointerClickHandler);
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
