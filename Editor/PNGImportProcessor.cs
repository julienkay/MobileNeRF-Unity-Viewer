using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Configures MobileNeRF feature textures to have the correct import settings.
/// </summary>
public class PNGImportProcessor : AssetPostprocessor {
    
    private void OnPreprocessTexture() {
        Regex featureTexturePattern = new Regex(@"shape[0-9].pngfeat[0-9]\.png");
        if (featureTexturePattern.IsMatch(assetPath)) {
            TextureImporter textureImporter = assetImporter as TextureImporter;
            textureImporter.maxTextureSize = 4096;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.sRGBTexture = false;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.mipmapEnabled = false;
            textureImporter.alphaIsTransparency = false;
        }
    }
}