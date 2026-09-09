//using System.Diagnostics;
using UnityEngine;

public class KeyItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Memastikan yang menyentuh adalah pemain
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.hasKey = true;
                Debug.Log("Keys berhasil diambil!");
                Destroy(gameObject); // Menghilangkan kunci dari level
            }
        }
    }
}