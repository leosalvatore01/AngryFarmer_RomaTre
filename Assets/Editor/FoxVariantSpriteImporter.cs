using UnityEditor;
using UnityEngine;

/// <summary>
/// Mantiene coerenti i frame dedicati delle volpi con gli sprite originali.
/// Il percorso e' volutamente ristretto per non modificare altri asset grafici.
/// </summary>
public sealed class FoxVariantSpriteImporter : AssetPostprocessor
{
    private const string CartellaVolpi = "Assets/Resources/Foxes/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(CartellaVolpi, System.StringComparison.Ordinal))
        {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 256f;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.alphaIsTransparency = true;
        importer.isReadable = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.Tight;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spritePivot = new Vector2(0.5f, 0.5f);
        importer.SetTextureSettings(settings);
    }
}
