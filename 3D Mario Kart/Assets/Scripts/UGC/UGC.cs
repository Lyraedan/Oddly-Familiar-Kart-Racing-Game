using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UGC : MonoBehaviour
{

    public static UGC Instance;
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
    public static Action OnReloadedCourses;
    public static Action OnReloadedCharacters;
    public static Action OnReloadedKarts;

    public static Action<CourseBundle> OnCourseBundleLoaded;
    public static Action<int> OnCourseLoadProgress;

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
        Instance = this;

        DontDestroyOnLoad(this.gameObject);
        UGCPaths.EnsureDirectoriesExist();
        StartCoroutine(LoadAllUGCAsync());
    }

    // Async
    private IEnumerator LoadAllUGCAsync()
    {
        yield return StartCoroutine(LoadCoursesFromDirectoryAsync());
        yield return StartCoroutine(LoadBundlesFromDirectoryAsync(UGCPaths.CharactersPath, CharacterBundles));

        yield return StartCoroutine(LoadBundlesFromDirectoryAsync(UGCPaths.KartsBodiesPath, KartBodyBundles));
        yield return StartCoroutine(LoadBundlesFromDirectoryAsync(UGCPaths.KartsWheelsPath, KartWheelBundles));
        yield return StartCoroutine(LoadBundlesFromDirectoryAsync(UGCPaths.KartsGlidersPath, KartGliderBundles));

        Debug.Log($"Loaded UGC: " +
            $"{CourseBundles.Count} courses, " +
            $"{CharacterBundles.Count} characters, " +
            $"{KartBodyBundles.Count} kart bodies, " +
            $"{KartWheelBundles.Count} wheels, " +
            $"{KartGliderBundles.Count} gliders");

        OnFinishedLoading?.Invoke();
    }

    private static IEnumerator LoadBundlesFromDirectoryAsync(string path, List<AssetBundle> cache)
    {
        if (!Directory.Exists(path))
            yield break;

        var files = Directory.GetFiles(path);

        foreach (var file in files)
        {
            if (file.EndsWith(".manifest") || file.EndsWith(".meta"))
                continue;

            var request = AssetBundle.LoadFromFileAsync(file);
            yield return request;

            var bundle = request.assetBundle;

            if (bundle == null)
            {
                Debug.LogWarning($"Failed to load bundle: {file}");
                continue;
            }

            cache.Add(bundle);
            Debug.Log($"Loaded bundle: {Path.GetFileName(file)}");
        }
    }

    private static IEnumerator LoadCoursesFromDirectoryAsync()
    {
        Debug.Log("Loading courses!");

        CourseBundles.Clear();

        string path = UGCPaths.CoursesPath;

        if (!Directory.Exists(path))
        {
            Debug.LogWarning($"Courses directory does not exist: {path}");
            yield break;
        }

        string[] files = Directory.GetFiles(path);

        // Filter valid bundle files first
        List<string> validFiles = new List<string>();
        foreach (string file in files)
        {
            if (file.EndsWith(".manifest") || file.EndsWith(".meta") || Directory.Exists(file))
                continue;

            validFiles.Add(file);
        }

        int totalFiles = validFiles.Count;
        int loadedFiles = 0;

        foreach (string file in validFiles)
        {
            string bundleName = Path.GetFileName(file);

            // Prevent duplicate load
            AssetBundle existingBundle = null;
            foreach (var loaded in AssetBundle.GetAllLoadedAssetBundles())
            {
                if (loaded.name == bundleName)
                {
                    existingBundle = loaded;
                    break;
                }
            }

            CourseBundle courseBundle;

            if (existingBundle != null)
            {
                courseBundle = new CourseBundle(file, existingBundle);
                if (courseBundle.Scenes.Count > 0)
                    CourseBundles.Add(courseBundle);

                loadedFiles++;
                OnCourseBundleLoaded?.Invoke(courseBundle);

                int progressPercentReuse = Mathf.RoundToInt((loadedFiles / (float)totalFiles) * 100f);
                OnCourseLoadProgress?.Invoke(progressPercentReuse);

                continue;
            }

            Debug.Log($"Loading course bundle: {bundleName}");

            var request = AssetBundle.LoadFromFileAsync(file);

            float lastLoggedProgress = -1f;

            while (!request.isDone)
            {
                float fileProgress = request.progress;
                float totalProgress = ((loadedFiles + fileProgress) / totalFiles) * 100f;

                // Only log every 5% change to prevent spam
                if (Mathf.Abs(totalProgress - lastLoggedProgress) >= 5f)
                {
                    lastLoggedProgress = totalProgress;
                    Debug.Log($"Course loading progress: {totalProgress:F1}%");
                }

                int totalPercent = Mathf.RoundToInt(((loadedFiles + fileProgress) / totalFiles) * 100f);
                OnCourseLoadProgress?.Invoke(totalPercent);

                yield return null;
            }

            AssetBundle bundle = request.assetBundle;

            loadedFiles++;

            if (bundle == null)
            {
                Debug.LogError($"Failed to load course bundle: {bundleName}");
                continue;
            }

            courseBundle = new CourseBundle(file, bundle);

            if (courseBundle.Scenes.Count == 0)
            {
                Debug.LogWarning($"Bundle contains no scenes: {bundleName}");
                bundle.Unload(false);
                continue;
            }

            CourseBundles.Add(courseBundle);

            // Invoke the event with the finished bundle
            OnCourseBundleLoaded?.Invoke(courseBundle);

            int progressPercent = Mathf.RoundToInt((loadedFiles / (float)totalFiles) * 100f);
            OnCourseLoadProgress?.Invoke(progressPercent);

            Debug.Log($"Loaded course bundle: {courseBundle.Name} ({courseBundle.Scenes.Count} scenes)");
        }

        Debug.Log($"Total courses loaded: {CourseBundles.Count}");
        OnCourseLoadProgress?.Invoke(100); // Ensure 100% at the end
    }

    public void ReloadAllAsync()
    {
        StartCoroutine(ReloadAllCoroutine());
    }

    public void ReloadCoursesAsync()
    {
        StartCoroutine(ReloadCoursesCoroutine());
    }

    public void ReloadCharactersAsync()
    {
        StartCoroutine(ReloadCharactersCoroutine());
    }

    public void ReloadKartsAsync()
    {
        StartCoroutine(ReloadKartsCoroutine());
    }

    private IEnumerator ReloadAllCoroutine()
    {
        yield return StartCoroutine(ReloadCoursesCoroutine());
        yield return StartCoroutine(ReloadCharactersCoroutine());
        yield return StartCoroutine(ReloadKartsCoroutine());
    }

    private IEnumerator ReloadCoursesCoroutine()
    {
        foreach (var courseBundle in CourseBundles)
            courseBundle.Bundle.Unload(true);

        CourseBundles.Clear();

        yield return StartCoroutine(LoadCoursesFromDirectoryAsync());

        OnReloadedCourses?.Invoke();
    }

    private IEnumerator ReloadCharactersCoroutine()
    {
        foreach (var bundle in CharacterBundles)
            bundle.Unload(true);
        CharacterBundles.Clear();
        yield return StartCoroutine(LoadBundlesFromDirectoryAsync(UGCPaths.CharactersPath, CharacterBundles));
        OnReloadedCharacters?.Invoke();
    }

    private IEnumerator ReloadKartsCoroutine()
    {
        foreach (var bundle in KartBodyBundles)
            bundle.Unload(true);
        KartBodyBundles.Clear();

        foreach (var bundle in KartWheelBundles)
            bundle.Unload(true);

        KartWheelBundles.Clear();
        foreach (var bundle in KartGliderBundles)
            bundle.Unload(true);

        KartGliderBundles.Clear();
        OnReloadedKarts?.Invoke();

        yield return StartCoroutine(LoadBundlesFromDirectoryAsync(UGCPaths.KartsBodiesPath, KartBodyBundles));
        yield return StartCoroutine(LoadBundlesFromDirectoryAsync(UGCPaths.KartsWheelsPath, KartWheelBundles));
        yield return StartCoroutine(LoadBundlesFromDirectoryAsync(UGCPaths.KartsGlidersPath, KartGliderBundles));
    }
}
