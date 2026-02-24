using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    [HideInInspector]
    public bool FellInWater = false;
    [HideInInspector]
    public bool outOfBounds = false;

    [HideInInspector] 
    public bool PlayerBeingMoved = false; //for camera

    public WaypointTracker tracker;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (FellInWater)
        {
            GetComponent<Rigidbody>().AddRelativeForce(Vector3.down * 5000 * Time.deltaTime, ForceMode.Acceleration);
        }
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Water")
        {
            if(gameObject.tag == "Player")
            {
                GetComponent<Player>().currentspeed = 0;
                FellInWater = true;
                PlayerBeingMoved = true;


                yield return new WaitForSeconds(0.5f);

                GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Rigidbody>().velocity = Vector3.zero;

                Transform currPoint = GetComponent<LapCounter>().checkpoints.GetChild(GetComponent<LapCounter>().currentCheckpointVal);

                

                transform.position = currPoint.position;
                transform.rotation = currPoint.rotation;


                //get item path checkpoint next
                float dist = Mathf.Infinity;
                int closestWaypoint = 0;

                for (int i = 0; i < tracker.ActivePath.childCount; i++)
                {
                    float d = Vector3.Distance(tracker.ActivePath.GetChild(i).position, transform.position);
                    if (d < dist)
                    {
                        dist = d;
                        closestWaypoint = i;
                    }
                }

                // Set the waypoint in the tracker
                tracker.SetCurrentWaypoint(closestWaypoint + 1);


                PlayerBeingMoved = false;

                yield return new WaitForSeconds(0.5f);
                GetComponent<Rigidbody>().isKinematic = false;
            }

            if(gameObject.tag == "Opponent")
            {
                GetComponent<ComputerDriver>().current_speed = 0;
                FellInWater = true;
                PlayerBeingMoved = true;
                yield return new WaitForSeconds(1);
                GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Rigidbody>().velocity = Vector3.zero;

                Transform currPoint = GetComponent<ComputerDriver>().path.GetChild(GetComponent<ComputerDriver>().current_node);
                GetComponent<LapCounter>().currentCheckpointVal++;

                //get item path checkpoint next
                float dist = 99999;
                int checkPointVal = 0;
                for (int i = 0; i < GetComponent<OpponentItemManager>().path.childCount; i++)
                {
                    if (Vector3.Distance(GetComponent<OpponentItemManager>().path.GetChild(i).position, transform.position) < dist)
                    {
                        dist = Vector3.Distance(GetComponent<OpponentItemManager>().path.GetChild(i).position, transform.position);
                        checkPointVal = i;
                    }
                }
                GetComponent<OpponentItemManager>().currentWayPoint = checkPointVal + 1;


                transform.position = currPoint.position;
                transform.rotation = currPoint.rotation;
                PlayerBeingMoved = false;

                yield return new WaitForSeconds(0.5f);
                GetComponent<Rigidbody>().isKinematic = false;

            }
        }
        else if(other.gameObject.tag == "OutOfBounds")
        {
            if (gameObject.tag == "Player")
            {
                GetComponent<Player>().currentspeed = 0;
                outOfBounds = true;
                PlayerBeingMoved = true;
                GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Rigidbody>().velocity = Vector3.zero;

                Transform currPoint;
                if (RACE_MANAGER.RACE_COMPLETED)
                {
                    currPoint = GetComponent<Player>().RaceEndPath.GetChild(GetComponent<Player>().currentWayPoint);
                    //set a few lap checkpoints to true ahead
                    for (int i = 1; i <= 3; i++)
                    {
                        GetComponent<LapCounter>().checkpointsVisited[GetComponent<LapCounter>().currentCheckpointVal + i] = true;
                    }
                }
                else
                {
                    currPoint = GetComponent<LapCounter>().checkpoints.GetChild(GetComponent<LapCounter>().currentCheckpointVal);
                }

                //get item path checkpoint next
                if (tracker.ActivePath == null)
                {
                    Debug.LogWarning("ActivePath is null on WaypointTracker!");
                    yield return null;
                }

                // Find the closest waypoint
                float dist = Mathf.Infinity;
                int closestWaypoint = 0;

                for (int i = 0; i < tracker.ActivePath.childCount; i++)
                {
                    float d = Vector3.Distance(tracker.ActivePath.GetChild(i).position, transform.position);
                    if (d < dist)
                    {
                        dist = d;
                        closestWaypoint = i;
                    }
                }

                // Update the current waypoint in the tracker
                tracker.SetCurrentWaypoint(closestWaypoint + 1);

                transform.position = currPoint.position;
                transform.rotation = currPoint.rotation;
                PlayerBeingMoved = false;

                yield return new WaitForSeconds(0.5f);
                GetComponent<Rigidbody>().isKinematic = false;
            }
            if (gameObject.tag == "Opponent")
            {
                GetComponent<ComputerDriver>().current_speed = 0;
                outOfBounds = true;
                PlayerBeingMoved = true;
                GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Rigidbody>().velocity = Vector3.zero;

                Transform currPoint = GetComponent<ComputerDriver>().path.GetChild(GetComponent<ComputerDriver>().current_node);
                //set a few lap checkpoints to true ahead
                for(int i = 1; i <= 3; i++)
                {
                    GetComponent<LapCounter>().checkpointsVisited[GetComponent<LapCounter>().currentCheckpointVal + i] = true;
                }

                //get item path checkpoint next
                float dist = 99999;
                int checkPointVal = 0;
                for (int i = 0; i < GetComponent<OpponentItemManager>().path.childCount; i++)
                {
                    if (Vector3.Distance(GetComponent<OpponentItemManager>().path.GetChild(i).position, transform.position) < dist)
                    {
                        dist = Vector3.Distance(GetComponent<OpponentItemManager>().path.GetChild(i).position, transform.position);
                        checkPointVal = i;
                    }
                }
                GetComponent<OpponentItemManager>().currentWayPoint = checkPointVal + 1;

                transform.position = currPoint.position;
                transform.rotation = currPoint.rotation;
                PlayerBeingMoved = false;

                yield return new WaitForSeconds(0.5f);
                GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Dirt")
        {
            FellInWater = false;
            outOfBounds = false;
        }
    }
}
