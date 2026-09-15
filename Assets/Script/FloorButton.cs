using UnityEngine;
using System.Collections.Generic;

public class FloorButton : MonoBehaviour
{
    [Header("Button Settings")]
    public int buttonID; // Samakan angka ini dengan ID target

    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite pressedSprite;

    private SpriteRenderer spriteRenderer;
    private List<ButtonTarget> linkedTargets = new List<ButtonTarget>();

    // Menghitung berapa objek yang sedang menindih tombol
    private int objectsOnButton = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = normalSprite;

        // Mencari semua target di scene dan menghubungkan yang ID-nya sama
        // Menggunakan FindObjectsByType untuk Unity 6
        ButtonTarget[] allTargets = FindObjectsByType<ButtonTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (ButtonTarget target in allTargets)
        {
            if (target.targetID == buttonID)
            {
                linkedTargets.Add(target);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hanya merespons objek dengan tag Player atau Stone
        if (collision.CompareTag("Player") || collision.CompareTag("Stone"))
        {
            if (objectsOnButton == 0)
            {
                ActivateButton();
            }
            objectsOnButton++;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Stone"))
        {
            objectsOnButton--;

            // Memastikan nilai tidak minus dan mematikan tombol jika kosong
            if (objectsOnButton <= 0)
            {
                objectsOnButton = 0;
                DeactivateButton();
            }
        }
    }

    private void ActivateButton()
    {
        spriteRenderer.sprite = pressedSprite;
        foreach (ButtonTarget target in linkedTargets)
        {
            target.ToggleVisibility(true);
        }
    }

    private void DeactivateButton()
    {
        spriteRenderer.sprite = normalSprite;
        foreach (ButtonTarget target in linkedTargets)
        {
            target.ToggleVisibility(false);
        }
    }
}