using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI lobbyTitleTmp;
    [SerializeField] TextMeshProUGUI lobbyPlayersTmp;
    [SerializeField] GameObject lockImage;

    LobbyList lobbyList;
    Lobby lobby;

    public void SetItem(LobbyList lobbyList, Lobby lobby)
    {
        this.lobbyList = lobbyList;
        this.lobby = lobby;

        if (lobby.HasPassword)
        {
            lockImage.SetActive(true);
        }
        else
        {
            lockImage.SetActive(false);
        }

        lobbyTitleTmp.text = lobby.Name;
        lobbyPlayersTmp.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
    }

    public void JoinPressed()
    {
        bool needPassword = lobby.HasPassword;
        lobbyList.JoinAsync(lobby, needPassword);
    }
}
