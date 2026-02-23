using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using static UGC;

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using static UGC;

public static class UGCBundleBuilder
{

    [MenuItem("Mod Tools/Build All UGC")]
    public static void BuildAllUGC()
    {
        BuildFilteredBundles(_ => true);
        Debug.Log("Built ALL UGC bundles.");
    }

    [MenuItem("Mod Tools/Build All Scenes")]
    public static void BuildAllScenes()
    {
        BuildFilteredBundles(name => name.EndsWith(".scene"));
        Debug.Log("Built all .scene bundles.");
    }

    [MenuItem("Mod Tools/Build All Assets")]
    public static void BuildAllAssets()
    {
        BuildFilteredBundles(name => name.EndsWith(".assets"));
        Debug.Log("Built all .assets bundles.");
    }

    [MenuItem("Mod Tools/Build Specific Bundle")]
    public static void OpenSpecificBundleWindow()
    {
        UGCSpecificBundleWindow.ShowWindow();
    }

    [MenuItem("Mod Tools/Build Map (Scene + Asset)")]
    public static void OpenBuildMapWindow()
    {
        UGCMapBuildWindow.ShowWindow();
    }

    [MenuItem("Mod Tools/View Valid Bundles")]
    public static void OpenBundleViewerWindow()
    {
        UGCBundleViewerWindow.ShowWindow();
    }

    static void BuildFilteredBundles(System.Func<string, bool> predicate)
    {
        UGCPaths.EnsureDirectoriesExist();

        var bundleNames = AssetDatabase.GetAllAssetBundleNames();

        foreach (var bundleName in bundleNames)
        {
            if (!predicate(bundleName))
                continue;

            BuildBundleByName(bundleName);
        }
    }

    public static void BuildBundleByName(string bundleName)
    {
        UGCPaths.EnsureDirectoriesExist();

        string outputDir = GetOutputDirectory(bundleName);

        if (string.IsNullOrEmpty(outputDir))
        {
            Debug.LogWarning($"Skipping unknown bundle: {bundleName}");
            return;
        }

        Directory.CreateDirectory(outputDir);

        // Append ".bundle" to the output name
        string outputBundlePath = Path.Combine(outputDir, bundleName + ".bundle");

        var build = new AssetBundleBuild
        {
            assetBundleName = bundleName, // The internal name stays the same
            assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName)
        };

        BuildPipeline.BuildAssetBundles(
            outputDir,
            new[] { build },
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64
        );

        // Rename the output file to include ".bundle"
        string builtFile = Path.Combine(outputDir, bundleName);
        if (File.Exists(builtFile))
        {
            if (File.Exists(outputBundlePath))
                File.Delete(outputBundlePath);
            File.Move(builtFile, outputBundlePath);
        }

        Debug.Log($"Built bundle: {bundleName}.bundle");
    }

    public static void BuildMapPair(string baseCourseName)
    {
        string sceneBundle = baseCourseName + ".scene";
        string assetBundle = baseCourseName + ".assets";

        bool sceneFound = false;
        bool assetFound = false;

        if (AssetDatabase.GetAllAssetBundleNames().Contains(sceneBundle))
        {
            sceneFound = true;
        }

        if (AssetDatabase.GetAllAssetBundleNames().Contains(assetBundle))
        {
            assetFound = true;
        }

        if(assetFound && !sceneFound)
        {
            Debug.LogError($"Asset bundle found without scene: {assetBundle}");
            return;
        }

        if(sceneFound && !assetFound)
        {
            Debug.LogError($"Scene bundle found without asset: {sceneBundle}");
            return;
        }

        if (assetFound && sceneFound)
        {
            BuildBundleByName(sceneBundle);
            BuildBundleByName(assetBundle);
            Debug.Log($"Built map pair: {baseCourseName}");
        }
    }

    static string GetOutputDirectory(string bundleName)
    {
        if (bundleName.StartsWith("course_"))
            return UGCPaths.CoursesPath;

        if (bundleName.StartsWith("character_"))
            return UGCPaths.CharactersPath;

        if (bundleName.StartsWith("kart_"))
        {
            var parts = bundleName.Split('_');

            if (parts.Length < 3)
                return null;

            string sub = parts[1].ToLower();

            switch (sub)
            {
                case "wheels": return UGCPaths.KartsWheelsPath;
                case "bodies": return UGCPaths.KartsBodiesPath;
                case "gliders": return UGCPaths.KartsGlidersPath;
            }
        }

        return null;
    }
}

// Specific bundle
public class UGCSpecificBundleWindow : EditorWindow
{
    private Vector2 scroll;

    public static void ShowWindow()
    {
        GetWindow<UGCSpecificBundleWindow>("Build Specific Bundle");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select Bundle", EditorStyles.boldLabel);

        var bundles = AssetDatabase.GetAllAssetBundleNames();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var bundle in bundles.OrderBy(b => b))
        {
            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.LabelField(bundle);

            if (GUILayout.Button("Build", GUILayout.Width(80)))
            {
                UGCBundleBuilder.BuildBundleByName(bundle);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}

// Assets + Scene
public class UGCMapBuildWindow : EditorWindow
{
    private Vector2 scroll;
    private List<string> mapBaseNames = new();

    public static void ShowWindow()
    {
        var window = GetWindow<UGCMapBuildWindow>("Build Map Pair");
        window.Refresh();
    }

    void Refresh()
    {
        var bundles = AssetDatabase.GetAllAssetBundleNames();

        mapBaseNames = bundles
            .Where(b => b.StartsWith("course_"))
            .Select(b =>
            {
                if (b.EndsWith(".scene"))
                    return b.Replace(".scene", "");
                if (b.EndsWith(".assets"))
                    return b.Replace(".assets", "");
                return null;
            })
            .Where(b => !string.IsNullOrEmpty(b))
            .Distinct()
            .OrderBy(b => b)
            .ToList();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
            Refresh();

        GUILayout.Space(10);
        GUILayout.Label("Build Map (Scene + Assets)", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var baseName in mapBaseNames)
        {
            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.LabelField(baseName);

            if (GUILayout.Button("Build Map", GUILayout.Width(100)))
            {
                UGCBundleBuilder.BuildMapPair(baseName);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}

// Viewer window
public class UGCBundleViewerWindow : EditorWindow
{
    private Vector2 scroll;
    private List<string> mapBaseNames = new();

    public static void ShowWindow()
    {
        var window = GetWindow<UGCBundleViewerWindow>("View Valid Bundles");
        window.Refresh();
    }

    void Refresh()
    {
        var bundles = AssetDatabase.GetAllAssetBundleNames();

        // Get unique base names for course bundles
        mapBaseNames = bundles
            .Where(b => b.StartsWith("course_"))
            .Select(b =>
            {
                if (b.EndsWith(".scene"))
                    return b.Replace(".scene", "");
                if (b.EndsWith(".assets"))
                    return b.Replace(".assets", "");
                return null;
            })
            .Where(b => !string.IsNullOrEmpty(b))
            .Distinct()
            .OrderBy(b => b)
            .ToList();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
            Refresh();

        GUILayout.Space(10);
        GUILayout.Label("UGC Bundles Status", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var baseName in mapBaseNames)
        {
            EditorGUILayout.BeginHorizontal("box");

            bool sceneExists = AssetDatabase.GetAllAssetBundleNames().Contains(baseName + ".scene");
            bool assetsExists = AssetDatabase.GetAllAssetBundleNames().Contains(baseName + ".assets");

            // Determine color
            Color labelColor = (sceneExists && assetsExists) ? Color.green : Color.red;
            GUI.contentColor = labelColor;

            EditorGUILayout.LabelField(baseName);

            GUI.contentColor = Color.white; // Reset color

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}