using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class RACE_MANAGER : MonoBehaviour
{
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
    bool FPCam = false; //first person cam

    public AudioSource music;
    public AudioSource musicFast;
    bool lastLap;

    public GameObject CountDownTimer => IngameUIHolder.Instance.CountDownTimer;
    public GameObject CoinCounter => IngameUIHolder.Instance.CoinCounter;
    public GameObject LapCounter => IngameUIHolder.Instance.LapCounter;
    public GameObject MiniMap => IngameUIHolder.Instance.MiniMap;
    public GameObject PositionCounter => IngameUIHolder.Instance.PositionCounter;
    public GameObject CourseNameUI => IngameUIHolder.Instance.CourseNameUI.gameObject;
    public GameObject CourseAuthorUI => IngameUIHolder.Instance.CourseAuthorUI.gameObject;

    private float RaceTime = 0;


    public List<LapCounter> lapCounters => IngameUIHolder.Instance.LapCounters;
    public List<LapCounter> sortedRacers = new List<LapCounter>();


    private float sortTime = 0;

    public static bool RACE_STARTED = false;
    public static bool RACE_COMPLETED = false;
    public static bool raceFinishStuff = false;

    private int lastPos;

    public GameObject spectatorSounds;
    public GameObject itemSystem => IngameUIHolder.Instance.ItemSystem;
    public ResultsUI resultsUI => IngameUIHolder.Instance.ResultsUI;
    public GameObject finishUI => IngameUIHolder.Instance.FinishUI;

    public GameObject RedShellWarning => IngameUIHolder.Instance.RedShellWarning;
    public GameObject BlueShellWarning => IngameUIHolder.Instance.BlueShellWarning;
    public GameObject StarWarning => IngameUIHolder.Instance.StarWarning;
    public GameObject BulletWarning => IngameUIHolder.Instance.BulletWarning;

    private Transform player;
    public Transform Canvas => IngameUIHolder.Instance.Canvas.transform;

    [Header("Sets to disable when doing the Scene Entry Camera")]
    public GameObject[] set1;
    public GameObject[] set2;
    public GameObject[] set3;

    public static Transform allPaths;

    public TrolleySystem trolleySystem;


    public static float countDownTime = 0;
    private bool startCountDownInternalTimer = false;

    [HideInInspector]
    public int currentBlueShellCount = 0;

    public int MAXLAPS = 3;

    void Awake()
    {
        Application.targetFrameRate = 60;
    }
    // Start is called before the first frame update
    void Start()
    {
        // If no name is specified, default to the clip name
        if (string.IsNullOrEmpty(courseMusic.songName))
        {
            courseMusic.songName = courseMusic.clip.name;
        }

        if (string.IsNullOrEmpty(finalLapCourseMusic.songName))
        {
            finalLapCourseMusic.songName = finalLapCourseMusic.clip.name;
        }

        IngameUIHolder.Instance.MusicDisplay.ChangeSongSpeed(music, courseMusic.playbackSpeed);
        IngameUIHolder.Instance.MusicDisplay.ChangeSongSpeed(musicFast, finalLapCourseMusic.playbackSpeed);

        music.clip = courseMusic.clip;
        musicFast.clip = finalLapCourseMusic.clip;

        allPaths = GameObject.Find("AI PATHS").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;

        IngameUIHolder.Instance.UpdateAuthor(Author);
        IngameUIHolder.Instance.UpdateCourse(CourseName, Console);
        IngameUIHolder.Instance.UpdateMinimapBackground(MinimapBackground);
    }

    // Update is called once per frame
    void Update()
    {
        if (startCountDownInternalTimer)
        {
            countDownTime += Time.deltaTime;
        }

        if (RACE_STARTED && !RACE_COMPLETED)
        {
            RaceTime += Time.deltaTime;
            if (!music.isPlaying && !lastLap && RaceTime > 0.5f)
            {
                IngameUIHolder.Instance.MusicDisplay.DisplayAndPlay(music, courseMusic.author, courseMusic.songName, "Normal");
            }
            sortTime += Time.deltaTime;

            if(sortTime > 0.1f)
            {
                calculateRacerPosition();
                sortTime = 0;
            }

        }
        //camera stuff
        
        if (Input.GetKeyDown(KeyCode.Alpha1) && RACE_STARTED && !RACE_COMPLETED) //if pressed 1 and back cam is not enabled, disable front cam and enable FP cam
        {
            if (!Input.GetKey(KeyCode.B))
            {
                FPCam = true;
                FrontFPCam.SetActive(true);
                FrontCam.GetComponent<Camera>().enabled = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && RACE_STARTED && !RACE_COMPLETED) //if pressed 1 and back cam is not enabled, disable FrontFP cam and enable regular front cam
        {
            if (!Input.GetKey(KeyCode.B))
            {
                FPCam = false;
                FrontFPCam.SetActive(false);
                FrontCam.GetComponent<Camera>().enabled = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.B) && RACE_STARTED && !RACE_COMPLETED)
        {
            if (FPCam)
            {
                BackCam.SetActive(true);
                FrontFPCam.SetActive(false);
            }
            else
            {
                BackCam.SetActive(true);
                FrontCam.GetComponent<Camera>().enabled = false;
            }

        }
        if (Input.GetKeyUp(KeyCode.B) && RACE_STARTED && !RACE_COMPLETED)
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

        if (RACE_COMPLETED && !raceFinishStuff)
        {
            raceFinishStuff = true;
            StartCoroutine(FinishRace());
        }

        if(GameObject.FindGameObjectWithTag("Player").GetComponent<LapCounter>().LAPCOUNT == MAXLAPS && !lastLap)
        {
            lastLap = true;
            music.Stop();
            PlayFinalLap();
        }
    }

    void PlayFinalLap()
    {
        StartCoroutine(PlayFinalLapEffect());
    }

    private IEnumerator PlayFinalLapEffect()
    {
        finalLapSound.Play();
        yield return new WaitForSeconds(finalLapSound.clip.length);
        IngameUIHolder.Instance.MusicDisplay.DisplayAndPlay(musicFast, finalLapCourseMusic.author, finalLapCourseMusic.songName, "Faster");
    }

    public IEnumerator CountDownTImerPlay()
    {
        GameObject.Find("FadeInOut").GetComponent<Animator>().SetTrigger("FadeIn");//fade in anim
        CoinCounter.SetActive(true);
        LapCounter.SetActive(true);
        MiniMap.SetActive(true);
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().SceneEntryFinished = true; //start the engine sounds

        //make the main camera active, start it's entry animation, wait a bit before playing the audio, and then disable the sceneEntry camera, and start the countdown in 4.5 seconds
        FrontCam.SetActive(true);                                                                   
        FrontCam.GetComponent<Animator>().SetTrigger("Entry");
        yield return new WaitForSeconds(0.5f);
        FrontCam.GetComponent<AudioSource>().Play();
        GameObject.Find("SceneEntryCamera").GetComponent<Camera>().enabled = false;
        yield return new WaitForSeconds(4.5f);
        CountDownTimer.GetComponent<Animator>().SetTrigger("Timer");

        startCountDownInternalTimer = true;
    }

    public void calculateRacerPosition()
    {

        sortedRacers = new List<LapCounter>(lapCounters);
        sortedRacers.Sort(SortByScore);


        for(int i = 0; i < sortedRacers.Count; i++)
        {
            sortedRacers[i].Position = i + 1;
        }

    }
    int SortByScore(LapCounter p1, LapCounter p2)
    {
        if(p1.totalCheckpointVal != p2.totalCheckpointVal)
            return -p1.totalCheckpointVal.CompareTo(p2.totalCheckpointVal);
        else
        {
            return p1.distanceToNextCheckpoint.CompareTo(p2.distanceToNextCheckpoint);

        }
    }

    // Oh my f*cking god! You ever hearding of CACHING
    IEnumerator FinishRace()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().goal.Play();

        if(spectatorSounds !=null)
            spectatorSounds.SetActive(false);
        itemSystem.SetActive(false);
        FrontCam.transform.parent.parent.GetComponent<AudioSource>().Stop();
        finishUI.GetComponent<Animator>().SetBool("Finish", true);
        BackCam.SetActive(false);
        FrontCam.SetActive(true);

        FrontCam.GetComponent<Camera>().enabled = true;
        FrontFPCam.SetActive(false);
        //end music based on position
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<LapCounter>().Position == 1)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Driver.SetBool("FirstPlace", true);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().firstPlaceVoice.Play();
            yield return new WaitForSeconds(1);

            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().firstPlaceResult.Play();
            yield return new WaitForSeconds(2.5f);
            FrontCam.GetComponent<Animator>().SetBool("RaceEndCam", true);
            yield return new WaitForSeconds(0.5f);
            resultsUI.createResults(sortedRacers);
            yield return new WaitForSeconds(3f);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().resultsGood.Play();

        }
        else if (GameObject.FindGameObjectWithTag("Player").GetComponent<LapCounter>().Position < 6)
        {
            //for now it is the same thing
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Driver.SetBool("FirstPlace", true);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().firstPlaceVoice.Play();
            yield return new WaitForSeconds(1);

            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().secondToSixth.Play(); //except for this
            yield return new WaitForSeconds(2.5f);
            FrontCam.GetComponent<Animator>().SetBool("RaceEndCam", true);
            yield return new WaitForSeconds(0.5f);
            resultsUI.createResults(sortedRacers);
            yield return new WaitForSeconds(3f);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().resultsGood.Play();
        }
        else
        {
            //for now it is the same thing
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Driver.SetBool("LoseAnim", true);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().marioLose.Play();
            yield return new WaitForSeconds(1);

            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().loseResult.Play(); //except for this
            yield return new WaitForSeconds(2.5f);
            FrontCam.GetComponent<Animator>().SetBool("RaceEndCam", true);
            yield return new WaitForSeconds(0.5f);
            resultsUI.createResults(sortedRacers);
            yield return new WaitForSeconds(3f);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSounds>().resultsBad.Play(); //and this
        }

    }


    public IEnumerator warningRedShell(Transform redshell) //this function gets called by the incoming redshell once, and the function will handle that redshell alone. This way, if there are more than one, the function can be called again, referring to another redshell
    {
        GameObject warning = Instantiate(RedShellWarning, RedShellWarning.transform.position, RedShellWarning.transform.rotation);
        warning.SetActive(true);
        warning.transform.SetParent(Canvas);

        while(redshell.GetComponent<RedShell>().isactive && !RACE_COMPLETED && redshell.GetComponent<RedShell>().current_node <= player.GetComponent<Player>().waypointTracker.CurrentWaypoint)
        {
            Vector3 myangle = player.position - redshell.position;
            Vector3 angle = Vector3.Cross(-player.forward, myangle);
            float dir = Vector3.Dot(angle, player.up);


            Vector3 oldPos = warning.GetComponent<RectTransform>().localPosition;
            oldPos.x = 0 + dir * 10;
           

            warning.GetComponent<RectTransform>().localPosition = oldPos;


            


            yield return new WaitForSeconds(0.02f);
        }

        Destroy(warning);
    }
    public IEnumerator warningBlueShell(Transform blueshell) //this function gets called by the incoming redshell once, and the function will handle that redshell alone. This way, if there are more than one, the function can be called again, referring to another redshell
    {
        GameObject warning = Instantiate(BlueShellWarning, BlueShellWarning.transform.position, BlueShellWarning.transform.rotation);
        warning.SetActive(true);
        warning.transform.SetParent(Canvas);

        while (blueshell.GetComponent<BlueShell>().isactive == true && !RACE_COMPLETED && blueshell.GetComponent<BlueShell>().current_node <= player.GetComponent<Player>().waypointTracker.CurrentWaypoint && Vector3.Distance(player.position, blueshell.position) < 100)
        {
            Vector3 myangle = player.position - blueshell.position;
            Vector3 angle = Vector3.Cross(-player.forward, myangle);
            float dir = Vector3.Dot(angle, player.up);


            Vector3 oldPos = warning.GetComponent<RectTransform>().localPosition;

            oldPos.x = 0 + dir * 10;


            warning.GetComponent<RectTransform>().localPosition = oldPos;

            if (!blueshell.GetComponent<BlueShell>().isactive)
            {
                break;
            }


            yield return new WaitForSeconds(0.02f);
        }

        Destroy(warning);
    }
    public IEnumerator warningStar(Transform opponent) //this function gets called by the incoming redshell once, and the function will handle that redshell alone. This way, if there are more than one, the function can be called again, referring to another redshell
    {
        GameObject warning = Instantiate(StarWarning, StarWarning.transform.position, StarWarning.transform.rotation);
        warning.SetActive(true);
        warning.transform.SetParent(Canvas);

        while (opponent.GetComponent<OpponentItemManager>().StarPowerUp && !RACE_COMPLETED && opponent.GetComponent<LapCounter>().totalCheckpointVal <= player.GetComponent<LapCounter>().totalCheckpointVal)
        {
            Vector3 myangle = player.position - opponent.position;
            Vector3 angle = Vector3.Cross(-player.forward, myangle);
            float dir = Vector3.Dot(angle, player.up);


            Vector3 oldPos = warning.GetComponent<RectTransform>().localPosition;
            oldPos.x = 0 + dir * 10;


            warning.GetComponent<RectTransform>().localPosition = Vector3.Lerp(warning.GetComponent<RectTransform>().localPosition, oldPos, 3 * Time.deltaTime);

            if (!opponent.GetComponent<OpponentItemManager>().StarPowerUp)
            {
                break;
            }


            yield return new WaitForSeconds(0.02f);
        }

        Destroy(warning);
    }

    public IEnumerator warningBullet(Transform opponent) //this function gets called by the incoming redshell once, and the function will handle that redshell alone. This way, if there are more than one, the function can be called again, referring to another redshell
    {
        GameObject warning = Instantiate(BulletWarning, BulletWarning.transform.position, BulletWarning.transform.rotation);
        warning.SetActive(true);
        warning.transform.SetParent(Canvas);

        while (opponent.GetComponent<OpponentItemManager>().isBullet && !RACE_COMPLETED && opponent.GetComponent<LapCounter>().totalCheckpointVal <= player.GetComponent<LapCounter>().totalCheckpointVal)
        {
            Vector3 myangle = player.position - opponent.position;
            Vector3 angle = Vector3.Cross(-player.forward, myangle);
            float dir = Vector3.Dot(angle, player.up);


            Vector3 oldPos = warning.GetComponent<RectTransform>().localPosition;

            oldPos.x = 0 + dir * 10;


            warning.GetComponent<RectTransform>().localPosition = Vector3.Lerp(warning.GetComponent<RectTransform>().localPosition, oldPos, 3 * Time.deltaTime);

            if (!opponent.GetComponent<OpponentItemManager>().isBullet)
            {
                break;
            }


            yield return new WaitForSeconds(0.02f);
        }

        Destroy(warning);
    }


    public void DisableSet1()
    {
        if (set1.Length == 0)
            return;

        for(int i = 0; i < set1.Length; i++)
        {
            if(set1[i].name.IndexOf("Cow") >= 0)
            {
                set1[i].transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                set1[i].SetActive(false);
            }
        }
    }
    public void DisableSet2()
    {
        if (set1.Length == 0)
            return;

        for (int i = 0; i < set1.Length; i++)
        {
            if (set1[i].name.IndexOf("Cow") >= 0)
            {
                set1[i].transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                set1[i].SetActive(true);
            }
        }

        if (set2.Length == 0)
            return;

        for (int i = 0; i < set2.Length; i++)
        {
            if (set2[i].name.IndexOf("Cow") >= 0)
            {
                set2[i].transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                set2[i].SetActive(false);
            }
        }
    }

    public void DisableSet3()
    {
        if (set2.Length == 0)
            return;

        for (int i = 0; i < set2.Length; i++)
        {
            if (set2[i].name.IndexOf("Cow") >= 0)
            {
                set2[i].transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                set2[i].SetActive(true);
            }
        }

        if (set3.Length == 0)
            return;

        for (int i = 0; i < set3.Length; i++)
        {
            if (set3[i].name.IndexOf("Cow") >= 0)
            {
                set3[i].transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                set3[i].SetActive(false);
            }
        }
    }
    public void enableAllSets()
    {
        if (set1.Length > 0)
        {
            for (int i = 0; i < set1.Length; i++)
            {
                if (set1[i].name.IndexOf("Cow") >= 0)
                {
                    set1[i].transform.GetChild(1).gameObject.SetActive(true);
                }
                else
                {
                    set1[i].SetActive(true);
                }
            }
        }

        if (set2.Length > 0)
        {
            for (int i = 0; i < set2.Length; i++)
            {
                if (set2[i].name.IndexOf("Cow") >= 0)
                {
                    set2[i].transform.GetChild(1).gameObject.SetActive(true);
                }
                else
                {
                    set2[i].SetActive(true);
                }
            }
        }

        if (set3.Length > 0)
        {
            for (int i = 0; i < set3.Length; i++)
            {
                if (set3[i].name.IndexOf("Cow") >= 0)
                {
                    set3[i].transform.GetChild(1).gameObject.SetActive(true);
                }
                else
                {
                    set3[i].SetActive(true);
                }
            }
        }
    }



}
