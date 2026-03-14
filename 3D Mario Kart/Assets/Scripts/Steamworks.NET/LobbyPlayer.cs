using Unity.Netcode;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    // Persistent lobby player that holds player data across scenes
    public GameObject playerPrefab;

    public NetworkVariable<int> selectedKart = new NetworkVariable<int>(0);
    public NetworkVariable<int> selectedRacer = new NetworkVariable<int>(0);
    public NetworkVariable<int> selectedKartSkin = new NetworkVariable<int>(0);

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log("This is my LobbyPlayer");
        }
    }

    //void SpawnPlayerRacer()
    //{
    //    GameObject obj = Instantiate(playerPrefab);

    //    obj.GetComponent<NetworkObject>().SpawnAsPlayerObject(OwnerClientId);

    //    var racer = obj.GetComponent<MKWKartCustomization>();
    //    racer.currentKartIndex = selectedKart.Value;
    //    racer.currentRacerIndex = selectedRacer.Value;
    //    racer.currentKartSkinIndex = selectedKartSkin.Value;
    //    racer.Spawn();
    //}

    [Rpc(SendTo.Server)]
    public void SetKartServerRpc(int kart)
    {
        selectedKart.Value = kart;
    }

    [Rpc(SendTo.Server)]
    public void SetRacerServerRpc(int racer)
    {
        selectedRacer.Value = racer;
    }

    [Rpc(SendTo.Server)]
    public void SetSkinServerRpc(int skin)
    {
        selectedKartSkin.Value = skin;
    }
}
