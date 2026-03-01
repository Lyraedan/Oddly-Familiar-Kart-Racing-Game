using UnityEngine;

public class MinimapVisualizer : MonoBehaviour
{
    public Transform start;
    public Transform end;

    public float playerSize = 10f;
    public Color color = Color.green;
    public Color pointColor = Color.blue;
    public Color playerColor = Color.yellow;

    public bool visible = true;

    private void OnDrawGizmos()
    {
        if (!visible || start == null || end == null)
            return;

        // Draw start and end points
        Gizmos.color = pointColor;
        Gizmos.DrawSphere(start.position, 0.5f);
        Gizmos.DrawSphere(end.position, 0.5f);

        // Draw minimap bounds cube
        Gizmos.color = color;
        Vector3 center = (start.position + end.position) * 0.5f;
        Vector3 size = new Vector3(
            Mathf.Abs(end.position.x - start.position.x),
            0.1f,
            Mathf.Abs(end.position.z - start.position.z)
        );
        Gizmos.DrawCube(center, size);

        // Calculate player spawn at center of bounds
        Vector3 playerWorldPos = (start.position + end.position) * 0.5f;

        // Draw the player sphere
        Gizmos.color = playerColor;
        Gizmos.DrawSphere(playerWorldPos, playerSize); // radius 1f
    }
}