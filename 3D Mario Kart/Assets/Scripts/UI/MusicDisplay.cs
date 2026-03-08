using System.Collections;
using UnityEngine;

public class MusicDisplay : MonoBehaviour
{
    private Coroutine currentRoutine;

    public void ChangeSongSpeed(AudioSource source, float speed = 1f)
    {
        source.pitch = speed;
    }

    public void DisplayAndPlay(AudioSource source, string author, string newSong, string songType = "Normal")
    {
        ShowDisplay(author, newSong, songType);
        source.Play();
    }

    public void ShowDisplay(string author, string newSong, string songType = "Normal")
    {
        CanvasGroup canvasGroup = IngameUIHolder.Instance.SongDisplayUI;

        IngameUIHolder.Instance.UpdateSong(author, newSong, songType);

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(DisplayRoutine(canvasGroup));
    }

    private IEnumerator DisplayRoutine(CanvasGroup group)
    {
        yield return StartCoroutine(FadeIn(group, 0.5f));

        yield return new WaitForSeconds(3f); // Visible duration

        yield return StartCoroutine(FadeOut(group, 0.5f));
    }

    IEnumerator FadeIn(CanvasGroup group, float duration = 0.5f)
    {
        group.alpha = 0f;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, time / duration);
            yield return null;
        }

        group.alpha = 1f;
    }

    IEnumerator FadeOut(CanvasGroup group, float duration = 0.5f)
    {
        float startAlpha = group.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, 0f, time / duration);
            yield return null;
        }

        group.alpha = 0f;
    }
}
