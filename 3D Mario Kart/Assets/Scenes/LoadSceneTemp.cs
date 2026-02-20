using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadSceneTemp : MonoBehaviour
{
    Resolution[] res;

    public Dropdown dropdown;
    public Dropdown ugc_dropdown;

    // Maps dropdown index → bundle + scene
    private List<(int bundleIndex, int sceneIndex)> ugcSceneMap = new();

    void Start()
    {
        res = Screen.resolutions;

        dropdown.ClearOptions();
        ugc_dropdown.ClearOptions();

        PopulateResolutionDropdown();
        UGC.OnFinishedLoading += PopulateUGCDropdown;
    }
    void PopulateResolutionDropdown()
    {
        List<string> resolutionStrings = new List<string>();

        int currentResIndex = 0;

        for (int i = 0; i < res.Length; i++)
        {
            string option = res[i].width + " x " + res[i].height;
            resolutionStrings.Add(option);

            if (res[i].height == Screen.height && res[i].width == Screen.width)
            {
                currentResIndex = i;
            }
        }

        dropdown.AddOptions(resolutionStrings);
        dropdown.value = currentResIndex;
        dropdown.RefreshShownValue();
    }

    void PopulateUGCDropdown()
    {
        List<string> options = new List<string>();
        ugcSceneMap.Clear();

        for (int bundleIndex = 0; bundleIndex < UGC.CourseBundles.Count; bundleIndex++)
        {
            var bundle = UGC.CourseBundles[bundleIndex];

            for (int sceneIndex = 0; sceneIndex < bundle.Scenes.Count; sceneIndex++)
            {
                string sceneName = bundle.Scenes[sceneIndex];

                // Example display name: map_desert / DesertTrack
                string displayName = bundle.Name + " / " + sceneName;

                options.Add(displayName);

                ugcSceneMap.Add((bundleIndex, sceneIndex));
            }
        }

        if (options.Count == 0)
        {
            options.Add("No UGC Courses Found");
        }

        ugc_dropdown.AddOptions(options);
        ugc_dropdown.RefreshShownValue();
    }

    public void setResolution(int resIndex)
    {
        Resolution selectedResolution = res[resIndex];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
    }

    public void setVSync(int index)
    {
        QualitySettings.vSyncCount = index;
    }

    public void LoadSelectedUGC()
    {
        int dropdownIndex = ugc_dropdown.value;

        if (dropdownIndex < 0 || dropdownIndex >= ugcSceneMap.Count)
        {
            Debug.LogError("Invalid UGC selection");
            return;
        }

        var map = ugcSceneMap[dropdownIndex];

        var courseBundle = UGC.CourseBundles[map.bundleIndex];

        string sceneName = courseBundle.Scenes[map.sceneIndex];

        Debug.Log($"Loading UGC Scene: {sceneName}");

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    // Your existing built-in scene loaders remain unchanged
    public void MooMooMeadows()
    {
        SceneManager.LoadScene("MooMooMeadows");
    }

    public void ToadHarbor()
    {
        SceneManager.LoadScene("Toad Harbor");
    }

    public void MarioCircuit()
    {
        SceneManager.LoadScene("Mario Circuit");
    }

    public void RainbowRoad()
    {
        SceneManager.LoadScene("Rainbow Road");
    }

    public void WaterPark()
    {
        SceneManager.LoadScene("Water Park");
    }
}