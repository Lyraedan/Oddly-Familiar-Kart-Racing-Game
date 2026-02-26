using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointID = 0;
    public bool autoAssignID = true;

    private BoxCollider checkpointCollider;

    private void Start()
    {
        checkpointCollider = GetComponent<BoxCollider>();
        if (checkpointCollider)
        {
            checkpointCollider.isTrigger = true; // THESE MUST BE TRIGGERS FOR THE CHECKPOINT SYSTEM TO WORK PROPERLY
        } else
        {
            Debug.LogError("Checkpoint " + gameObject.name + " does not have a BoxCollider component. Please add one and set it as a trigger.");
        }
        if (autoAssignID)
        {
            checkpointID = transform.GetSiblingIndex();
        }
    }
}
