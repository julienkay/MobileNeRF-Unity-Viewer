using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static WebRequestAsyncUtility;

public class MobileNeRFImporter {

    private static readonly string DownloadTitle = "Downloading Assets";
    private static readonly string DownloadInfo = "Downloading Assets for ";
    private static readonly string DownloadAllTitle = "Downloading All Assets";
    private static readonly string DownloadAllMsg = "You are about to download all the demo scenes from the MobileNeRF paper!\nDownloading/Processing might take a few minutes and take ~3.3GB of disk space.\n\nClick 'OK', if you wish to continue.";
    private static readonly string FolderTitle = "Select folder with MobileNeRF Source Files";
    private static readonly string FolderExistsTitle = "Folder already exists";
    private static readonly string FolderExistsMsg = "A folder for this asset already exists in the Unity project. Overwrite?";
    private static readonly string OK = "OK";
    private static readonly string SwitchAxisTitle = "Switch y-z axis?";
    private static readonly string SwitchAxisMsg = "Some scenes (360° scenes / unbounded 360° scenes) require switching the y and z axis in the shader. With forward-facing scenes this is not necessary. Do you want to switch the y and z axis?";
    private static readonly string Switch = "Switch";
    private static readonly string NoSwitch = "Don't Switch";
    private static readonly string ImportErrorTitle = "Error importing MobileNeRF assets";

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
        if (!EditorUtility.DisplayDialog(DownloadAllTitle, DownloadAllMsg, "OK")) {
            return;
        }

        foreach (var scene in (MNeRFScene[])Enum.GetValues(typeof(MNeRFScene))) {
            if (scene.Equals(MNeRFScene.Custom)) {
                continue;
            }
            await ImportDemoSceneAsync(scene);
        }
    }

    [MenuItem("MobileNeRF/Import from disk", false, 0)]
    public static void ImportAssetsFromDisk() {
        // select folder with custom data
        string path = EditorUtility.OpenFolderPanel(FolderTitle, "", "");
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) {
            return;
        }

        // ask whether to overwrite existing folder
        string objName = new DirectoryInfo(path).Name;
        if (Directory.Exists(GetBasePath(objName))) {
            if (!EditorUtility.DisplayDialog(FolderExistsTitle, FolderExistsMsg, OK)) {
                return;
            }
        }

        // ask for axis switch behaviour
        if (EditorUtility.DisplayDialog(SwitchAxisTitle, SwitchAxisMsg, Switch, NoSwitch)) {
            SwizzleAxis = true;
        } else {
            SwizzleAxis = false;
        }

        ImportCustomScene(path);
    }

#pragma warning disable CS4014
    [MenuItem("MobileNeRF/Asset Downloads/Chair", false, 0)]
    public static void DownloadChairAssets() {
        ImportDemoSceneAsync(MNeRFScene.Chair);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Drums", false, 0)]
    public static void DownloadDrumsAssets() {
        ImportDemoSceneAsync(MNeRFScene.Drums);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Ficus", false, 0)]
    public static void DownloadFicusAssets() {
        ImportDemoSceneAsync(MNeRFScene.Ficus);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Hotdog", false, 0)]
    public static void DownloadHotdogAssets() {
        ImportDemoSceneAsync(MNeRFScene.Hotdog);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Lego", false, 0)]
    public static void DownloadLegoAssets() {
        ImportDemoSceneAsync(MNeRFScene.Lego);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Materials", false, 0)]
    public static void DownloadMaterialsAssets() {
        ImportDemoSceneAsync(MNeRFScene.Materials);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Mic", false, 0)]
    public static void DownloadMicAssets() {
        ImportDemoSceneAsync(MNeRFScene.Mic);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Ship", false, 0)]
    public static void DownloadShipsAssets() {
        ImportDemoSceneAsync(MNeRFScene.Ship);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Fern", false, 50)]
    public static void DownloadFernAssets() {
        ImportDemoSceneAsync(MNeRFScene.Fern);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Flower", false, 50)]
    public static void DownloadFlowerAssets() {
        ImportDemoSceneAsync(MNeRFScene.Flower);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Fortress", false, 50)]
    public static void DownloadFortressAssets() {
        ImportDemoSceneAsync(MNeRFScene.Fortress);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Horns", false, 50)]
    public static void DownloadHornsAssets() {
        ImportDemoSceneAsync(MNeRFScene.Horns);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Leaves", false, 50)]
    public static void DownloadLeavesAssets() {
        ImportDemoSceneAsync(MNeRFScene.Leaves);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Orchids", false, 50)]
    public static void DownloadOrchidsAssets() {
        ImportDemoSceneAsync(MNeRFScene.Orchids);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Room", false, 50)]
    public static void DownloadRoomAssets() {
        ImportDemoSceneAsync(MNeRFScene.Room);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Trex", false, 50)]
    public static void DownloadTrexAssets() {
        ImportDemoSceneAsync(MNeRFScene.Trex);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Bicycle", false, 100)]
    public static void DownloadBicycleAssets() {
        ImportDemoSceneAsync(MNeRFScene.Bicycle);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Garden Vase", false, 100)]
    public static void DownloadGardenAssets() {
        ImportDemoSceneAsync(MNeRFScene.Gardenvase);
    }
    [MenuItem("MobileNeRF/Asset Downloads/Stump", false, 100)]
    public static void DownloadStumpAssets() {
        ImportDemoSceneAsync(MNeRFScene.Stump);
    }
#pragma warning restore CS4014

    /// <summary>
    /// Some scenes require switching the y and z axis in the shader.
    /// For custom scenes this tracks, which one should be used.
    /// </summary>
    public static bool SwizzleAxis = false;

    private const string BASE_URL = "https://storage.googleapis.com/jax3d-public/projects/mobilenerf/mobilenerf_viewer_mac/";
    private const string BASE_FOLDER = "Assets/MobileNeRF Data/";

    private static string GetBasePath(string objName) {
        return $"{BASE_FOLDER}{objName}";
    }

    private static string GetMLPUrl(MNeRFScene scene) {
        return $"{BASE_URL}{scene.String()}_mac/mlp.json";
    }

    private static string GetPNGUrl(MNeRFScene scene, int shapeNum, int featureNum) {
        return $"{BASE_URL}{scene.String()}_mac/shape{shapeNum}.pngfeat{featureNum}.png";
    }

    private static string GetOBJUrl(MNeRFScene scene, int i, int j) {
        return $"{BASE_URL}{scene.String()}_mac/shape{i}_{j}.obj";
    }

    private static string GetMLPAssetPath(string objName) {
        string path = $"{GetBasePath(objName)}/MLP/{objName}.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static string GetFeatureTextureAssetPath(string objName, int shapeNum, int featureNum) {
        string path = $"{GetBasePath(objName)}/PNGs/shape{shapeNum}.pngfeat{featureNum}.png";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static string GetObjBaseAssetPath(string objName) {
        string path = $"{GetBasePath(objName)}/OBJs/";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static string GetObjAssetPath(string objName, int i, int j, bool splitShapes) {
        string path;
        if (splitShapes) {
            path = $"{GetBasePath(objName)}/OBJs/shape{i}_{j}.obj";
        } else {
            path = $"{GetBasePath(objName)}/OBJs/shape{i}.obj";
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static string GetShaderAssetPath(string objName) {
        string path = $"{GetBasePath(objName)}/Shaders/{objName}_shader.shader";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static string GetDefaultMaterialAssetPath(string objName, int i, int j, bool splitShapes) {
        string path;
        if (splitShapes) {
            path = $"{GetBasePath(objName)}/OBJs/Materials/shape{i}_{j}-defaultMat.mat";
        } else {
            path = $"{GetBasePath(objName)}/OBJs/Materials/shape{i}-defaultMat.mat";
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetPrefabAssetPath(string objName) {
        string path = $"{GetBasePath(objName)}/{objName}.prefab";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    /// <summary>
    /// Creates Unity assets for the given MobileNeRF assets on disk.
    /// </summary>
    /// <param name="path">The path to the folder with the MobileNeRF assets (OBJs, PNGs, mlp.json)</param>
    private static void ImportCustomScene(string path) {
        string objName = new DirectoryInfo(path).Name;

        Mlp mlp = CopyMLPFromPath(path);
        if (mlp == null) {
            return;
        }
        if (!CopyPNGsFromPath(path, mlp)) {
            return;
        }
        if (!CopyOBJsFromPath(path, mlp)) {
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ProcessAssets(objName);
    }

    /// <summary>
    /// Downloads the given MobileNeRF demo scene and
    /// creates the Unity assets necessary to display it.
    /// </summary>
    private static async Task ImportDemoSceneAsync(MNeRFScene scene) {
        string objName = scene.String();

        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.1f);
        Mlp mlp = await DownloadMlpAsync(scene);

        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.2f);

        await DonloadPNGsAsync(scene, mlp);
        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.6f);

        await DownloadOBJsAsync(scene, mlp);
        EditorUtility.DisplayProgressBar(DownloadTitle, $"{DownloadInfo}'{objName}'...", 0.8f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.ClearProgressBar();

        ProcessAssets(objName);
    }

    /// <summary>
    /// Set specific import settings on OBJs/PNGs.
    /// Creates Materials and Shader from MLP data.
    /// Creates a convenient prefab for the MobileNeRF object.
    /// </summary>
    private static void ProcessAssets(string objName) {
        Mlp mlp = GetMlp(objName);
        CreateShader(objName, mlp);
        // PNGs are configured in PNGImportProcessor.cs
        ProcessOBJs(objName, mlp);
        CreatePrefab(objName, mlp);
    }

    /// <summary>
    /// Looks for a mlp.json at <paramref name="path"/> and imports it.
    /// </summary>
    private static Mlp CopyMLPFromPath(string path) {
        string objName = new DirectoryInfo(path).Name;

        string[] mlpPaths = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
        if (mlpPaths.Length > 1) {
            EditorUtility.DisplayDialog(ImportErrorTitle, "Multiple mlp.json files found", OK);
            return null;
        }
        if (mlpPaths.Length <= 0) {
            EditorUtility.DisplayDialog(ImportErrorTitle, "No mlp.json files found", OK);
            return null;
        }

        string mlpJson = File.ReadAllText(mlpPaths[0]);
        TextAsset mlpJsonTextAsset = new TextAsset(mlpJson);
        AssetDatabase.CreateAsset(mlpJsonTextAsset, GetMLPAssetPath(objName));
        Mlp mlp = JsonConvert.DeserializeObject<Mlp>(mlpJson);
        return mlp;
    }

    /// <summary>
    /// Downloads the MLP for the given MobileNeRF demo scene.
    /// </summary>
    private static async Task<Mlp> DownloadMlpAsync(MNeRFScene scene) {
        string mlpJson = await SimpleHttpRequestAsync(GetMLPUrl(scene), HTTPVerb.GET);
        TextAsset mlpJsonTextAsset = new TextAsset(mlpJson);
        AssetDatabase.CreateAsset(mlpJsonTextAsset, GetMLPAssetPath(scene.String()));
        return JsonConvert.DeserializeObject<Mlp>(mlpJson);
    }

    private static Mlp GetMlp(string objName) {
        string mlpAssetPath = GetMLPAssetPath(objName);
        string mlpJson = AssetDatabase.LoadAssetAtPath<TextAsset>(mlpAssetPath).text;
        return JsonConvert.DeserializeObject<Mlp>(mlpJson);
    }

    /// <summary>
    /// Looks for and imports all feature textures for a given MobileNeRF scene.
    /// </summary>
    private static bool CopyPNGsFromPath(string path, Mlp mlp) {
        string objName = new DirectoryInfo(path).Name;
        int totalPNGs = mlp.ObjNum * 2;

        string[] pngPaths = Directory.GetFiles(path, "shape*.pngfeat*.png", SearchOption.TopDirectoryOnly);
        if (pngPaths.Length != totalPNGs) {
            EditorUtility.DisplayDialog(ImportErrorTitle, $"Invalid number of feature textures found. Expected: {totalPNGs}. Actual: {pngPaths.Length}", OK);
            return false;
        }

        for (int i = 0; i < mlp.ObjNum; i++) {
            for (int j = 0; j < 2; j++) {
                string featPath = Path.Combine(path, $"shape{i}.pngfeat{j}.png");
                string featAssetPath = GetFeatureTextureAssetPath(objName, i, j);

                if (!File.Exists(featPath)) {
                    EditorUtility.DisplayDialog(ImportErrorTitle, $"Required texture not found: {featPath}", OK);
                    return false;
                }

                try {
                    File.Copy(featPath, featAssetPath, overwrite: true);
                } catch (Exception e) {
                    Debug.LogException(e);
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Downloads the feature textures for the given MobileNeRF demo scene.
    /// </summary>
    private static async Task DonloadPNGsAsync(MNeRFScene scene, Mlp mlp) {
        for (int i = 0; i < mlp.ObjNum; i++) {
            string feat0url = GetPNGUrl(scene, i, 0);
            string feat1url = GetPNGUrl(scene, i, 1);

            string feat0AssetPath = GetFeatureTextureAssetPath(scene.String(), i, 0);
            string feat1AssetPath = GetFeatureTextureAssetPath(scene.String(), i, 1);

            if (File.Exists(feat0AssetPath) && File.Exists(feat1AssetPath)) {
                continue;
            }

            byte[] feat0png = await BinaryHttpRequestAsync(feat0url, HTTPVerb.GET);
            File.WriteAllBytes(feat0AssetPath, feat0png);

            byte[] feat1png = await BinaryHttpRequestAsync(feat1url, HTTPVerb.GET);
            File.WriteAllBytes(feat1AssetPath, feat1png);
        }
    }

    /// <summary>
    /// Looks for and imports all 3D models for a given MobileNeRF scene.
    /// </summary>
    private static bool CopyOBJsFromPath(string path, Mlp mlp) {
        string objName = new DirectoryInfo(path).Name;

        string[] objPaths = Directory.GetFiles(path, "shape*.obj", SearchOption.TopDirectoryOnly);
        bool splitShapes = AreOBJsSplit(path);
        int numSplitShapes = GetNumSplitShapes(splitShapes);

        if (splitShapes && objPaths.Length != mlp.ObjNum * 8) {
            EditorUtility.DisplayDialog(ImportErrorTitle, $"Invalid number of shape files found. Expected: {mlp.ObjNum * 8}. Actual: {objPaths.Length}", OK);
            return false;
        } else if (!splitShapes && objPaths.Length != mlp.ObjNum) {
            EditorUtility.DisplayDialog(ImportErrorTitle, $"Invalid number of shape files found. Expected: {mlp.ObjNum    }. Actual: {objPaths.Length}", OK);
            return false;
        }

        for (int i = 0; i < mlp.ObjNum; i++) {
            for (int j = 0; j < numSplitShapes; j++) {
                string objPath;
                if (splitShapes) {
                    objPath = Path.Combine(path, $"shape{i}_{j}.obj");
                } else {
                    objPath = Path.Combine(path, $"shape{i}.obj");
                }

                string objAssetPath = GetObjAssetPath(objName, i, j, splitShapes);

                if (!File.Exists(objPath)) {
                    EditorUtility.DisplayDialog(ImportErrorTitle, $"Required .obj file not found: {objPath}", OK);
                    return false;
                }
                try {
                    File.Copy(objPath, objAssetPath, overwrite: true);
                } catch (Exception e) {
                    Debug.LogException(e);
                    return false;
                }
            }
        }

        return true;
    }

    private static bool AreOBJsSplit(string path) {
        if (Directory.GetFiles(path, "shape*_*.obj", SearchOption.TopDirectoryOnly).Length > 0) {
            return true;
        } else if (Directory.GetFiles(path, "shape*.obj", SearchOption.TopDirectoryOnly).Length > 0) {
            return false;
        } else {
            return false;
        }
    }

    private static int GetNumSplitShapes(bool splitShapes) {
        return splitShapes ? 8 : 1;
    }

    /// <summary>
    /// Downloads the 3D models for the given MobileNeRF demo scene.
    /// </summary>
    private static async Task DownloadOBJsAsync(MNeRFScene scene, Mlp mlp) {
        for (int i = 0; i < mlp.ObjNum; i++) {
            for (int j = 0; j < 8; j++) {
                string objUrl = GetOBJUrl(scene, i, j);
                string objAssetPath = GetObjAssetPath(scene.String(), i, j, splitShapes: true);

                if (File.Exists(objAssetPath)) {
                    continue;
                }

                byte[] objData = await BinaryHttpRequestAsync(objUrl, HTTPVerb.GET);
                File.WriteAllBytes(objAssetPath, objData);
            }
        }
    }

    private static void ProcessOBJs(string objName, Mlp mlp) {
        bool splitShapes = AreOBJsSplit(GetObjBaseAssetPath(objName));
        int numSplitShapes = GetNumSplitShapes(splitShapes);

        for (int i = 0; i < mlp.ObjNum; i++) {
            for (int j = 0; j < numSplitShapes; j++) {
                string objAssetPath = GetObjAssetPath(objName, i, j, splitShapes);

                // create material
                string shaderAssetPath = GetShaderAssetPath(objName);
                Shader mobileNeRFShader = AssetDatabase.LoadAssetAtPath<Shader>(shaderAssetPath);
                string materialAssetPath = GetDefaultMaterialAssetPath(objName, i, j, splitShapes);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
                material.shader = mobileNeRFShader;

                // assign feature textures
                string feat0AssetPath = GetFeatureTextureAssetPath(objName, i, 0);
                string feat1AssetPath = GetFeatureTextureAssetPath(objName, i, 1);
                Texture2D featureTex1 = AssetDatabase.LoadAssetAtPath<Texture2D>(feat0AssetPath);
                Texture2D featureTex2 = AssetDatabase.LoadAssetAtPath<Texture2D>(feat1AssetPath);
                material.SetTexture("tDiffuse0x", featureTex1);
                material.SetTexture("tDiffuse1x", featureTex2);

                // assign material to renderer
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(objAssetPath);
                obj.GetComponentInChildren<MeshRenderer>().sharedMaterial = material;
            }
        }
    }

    /// <summary>
    /// Create shader and material for the specific object
    /// </summary>
    private static void CreateShader(string objName, Mlp mlp) {
        int width = mlp._0Bias.Length;

        StringBuilder biasListZero = toConstructorList(mlp._0Bias);
        StringBuilder biasListOne  = toConstructorList(mlp._1Bias);
        StringBuilder biasListTwo  = toConstructorList(mlp._2Bias);

        string shaderSource = ViewDependenceNetworkShader.Template;
        shaderSource = new Regex("OBJECT_NAME"       ).Replace(shaderSource, $"{objName}");
        shaderSource = new Regex("BIAS_LIST_ZERO"    ).Replace(shaderSource, $"{biasListZero}");
        shaderSource = new Regex("BIAS_LIST_ONE"     ).Replace(shaderSource, $"{biasListOne}");
        shaderSource = new Regex("BIAS_LIST_TWO"     ).Replace(shaderSource, $"{biasListTwo}");

        for (int i = 0; i < mlp._0Weights.Length; i++) {
            shaderSource = new Regex($"__W0_{i}__").Replace(shaderSource, $"{toConstructorList(mlp._0Weights[i])}");
        }
        for (int i = 0; i < mlp._1Weights.Length; i++) {
            shaderSource = new Regex($"__W1_{i}__").Replace(shaderSource, $"{toConstructorList(mlp._1Weights[i])}");
        }
        for (int i = 0; i < mlp._2Weights.Length; i++) {
            shaderSource = new Regex($"__W2_{i}__").Replace(shaderSource, $"{toConstructorList(mlp._2Weights[i])}");
        }

        // hack way to flip axes depending on scene
        string axisSwizzle = MNeRFSceneExtensions.ToEnum(objName).GetAxisSwizzleString();
        shaderSource = new Regex("AXIS_SWIZZLE"      ).Replace(shaderSource, $"{axisSwizzle}");

        string shaderAssetPath = GetShaderAssetPath(objName);
        File.WriteAllText(shaderAssetPath, shaderSource);
        AssetDatabase.Refresh();
    }

    private static StringBuilder toConstructorList(double[] list) {
        System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
        int width = list.Length;
        StringBuilder biasList = new StringBuilder(width * 12);
        for (int i = 0; i < width; i++) {
            double bias = list[i];
            biasList.Append(bias.ToString("F7", culture));
            if (i + 1 < width) {
                biasList.Append(", ");
            }
        }
        return biasList;
    }

    private static void CreatePrefab(string objName, Mlp mlp) {
        bool splitShapes = AreOBJsSplit(GetObjBaseAssetPath(objName));
        int numSplitShapes = GetNumSplitShapes(splitShapes);
        GameObject prefabObject = new GameObject(objName);
        for (int i = 0; i < mlp.ObjNum; i++) {
            for (int j = 0; j < numSplitShapes; j++) {
                GameObject shapeModel = AssetDatabase.LoadAssetAtPath<GameObject>(GetObjAssetPath(objName, i, j, splitShapes));
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