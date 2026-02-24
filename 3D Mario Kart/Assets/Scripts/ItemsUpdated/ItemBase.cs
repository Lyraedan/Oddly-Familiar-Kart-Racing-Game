using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    protected Player player;
    protected ItemManager itemManager;

    [Header("The scale of the item when spawned in the world")]
    public Vector3 spawnScale = Vector3.one;

    [Header("Spawns")]
    protected Transform forwardSpawn;  // Infront of kart
    protected Transform backSpawn;     // Behind kart
    protected Transform handSpawn;     // In characters hand
    protected Transform throwSpawn;    // Thrown forward

    public virtual void Initialize(Player p, ItemManager manager)
    {
        player = p;
        itemManager = manager;
    }

    public void SetFrontSpawn(Transform forward)
    {
        forwardSpawn = forward;
    }

    public void SetBackSpawn(Transform back)
    {
        backSpawn = back;
    }

    public void SetHandSpawn(Transform hand)
    {
        handSpawn = hand;
    }

    public void SetThrowSpawn(Transform throwPos)
    {
        throwSpawn = throwPos;
    }

    public abstract void Use(bool forward);
}
