using UnityEngine;
using UnityEngine.Splines;

public class LuaPathTool : LuaComponent
{
    public PathTool pathTool;

    private float progress;

    protected override void Awake()
    {
        base.Awake();

        if (pathTool == null)
            pathTool = GetComponent<PathTool>();

        if (pathTool == null)
        {
            Debug.LogError("LuaPathTool requires a PathTool reference.");
        }
    }

    private Spline GetSpline()
    {
        if (pathTool == null || pathTool.splineContainer == null)
            return null;

        return pathTool.splineContainer.Spline;
    }

    public void SetProgress(float value)
    {
        progress = Mathf.Clamp01(value);
    }

    public float GetProgress()
    {
        return progress;
    }

    // Position along spline (0-1)
    public Vector3 GetPosition(float t)
    {
        var spline = GetSpline();
        if (spline == null) return Vector3.zero;

        return pathTool.transform.TransformPoint(spline.EvaluatePosition(t));
    }

    public Vector3 GetDirection(float t)
    {
        var spline = GetSpline();
        if (spline == null) return Vector3.forward;

        return pathTool.transform.TransformDirection(
            spline.EvaluateTangent(t)
        ).normalized;
    }

    public void MoveActor(float t)
    {
        actor.transform.position = GetPosition(t);
    }

    public void MoveActorAligned(float t)
    {
        Vector3 pos = GetPosition(t);
        Vector3 dir = GetDirection(t);

        actor.transform.position = pos;

        if (dir != Vector3.zero)
            actor.transform.rotation = Quaternion.LookRotation(dir).eulerAngles;
    }

    public float GetNearestT(Vector3 worldPos)
    {
        var spline = GetSpline();
        if (spline == null) return 0f;

        float nearestT = 0f;
        float minDist = float.MaxValue;

        int samples = 100;

        for (int i = 0; i <= samples; i++)
        {
            float sampleT = i / (float)samples;
            Vector3 p = GetPosition(sampleT);

            float d = Vector3.Distance(worldPos, p);

            if (d < minDist)
            {
                minDist = d;
                nearestT = sampleT;
            }
        }

        return nearestT;
    }

    public float GetLength()
    {
        if (pathTool == null || pathTool.splineContainer == null)
            return 0f;

        return pathTool.splineContainer.CalculateLength();
    }

    public Vector3 GetPointAhead(float t, float offset)
    {
        t += offset;

        if (t > 1f)
            t -= 1f;

        return GetPosition(t);
    }

    public int GetChildCount()
    {
        if (pathTool == null || pathTool.pathRoot == null)
            return 0;

        return pathTool.pathRoot.childCount;
    }

    public void FollowPath(float speed, float deltaTime)
    {
        progress += deltaTime * speed;

        if (progress > 1f)
            progress -= 1f;

        MoveActorAligned(progress);
    }
}