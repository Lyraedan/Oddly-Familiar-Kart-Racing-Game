using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;
using Steamworks;

public class SteamP2PRelayTransport : NetworkTransport
{
    private Dictionary<ulong, HSteamNetConnection> connectedClients = new();

    private bool steamInitialized = false;
    private CSteamID localSteamId;

    private Callback<SteamNetConnectionStatusChangedCallback_t> connectionStatusChangedCallback;

    private HSteamNetPollGroup pollGroup;
    private HSteamListenSocket listenSocket;
    public ulong HostSteamId = 0; // <-- Must be set to the ID of the host before StartClient, Can be recieved from a Lobby

    public override ulong ServerClientId => HostSteamId;
    public ulong LocalID => localSteamId.IsValid() ? localSteamId.m_SteamID : 0;

    public bool isHost = false;
    public bool isClient = false;

    private CSteamID lobbyId;
    private Callback<LobbyCreated_t> lobbyCreatedCallback;

    public override void Initialize(NetworkManager networkManager = null)
    {
        if (!SteamAPI.Init())
        {
            Debug.LogError("[SteamTransport] SteamAPI failed to init.");
            return;
        }

        steamInitialized = true;
        localSteamId = SteamUser.GetSteamID();
        Debug.Log($"[SteamTransport] Steam Initialized: {localSteamId}");

        // Register callback
        connectionStatusChangedCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);
        lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
    }

    public override bool StartServer()
    {
        if (!steamInitialized) return false;
        SteamAPICall_t handle = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
        Debug.Log("[SteamTransport] Lobby creation requested...");
        return true;
    }

    public override bool StartClient()
    {
        if (!steamInitialized)
        {
            Debug.LogError("[SteamTransport] Steam not initialized!");
            return false;
        }

        HostSteamId = ulong.Parse(SteamMatchmaking.GetLobbyData(lobbyId, "host"));
        if (HostSteamId == 0)
        {
            Debug.LogError("[SteamTransport] HostSteamId not set! Cannot start client.");
            return false;
        }

        try
        {
            var identity = new SteamNetworkingIdentity();
            identity.SetSteamID64(HostSteamId);

            HSteamNetConnection conn = SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, null);
            if (conn.m_HSteamNetConnection == 0)
            {
                Debug.LogError("[SteamTransport] Failed to create P2P connection to host.");
                return false;
            }

            connectedClients[HostSteamId] = conn;

            Debug.Log($"[SteamTransport] Client started, connecting to host {HostSteamId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SteamTransport] Exception starting client: {ex}");
            return false;
        }
        isClient = true;
    }

    public override void Shutdown()
    {
        DisconnectLocalClient();
        LeaveLobby();

        if (pollGroup.m_HSteamNetPollGroup != 0)
        {
            SteamNetworkingSockets.DestroyPollGroup(pollGroup);
            pollGroup = default;
        }

        if (listenSocket.m_HSteamListenSocket != 0)
        {
            SteamNetworkingSockets.CloseListenSocket(listenSocket);
            listenSocket = default;
        }

        isHost = false;
        isClient = false;
        SteamAPI.Shutdown();
        steamInitialized = false;
        connectedClients.Clear();
    }

    public override void DisconnectLocalClient()
    {
        foreach (var conn in connectedClients.Values)
        {
            SteamNetworkingSockets.CloseConnection(conn, 0, "Server shutdown", false);
        }
        connectedClients.Clear();
        HostSteamId = 0;
    }

    public override void DisconnectRemoteClient(ulong clientId)
    {
        if (connectedClients.TryGetValue(clientId, out HSteamNetConnection conn))
        {
            SteamNetworkingSockets.CloseConnection(conn, 0, "Disconnected by server", false);
            connectedClients.Remove(clientId);
        }
    }

    public override ulong GetCurrentRtt(ulong clientId)
    {
        if (!connectedClients.TryGetValue(clientId, out HSteamNetConnection conn))
            return 0;

        SteamNetConnectionRealTimeStatus_t status = default;
        SteamNetConnectionRealTimeLaneStatus_t laneStatus = default;
        var result = SteamNetworkingSockets.GetConnectionRealTimeStatus(conn, ref status, 0, ref laneStatus);

        if (result != EResult.k_EResultOK)
            return 0;

        return (ulong)status.m_nPing;
    }

    public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
    {
        if (!connectedClients.TryGetValue(clientId, out HSteamNetConnection conn))
            return;

        EP2PSend sendType = networkDelivery switch
        {
            NetworkDelivery.Reliable => EP2PSend.k_EP2PSendReliable,
            NetworkDelivery.ReliableSequenced => EP2PSend.k_EP2PSendReliable,
            NetworkDelivery.ReliableFragmentedSequenced => EP2PSend.k_EP2PSendReliable,
            NetworkDelivery.Unreliable => EP2PSend.k_EP2PSendUnreliable,
            NetworkDelivery.UnreliableSequenced => EP2PSend.k_EP2PSendUnreliable,
            _ => EP2PSend.k_EP2PSendReliable,
        };

        SteamNetworkingSockets.SendMessageToConnection(conn, payload.Array, payload.Count, sendType);
    }

    public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
    {
        clientId = 0;
        payload = default;
        receiveTime = 0f;

        if (!steamInitialized)
            return NetworkEvent.Nothing;

        int maxMessages = 32;
        var messages = new IntPtr[maxMessages];

        foreach (var kvp in connectedClients)
        {
            int msgCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(kvp.Value, messages, maxMessages);
            for (int i = 0; i < msgCount; i++)
            {
                var msg = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messages[i]);
                byte[] data = new byte[msg.m_cbSize];
                Marshal.Copy(msg.m_pData, data, 0, msg.m_cbSize);

                clientId = kvp.Key;
                payload = new ArraySegment<byte>(data);
                receiveTime = Time.time;

                SteamNetworkingMessage_t.Release(messages[i]);
                return NetworkEvent.Data;
            }
        }

        return NetworkEvent.Nothing;
    }

    private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t data)
    {
        var conn = data.m_hConn;
        var remoteId = data.m_info.m_identityRemote.GetSteamID();
        ulong clientId = remoteId.m_SteamID;
        var state = data.m_info.m_eState;

        switch (state)
        {
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                SteamNetworkingSockets.AcceptConnection(conn);
                SteamNetworkingSockets.SetConnectionPollGroup(conn, pollGroup);
                break;

            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                if (!connectedClients.ContainsKey(clientId))
                {
                    connectedClients[clientId] = conn;
                    Debug.Log($"[SteamTransport] Client connected: {remoteId}");
                }
                break;

            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
                if (connectedClients.ContainsKey(clientId))
                {
                    connectedClients.Remove(clientId);
                    Debug.Log($"[SteamTransport] Client disconnected: {remoteId}");
                }
                SteamNetworkingSockets.CloseConnection(conn, 0, "Closed", false);
                break;
        }
    }

    private void OnLobbyCreated(LobbyCreated_t result)
    {
        if (result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("[SteamTransport] Failed to create lobby.");
            return;
        }

        lobbyId = new CSteamID(result.m_ulSteamIDLobby);
        HostSteamId = (ulong)SteamUser.GetSteamID();
        Debug.Log($"[SteamTransport] Lobby created: {lobbyId}, Host: {HostSteamId}");

        SteamMatchmaking.SetLobbyData(lobbyId, "host", HostSteamId.ToString());
        Debug.Log($"[SteamTransport] Lobby host set to {HostSteamId}");

        if (!StartNetworkingServer())
        {
            Debug.LogError("[SteamTransport] Failed to start networking server after lobby creation.");
            return;
        }

        isHost = true;
        Debug.Log("[SteamTransport] Server fully started with lobby and networking.");
    }

    private bool StartNetworkingServer()
    {
        pollGroup = SteamNetworkingSockets.CreatePollGroup();
        if (pollGroup.m_HSteamNetPollGroup == 0)
        {
            Debug.LogError("[SteamTransport] Failed to create poll group!");
            return false;
        }

        listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, null);
        if (listenSocket.m_HSteamListenSocket == 0)
        {
            Debug.LogError("[SteamTransport] Failed to create listen socket!");
            return false;
        }

        Debug.Log("[SteamTransport] Networking server initialized successfully.");
        return true;
    }

    private void LeaveLobby()
    {
        if (!lobbyId.IsValid())
            return;

        // Only the host should mark it private to prevent new joins
        if (isHost)
        {
            SteamMatchmaking.SetLobbyType(lobbyId, ELobbyType.k_ELobbyTypePrivate);
            Debug.Log("[SteamTransport] Lobby set to private before leaving.");
        }

        Debug.Log($"[SteamTransport] Leaving lobby {lobbyId}");
        SteamMatchmaking.LeaveLobby(lobbyId);
        lobbyId = default;
    }
}