using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LapCounter : MonoBehaviour
{
    public int LAPCOUNT = 1;
    public Transform checkpoints; //parent
    [HideInInspector]
    public bool[] checkpointsVisited; //visited or not array
    [HideInInspector]
    public int currentCheckpointVal = 0;
    [HideInInspector]
    public int totalCheckpointVal = 0;
    [HideInInspector]
    public float distanceToNextCheckpoint;
    public int Position = 0;

    public int endPosition = 0;

    public PathTool pathTool;
    public int lastCheckpointID = -1;
    public int highestCheckpointID = -1;    // farthest checkpoint reached moving forward

    private RACE_MANAGER rm;
    // Start is called before the first frame update
    void Start()
    {
        if(pathTool != null)
        {
            checkpoints = pathTool.pathRoot;
            checkpoints.GetChild(0).gameObject.tag = "NextLapCollider"; // Mark the first collider as the next lap collider
        }

        checkpointsVisited = new bool[checkpoints.childCount];
        for(int i = 0; i < checkpointsVisited.Length; i++)
        {
            checkpointsVisited[i] = false;
        }
        rm = GameObject.Find("RaceManager").GetComponent<RACE_MANAGER>();

        if(checkpoints == null)
        {
            checkpoints = GameObject.FindGameObjectWithTag("LapCheckpointsContainer").transform;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (currentCheckpointVal >= checkpoints.childCount)
        {
            currentCheckpointVal = 0;
        }

        calculateDist();


        if (gameObject.tag == "Player" && LAPCOUNT <= rm.MAXLAPS)
        {
            IngameUIHolder.Instance.lapCounterUI.UpdateText(LAPCOUNT + "/" + rm.MAXLAPS);

        }
        if (gameObject.tag == "Player" && LAPCOUNT > rm.MAXLAPS && endPosition == 0)
        {
            RACE_MANAGER.RACE_COMPLETED = true;
            endPosition = Position;
            GetComponent<Player>().stopDrift();
            StartCoroutine(stopDriftRot());
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NextLapCollider") && UpdatedCheckAllPoints())
        {
            LAPCOUNT++;
            for (int i = 0; i < checkpointsVisited.Length; i++)
                checkpointsVisited[i] = false;

            if (gameObject.tag != "Player")
            {
                int max = RACE_MANAGER.allPaths.childCount;
                int rand = Random.Range(0, max);
                GetComponent<ComputerDriver>().path = RACE_MANAGER.allPaths.GetChild(rand); // new path
            }
        }
        else
        {
            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            if (checkpoint != null)
            {
                UpdateCheckpointProgress(checkpoint);
            }
        }
    }

    bool checkAllPoints()
    {
        for(int i = 0; i < checkpointsVisited.Length; i++)
        {
            if(checkpointsVisited[i] == false)
            {
                return false;
            }
        }

        return true;
    }

    bool UpdatedCheckAllPoints()
    {
        // Lap is complete only when highestCheckpointID has reached the last checkpoint
        return highestCheckpointID >= checkpoints.childCount - 1;
    }

    IEnumerator stopDriftRot()
    {
 
            for (int i = 0; i < 120; i++)
            {
                yield return new WaitForSeconds(0.01f);
                transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, Quaternion.Euler(0, 0, 0), 8f * Time.deltaTime);
            }
    }

    void calculateDist()
    {
        Vector3 trackDirection;

        if (currentCheckpointVal-1 >= 0)
        {
            trackDirection = checkpoints.GetChild(currentCheckpointVal).position - checkpoints.GetChild(currentCheckpointVal - 1).position;
        }
        else
        {
            trackDirection = checkpoints.GetChild(currentCheckpointVal).position - checkpoints.GetChild(checkpoints.childCount - 1).position;
        }

        Vector3 playerToGoal = transform.position - checkpoints.GetChild(currentCheckpointVal).transform.position;
        Vector3 projectedPlayerToGoal = Vector3.Project(playerToGoal, trackDirection);
        distanceToNextCheckpoint = projectedPlayerToGoal.magnitude;


        //distanceToNextCheckpoint = Vector3.Distance(transform.position, checkpoints.GetChild(currentCheckpointVal).position);

    }

    public void UpdateCheckpointProgress(Checkpoint checkpoint)
    {
        int id = checkpoint.checkpointID;

        // First checkpoint hit
        if (lastCheckpointID == -1)
        {
            lastCheckpointID = id;
            highestCheckpointID = id;
            checkpointsVisited[id] = true;
            currentCheckpointVal = id + 1;
            totalCheckpointVal = id + 1;
            return;
        }

        // Moving forward (including skipping checkpoints)
        if (IsForward(id, lastCheckpointID))
        {
            checkpointsVisited[id] = true;
            totalCheckpointVal++;
            currentCheckpointVal = id + 1;

            // Update highest checkpoint only if it’s further along the track
            if (id > highestCheckpointID)
                highestCheckpointID = id;
        }
        // Moving backward, ignore for progress (you can mark visited false if needed)
        else if (IsBackward(id, lastCheckpointID))
        {
            checkpointsVisited[lastCheckpointID] = false;
            totalCheckpointVal--;
            currentCheckpointVal = id;
        }

        lastCheckpointID = id;
    }

    /// <summary>
    /// Determines if `nextID` is forward relative to `currentID`, considering wrap-around
    /// </summary>
    private bool IsForward(int nextID, int currentID)
    {
        int count = checkpoints.childCount;
        return (nextID - currentID + count) % count > 0;
    }

    /// <summary>
    /// Determines if `nextID` is backward relative to `currentID`, considering wrap-around
    /// </summary>
    private bool IsBackward(int nextID, int currentID)
    {
        int count = checkpoints.childCount;
        return (currentID - nextID + count) % count > 0;
    }

    void OldTriggereEnter(Collider other)
    {
        if (other.transform.tag == "NextLapCollider")
        {
            if (checkAllPoints())
            {
                LAPCOUNT++;
                for (int i = 0; i < checkpointsVisited.Length; i++)
                {
                    checkpointsVisited[i] = false;
                }
                if (gameObject.tag != "Player")
                {
                    int max = RACE_MANAGER.allPaths.childCount;

                    int rand = Random.Range(0, max);

                    GetComponent<ComputerDriver>().path = RACE_MANAGER.allPaths.GetChild(rand); //assigning a new path
                }

            }
        }
        else if (currentCheckpointVal < checkpoints.childCount && other.transform == checkpoints.GetChild(currentCheckpointVal))
        {
            checkpointsVisited[currentCheckpointVal] = true;
            currentCheckpointVal++;
            totalCheckpointVal++;


        }
    }
}
