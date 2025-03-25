using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerSingleton : MonoBehaviour
{
    static ServerSingleton instance;

    public Dictionary<ulong, UserData> clientIdToUserData = new Dictionary<ulong, UserData>();
    public Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();
    public Dictionary<GameRole, uint> gameRoleToPrefabHash = new Dictionary<GameRole, uint>();

    public Action<string> OnClientLeft;

    public Action<UserData> OnUserJoined;
    public Action<UserData> OnUserLeft;

    public ServerGameManager serverManager;

    public static ServerSingleton Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject singletonObject = new GameObject("ServerSingleton");
                instance = singletonObject.AddComponent<ServerSingleton>();

                DontDestroyOnLoad(singletonObject);
            }
            return instance;
        }
    }

    public void Init()
    {
        SpawnPoint.Clear();
        LoadPrefabHashes();
    }

    void LoadPrefabHashes()
    {
        if (gameRoleToPrefabHash.Count > 0)
        {
            return;
        }

        uint kingPrefabHash = Managers.Resource.Load<GameObject>("King").GetComponent<NetworkObject>().PrefabIdHash;
        uint archerPrefabHash = Managers.Resource.Load<GameObject>("Archer").GetComponent<NetworkObject>().PrefabIdHash;
        uint swordmanPrefabHash = Managers.Resource.Load<GameObject>("Swordman").GetComponent<NetworkObject>().PrefabIdHash;

        gameRoleToPrefabHash.Add(GameRole.King, kingPrefabHash);
        gameRoleToPrefabHash.Add(GameRole.Archer, archerPrefabHash);
        gameRoleToPrefabHash.Add(GameRole.Swordman, swordmanPrefabHash);
    }

    void OnEnable()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }


    void OnDisable()
    {
        if (NetworkManager.Singleton!=null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }


    public async Task CreateServer()
    {
        await UnityServices.InitializeAsync();

        serverManager = new ServerGameManager(
                ApplicationData.IP(),
                (ushort)ApplicationData.Port(),
                (ushort)ApplicationData.QPort(),
                NetworkManager.Singleton
            );
    }

    public bool OpenConnection(string ip, ushort port)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, port);

        return NetworkManager.Singleton.StartServer();
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request, 
        NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonConvert.DeserializeObject<UserData>(payload);
        Debug.Log($"User Data : {userData.userName}");

        clientIdToUserData[request.ClientNetworkId] = userData;
        authIdToUserData[userData.userAuthId] = userData;

        OnUserJoined?.Invoke(userData);

        Debug.Log($"Id : {userData.userAuthId}, preference : {userData.userGamePreferences}");

        response.Approved = true;
        //response.CreatePlayerObject = false;

        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            response.CreatePlayerObject = true;
            response.PlayerPrefabHash = gameRoleToPrefabHash[userData.userGamePreferences.gameRole];
            response.Position = SpawnPoint.GetSpawnPoint(userData.userGamePreferences.gameTeam, userData.userGamePreferences.gameRole); 
            response.Rotation = Quaternion.identity;
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientIdToUserData.ContainsKey(clientId))
        {
            string authId = clientIdToUserData[clientId].userAuthId;

            OnUserLeft?.Invoke(authIdToUserData[authId]);

            clientIdToUserData.Remove(clientId);

            authIdToUserData.Remove(authId);

            OnClientLeft?.Invoke(authId);
        }
    }

}
