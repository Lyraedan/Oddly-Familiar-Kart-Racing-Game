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

    public string tag
    {
        get => obj.tag;
        set => obj.tag = value;
    }

    public string name
    {
        get => obj.name;
        set => obj.name = value;
    }

    public string layer
    {
        get => LayerMask.LayerToName(obj.layer);
        set
        {
            int layerIndex = LayerMask.NameToLayer(value);
            if (layerIndex != -1)
                obj.layer = layerIndex;
            else
                Debug.LogError($"Layer '{value}' does not exist.");
        }
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

    public void Destroy(float delay)
    {
        Object.Destroy(obj, delay);
    }

    public bool CompareTag(string tag)
    {
        return obj.CompareTag(tag);
    }

    public LuaGameObject Find(string name)
    {
        Transform child = obj.transform.Find(name);

        if (child == null)
            return null;

        return new LuaGameObject(child.gameObject);
    }

    public LuaGameObject GetParent()
    {
        if (obj.transform.parent == null)
            return null;

        return new LuaGameObject(obj.transform.parent.gameObject);
    }

    public void SetParent(LuaGameObject parent)
    {
        if (parent == null)
            obj.transform.SetParent(null);
        else
            obj.transform.SetParent(parent.GetInternal().transform);
    }

    public bool IsNull()
    {
        return obj == null;
    }
}