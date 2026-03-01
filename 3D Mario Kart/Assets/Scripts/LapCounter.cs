using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.LowLevel;
using UnityEngine.Splines;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LapCounter : MonoBehaviour
{
    public int LAPCOUNT = 1;

    public Transform checkpoints; // Parent containing all checkpoint colliders

    [HideInInspector] public bool[] checkpointsVisited;
    [HideInInspector] public int currentCheckpointVal = 0; // Next checkpoint expected
    [HideInInspector] public float distanceToNextCheckpoint;

    public int Position = 0;
    public int endPosition = 0;

    public PathTool pathTool;
    public bool usePathToolPath = false;

    private int lastCheckpointID = -1;

    private RACE_MANAGER rm;

    [Header("Debug Info (Read-Only in Inspector)")]
    [SerializeField] private int debugProgressIndex;
    [SerializeField] private int debugRaceProgressScore;

    public UnityAction<int> onPositionChanged;
    public UnityAction<int> onLapCompleted;
    public UnityAction<int> onCheckpointReached;
    public UnityAction<int> onLastLap;

    public int ProgressIndex
    {
        get { return Mathf.Max(0, lastCheckpointID); }
    }

    public int RaceProgressScore
    {
        get
        {
            int checkpointCount = checkpoints.childCount;
            return (LAPCOUNT * checkpointCount) + ProgressIndex;
        }
    }

    void Start()
    {
        if (pathTool != null && usePathToolPath)
            checkpoints = pathTool.pathRoot;

        if (checkpoints == null)
            checkpoints = GameObject.FindGameObjectWithTag("LapCheckpointsContainer").transform;

        checkpointsVisited = new bool[checkpoints.childCount];

        for (int i = 0; i < checkpointsVisited.Length; i++)
            checkpointsVisited[i] = false;

        rm = GameObject.Find("RaceManager").GetComponent<RACE_MANAGER>();
    }

    void Update()
    {
        debugProgressIndex = ProgressIndex;
        debugRaceProgressScore = RaceProgressScore;

        if (currentCheckpointVal >= checkpoints.childCount)
            currentCheckpointVal = 0;

        CalculateDistanceToNextCheckpoint();

        var allRacers = IngameUIHolder.Instance.LapCounters;
        // Update position every frame (or you can throttle for performance)
        if (allRacers != null && allRacers.Count > 0)
        {
            UpdatePositions(allRacers);
        }

        // Update UI for player
        if (gameObject.CompareTag("Player") && LAPCOUNT <= rm.MAXLAPS)
        {
            IngameUIHolder.Instance.lapCounterUI.UpdateText(LAPCOUNT + "/" + rm.MAXLAPS);
        }

        if (gameObject.CompareTag("Player") && LAPCOUNT > rm.MAXLAPS && endPosition == 0)
        {
            RACE_MANAGER.RACE_COMPLETED = true;
            endPosition = Position;

            GetComponent<Player>().stopDrift();
            StartCoroutine(StopDriftRotation());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Checkpoint checkpoint = other.GetComponent<Checkpoint>();
        if (checkpoint == null)
            return;

        int id = checkpoint.checkpointID;

        // FIRST CHECKPOINT EVER MUST BE 0
        if (lastCheckpointID == -1)
        {
            if (id == 0)
            {
                RegisterCheckpoint(id);
            }
            return;
        }

        int expectedNext = (lastCheckpointID + 1) % checkpoints.childCount;

        if (id == expectedNext)
        {
            RegisterCheckpoint(id);
        }
    }

    void PlayerLog(string log)
    {
        if(tag == "Player")
        {
            Debug.Log(log);
        }
    }

    void RegisterCheckpoint(int id)
    {
        bool lapCompleted = id == 0 && lastCheckpointID >= checkpoints.childCount - 1;

        lastCheckpointID = id;
        checkpointsVisited[id] = true;
        currentCheckpointVal = (id + 1) % checkpoints.childCount;
        onCheckpointReached?.Invoke(id);

        // LAP COMPLETION (crossing checkpoint 0 after last checkpoint)
        if (lapCompleted)
        {
            CompleteLap();
        }
    }

    void CompleteLap()
    {
        LAPCOUNT++;

        onLapCompleted?.Invoke(LAPCOUNT);

        if (LAPCOUNT == rm.MAXLAPS)
        {
            onLastLap?.Invoke(LAPCOUNT);
        }


        // Reset for next lap
        for (int i = 0; i < checkpointsVisited.Length; i++)
            checkpointsVisited[i] = false;

        lastCheckpointID = 0;
        checkpointsVisited[0] = true;
        currentCheckpointVal = 1;

        if (!gameObject.CompareTag("Player"))
        {
            int max = RACE_MANAGER.allPaths.childCount;
            int rand = Random.Range(0, max);
            GetComponent<ComputerDriver>().path = RACE_MANAGER.allPaths.GetChild(rand);
        }
    }

    void CalculateDistanceToNextCheckpoint()
    {
        if (checkpoints.childCount == 0)
            return;

        int prevIndex = lastCheckpointID < 0
            ? checkpoints.childCount - 1
            : lastCheckpointID;

        Vector3 from = checkpoints.GetChild(prevIndex).position;
        Vector3 to = checkpoints.GetChild(currentCheckpointVal).position;

        Vector3 trackDirection = to - from;

        Vector3 playerToGoal = transform.position - to;
        Vector3 projected = Vector3.Project(playerToGoal, trackDirection);

        distanceToNextCheckpoint = projected.magnitude;
    }

    public static void UpdatePositions(List<LapCounter> racers)
    {
        // Sort racers by race progress score, then distance to next checkpoint
        racers.Sort((p1, p2) =>
        {
            if (p1.RaceProgressScore != p2.RaceProgressScore)
                return p2.RaceProgressScore.CompareTo(p1.RaceProgressScore); // higher score first
            return p1.distanceToNextCheckpoint.CompareTo(p2.distanceToNextCheckpoint); // closer to next checkpoint first
        });

        // Assign positions
        for (int i = 0; i < racers.Count; i++)
        {
            int oldPos = racers[i].Position;
            racers[i].Position = i + 1; // 1 = first place
            
            if (racers[i].onPositionChanged != null && oldPos != racers[i].Position)
            {
                racers[i].onPositionChanged.Invoke(racers[i].Position);
            }
        }
    }

    IEnumerator StopDriftRotation()
    {
        for (int i = 0; i < 120; i++)
        {
            yield return new WaitForSeconds(0.01f);

            Transform model = transform.GetChild(0);
            model.localRotation = Quaternion.Lerp(
                model.localRotation,
                Quaternion.Euler(0, 0, 0),
                8f * Time.deltaTime
            );
        }
    }

    public bool TryGetLastCheckpointSplinePose(PathTool pathTool,
    out Vector3 position,
    out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        if (lastCheckpointID < 0)
            return false;

        if (pathTool == null || pathTool.splineContainer == null)
            return false;

        var spline = pathTool.splineContainer.Spline;
        if (spline == null || spline.Count < 2)
            return false;

        // Get checkpoint world position
        Vector3 checkpointWorldPos =
            checkpoints.GetChild(lastCheckpointID).position;

        // Convert to spline local space
        Vector3 localPoint =
            pathTool.transform.InverseTransformPoint(checkpointWorldPos);

        // Find nearest point on spline
        float t;

        SplineUtility.GetNearestPoint(
            spline,
            localPoint,
            out float3 nearestPoint,
            out t
        );

        // Evaluate spline at that t
        Vector3 splineLocalPos = spline.EvaluatePosition(t);

        // Convert back to world
        Vector3 splineWorldPos =
            pathTool.transform.TransformPoint(splineLocalPos);

        // Get forward direction
        Vector3 tangent = spline.EvaluateTangent(t);
        Vector3 worldTangent =
            pathTool.transform.TransformDirection(tangent);

        position = splineWorldPos;
        rotation = Quaternion.LookRotation(worldTangent);

        return true;
    }

    public bool IsGoingWrongWay(Transform vehicleTransform, float checkDistance = 5f)
    {
        if (checkpoints == null || checkpoints.childCount < 2)
            return false; // nothing to check against

        // Get the next expected checkpoint
        Transform nextCheckpoint = checkpoints.GetChild(currentCheckpointVal);

        // Direction from current position to next checkpoint
        Vector3 toNext = (nextCheckpoint.position - vehicleTransform.position).normalized;

        // Vehicle forward direction (local z axis)
        Vector3 forward = vehicleTransform.forward;

        // Check dot product: forward dot toNext
        float dot = Vector3.Dot(forward, toNext);

        if (dot < 0)
        {
            // vehicle is facing away from next checkpoint
            Debug.Log("Wrong Way");
            return true;
        }

        // Optional: check if already passed checkpoint but not updated yet
        if (Vector3.Distance(vehicleTransform.position, nextCheckpoint.position) > checkDistance)
        {
            // too far behind
            Debug.Log("Wrong Way (behind)");
            return true;
        }

        return false;
    }

}