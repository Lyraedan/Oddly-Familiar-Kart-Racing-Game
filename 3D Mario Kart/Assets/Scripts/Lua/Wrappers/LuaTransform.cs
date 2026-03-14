using UnityEngine;

public class LuaTransform : LuaComponent
{
    private Transform t;

    protected override void Awake()
    {
        base.Awake();
        t = transform;
    }

    public override string GetLuaTable()
    {
        return "transforms";
    }

    public void SetPosition(float x, float y, float z)
    {
        t.position = new Vector3(x, y, z);
    }

    public Vector3 GetPosition()
    {
        return t.position;
    }

    public void Translate(float x, float y, float z)
    {
        t.Translate(new Vector3(x, y, z));
    }

    public void SetRotation(float x, float y, float z)
    {
        t.rotation = Quaternion.Euler(x, y, z);
    }

    public Vector3 GetRotation()
    {
        return t.eulerAngles;
    }

    public void Rotate(float x, float y, float z)
    {
        t.Rotate(new Vector3(x, y, z));
    }

    public void SetScale(float x, float y, float z)
    {
        t.localScale = new Vector3(x, y, z);
    }

    public Vector3 GetScale()
    {
        return t.localScale;
    }

    public void MoveForward(float amount)
    {
        t.Translate(Vector3.forward * amount);
    }

    public void MoveRight(float amount)
    {
        t.Translate(Vector3.right * amount);
    }

    public void MoveUp(float amount)
    {
        t.Translate(Vector3.up * amount);
    }

    public void SetParent(GameObject parent)
    {
        if (parent != null)
            t.SetParent(parent.transform);
    }
}