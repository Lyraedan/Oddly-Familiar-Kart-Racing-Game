using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LuaAnimator : LuaComponent
{
    private Animator anim;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
    }

    public override string GetLuaTable()
    {
        return "animators";
    }

    public void Play(string state)
    {
        anim.Play(state);
    }

    public void PlayLayer(string state, int layer)
    {
        anim.Play(state, layer);
    }

    public void CrossFade(string state, float transition)
    {
        anim.CrossFade(state, transition);
    }

    public void CrossFadeLayer(string state, float transition, int layer)
    {
        anim.CrossFade(state, transition, layer);
    }

    public void SetTrigger(string name)
    {
        anim.SetTrigger(name);
    }

    public void ResetTrigger(string name)
    {
        anim.ResetTrigger(name);
    }

    public void SetBool(string name, bool value)
    {
        anim.SetBool(name, value);
    }

    public bool GetBool(string name)
    {
        return anim.GetBool(name);
    }

    public void SetFloat(string name, float value)
    {
        anim.SetFloat(name, value);
    }

    public float GetFloat(string name)
    {
        return anim.GetFloat(name);
    }

    public void SetInt(string name, int value)
    {
        anim.SetInteger(name, value);
    }

    public int GetInt(string name)
    {
        return anim.GetInteger(name);
    }

    public void SetSpeed(float speed)
    {
        anim.speed = speed;
    }

    public float GetSpeed()
    {
        return anim.speed;
    }
    public void SetLayerWeight(int layer, float weight)
    {
        anim.SetLayerWeight(layer, weight);
    }

    public float GetLayerWeight(int layer)
    {
        return anim.GetLayerWeight(layer);
    }

    public bool IsPlaying(string state)
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        return info.IsName(state);
    }

    public float GetNormalizedTime()
    {
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public float GetStateLength()
    {
        return anim.GetCurrentAnimatorStateInfo(0).length;
    }

    public void Rebind()
    {
        anim.Rebind();
    }

    public void UpdateAnimator(float deltaTime)
    {
        anim.Update(deltaTime);
    }

    public bool HasState(int layer, string state)
    {
        return anim.HasState(layer, Animator.StringToHash(state));
    }
}