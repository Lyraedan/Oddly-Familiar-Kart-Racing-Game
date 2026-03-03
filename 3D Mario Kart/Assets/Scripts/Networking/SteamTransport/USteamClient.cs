using UnityEngine;
using Steamworks;
using UnityEditor.MemoryProfiler;

public class USteamClient : MonoBehaviour
{
    /// <summary>
    /// The Steam App ID.
    /// Use your own AppID here; 480 works for development/testing.
    /// </summary>
    public uint steamAppId = 480;

    private bool steamInitialized = false;

    public static HSteamNetConnection? Connection { get; private set; }
    private static SteamNetConnectionRealTimeStatus_t? connectionHealth = null;

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (!SteamAPI.RestartAppIfNecessary((AppId_t)steamAppId))
        {
            Debug.LogError("SteamAPI RestartAppIfNecessary failed.");
            return;
        }

        steamInitialized = SteamAPI.Init();
        if (!steamInitialized)
        {
            Debug.LogError("SteamAPI.Init() failed. Make sure Steam is running and steam_appid.txt is present.");
            return;
        }

        Debug.Log("Steam initialized!");
    }

    void Update()
    {
        if (steamInitialized)
        {
            SteamAPI.RunCallbacks();
            connectionHealth = QueryConnectionHealth();
        }
    }

    void OnApplicationQuit()
    {
        if (steamInitialized)
        {
            SteamAPI.Shutdown();
            Debug.Log("Steam API shutdown.");
        }
    }

    public static SteamNetConnectionRealTimeStatus_t? QueryConnectionHealth()
    {
        if (Connection.HasValue)
        {
            SteamNetConnectionRealTimeStatus_t status = default;
            SteamNetConnectionRealTimeLaneStatus_t laneStatus = default;

            EResult res = SteamNetworkingSockets.GetConnectionRealTimeStatus(
                    Connection.Value,
                    ref status,
                    0,
                    ref laneStatus
            );

            if (res == EResult.k_EResultOK)
            {
                return status;
            }
        }
        return null;
    }

    #region Connection Health
    // 1 is good, 0.85 is degraded, 0.7 is bad, packet quality locally
    public static float GetLocalPacketQuality()
    {
        if (!connectionHealth.HasValue)
            return 0f;

        return connectionHealth.Value.m_flConnectionQualityLocal;
    }

    // 1 is good, 0.85 is degraded, 0.7 is bad, packet quality from the host
    public static float GetRemotePacketQuality()
    {
        if (!connectionHealth.HasValue)
            return 0f;

        return connectionHealth.Value.m_flConnectionQualityRemote;
    }

    public static int GetPingToHost()
    {
        if (!connectionHealth.HasValue)
            return -1;

        return connectionHealth.Value.m_nPing;
    }

    public static int GetUnackedReliable()
    {
        if (!connectionHealth.HasValue)
            return -1;

        return connectionHealth.Value.m_cbSentUnackedReliable;
    }

    public static int GetPendingUnreliable()
    {
        if (!connectionHealth.HasValue)
            return -1;

        return connectionHealth.Value.m_cbPendingUnreliable;
    }

    public static long GetUsecQueueTime()
    {
        if (!connectionHealth.HasValue)
            return -1;

        return (long)connectionHealth.Value.m_usecQueueTime;
    }
    #endregion
}