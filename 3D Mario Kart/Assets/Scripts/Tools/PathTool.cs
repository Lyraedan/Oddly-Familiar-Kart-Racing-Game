using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using Random = UnityEngine.Random;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(SplineContainer))]
public class PathTool : MonoBehaviour
{
    [Header("Spline")]
    public SplineContainer splineContainer;

    [Header("Path Duplication")]
    public float positionVariance = 0.5f;
    public float rotationVariance = 5f; // degrees

    [Header("Collider Settings")]
    [Min(1)]
    public int samplesPerCurve = 10;
    public Vector3 colliderSize = new Vector3(1, 1, 1);
    public bool alignToSpline = true;
    public bool autoUpdate = true;

    [Header("Visualization")]
    public Color lineColor = Color.green;
    public Color colliderColor = Color.red;

    public Transform pathRoot;

    private void Reset()
    {
        splineContainer = GetComponent<SplineContainer>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && autoUpdate)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                    RebuildColliders();
            };
        }
    }

    [ContextMenu("Duplicate Path With Variance")]
    public void DuplicatePathWithVariance()
    {
        if (splineContainer == null || splineContainer.Spline == null)
        {
            Debug.LogWarning("No spline to duplicate.");
            return;
        }

        GameObject dupObj = new GameObject(name + "_Duplicate");
        dupObj.transform.SetParent(transform.parent);
        dupObj.transform.position = transform.position;
        dupObj.transform.rotation = transform.rotation;

        var dupTool = dupObj.AddComponent<PathTool>();

        var dupSplineContainer = dupObj.AddComponent<SplineContainer>();
        dupTool.splineContainer = dupSplineContainer;

        // Randomize colors for this duplicate
        dupTool.lineColor = Random.ColorHSV();
        dupTool.colliderColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f, 0.25f, 1f);

        var originalSpline = splineContainer.Spline;
        var newSpline = new Spline();

        // Loop over existing BezierKnots
        for (int i = 0; i < originalSpline.Count; i++)
        {
            var knot = originalSpline[i];

            // Apply random variance on position
            float3 variedPos = knot.Position + new float3(
                UnityEngine.Random.Range(-positionVariance, positionVariance),
                UnityEngine.Random.Range(-positionVariance, positionVariance),
                UnityEngine.Random.Range(-positionVariance, positionVariance)
            );

            // Apply small random variance to rotation
            quaternion variedRot = math.mul(
                knot.Rotation,
                quaternion.EulerXYZ(new float3(
                    0,
                    math.radians(UnityEngine.Random.Range(-rotationVariance, rotationVariance)),
                    0
                ))
            );

            // Create new variant knot
            BezierKnot newKnot = new BezierKnot
            {
                Position = variedPos,
                TangentIn = knot.TangentIn,
                TangentOut = knot.TangentOut,
                Rotation = variedRot,
            };

            newSpline.Add(newKnot);
        }

        dupSplineContainer.Spline = newSpline;
        dupTool.RebuildColliders();

        Debug.Log("Spline duplicated with variance!");
    }
#endif

    public void RebuildColliders()
    {
        if (splineContainer == null)
            splineContainer = GetComponent<SplineContainer>();

        if (splineContainer == null || splineContainer.Spline == null)
            return;

#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(gameObject, "Rebuild Path Colliders");
#endif

        ClearExisting();

        Transform root = GetOrCreateColliderRoot();

        var spline = splineContainer.Spline;
        int curveCount = spline.Count;

        if (curveCount < 2)
            return;

        int totalSamples = curveCount * samplesPerCurve;

        for (int i = 0; i <= totalSamples; i++)
        {
            float t = i / (float)totalSamples;

            Vector3 localPos = spline.EvaluatePosition(t);
            Vector3 worldPos = transform.TransformPoint(localPos);

            GameObject colObj = new GameObject($"PointCollider_{i}");
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(colObj, "Create Path Collider");
#endif
            colObj.transform.SetParent(root);
            colObj.transform.position = worldPos;

            if (alignToSpline)
            {
                Vector3 tangent = spline.EvaluateTangent(t);
                if (tangent != Vector3.zero)
                {
                    colObj.transform.rotation =
                        Quaternion.LookRotation(transform.TransformDirection(tangent));
                }
            }

            var box = colObj.AddComponent<BoxCollider>();
            box.size = colliderSize;
            box.isTrigger = true;
        }
    }

    private Transform GetOrCreateColliderRoot()
    {
        Transform root = pathRoot;

        if (root == null)
        {
            GameObject rootObj = new GameObject("PathRoot");
            rootObj.transform.SetParent(transform);
            rootObj.transform.localPosition = Vector3.zero;
            rootObj.transform.localRotation = Quaternion.identity;
            rootObj.transform.localScale = Vector3.one;
            root = rootObj.transform;
            pathRoot = root;
        }

        return root;
    }

    private void ClearExisting()
    {
        Transform root = pathRoot;
        if (root == null) return;

#if UNITY_EDITOR
        while (root.childCount > 0)
        {
            Undo.DestroyObjectImmediate(root.GetChild(0).gameObject);
        }
#else
        foreach (Transform child in root)
        {
            Destroy(child.gameObject);
        }
#endif
    }

    private void OnDrawGizmos()
    {
        if (splineContainer == null || splineContainer.Spline == null)
            return;

        var spline = splineContainer.Spline;

        Gizmos.color = lineColor;

        int resolution = spline.Count * samplesPerCurve;
        if (resolution < 2) return;

        Vector3 prev = transform.TransformPoint(spline.EvaluatePosition(0f));

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 current = transform.TransformPoint(spline.EvaluatePosition(t));
            Gizmos.DrawLine(prev, current);
            prev = current;
        }

        // Draw collider preview
        Gizmos.color = colliderColor;

        Transform root = pathRoot;
        if (root == null) return;

        foreach (Transform child in root)
        {
            BoxCollider box = child.GetComponent<BoxCollider>();
            if (box == null) continue;

            Gizmos.matrix = child.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, box.size);
        }
    }
}