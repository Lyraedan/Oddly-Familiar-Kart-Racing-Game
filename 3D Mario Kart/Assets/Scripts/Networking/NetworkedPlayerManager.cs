using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkedPlayerManager : NetworkBehaviour
{
    public Player player; // Reference to the Player script on this GameObject
    public bool isLocal = true; // Use to detect our local player

    private void Awake()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Transform spawnPoint = GameObject.Find("PlayerSpawnPoint").transform;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        isLocal = IsOwner; // Set isLocal based on ownership of the network object
        if(!isLocal)
        {
            player.enabled = false; // Disable player script for non-local players to prevent input processing
        } 
        else
        {
            var cam = FindFirstObjectByType<Camerafollow>();
            cam.player = transform; // Set to us
        }

        FindAllAnimatorsAndNetwork();
    }

    public void FindAllAnimatorsAndNetwork()
    {
        // Find all Animator components in this GameObject and its children
        Animator[] animators = GetComponentsInChildren<Animator>();

        // For each animator, add a NetworkAnimator component if it doesn't already have one
        foreach (Animator animator in animators)
        {
            if (animator.GetComponent<NetworkAnimator>() == null)
            {
                NetworkAnimator networkAnimator = animator.gameObject.AddComponent<NetworkAnimator>();
                networkAnimator.Animator = animator; // Set the Animator reference
            }
        }
    }
}
