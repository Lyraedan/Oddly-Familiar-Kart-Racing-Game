using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomSongPlayer : MonoBehaviour
{
    public List<AudioClip> songs = new();
    public bool loop = false; // If true, loop current song instead of picking new one
    [UnityEngine.Range(0, 1)] public float volume = 1f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = loop;

        PlayRandomSong();
    }

    void Update()
    {
        audioSource.volume = volume;
        // Only pick a new random song if looping is OFF
        if (!loop && !audioSource.isPlaying)
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
    }
}