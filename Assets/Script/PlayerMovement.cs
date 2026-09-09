//using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check (Opsional untuk saat ini)")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Inventory")]
    public bool hasKey = false;

    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    private Rigidbody2D rb;
    private float horizontalInput;

    // Deklarasi variabel Input Action langsung di dalam skrip
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction interactAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 1. Setup Input Movement (A = Kiri [-1], D = Kanan [1])
        moveAction = new InputAction("Move");
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");

        // 2. Setup Input Jump (Spasi)
        jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
        jumpAction.performed += ctx => Jump(); // Panggil fungsi Jump() saat ditekan

        // 3. Setup Input Interact (E)
        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.performed += ctx => Interact(); // Panggil fungsi Interact() saat ditekan
    }

    // Input Action WAJIB diaktifkan dan dinonaktifkan
    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        interactAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        interactAction.Disable();
    }

    void Update()
    {
        // Membaca nilai dari aksi bergerak setiap frame
        horizontalInput = moveAction.ReadValue<float>();
    }

    void FixedUpdate()
    {
        // Di Unity 6, Rigidbody2D menggunakan 'linearVelocity' (menggantikan 'velocity')
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // Perbarui fungsi Interact() menjadi seperti ini:
    private void Interact()
    {
        // Mencari objek di sekitar pemain yang berada di layer Interactable
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactableLayer);

        if (hit != null)
        {
            KeysChamber chamber = hit.GetComponent<KeysChamber>();

            if (chamber != null)
            {
                if (hasKey)
                {
                    chamber.InsertKey();
                    hasKey = false; // Kunci terpakai
                }
                else
                {
                    Debug.Log("Kamu butuh Keys untuk mengaktifkan Chamber ini!");
                }
            }
        }
    }

    // Tambahan opsional agar radius interaksi terlihat di Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }

    private bool IsGrounded()
    {
        // Ubah return true menjadi return false agar tidak bisa lompat di udara
        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck belum dipasang di Inspector!");
            return false;
        }

        // Mengecek area di bawah kaki karakter apakah menyentuh layer Ground
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
}