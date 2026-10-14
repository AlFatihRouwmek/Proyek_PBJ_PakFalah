using UnityEngine;
using Unity.Netcode; // 1. Tambahkan namespace Netcode

public class KeysChamber : NetworkBehaviour // 2. Ubah MonoBehaviour menjadi NetworkBehaviour
{
    [SerializeField] private Sprite keysWithChamberSprite;
    private SpriteRenderer spriteRenderer;

    // 3. Gunakan NetworkVariable agar nilainya tersinkron otomatis ke seluruh Client/Pemain
    public NetworkVariable<bool> isActivated = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner // Atau Server tergantung otoritas
    );

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Daftarkan listener saat nilai isActivated berubah di jaringan
        isActivated.OnValueChanged += OnChamberStateChanged;

        // Jika pemain baru bergabung dan chamber sudah aktif, perbarui spritenya
        if (isActivated.Value)
        {
            UpdateSprite();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isActivated.OnValueChanged -= OnChamberStateChanged;
    }

    // Dipanggil saat variabel isActivated berubah nilainya
    private void OnChamberStateChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            UpdateSprite();
        }
    }

    private void UpdateSprite()
    {
        if (keysWithChamberSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = keysWithChamberSprite;
        }
    }

    // Fungsi ini dipanggil dari PlayerMovement ketika interaksi berhasil
    public void InsertKey()
    {
        if (!isActivated.Value)
        {
            // Jika dipanggil dari Client, minta Server/Host untuk merubah nilainya lewat RPC
            if (IsServer)
            {
                isActivated.Value = true;
            }
            else
            {
                InsertKeyServerRpc();
            }
            Debug.Log("Keys Chamber Aktif!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InsertKeyServerRpc()
    {
        isActivated.Value = true;
    }
}