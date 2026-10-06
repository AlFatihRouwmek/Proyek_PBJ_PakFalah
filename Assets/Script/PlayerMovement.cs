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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Setup Input Actions
        moveAction = new InputAction("Move");
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");

        jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
        jumpAction.performed += ctx => Jump();

        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.performed += ctx => Interact();
    }

    // Gantikan OnEnable/OnDisable dengan OnNetworkSpawn/OnNetworkDespawn
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // HANYA aktifkan input jika objek karakter ini adalah milik pemain lokal ini
        if (IsOwner)
        {
            moveAction.Enable();
            jumpAction.Enable();
            interactAction.Enable();
        }
        else
        {
            // Nonaktifkan input untuk karakter pemain lain yang dirender di layar kita
            moveAction.Disable();
            jumpAction.Disable();
            interactAction.Disable();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        moveAction.Disable();
        jumpAction.Disable();
        interactAction.Disable();
    }

    void Update()
    {
        if (!IsOwner) return;

        horizontalInput = moveAction.ReadValue<float>();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        if (!IsOwner) return;

        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void Interact()
    {
        if (!IsOwner) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactableLayer);

        if (hit != null)
        {
            KeysChamber chamber = hit.GetComponent<KeysChamber>();

            if (chamber != null)
            {
                if (hasKey)
                {
                    chamber.InsertKey();
                    hasKey = false;
                }
                else
                {
                    Debug.Log("Kamu butuh Keys untuk mengaktifkan Chamber ini!");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }

    private bool IsGrounded()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck belum dipasang di Inspector!");
            return false;
        }

        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
}