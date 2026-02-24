using System.Collections.Generic;
using UnityEngine;

public class KartCustomization : MonoBehaviour
{
    // Bikes should have the same number of axels as a normal kart, just have 2 tire mesh renderers disabled

    [System.Serializable]
    public struct AxelConfig
    {
        public LayerMask mask;
        public Transform WheelToLookAt;
        public float hitDistance;
    }
    [Header("This must match the number of axels in the kart chassis")]
    public List<AxelConfig> WheelsToLookAt = new();

    [Header("Kart Bodies")]
    public int CurrentlySelectedKartBodyIndex = 0;
    public Transform bodyTransform;
    public List<GameObject> kartBodies;

    [Header("Debugging")]
    public GameObject ChassisVisual;

    public ChassisConfig CurrentChassis { get; private set; }

    public void Start()
    {
        RemoveVisuals();
        SpawnKartBody();
    }

    void RemoveVisuals()
    {
        if (ChassisVisual != null)
        {
            Destroy(ChassisVisual);
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
    }
}