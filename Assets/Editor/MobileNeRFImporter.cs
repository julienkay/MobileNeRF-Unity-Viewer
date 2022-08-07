using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using static WebRequestAsyncUtility;

public class MobileNeRFImporter {

    private static readonly string DownloadTitle = "Downloading Assets";
    private static readonly string DownloadInfo = "Downloading Assets for ";
    private static readonly string DownloadAllTitle = "Downloading All Assets";
    private static readonly string DownloadAllMessage = "You are about to download all the demo scenes from the MobileNeRF paper!\nDownloading/Processing might take a few minutes and take ~3.3GB of disk space.\n\nClick 'OK', if you wish to continue.";

    [MenuItem("MobileNeRF/Asset Downloads/-- Synthetic 360° scenes --", false, -1)]
    public static void Separator0() { }
    [MenuItem("MobileNeRF/Asset Downloads/-- Synthetic 360° scenes --", true, -1)]
    public static bool Separator0Validate() {
        return false;
    }
    [MenuItem("MobileNeRF/Asset Downloads/-- Forward-facing scenes --", false, 49)]
    public static void Separator1() { }
    [MenuItem("MobileNeRF/Asset Downloads/-- Forward-facing scenes --", true, 49)]
    public static bool Separator1Validate() {
        return false;
    }
    [MenuItem("MobileNeRF/Asset Downloads/-- Unbounded 360° scenes --", false, 99)]
    public static void Separator2() { }
    [MenuItem("MobileNeRF/Asset Downloads/-- Unbounded 360° scenes --", true, 99)]
    public static bool Separator2Validate() {
        return false;
    }

    [MenuItem("MobileNeRF/Asset Downloads/Download All", false, -20)]
    public static async void DownloadAllAssets() {
        if (!EditorUtility.DisplayDialog(DownloadAllTitle, DownloadAllMessage, "OK")) {
            return;
        }

        foreach (var scene in (MNeRFScene[])Enum.GetValues(typeof(MNeRFScene))) {
            await DownloadAssets(scene);
        }
    }

#pragma warning disable CS4014
    [MenuItem("MobileNeRF/Asset Downloads/Chair", false, 0)]
    public static void DownloadChairAssets() {
        ImportAssetsAsync("chair");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Drums", false, 0)]
    public static void DownloadDrumsAssets() {
        ImportAssetsAsync("drums");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Ficus", false, 0)]
    public static void DownloadFicusAssets() {
        ImportAssetsAsync("ficus");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Hotdog", false, 0)]
    public static void DownloadHotdogAssets() {
        ImportAssetsAsync("hotdog");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Lego", false, 0)]
    public static void DownloadLegoAssets() {
        ImportAssetsAsync("lego");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Materials", false, 0)]
    public static void DownloadMaterialsAssets() {
        ImportAssetsAsync("materials");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Mic", false, 0)]
    public static void DownloadMicAssets() {
        ImportAssetsAsync("mic");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Ship", false, 0)]
    public static void DownloadShipsAssets() {
        ImportAssetsAsync("ship");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Fern", false, 50)]
    public static void DownloadFernAssets() {
        ImportAssetsAsync("fern");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Flower", false, 50)]
    public static void DownloadFlowerAssets() {
        ImportAssetsAsync("flower");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Fortress", false, 50)]
    public static void DownloadFortressAssets() {
        ImportAssetsAsync("fortress");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Horns", false, 50)]
    public static void DownloadHornsAssets() {
        ImportAssetsAsync("horns");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Leaves", false, 50)]
    public static void DownloadLeavesAssets() {
        ImportAssetsAsync("leaves");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Orchids", false, 50)]
    public static void DownloadOrchidsAssets() {
        ImportAssetsAsync("orchids");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Room", false, 50)]
    public static void DownloadRoomAssets() {
        ImportAssetsAsync("room");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Trex", false, 50)]
    public static void DownloadTrexAssets() {
        ImportAssetsAsync("trex");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Bicycle", false, 100)]
    public static void DownloadBicycleAssets() {
        ImportAssetsAsync("bicycle");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Garden Vase", false, 100)]
    public static void DownloadGardenAssets() {
        ImportAssetsAsync("gardenvase");
    }
    [MenuItem("MobileNeRF/Asset Downloads/Stump", false, 100)]
    public static void DownloadStumpAssets() {
        ImportAssetsAsync("stump");
    }
#pragma warning restore CS4014

    private static async Task DownloadAssets(MNeRFScene scene) {
        await ImportAssetsAsync(scene.ToString().ToLower());
    }

    private const string BASE_URL = "https://storage.googleapis.com/jax3d-public/projects/mobilenerf/mobilenerf_viewer_mac/";
    //"https://storage.googleapis.com/jax3d-public/projects/mobilenerf/mobilenerf_viewer_mac/zdeferred_syn_mac.html?obj=chair";
    //https://storage.googleapis.com/jax3d-public/projects/mobilenerf/mobilenerf_viewer_mac/chair_mac/mlp.json

    private const string BASE_FOLDER = "Assets/MobileNeRF Data/";

    private static string GetBasePath(string objName) {
        return $"{BASE_FOLDER}{objName}";
    }

    private static string GetMLPUrl(string objName) {
        return $"{BASE_URL}{objName}_mac/mlp.json";
    }

    private static string GetPNGUrl(string objName, int shapeNum, int featureNum) {
        return $"{BASE_URL}{objName}_mac/shape{shapeNum}.pngfeat{featureNum}.png";
    }

    private static string GetOBJUrl(string objName, int i, int j) {
        return $"{BASE_URL}{objName}_mac/shape{i}_{j}.obj";
    }

    private static string GetMLPAssetPath(string objName) {
        string path = $"{GetBasePath(objName)}/MLP/{objName}.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetWeightsAssetPath(string objName, int i) {
        string path = $"{GetBasePath(objName)}/MLP/weightsTex{i}.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static string GetFeatureTextureAssetPath(string objName, int shapeNum, int featureNum) {
        string path = $"{GetBasePath(objName)}/PNGs/shape{shapeNum}.pngfeat{featureNum}.png";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static string GetObjAssetPath(string objName, int i, int j) {
        string path = $"{GetBasePath(objName)}/OBJs/shape{i}_{j}.obj";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static string GetShaderAssetPath(string objName) {
        string path = $"{GetBasePath(objName)}/Shaders/{objName}_shader.shader";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static string GetDefaultMaterialAssetPath(string objName, int i, int j) {
        string path = $"{GetBasePath(objName)}/OBJs/Materials/shape{i}_{j}-defaultMat.mat";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetPrefabAssetPath(string objName) {
        string path = $"{GetBasePath(objName)}/{objName}.prefab";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static async Task ImportAssetsAsync(string objName) {
        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.1f);
        var mlp = await DownloadMlpAsync(objName);
        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.2f);

        CreateShader(objName, mlp);
        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.3f);

        CreateWeightTextures(objName, mlp);
        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.4f);

        await DonloadPNGsAsync(objName, mlp);
        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.6f);

        await DownloadOBJsAsync(objName, mlp);
        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.8f);

        CreatePrefab(objName, mlp);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.ClearProgressBar();
    }

    private static async Task<Mlp> DownloadMlpAsync(string objName) {
        string mlpJson = await SimpleHttpRequestAsync(GetMLPUrl(objName), HTTPVerb.GET);
        TextAsset mlpJsonTextAsset = new TextAsset(mlpJson);
        AssetDatabase.CreateAsset(mlpJsonTextAsset, GetMLPAssetPath(objName));

        Mlp mlp = JsonConvert.DeserializeObject<Mlp>(mlpJson);
        return mlp;
    }

    private static async Task DonloadPNGsAsync(string objName, Mlp mlp) {
        int gTotalPNGs = mlp.ObjNum * 2;

        for (int i = 0; i < mlp.ObjNum; i++) {
            string feat0url = GetPNGUrl(objName, i, 0);
            string feat1url = GetPNGUrl(objName, i, 1);

            string feat0AssetPath = GetFeatureTextureAssetPath(objName, i, 0);
            string feat1AssetPath = GetFeatureTextureAssetPath(objName, i, 1);

            if (File.Exists(feat0AssetPath) && File.Exists(feat1AssetPath)) {
                continue;
            }

            byte[] feat0png = await BinaryHttpRequestAsync(feat0url, HTTPVerb.GET);
            File.WriteAllBytes(feat0AssetPath, feat0png);

            byte[] feat1png = await BinaryHttpRequestAsync(feat1url, HTTPVerb.GET);
            File.WriteAllBytes(feat1AssetPath, feat1png);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ApplyTextureSettings(feat0AssetPath);
            AssetDatabase.ImportAsset(feat0AssetPath);

            ApplyTextureSettings(feat1AssetPath);
            AssetDatabase.ImportAsset(feat1AssetPath);

            AssetDatabase.SaveAssets();
        }
    }

    private static void ApplyTextureSettings(string textureAssetPath) {
        TextureImporter textureImporter = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
        textureImporter.maxTextureSize = 4096;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.sRGBTexture = false;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.mipmapEnabled = false;
        textureImporter.alphaIsTransparency = true;
    }

    private static async Task DownloadOBJsAsync(string objName, Mlp mlp) {
        for (int i = 0; i < mlp.ObjNum; i++) {
            for (int j = 0; j < 8; j++) {
                string objUrl = GetOBJUrl(objName, i, j);
                string objAssetPath = GetObjAssetPath(objName, i, j);

                if (File.Exists(objAssetPath)) {
                    continue;
                }

                byte[] objData = await BinaryHttpRequestAsync(objUrl, HTTPVerb.GET);
                File.WriteAllBytes(objAssetPath, objData);
                AssetDatabase.Refresh();

                ModelImporter modelImport = AssetImporter.GetAtPath(objAssetPath) as ModelImporter;
                modelImport.materialLocation = ModelImporterMaterialLocation.External;
                modelImport.materialName = ModelImporterMaterialName.BasedOnModelNameAndMaterialName;
                AssetDatabase.ImportAsset(objAssetPath);

                // create material
                string shaderAssetPath = GetShaderAssetPath(objName);
                Shader mobileNeRFShader = AssetDatabase.LoadAssetAtPath<Shader>(shaderAssetPath);
                string materialAssetPath = GetDefaultMaterialAssetPath(objName, i, j);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
                material.shader = mobileNeRFShader;
                //material.name = $"defaultMat_{i}_{j}";

                Texture2D weightsTexZero = AssetDatabase.LoadAssetAtPath<Texture2D>(GetWeightsAssetPath(objName, 0));
                Texture2D weightsTexOne  = AssetDatabase.LoadAssetAtPath<Texture2D>(GetWeightsAssetPath(objName, 1));
                Texture2D weightsTexTwo  = AssetDatabase.LoadAssetAtPath<Texture2D>(GetWeightsAssetPath(objName, 2));
                material.SetTexture("weightsZero", weightsTexZero);
                material.SetTexture("weightsOne", weightsTexOne);
                material.SetTexture("weightsTwo", weightsTexTwo);

                string feat0AssetPath = GetFeatureTextureAssetPath(objName, i, 0);
                string feat1AssetPath = GetFeatureTextureAssetPath(objName, i, 1);
                Texture2D featureTex1 = AssetDatabase.LoadAssetAtPath<Texture2D>(feat0AssetPath);
                Texture2D featureTex2 = AssetDatabase.LoadAssetAtPath<Texture2D>(feat1AssetPath);
                material.SetTexture("tDiffuse0x", featureTex1);
                material.SetTexture("tDiffuse1x", featureTex2);

                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(objAssetPath);
                obj.GetComponentInChildren<MeshRenderer>().sharedMaterial = material;

                /*string defaultMaterialAssetPath = $"{GetBasePath(objName)}/Materials/{material.name}.mat";
                AssetDatabase.CreateAsset(material, defaultMaterialAssetPath);
                AssetDatabase.SaveAssets();*/

            }
        }
    }

    /// <summary>
    /// Create shader and material for the specific object
    /// </summary>
    private static void CreateShader(string objName, Mlp mlp) {
        int width = mlp._0Bias.Length;

        StringBuilder biasListZero = toBiasList(mlp._0Bias);
        StringBuilder biasListOne  = toBiasList(mlp._1Bias);
        StringBuilder biasListTwo  = toBiasList(mlp._2Bias);

        int channelsZero  = mlp._0Weights.Length;
        int channelsOne   = mlp._0Bias.Length;
        int channelsTwo   = mlp._1Bias.Length;
        int channelsThree = mlp._2Bias.Length;

        string shaderSource = ViewDependenceNetworkShader.Template;
        shaderSource = new Regex("OBJECT_NAME"       ).Replace(shaderSource, $"{objName}");
        shaderSource = new Regex("NUM_CHANNELS_ZERO" ).Replace(shaderSource, $"{channelsZero}");
        shaderSource = new Regex("NUM_CHANNELS_ONE"  ).Replace(shaderSource, $"{channelsOne}");
        shaderSource = new Regex("NUM_CHANNELS_TWO"  ).Replace(shaderSource, $"{channelsTwo}");
        shaderSource = new Regex("NUM_CHANNELS_THREE").Replace(shaderSource, $"{channelsThree}");
        shaderSource = new Regex("BIAS_LIST_ZERO"    ).Replace(shaderSource, $"{biasListZero}");
        shaderSource = new Regex("BIAS_LIST_ONE"     ).Replace(shaderSource, $"{biasListOne}");
        shaderSource = new Regex("BIAS_LIST_TWO"     ).Replace(shaderSource, $"{biasListTwo}");

        // hack way to flip axes depending on scene
        string axisSwizzle = MNeRFSceneExtensions.ToEnum(objName).GetAxisSwizzleString();
        shaderSource = new Regex("AXIS_SWIZZLE"      ).Replace(shaderSource, $"{axisSwizzle}");

        string shaderAssetPath = GetShaderAssetPath(objName);
        File.WriteAllText(shaderAssetPath, shaderSource);
        AssetDatabase.Refresh();

        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderAssetPath);
    }

    private static void CreateWeightTextures(string objName, Mlp mlp) {
        Texture2D weightsTexZero = createFloatTextureFromData(mlp._0Weights);
        Texture2D weightsTexOne  = createFloatTextureFromData(mlp._1Weights);
        Texture2D weightsTexTwo  = createFloatTextureFromData(mlp._2Weights);
        AssetDatabase.CreateAsset(weightsTexZero, GetWeightsAssetPath(objName, 0));
        AssetDatabase.CreateAsset(weightsTexOne,  GetWeightsAssetPath(objName, 1));
        AssetDatabase.CreateAsset(weightsTexTwo,  GetWeightsAssetPath(objName, 2));
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Creates a float32 texture from an array of floats.
    /// </summary>
    private static Texture2D createFloatTextureFromData(double[][] weights) {
        int width = weights.Length;
        int height = weights[0].Length;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RFloat, mipChain: false, linear: true);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        NativeArray<float> textureData = texture.GetRawTextureData<float>();
        FillTexture(textureData, weights);
        texture.Apply();

        return texture;
    }

    private static void FillTexture(NativeArray<float> textureData, double[][] data) {
        int width = data.Length;
        int height = data[0].Length;

        for (int co = 0; co < height; co++) {
            for (int ci = 0; ci < width; ci++) {
                int index = co * width + ci;
                double weight = data[ci][co];
                textureData[index] = (float)weight;
            }
        }
    }

    private static StringBuilder toBiasList(double[] biases) {
        System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
        int width = biases.Length;
        StringBuilder biasList = new StringBuilder(width * 12);
        for (int i = 0; i < width; i++) {
            double bias = biases[i];
            biasList.Append(bias.ToString("F7", culture));
            if (i + 1 < width) {
                biasList.Append(", ");
            }
        }
        return biasList;
    }

    private static void CreatePrefab(string objName, Mlp mlp) {
        GameObject prefabObject = new GameObject(objName);
        for (int i = 0; i < mlp.ObjNum; i++) {
            for (int j = 0; j < 8; j++) {
                GameObject shapeModel = AssetDatabase.LoadAssetAtPath<GameObject>(GetObjAssetPath(objName, i, j));
                GameObject shape = GameObject.Instantiate(shapeModel);
                shape.name = shape.name.Replace("(Clone)", "");
                shape.transform.SetParent(prefabObject.transform, false);
            }
        }
        PrefabUtility.SaveAsPrefabAsset(prefabObject, GetPrefabAssetPath(objName));
        GameObject.DestroyImmediate(prefabObject);
    }

    private static async Task<T> HttpRequestAsync<T>(string url, HTTPVerb verb, string postData = null, params Tuple<string, string>[] requestHeaders) {
        T result = await WebRequestAsync<T>.SendWebRequestAsync(url, verb, postData, requestHeaders);
        return result;
    }
    private static async Task<string> SimpleHttpRequestAsync(string url, HTTPVerb verb, string postData = null, params Tuple<string, string>[] requestHeaders) {
        return await WebRequestSimpleAsync.SendWebRequestAsync(url, verb, postData, requestHeaders);
    }
    private static async Task<byte[]> BinaryHttpRequestAsync(string url, HTTPVerb verb, string postData = null, params Tuple<string, string>[] requestHeaders) {
        return await WebRequestBinaryAsync.SendWebRequestAsync(url, verb, postData, requestHeaders);
    }
}

// MobileNeRFs don't use normals, so we disable trying to read them when importing
// .obj files (which happens by default) to prevent throwing warnings.
public class ObjImportProcessor : AssetPostprocessor {
    private void OnPreprocessModel() {
        if (assetPath.Contains(".obj")) {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            modelImporter.importNormals = ModelImporterNormals.None;
        }
    }
}

public enum MNeRFScene {
    Chair,
    Drums,
    Ficus,
    Hotdog,
    Lego,
    Materials,
    Mic,
    Ship,
    Fern,
    Flower,
    Fortress,
    Horns,
    Leaves,
    Orchids,
    Room,
    Trex,
    Bicycle,
    Gardenvase,
    Stump
}

public static class MNeRFSceneExtensions {

    public static string String(this MNeRFScene scene) {
        return scene.ToString().ToLower();
    }

    public static MNeRFScene ToEnum(string value) {
        return (MNeRFScene)Enum.Parse(typeof(MNeRFScene), value, true);
    }

    public static string GetAxisSwizzleString(this MNeRFScene scene) {
        switch (scene) {
            case MNeRFScene.Chair:
            case MNeRFScene.Drums:
            case MNeRFScene.Ficus:
            case MNeRFScene.Hotdog:
            case MNeRFScene.Lego:
            case MNeRFScene.Materials:
            case MNeRFScene.Mic:
            case MNeRFScene.Ship:
                // Synthetic 360° scenes
                return "o.rayDirection.xz = -o.rayDirection.xz;" +
                       "o.rayDirection.xyz = o.rayDirection.xzy;";
            case MNeRFScene.Fern:
            case MNeRFScene.Flower:
            case MNeRFScene.Fortress:
            case MNeRFScene.Horns:
            case MNeRFScene.Leaves:
            case MNeRFScene.Orchids:
            case MNeRFScene.Room:
            case MNeRFScene.Trex:
                // Forward-facing scenes
                return "o.rayDirection.x = -o.rayDirection.x;";
            case MNeRFScene.Bicycle:
            case MNeRFScene.Gardenvase:
            case MNeRFScene.Stump:
                // Unbounded 360° scenes
                return "o.rayDirection.xz = -o.rayDirection.xz;" +
                       "o.rayDirection.xyz = o.rayDirection.xzy;";

            default:
                return "";
        }
    }
}