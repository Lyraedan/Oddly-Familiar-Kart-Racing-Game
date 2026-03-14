using UnityEngine;

public class LuaGameObject
{
    private GameObject obj;
    private LuaTransform luaTransform;

    public LuaGameObject(GameObject gameObject)
    {
        obj = gameObject;
    }

    public GameObject GetInternal()
    {
        return obj;
    }

    public LuaTransform transform
    {
        get
        {
            if (luaTransform == null)
                luaTransform = new LuaTransform(obj.transform);

            return luaTransform;
        }
    }

    public void SetName(string name)
    {
        obj.name = name;
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
}