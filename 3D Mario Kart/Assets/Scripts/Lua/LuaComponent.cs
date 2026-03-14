using UnityEngine;

public abstract class LuaComponent : MonoBehaviour
{
    [Tooltip("Name used in Lua")]
    public string luaName = "component";

    protected LuaBehaviour luaBehaviour;

    protected virtual void Awake()
    {
        luaBehaviour = GetComponentInParent<LuaBehaviour>();

        if (luaBehaviour == null)
        {
            Debug.LogError($"LuaComponent '{luaName}' requires a LuaBehaviour on the same GameObject or a parent GameObject.");
        }
    }

    public virtual string GetLuaTable()
    {
        return "components";
    }
}
