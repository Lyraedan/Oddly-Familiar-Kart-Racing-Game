using UnityEngine;
using UnityEngine.Splines;

public class LuaPathTool : LuaComponent
{
    public PathTool pathTool;

    public bool resetProgressOnFinish = true;
    public bool startAtActorPosition = true;

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

        if (startAtActorPosition)
            SetProgressFromActorPosition();
    }

    public void SetProgressFromActorPosition()
    {
        progress = GetNearestT(transform.position);
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
        actor.transform.position = new LuaVector3(GetPosition(t));
    }

    public void MoveActorAligned(float t)
    {
        Vector3 pos = GetPosition(t);
        Vector3 dir = GetDirection(t);

        actor.transform.position = new LuaVector3(pos);

        if (dir != Vector3.zero)
            actor.transform.rotation = new LuaVector3(Quaternion.LookRotation(dir).eulerAngles);
    }

    /// <summary>
    /// Find the nearest point on the spline to the given world position and return its t value (0-1).
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
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

    public void FollowPath(float speed, float deltaTime, bool aligned, bool reversed)
    {
        if (!reversed)
        {
            progress += deltaTime * speed;
            progress = Mathf.Clamp01(progress);

            if (progress >= 1f)
            {
                luaBehaviour.CallLua($"{luaName}_OnPathFinished");
                if (resetProgressOnFinish)
                    progress -= 1f;
            }
        }
        else
        {
            progress -= deltaTime * speed;
            progress = Mathf.Clamp01(progress);
            if (progress <= 0f)
            {
                luaBehaviour.CallLua($"{luaName}_OnPathFinished");
                if (resetProgressOnFinish)
                    progress += 1f;
            }
        }

        if (!aligned)
            MoveActor(progress);
        else
            MoveActorAligned(progress);
    }
}