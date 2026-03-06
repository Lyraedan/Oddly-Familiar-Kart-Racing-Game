using System;
using System.Collections;
using System.Collections.Generic;
using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerMenu : MonoBehaviour
{
    public MainMenu mainMenu;
    public CanvasGroup MultiplayerMenuButtons;
    public CanvasGroup FindGroup;
    public CanvasGroup Lobby;

    [Space(10)]

    public GameObject LobbyMemberPrefab;
    public GameObject BrowserEntryPrefab;

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
        USteamClient.Instance.SearchLobbies();
        mainMenu.UpdateMenu(MainMenu.MenuState.Multiplayer_FindLobby);
    }

    private void HandleLobbyMembersUpdated(List<CSteamID> list)
    {

    }

    private void HandleLobbyListReceived(List<CSteamID> list)
    {

    }
}
