using UnityEngine;

public class LuaTransform
{
    private Transform t;

    public LuaTransform(Transform transform)
    {
        t = transform;
    }

    public void SetPosition(float x, float y, float z)
    {
        t.position = new Vector3(x, y, z);
    }

    public float GetPositionX() => t.position.x;
    public float GetPositionY() => t.position.y;
    public float GetPositionZ() => t.position.z;

    public void Translate(float x, float y, float z)
    {
        t.Translate(x, y, z);
    }

    public void SetRotation(float x, float y, float z)
    {
        t.rotation = Quaternion.Euler(x, y, z);
    }

    public float GetRotationX() => t.eulerAngles.x;
    public float GetRotationY() => t.eulerAngles.y;
    public float GetRotationZ() => t.eulerAngles.z;

    public void Rotate(float x, float y, float z)
    {
        t.Rotate(x, y, z);
    }

    public void SetScale(float x, float y, float z)
    {
        t.localScale = new Vector3(x, y, z);
    }

    public float GetScaleX() => t.localScale.x;
    public float GetScaleY() => t.localScale.y;
    public float GetScaleZ() => t.localScale.z;

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

    public void SetParent(LuaGameObject parent)
    {
        if (parent == null) return;

        var go = parent.GetInternal();
        if (go != null)
            t.SetParent(go.transform);
    }

    public LuaGameObject GetParent()
    {
        if (t.parent == null)
            return null;

        return new LuaGameObject(t.parent.gameObject);
    }
}