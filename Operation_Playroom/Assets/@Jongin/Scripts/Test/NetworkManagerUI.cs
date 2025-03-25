using Unity.Netcode;
using UnityEngine;

public class NetworkManagerUI : MonoBehaviour
{
    public void HostPressed()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void ClientPressed()
    {
        NetworkManager.Singleton.StartClient();
    }
}