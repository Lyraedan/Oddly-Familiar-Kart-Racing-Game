using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public static Action<string> OnFailedToLoadCourse;

    public List<string> FailedBundles = new();

    public struct CourseBundle
    {
        public string Name;
        public string SceneFilePath;
        public string AssetFilePath;

        public AssetBundle SceneBundle;
        public AssetBundle AssetBundle;

        public List<string> Scenes;

        public CourseBundle(
            string name,
            string sceneFilePath,
            string assetFilePath,
            AssetBundle sceneBundle,
            AssetBundle assetBundle)
        {
            Name = name;
            SceneFilePath = sceneFilePath;
            AssetFilePath = assetFilePath;

            SceneBundle = sceneBundle;
            AssetBundle = assetBundle;

            Scenes = new List<string>();

            if (sceneBundle != null)
            {
                var scenePaths = sceneBundle.GetAllScenePaths();

                foreach (var path in scenePaths)
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
        public static void LoadSceneFromBundle(CourseBundle course, string sceneName)
        {
            var scenePaths = course.SceneBundle.GetAllScenePaths();

            foreach (var path in scenePaths)
            {
                string name = Path.GetFileNameWithoutExtension(path);

                if (name == sceneName)
                {
                    SceneManager.LoadScene(name, LoadSceneMode.Single);
                    return;
                }
            }

            Debug.LogError($"Scene '{sceneName}' not found in course '{course.Name}'");
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
                OnFailedToLoadCourse?.Invoke(file);
                Instance.FailedBundles.Add(file);
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

        var files = Directory.GetFiles(path)
            .Where(f => !f.EndsWith(".manifest") && !f.EndsWith(".meta"))
            .ToList();

        // Group by base course name
        Dictionary<string, (string sceneFile, string assetFile)> grouped =
            new Dictionary<string, (string, string)>();

        foreach (var file in files)
        {
            string filename = Path.GetFileName(file);

            if (filename.EndsWith(".scene.bundle"))
            {
                string baseName = filename.Replace(".scene.bundle", "");
                grouped.TryAdd(baseName, (null, null));
                grouped[baseName] = (file, grouped[baseName].assetFile);
            }
            else if (filename.EndsWith(".assets.bundle"))
            {
                string baseName = filename.Replace(".assets.bundle", "");
                grouped.TryAdd(baseName, (null, null));
                grouped[baseName] = (grouped[baseName].sceneFile, file);
            }
        }

        int total = grouped.Count;
        int loaded = 0;

        foreach (var pair in grouped)
        {
            string courseName = pair.Key;
            string sceneFile = pair.Value.sceneFile;
            string assetFile = pair.Value.assetFile;

            AssetBundle sceneBundle = null;
            AssetBundle assetBundle = null;

            if (!string.IsNullOrEmpty(sceneFile))
            {
                Debug.Log("Loading scene bundle for course: " + courseName);
                var sceneRequest = AssetBundle.LoadFromFileAsync(sceneFile);
                yield return sceneRequest;
                sceneBundle = sceneRequest.assetBundle;
            }

            if (!string.IsNullOrEmpty(assetFile))
            {
                Debug.Log("Loading assets bundle for course: " + courseName);
                var assetRequest = AssetBundle.LoadFromFileAsync(assetFile);
                yield return assetRequest;
                assetBundle = assetRequest.assetBundle;
            }

            if (sceneBundle == null)
            {
                Debug.LogWarning($"Course '{courseName}' has no scene bundle. Skipping.");
                continue;
            }

            var courseBundle = new CourseBundle(
                courseName,
                sceneFile,
                assetFile,
                sceneBundle,
                assetBundle
            );

            CourseBundles.Add(courseBundle);

            loaded++;
            int percent = Mathf.RoundToInt((loaded / (float)total) * 100f);
            OnCourseLoadProgress?.Invoke(percent);
            OnCourseBundleLoaded?.Invoke(courseBundle);

            Debug.Log($"Loaded course: {courseName}");
        }

        OnCourseLoadProgress?.Invoke(100);
        Debug.Log($"Total courses loaded: {CourseBundles.Count}");
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
        {
            if (courseBundle.SceneBundle != null)
                courseBundle.SceneBundle.Unload(true);

            if (courseBundle.AssetBundle != null)
                courseBundle.AssetBundle.Unload(true);
        }

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
