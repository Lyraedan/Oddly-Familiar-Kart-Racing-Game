using UnityEngine;

public class WheelSpawnDebug : MonoBehaviour
{
    public float axisLength = 1.0f;
    public float sphereRadius = 0.1f;

    void OnDrawGizmos()
    {
        Vector3 pos = transform.position;

        // Draw center sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pos, sphereRadius);

        // X axis (Right) - Red
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + transform.right * axisLength);

        // Y axis (Up) - Green
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + transform.up * axisLength);

        // Z axis (Forward) - Blue
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + transform.forward * axisLength);
    }
}