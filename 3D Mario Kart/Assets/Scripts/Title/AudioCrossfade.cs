using UnityEngine;
using System.Collections;

public class AudioCrossfade : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource audioSource1;
    public AudioSource audioSource2;

    [Header("Fade Settings")]
    public float fadeDuration = 2f;

    /// <summary>
    /// Start the crossfade from audioSource1 to audioSource2
    /// </summary>
    public void StartCrossfade()
    {
        if (audioSource1 == null || audioSource2 == null)
        {
            Debug.LogWarning("Audio sources not set!");
            return;
        }

        // If either source has a RandomSongPlayer, use its fadeMultiplier system
        RandomSongPlayer player1 = audioSource1.GetComponent<RandomSongPlayer>();
        RandomSongPlayer player2 = audioSource2.GetComponent<RandomSongPlayer>();

        if (player1 != null || player2 != null)
        {
            StartCoroutine(CrossfadeWithPlayers(player1, player2, fadeDuration));
        }
        else
        {
            StartCoroutine(CrossfadeCoroutine(audioSource1, audioSource2, fadeDuration));
        }
    }

    private IEnumerator CrossfadeWithPlayers(RandomSongPlayer fromPlayer, RandomSongPlayer toPlayer, float duration)
    {
        // Fade out the first player
        if (fromPlayer != null)
            fromPlayer.StartFade(0f, stopAfterFade: true);

        // Fade in the second player
        if (toPlayer != null)
        {
            if (!toPlayer.GetComponent<AudioSource>().isPlaying)
                toPlayer.GetComponent<AudioSource>().Play();
            toPlayer.StartFade(1f);
        }

        yield return null; // nothing else needed; players handle the fade themselves
    }

    private IEnumerator CrossfadeCoroutine(AudioSource from, AudioSource to, float duration)
    {
        float time = 0f;

        if (!to.isPlaying)
            to.Play();

        float fromStartVolume = from.volume;
        float toStartVolume = to.volume;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            from.volume = Mathf.Lerp(fromStartVolume, 0f, t);
            to.volume = Mathf.Lerp(toStartVolume, 1f, t);

            yield return null;
        }

        from.volume = 0f;
        to.volume = 1f;
        from.Stop();
    }
}