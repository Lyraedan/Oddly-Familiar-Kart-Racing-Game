using UnityEngine;

public class LobbyMemberEntry : MonoBehaviour
{
    public string PlayerName;
    public bool IsHost;
    public ulong SteamId;

    public void UpdateEntry(string playerName, ulong steamId, bool isHost)
    {
        PlayerName = playerName;
        SteamId = steamId;
        IsHost = isHost;
    }
}
