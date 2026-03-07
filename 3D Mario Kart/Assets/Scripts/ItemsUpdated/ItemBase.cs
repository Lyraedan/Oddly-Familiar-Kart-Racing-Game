using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    protected Player player;
    protected ItemManager itemManager;

    public NetworkObject networkedObject;

    [Header("The scale of the item when spawned in the world")]
    public Vector3 spawnScale = Vector3.one;
    public bool isHeld = false;
    public Transform holdPoint;

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

    [Rpc(SendTo.Server)]
    public void UpdateHoldPoint(Transform point)
    {
        holdPoint = point;
    }

    public abstract void Use(bool forward, GameObject user);

    public void MoveToHoldPoint()
    {
        transform.position = holdPoint.position;
    }

    public void Update()
    {
        if (isHeld)
        {
            MoveToHoldPoint();
        }
    }

    /// <summary>
    /// Releases the item from the player, unparents it, and allows it to exist independently in the world.
    /// </summary>
    public void Release()
    {
        isHeld = false;
        holdPoint = null;
        networkedObject.TryRemoveParent(true);
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
