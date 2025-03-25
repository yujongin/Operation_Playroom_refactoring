using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class ServerGameManager : IDisposable
{
    string serverIp;
    ushort serverPort;
    ushort queryPort;
    MultiplayAllocationService multiplayAllocationService;
    MatchplayBackfiller backfiller;

    public ServerGameManager(string serverIp, ushort serverPort, ushort queryPort, NetworkManager manager)
    {
        this.serverIp = serverIp;
        this.serverPort = serverPort;
        this.queryPort = queryPort;
        multiplayAllocationService = new MultiplayAllocationService();
    }   

    public async Task StartGameServerAsync()
    {
        await multiplayAllocationService.BeginServerCheck();

        try
        {
            MatchmakingResults matchmakerPayload = await GetMatchmakerPayload();

            if (matchmakerPayload != null)
            {
                await StartBackfill(matchmakerPayload);
                ServerSingleton.Instance.OnUserJoined -= UserJoined;
                ServerSingleton.Instance.OnUserLeft -= UserLeft;
                ServerSingleton.Instance.OnUserJoined += UserJoined;
                ServerSingleton.Instance.OnUserLeft += UserLeft;
            }
            else
            {
                Debug.LogWarning("matchmaker payload timed out");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        if (!ServerSingleton.Instance.OpenConnection(serverIp, serverPort))
        {
            Debug.LogWarning("Network Server not started");
            return;
        }

        // TODO: 서버가 정상적으로 작동할 경우, 로드할 씬 이름 변경하기
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene",
            UnityEngine.SceneManagement.LoadSceneMode.Single);

        Debug.Log("Load Game Scene");
    }

    async Task StartBackfill(MatchmakingResults payload)
    {
        backfiller = new MatchplayBackfiller(
            serverIp + ":" + serverPort,
            payload.QueueName,
            payload.MatchProperties,
            10);

        if (backfiller.NeedsPlayers())
        {
            await backfiller.BeginBackfilling();
        }
    }

    async void CloseServer()
    {
        await backfiller.StopBackfill();
        Dispose();
        Application.Quit();
    }

    void UserJoined(UserData user)
    {
        backfiller.AddPlayerToMatch(user);
        multiplayAllocationService.AddPlayer();
        if (!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
        {
            _ = backfiller.StopBackfill();
        }
    }

    void UserLeft(UserData user)
    {
        int playerCount = backfiller.RemovePlayerFromMatch(user.userAuthId);
        multiplayAllocationService.RemovePlayer();

        if (playerCount<=0)
        {
            CloseServer();
            return;
        }

        if (backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
        {
            _ = backfiller.BeginBackfilling();
        }
    }

    async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        Task<MatchmakingResults> matchmakerPayloadTask =
            multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }

        return null;
    }

    public void Dispose()
    {
        ServerSingleton.Instance.OnUserJoined -= UserJoined;
        ServerSingleton.Instance.OnUserLeft -= UserLeft;

        backfiller?.Dispose();
        multiplayAllocationService?.Dispose();
    }
}
