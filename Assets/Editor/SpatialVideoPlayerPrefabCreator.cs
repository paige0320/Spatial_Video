using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using SpatialVideo;

public static class SpatialVideoPlayerPrefabCreator
{
    private const string PrefabFolder = "Assets/Prefabs";
    private const string RenderTextureFolder = "Assets/RenderTextures";
    private const string MaterialFolder = "Assets/Materials";

    [MenuItem("Tools/Spatial Video/Create Video Player Prefab")]
    public static void CreatePrefab()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder(RenderTextureFolder);
        EnsureFolder(MaterialFolder);

        RenderTexture rt = CreateOrLoadRenderTexture();
        Material mat = CreateOrLoadMaterial(rt);

        GameObject root = new GameObject("SpatialVideoPlayer");

        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        screen.name = "Screen";
        screen.transform.SetParent(root.transform);
        screen.transform.localPosition = new Vector3(0f, 1.5f, 2f);
        screen.transform.localRotation = Quaternion.identity;
        screen.transform.localScale = new Vector3(3.2f, 1.8f, 1f); // 16:9
        Object.DestroyImmediate(screen.GetComponent<MeshCollider>());

        Renderer screenRenderer = screen.GetComponent<Renderer>();
        screenRenderer.sharedMaterial = mat;

        AudioSource audioSource = root.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;

        VideoPlayer videoPlayer = root.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = rt;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.isLooping = true;

        SpatialVideoPlayer controller = root.AddComponent<SpatialVideoPlayer>();
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("screenRenderer").objectReferenceValue = screenRenderer;
        so.FindProperty("renderTexture").objectReferenceValue = rt;
        VideoClip defaultClip = FindFirstVideoClip();
        if (defaultClip != null)
        {
            so.FindProperty("clip").objectReferenceValue = defaultClip;
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        string prefabPath = PrefabFolder + "/SpatialVideoPlayer.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"SpatialVideoPlayer prefab created at {prefabPath}" +
                   (defaultClip != null ? $" (default clip: {defaultClip.name})" : " (no default clip assigned)"));

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }

    private static RenderTexture CreateOrLoadRenderTexture()
    {
        string path = RenderTextureFolder + "/SpatialVideoRT.renderTexture";
        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
        if (rt == null)
        {
            rt = new RenderTexture(1920, 1080, 0) { name = "SpatialVideoRT" };
            AssetDatabase.CreateAsset(rt, path);
        }
        return rt;
    }

    private static Material CreateOrLoadMaterial(RenderTexture rt)
    {
        string path = MaterialFolder + "/SpatialVideoScreen.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Unlit/Texture");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.mainTexture = rt;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static VideoClip FindFirstVideoClip()
    {
        string[] guids = AssetDatabase.FindAssets("t:VideoClip");
        if (guids.Length == 0) return null;
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<VideoClip>(path);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folderName = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}
