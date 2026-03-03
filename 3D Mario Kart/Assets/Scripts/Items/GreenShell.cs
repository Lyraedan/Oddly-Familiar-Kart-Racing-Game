using System.Collections;
using UnityEngine;

// Handles Green Shell movement, collisions, and anti-gravity behavior
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class GreenShell : MonoBehaviour
{
    [Header("Shell Settings")]
    public LayerMask groundMask;
    public float velocityMagnitude = 20f;
    public bool needsExtraDownForceAntigravity = false;

    [HideInInspector] public string thrownBy;
    [HideInInspector] public bool AntiGravity = false;
    [HideInInspector] public float lifetime;

    private Rigidbody rb;
    private SphereCollider sphereCollider;

    private bool grounded;
    private bool antiGravityGrounded;
    private Vector3 moveDirection;

    // References injected at spawn
    private Player player;
    private PlayerSounds playerSounds;

    public void Initialize(Vector3 direction, string owner, Player mainPlayer)
    {
        moveDirection = direction;
        thrownBy = owner;

        player = mainPlayer;
        playerSounds = mainPlayer.GetComponent<PlayerSounds>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        Debug.LogError("OBSOLETE GREEN SHELL SCRIPT IN USE! Please replace with GreenShellItem.cs");
        Initialize(moveDirection, thrownBy, player);
    }

    void FixedUpdate()
    {
        MoveShell();
        AlignToGround();
        lifetime += Time.deltaTime;
    }

    private void MoveShell()
    {
        Vector3 velocity = moveDirection.normalized * velocityMagnitude * Time.deltaTime;

        if (!AntiGravity)
            velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        ApplyDownwardForce();
    }

    private void ApplyDownwardForce()
    {
        float force = AntiGravity ? 10000f : 20000f;

        if (!AntiGravity && !player.GLIDER_FLY)
            rb.AddForce(Vector3.down * force * Time.deltaTime, ForceMode.Acceleration);
        else if (AntiGravity)
            rb.AddRelativeForce(Vector3.down * force * Time.deltaTime, ForceMode.Acceleration);

        if (needsExtraDownForceAntigravity && AntiGravity && !antiGravityGrounded)
            rb.AddRelativeForce(Vector3.down * 100000f * Time.deltaTime, ForceMode.Acceleration);
    }

    private void AlignToGround()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 10f, groundMask))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up * 2, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.LerpUnclamped(transform.rotation, targetRotation, 13f * Time.deltaTime);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("AntiGravity"))
            AntiGravity = true;
        else if (other.CompareTag("AntiGravityFalse"))
            AntiGravity = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;

        if (IsIgnoredCollision(tag)) return;

        switch (tag)
        {
            case "Shell":
            case "Banana":
            case "Cow":
                HandleShellCollision(collision);
                break;

            case "Opponent":
                HitOpponent(collision.gameObject);
                ReflectShell(collision);
                break;

            case "Player":
                HitPlayer(collision.gameObject);
                ReflectShell(collision);
                break;

            default:
                ReflectShell(collision);
                break;
        }
    }

    private bool IsIgnoredCollision(string tag)
    {
        return tag == "Ground" || tag == "Dirt" || tag == "JumpPanel" || tag == "ShellPlatforms" || tag == "GliderPanel";
    }

    private void HandleShellCollision(Collision collision)
    {
        if (collision.gameObject.tag != "Cow")
            Destroy(collision.gameObject);

        destroyShell();
    }

    private void ReflectShell(Collision collision)
    {
        rb.linearVelocity = Vector3.zero;
        moveDirection = Vector3.Reflect(moveDirection, collision.contacts[0].normal);

        if (lifetime > 20f)
            destroyShell();
    }

    private void HitOpponent(GameObject opponent)
    {
        var manager = opponent.GetComponent<OpponentItemManager>();
        if (manager != null && !manager.StarPowerUp && lifetime > 0.1f)
        {
            manager.hitByShell();
            if (thrownBy == "Mario")
            {
                player.Driver.SetTrigger("HitItem");
                if (playerSounds.CanPlayCharacterSound())
                    playerSounds.marioItemHit.Play();
            }
            destroyShell();
        }
    }

    private void HitPlayer(GameObject playerGO)
    {
        if (lifetime <= 0.05f) return;

        var itemManager = playerGO.GetComponent<ItemManager>();
        if (!itemManager.StarPowerUp)
        {
            StartCoroutine(playerGO.GetComponent<Player>().hitByShell());
            if (Camera.main.gameObject.activeSelf)
                Camera.main.GetComponent<Animator>().SetTrigger("ShellHit");
        }
        destroyShell();
    }

    private void OnCollisionStay(Collision collision)
    {
        string tag = collision.gameObject.tag;
        grounded |= (tag != "Ground" && tag != "Dirt");
        antiGravityGrounded |= (tag == "Ground" || tag == "Dirt");
    }

    private void OnCollisionExit(Collision collision)
    {
        string tag = collision.gameObject.tag;
        grounded &= !(tag != "Ground" && tag != "Dirt");
        antiGravityGrounded &= !(tag == "Ground" || tag == "Dirt");
    }

    public void destroyShell()
    {
        // Play particles
        Transform particlesParent = transform.GetChild(0);
        for (int i = 0; i < particlesParent.childCount; i++)
            particlesParent.GetChild(i).GetComponent<ParticleSystem>().Play();

        // Hide mesh
        transform.GetChild(1).GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false;

        // Disable physics
        sphereCollider.enabled = false;
        rb.isKinematic = true;

        Destroy(gameObject, 3f);
    }
}