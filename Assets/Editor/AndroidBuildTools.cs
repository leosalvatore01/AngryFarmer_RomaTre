using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Preparazione ripetibile, senza installare SDK o conservare credenziali.</summary>
public static class AndroidBuildTools
{
    public const string PackageId = "com.leosalvatore01.angryfarmer";
    public const string PercorsoApk = "Builds/Android/AngryFarmer-test.apk";
    public const string PercorsoAab = "Builds/Android/AngryFarmer.aab";
    private static readonly string[] SceneRichieste =
    {
        "Assets/Scenes/MenuIniziale.unity", "Assets/Scenes/SampleScene.unity"
    };

    [MenuItem("Tools/Angry Farmer/Android/1 - Configura progetto")]
    public static void ConfiguraProgetto()
    {
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.bundleVersionCode = Math.Max(1, PlayerSettings.Android.bundleVersionCode);
        PlayerSettings.Android.optimizedFramePacing = true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        Texture2D icona = AssetDatabase.LoadAssetAtPath<Texture2D>(ReleaseBuildTools.PercorsoIcona);
        int[] dimensioni = PlayerSettings.GetIconSizes(NamedBuildTarget.Android, IconKind.Application);
        if (icona != null && dimensioni.Length > 0)
            PlayerSettings.SetIcons(NamedBuildTarget.Android,
                Enumerable.Repeat(icona, dimensioni.Length).ToArray(), IconKind.Application);

        int texture = ConfiguraTexture();
        AssetDatabase.SaveAssets();
        Debug.Log("[ANDROID] Configurazione salvata: ARM64, IL2CPP, Android 8+, OpenGL ES 3, " +
            texture + " importazioni aggiornate. Non e' stata generata una build.");
    }

    // Manteniamo senza perdita gli sprite minuscoli; ETC2 conserva l'alpha degli atlanti.
    public static TextureImporterFormat FormatoSprite(int larghezza, int altezza) =>
        Math.Max(larghezza, altezza) >= 256 ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.RGBA32;

    private static int ConfiguraTexture()
    {
        int modificati = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Art", "Assets/Resources" }))
        {
            string percorso = AssetDatabase.GUIDToAssetPath(guid);
            if (!(AssetImporter.GetAtPath(percorso) is TextureImporter importer) ||
                importer.textureType != TextureImporterType.Sprite) continue;
            importer.GetSourceTextureWidthAndHeight(out int w, out int h);
            TextureImporterFormat formato = FormatoSprite(w, h);
            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            if (android.overridden && android.format == formato && !android.crunchedCompression &&
                android.compressionQuality == 100) continue;
            android.name = "Android";
            android.overridden = true;
            android.format = formato;
            android.compressionQuality = 100;
            android.crunchedCompression = false;
            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport();
            modificati++;
        }
        return modificati;
    }

    [MenuItem("Tools/Angry Farmer/Android/2 - Verifica requisiti")]
    public static void VerificaRequisiti()
    {
        List<string> problemi = ControllaRequisiti(false, false);
        string testo = problemi.Count == 0
            ? "[ANDROID] Requisiti APK presenti. Selezionare Android nelle Build Profiles prima della build."
            : "[ANDROID] Preparazione disponibile, build non ancora pronta:\n- " + string.Join("\n- ", problemi);
        Debug.Log(testo);
        if (!Application.isBatchMode) EditorUtility.DisplayDialog("Preparazione Android", testo, "OK");
    }

    public static List<string> ControllaRequisiti(bool bundle, bool richiedeTargetAndroid)
    {
        var problemi = new List<string>();
        bool modulo = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android);
        if (!modulo)
            problemi.Add("In Unity Hub aggiungere Android Build Support, Android SDK & NDK Tools e OpenJDK a Unity " + Application.unityVersion + ".");
        else
        {
            string player = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer");
            string sdk = PercorsoStrumento("sdkRootPath", Path.Combine(player, "SDK"));
            string ndk = PercorsoStrumento("ndkRootPath", Path.Combine(player, "NDK"));
            string jdk = PercorsoStrumento("jdkRootPath", Path.Combine(player, "OpenJDK"));
            if (!Directory.Exists(Path.Combine(sdk, "platform-tools")) ||
                !HaSottocartelle(Path.Combine(sdk, "platforms")) || !HaSottocartelle(Path.Combine(sdk, "build-tools")))
                problemi.Add("SDK Android incompleto: verificare SDK, piattaforme e build-tools in Preferences > External Tools.");
            if (!File.Exists(Path.Combine(ndk, "source.properties")))
                problemi.Add("NDK Android mancante: verificare Preferences > External Tools.");
            string java = Application.platform == RuntimePlatform.WindowsEditor ? "java.exe" : "java";
            if (!File.Exists(Path.Combine(jdk, "bin", java)))
                problemi.Add("OpenJDK mancante: verificare Preferences > External Tools.");
        }
        if (richiedeTargetAndroid && EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            problemi.Add("Aprire File > Build Profiles, selezionare Android e Switch Platform. Da terminale usare -buildTarget Android.");
        string[] scene = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (!scene.SequenceEqual(SceneRichieste) || scene.Any(s => !File.Exists(s)))
            problemi.Add("Le scene abilitate devono essere MenuIniziale e SampleScene, in questo ordine.");
        if (bundle && (!PlayerSettings.Android.useCustomKeystore ||
            !File.Exists(PlayerSettings.Android.keystoreName) ||
            string.IsNullOrWhiteSpace(PlayerSettings.Android.keyaliasName) ||
            string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) ||
            string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass)))
            problemi.Add("Per AAB distribuzione configurare keystore, alias e password in Player Settings > Publishing Settings. Non inserirli nel repository.");
        return problemi;
    }

    private static bool HaSottocartelle(string percorso) =>
        Directory.Exists(percorso) && Directory.GetDirectories(percorso).Length > 0;

    private static string PercorsoStrumento(string proprieta, string fallback)
    {
        // L'assembly Android non esiste prima dell'installazione del modulo.
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type tipo = assembly.GetType("UnityEditor.Android.AndroidExternalToolsSettings");
            if (tipo == null) continue;
            string valore = tipo.GetProperty(proprieta, BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string;
            if (!string.IsNullOrEmpty(valore)) return valore;
        }
        return fallback;
    }

    [MenuItem("Tools/Angry Farmer/Android/3 - Build APK di prova")]
    public static void BuildApk() => EseguiBuild(false);

    [MenuItem("Tools/Angry Farmer/Android/4 - Build AAB distribuzione")]
    public static void BuildAab() => EseguiBuild(true);

    private static void EseguiBuild(bool bundle)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Fermare Play prima di creare la build Android.");
        List<string> problemi = ControllaRequisiti(bundle, true);
        if (problemi.Count > 0) throw new InvalidOperationException(string.Join("\n", problemi));
        ConfiguraProgetto();
        bool vecchioBundle = EditorUserBuildSettings.buildAppBundle;
        bool vecchiaEsportazione = EditorUserBuildSettings.exportAsGoogleAndroidProject;
        bool vecchiaFirma = PlayerSettings.Android.useCustomKeystore;
        string percorso = Path.GetFullPath(bundle ? PercorsoAab : PercorsoApk);
        try
        {
            EditorUserBuildSettings.buildAppBundle = bundle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            if (!bundle) PlayerSettings.Android.useCustomKeystore = false;
            Directory.CreateDirectory(Path.GetDirectoryName(percorso));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = SceneRichieste,
                target = BuildTarget.Android,
                locationPathName = percorso,
                options = BuildOptions.StrictMode | BuildOptions.CompressWithLz4HC
            });
            if (report.summary.result != BuildResult.Succeeded || !File.Exists(percorso))
                throw new InvalidOperationException("Build Android fallita: " + report.summary.result + " (" + report.summary.totalErrors + " errori).");
            using (SHA256 hash = SHA256.Create())
            using (FileStream stream = File.OpenRead(percorso))
            {
                var ricevuta = new RicevutaBuild
                {
                    file = Path.GetFileName(percorso), packageId = PackageId,
                    versione = PlayerSettings.bundleVersion, codiceVersione = PlayerSettings.Android.bundleVersionCode,
                    unity = Application.unityVersion, utc = DateTime.UtcNow.ToString("o"),
                    byteTotali = stream.Length,
                    sha256 = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant(),
                    firma = bundle ? "keystore distribuzione" : "debug (solo test)",
                    verificaDispositivo = "Da eseguire su telefono reale"
                };
                File.WriteAllText(percorso + ".json", JsonUtility.ToJson(ricevuta, true));
            }
            Debug.Log("[ANDROID] Build pronta: " + percorso + ". Verifica su dispositivo ancora necessaria.");
        }
        finally
        {
            EditorUserBuildSettings.buildAppBundle = vecchioBundle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = vecchiaEsportazione;
            PlayerSettings.Android.useCustomKeystore = vecchiaFirma;
            AssetDatabase.SaveAssets();
        }
    }

    [Serializable]
    private sealed class RicevutaBuild
    {
        public string file, packageId, versione, unity, utc, sha256, firma, verificaDispositivo;
        public int codiceVersione;
        public long byteTotali;
    }
}
