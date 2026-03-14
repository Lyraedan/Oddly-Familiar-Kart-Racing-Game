using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LuaAudioSource : LuaComponent
{
    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
    }

    public void Play()
    {
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    public void Pause()
    {
        audioSource.Pause();
    }

    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public float GetVolume()
    {
        return audioSource.volume;
    }

    public void SetPitch(float pitch)
    {
        audioSource.pitch = pitch;
    }

    public float GetPitch()
    {
        return audioSource.pitch;
    }

    public void SetLoop(bool value)
    {
        audioSource.loop = value;
    }

    public bool GetLoop()
    {
        return audioSource.loop;
    }
}