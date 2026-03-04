using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RacerSpawn : MonoBehaviour
{
    public List<Transform> SpawnPoints = new();

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
