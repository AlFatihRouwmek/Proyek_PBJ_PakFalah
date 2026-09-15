//using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement; // Diperlukan untuk mereset/pindah level

public class HazardSpike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Mengecek apakah objek yang menyentuh duri memiliki tag Player
        if (collision.CompareTag("Player"))
        {
            PlayerDie();
        }
    }

    // Mengantisipasi jika collider duri tidak diset sebagai Trigger
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerDie();
        }
    }

    private void PlayerDie()
    {
        Debug.Log("Pemain menyentuh duri! Mengulang level...");

        // Memuat ulang scene/level yang sedang aktif saat ini
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}