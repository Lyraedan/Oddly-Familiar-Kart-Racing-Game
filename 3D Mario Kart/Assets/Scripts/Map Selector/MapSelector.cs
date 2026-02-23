using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapSelector : MonoBehaviour
{
    // Maps dropdown index → bundle + scene
    private List<(int bundleIndex, int sceneIndex)> ugcSceneMap = new();

    public GameObject mapOptionPrefab;
    public Transform contentRoot;
    public Button RefreshButton;
    public TextMeshProUGUI ugcLoadProgress;

    public MainMenu mainMenu;

    public List<MapSelectorOption> Default_Maps = new();

    public List<GameObject> UGC_Maps = new();

    private List<MapSelectorOption> AllMaps
    {
        get
        {
            List<MapSelectorOption> combined = new List<MapSelectorOption>();
            combined.AddRange(Default_Maps);
            foreach (var go in UGC_Maps)
            {
                var opt = go.GetComponent<MapSelectorOption>();
                if (opt != null)
                    combined.Add(opt);
            }
            return combined;
        }
    }

    private string currentLoadingUGC = string.Empty;
    private int currentLoadingPercent = 0;

    public bool UGCRefreshing { get; private set; } = false;

    [Header("Input")]
    public InputActionAsset controls;

    private InputActionMap inputMap;
    private InputAction menuUpAction;
    private InputAction menuDownAction;
    private InputAction selectAction;

    private int currentIndex = 0;

    private void Awake()
    {
        // By default we're already loading
        ugcLoadProgress.text = "Refreshing UGC! Please wait...";
        UGCRefreshing = true;

        UGC.OnFinishedLoading += () => {
            ugcLoadProgress.text = string.Empty;
            PopulateUGCOptions();
        };
        UGC.OnReloadedCourses += () => {
            ugcLoadProgress.text = string.Empty;
            PopulateUGCOptions();
        };
        UGC.OnCourseLoadFinished += () =>
        {
            UGCRefreshing = false;
            ugcLoadProgress.text = string.Empty;
        };
        UGC.OnLoadingCourseStarted += (courseName) =>
        {
            currentLoadingUGC = courseName;
            ugcLoadProgress.text = "Loading " + courseName + "..." + currentLoadingPercent + "%";
        };

        UGC.OnCurrentLoadingProgress += (percent) =>
        {
            currentLoadingPercent = Mathf.RoundToInt(percent);
            ugcLoadProgress.text = "Loading " + currentLoadingUGC + "... " + currentLoadingPercent + "%";
        };

        inputMap = controls.FindActionMap("Menu", true);

        menuUpAction = inputMap.FindAction("MenuUp", true);
        menuDownAction = inputMap.FindAction("MenuDown", true);
        selectAction = inputMap.FindAction("Select", true);

        menuUpAction.performed += OnMenuUp;
        menuDownAction.performed += OnMenuDown;
        selectAction.performed += OnSelect;

        inputMap.Enable();
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

        // Broken right now
        //UGC.OnCourseLoadProgress += (progress) =>
        //{
        //    ugcLoadProgress.text = "Loading UGC... " + Mathf.RoundToInt(progress) + "%";

        //    if (progress >= 100f)
        //        ugcLoadProgress.text = string.Empty;
        //};

        PopulateUGCOptions();
    }

    private void Update()
    {
        RefreshButton.interactable = !UGCRefreshing;
    }

    public void Refresh_UGC()
    {
        UGCRefreshing = true;
        ugcLoadProgress.text = "Refreshing UGC! Please wait...";
        ClearUGCOptions();
        UGC.Instance.ReloadCoursesAsync();
    }

    void PopulateUGCOptions()
    {
        ClearUGCOptions();
        for (int bundleIndex = 0; bundleIndex < UGC.CourseBundles.Count; bundleIndex++)
        {
            var bundle = UGC.CourseBundles[bundleIndex];
            if (bundle.SceneBundle == null || bundle.Scenes.Count == 0) continue;

            GenerateFromBundle(bundle, bundleIndex);
        }

        currentIndex = 0;
        UpdateSelection();

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

    private void UpdateSelection()
    {
        var maps = AllMaps;
        if (maps.Count == 0 || currentIndex < 0 || currentIndex >= maps.Count)
            return;

        var selected = maps[currentIndex];
        if (selected != null && selected.PlayButton != null)
            EventSystem.current.SetSelectedGameObject(selected.PlayButton.gameObject);
    }

    private void OnMenuUp(InputAction.CallbackContext context)
    {
        if (!mainMenu.CurrentMenu.Equals(MainMenu.MenuState.Singleplayer))
            return;

        var maps = AllMaps;
        if (maps.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = maps.Count - 1;

        UpdateSelection();
    }

    private void OnMenuDown(InputAction.CallbackContext context)
    {
        if (!mainMenu.CurrentMenu.Equals(MainMenu.MenuState.Singleplayer))
            return;

        var maps = AllMaps;
        if (maps.Count == 0) return;

        currentIndex++;
        if (currentIndex >= maps.Count)
            currentIndex = 0;

        UpdateSelection();
    }

    private void OnSelect(InputAction.CallbackContext context)
    {
        if (!mainMenu.CurrentMenu.Equals(MainMenu.MenuState.Singleplayer))
            return;
        
        var maps = AllMaps;
        if (maps.Count == 0) return;

        var selected = maps[currentIndex];
        if (selected != null && selected.PlayButton != null && selected.PlayButton.interactable)
            selected.OnClick_Play();
    }
}