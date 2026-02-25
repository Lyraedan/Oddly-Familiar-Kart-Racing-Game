using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class BoxColliderVisualizer : MonoBehaviour
{
    public enum VisualizationMode
    {
        Singular,
        List
    }

    [Header("Mode")]
    public VisualizationMode mode = VisualizationMode.Singular;

    [Header("Singular")]
    public BoxCollider singleCollider;

    [Header("List")]
    public List<BoxCollider> collidersToVisualize = new();

    [Header("Auto Populate (List Mode Only)")]
    public bool autoFindChildren = false;
    public bool includeInactive = true;

    [Header("Visual")]
    public Color color = new Color(0f, 1f, 0f, 0.5f);

#if UNITY_EDITOR

    private void OnValidate()
    {
        TryAutoPopulate();
    }

    private void OnTransformChildrenChanged()
    {
        TryAutoPopulate();
    }

#endif

    [ContextMenu("Find Colliders In Child Objects")]
    public void PopulateColliders()
    {
        collidersToVisualize.Clear();
        collidersToVisualize.AddRange(
            GetComponentsInChildren<BoxCollider>(includeInactive)
        );
    }

    private void TryAutoPopulate()
    {
        if (mode == VisualizationMode.List && autoFindChildren)
        {
            PopulateColliders();
        }
    }

    private void OnDrawGizmos()
    {
        switch (mode)
        {
            case VisualizationMode.Singular:
                DrawCollider(singleCollider);
                break;

            case VisualizationMode.List:
                if (collidersToVisualize == null) return;

                foreach (var col in collidersToVisualize)
                {
                    DrawCollider(col);
                }
                break;
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    private void DrawCollider(BoxCollider col)
    {
        if (col == null) return;

        Gizmos.matrix = col.transform.localToWorldMatrix;

        // Transparent fill
        Gizmos.color = color;
        Gizmos.DrawCube(col.center, col.size);

        // Solid outline
        Gizmos.color = new Color(color.r, color.g, color.b, 1f);
        Gizmos.DrawWireCube(col.center, col.size);
    }
}