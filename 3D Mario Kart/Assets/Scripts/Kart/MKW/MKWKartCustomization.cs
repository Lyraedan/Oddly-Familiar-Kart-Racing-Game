using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

public class MKWKartCustomization : MonoBehaviour
{

    [Header("Selection")]
    public KartType kartType;
    public int currentKartIndex = 0;
    public int currentRacerIndex = 0;
    public int currentKartSkinIndex = 0;

    [Header("Setup")]
    public Transform kartSpawnPoint;

    [Header("Karts")]
    public List<GameObject> karts = new();

    [Header("Characters")]
    public List<GameObject> racers = new();

    [Header("Debug")]
    public GameObject KartVisual;

    private GameObject Kart;
    private GameObject Racer;

    public KartConfig CurrentKartConfig { get; private set; }
    public RacerConfig CurrentRacerConfig { get; private set; }

    public UnityEvent<GameObject, GameObject> OnSpawned;

    void Start()
    {
        RemoveVisual();
        Spawn();
    }

    public void RemoveVisual()
    {
        if (KartVisual != null)
        {
            Destroy(KartVisual);
        }
    }

    public void Spawn()
    {
        Kart = SpawnKart();
        Racer = SpawnRacer();
        OnSpawned.Invoke(Kart, Racer);
    }

    public GameObject SpawnKart()
    {
        GameObject kartToSpawn = karts[currentKartIndex];
        GameObject kart = Instantiate(kartToSpawn, kartSpawnPoint.position, kartSpawnPoint.rotation, transform);
        kart.transform.SetAsFirstSibling(); // Kart should be the very first sibling
        CurrentKartConfig = kart.GetComponent<KartConfig>();
        UpdateKartSkin(currentKartSkinIndex);
        return kart;
    }

    public void UpdateKartSkin(int index)
    {
        if (CurrentKartConfig.Chassis.skinnable)
        {
            CurrentKartConfig.Chassis.CurrentSkin = currentKartSkinIndex;
            CurrentKartConfig.Chassis.ApplySkin();
        }
    }

    public GameObject SpawnRacer()
    {
        GameObject racerToSpawn = racers[currentRacerIndex];
        Transform spawnPoint = CurrentKartConfig.Chassis.CharacterContainer;
        GameObject racer = Instantiate(racerToSpawn, spawnPoint);
        // Racer MUST be zeroed in
        racer.transform.localPosition = Vector3.zero;
        racer.transform.localRotation = Quaternion.identity;
        racer.transform.localScale = Vector3.one;

        CurrentRacerConfig = racer.GetComponent<RacerConfig>();
        return racer;
    }

}
