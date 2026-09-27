using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class ReleaseBuildTools
{
    public const string VersioneRelease = "1.0.0-rc";
    public const string PercorsoEseguibile =
        "Builds/Windows/AngryFarmer.exe";
    public const string PercorsoIcona =
        "Assets/Branding/AngryFarmerIcon.png";

    [MenuItem("Tools/Angry Farmer/Release/Configura metadata")]
    public static void ConfiguraMetadata()
    {
        Texture2D icona = AssetDatabase.LoadAssetAtPath<Texture2D>(
            PercorsoIcona
        );
        if (icona == null)
        {
            throw new InvalidOperationException(
                "Icona di release non trovata: " + PercorsoIcona
            );
        }

        PlayerSettings.companyName = "leosalvatore01";
        PlayerSettings.productName = "Angry Farmer";
        PlayerSettings.bundleVersion = VersioneRelease;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.SetApplicationIdentifier(
            NamedBuildTarget.Standalone,
            "com.leosalvatore01.angryfarmer"
        );
        int[] dimensioniIcona = PlayerSettings.GetIconSizes(
            NamedBuildTarget.Standalone,
            IconKind.Application
        );
        Texture2D[] icone = new Texture2D[dimensioniIcona.Length];
        Array.Fill(icone, icona);
        PlayerSettings.SetIcons(
            NamedBuildTarget.Standalone,
            icone,
            IconKind.Application
        );

        AssetDatabase.SaveAssets();
        Debug.Log("[RELEASE] Metadata e icona configurati.");
    }

    [MenuItem("Tools/Angry Farmer/Release/Build Windows RC")]
    public static void BuildWindowsRc()
    {
        BuildWindowsRcInterna(false);
    }

    public static void BuildWindowsRcCommandLine()
    {
        BuildWindowsRcInterna(true);
    }

    private static void BuildWindowsRcInterna(bool daRigaDiComando)
    {
        ConfiguraMetadata();

        EditorBuildSettingsScene[] sceneAbilitate = Array.FindAll(
            EditorBuildSettings.scenes,
            scena => scena.enabled
        );
        string[] scene = Array.ConvertAll(
            sceneAbilitate,
            scena => scena.path
        );
        if (scene.Length == 0)
        {
            throw new InvalidOperationException(
                "Nessuna scena abilitata nelle Build Settings."
            );
        }

        string percorsoAssoluto = Path.GetFullPath(PercorsoEseguibile);
        Directory.CreateDirectory(Path.GetDirectoryName(percorsoAssoluto));

        BuildPlayerOptions opzioni = new BuildPlayerOptions
        {
            scenes = scene,
            locationPathName = percorsoAssoluto,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.StrictMode
        };
        BuildReport rapporto = BuildPipeline.BuildPlayer(opzioni);
        if (rapporto.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Build Windows fallita: " + rapporto.summary.result +
                " (" + rapporto.summary.totalErrors + " errori)."
            );
        }

        Debug.Log(
            "[RELEASE] Build Windows pronta: " + percorsoAssoluto +
            " | " + rapporto.summary.totalSize + " byte"
        );

        if (daRigaDiComando)
        {
            EditorApplication.Exit(0);
        }
    }
}
