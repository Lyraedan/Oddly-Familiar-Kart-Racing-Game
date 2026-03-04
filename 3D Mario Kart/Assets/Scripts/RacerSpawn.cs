using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class RacerSpawn : MonoBehaviour
{

    public static RacerSpawn Instance;
    public List<Transform> SpawnPoints = new();

    private Dictionary<int, bool> Occupied = new();
    private List<GameObject> racers = new(); // A list of computer racers

    public GameObject ComputerRacerPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        FreeAllSpaces();
    }

    public Transform AssignSpace()
    {
        int spaceIndex = GetNextAvailableSpace();
        if(spaceIndex == -1)
        {
            // Default to the last space
            return SpawnPoints[SpawnPoints.Count - 1];
        }

        Occupied[spaceIndex] = true;
        return SpawnPoints[spaceIndex];
    }

    public Transform AssignSpace(int index)
    {
        if (index < 0 || index >= SpawnPoints.Count)
        {
            Debug.LogError($"Invalid spawn index: {index}. Defaulting to last spawn point.");
            index = SpawnPoints.Count - 1;
        }
        Occupied[index] = true;
        return SpawnPoints[index];
    }

    public void FreeSpace(int i) 
    {
        if (Occupied.ContainsKey(i))
        {
            Occupied[i] = false;
        } 
        else
        {
            Occupied.Add(i, false);
        }
    }

    public void FreeAllSpaces()
    {
        for (int i = 0; i < SpawnPoints.Count; i++)
        {
            FreeSpace(i);
        }
    }

    public int GetNextAvailableSpace()
    {
        for (int i = 0; i < SpawnPoints.Count; i++)
        {
            if (!Occupied.ContainsKey(i) || !Occupied[i])
            {
                Occupied[i] = true;
                return i;
            }
        }
        Debug.LogError("No available spawn spaces!");
        return -1; // No available spaces
    }

    /// <summary>
    /// Host only
    /// </summary>
    public void SpawnComputerRacers()
    {
        // Only the host can spawn racers
        if (!NetworkManager.Singleton.IsHost)
            return;

        for (int i = 0; i < Occupied.Count; i++)
        {
            if (!Occupied[i])
            {
                GameObject computerRacer = Instantiate(ComputerRacerPrefab);
                Transform spawnPoint = AssignSpace();
                computerRacer.transform.position = spawnPoint.position;
                computerRacer.transform.rotation = spawnPoint.rotation;

                NetworkObject networkedObject = computerRacer.GetComponent<NetworkObject>();
                if (networkedObject != null)
                {
                    networkedObject.Spawn();
                }
            }
        }
    }

    public void RemoveComputerRacer() // Remove the racer behind the player
    {
        // Only the host can remove racers
        if (!NetworkManager.Singleton.IsHost)
            return;
        if (racers.Count > 0)
        {
            GameObject racer = racers[0];
            if (racer != null)
            {
                NetworkObject networkedObject = racer.GetComponent<NetworkObject>();
                if (networkedObject != null && networkedObject.IsSpawned)
                {
                    networkedObject.Despawn();
                }
                Destroy(racer);
            }
            racers.RemoveAt(0);
        }
    }

    public void RemoveAllComputerRacer()
    {
        // Only the host can remove racers
        if (!NetworkManager.Singleton.IsHost)
            return;

        foreach (var racer in racers)
        {
            if (racer != null)
            {
                NetworkObject networkedObject = racer.GetComponent<NetworkObject>();
                if (networkedObject != null && networkedObject.IsSpawned)
                {
                    networkedObject.Despawn();
                }
                Destroy(racer);
            }
        }
        racers.Clear();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnDrawGizmos()
    {
        if (SpawnPoints == null)
            return;

        Gizmos.color = Color.blue;

        foreach (var point in SpawnPoints)
        {
            if (point == null)
                continue;

            Gizmos.DrawSphere(point.position, 0.5f);
        }
    }

    [ContextMenu("Collect Child Transforms")]
    private void CollectChildTransforms()
    {
        SpawnPoints.Clear();

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child == transform) // Skip root object
                continue;

            SpawnPoints.Add(child);
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
