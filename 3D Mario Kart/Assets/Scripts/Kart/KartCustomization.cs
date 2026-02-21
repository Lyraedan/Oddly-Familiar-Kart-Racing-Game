using System.Collections.Generic;
using UnityEngine;

public class KartCustomization : MonoBehaviour
{
    // Used purely for different colored variants of parts
    [System.Serializable]
    public class KartSkin
    {
        public string skinName;
        public List<Material> materials = new();
    }

    [System.Serializable]
    public class MultiMeshPart
    {
        public bool skinnable = true;
        public List<KartSkin> skins = new();
        public string partName;
        public List<Mesh> meshes = new();
    }

    [System.Serializable]
    public class RenderableMesh
    {
        public MeshFilter filter;
        public MeshRenderer renderer;

        public void SetMesh(Mesh mesh)
        {
            if (filter != null)
                filter.mesh = mesh;
        }

        public void SetVisible(bool visible)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }

        public void SetMaterials(List<Material> materials)
        {
            if (renderer != null && materials != null && materials.Count > 0)
            {
                renderer.materials = materials.ToArray();
            }
        }
    }

    [System.Serializable]
    public struct SkinSelection
    {
        public int bodySkinIndex;
        public int wheelSkinIndex;
        public int gliderSkinIndex;
    }

    [Header("Selection")]
    public int currentBodyIndex = 0;
    public int currentWheelIndex = 0;
    public int currentGliderIndex = 0;
    [Header("Skin Selection")]
    public SkinSelection currentSkin;

    [Header("Selectable Options")]
    public List<MultiMeshPart> bodyMeshes = new();
    public List<MultiMeshPart> wheelMeshes = new();
    public List<MultiMeshPart> gliderMeshes = new();

    [Header("Kart References")]
    public RenderableMesh mainChassis;
    public RenderableMesh[] wheels;
    public RenderableMesh glider;

    void Start()
    {
        ApplyCustomization();
    }

    public void ApplyCustomization()
    {
        ApplyBody();
        //ApplyWheels(); // Wheels are WIP
        //ApplyGlider(); // Disabled because I gotta change how the glider works (its skinned mesh setup wont cut it)
    }

    void ApplyBody()
    {
        if (bodyMeshes.Count == 0 || mainChassis == null)
            return;

        var part = bodyMeshes[Mathf.Clamp(currentBodyIndex, 0, bodyMeshes.Count - 1)];

        if (part.meshes.Count > 0)
        {
            mainChassis.SetMesh(part.meshes[0]);
            mainChassis.SetVisible(true);

            ApplySkin(part, mainChassis, currentSkin.bodySkinIndex);
        }
        else
        {
            mainChassis.SetVisible(false);
        }
    }

    void ApplyWheels()
    {
        if (wheelMeshes.Count == 0 || wheels == null)
            return;

        var part = wheelMeshes[Mathf.Clamp(currentWheelIndex, 0, wheelMeshes.Count - 1)];

        for (int i = 0; i < wheels.Length; i++)
        {
            if (i < part.meshes.Count && part.meshes[i] != null)
            {
                wheels[i].SetMesh(part.meshes[i]);
                wheels[i].SetVisible(true);

                ApplySkin(part, wheels[i], currentSkin.wheelSkinIndex);
            }
            else
            {
                wheels[i].SetVisible(false);
            }
        }
    }

    void ApplyGlider()
    {
        if (gliderMeshes.Count == 0 || glider == null)
            return;

        var part = gliderMeshes[Mathf.Clamp(currentGliderIndex, 0, gliderMeshes.Count - 1)];

        if (part.meshes.Count > 0)
        {
            glider.SetMesh(part.meshes[0]);
            glider.SetVisible(true);

            ApplySkin(part, glider, currentSkin.gliderSkinIndex);
        }
        else
        {
            glider.SetVisible(false);
        }
    }

    void ApplySkin(MultiMeshPart part, RenderableMesh renderable, int skinIndex)
    {
        if (!part.skinnable)
            return;

        if (part.skins == null || part.skins.Count == 0)
            return;

        int clampedIndex = Mathf.Clamp(skinIndex, 0, part.skins.Count - 1);

        var skin = part.skins[clampedIndex];

        renderable.SetMaterials(skin.materials);
    }
}