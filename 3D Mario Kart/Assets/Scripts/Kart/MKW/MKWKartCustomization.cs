using System.Collections.Generic;
using System.Linq;
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

    [Header("Randomization (On Spawn)")]
    public bool randomizeKart = false;
    public bool randomizeRacer = false;
    public bool randomizeKartSkin = false;

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

    [Header("Combined renderers for Star Power")]
    public List<Renderer> RacerRenderers = new();
    public List<Material> RacerMaterials = new();

    public UnityEvent<GameObject, GameObject> OnSpawned;

    private Vector3 visualPos;

    void Start()
    {
        RemoveVisual();
        Spawn();
    }

    public void RemoveVisual()
    {
        if (KartVisual != null)
        {
            visualPos = KartVisual.transform.position;
            Destroy(KartVisual);
        }
    }

    public void Spawn(bool triggerOnSpawn = true)
    {
        if(randomizeKart)
            currentKartIndex = Random.Range(0, karts.Count);

        if (randomizeRacer)
            currentRacerIndex = Random.Range(0, racers.Count);

        Kart = SpawnKart();
        Racer = SpawnRacer();
        gameObject.name = GetRacerName();
        CurrentKartConfig.UpdateEmbelem(CurrentRacerConfig);
        GetAllStarPowerRenderers();
        
        if(triggerOnSpawn)
            OnSpawned.Invoke(Kart, Racer);

    }

    public void Refresh()
    {
        if (Kart != null)
            Destroy(Kart);

        if (Racer != null)
            Destroy(Racer);

        Kart = null;
        Racer = null;
        CurrentKartConfig = null;
        CurrentRacerConfig = null;

        Spawn();
    }

    public GameObject SpawnKart()
    {
        currentKartIndex = Mathf.Clamp(currentKartIndex, 0, karts.Count - 1);

        GameObject kartToSpawn = karts[currentKartIndex];
        GameObject kart = Instantiate(kartToSpawn, kartSpawnPoint.position, kartSpawnPoint.rotation, transform);
        kart.transform.SetAsFirstSibling(); // Kart should be the very first sibling

        CurrentKartConfig = kart.GetComponent<KartConfig>();

        kart.transform.localPosition = visualPos;

        if (randomizeKartSkin)
            currentKartSkinIndex = Random.Range(0, CurrentKartConfig.Chassis.Skins.Count);

        UpdateKartSkin(currentKartSkinIndex);
        return kart;
    }

    public void UpdateKartSkin(int index)
    {
        if (CurrentKartConfig.Chassis.skinnable)
        {
            CurrentKartConfig.Chassis.CurrentSkin = index;
            CurrentKartConfig.Chassis.ApplySkin();
        }
    }

    public GameObject SpawnRacer()
    {
        currentRacerIndex = Mathf.Clamp(currentRacerIndex, 0, racers.Count - 1);

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


    public void GetAllStarPowerRenderers()
    {
        RacerRenderers.Clear();
        if (CurrentKartConfig?.KartRenderers != null)
            RacerRenderers.AddRange(CurrentKartConfig.KartRenderers);
        if (CurrentRacerConfig?.CharacterRenderers != null)
            RacerRenderers.AddRange(CurrentRacerConfig.CharacterRenderers);

        RacerMaterials.Clear();
        foreach (var renderer in RacerRenderers)
        {
            if (renderer == null) continue;
            RacerMaterials.AddRange(renderer.sharedMaterials);
        }

        Debug.Log($"Cached {RacerMaterials.Count} materials from {RacerRenderers.Count} renderers.");
    }

    public string GetRacerName()
    {
        string racerName = "Unknown Racer";
        if (Racer != null)
            racerName = Racer.name;

        if (transform.CompareTag("Player"))
            racerName += " (Player)";

        return racerName;
    }

}
