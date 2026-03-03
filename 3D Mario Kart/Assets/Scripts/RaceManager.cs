using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public SongDetails courseMusic;
    public SongDetails finalLapCourseMusic;
    public AudioSource finalLapSound;

    [Header("Setup")]
    public GameObject FrontCam;
    public GameObject FrontFPCam;
    public GameObject BackCam;
    public Transform AIPathRoot;
    [HideInInspector] public List<PathTool> AIPaths = new List<PathTool>();
    private bool FPCam = false; // First-person camera flag

    public AudioSource music;
    public AudioSource musicFast;
    private bool lastLap = false;

    public int MAXLAPS = 3;

    private Transform player;
    private PlayerSounds playerSounds;
    private LapCounter playerLap;

    private float raceTime = 0f;
    private float sortTime = 0f;

    public static bool RACE_STARTED = false;
    public static bool RACE_COMPLETED = false;
    public static bool raceFinishStuff = false;

    public List<LapCounter> lapCounters => IngameUIHolder.Instance.LapCounters;
    public List<LapCounter> sortedRacers = new List<LapCounter>();

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

    [HideInInspector] public Player LocalPlayer;
    [HideInInspector] public LapCounter LocalPlayerLap;
    [HideInInspector] public PlayerSounds LocalPlayerSounds;

    public List<Player> AllPlayers = new List<Player>();

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
        allPaths = GameObject.Find("AI PATHS")?.transform;

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
        if (playerLap != null && playerLap.LAPCOUNT == MAXLAPS && !lastLap)
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
        // Play goal sound
        playerSounds.goal.Play();

        // Disable spectator sounds and item system
        if (spectatorSounds != null)
            spectatorSounds.SetActive(false);
        itemSystem.SetActive(false);

        // Stop camera audio on the FrontCam parent
        FrontCam.transform.parent.parent.GetComponent<AudioSource>()?.Stop();

        // Enable finish UI animations
        finishUI.GetComponent<Animator>().SetBool("Finish", true);

        // Camera setup
        BackCam.SetActive(false);
        FrontCam.SetActive(true);
        FrontCam.GetComponent<Camera>().enabled = true;
        if (FrontFPCam != null) FrontFPCam.SetActive(false);

        // Check player's final position
        int playerPos = playerLap.Position;

        if (playerPos == 1)
        {
            // First place sequence
            LocalPlayer.Driver.SetBool("FirstPlace", true);
            playerSounds.firstPlaceVoice.Play();
            yield return new WaitForSeconds(1f);

            playerSounds.firstPlaceResult.Play();
            yield return new WaitForSeconds(2.5f);

            FrontCam.GetComponent<Animator>().SetBool("RaceEndCam", true);
            yield return new WaitForSeconds(0.5f);

            resultsUI.createResults(sortedRacers);
            yield return new WaitForSeconds(3f);

            playerSounds.resultsGood.Play();
        }
        else if (playerPos < 6)
        {
            // 2nd–5th place sequence
            LocalPlayer.Driver.SetBool("FirstPlace", true); // maybe this should be adjusted for non-first places
            playerSounds.firstPlaceVoice.Play();
            yield return new WaitForSeconds(1f);

            playerSounds.secondToSixth.Play();
            yield return new WaitForSeconds(2.5f);

            FrontCam.GetComponent<Animator>().SetBool("RaceEndCam", true);
            yield return new WaitForSeconds(0.5f);

            resultsUI.createResults(sortedRacers);
            yield return new WaitForSeconds(3f);

            playerSounds.resultsGood.Play();
        }
        else
        {
            // Last place or beyond
            LocalPlayer.Driver.SetBool("LoseAnim", true);
            playerSounds.marioLose.Play();
            yield return new WaitForSeconds(1f);

            playerSounds.loseResult.Play();
            yield return new WaitForSeconds(2.5f);

            FrontCam.GetComponent<Animator>().SetBool("RaceEndCam", true);
            yield return new WaitForSeconds(0.5f);

            resultsUI.createResults(sortedRacers);
            yield return new WaitForSeconds(3f);

            playerSounds.resultsBad.Play();
        }
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

        playerSounds.SceneEntryFinished = true;

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
            Vector3 toTarget = target.position - player.position;
            Vector3 cross = Vector3.Cross(-player.forward, toTarget);
            float dir = Vector3.Dot(cross, player.up);

            Vector3 pos = warningRect.localPosition;
            pos.x = Mathf.Lerp(pos.x, dir * 10f, 3f * Time.deltaTime);
            warningRect.localPosition = pos;

            yield return new WaitForSeconds(0.02f);
        }

        Destroy(warning);
    }

    public IEnumerator WarningRedShell(Transform redshell) => WarningRoutine<RedShell>(redshell, RedShellWarning, c => c.isactive && c.current_node <= playerLap.currentCheckpointVal);
    public IEnumerator WarningBlueShell(Transform blueshell) => WarningRoutine<BlueShell>(blueshell, BlueShellWarning, c => c.isactive && c.current_node <= playerLap.currentCheckpointVal && Vector3.Distance(player.position, blueshell.position) < 100f);
    public IEnumerator WarningStar(Transform opponent) => WarningRoutine<OpponentItemManager>(opponent, StarWarning, c => c.StarPowerUp && opponent.GetComponent<LapCounter>().RaceProgressScore <= playerLap.RaceProgressScore);
    public IEnumerator WarningBullet(Transform opponent) => WarningRoutine<OpponentItemManager>(opponent, BulletWarning, c => c.isBullet && opponent.GetComponent<LapCounter>().RaceProgressScore <= playerLap.RaceProgressScore);
    #endregion
}