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

    [Header("Player Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite redPlayerSprite;
    [SerializeField] private Sprite bluePlayerSprite;

    // NetworkVariable untuk menentukan apakah player ini warna Biru atau bukan (False = Merah, True = Biru)
    public NetworkVariable<bool> isBluePlayer = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 1. HANYA SERVER YANG MENENTUKAN WARNA BERDASARKAN URUTAN MASUK (CLIENT ID)
        if (IsServer)
        {
            // Jika Client ID lebih dari 0 (berarti Player 2 / Client), set jadi Biru (true)
            if (OwnerClientId > 0)
            {
                isBluePlayer.Value = true;
            }
            else
            {
                isBluePlayer.Value = false; // Host / Player 1 tetap Merah
            }
        }

        // 2. TERAPKAN SPRITE AWAL SAAT SPAWN
        UpdatePlayerSprite(isBluePlayer.Value);

        // 3. DAFTARKAN LISTENER AGAR SPRITE BERUBAH JIKA NILAI NETWORK VARIABLE BERUBAH
        isBluePlayer.OnValueChanged += OnPlayerColorChanged;

        // 4. AKTIFKAN INPUT HANYA UNTUK PEMILIK LOKAL
        if (IsOwner)
        {
            moveAction.Enable();
            jumpAction.Enable();
            interactAction.Enable();
        }
        else
        {
            moveAction.Disable();
            jumpAction.Disable();
            interactAction.Disable();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isBluePlayer.OnValueChanged -= OnPlayerColorChanged;

        moveAction.Disable();
        jumpAction.Disable();
        interactAction.Disable();
    }

    private void OnPlayerColorChanged(bool previousValue, bool newValue)
    {
        UpdatePlayerSprite(newValue);
    }

    private void UpdatePlayerSprite(bool isBlue)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isBlue ? bluePlayerSprite : redPlayerSprite;
        }
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

        // 1. Cek apakah menyentuh lantai (Ground Layer)
        bool touchingGround = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // 2. Cek apakah menyentuh kepala/badan pemain lain (menggunakan OverlapCircleAll atau OverlapCircle dengan Collider)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, 0.2f);
        bool touchingPlayer = false;

        foreach (Collider2D col in colliders)
        {
            // Memastikan objek yang diinjak adalah Player lain (bukan diri sendiri)
            if (col.CompareTag("Player") && col.gameObject != gameObject)
            {
                touchingPlayer = true;
                break;
            }
        }

        // Karakter dianggap "di tanah" (bisa lompat) jika menyentuh Ground ATAU menyentuh Player lain
        return touchingGround || touchingPlayer;
    }
}