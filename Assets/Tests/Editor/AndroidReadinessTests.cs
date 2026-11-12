using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class AndroidReadinessTests
{
    [TestCase(0, 60)]
    [TestCase(-1, 60)]
    [TestCase(30, 30)]
    [TestCase(60, 60)]
    [TestCase(120, 60)]
    public void FpsMobile_ValoriSupportati(int valore, int atteso) =>
        Assert.That(MobilePerformance.NormalizzaFps(valore), Is.EqualTo(atteso));

    [TestCase(16, 32, TextureImporterFormat.RGBA32)]
    [TestCase(128, 128, TextureImporterFormat.RGBA32)]
    [TestCase(256, 64, TextureImporterFormat.ETC2_RGBA8)]
    [TestCase(128, 1024, TextureImporterFormat.ETC2_RGBA8)]
    public void Compressione_PreservaSpritePiccoliEAlpha(int w, int h, TextureImporterFormat atteso) =>
        Assert.That(AndroidBuildTools.FormatoSprite(w, h), Is.EqualTo(atteso));

    [Test]
    public void PreferenzeMobile_PersistonoSenzaSovrascrivereQuellePc()
    {
        string root = Path.Combine(Path.GetTempPath(), "AngryFarmerAndroid_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using (SaveService.IsolaDatiPerTest(root))
            {
                SaveService.ModificaDispositivo(d =>
                {
                    d.fpsMobile = 30;
                    d.qualitaMobileRidotta = true;
                    d.limiteFps = 144;
                    d.vSyncAttivo = true;
                }, true);
                SaveService.RicaricaDatiPerTest();
                Assert.That(SaveService.Dispositivo.fpsMobile, Is.EqualTo(30));
                Assert.That(SaveService.Dispositivo.qualitaMobileRidotta, Is.True);
                Assert.That(SaveService.Dispositivo.limiteFps, Is.EqualTo(144));
                Assert.That(SaveService.Dispositivo.vSyncAttivo, Is.True);
            }
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void MigrazioneV4_AggiungeDefaultMobileEPreservaApticaEProgressi()
    {
        string root = Path.Combine(Path.GetTempPath(), "AngryFarmerAndroidMigration_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using (SaveService.IsolaDatiPerTest(root))
            {
                SaveService.ModificaProfilo(p => p.progressionePermanente.saldoGettoni = 123, true);
                SaveService.ModificaDispositivo(d => d.apticaAttiva = false, true);
                DeviceSettingsData vecchio = JsonUtility.FromJson<DeviceSettingsData>(File.ReadAllText(SaveService.PercorsoImpostazioniDispositivo));
                vecchio.versioneSchema = 4;
                vecchio.fpsMobile = 0;
                File.WriteAllText(SaveService.PercorsoImpostazioniDispositivo, JsonUtility.ToJson(vecchio));
                SaveService.RicaricaDatiPerTest();
                Assert.That(SaveService.Dispositivo.versioneSchema, Is.EqualTo(5));
                Assert.That(SaveService.Dispositivo.fpsMobile, Is.EqualTo(60));
                Assert.That(SaveService.Dispositivo.qualitaMobileRidotta, Is.False);
                Assert.That(SaveService.Dispositivo.apticaAttiva, Is.False);
                Assert.That(SaveService.Profilo.progressionePermanente.saldoGettoni, Is.EqualTo(123));
            }
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void Preflight_SegnalaModuloAssenteSenzaTentareUnaBuild()
    {
        if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            Assert.Ignore("Il modulo Android e' installato su questa macchina.");
        Assert.That(AndroidBuildTools.ControllaRequisiti(false, false),
            Has.Some.Contains("Android Build Support"));
    }
}
