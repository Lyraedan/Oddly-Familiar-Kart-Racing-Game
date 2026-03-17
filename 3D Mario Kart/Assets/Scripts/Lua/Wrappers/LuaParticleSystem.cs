using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class LuaParticleSystem : LuaComponent
{
    private ParticleSystem ps;

    protected override void Awake()
    {
        base.Awake();
        ps = GetComponent<ParticleSystem>();
    }

    public void Play()
    {
        ps.Play();
    }

    public void Stop()
    {
        ps.Stop();
    }

    public void StopClear()
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void Pause()
    {
        ps.Pause();
    }

    public void Clear()
    {
        ps.Clear();
    }

    public bool IsPlaying()
    {
        return ps.isPlaying;
    }

    public bool IsAlive()
    {
        return ps.IsAlive();
    }

    public void Emit(int count)
    {
        ps.Emit(count);
    }

    public void SetTime(float time)
    {
        ps.time = time;
    }

    public float GetTime()
    {
        return ps.time;
    }

    public void Simulate(float time, bool withChildren = true, bool restart = true)
    {
        ps.Simulate(time, withChildren, restart);
    }

    public void SetLooping(bool value)
    {
        var main = ps.main;
        main.loop = value;
    }

    public bool GetLooping()
    {
        return ps.main.loop;
    }

    public void SetDuration(float duration)
    {
        var main = ps.main;
        main.duration = duration;
    }

    public float GetDuration()
    {
        return ps.main.duration;
    }

    public void SetStartLifetime(float lifetime)
    {
        var main = ps.main;
        main.startLifetime = lifetime;
    }

    public float GetStartLifetime()
    {
        return ps.main.startLifetime.constant;
    }

    public void SetStartSpeed(float speed)
    {
        var main = ps.main;
        main.startSpeed = speed;
    }

    public float GetStartSpeed()
    {
        return ps.main.startSpeed.constant;
    }

    public void SetStartSize(float size)
    {
        var main = ps.main;
        main.startSize = size;
    }

    public float GetStartSize()
    {
        return ps.main.startSize.constant;
    }

    public void SetEmissionRate(float rate)
    {
        var emission = ps.emission;
        emission.rateOverTime = rate;
    }

    public float GetEmissionRate()
    {
        return ps.emission.rateOverTime.constant;
    }

    public void SetEmissionEnabled(bool enabled)
    {
        var emission = ps.emission;
        emission.enabled = enabled;
    }

    public bool GetEmissionEnabled()
    {
        return ps.emission.enabled;
    }

    public int GetParticleCount()
    {
        return ps.particleCount;
    }
}