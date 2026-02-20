using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using static UGC;

public static class UGCBundleBuilder
{
    [MenuItem("Mod Tools/Build UGC Bundles")]
    public static void BuildAllUGCBundles()
    {
        UGCPaths.EnsureDirectoriesExist();

        var bundleNames = AssetDatabase.GetAllAssetBundleNames();

        foreach (var bundleName in bundleNames)
        {
            string outputDir = GetOutputDirectory(bundleName);

            if (string.IsNullOrEmpty(outputDir))
            {
                Debug.LogWarning($"Unknown bundle type: {bundleName}");
                continue;
            }

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            BuildSingleBundle(bundleName, outputDir);
        }

        Debug.Log("UGC bundles built successfully.");
    }

    static void BuildSingleBundle(string bundleName, string outputDir, BuildTarget buildTarget = BuildTarget.StandaloneWindows64)
    {
        var build = new AssetBundleBuild
        {
            assetBundleName = bundleName,
            assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName)
        };

        BuildPipeline.BuildAssetBundles(
            outputDir,
            new[] { build },
            BuildAssetBundleOptions.None,
            buildTarget
        );

        Debug.Log($"Built bundle: {bundleName} → {outputDir}");
    }

    static string GetOutputDirectory(string bundleName)
    {
        // course_<name>
        if (bundleName.StartsWith("course_"))
        {
            return UGCPaths.CoursesPath;
        }

        // character_<name>
        if (bundleName.StartsWith("character_"))
        {
            return UGCPaths.CharactersPath;
        }

        // kart_<sub>_<name>
        if (bundleName.StartsWith("kart_"))
        {
            var parts = bundleName.Split('_');

            if (parts.Length < 3)
                return null;

            string sub = parts[1].ToLower();

            switch (sub)
            {
                case "wheels":
                    return UGCPaths.KartsWheelsPath;

                case "bodies":
                    return UGCPaths.KartsBodiesPath;

                case "gliders":
                    return UGCPaths.KartsGlidersPath;

                default:
                    Debug.LogError($"Unknown kart subcategory: {sub}");
                    return null;
            }
        }

        Debug.LogError("Invalid bundle name format: " + bundleName + "\nShould start with course_, character_, kart_wheels_, kart_bodies_ or kart_gliders_");
        return null;
    }

    
}