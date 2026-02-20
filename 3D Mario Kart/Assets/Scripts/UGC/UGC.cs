using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UGC : MonoBehaviour
{
    public static class UGCPaths
    {
        // The path to the Mods folder, which is one level above the Assets folder
        public static string ModsPath = Path.Combine(Application.dataPath, "..", "Mods");

        // The paths to the Courses, Characters, and Karts folders within the Mods folder
        public static string CoursesPath = Path.Combine(ModsPath, "Courses");
        public static string CharactersPath = Path.Combine(ModsPath, "Characters");
        public static string KartsPath = Path.Combine(ModsPath, "Karts");

        // The paths to the subfolders within the Karts folder
        public static string KartsWheelsPath = Path.Combine(KartsPath, "Wheels");
        public static string KartsBodiesPath = Path.Combine(KartsPath, "Bodies");
        public static string KartsGlidersPath = Path.Combine(KartsPath, "Gliders");

        public static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(ModsPath);
            Directory.CreateDirectory(CoursesPath);
            Directory.CreateDirectory(CharactersPath);
            Directory.CreateDirectory(KartsPath);
            Directory.CreateDirectory(KartsWheelsPath);
            Directory.CreateDirectory(KartsBodiesPath);
            Directory.CreateDirectory(KartsGlidersPath);
        }
    }

    // Bundle caches
    public static List<AssetBundle> CourseBundles = new();
    public static List<AssetBundle> CharacterBundles = new();

    public static List<AssetBundle> KartBodyBundles = new();
    public static List<AssetBundle> KartWheelBundles = new();
    public static List<AssetBundle> KartGliderBundles = new();

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        UGCPaths.EnsureDirectoriesExist();

        LoadAllUGC();
    }

    void LoadAllUGC()
    {
        LoadBundlesFromDirectory(UGCPaths.CoursesPath, CourseBundles);
        LoadBundlesFromDirectory(UGCPaths.CharactersPath, CharacterBundles);

        LoadBundlesFromDirectory(UGCPaths.KartsBodiesPath, KartBodyBundles);
        LoadBundlesFromDirectory(UGCPaths.KartsWheelsPath, KartWheelBundles);
        LoadBundlesFromDirectory(UGCPaths.KartsGlidersPath, KartGliderBundles);

        Debug.Log($"Loaded UGC: " +
            $"{CourseBundles.Count} courses, " +
            $"{CharacterBundles.Count} characters, " +
            $"{KartBodyBundles.Count} kart bodies, " +
            $"{KartWheelBundles.Count} wheels, " +
            $"{KartGliderBundles.Count} gliders");
    }

    void LoadBundlesFromDirectory(string path, List<AssetBundle> cache)
    {
        if (!Directory.Exists(path))
            return;

        var files = Directory.GetFiles(path);

        foreach (var file in files)
        {
            // skip manifest files
            if (file.EndsWith(".manifest"))
                continue;

            var bundle = AssetBundle.LoadFromFile(file);

            if (bundle == null)
            {
                Debug.LogWarning($"Failed to load bundle: {file}");
                continue;
            }

            cache.Add(bundle);

            Debug.Log($"Loaded bundle: {Path.GetFileName(file)}");
        }
    }
}
