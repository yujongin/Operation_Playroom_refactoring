using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName
{
    MainScene,
    LoadingScene,
    GameScene
}

public class ClientSingleton : MonoBehaviour
{
    static ClientSingleton instance;
    MatchplayMatchmaker matchmaker;
    UserData userData;

    public UserData UserData
    {
        get { return userData; }
    }

    string disconnectScene = "MainScene";

    public static ClientSingleton Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject singletonObject = new GameObject("ClientSingleton");
                instance = singletonObject.AddComponent<ClientSingleton>();

                DontDestroyOnLoad(singletonObject);
            }
            return instance;
        }
    }

    JoinAllocation allocation;

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        AuthState state = await Authenticator.DoAuth();

        matchmaker = new MatchplayMatchmaker();

        if (state == AuthState.Authenticated)
        {
            userData = new UserData()
            {
                userName = AuthenticationService.Instance.PlayerName ?? "Unknown",
                userAuthId = AuthenticationService.Instance.PlayerId
            };

            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
            return true;
        }

        return false;
    }

    public void SetDisconnectScene(string sceneName)
    {
        Debug.Log($"Set Disconnect Scene : {sceneName}");
        disconnectScene = sceneName;
    }

    private void OnDisconnected(ulong clientId)
    {
        if (clientId != 0 && clientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name != disconnectScene)
        {
            SceneManager.LoadScene(disconnectScene);
        }
    }

    public async Task<bool> StartClientAsync(string joinCode)
    {
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            return false;
        }

        UnityTransport transport= NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        ConnectClient();
        return true;
    }

    public async void MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakeResponse)
    {
        if (matchmaker.IsMatchmaking)
        {
            return;
        }

        MatchmakerPollingResult result = await GetMatchAsync();
        onMatchmakeResponse?.Invoke(result);
    }

    public async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        MatchmakingResult result = await matchmaker.Matchmake(userData);
        Debug.Log(result.resultMessage);

        if (result.result == MatchmakerPollingResult.Success)
        {
            StartClient(result.ip,(ushort)result.port);
        }

        return result.result;
    }

    public async void StartClient(string ip, ushort port)
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            disconnectScene = SceneName.LoadingScene.ToString();
            NetworkManager.Singleton.Shutdown();

            int timer = 10000; // 10초
            while (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
            {
                await Task.Delay(500);
                timer -= 500;

                if (timer <= 0) break;
            }
        }

        disconnectScene = SceneName.MainScene.ToString();

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, port);

        ConnectClient();
    }

    void ConnectClient()
    {
        // payload 만들기

        string payload = JsonConvert.SerializeObject(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();

        Debug.Log($"ID : {userData.userAuthId} Team : {userData.userGamePreferences.gameTeam} Role : {userData.userGamePreferences.gameRole}");
    }

    public async Task CancelMatchmaking()
    {
        await matchmaker.CancelMatchmaking();
    }
}
