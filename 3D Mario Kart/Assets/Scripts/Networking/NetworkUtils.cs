using System.Collections;
using Steamworks;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkUtils : MonoBehaviour
{
    public static NetworkUtils Instance;
    public CanvasGroup loadingScreenCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMapAndHost(string sceneName)
    {
        StartCoroutine(LoadSceneAndStartServer(sceneName));
    }

    private IEnumerator LoadSceneAndStartServer(string sceneName)
    {
        // Fade in the loading screen if it's not already visible
        if (loadingScreenCanvas.alpha != 0f)
            yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(loadingScreenCanvas, 1f, 0.25f));

        // Load scene locally on host
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        yield return asyncLoad;
        OnSceneLoaded();
        NetworkManager.Singleton.StartHost();
        SteamMatchmaking.SetLobbyData(USteamClient.Instance.CurrentLobbyId, "started", "1");
        yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(loadingScreenCanvas, 0f, 0.25f)); // Fade the loading screen away after the scene has loaded
    }

    public void LoadMapAndStartClient(string sceneName)
    {
        StartCoroutine(LoadSceneAndStartClient(sceneName));
    }

    private IEnumerator LoadSceneAndStartClient(string sceneName)
    {
        if (loadingScreenCanvas.alpha != 0f)
            yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(loadingScreenCanvas, 1f, 0.25f));

        // Load scene locally on host
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        yield return asyncLoad;
        CustomizableSpriteLibrary.Instance.AssignSpritesInScene(); // Update custom UI
        OnSceneLoaded();
        NetworkManager.Singleton.StartClient();
        yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(loadingScreenCanvas, 0f, 0.25f)); // Fade the loading screen away after the scene has loaded
    }


    void OnSceneLoaded()
    {
        CustomizableSpriteLibrary.Instance.AssignSpritesInScene(); // Update custom UI
    }

    public void HostEndGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            SteamMatchmaking.SetLobbyData(USteamClient.Instance.CurrentLobbyId, "started", "0");
            NetworkManager.Singleton.Shutdown();
        }
    }
}
