using System.Collections.Generic;
using UnityEngine;

public class TransformVisualizer : MonoBehaviour
{
    public List<Transform> transforms = new();
    public Color color = Color.white;
    public float radius = 0.1f;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;

        foreach (Transform t in transforms)
        {
            if (t != null)
                Gizmos.DrawSphere(t.position, radius);
        }
    }

    private void OnTransformChildrenChanged()
    {
        FetchTransforms();
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        FetchTransforms();
    }
#endif
}