using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    #region Dependencies

    private Player player;
    private ItemManager itemManager;

    #endregion

    #region ===== KART SOUNDS =====

    [Header("Kart Sounds")]
    public AudioSource kartEngine;   // Assign engine loop sound
    public AudioSource kartIdle;     // Assign idle engine sound
    #endregion

    #region ===== ENVIRONMENTAL SOUNDS =====
    [Header("Environmental Sounds")]
    public AudioSource driftSteer;           // DriftSteer
    public AudioSource driftSpark;           // DriftSpark
    public AudioSource gliderOpen;           // GliderOpen
    public AudioSource gliderFlutter;        // GliderFlutter
    public AudioSource landGroundSkid;       // LandGroundSkid
    public AudioSource gliderClose;          // GliderClose
    public AudioSource driftHop;             // DriftHop
    public AudioSource chassisCrash;         // ChassisCrash
    public AudioSource jumpboard;            // Jumpboard
    public AudioSource coinSound;            // CoinSound
    public AudioSource kartBump;             // KartBump
    public AudioSource accelerateBeforeStart;// AccelerateBeforeStart
    public AudioSource gliderFlapOpen;       // GliderFlapOpen
    public AudioSource startBoost;           // StartBoost
    public AudioSource goal;                 // Goal
    public AudioSource firstPlaceResult;     // First
    public AudioSource secondToSixth;        // Second-Sixth
    public AudioSource resultsGood;          // Results Good
    public AudioSource marioItemHit;         // Mario Item Hit
    public AudioSource cowHit;               // Cow Hit
    public AudioSource kartSkidReverse;      // KartSkidReverse
    public AudioSource loseResult;           // Lose
    public AudioSource resultsBad;           // Results Bad
    public AudioSource antiGravityEnter;     // AntigravityEnter
    public AudioSource antiGravityExit;      // AntigravityExit
    public AudioSource gravitySpin;          // GravitySpin
    #endregion

    #region ===== CHARACTER SOUNDS =====
    [Header("Character Sounds")]
    public List<AudioSource> boostSounds = new();       // BoostSound, BoostSound1, BoostSound2, BoostSound3, BoostSound4, BoostSound5
    public List<AudioSource> starSounds = new();        // StarSound, StarSound1, StarSound2
    public List<AudioSource> jumpTrickSounds = new();   // JumpTrick1, JumpTrick2, JumpTrick3
    public AudioSource bulletFly;
    public AudioSource bulletStart;
    public AudioSource bulletEnd;
    public List<AudioSource> hurtSounds = new();        // Hurt1, Hurt2, Hurt3

    public AudioSource firstPlaceVoice;  // FirstPlace
    public AudioSource marioLose;        // MarioLose
    public AudioSource gliderVoice;      // GliderSound

    #endregion

    private int boostIndex;
    public int CurrentBoostIndex => boostIndex;

    private int starIndex;
    private int hurtIndex;

    public bool SceneEntryFinished;

    #region UNITY

    private void Awake()
    {
        player = GetComponent<Player>();
        itemManager = GetComponent<ItemManager>();
    }

    private void Update()
    {
        HandleKartSounds();

        if (SceneEntryFinished)
            kartIdle.volume = Mathf.Lerp(kartIdle.volume, 0.8f, Time.deltaTime);
    }

    #endregion

    #region ===== KART LOGIC =====

    private void HandleKartSounds()
    {
        float speed = player.currentspeed;

        HandleIdle(speed);
        HandleEngine(speed);
        HandleVolume();
    }

    private void HandleIdle(float speed)
    {
        if (speed > -10 && speed < 10)
        {
            if (!kartIdle.isPlaying)
                kartIdle.Play();
        }
        else if (speed < -10)
        {
            kartIdle.Stop();
        }
    }

    private void HandleEngine(float speed)
    {
        bool canPlay = speed >= 5 && !player.GLIDER_FLY && !itemManager.isBullet;

        if (!canPlay)
        {
            kartEngine.Stop();
            return;
        }

        if (!kartEngine.isPlaying)
            kartEngine.Play();

        UpdateEngineTime(speed);
        UpdateEnginePitch();

        kartIdle.Stop();
    }

    private void UpdateEngineTime(float speed)
    {
        float targetTime = Mathf.Clamp(Mathf.Floor(speed / 10f), 1f, 7f);
        kartEngine.time = Mathf.Lerp(kartEngine.time, targetTime, 4f * Time.deltaTime);
    }

    private void UpdateEnginePitch()
    {
        float targetPitch = 1f;

        if (player.Boost && !player.GLIDER_FLY)
            targetPitch = 1.3f;
        else if (player.Boost && player.GLIDER_FLY)
            targetPitch = 1.5f;

        kartEngine.pitch = Mathf.Lerp(kartEngine.pitch, targetPitch, 5f * Time.deltaTime);
    }

    private void HandleVolume()
    {
        if (RACE_MANAGER.RACE_COMPLETED)
        {
            if (kartEngine.volume > 0f)
                kartEngine.volume -= 0.01f;
            return;
        }

        kartEngine.volume = player.GLIDER_FLY ? 0.3f : 0.45f;
    }

    #endregion

    #region ===== CHARACTER PLAYERS =====

    public void PlayBoost()
    {
        if (!CanPlayCharacterSound() || boostSounds.Count == 0) return;

        boostSounds[boostIndex].Play();
        boostIndex = (boostIndex + 1) % boostSounds.Count;
    }

    public void PlayStar()
    {
        if (starSounds.Count == 0) return;

        starSounds[starIndex].Play();
        starIndex = (starIndex + 1) % starSounds.Count;
    }

    public void PlayHurt()
    {
        if (RACE_MANAGER.RACE_COMPLETED || hurtSounds.Count == 0) return;

        hurtSounds[hurtIndex].Play();
        hurtIndex = (hurtIndex + 1) % hurtSounds.Count;
    }

    public bool CanPlayCharacterSound()
    {
        if (itemManager.isBullet) return false;

        foreach (var sound in boostSounds)
            if (sound.isPlaying)
                return false;

        return true;
    }

    #endregion

    #region Environmental play functions
    public void PlayDriftSpark() => driftSpark?.Play();
    public void PlayDriftSteer() => driftSteer?.Play();
    public void PlayCoin() => coinSound?.Play();
    public void PlayKartBump() => kartBump?.Play();
    public void PlayGliderOpen() => gliderOpen?.Play();
    public void PlayGliderClose() => gliderClose?.Play();
    public void PlayGoal() => goal?.Play();
    public void PlayResultsGood() => resultsGood?.Play();
    public void PlayResultsBad() => resultsBad?.Play();
    public void PlayAntiGravityEnter() => antiGravityEnter?.Play();
    public void PlayAntiGravityExit() => antiGravityExit?.Play();

    public void PlayGravitySpin() => gravitySpin?.Play();

    public void PlayChassisCrash() => chassisCrash?.Play();
    public void PlayLandGroundSkid() => landGroundSkid?.Play();
    #endregion

    public void LoadCharacterSounds(RacerConfig config)
    {
        boostSounds = config.RacerSounds.Boost;
        starSounds = config.RacerSounds.Star;
        jumpTrickSounds = config.RacerSounds.JumpTrick;
        hurtSounds = config.RacerSounds.HurtSounds;
        firstPlaceVoice = config.RacerSounds.FirstPlace;
        marioLose = config.RacerSounds.Lose;
        gliderVoice = config.RacerSounds.Glider;
    }
}