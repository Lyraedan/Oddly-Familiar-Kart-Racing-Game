using UnityEngine;

public class LuaGameObject
{
    private GameObject obj;

    public LuaGameObject(GameObject gameObject)
    {
        obj = gameObject;
    }

    public string GetName()
    {
        return obj.name;
    }

    public void SetActive(bool value)
    {
        obj.SetActive(value);
    }

    public bool IsActive()
    {
        return obj.activeSelf;
    }

    public void Destroy()
    {
        Object.Destroy(obj);
    }

    public LuaTransform GetTransform()
    {
        return obj.GetComponent<LuaTransform>();
    }
}