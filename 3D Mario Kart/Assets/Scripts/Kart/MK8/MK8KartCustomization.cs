using System.Collections.Generic;
using UnityEngine;

public class MK8KartCustomization : MonoBehaviour
{
    // Bikes should have the same number of axels as a normal kart, just have 2 tire mesh renderers disabled

    public KartType type;

    public int CurrentlySelectedKartBodyIndex = 0;
    public int CurrentlySelectedKartWheelIndex = 0;

    [Header("This must match the number of axels in the kart chassis")]
    public List<AxelConfig> WheelsToLookAt = new();

    [Header("Kart Bodies")]
    public Transform bodyTransform;
    public List<GameObject> kartBodies;

    [Header("Kart Wheels - Bikes have their front and back right wheel renderers disabled!")]
    public Transform frontLeftWheelSpawn;
    public Transform frontRightWheelSpawn;
    public Transform backLeftWheelSpawn;
    public Transform backRightWheelSpawn;

    [System.Serializable]
    public class WheelConfig
    {
        public GameObject wheelPrefab;
        [Header("Wheel scales")]
        public Vector3 KartFrontLeftWheel = Vector3.one;
        public Vector3 KartFrontRightWheel = Vector3.one;
        public Vector3 KartBackLeftWheel = Vector3.one;
        public Vector3 KartBackRightWheel = Vector3.one;
        [Space(10)]
        public Vector3 BikeFrontWheel = Vector3.one;
        public Vector3 BikeBackWheel = Vector3.one;
    }
    public List<WheelConfig> kartWheels;

    [Header("Debugging")]
    public GameObject ChassisVisual;
    public List<GameObject> WheelVisuals = new();

    public ChassisConfig CurrentChassis { get; private set; }

    private List<GameObject> KartParts = new();

    public void Start()
    {
        RemoveVisuals();
        SpawnKartBody();
        SpawnKartWheels();

        if (KartType.Bike == type)
        {
            frontRightWheelSpawn.gameObject.SetActive(false);
            backRightWheelSpawn.gameObject.SetActive(false);
        }
    }

    void RemoveVisuals()
    {
        if (ChassisVisual != null)
        {
            Destroy(ChassisVisual);
        }

        if(WheelVisuals.Count > 0)
        {
            foreach (GameObject wheelVisual in WheelVisuals)
            {
                Destroy(wheelVisual);
            }
        }
    }

    public void DestroyKart()
    {
        foreach (GameObject part in KartParts)
        {
            Destroy(part);
        }
        KartParts.Clear();
    }

    public void RefreshKart()
    {
        DestroyKart();
        frontRightWheelSpawn.gameObject.SetActive(true);
        backRightWheelSpawn.gameObject.SetActive(true);
        SpawnKartBody();
        SpawnKartWheels();
        if (KartType.Bike == type)
        {
            frontRightWheelSpawn.gameObject.SetActive(false);
            backRightWheelSpawn.gameObject.SetActive(false);
        }
    }

    public void SpawnKartBody()
    {
        if (kartBodies.Count == 0)
        {
            Debug.LogError("No kart bodies assigned to KartCustomization!");
            return;
        }

        GameObject selectedKartBody = kartBodies[CurrentlySelectedKartBodyIndex];
        GameObject newKartBody = Instantiate(selectedKartBody, bodyTransform);
        ChassisConfig chassisConfig = newKartBody.GetComponent<ChassisConfig>();
        if (chassisConfig == null)
        {
            Debug.LogError("No ChassisConfig found on KartCustomization object!");
            Destroy(newKartBody);
            return;
        }

        if(WheelsToLookAt.Count != chassisConfig.Axels.Count)
        {
            Debug.LogError("Number of axels in ChassisConfig does not match number of WheelsToLookAt in KartCustomization!");
            Destroy(newKartBody);
            return;
        }

        // Apply the wheel look at and mask settings to the chassis config
        for (int i = 0; i < WheelsToLookAt.Count; i++)
        {
            chassisConfig.Axels[i].mask = WheelsToLookAt[i].mask;
            chassisConfig.Axels[i].wheelLookAt = WheelsToLookAt[i].WheelToLookAt;
            chassisConfig.Axels[i].hitDistance = WheelsToLookAt[i].hitDistance;
        }

        newKartBody.transform.localPosition = Vector3.zero;
        newKartBody.transform.SetAsLastSibling();
        CurrentChassis = chassisConfig;

        KartParts.Add(newKartBody);
    }

    public void SpawnKartWheels()
    {
        if (kartWheels.Count == 0)
        {
            Debug.LogError("No kart wheels assigned to KartCustomization!");
            return;
        }

        WheelConfig selectedWheelConfig = kartWheels[CurrentlySelectedKartWheelIndex];

        switch (type)
        {
            case KartType.Kart:
                SpawnWheelAt(frontLeftWheelSpawn, selectedWheelConfig.KartFrontLeftWheel);
                SpawnWheelAt(frontRightWheelSpawn, selectedWheelConfig.KartFrontRightWheel);
                SpawnWheelAt(backLeftWheelSpawn, selectedWheelConfig.KartBackLeftWheel);
                SpawnWheelAt(backRightWheelSpawn, selectedWheelConfig.KartBackRightWheel);
                break;

            case KartType.Bike:
                SpawnWheelAt(frontLeftWheelSpawn, selectedWheelConfig.BikeFrontWheel);
                SpawnWheelAt(frontRightWheelSpawn, Vector3.zero);
                SpawnWheelAt(backLeftWheelSpawn, selectedWheelConfig.BikeBackWheel);
                SpawnWheelAt(backRightWheelSpawn, Vector3.zero);
                break;
        }
    }

    public void SpawnWheelAt(Transform spawnPoint, Vector3 scale)
    {
        if (kartWheels.Count == 0)
        {
            Debug.LogError("No kart wheels assigned to KartCustomization!");
            return;
        }
        GameObject wheelPrefab = kartWheels[CurrentlySelectedKartWheelIndex].wheelPrefab;
        GameObject newWheel = Instantiate(wheelPrefab, spawnPoint);
        // I think we only need to set the local scale
        newWheel.transform.localPosition = Vector3.zero;
        newWheel.transform.localRotation = Quaternion.identity;
        newWheel.transform.localScale = scale;

        KartParts.Add(newWheel);
    }
}