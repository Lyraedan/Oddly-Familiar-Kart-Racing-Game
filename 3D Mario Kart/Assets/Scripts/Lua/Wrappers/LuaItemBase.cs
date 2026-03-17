using UnityEngine;

[RequireComponent(typeof(LuaBehaviour))]
public class LuaItemBase : ItemBase
{
    private LuaBehaviour luaBehaviour;

    private void Awake()
    {
        luaBehaviour = GetComponent<LuaBehaviour>();
    }

    public override void Initialize(Player p, ItemManager manager)
    {
        base.Initialize(p, manager);

        if (luaBehaviour != null)
        {
            luaBehaviour.CallLua("Initialize", this, p, manager);
        }
    }

    public override void Use(bool forward, GameObject user)
    {
        if (!ReadyForUse)
            return;

        if (luaBehaviour == null)
            return;

        luaBehaviour.CallLua("Use", forward, user);
    }
}