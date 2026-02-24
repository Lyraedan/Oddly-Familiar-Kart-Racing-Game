using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ChassisConfig : MonoBehaviour
{
    public Transform AxelContainer;
    public List<TireTest> Axels;
    public List<Transform> TireArms;

    [ContextMenu("Get Axels")]
    public void GetAxels()
    {
        Axels = new List<TireTest>(GetComponentsInChildren<TireTest>());
    }
}
