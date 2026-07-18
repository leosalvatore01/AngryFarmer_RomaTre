using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

public static class PixelFontAssetBuilder
{
    private const string PercorsoSorgente =
        "Assets/Resources/Fonts/PixelifySans-SemiBold.ttf";
    private const string PercorsoDestinazione =
        "Assets/Resources/Fonts/PixelifySans-SemiBold SDF.asset";

    [MenuItem("Tools/Angry Farmer/Rigenera font pixel")]
    public static void Rigenera()
    {
        AssetDatabase.ImportAsset(
            PercorsoSorgente,
            ImportAssetOptions.ForceSynchronousImport
        );

        Font sorgente = AssetDatabase.LoadAssetAtPath<Font>(PercorsoSorgente);
        if (sorgente == null)
        {
            throw new System.InvalidOperationException(
                "Font sorgente non trovato: " + PercorsoSorgente
            );
        }

        AssetDatabase.DeleteAsset(PercorsoDestinazione);
        TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(
            sorgente,
            72,
            9,
            GlyphRenderMode.SDFAA_HINTED,
            1024,
            1024,
            AtlasPopulationMode.Dynamic,
            false
        );
        if (font == null)
        {
            throw new System.InvalidOperationException(
                "TextMesh Pro non ha potuto creare il font Pixelify Sans."
            );
        }

        font.name = "PixelifySans-SemiBold SDF";
        font.boldStyle = 0.35f;
        font.boldSpacing = 3f;
        font.atlasTexture.name = "PixelifySans-SemiBold Atlas";
        font.material.name = "PixelifySans-SemiBold Material";
        font.material.SetFloat(ShaderUtilities.ID_WeightBold, font.boldStyle);

        AssetDatabase.CreateAsset(font, PercorsoDestinazione);
        AssetDatabase.AddObjectToAsset(font.atlasTexture, font);
        AssetDatabase.AddObjectToAsset(font.material, font);

        string mancanti;
        font.TryAddCharacters(CreaSetCaratteri(), out mancanti);
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        font.isMultiAtlasTexturesEnabled = false;

        TMP_FontAsset fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/" +
            "LiberationSans SDF.asset"
        );
        if (font.fallbackFontAssetTable == null)
        {
            font.fallbackFontAssetTable = new List<TMP_FontAsset>();
        }
        if (fallback != null && !font.fallbackFontAssetTable.Contains(fallback))
        {
            font.fallbackFontAssetTable.Add(fallback);
        }

        EditorUtility.SetDirty(font);
        EditorUtility.SetDirty(font.atlasTexture);
        EditorUtility.SetDirty(font.material);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(
            PercorsoDestinazione,
            ImportAssetOptions.ForceUpdate
        );

        if (!string.IsNullOrEmpty(mancanti))
        {
            Debug.LogWarning(
                "Pixelify Sans non contiene alcuni simboli facoltativi: " +
                mancanti
            );
        }
        Debug.Log("[UI] Font Pixelify Sans SDF generato correttamente.");
    }

    public static void RigeneraDaBatch()
    {
        Rigenera();
    }

    private static string CreaSetCaratteri()
    {
        StringBuilder caratteri = new StringBuilder(512);
        AggiungiIntervallo(caratteri, 0x20, 0x7E);
        caratteri.Append(
            "\u00C0\u00C8\u00C9\u00CC\u00D2\u00D9" +
            "\u00E0\u00E8\u00E9\u00EC\u00F2\u00F9" +
            "\u20AC\u2022\u00D7"
        );
        return caratteri.ToString();
    }

    private static void AggiungiIntervallo(
        StringBuilder destinazione,
        int inizio,
        int fine
    )
    {
        for (int codice = inizio; codice <= fine; codice++)
        {
            destinazione.Append((char)codice);
        }
    }
}
