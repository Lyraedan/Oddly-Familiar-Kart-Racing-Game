using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ChassisConfig : MonoBehaviour
{
    public Transform CharacterContainer;
    public Transform AxelContainer;
    public List<TireTest> Axels;
    public List<Transform> TireArms;

    [Header("Skinning")]
    public bool skinnable = false;
    public int CurrentSkin = 0;

    [System.Serializable]
    public struct KartSkinPart
    {
        public MeshRenderer renderer;
        public Material skinnedMaterial;
    }

    [System.Serializable]
    public struct KartSkin
    {
        public string skinName;
        public List<KartSkinPart> parts;
    }

    public List<KartSkin> Skins = new();

    // Override the materials of the mesh renderers on the kart body to match the racer
    public List<MeshRenderer> emblems = new();

    public void ApplySkin()
    {
        if (!skinnable)
            return;

        CurrentSkin = Mathf.Clamp(CurrentSkin, 0, Skins.Count - 1);

        KartSkin skin = Skins[CurrentSkin];
        foreach(KartSkinPart part in skin.parts)
        {
            part.renderer.material = part.skinnedMaterial;
        }
    }

    [ContextMenu("Get Axels")]
    public void GetAxels()
    {
        Axels = new List<TireTest>(GetComponentsInChildren<TireTest>());
    }

    // Used mainly to test skins in the editor
    [ContextMenu("Test: Apply Skin")]
    public void ContextMenuApplySkin()
    {
        ApplySkin();
    }
}
