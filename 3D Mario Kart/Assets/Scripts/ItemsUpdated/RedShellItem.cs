using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class RedShellItem : ItemBase
{
    [Header("Paths")]
    public List<PathTool> paths = new();
    private PathTool currentPath;
    private int currentNode = 0;

    [Header("Movement")]
    public float speed = 45f;
    public float rotationSpeed = 6f;
    public LayerMask groundMask;

    [Header("FX")]
    public Transform particlesParent;
    public SkinnedMeshRenderer shellMesh;

    [HideInInspector] public bool AntiGravity = false;
    [HideInInspector] public float lifetime;

    private Rigidbody rb;
    private SphereCollider sphereCollider;

    private LapCounter thrownBy;
    private Transform chaseTarget;
    private bool lockedOnTarget = false;

    private Transform playerTransform;
    private bool closeToPlayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        rb.isKinematic = true;
    }

    public override void Use(bool forward, GameObject user)
    {
        Transform spawn = forward ? forwardSpawn : backSpawn;
        UpdateHoldPoint(spawn);
        PlayPlayerAnim(forward);

        transform.parent = null;

        thrownBy = user.GetComponent<LapCounter>();
        AntiGravity = user.GetComponent<Player>().antiGravity;

        paths = RaceManager.Instance.AIPaths;
        playerTransform = player.transform;

        currentPath = GetClosestPath();
        currentNode = GetClosestNode();

        rb.isKinematic = false;
        sphereCollider.enabled = true;

        StartUseDelay(0.25f);
        itemManager.ConsumeItem(shouldDestroy: false);
        Release();
    }


    void FixedUpdate()
    {
        if (rb.isKinematic) return;

        lifetime += Time.deltaTime;

        DetectTarget();

        if (lockedOnTarget)
            ChaseTarget();
        else
            FollowSpline();

        ApplyDownForce();
        GroundAlign();
        HandleWarning();
    }

    // Call this in FixedUpdate
    void DetectTarget()
    {
        if (lockedOnTarget)
            return;

        // Find the closest player ahead in the race
        var racers = IngameUIHolder.Instance.LapCounters;
        LapCounter target = null;
        float closestDistance = float.MaxValue;

        foreach (var r in racers)
        {
            // Skip self if attached to thrower object
            if (r.RacerID == thrownBy.RacerID) continue;

            // Distance to this racer
            float dist = Vector3.Distance(transform.position, r.transform.position);

            // If no target yet, pick the closest ahead racer
            if (dist < closestDistance)
            {
                closestDistance = dist;
                target = r;
            }
        }

        if (target != null)
        {
            chaseTarget = target.transform;
            lockedOnTarget = true;
        }
    }

    void FollowSpline()
    {
        if (currentPath == null || currentPath.pathRoot == null)
            return;

        Transform node = currentPath.pathRoot.GetChild(currentNode);
        Vector3 targetPos = node.position;

        SteerTowards(targetPos);
        MoveForward();

        Vector3 toNode = targetPos - transform.position;

        // Distance check
        if (toNode.magnitude < 2f)
        {
            AdvanceNode();
        }
        else
        {
            // 🔥 Overshoot detection (prevents orbiting)
            if (Vector3.Dot(transform.forward, toNode) < 0f)
            {
                AdvanceNode();
            }
        }
    }

    void AdvanceNode()
    {
        currentNode++;
        if (currentNode >= currentPath.pathRoot.childCount)
            currentNode = 0;
    }

    void ChaseTarget()
    {
        if (chaseTarget == null)
        {
            lockedOnTarget = false;
            return;
        }

        SteerTowards(chaseTarget.position);
        MoveForward();
    }

    void SteerTowards(Vector3 target)
    {
        Vector3 desiredDir = (target - transform.position).normalized;

        float maxTurn = rotationSpeed * Time.deltaTime;

        Vector3 newDir = Vector3.RotateTowards(
            transform.forward,
            desiredDir,
            maxTurn,
            0f
        );

        transform.rotation = Quaternion.LookRotation(newDir, transform.up);
    }

    void MoveForward()
    {
        rb.velocity = transform.forward * speed;
    }

    PathTool GetClosestPath()
    {
        float best = float.MaxValue;
        PathTool bestPath = null;

        foreach (var p in paths)
        {
            if (p == null || p.pathRoot == null) continue;

            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < best)
            {
                best = dist;
                bestPath = p;
            }
        }

        return bestPath;
    }

    int GetClosestNode()
    {
        if (currentPath == null || currentPath.pathRoot == null)
            return 0;

        float best = float.MaxValue;
        int bestIndex = 0;

        for (int i = 0; i < currentPath.pathRoot.childCount; i++)
        {
            float dist = Vector3.Distance(
                transform.position,
                currentPath.pathRoot.GetChild(i).position
            );

            if (dist < best)
            {
                best = dist;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    void ApplyDownForce()
    {
        if (!AntiGravity)
            rb.AddForce(Vector3.down * 40f, ForceMode.Acceleration);
        else
            rb.AddRelativeForce(Vector3.down * 60f, ForceMode.Acceleration);
    }

    void GroundAlign()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 5f, groundMask))
        {
            Quaternion rot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, 10f * Time.deltaTime);
        }
    }

    void HandleWarning()
    {
        if (closeToPlayer || playerTransform == null)
            return;

        if (Vector3.Distance(playerTransform.position, transform.position) < 100 &&
            thrownBy.gameObject != playerTransform.gameObject)
        {
            closeToPlayer = true;
            StartCoroutine(RaceManager.Instance.WarningRedShell(transform));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!ReadyForUse)
            return;

        if (collision.gameObject.CompareTag("Opponent"))
        {
            var manager = collision.gameObject.GetComponent<OpponentItemManager>();
            if (manager != null && !manager.StarPowerUp)
            {
                manager.hitByShell();
                DestroyShell();
            }
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            var im = collision.gameObject.GetComponent<ItemManager>();
            if (!im.StarPowerUp)
                StartCoroutine(player.hitByShell());

            DestroyShell();
        }

        if (collision.gameObject.CompareTag("Shell") ||
            collision.gameObject.CompareTag("Banana") ||
            collision.gameObject.CompareTag("Cow"))
        {
            DestroyShell();
        }
    }

    void DestroyShell()
    {
        foreach (Transform t in particlesParent)
            t.GetComponent<ParticleSystem>().Play();

        shellMesh.enabled = false;
        sphereCollider.enabled = false;
        rb.isKinematic = true;

        Destroy(gameObject, 3f);
    }
}