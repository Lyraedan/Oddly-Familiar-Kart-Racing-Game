using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomSongPlayer : MonoBehaviour
{
    [Header("Song Settings")]
    public List<AudioClip> songs = new();
    public bool loop = false;
    public bool PlayOnAwake = true;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float volume = 1f;
    public float fadeDuration = 1.5f;

    private AudioSource audioSource;
    private Coroutine fadeRoutine;

    private float fadeMultiplier = 0f;
    private bool isStopped = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = loop;

        ChooseRandomSong();

        if (PlayOnAwake)
        {
            Play();
        }
    }

    void Update()
    {
        audioSource.volume = volume * fadeMultiplier;

        if (!loop && !audioSource.isPlaying && !isStopped)
        {
            PlayRandomSong();
        }
    }
    void ChooseRandomSong()
    {
        if (songs == null || songs.Count == 0)
        {
            Debug.LogWarning("No songs assigned!");
            return;
        }

        int randomIndex = Random.Range(0, songs.Count);
        audioSource.clip = songs[randomIndex];
    }

    void PlayRandomSong()
    {
        ChooseRandomSong();
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
            ChooseRandomSong();
        }

        isStopped = false;
        audioSource.Play();
        StartFade(1f);
    }

    public void StartFade(float target, bool stopAfterFade = false)
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