using System.Text.RegularExpressions;
using UnityEditor;

/// <summary>
/// MobileNeRFs don't use normals, so we disable trying to read them when importing .obj files
/// This is not strictly necessary, but prevents the warnings showing in the console.
/// </summary>
public class ObjImportProcessor : AssetPostprocessor {
    private void OnPreprocessModel() {
        Regex objPattern = new Regex("shape[0-9]_[0-9].obj");

        if (objPattern.IsMatch(assetPath)) {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            modelImporter.importTangents = ModelImporterTangents.None;
            modelImporter.importNormals = ModelImporterNormals.None;
        }
    }
}