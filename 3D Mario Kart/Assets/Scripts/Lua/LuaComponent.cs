using UnityEngine;

public abstract class LuaComponent : MonoBehaviour
{
    [Tooltip("Name used in Lua")]
    public string luaName = "component";

    protected LuaBehaviour luaBehaviour;

    private LuaGameObject _actor;

    public LuaGameObject actor
    {
        get
        {
            if (_actor == null)
                _actor = new LuaGameObject(gameObject);
            return _actor;
        }
    }

    protected virtual void Awake()
    {
        luaBehaviour = GetComponentInParent<LuaBehaviour>();

        if (luaBehaviour == null)
        {
            Debug.LogError($"LuaComponent '{luaName}' requires a LuaBehaviour on the same GameObject or a parent GameObject.");
        }
    }
}
