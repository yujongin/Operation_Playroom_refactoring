using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] TMP_InputField joinCodeField;
    [SerializeField] TMP_InputField userNameField;

    [SerializeField] TMP_Text findMatchStatusText;
    [SerializeField] TMP_Text findButtonText;

    bool isMatchmaking;
    private bool isCancelling;
    void Start()
    {
        if (GameObject.FindFirstObjectByType<NetworkManager>() == null)
        {
            SceneManager.LoadScene("NetConnectScene");
        }
        try
        {
            string userName = AuthenticationService.Instance.PlayerName;
            if (userName.Contains("#"))
            {
                userName = userName.Substring(0, userName.LastIndexOf("#"));
            }
            userNameField.text = userName ?? "";
        }
        catch
        {

        }
    }

    public async void StartHost()
    {
        await HostSingleton.Instance.StartHostAsync();

    }
    public async void StartClient()
    {
        await ClientSingleton.Instance.StartClientAsync(joinCodeField.text);
    }

    public async void ChangeName()
    {
        await AuthenticationService.Instance.UpdatePlayerNameAsync(userNameField.text);
    }

    public async void FindMatchPressed()
    {
        if (isMatchmaking)
        {
            //cancel
            isCancelling = true;
            findMatchStatusText.text = "Cancelling...";

            await ClientSingleton.Instance.CancelMatchmaking();

            isCancelling = false;
            isMatchmaking = false;
            findMatchStatusText.text = "";
            findButtonText.text = "Find Match";
        }
        else
        {
            //match
            findMatchStatusText.text = "Searching...";
            findButtonText.text = "Cancel";
            isMatchmaking = true;
            isCancelling = false;

            ClientSingleton.Instance.MatchmakeAsync(OnMatchMade);
        }
    }

    void OnMatchMade(MatchmakerPollingResult result)
    {
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                findMatchStatusText.text = "connecting...";
                break;
            default:
                isMatchmaking = false;
                findMatchStatusText.text = "error" + result;
                break;
        }
        isMatchmaking = false;
    }
}
