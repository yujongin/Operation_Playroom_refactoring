using System.Threading.Tasks;
using Unity.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{
    ApplicationData appData;

    async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(MultiplayerRolesManager.ActiveMultiplayerRoleMask == MultiplayerRoleFlags.Server);
    }

    async Task LaunchInMode(bool isDedicatedServer)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        Managers.Resource.LoadAllAsync<Object>("default", (name, loadCount, totalCount) =>
        {
            if (loadCount >= totalCount)
            {
                Debug.Log($"({loadCount}/{totalCount})");
                Debug.Log("addressables load complete");
                Managers.Data.Init();
                tcs.SetResult(true);
            }
        });

        await tcs.Task;

        ServerSingleton.Instance.Init();

        float timer = 10f;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if (ServerSingleton.Instance.gameRoleToPrefabHash.Count >= 3)
            {
                break;
            }
        }

        if (isDedicatedServer)
        {
            appData = new ApplicationData();
            
            await ServerSingleton.Instance.CreateServer();
            await ServerSingleton.Instance.serverManager.StartGameServerAsync();
        }
        else
        {
            bool authenticated = await ClientSingleton.Instance.InitAsync();

            HostSingleton hostSingleton = HostSingleton.Instance;

            if (authenticated)
            {
                GotoMenu();
            }
            else
            {
                //TODO: 로그인 실패했을 경우 재시도
            }
        }
    }

    public void GotoMenu()
    {
        SceneManager.LoadScene("MainScene");
    }
}
