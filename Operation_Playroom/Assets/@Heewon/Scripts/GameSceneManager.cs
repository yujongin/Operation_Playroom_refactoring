using Unity.Netcode;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.F1))
        //{
        //    Disconnect();
        //}
    }
}
