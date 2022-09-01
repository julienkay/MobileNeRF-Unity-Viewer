using System.Text.RegularExpressions;
using UnityEditor;

/// <summary>
/// Custom import settings for MobileNeRF OBJs.
/// </summary>
public class ObjImportProcessor : AssetPostprocessor {
    private void OnPreprocessModel() {
        Regex objPattern = new Regex(@"shape.*\.obj");

        if (objPattern.IsMatch(assetPath)) {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            // MobileNeRFs don't use normals, so we disable trying to read them when importing .obj files
            // This is not strictly necessary, but prevents the warnings showing in the console.
            modelImporter.importTangents = ModelImporterTangents.None;
            modelImporter.importNormals = ModelImporterNormals.None;

            // one material per shape (each has individual feature textures)
            modelImporter.materialLocation = ModelImporterMaterialLocation.External;
            modelImporter.materialName = ModelImporterMaterialName.BasedOnModelNameAndMaterialName;
        }
    }
}