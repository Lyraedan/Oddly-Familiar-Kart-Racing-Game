using UnityEngine;

public class RedShellItem : ItemBase
{
    public GameObject redShellPrefab;
    public Transform forwardSpawn;
    public Transform backSpawn;

    public override void Use(bool forward)
    {
        Transform spawn = forward ? forwardSpawn : backSpawn;

        GameObject shell = Instantiate(redShellPrefab, spawn.position, spawn.rotation);
        RedShell rs = shell.GetComponent<RedShell>();

        rs.AntiGravity = player.antiGravity;
        rs.who_threw_shell = player.name;
        rs.current_node = player.waypointTracker.CurrentWaypoint;

        itemManager.ConsumeItem();
    }
}