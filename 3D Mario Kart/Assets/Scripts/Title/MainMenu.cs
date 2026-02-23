using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public enum MenuState
    {
        None,
        Main,
        Singleplayer,
        Multiplayer,
        Settings
    }

    [Header("UI References")]
    public RectTransform logoPanel;
    public RectTransform leftPanel;
    public RectTransform rightPanel;
    public GameObject startTextObject;

    [Header("Fade Group")]
    public CanvasGroup menuButtonsCanvasGroup;

    [Header("Submenus")]
    public CanvasGroup singleplayerCanvasGroup;

    [Header("Animation Speeds")]
    public float logoMoveTime = 0.35f;
    public float sideMoveTime = 0.15f;
    public float fadeTime = 0.25f;

    [Header("Audio")]
    public AudioSource startGameSound;
    public AudioSource returnSound;
    public AudioSource buttonSelectSound;

    public AudioCrossfade menuToSub;
    public AudioCrossfade subToMenu;

    [Header("Input")]
    public InputActionAsset controls;

    [Header("Menu Buttons (Main Menu Only)")]
    public List<Button> menuButtons = new List<Button>();

    private int currentIndex = 0;

    private InputActionMap inputMap;

    private InputAction startAction;
    private InputAction selectAction;
    private InputAction returnAction;
    private InputAction menuUpAction;
    private InputAction menuDownAction;

    public MenuState CurrentMenu { get; private set; } = MenuState.None;

    private bool canStart = false;
    private bool isAnimating = false;

    private void Awake()
    {
        inputMap = controls.FindActionMap("Menu", true);

        startAction = inputMap.FindAction("Start", true);
        selectAction = inputMap.FindAction("Select", true);
        returnAction = inputMap.FindAction("Return", true);
        menuUpAction = inputMap.FindAction("MenuUp", true);
        menuDownAction = inputMap.FindAction("MenuDown", true);

        startAction.performed += OnStartPressed;
        selectAction.performed += OnSelectPressed;
        returnAction.performed += OnReturnPressed;
        menuUpAction.performed += OnMenuUp;
        menuDownAction.performed += OnMenuDown;

        inputMap.Enable();
    }

    void Start()
    {
        logoPanel.anchoredPosition = new Vector2(0f, 0f);
        leftPanel.anchoredPosition = new Vector2(0f, 1080f);
        rightPanel.anchoredPosition = new Vector2(0f, -1080f);

        if (menuButtonsCanvasGroup != null)
        {
            menuButtonsCanvasGroup.alpha = 0f;
            menuButtonsCanvasGroup.interactable = false;
            menuButtonsCanvasGroup.blocksRaycasts = false;
        }

        if (singleplayerCanvasGroup != null)
        {
            singleplayerCanvasGroup.alpha = 0f;
            singleplayerCanvasGroup.interactable = false;
            singleplayerCanvasGroup.blocksRaycasts = false;
        }

        StartCoroutine(WaitToStart());

        if (menuButtons.Count > 0)
        {
            currentIndex = 0;
            UpdateSelection();
        }
    }

    private void UpdateSelection()
    {
        if (menuButtons.Count == 0)
            return;

        EventSystem.current.SetSelectedGameObject(menuButtons[currentIndex].gameObject);
    }

    IEnumerator WaitToStart()
    {
        startTextObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        canStart = true;
        startTextObject.SetActive(true);
    }

    IEnumerator ToggleMenu(bool open)
    {
        isAnimating = true;
        CurrentMenu = open ? MenuState.Main : MenuState.None;

        Vector2 logoTarget = open ? new Vector2(-730f, 0f) : new Vector2(0f, 0f);
        Vector2 leftTarget = open ? new Vector2(0f, 0f) : new Vector2(0f, 1080f);
        Vector2 rightTarget = open ? new Vector2(0f, 0f) : new Vector2(0f, -1080f);

        if (open)
        {
            startGameSound.Play();

            // Start all animations at once
            Coroutine c1 = StartCoroutine(MoveUI(logoPanel, logoTarget, logoMoveTime));
            Coroutine c2 = StartCoroutine(MoveUI(leftPanel, leftTarget, sideMoveTime));
            Coroutine c3 = StartCoroutine(MoveUI(rightPanel, rightTarget, sideMoveTime));

            // Wait for all to finish
            yield return c1;
            yield return c2;
            yield return c3;

            if (menuButtonsCanvasGroup != null)
                yield return StartCoroutine(FadeCanvas(menuButtonsCanvasGroup, 1f, fadeTime, true));
        }
        else
        {
            returnSound.Play();

            if (menuButtonsCanvasGroup != null)
                yield return StartCoroutine(FadeCanvas(menuButtonsCanvasGroup, 0f, fadeTime, false));

            // Start all animations at once
            Coroutine c1 = StartCoroutine(MoveUI(logoPanel, logoTarget, logoMoveTime));
            Coroutine c2 = StartCoroutine(MoveUI(leftPanel, leftTarget, sideMoveTime));
            Coroutine c3 = StartCoroutine(MoveUI(rightPanel, rightTarget, sideMoveTime));

            yield return c1;
            yield return c2;
            yield return c3;
        }

        isAnimating = false;
    }

    IEnumerator MoveUI(RectTransform target, Vector2 targetPos, float duration)
    {
        Vector2 startPos = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            target.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        target.anchoredPosition = targetPos;
    }

    IEnumerator FadeCanvas(CanvasGroup canvasGroup, float targetAlpha, float duration, bool enableInteraction)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = enableInteraction;
        canvasGroup.blocksRaycasts = enableInteraction;
    }

    IEnumerator SwitchMenu(CanvasGroup from, CanvasGroup to)
    {
        if (from != null)
            yield return StartCoroutine(FadeCanvas(from, 0f, fadeTime, false));

        if (to != null)
            yield return StartCoroutine(FadeCanvas(to, 1f, fadeTime, true));
    }

    public void OnClick_Singleplayer()
    {
        buttonSelectSound.Play();
        StartCoroutine(SwitchMenu(menuButtonsCanvasGroup, singleplayerCanvasGroup));
        menuToSub.StartCrossfade();
        CurrentMenu = MenuState.Singleplayer;
    }

    public void OnClick_Multiplayer()
    {
        buttonSelectSound.Play();
        CurrentMenu = MenuState.Multiplayer;
        Debug.Log("Multiplayer button clicked!");
    }

    public void OnClick_Settings()
    {
        buttonSelectSound.Play();
        CurrentMenu = MenuState.Settings;
        Debug.Log("Settings button clicked!");
    }

    public void OnClick_Quit()
    {
        buttonSelectSound.Play();
        Application.Quit();
    }

    public void OnClick_ReturnFromSingleplayer()
    {
        returnSound.Play();
        StartCoroutine(SwitchMenu(singleplayerCanvasGroup, menuButtonsCanvasGroup));
        subToMenu.StartCrossfade();
        CurrentMenu = MenuState.Main;
    }

    private void OnStartPressed(InputAction.CallbackContext context)
    {
        if (!canStart || isAnimating)
            return;

        if (CurrentMenu.Equals(MenuState.None))
            StartCoroutine(ToggleMenu(true));
    }

    private void OnReturnPressed(InputAction.CallbackContext context)
    {
        if (!canStart || isAnimating)
            return;

        switch (CurrentMenu)
        {
            case MenuState.Singleplayer:
                OnClick_ReturnFromSingleplayer();
                break;

            case MenuState.Main:
                StartCoroutine(ToggleMenu(false));
                break;
        }
    }

    private void OnMenuUp(InputAction.CallbackContext context)
    {
        if (CurrentMenu != MenuState.Main || isAnimating || menuButtons.Count == 0)
            return;

        int startingIndex = currentIndex;

        do
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = menuButtons.Count - 1;

            // Stop if we’ve looped all the way around
            if (currentIndex == startingIndex)
                break;
        }
        while (!menuButtons[currentIndex].interactable);

        UpdateSelection();
    }

    private void OnMenuDown(InputAction.CallbackContext context)
    {
        if (CurrentMenu != MenuState.Main || isAnimating || menuButtons.Count == 0)
            return;

        int startingIndex = currentIndex;

        do
        {
            currentIndex++;
            if (currentIndex >= menuButtons.Count)
                currentIndex = 0;

            // Stop if we’ve looped all the way around
            if (currentIndex == startingIndex)
                break;
        }
        while (!menuButtons[currentIndex].interactable);

        UpdateSelection();
    }

    private void OnSelectPressed(InputAction.CallbackContext context)
    {
        if (CurrentMenu != MenuState.Main || isAnimating || menuButtons.Count == 0)
            return;

        Button button = menuButtons[currentIndex];

        if (button != null && button.interactable)
            button.onClick.Invoke();
    }
}