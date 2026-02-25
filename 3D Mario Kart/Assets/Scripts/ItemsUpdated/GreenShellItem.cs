using System.Collections;
using UnityEngine;

// GreenShellItem: Acts as both the shell prefab and the item behavior
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class GreenShellItem : ItemBase
{
    [Header("Shell Settings")]
    public LayerMask groundMask;
    public float forwardVelocity = 6f;
    public float backwardVelocity = 3.5f;
    public bool needsExtraDownForceAntigravity = false;

    [Header("Particles & Mesh")]
    public Transform particlesParent;
    public SkinnedMeshRenderer shellMesh;

    [HideInInspector] public string thrownBy;
    [HideInInspector] public bool AntiGravity = false;
    [HideInInspector] public float lifetime;

    private Rigidbody rb;
    private SphereCollider sphereCollider;

    private bool grounded;
    private bool antiGravityGrounded;
    private Vector3 moveDirection;

    private PlayerSounds playerSounds;

    public override void Use(bool forward)
    {
        Transform spawn = forward ? forwardSpawn : backSpawn;

        if(forward)
            player.Driver.SetTrigger("ThrowForward");
        else
            player.Driver.SetTrigger("ThrowBackward");

        // Set move direction
        moveDirection = forward ? player.transform.forward : -player.transform.forward;
        thrownBy = player.tag;
        AntiGravity = player.antiGravity;
        lifetime = 0f;

        // Detach from player (unparent)
        transform.parent = null;

        // Teleport shell to spawn (world space)
        transform.position = spawn.position;
        transform.rotation = spawn.rotation;

        // Set initial velocity
        rb.linearVelocity = moveDirection * (forward ? forwardVelocity : backwardVelocity);

        // Cache player sounds
        playerSounds = player.GetComponent<PlayerSounds>();

        // Activate physics
        rb.isKinematic = false;
        sphereCollider.enabled = true;

        // Consume the item from the player
        itemManager.ConsumeItem(shouldDestroy: false); // remove item but do not destroy shell
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        rb.isKinematic = true; // initially kinematic until Use() is called
    }

    public void SetPlayer(Player p)
    {
        player = p;
    }

    void FixedUpdate()
    {
        if (rb.isKinematic) return; // skip if not yet used

        MoveShell();
        AlignToGround();
        lifetime += Time.deltaTime;
    }

    private void MoveShell()
    {
        Vector3 velocity = moveDirection.normalized * (forwardVelocity) * Time.deltaTime;

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

        if (tag == "Ground" || tag == "Dirt" || tag == "JumpPanel" || tag == "ShellPlatforms" || tag == "GliderPanel")
            return;

        if (tag == "Shell" || tag == "Banana" || tag == "Cow")
        {
            if (tag != "Cow")
                Destroy(collision.gameObject);
            DestroyShell();
            return;
        }

        if (tag == "Opponent")
        {
            var manager = collision.gameObject.GetComponent<OpponentItemManager>();
            if (manager != null && !manager.StarPowerUp && lifetime > 0.1f)
            {
                manager.hitByShell();
                if (thrownBy == "Player")
                {
                    player.Driver.SetTrigger("HitItem");
                    if (playerSounds.CanPlayCharacterSound())
                        playerSounds.marioItemHit.Play();
                }
                DestroyShell();
            }
            ReflectShell(collision);
            return;
        }

        if (tag == "Player")
        {
            var itemManager = collision.gameObject.GetComponent<ItemManager>();
            if (!itemManager.StarPowerUp && lifetime > 0.05f)
            {
                StartCoroutine(collision.gameObject.GetComponent<Player>().hitByShell());
                if (Camera.main != null && Camera.main.gameObject.activeSelf)
                    Camera.main.GetComponent<Animator>().SetTrigger("ShellHit");
            }
            DestroyShell();
            ReflectShell(collision);
            return;
        }

        ReflectShell(collision);
    }

    private void ReflectShell(Collision collision)
    {
        rb.linearVelocity = Vector3.zero;
        moveDirection = Vector3.Reflect(moveDirection, collision.contacts[0].normal);

        if (lifetime > 10f)
            DestroyShell();
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

    private void DestroyShell()
    {
        if (particlesParent != null)
        {
            for (int i = 0; i < particlesParent.childCount; i++)
                particlesParent.GetChild(i).GetComponent<ParticleSystem>().Play();
        }

        if (shellMesh != null)
            shellMesh.enabled = false;

        sphereCollider.enabled = false;
        rb.isKinematic = true;

        Destroy(gameObject, 3f);
    }
}