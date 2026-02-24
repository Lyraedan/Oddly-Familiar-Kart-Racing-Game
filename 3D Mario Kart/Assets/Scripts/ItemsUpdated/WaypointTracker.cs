using UnityEngine;

public class WaypointTracker : MonoBehaviour
{
    public Transform path1;
    public Transform path2;

    private Transform activePath;

    public Transform ActivePath => activePath;

    public int CurrentWaypoint { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (activePath == null) return;

        if (activePath.GetChild(CurrentWaypoint) == other.transform)
        {
            if (CurrentWaypoint == activePath.childCount - 1)
                CurrentWaypoint = 0;
            else
                CurrentWaypoint++;
        }
    }

    public void SetCurrentWaypoint(int index)
    {
        CurrentWaypoint = index % ActivePath.childCount;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.name == "ItemPathColliderPath1")
            activePath = path1;

        if (other.name == "ItemPathColliderPath2")
            activePath = path2;
    }
}