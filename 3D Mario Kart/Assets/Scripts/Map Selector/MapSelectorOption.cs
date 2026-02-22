using System.Collections;
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

    public AudioSource pressSound;

    private void Start()
    {
        MapNameText.text = MapName;
    }

    public void OnClick_Play()
    {
        PlayButton.interactable = false;
        StartCoroutine(DelayedPlay());
    }

    IEnumerator DelayedPlay()
    {
        if (pressSound != null)
            pressSound.Play();

        yield return new WaitForSeconds(pressSound.clip.length);
        PlayButton.interactable = true;
        PlayMap();
    }
    public void PlayMap()
    {
        SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
    }
}
