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
    public GameObject MiniMap;
    public GameObject PositionCounter;
    public GameObject CourseNameUI;
    public List<LapCounter> LapCounters = new();
    [Space(10)]

    public GameObject ItemSystem;
    public Image YourItem;
    public ResultsUI ResultsUI;
    public GameObject FinishUI;
    public GameObject RedShellWarning;
    public GameObject BlueShellWarning;
    public GameObject StarWarning;
    public GameObject BulletWarning;
    [Space(10)]

    public TextUIElement coinUI = new();
    public TextUIElement lapCounterUI = new();

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
    }
}