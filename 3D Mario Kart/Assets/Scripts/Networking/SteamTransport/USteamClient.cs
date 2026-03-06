using Steamworks;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class USteamClient : MonoBehaviour
{
    public static USteamClient Instance { get; private set; }

    [Header("Steam Settings")]
    [SerializeField] private uint steamAppId = 480;

    public uint SteamAppId => steamAppId;
    public bool IsInitialized { get; private set; }
    public CSteamID CurrentLobbyId { get; private set; }

    public List<CSteamID> LobbyMembers { get; private set; } = new List<CSteamID>();

    // Events
    public event Action OnSteamInitialized;
    public event Action<CSteamID> OnLobbyCreated;
    public event Action<CSteamID> OnLobbyJoined;
    public event Action OnLobbyLeft;
    public event Action<List<CSteamID>> OnLobbyListReceived;
    public event Action<List<CSteamID>> OnLobbyMembersUpdated;

    private Callback<LobbyCreated_t> _lobbyCreated;
    private Callback<LobbyEnter_t> _lobbyEntered;
    private Callback<LobbyMatchList_t> _lobbyMatchList;
    private Callback<GameLobbyJoinRequested_t> _joinRequested;

    private bool _steamInitializedInternally;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSteam();
    }

    private void InitializeSteam()
    {
        if (IsInitialized)
            return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        WriteSteamAppIdFile();
#endif

        try
        {
            if (!SteamAPI.Init())
            {
                Debug.LogError("SteamAPI.Init() failed.");
                return;
            }

            _steamInitializedInternally = true;
            IsInitialized = true;

            RegisterCallbacks();

            Debug.Log($"Steam initialized. AppId: {steamAppId}");
            OnSteamInitialized?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Steam initialization exception: {e}");
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void WriteSteamAppIdFile()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "steam_appid.txt");
        File.WriteAllText(path, steamAppId.ToString());
    }
#endif

    private void RegisterCallbacks()
    {
        _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreatedCallback);
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEnteredCallback);
        _lobbyMatchList = Callback<LobbyMatchList_t>.Create(OnLobbyMatchListCallback);
        _joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequestedCallback);
    }

    private void Update()
    {
        if (IsInitialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    private void OnDestroy()
    {
        if (_steamInitializedInternally)
        {
            SteamAPI.Shutdown();
        }
    }

    #region Lobby API

    public void CreatePublicLobby()
    {
        CreateLobby(ELobbyType.k_ELobbyTypePublic, 12);
    }

    public void CreateFriendsOnlyLobby()
    {
        CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 12);
    }

    public void CreateInvisibleLobby()
    {
        CreateLobby(ELobbyType.k_ELobbyTypeInvisible, 12);
    }

    public void CreatePrivateLobby()
    {
        CreateLobby(ELobbyType.k_ELobbyTypePrivate, 12);
    }

    public void CreatePrivateUniqueLobby()
    {
        CreateLobby(ELobbyType.k_ELobbyTypePrivateUnique, 12);
    }

    public void CreateLobby(ELobbyType type, int maxMembers)
    {
        SteamMatchmaking.CreateLobby(type, maxMembers);
    }

    public void SearchLobbies()
    {
        SteamMatchmaking.RequestLobbyList();
    }

    public void JoinLobby(CSteamID lobbyId)
    {
        SteamMatchmaking.JoinLobby(lobbyId);
    }

    public void LeaveLobby()
    {
        if (CurrentLobbyId.IsValid())
        {
            SteamMatchmaking.LeaveLobby(CurrentLobbyId);
            CurrentLobbyId = CSteamID.Nil;
            LobbyMembers.Clear();
            OnLobbyLeft?.Invoke();
        }
    }

    private void UpdateLobbyMembers()
    {
        if (!CurrentLobbyId.IsValid())
            return;

        LobbyMembers.Clear();
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(CurrentLobbyId);

        for (int i = 0; i < memberCount; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobbyId, i);
            LobbyMembers.Add(memberId);
        }

        OnLobbyMembersUpdated?.Invoke(new List<CSteamID>(LobbyMembers));
    }

    #endregion

    #region Steam Callbacks

    private void OnLobbyCreatedCallback(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Lobby creation failed.");
            return;
        }

        CurrentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        UpdateLobbyMembers();
        OnLobbyCreated?.Invoke(CurrentLobbyId);
    }

    private void OnLobbyEnteredCallback(LobbyEnter_t callback)
    {
        CurrentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        UpdateLobbyMembers();
        OnLobbyJoined?.Invoke(CurrentLobbyId);
    }

    private void OnLobbyMatchListCallback(LobbyMatchList_t callback)
    {
        var lobbyIds = new List<CSteamID>();

        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            lobbyIds.Add(SteamMatchmaking.GetLobbyByIndex(i));
        }

        OnLobbyListReceived?.Invoke(lobbyIds);
    }

    private void OnJoinRequestedCallback(GameLobbyJoinRequested_t callback)
    {
        JoinLobby(callback.m_steamIDLobby);
    }

    #endregion
}