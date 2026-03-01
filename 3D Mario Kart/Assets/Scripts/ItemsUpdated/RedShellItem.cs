using System.ComponentModel;
using UnityEngine;

public class RedShellItem : ItemBase
{
    public GameObject redShellPrefab;

    [HideInInspector] public string thrownBy;
    [HideInInspector] public bool AntiGravity = false;
    [HideInInspector] public float lifetime;

    private PathTool pathTool; // Fetch the spline
    private LapCounter lapCounter; // For tracking the checkpoints of the target

    private PlayerSounds playerSounds;

    public override void Use(bool forward, GameObject user)
    {
        Transform spawn = forward ? forwardSpawn : backSpawn;
        ReparentAndZero(spawn);
        PlayPlayerAnim(forward);

        SetPath(user);
        lapCounter = user.GetComponent<LapCounter>(); // User should always have a lap counter
    }

    void DetectTarget()
    {

    }

    void SetPath(GameObject user)
    {
        bool isPlayer = user.CompareTag("Player");
        bool isOpponent = user.CompareTag("Opponent");

        if (isPlayer)
        {
            if (player != null)
            {
                pathTool = player.raceEndPathTool;
            }
        }

        if (isOpponent)
        {
            ComputerDriver driver = user.GetComponent<ComputerDriver>();
            if (driver != null)
                pathTool = driver.SelectedPathTool;
        }
    }
}