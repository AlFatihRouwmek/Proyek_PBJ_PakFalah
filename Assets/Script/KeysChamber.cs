//using System.Diagnostics;
using UnityEngine;

public class KeysChamber : MonoBehaviour
{
    [SerializeField] private Sprite keysWithChamberSprite; // Masukkan gambar KeysWithChamber di Inspector
    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void InsertKey()
    {
        if (!isActivated)
        {
            spriteRenderer.sprite = keysWithChamberSprite; // Mengubah gambar
            isActivated = true;
            Debug.Log("Keys Chamber Aktif!");
        }
    }
}