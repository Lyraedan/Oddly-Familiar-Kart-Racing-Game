using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class MapSelector : MonoBehaviour
{

    // Maps dropdown index → bundle + scene
    private List<(int bundleIndex, int sceneIndex)> ugcSceneMap = new();
    public GameObject mapOptionPrefab;
    public Transform contentRoot;
    public Button RefreshButton;
    public TextMeshProUGUI ugcLoadProgress;

    public List<GameObject> UGC_Maps;

    private void Awake()
    {
        UGC.OnFinishedLoading += PopulateUGCOptions;
        UGC.OnReloadedCourses += PopulateUGCOptions;
    }

    private void Start()
    {
        UGC.OnCourseBundleLoaded += (bundle) =>
        {
            GenerateFromBundle(bundle, UGC.CourseBundles.Count - 1);
        };

        UGC.OnCourseLoadProgress += (progress) =>
        {
            ugcLoadProgress.text = "Loading UGC... " + Mathf.RoundToInt(progress) + "%";
            if(progress >= 100f)
            {
                ugcLoadProgress.text = string.Empty;
            }
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
            GenerateFromBundle(bundle, bundleIndex);
        }
        RefreshButton.interactable = true;
    }

    private void GenerateFromBundle(UGC.CourseBundle bundle, int bundleIndex)
    {
        for (int sceneIndex = 0; sceneIndex < bundle.Scenes.Count; sceneIndex++)
        {
            string sceneName = bundle.Scenes[sceneIndex];

            // Example display name: map_desert / DesertTrack
            string displayName = bundle.Name.Substring("course_".Length) + " / " + sceneName;
            GenerateOption(displayName, sceneName);

            ugcSceneMap.Add((bundleIndex, sceneIndex));
        }
    }

    void ClearUGCOptions()
    {
        foreach (GameObject option in UGC_Maps)
        {
            Destroy(option);
        }
        UGC_Maps.Clear();
    }

    void GenerateOption(string mapName, string sceneName)
    {
        GameObject option = Instantiate(mapOptionPrefab, contentRoot);
        MapSelectorOption optionScript = option.GetComponent<MapSelectorOption>();
        optionScript.MapName = mapName;
        optionScript.SceneName = sceneName;
        optionScript.MapNameText.text = optionScript.MapName;
        UGC_Maps.Add(option);
    }
}
