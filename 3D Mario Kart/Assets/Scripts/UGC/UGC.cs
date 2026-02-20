using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public static Action OnFinishedLoading;

    public struct CourseBundle
    {
        public string Name;
        public string FilePath;
        public AssetBundle Bundle;
        public List<string> Scenes;

        public CourseBundle(string filePath, AssetBundle bundle)
        {
            FilePath = filePath;
            Bundle = bundle;
            Name = Path.GetFileName(filePath);

            Scenes = new List<string>();

            var scenePaths = bundle.GetAllScenePaths();

            foreach (var path in scenePaths)
            {
                Scenes.Add(Path.GetFileNameWithoutExtension(path));
            }
        }
    }

    // Bundle caches
    public static List<CourseBundle> CourseBundles = new();
    public static List<AssetBundle> CharacterBundles = new();
    public static List<AssetBundle> KartBodyBundles = new();
    public static List<AssetBundle> KartWheelBundles = new();
    public static List<AssetBundle> KartGliderBundles = new();

    public static class CourseLoader
    {
        public static void LoadSceneFromBundle(AssetBundle bundle, string sceneName)
        {
            var scenePaths = bundle.GetAllScenePaths();

            foreach (var path in scenePaths)
            {
                string name = Path.GetFileNameWithoutExtension(path);

                if (name == sceneName)
                {
                    SceneManager.LoadScene(name, LoadSceneMode.Single);
                    return;
                }
            }

            Debug.LogError($"Scene '{sceneName}' not found in bundle '{bundle.name}'");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        UGCPaths.EnsureDirectoriesExist();

        LoadAllUGC();
    }

    void LoadAllUGC()
    {
        LoadCoursesFromDirectory();
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

        OnFinishedLoading?.Invoke();
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

    public static void LoadCoursesFromDirectory()
    {
        CourseBundles.Clear();

        string path = UGCPaths.CoursesPath;

        if (!Directory.Exists(path))
        {
            Debug.LogWarning($"Courses directory does not exist: {path}");
            return;
        }

        string[] files = Directory.GetFiles(path);

        foreach (string file in files)
        {
            // Skip manifest files
            if (file.EndsWith(".manifest"))
                continue;

            // Skip meta files
            if (file.EndsWith(".meta"))
                continue;

            // Skip directories accidentally returned
            if (Directory.Exists(file))
                continue;

            // Prevent loading same bundle twice
            if (CourseBundles.Exists(x => x.FilePath == file))
                continue;

            AssetBundle bundle = AssetBundle.LoadFromFile(file);

            if (bundle == null)
            {
                Debug.LogError($"Failed to load course bundle: {file}");
                continue;
            }

            CourseBundle courseBundle = new CourseBundle(file, bundle);

            if (courseBundle.Scenes.Count == 0)
            {
                Debug.LogWarning($"Bundle contains no scenes: {file}");
                bundle.Unload(false);
                continue;
            }

            CourseBundles.Add(courseBundle);

            Debug.Log(
                $"Loaded course bundle: {courseBundle.Name} " +
                $"({courseBundle.Scenes.Count} scenes)"
            );
        }

        Debug.Log($"Total courses loaded: {CourseBundles.Count}");
    }
}
