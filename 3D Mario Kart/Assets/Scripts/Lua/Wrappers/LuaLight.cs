using UnityEngine;

[RequireComponent(typeof(Light))]
public class LuaLight : LuaComponent
{
    private Light lightComp;

    protected override void Awake()
    {
        base.Awake();
        lightComp = GetComponent<Light>();
    }

    public override string GetLuaTable()
    {
        return "lights";
    }

    public void SetEnabled(bool value)
    {
        lightComp.enabled = value;
    }

    public bool IsEnabled()
    {
        return lightComp.enabled;
    }

    public void SetIntensity(float intensity)
    {
        lightComp.intensity = intensity;
    }

    public float GetIntensity()
    {
        return lightComp.intensity;
    }

    public void SetRange(float range)
    {
        lightComp.range = range;
    }

    public float GetRange()
    {
        return lightComp.range;
    }

    public void SetColor(float r, float g, float b)
    {
        lightComp.color = new Color(r, g, b);
    }

    public Color GetColor()
    {
        return lightComp.color;
    }
}