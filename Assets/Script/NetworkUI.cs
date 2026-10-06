using UnityEngine;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost(); // Membuka room sekaligus main
        gameObject.SetActive(false); // Menyembunyikan tombol
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient(); // Masuk ke room yang sudah ada
        gameObject.SetActive(false);
    }
}