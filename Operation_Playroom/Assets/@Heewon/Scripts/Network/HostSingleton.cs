using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using WebSocketSharp;

public class HostSingleton : MonoBehaviour
{
    static HostSingleton instance;

    public static HostSingleton Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject singletonObject = new GameObject("HostSingleton");
                instance = singletonObject.AddComponent<HostSingleton>();

                DontDestroyOnLoad(singletonObject);
            }
            return instance;
        }
    }

    const int MAX_CONNECTIONS = 6;
    Allocation allocation;
    public string joinCode;
    public string lobbyId;

    public string LobbyId
    {
        get { return lobbyId; }
    }

    public async Task UpdateLobbyHost(string newHostAuthId)
    {
        try
        {
            UpdateLobbyOptions op = new UpdateLobbyOptions
            {
                HostId = newHostAuthId
            };

            await LobbyService.Instance.UpdateLobbyAsync(lobbyId, op);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task StartHostAsync(CreateLobbyOptions options = null, string lobbyName = "")
    {
        // 릴레이 접속
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MAX_CONNECTIONS);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        // 로비 만들기
        try
        {
            if (options == null)
            {
                options = new CreateLobbyOptions();
                options.IsPrivate = false;
            }
            options.Data = new Dictionary<string, DataObject>
            {
                {
                    "JoinCode",
                    new DataObject(DataObject.VisibilityOptions.Member, joinCode)
                }
            };

            if (lobbyName.Length == 0)
            {
                lobbyName = joinCode;
            }

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_CONNECTIONS, options);
            lobbyId = lobby.Id;

            StartCoroutine(HeartBeatLobby(15f));
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
            return;
        }

        // --- 여기까지 로비 ---

        ServerSingleton.Instance.Init();

        // make payload
        UserData userData = new UserData()
        {
            userName = AuthenticationService.Instance.PlayerName ?? "Unknown",
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonConvert.SerializeObject(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        ServerSingleton.Instance.OnClientLeft -= HandleClientLeft;
        ServerSingleton.Instance.OnClientLeft += HandleClientLeft;

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private async void HandleClientLeft(string authId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, authId);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    IEnumerator HeartBeatLobby(float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }


    public async void ShutDown()
    {
        ServerSingleton.Instance.OnClientLeft -= HandleClientLeft;
        StopAllCoroutines();

        if (!lobbyId.IsNullOrEmpty())
        {
            try
            {
                var netObjs = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
                foreach (var obj in netObjs)
                {
                    if (obj.IsSpawned)
                    {
                        obj.Despawn();
                    }
                }

                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
            }
            lobbyId = null;
        }

    }
}
