using System;
using System.Collections;
using System.Collections.Generic;
using Netcode.Transports;
using Unity.Netcode;
using Unity.Netcode.Transports.SinglePlayer;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static string Version = "0.4";
    
    public enum MenuState
    {
        None,
        Main,
        Singleplayer,
        Multiplayer,
        Multiplayer_FindLobby,
        Multiplayer_Lobby,
        Settings
    }

    public enum NetworkTransport
    {
        SINGLEPLAYER,
        UNITY,
        STEAM
    }

    [Header("UI References")]
    public RectTransform logoPanel;
    public RectTransform leftPanel;
    public RectTransform rightPanel;
    public GameObject startTextObject;

    [Header("Networking")]
    public NetworkTransport networkTransport = NetworkTransport.SINGLEPLAYER;
    public GameObject DefaultPlayerPrefab;
    public NetworkPrefabsList NetworkPrefabsList;

    [Header("Fade Group")]
    public CanvasGroup menuButtonsCanvasGroup;

    [Header("Submenus")]
    public CanvasGroup singleplayerCanvasGroup;
    public CanvasGroup multiplayerCanvasGroup;
    public MultiplayerMenu multiplayerMenu; // Has steam lobby callbacks on it

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

        if (multiplayerCanvasGroup != null)
        {
            multiplayerCanvasGroup.alpha = 0f;
            multiplayerCanvasGroup.interactable = false;
            multiplayerCanvasGroup.blocksRaycasts = false;
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
        StartCoroutine(SwitchMenu(menuButtonsCanvasGroup, multiplayerCanvasGroup));
        menuToSub.StartCrossfade();
        UpdateNetworkTransport(NetworkTransport.STEAM);
        CurrentMenu = MenuState.Multiplayer;
        Debug.Log("Multiplayer button clicked!");
    }

    public void OnClick_HostGame()
    {
        buttonSelectSound.Play();
        USteamClient.Instance.CreatePublicLobby();
    }

    public void OnClick_LeaveLobby()
    {
        buttonSelectSound.Play();
        USteamClient.Instance.LeaveLobby();
    }

    public void OnClick_FindGame()
    {
        buttonSelectSound.Play();
        multiplayerMenu.RequestSearchLobby();
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

    public void OnClick_ReturnFromMultiplayer()
    {
        returnSound.Play();
        StartCoroutine(SwitchMenu(multiplayerCanvasGroup, menuButtonsCanvasGroup));
        subToMenu.StartCrossfade();
        UpdateNetworkTransport(NetworkTransport.SINGLEPLAYER);
        CurrentMenu = MenuState.Main;
    }

    public void OnClick_ReturnFromMultiplayerBrowser()
    {
        returnSound.Play();
        CurrentMenu = MenuState.Multiplayer;
    }

    public void OnClick_ReturnFromMultiplayerLobby()
    {
        returnSound.Play();
        USteamClient.Instance.LeaveLobby();
        CurrentMenu = MenuState.Multiplayer;
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
            case MenuState.Multiplayer:
                OnClick_ReturnFromMultiplayer();
                break;
            case MenuState.Multiplayer_FindLobby:
                OnClick_ReturnFromMultiplayerBrowser();
                break;
            case MenuState.Multiplayer_Lobby:
                OnClick_ReturnFromMultiplayerLobby();
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

    public void UpdateNetworkTransport(NetworkTransport transport)
    {
        GameObject networkManager = GameObject.Find("NetworkManager"); // Because this is a do not destroy

        // Reset transport
        if(networkManager.GetComponent<SinglePlayerTransport>() != null)
            Destroy(networkManager.GetComponent<SinglePlayerTransport>());

        if(networkManager.GetComponent<UnityTransport>() != null)
            Destroy(networkManager.GetComponent<UnityTransport>());

        // Steam TODO ADD
        if (networkManager.GetComponent<SteamNetworkingSocketsTransport>() != null)
            Destroy(networkManager.GetComponent<SteamNetworkingSocketsTransport>());

        Unity.Netcode.NetworkTransport newTransport = null;

        switch (transport)
        {
            case NetworkTransport.SINGLEPLAYER:
                newTransport = networkManager.AddComponent<SinglePlayerTransport>();
                break;

            case NetworkTransport.UNITY:
                newTransport = networkManager.AddComponent<UnityTransport>();
                break;

            case NetworkTransport.STEAM:
                newTransport = networkManager.AddComponent<SteamNetworkingSocketsTransport>();
                break;
        }

        NetworkManager nm = NetworkManager.Singleton;
        nm.NetworkConfig.NetworkTransport = newTransport;

        nm.NetworkConfig.PlayerPrefab = DefaultPlayerPrefab;

        nm.NetworkConfig.Prefabs.NetworkPrefabsLists.Clear();
        nm.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(NetworkPrefabsList);
    }

    /// <summary>
    /// When joining a lobby update the host ID to this
    /// </summary>
    /// <param name="hostId"></param>
    public void UpdateLobbyHost(ulong hostId)
    {
        SteamNetworkingSocketsTransport transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as SteamNetworkingSocketsTransport;
        if (transport != null)
        {
            transport.ConnectToSteamID = hostId;
        }
    }

    public void UpdateMenu(MenuState menu)
    {
        CurrentMenu = menu;
    }
}