using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class IngameUIHolder : MonoBehaviour
{
    private static IngameUIHolder _instance;
    public static IngameUIHolder Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindFirstObjectByType<IngameUIHolder>();
                if (_instance == null)
                {
                    Debug.LogError("No IngameUIHolder found in the scene!");
                }
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    [System.Serializable]
    public class TextUIElement
    {
        public Text shadow;
        public Text top;

        public void UpdateText(string newText)
        {
            if (shadow != null)
                shadow.text = newText;
            if (top != null)
                top.text = newText;
        }
    }

    public Canvas Canvas;
    public GameObject CountDownTimer;
    public GameObject CoinCounter;
    public GameObject LapCounter;
    [Header("Minimap")]
    public Image MinimapBackground;
    public GameObject MiniMap;
    public GameObject MinimapIcon;
    public RectTransform Map2dEnd;
    [Space(10)]
    public GameObject PositionCounter;
    public CanvasGroup CourseNameUI;
    public Text CourseNameText;
    public Text ConsoleText;

    public CanvasGroup CourseAuthorUI;
    public Text CourseAuthorText;

    [Space(10)]
    public CanvasGroup SongDisplayUI;
    public Text SongTypeText;
    public Text SongNameText;
    public Text SongAuthorText;
    public MusicDisplay MusicDisplay;

    public List<LapCounter> LapCounters = new();
    [Space(10)]

    public GameObject ItemSystem;
    public Image YourItem;
    [System.Serializable]
    public struct UIItem
    {
        public Animator Main;
        public Animator List;
        public Image OurItem;
    }
    [Header("UI")]
    public UIItem PrimaryItem;
    public UIItem SecondaryItem;

    public ResultsUI ResultsUI;
    public GameObject FinishUI;
    public GameObject RedShellWarning;
    public GameObject BlueShellWarning;
    public GameObject StarWarning;
    public GameObject BulletWarning;
    [Space(10)]

    public TextUIElement coinUI = new();
    public TextUIElement lapCounterUI = new();

    [Space(10)]
    public CanvasGroup WaitingForPlayers;
    public Text WaitingForPlayersCount;

    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        if (LapCounters.Count == 0)
        {
            LapCounters = new List<LapCounter>(FindObjectsByType<LapCounter>(FindObjectsSortMode.None));
        }
        //GenerateAllRacerMinimapIcons();
    }

    public void GenerateIconFor(LapCounter racer)
    {
        MinimapIcon icon = Instantiate(MinimapIcon, MiniMap.transform).GetComponent<MinimapIcon>();
        icon.IsOurIcon = racer.CompareTag("Player");
        if (icon.IsOurIcon)
        {
            icon.transform.SetAsFirstSibling();
        }
        else
        {
            icon.transform.SetSiblingIndex(1);
        }

        Minimap minimap = racer.GetComponent<Minimap>();
        if (minimap)
        {
            minimap.playerInMap = icon.transform.GetComponent<RectTransform>();
            minimap.map2dEnd = Map2dEnd;
            if (minimap.config == null)
            {
                Debug.LogWarning($"Racer {racer.name} has no MinimapConfig assigned! Defaulting to {MinimapIcon.name}");
                Destroy(icon.gameObject); // Remove invalid icon
                return;
            }
            icon.UpdateSprite(minimap.config.MinimapIcon);
        }
    }

    private void GenerateAllRacerMinimapIcons()
    {
        foreach (LapCounter racer in LapCounters)
        {
            GenerateIconFor(racer);
        }
    }

    public void UpdateSong(string author, string songName, string songType = "Normal")
    {
        SongAuthorText.text = author;
        SongNameText.text = songName;
        SongTypeText.text = songType;
    }

    public void UpdateAuthor(string author)
    {
        CourseAuthorText.text = author;
    }

    public void UpdateCourse(string courseName, string console)
    {
        CourseNameText.text = courseName;
        ConsoleText.text = console;
    }

    public void UpdateMinimapBackground(Sprite newBackground)
    {
        if (MinimapBackground != null)
        {
            MinimapBackground.sprite = newBackground;
        }
    }
}