using System;
using System.Collections;
using System.Collections.Generic;
using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class MultiplayerMenu : MonoBehaviour
{
    // Seperate from the MainMenu because it is wired into the USteamClient

    public MainMenu mainMenu;
    public CanvasGroup MultiplayerMenuButtons;
    public CanvasGroup FindGroup;
    public CanvasGroup Lobby;

    [Space(10)]

    public GameObject LobbyMemberPrefab;
    public GameObject BrowserEntryPrefab;

    [Space(10)]
    public Transform ServerBrowserContent; // Content of the server browser scroll pane
    public Transform LobbyContent; // Content of the lobby scroll pane

    [Space(10)]
    public GameObject NoLobbiesText;

    void Start()
    {
        USteamClient.Instance.OnLobbyCreated += HandleLobbyCreated;
        USteamClient.Instance.OnLobbyJoined += HandleLobbyJoined;
        USteamClient.Instance.OnLobbyLeft += HandleLobbyLeft;
        USteamClient.Instance.OnLobbyListReceived += HandleLobbyListReceived;
        USteamClient.Instance.OnLobbyMembersUpdated += HandleLobbyMembersUpdated;
    }

    private void HandleLobbyCreated(CSteamID id)
    {
        StartCoroutine(OnLobbyCreated(id));
    }

    private IEnumerator OnLobbyCreated(CSteamID id)
    {
        yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(MultiplayerMenuButtons, 0f, 0.1f));
        yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(Lobby, 1f, 0.5f));
        mainMenu.UpdateMenu(MainMenu.MenuState.Multiplayer_Lobby);
    }

    private void HandleLobbyJoined(CSteamID id)
    {
        StartCoroutine(OnLobbyJoined(id));
    }

    IEnumerator OnLobbyJoined(CSteamID id)
    {
        yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(FindGroup, 0f, 0.1f));
        yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(Lobby, 1f, 0.5f));
        mainMenu.UpdateLobbyHost(id.m_SteamID);
        mainMenu.UpdateMenu(MainMenu.MenuState.Multiplayer_Lobby);
    }

    private void HandleLobbyLeft()
    {
        StartCoroutine(OnLobbyLeft());
    }

    IEnumerator OnLobbyLeft()
    {
        yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(Lobby, 0f, 0.5f));
        yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(MultiplayerMenuButtons, 1f, 0.1f));
    }

    public void RequestSearchLobby()
    {
        StartCoroutine(OnSearchLobby());
    }

    IEnumerator OnSearchLobby()
    {
        yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(MultiplayerMenuButtons, 0f, 0.5f));
        yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(FindGroup, 1f, 0.1f));
        RefreshServerBrowser();
        mainMenu.UpdateMenu(MainMenu.MenuState.Multiplayer_FindLobby);
    }

    private void HandleLobbyMembersUpdated(List<CSteamID> list)
    {
        ClearMemberEntries();

        CSteamID lobbyId = USteamClient.Instance.CurrentLobbyId;
        CSteamID hostId = SteamMatchmaking.GetLobbyOwner(lobbyId);

        foreach (var member in list)
        {
            for (int i = 0; i < 12; i++)
            {
                GameObject entry = SpawnMemberEntry();

                //SteamFriends.RequestUserInformation(member, true); // TODO Implement later for public lobbies
                string name = SteamFriends.GetFriendPersonaName(member);

                bool isHost = member == hostId;


                LobbyMemberEntry memberEntry = entry.GetComponent<LobbyMemberEntry>();
                memberEntry.UpdateEntry(name, member.m_SteamID, isHost);
                memberEntry.RequestAvatar(member);
            }
        }
    }

    private void HandleLobbyListReceived(List<CSteamID> list)
    {
        NoLobbiesText.SetActive(list.Count == 0);

        foreach (var lobby in list)
        {
            GameObject entry = SpawnServerEntry();

            string hostId = SteamMatchmaking.GetLobbyData(lobby, "host_id");

            // Ping
            string locationString = SteamMatchmaking.GetLobbyData(lobby, "ping_location");
            SteamNetworkPingLocation_t remoteLocation = new SteamNetworkPingLocation_t();
            SteamNetworkingUtils.ParsePingLocationString(locationString, out remoteLocation);
            int ping = SteamNetworkingUtils.EstimatePingTimeFromLocalHost(ref remoteLocation);

            string lobbyName = SteamMatchmaking.GetLobbyData(lobby, "lobby_name");
            int playerCount = SteamMatchmaking.GetNumLobbyMembers(lobby);
            int maxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobby);

            ServerEntry details = entry.GetComponent<ServerEntry>();
            details.UpdateEntry(lobbyName, playerCount, maxPlayers, ping);
        }
    }

    public void RefreshServerBrowser()
    {
        ClearServerEntries();
        USteamClient.Instance.SearchLobbies();
    }

    private void ClearServerEntries()
    {
        // First index is the "No lobbies found" text, so we start at 1
        for (int i = 1; i < ServerBrowserContent.childCount; i++)
        {
            Destroy(ServerBrowserContent.GetChild(i).gameObject);
        }
    }

    private void ClearMemberEntries()
    {
        for (int i = LobbyContent.childCount - 1; i >= 0; i--)
        {
            Destroy(LobbyContent.GetChild(i).gameObject);
        }
    }

    public GameObject SpawnServerEntry()
    {
        GameObject entry = Instantiate(BrowserEntryPrefab);
        entry.transform.SetParent(ServerBrowserContent, false);
        entry.transform.localScale = Vector3.one;
        return entry;
    }

    public GameObject SpawnMemberEntry()
    {
        GameObject entry = Instantiate(LobbyMemberPrefab);
        entry.transform.SetParent(LobbyContent, false);
        entry.transform.localScale = Vector3.one;
        return entry;
    }
}
