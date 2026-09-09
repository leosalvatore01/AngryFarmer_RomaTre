using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public sealed class PcExperienceTests
{
    private string cartellaTemporanea;
    private IDisposable ambienteDati;
    private GameObject oggettoInput;

    [SetUp]
    public void SetUp()
    {
        if (FarmerInputController.Instance != null)
        {
            UnityEngine.Object.DestroyImmediate(
                FarmerInputController.Instance.gameObject
            );
        }

        cartellaTemporanea = Path.Combine(
            Path.GetTempPath(),
            "AngryFarmerPcExperience_" + Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(cartellaTemporanea);
        ambienteDati = SaveService.IsolaDatiPerTest(cartellaTemporanea);
    }

    [TearDown]
    public void TearDown()
    {
        if (oggettoInput != null)
        {
            UnityEngine.Object.DestroyImmediate(oggettoInput);
        }
        if (FarmerInputController.Instance != null)
        {
            UnityEngine.Object.DestroyImmediate(
                FarmerInputController.Instance.gameObject
            );
        }

        ambienteDati?.Dispose();
        if (Directory.Exists(cartellaTemporanea))
        {
            Directory.Delete(cartellaTemporanea, true);
        }
    }

    [Test]
    public void RimappaturaTastiera_VieneSalvataERicaricata()
    {
        FarmerInputController input = CreaInput();
        Assert.That(
            input.ProvaImpostaBinding(
                ComandoRimappabile.Schivata,
                "<Keyboard>/q"
            ),
            Is.True
        );
        Assert.That(
            input.OttieniPercorsoBindingEffettivo(
                ComandoRimappabile.Schivata
            ),
            Is.EqualTo("<Keyboard>/q").IgnoreCase
        );

        UnityEngine.Object.DestroyImmediate(oggettoInput);
        oggettoInput = null;
        FarmerInputController ricaricato = CreaInput();

        Assert.That(
            ricaricato.OttieniPercorsoBindingEffettivo(
                ComandoRimappabile.Schivata
            ),
            Is.EqualTo("<Keyboard>/q").IgnoreCase
        );
        Assert.That(
            SaveService.Dispositivo.overrideBindingGameplayJson,
            Is.Not.Empty
        );
    }

    [Test]
    public void OpzioniVideo_LocaliVengonoNormalizzateEPersistite()
    {
        Assert.That(
            SaveService.ModificaDispositivo(dati =>
            {
                dati.larghezzaRisoluzione = 9000;
                dati.altezzaRisoluzione = 100;
                dati.modalitaSchermo = 999;
                dati.limiteFps = 141;
                dati.vSyncAttivo = false;
            }, true),
            Is.True
        );
        SaveService.RicaricaDatiPerTest();

        DeviceSettingsData dati = SaveService.Dispositivo;
        Assert.That(dati.larghezzaRisoluzione, Is.EqualTo(7680));
        Assert.That(dati.altezzaRisoluzione, Is.EqualTo(360));
        Assert.That(
            dati.modalitaSchermo,
            Is.EqualTo((int)FullScreenMode.FullScreenWindow)
        );
        Assert.That(dati.limiteFps, Is.EqualTo(144));
        Assert.That(dati.vSyncAttivo, Is.False);
    }

    [Test]
    public void PerditaFocus_MetteInPausaSoloDuranteIlGameplay()
    {
        Assert.That(
            GameManager.DevePausarePerPerditaFocus(false, true),
            Is.True
        );
        Assert.That(
            GameManager.DevePausarePerPerditaFocus(true, true),
            Is.False
        );
        Assert.That(
            GameManager.DevePausarePerPerditaFocus(false, false),
            Is.False
        );
    }

    [Test]
    public void Tutorial_AppareSoloAllaPrimaOndataEffettiva()
    {
        Assert.That(
            PcOnboardingController.DeveMostrare(false, true, true),
            Is.True
        );
        Assert.That(
            PcOnboardingController.DeveMostrare(true, true, true),
            Is.False
        );
        Assert.That(
            PcOnboardingController.DeveMostrare(false, false, true),
            Is.False
        );
        Assert.That(
            PcOnboardingController.DeveMostrare(false, true, false),
            Is.False
        );
    }

    private FarmerInputController CreaInput()
    {
        oggettoInput = new GameObject("InputPcExperience_Test");
        return oggettoInput.AddComponent<FarmerInputController>();
    }
}
