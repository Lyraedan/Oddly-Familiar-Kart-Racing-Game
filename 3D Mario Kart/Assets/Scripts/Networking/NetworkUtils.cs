using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkUtils : MonoBehaviour
{
    public static NetworkUtils Instance;

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
        // Load scene locally on host
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        yield return asyncLoad;
        NetworkManager.Singleton.StartHost();
    }
}
