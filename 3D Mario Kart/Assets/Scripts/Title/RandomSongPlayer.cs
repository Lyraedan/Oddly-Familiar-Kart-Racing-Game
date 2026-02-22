using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomSongPlayer : MonoBehaviour
{
    public List<AudioClip> songs = new();
    public bool loop = false;

    [Range(0f, 1f)] public float volume = 1f;
    public float fadeDuration = 1.5f;

    private AudioSource audioSource;
    private Coroutine fadeRoutine;

    private float fadeMultiplier = 0f; // 0 = silent, 1 = full volume
    private bool isStopped = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = loop;

        PlayRandomSong();
    }

    void Update()
    {
        // Always apply final volume
        audioSource.volume = volume * fadeMultiplier;

        if (!loop && !audioSource.isPlaying && !isStopped)
        {
            PlayRandomSong();
        }
    }

    void PlayRandomSong()
    {
        if (songs == null || songs.Count == 0)
        {
            Debug.LogWarning("No songs assigned!");
            return;
        }

        int randomIndex = Random.Range(0, songs.Count);
        audioSource.clip = songs[randomIndex];
        audioSource.Play();

        StartFade(1f);
    }

    public void StopWithFade()
    {
        isStopped = true;
        StartFade(0f, stopAfterFade: true);
    }

    public void Play()
    {
        if (audioSource.clip == null)
        {
            PlayRandomSong();
            return;
        }

        isStopped = false;
        audioSource.Play();
        StartFade(1f);
    }

    void StartFade(float target, bool stopAfterFade = false)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(target, stopAfterFade));
    }

    IEnumerator FadeRoutine(float target, bool stopAfterFade)
    {
        float start = fadeMultiplier;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeMultiplier = Mathf.Lerp(start, target, time / fadeDuration);
            yield return null;
        }

        fadeMultiplier = target;

        if (stopAfterFade)
            audioSource.Stop();
    }
}