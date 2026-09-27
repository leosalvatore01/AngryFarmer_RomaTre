using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public sealed class ReleaseReadinessTests
{
    [Test]
    public void BuildSettings_ApronoIlMenuPrimaDelGameplay()
    {
        EditorBuildSettingsScene[] scene = EditorBuildSettings.scenes;
        Assert.That(scene, Has.Length.GreaterThanOrEqualTo(2));
        Assert.That(scene[0].enabled, Is.True);
        Assert.That(scene[0].path, Is.EqualTo("Assets/Scenes/MenuIniziale.unity"));
        Assert.That(scene[1].enabled, Is.True);
        Assert.That(scene[1].path, Is.EqualTo("Assets/Scenes/SampleScene.unity"));
    }

    [Test]
    public void Branding_HaIconaQuadrataAdAltaRisoluzione()
    {
        Texture2D icona = AssetDatabase.LoadAssetAtPath<Texture2D>(
            ReleaseBuildTools.PercorsoIcona
        );
        Assert.That(icona, Is.Not.Null);
        Assert.That(icona.width, Is.EqualTo(icona.height));
        Assert.That(icona.width, Is.GreaterThanOrEqualTo(512));
    }

    [Test]
    public void Metadata_IdentificanoLaReleaseCandidate()
    {
        Assert.That(PlayerSettings.productName, Is.EqualTo("Angry Farmer"));
        Assert.That(
            PlayerSettings.bundleVersion,
            Is.EqualTo(ReleaseBuildTools.VersioneRelease)
        );
        Assert.That(PlayerSettings.resizableWindow, Is.True);
        Assert.That(
            PlayerSettings.GetApplicationIdentifier(
                NamedBuildTarget.Standalone
            ),
            Is.EqualTo("com.leosalvatore01.angryfarmer")
        );
    }
}
