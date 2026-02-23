using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSelector : MonoBehaviour
{
    // Maps dropdown index → bundle + scene
    private List<(int bundleIndex, int sceneIndex)> ugcSceneMap = new();

    public GameObject mapOptionPrefab;
    public Transform contentRoot;
    public Button RefreshButton;
    public TextMeshProUGUI ugcLoadProgress;

    public List<GameObject> UGC_Maps = new();

    private void Awake()
    {
        UGC.OnFinishedLoading += PopulateUGCOptions;
        UGC.OnReloadedCourses += PopulateUGCOptions;
    }

    private void Start()
    {
        UGC.OnCourseBundleLoaded += (bundle) =>
        {
            // Only generate if this bundle has a scene bundle AND a valid assets bundle
            bool validSceneBundle = bundle.SceneBundle != null && bundle.Scenes.Count > 0;
            bool validAssetBundle = bundle.AssetBundle != null;
            if (validSceneBundle && validAssetBundle)
            {
                int index = UGC.CourseBundles.IndexOf(bundle);
                if (index >= 0)
                    GenerateFromBundle(bundle, index);
            }
        };

        UGC.OnCourseLoadProgress += (progress) =>
        {
            ugcLoadProgress.text = "Loading UGC... " + Mathf.RoundToInt(progress) + "%";

            if (progress >= 100f)
                ugcLoadProgress.text = string.Empty;
        };

        PopulateUGCOptions();
    }

    private void Update()
    {
        RefreshButton.interactable = string.IsNullOrEmpty(ugcLoadProgress.text);
    }

    public void Refresh_UGC()
    {
        ClearUGCOptions();
        UGC.Instance.ReloadCoursesAsync();
    }

    void PopulateUGCOptions()
    {
        ClearUGCOptions();

        for (int bundleIndex = 0; bundleIndex < UGC.CourseBundles.Count; bundleIndex++)
        {
            var bundle = UGC.CourseBundles[bundleIndex];

            // Only populate from .scene bundles
            if (bundle.SceneBundle == null || bundle.Scenes.Count == 0)
                continue;

            GenerateFromBundle(bundle, bundleIndex);
        }

        RefreshButton.interactable = true;
    }

    private void GenerateFromBundle(UGC.CourseBundle bundle, int bundleIndex)
    {
        if (bundle.SceneBundle == null)
            return;

        for (int sceneIndex = 0; sceneIndex < bundle.Scenes.Count; sceneIndex++)
        {
            string sceneName = bundle.Scenes[sceneIndex];

            string displayName = bundle.Name;

            if (displayName.StartsWith("course_"))
                displayName = displayName.Substring("course_".Length);

            displayName += " / " + sceneName;

            GenerateOption(displayName, sceneName);

            ugcSceneMap.Add((bundleIndex, sceneIndex));
        }
    }

    void ClearUGCOptions()
    {
        foreach (GameObject option in UGC_Maps)
            Destroy(option);

        UGC_Maps.Clear();
        ugcSceneMap.Clear();
    }

    void GenerateOption(string mapName, string sceneName)
    {
        GameObject option = Instantiate(mapOptionPrefab, contentRoot);

        MapSelectorOption optionScript = option.GetComponent<MapSelectorOption>();
        optionScript.MapName = mapName;
        optionScript.SceneName = sceneName;
        optionScript.MapNameText.text = mapName;

        UGC_Maps.Add(option);
    }
}