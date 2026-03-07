using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerEntry : MonoBehaviour
{
    public TextMeshProUGUI LobbyName;
    public TextMeshProUGUI PlayerCount;
    public TextMeshProUGUI Ping;
    public Button Join;

    public void UpdateEntry(string lobbyName, int playerCount, int maxPlayers, int ping)
    {
        SetLobbyName(lobbyName);
        SetPlayerCount(playerCount, maxPlayers);
        SetPing(ping);
    }

    public void SetLobbyName(string lobbyName)
    {
        LobbyName.text = lobbyName;
    }

    public void SetPlayerCount(int playerCount, int maxPlayers)
    {
        PlayerCount.text = $"{playerCount}/{maxPlayers}";
    }

    public void SetPing(int ping)
    {
        Ping.text = $"{ping} ms";
    }
}
