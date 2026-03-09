using System.Collections.Generic;
using UnityEngine;

public class TransformVisualizer : MonoBehaviour
{
    public List<Transform> transforms = new();
    public Color color = Color.white;
    public float radius = 0.1f;

    public void OnDrawGizmos()
    {
        Gizmos.color = color;
        foreach (Transform t in transforms)
        {
            Gizmos.DrawSphere(t.position, radius);
        }
    }

    [ContextMenu("Fetch Transforms")]
    public void FetchTransforms()
    {
        transforms.Clear();
        foreach (Transform t in transform)
        {
            transforms.Add(t);
        }
    }
}
