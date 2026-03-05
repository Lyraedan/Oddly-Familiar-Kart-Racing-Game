using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.SinglePlayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSelectorOption : MonoBehaviour
{
    public string MapName;
    public string SceneName;
    public UGC.CourseBundle Bundle; // Not used but could come in handy in the future

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
        NetworkUtils.Instance.LoadMapAndHost(SceneName);
    }
}
