using System.Collections;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    [Header("Water Settings")]
    [SerializeField] private float waterSinkForce = 5000f;
    [SerializeField] private float playerWaterDelay = 0.5f;
    [SerializeField] private float opponentWaterDelay = 1f;

    [Header("Respawn Settings")]
    [SerializeField] private float respawnFreezeTime = 0.5f;
    [SerializeField] private float forwardSpawnOffset = 0.002f; // small t offset forward
    private PathTool pathTool;

    [HideInInspector] public bool FellInWater;
    [HideInInspector] public bool OutOfBoundsState;
    [HideInInspector] public bool PlayerBeingMoved;

    private Rigidbody rb;
    private Player player;
    private ComputerDriver computerDriver;
    private LapCounter lapCounter;
    private OpponentItemManager opponentItemManager;

    private bool isPlayer;
    private bool isOpponent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        computerDriver = GetComponent<ComputerDriver>();
        lapCounter = GetComponent<LapCounter>();
        opponentItemManager = GetComponent<OpponentItemManager>();

        isPlayer = CompareTag("Player");
        isOpponent = CompareTag("Opponent");
    }

    private void Update()
    {
        if (FellInWater)
        {
            rb.AddRelativeForce(Vector3.down * waterSinkForce * Time.deltaTime, ForceMode.Acceleration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
            StartCoroutine(HandleWater());

        else if (other.CompareTag("OutOfBounds"))
            StartCoroutine(HandleOutOfBounds());
    }

    #region WATER

    private IEnumerator HandleWater()
    {
        StopMovement();

        FellInWater = true;
        PlayerBeingMoved = true;

        yield return new WaitForSeconds(isOpponent ? opponentWaterDelay : playerWaterDelay);

        Freeze();

        RespawnToLastCheckpointSpline();

        UpdateOpponentItemWaypoint();

        PlayerBeingMoved = false;

        yield return new WaitForSeconds(respawnFreezeTime);

        Unfreeze();
    }

    #endregion

    #region OUT OF BOUNDS

    private IEnumerator HandleOutOfBounds()
    {
        StopMovement();

        OutOfBoundsState = true;
        PlayerBeingMoved = true;

        Freeze();

        RespawnToLastCheckpointSpline();

        MarkFutureCheckpointsVisitedIfNeeded();
        UpdateOpponentItemWaypoint();

        PlayerBeingMoved = false;

        yield return new WaitForSeconds(0.5f);

        Unfreeze();
    }

    #endregion

    #region CORE HELPERS

    private void StopMovement()
    {
        if (isPlayer && player != null)
            player.currentspeed = 0;

        if (isOpponent && computerDriver != null)
            computerDriver.current_speed = 0;
    }

    private void Freeze()
    {
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
    }

    private void Unfreeze()
    {
        rb.isKinematic = false;
    }

    private void RespawnToLastCheckpointSpline()
    {
        if (lapCounter == null)
            return;

        // Fetch latest pathtools (because Opponents can change)
        if (isPlayer)
            pathTool = player.raceEndPathTool;

        if (isOpponent)
            pathTool = computerDriver.SelectedPathTool;

        bool success = lapCounter.TryGetLastCheckpointSplinePose(pathTool,
            out Vector3 pos,
            out Quaternion rot);

        Debug.Log("Spline respawn success: " + success);

        if (success)
        {
            transform.SetPositionAndRotation(pos, rot);
        }
        else if (lapCounter.checkpoints != null)
        {
            Debug.Log("Using checkpoint fallback");

            int lastID = lapCounter.ProgressIndex;

            Transform fallback =
                lapCounter.checkpoints.GetChild(lastID);

            transform.SetPositionAndRotation(
                fallback.position,
                fallback.rotation
            );
        }
    }

    private void UpdateOpponentItemWaypoint()
    {
        if (!isOpponent || opponentItemManager == null || opponentItemManager.path == null)
            return;

        float minDist = Mathf.Infinity;
        int closestIndex = 0;

        for (int i = 0; i < opponentItemManager.path.childCount; i++)
        {
            float dist = Vector3.Distance(
                opponentItemManager.path.GetChild(i).position,
                transform.position
            );

            if (dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }

        opponentItemManager.currentWayPoint = closestIndex + 1;
    }

    private void MarkFutureCheckpointsVisitedIfNeeded()
    {
        if (lapCounter == null)
            return;

        for (int i = 1; i <= 3; i++)
        {
            int index = lapCounter.currentCheckpointVal + i;

            if (index < lapCounter.checkpointsVisited.Length)
                lapCounter.checkpointsVisited[index] = true;
        }
    }

    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Dirt"))
        {
            FellInWater = false;
            OutOfBoundsState = false;
        }
    }
}