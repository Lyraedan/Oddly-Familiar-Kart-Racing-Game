using System.Collections;
using System.Collections.Generic;
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

    [HideInInspector] public bool ReadyForUse = false; // Set to true when the item is ready to be used (e.g., after a delay or animation)
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

    public abstract void Use(bool forward, GameObject user);

    /// <summary>
    /// Reparent the item and zero in its local position
    /// </summary>
    /// <param name="newParent"></param>
    public void ReparentAndZero(Transform newParent)
    {
        transform.SetParent(newParent);
        transform.localPosition = Vector3.zero;
    }
    public void PlayPlayerAnim(bool forward)
    {
        if (player == null || player.Driver == null)
            return;

        if (forward)
            player.Driver.SetTrigger("ThrowForward");
        else
            player.Driver.SetTrigger("ThrowBackward");
    }

    public void StartUseDelay(float delay)
    {
        ReadyForUse = false;
        itemManager.StartCoroutine(FlagAfterDelay(delay));
    }

    IEnumerator FlagAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        ReadyForUse = true;
    }

}
