using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkedPlayerManager : NetworkBehaviour
{
    public Player player; // Reference to the Player script on this GameObject
    public bool isLocal = true; // Use to detect our local player

    public NetworkVariable<int> KartId = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> KartSkinId = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> RacerId = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    private void Awake()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        int spawnIndex = ((int)OwnerClientId) % RaceManager.Instance.RacerSpawns.SpawnPoints.Count;
        Transform spawnPoint = RaceManager.Instance.RacerSpawns.SpawnPoints[spawnIndex].transform;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        isLocal = IsLocalPlayer;
        MKWKartCustomization customization = GetComponent<MKWKartCustomization>();

        if (!isLocal)
        {
            player.enabled = false;
            gameObject.name += " (OtherPlayer " + OwnerClientId + ")";
            gameObject.tag = "OtherPlayer";
            //LoadPlayersCharacter(customization);
        }
        else
        {
            RaceManager.Instance.RegisterLocalPlayer(player);
            KartId.Value = customization.currentKartIndex;
            KartSkinId.Value = customization.currentKartSkinIndex;
            RacerId.Value = customization.currentRacerIndex;
            gameObject.name += " (LocalPlayer " + OwnerClientId + ")";
            gameObject.tag = "Player";
            // This racer should already be spawned in so we do NOT need to load their customization
            //LoadPlayersCharacter(customization);
        }

        FindAllAnimatorsAndNetwork();

        RaceManager.Instance.RegisterPlayer(player);
        IngameUIHolder.Instance.WaitingForPlayersCount.text = $"{RaceManager.Instance.AllPlayers.Count} players";
        Debug.Log("Spawned player with client ID: " + OwnerClientId + " GameObject: " + gameObject.name);
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

    public void LoadPlayersCharacter(MKWKartCustomization customization)
    {
        if(customization == null)
        {
            Debug.LogError("MKWKartCustomization component not found on player GameObject. Cannot load character.");
            return;
        }

        Debug.Log("Loading player customization!");
        customization.currentKartIndex = KartId.Value;
        customization.currentKartSkinIndex = KartSkinId.Value;
        customization.currentRacerIndex = RacerId.Value;
        customization.Refresh();

    }
}
