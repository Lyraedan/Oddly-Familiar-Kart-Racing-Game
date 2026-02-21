using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSelectorOption : MonoBehaviour
{
    public string MapName;
    public string SceneName;

    public TextMeshProUGUI MapNameText;
    public Button PlayButton;

    private void Start()
    {
        MapNameText.text = MapName;
    }

    public void PlayMap()
    {
        SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
    }
}
