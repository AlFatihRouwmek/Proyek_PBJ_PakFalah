using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Inventory")]
    public bool hasKey = false;

    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    private Rigidbody2D rb;

    private float horizontalInput;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction interactAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // =========================
        // MOVEMENT - A / D
        // =========================
        moveAction = new InputAction("Move");

        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");

        // =========================
        // JUMP - SPACE
        // =========================
        jumpAction = new InputAction(
            "Jump",
            binding: "<Keyboard>/space"
        );

        jumpAction.performed += OnJump;

        // =========================
        // INTERACT - E
        // =========================
        interactAction = new InputAction(
            "Interact",
            binding: "<Keyboard>/e"
        );

        interactAction.performed += OnInteract;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        interactAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        interactAction.Disable();
    }

    // =========================================================
    // NETWORK SPAWN
    // =========================================================

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log(
            "PLAYER SPAWNED | " +
            "Owner: " + OwnerClientId +
            " | Local Client: " +
            NetworkManager.Singleton.LocalClientId +
            " | IsOwner: " + IsOwner
        );

        // Only the owner controls this player.
        if (!IsOwner)
        {
            Debug.Log(
                "This player belongs to another client."
            );
        }
        else
        {
            Debug.Log(
                "This is MY player. Input enabled."
            );
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // IMPORTANT:
        // Only the client that owns this player can control it.
        if (!IsOwner)
            return;

        horizontalInput = moveAction.ReadValue<float>();
    }

    // =========================================================
    // PHYSICS MOVEMENT
    // =========================================================

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        rb.linearVelocity = new Vector2(
            horizontalInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    // =========================================================
    // JUMP
    // =========================================================

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner)
            return;

        Jump();
    }

    private void Jump()
    {
        if (!IsGrounded())
            return;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );
    }

    // =========================================================
    // GROUND CHECK
    // =========================================================

    private bool IsGrounded()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning(
                "GroundCheck belum dipasang di Inspector!"
            );

            return false;
        }

        return Physics2D.OverlapCircle(
            groundCheck.position,
            0.2f,
            groundLayer
        );
    }

    // =========================================================
    // INTERACTION
    // =========================================================

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!IsOwner)
            return;

        Interact();
    }

    private void Interact()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            interactRange,
            interactableLayer
        );

        if (hit == null)
        {
            Debug.Log("Tidak ada object yang bisa di-interact.");
            return;
        }

        KeysChamber chamber =
            hit.GetComponent<KeysChamber>();

        if (chamber == null)
        {
            Debug.Log(
                "Object ditemukan, tetapi bukan KeysChamber."
            );

            return;
        }

        if (hasKey)
        {
            chamber.InsertKey();

            hasKey = false;

            Debug.Log(
                "Key berhasil dimasukkan ke Chamber."
            );
        }
        else
        {
            Debug.Log(
                "Kamu butuh Key untuk mengaktifkan Chamber!"
            );
        }
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        // Interaction range
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            interactRange
        );

        // Ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                0.2f
            );
        }
    }
}