using UnityEngine;

public class ButtonTarget : MonoBehaviour
{
    [Header("Target Settings")]
    public int targetID; // Samakan angka ini dengan ID tombol

    [Tooltip("Apakah objek ini sudah muncul sejak awal permainan?")]
    [SerializeField] private bool startActive = false;

    void Start()
    {
        // Mengatur status awal objek saat game dimulai
        gameObject.SetActive(startActive);
    }

    public void ToggleVisibility(bool isPressed)
    {
        // Jika tombol ditekan, statusnya dibalik dari kondisi awalnya
        if (startActive)
        {
            gameObject.SetActive(!isPressed); // Hilang saat ditekan
        }
        else
        {
            gameObject.SetActive(isPressed);  // Muncul saat ditekan
        }
    }
}