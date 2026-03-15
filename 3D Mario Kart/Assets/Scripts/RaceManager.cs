using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Splines;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.SinglePlayer;
using Netcode.Transports;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [System.Serializable]
    public struct SongDetails
    {
        public string songName;
        public string author;
        public float playbackSpeed;
        public AudioClip clip;
    }

    [Header("Course Config")]
    public string CourseName = "Map Name";
    public string Console = "Unity";
    public string Author = "Your Name Here";
    public Sprite MinimapBackground;
    [Tooltip("The root object containing the track, used to calculate the dimensions for the minimap")]
    public Transform TrackRoot;
    /// <summary>
    /// (-1, 0) means north is west, (1, 0) means north is east, (0, 1) means north is north, (0, -1) means north is south
    /// </summary>
    public Vector2 MinimapOrientation = new Vector2(0f, 1f);
    public bool SpawnComputerRacers = true;

    public SongDetails courseMusic;
    public SongDetails finalLapCourseMusic;
    public AudioSource finalLapSound;

    [HideInInspector]public List<Player> AllPlayers = new List<Player>();

    [Space(25)]
    public GameObject FrontCam;
    [HideInInspector] public GameObject FrontFPCam;
    public GameObject BackCam;
    public RacerSpawn RacerSpawns;
    public Transform AIPathRoot;
    [HideInInspector] public List<PathTool> AIPaths = new List<PathTool>();
    private bool FPCam = false; // First-person camera flag

    public AudioSource music;
    public AudioSource musicFast;
    private bool lastLap = false;

    public int MAXLAPS = 3;

    private float raceTime = 0f;
    private float sortTime = 0f;

    public static bool RACE_STARTED = false;
    public static bool RACE_COMPLETED = false;
    public static bool raceFinishStuff = false;

    public List<LapCounter> lapCounters => IngameUIHolder.Instance.LapCounters;
    [HideInInspector] public List<LapCounter> sortedRacers = new List<LapCounter>();

    [Header("Scene Entry Camera Disables")]
    public List<GameObject> set1 = new();
    public List<GameObject> set2 = new();
    public List<GameObject> set3 = new();

    public TrolleySystem trolleySystem;
    public static Transform allPaths;

    public static float countDownTime = 0;
    private bool startCountDownInternalTimer = false;

    [HideInInspector]
    public int currentBlueShellCount = 0;

    #region Cached UI
    private GameObject CountDownTimer => IngameUIHolder.Instance.CountDownTimer;
    private GameObject CoinCounter => IngameUIHolder.Instance.CoinCounter;
    private GameObject LapCounterUI => IngameUIHolder.Instance.LapCounter;
    private GameObject MiniMap => IngameUIHolder.Instance.MiniMap;
    private GameObject PositionCounter => IngameUIHolder.Instance.PositionCounter;
    private GameObject CourseNameUI => IngameUIHolder.Instance.CourseNameUI.gameObject;
    private GameObject CourseAuthorUI => IngameUIHolder.Instance.CourseAuthorUI.gameObject;

    private GameObject itemSystem => IngameUIHolder.Instance.ItemSystem;
    private ResultsUI resultsUI => IngameUIHolder.Instance.ResultsUI;
    private GameObject finishUI => IngameUIHolder.Instance.FinishUI;

    private GameObject RedShellWarning => IngameUIHolder.Instance.RedShellWarning;
    private GameObject BlueShellWarning => IngameUIHolder.Instance.BlueShellWarning;
    private GameObject StarWarning => IngameUIHolder.Instance.StarWarning;
    private GameObject BulletWarning => IngameUIHolder.Instance.BulletWarning;
    private Transform Canvas => IngameUIHolder.Instance.Canvas.transform;
    #endregion

    public GameObject spectatorSounds;

    [Header("Scene Entry Camera")]
    public Animator sceneEntryCamera;
    public AudioSource sceneEntrySound;

    [HideInInspector] public Player LocalPlayer;
    [HideInInspector] public LapCounter LocalPlayerLap;
    [HideInInspector] public PlayerSounds LocalPlayerSounds;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Application.targetFrameRate = 60;
    }

    void Start()
    {
        // Cache AI paths
        AIPaths = AIPathRoot.GetComponentsInChildren<PathTool>().ToList();
        allPaths = AIPathRoot.transform;

        // Default song names
        if (string.IsNullOrEmpty(courseMusic.songName)) courseMusic.songName = courseMusic.clip.name;
        if (string.IsNullOrEmpty(finalLapCourseMusic.songName)) finalLapCourseMusic.songName = finalLapCourseMusic.clip.name;

        // Setup music playback speed
        IngameUIHolder.Instance.MusicDisplay.ChangeSongSpeed(music, courseMusic.playbackSpeed);
        IngameUIHolder.Instance.MusicDisplay.ChangeSongSpeed(musicFast, finalLapCourseMusic.playbackSpeed);

        music.clip = courseMusic.clip;
        musicFast.clip = finalLapCourseMusic.clip;

        // Update UI
        IngameUIHolder.Instance.UpdateAuthor(Author);
        IngameUIHolder.Instance.UpdateCourse(CourseName, Console);
        IngameUIHolder.Instance.UpdateMinimapBackground(MinimapBackground);

        StartCoroutine(WaitToPlaySceneEntry());
    }

    IEnumerator WaitToPlaySceneEntry()
    {
        // Setup UI for scene entry
        IngameUIHolder.Instance.CourseAuthorUI.alpha = 0;
        IngameUIHolder.Instance.CourseNameUI.alpha = 0;
        yield return UtilityFunctions.FadeCanvasGroup(IngameUIHolder.Instance.WaitingForPlayers, 1f, 0.25f);
        
        // Wait for racers
        yield return new WaitUntil(() => LocalPlayer != null);
        yield return new WaitUntil(() => ReadyToStartGame());
        if(SpawnComputerRacers)
            RacerSpawn.Instance.SpawnComputerRacers(); // Disabled for testing
        IngameUIHolder.Instance.FetchLapCounters();

        // Fade UI
        yield return UtilityFunctions.FadeCanvasGroup(IngameUIHolder.Instance.WaitingForPlayers, 0f, 0.25f);
        IngameUIHolder.Instance.CourseAuthorUI.alpha = 1f;
        IngameUIHolder.Instance.CourseNameUI.alpha = 1f;

        // Start camera sequence
        sceneEntrySound.Play();
        sceneEntryCamera.enabled = true;
    }

    public bool ReadyToStartGame()
    {
        if(NetworkManager.Singleton.NetworkConfig.NetworkTransport is SinglePlayerTransport)
        {
            Debug.Log("Singleplayer mode detected, skipping wait for players.");
            // Singleplayer, don't need to wait
            return true;
        }

        // We are relaying through Steam so we can check lobby members instead of waiting for network objects to spawn
        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is SteamNetworkingSocketsTransport)
        {
            // All players get automatically populated on player spawn so we can just check if we have enough player objects for all lobby members
            return AllPlayers.Count >= USteamClient.Instance.LobbyMembers.Count;
        }

        // Wait for each connected client to have a spawned player
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null || !client.PlayerObject.IsSpawned)
            {
                return false;
            }
        }

        return true;
    }

    public void RegisterLocalPlayer(Player player)
    {
        LocalPlayer = player;
        LocalPlayerLap = player.GetComponent<LapCounter>();
        LocalPlayerSounds = player.GetComponent<PlayerSounds>();
    }

    public void RegisterPlayer(Player player)
    {
        if (!AllPlayers.Contains(player))
            AllPlayers.Add(player);
    }

    public void UnregisterPlayer(Player player)
    {
        if (AllPlayers.Contains(player))
            AllPlayers.Remove(player);
    }

    void Update()
    {
        HandleCountdown();
        HandleRaceTime();
        HandleCameraSwitch();
        CheckFinalLap();
        CheckRaceCompletion();
    }

    private void HandleCountdown()
    {
        if (startCountDownInternalTimer) countDownTime += Time.deltaTime;
    }

    private void HandleRaceTime()
    {
        if (!RACE_STARTED || RACE_COMPLETED) return;

        raceTime += Time.deltaTime;

        if (!music.isPlaying && !lastLap && raceTime > 0.5f)
            IngameUIHolder.Instance.MusicDisplay.DisplayAndPlay(music, courseMusic.author, courseMusic.songName, "Normal");

        sortTime += Time.deltaTime;
        if (sortTime > 0.1f)
        {
            CalculateRacerPosition();
            sortTime = 0f;
        }
    }

    private void HandleCameraSwitch()
    {
        if (!RACE_STARTED || RACE_COMPLETED) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && !PlayerControls.GetButton(PlayerControls.LOOK_BEHIND))
            SwitchToFPCam();
        if (Input.GetKeyDown(KeyCode.Alpha2) && !PlayerControls.GetButton(PlayerControls.LOOK_BEHIND))
            SwitchToFrontCam();
        if (PlayerControls.GetButtonDown(PlayerControls.LOOK_BEHIND))
            SwitchToBackCam();
        if (PlayerControls.GetButtonUp(PlayerControls.LOOK_BEHIND))
            RestoreCamera();
    }

    private void SwitchToFPCam()
    {
        FPCam = true;
        FrontFPCam.SetActive(true);
        FrontCam.GetComponent<Camera>().enabled = false;
    }

    private void SwitchToFrontCam()
    {
        FPCam = false;
        FrontFPCam.SetActive(false);
        FrontCam.GetComponent<Camera>().enabled = true;
    }

    private void SwitchToBackCam()
    {
        BackCam.SetActive(true);
        FrontCam.GetComponent<Camera>().enabled = !FPCam;
        FrontFPCam.SetActive(FPCam ? false : FrontFPCam.activeSelf);
    }

    private void RestoreCamera()
    {
        if (FPCam)
        {
            FrontFPCam.SetActive(true);
            BackCam.SetActive(false);
        }
        else
        {
            BackCam.SetActive(false);
            FrontCam.GetComponent<Camera>().enabled = true;
        }
    }

    private void CheckFinalLap()
    {
        if (LocalPlayerLap != null && LocalPlayerLap.LAPCOUNT == MAXLAPS && !lastLap)
        {
            lastLap = true;
            music.Stop();
            PlayFinalLap();
        }
    }

    private void CheckRaceCompletion()
    {
        if (RACE_COMPLETED && !raceFinishStuff)
        {
            raceFinishStuff = true;
            StartCoroutine(FinishRace());
        }
    }

    private IEnumerator FinishRace()
    {
        LocalPlayerSounds.goal.Play();

        if(spectatorSounds != null)
            spectatorSounds?.SetActive(false);
        
        itemSystem.SetActive(false);

        if(FrontCam != null)
            FrontCam.transform.parent.parent.GetComponent<AudioSource>()?.Stop(); // This is f*cking foul

        finishUI.GetComponent<Animator>().SetBool("Finish", true);

        if(BackCam != null)
            BackCam.SetActive(false);
        if (FrontCam != null)
        {
            FrontCam.SetActive(true);
            FrontCam.GetComponent<Camera>().enabled = true;
        }
        
        if(FrontFPCam != null)
            FrontFPCam?.SetActive(false);

        int playerPos = LocalPlayerLap.Position;

        if (playerPos == 1)
        {
            yield return StartCoroutine(FirstPlaceSequence());
        }
        else if (playerPos <= 6)
        {
            yield return StartCoroutine(MidPlaceSequence());
        }
        else
        {
            yield return StartCoroutine(LoseSequence());
        }

        yield return StartCoroutine(ShowResults(playerPos < 6));
    }

    private IEnumerator FirstPlaceSequence()
    {
        LocalPlayer.Driver.SetBool("FirstPlace", true);

        LocalPlayerSounds.firstPlaceVoice.Play();
        yield return new WaitForSeconds(1f);

        LocalPlayerSounds.firstPlaceResult.Play();
        yield return new WaitForSeconds(2.5f);
    }

    private IEnumerator MidPlaceSequence()
    {
        LocalPlayer.Driver.SetBool("FirstPlace", true); // adjust if needed

        LocalPlayerSounds.firstPlaceVoice.Play();
        yield return new WaitForSeconds(1f);

        LocalPlayerSounds.secondToSixth.Play();
        yield return new WaitForSeconds(2.5f);
    }

    private IEnumerator LoseSequence()
    {
        LocalPlayer.Driver.SetBool("LoseAnim", true);

        LocalPlayerSounds.marioLose.Play();
        yield return new WaitForSeconds(1f);

        LocalPlayerSounds.loseResult.Play();
        yield return new WaitForSeconds(2.5f);
    }

    private IEnumerator ShowResults(bool goodResult)
    {
        FrontCam?.GetComponent<Animator>().SetBool("RaceEndCam", true);
        yield return new WaitForSeconds(0.5f);

        CanvasGroup resultsGroup = resultsUI.GetComponent<CanvasGroup>();

        yield return StartCoroutine(UtilityFunctions.FadeCanvasGroup(resultsGroup, 1f, 0.5f));

        resultsUI.CreateResults();
        yield return new WaitForSeconds(3f);

        if (goodResult)
            LocalPlayerSounds.resultsGood.Play();
        else
            LocalPlayerSounds.resultsBad.Play();
    }

    private void PlayFinalLap() => StartCoroutine(PlayFinalLapEffect());

    private IEnumerator PlayFinalLapEffect()
    {
        finalLapSound?.Play();
        yield return new WaitForSeconds(finalLapSound.clip.length);
        IngameUIHolder.Instance.MusicDisplay.DisplayAndPlay(musicFast, finalLapCourseMusic.author, finalLapCourseMusic.songName, "Faster");
    }

    public IEnumerator CountDownTimerPlay()
    {
        GameObject.Find("FadeInOut")?.GetComponent<Animator>()?.SetTrigger("FadeIn");

        yield return new WaitUntil(() => LocalPlayer != null);
        CoinCounter.SetActive(true);
        LapCounterUI.SetActive(true);
        MiniMap.SetActive(true);

        LocalPlayerSounds.SceneEntryFinished = true;

        FrontCam.SetActive(true);
        FrontCam.GetComponent<Animator>()?.SetTrigger("Entry");
        yield return new WaitForSeconds(0.5f);
        FrontCam.GetComponent<AudioSource>()?.Play();

        var sceneEntryCamera = GameObject.Find("SceneEntryCamera");
        if (sceneEntryCamera != null)
        {
            var cameraComponent = sceneEntryCamera.GetComponent<Camera>();
            if (cameraComponent != null)
            {
                cameraComponent.enabled = false;
            }
        }

        yield return new WaitForSeconds(4.5f);
        CountDownTimer.GetComponent<Animator>()?.SetTrigger("Timer");

        startCountDownInternalTimer = true;
    }

    public void CalculateRacerPosition()
    {
        sortedRacers = new List<LapCounter>(lapCounters);
        sortedRacers.Sort(SortByScore);

        for (int i = 0; i < sortedRacers.Count; i++)
            sortedRacers[i].Position = i + 1;
    }

    private int SortByScore(LapCounter p1, LapCounter p2)
    {
        if (p1.RaceProgressScore != p2.RaceProgressScore)
            return p2.RaceProgressScore.CompareTo(p1.RaceProgressScore);

        return p1.distanceToNextCheckpoint.CompareTo(p2.distanceToNextCheckpoint);
    }

    #region Sets Enable/Disable Helpers
    private void ToggleSet(List<GameObject> set, bool enable, bool cowChildActive = true)
    {
        if (set == null || set.Count == 0) return;

        foreach (var obj in set)
        {
            if (obj == null) continue; // Skip null entries

            if (obj.name.Contains("Cow"))
                obj.transform.GetChild(1).gameObject.SetActive(cowChildActive);
            else
                obj.SetActive(enable);
        }
    }

    public void DisableSet1() => ToggleSet(set1, false, false);
    public void DisableSet2()
    {
        ToggleSet(set1, true, true);
        ToggleSet(set2, false, false);
    }
    public void DisableSet3()
    {
        ToggleSet(set2, true, true);
        ToggleSet(set3, false, true);
    }
    public void EnableAllSets()
    {
        ToggleSet(set1, true, true);
        ToggleSet(set2, true, true);
        ToggleSet(set3, true, true);
    }
    #endregion

    #region Warnings
    private IEnumerator WarningRoutine<T>(Transform target, GameObject warningPrefab, System.Func<T, bool> condition) where T : Component
    {
        GameObject warning = Instantiate(warningPrefab, warningPrefab.transform.position, warningPrefab.transform.rotation, Canvas);
        warning.SetActive(true);

        RectTransform warningRect = warning.GetComponent<RectTransform>();

        T component = target.GetComponent<T>();
        while (condition(component) && !RACE_COMPLETED)
        {
            Vector3 toTarget = target.position - LocalPlayer.transform.position;
            Vector3 cross = Vector3.Cross(-LocalPlayer.transform.forward, toTarget);
            float dir = Vector3.Dot(cross, LocalPlayer.transform.up);

            Vector3 pos = warningRect.localPosition;
            pos.x = Mathf.Lerp(pos.x, dir * 10f, 3f * Time.deltaTime);
            warningRect.localPosition = pos;

            yield return new WaitForSeconds(0.02f);
        }

        Destroy(warning);
    }

    public IEnumerator WarningRedShell(Transform redshell) => WarningRoutine<RedShell>(redshell, RedShellWarning, c => c.isactive && c.current_node <= LocalPlayerLap.currentCheckpointVal);
    public IEnumerator WarningBlueShell(Transform blueshell) => WarningRoutine<BlueShell>(blueshell, BlueShellWarning, c => c.isactive && c.current_node <= LocalPlayerLap.currentCheckpointVal && Vector3.Distance(LocalPlayer.transform.position, blueshell.position) < 100f);
    public IEnumerator WarningStar(Transform opponent) => WarningRoutine<OpponentItemManager>(opponent, StarWarning, c => c.StarPowerUp && opponent.GetComponent<LapCounter>().RaceProgressScore <= LocalPlayerLap.RaceProgressScore);
    public IEnumerator WarningBullet(Transform opponent) => WarningRoutine<OpponentItemManager>(opponent, BulletWarning, c => c.isBullet && opponent.GetComponent<LapCounter>().RaceProgressScore <= LocalPlayerLap.RaceProgressScore);
    #endregion

    public void ChooseRandomAIPath(out Transform outPath, out SplineContainer outPathSpline, out PathTool outSelectedPathTool)
    {
        int randomPath = UnityEngine.Random.Range(0, AIPaths.Count);
        randomPath = Mathf.Clamp(randomPath, 0, AIPaths.Count - 1);

        PathTool pathTool = AIPaths[randomPath];
        outPath = pathTool.pathRoot;
        outPathSpline = pathTool.GetComponent<SplineContainer>();
        outSelectedPathTool = pathTool;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}