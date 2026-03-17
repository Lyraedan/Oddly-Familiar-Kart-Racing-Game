using UnityEngine;

public class LuaTransform : LuaAutoRegister
{
    private Transform t;

    public LuaTransform(Transform transform)
    {
        t = transform;
    }

    public LuaGameObject parent
    {
        get => GetParent();
        set => SetParent(value);
    }

    public LuaVector3 position
    {
        get => new LuaVector3(GetPositionX(), GetPositionY(), GetPositionZ());
        set => SetPosition(value.x, value.y, value.z);
    }

    public LuaVector3 rotation
    {
        get => new LuaVector3(GetRotationX(), GetRotationY(), GetRotationZ());
        set => SetRotation(value.x, value.y, value.z);
    }

    public LuaVector3 scale
    {
        get => new LuaVector3(GetScaleX(), GetScaleY(), GetScaleZ());
        set => SetScale(value.x, value.y, value.z);
    }

    public LuaVector3 up => new LuaVector3(t.up);

    public LuaVector3 right => new LuaVector3(t.right);

    public LuaVector3 forward => new LuaVector3(t.forward);

    public void LookAt(LuaVector3 lookAt) => t.LookAt(lookAt.ToUnityVector());
    public void LookAt(float x, float y, float z) => t.LookAt(new Vector3(x, y, z));

    public void MoveTowards(LuaVector3 target, float maxDistanceDelta) => t.position = Vector3.MoveTowards(t.position, target.ToUnityVector(), maxDistanceDelta);
    public void MoveTowards(float x, float y, float z, float maxDistanceDelta) => t.position = Vector3.MoveTowards(t.position, new Vector3(x, y, z), maxDistanceDelta);

    public void Translate(float x, float y, float z) => t.Translate(x, y, z);
    public void MoveForward(float amount) => t.Translate(Vector3.forward * amount);
    public void MoveRight(float amount) => t.Translate(Vector3.right * amount);
    public void MoveUp(float amount) => t.Translate(Vector3.up * amount);

    public void SetParent(LuaGameObject parent)
    {
        if (parent == null)
        {
            t.SetParent(null);
            return;
        }

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

    public void SetPosition(LuaVector3 position) => t.position = position.ToUnityVector();
    public void SetPosition(float x, float y, float z) => t.position = new Vector3(x, y, z);
    public float GetPositionX() => t.position.x;
    public float GetPositionY() => t.position.y;
    public float GetPositionZ() => t.position.z;

    public void SetRotation(LuaVector3 eulars) => t.rotation = Quaternion.Euler(eulars.ToUnityVector());
    public void SetRotation(float x, float y, float z) => t.rotation = Quaternion.Euler(x, y, z);
    public float GetRotationX() => t.eulerAngles.x;
    public float GetRotationY() => t.eulerAngles.y;
    public float GetRotationZ() => t.eulerAngles.z;

    public void Rotate(float x, float y, float z) => t.Rotate(x, y, z);
    public void Rotate(LuaVector3 eulars) => t.Rotate(eulars.ToUnityVector());

    public void SetScale(LuaVector3 scale) => t.localScale = scale.ToUnityVector();
    public void SetScale(float x, float y, float z) => t.localScale = new Vector3(x, y, z);
    public float GetScaleX() => t.localScale.x;
    public float GetScaleY() => t.localScale.y;
    public float GetScaleZ() => t.localScale.z;
}