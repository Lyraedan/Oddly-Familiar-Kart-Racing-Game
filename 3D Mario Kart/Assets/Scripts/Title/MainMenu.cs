using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
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

    bool canStart = false;
    bool isMenuOpen = false;
    bool isSubmenuOpen = false;
    bool isAnimating = false;

    void Start()
    {
        // Initial state → Logo shown, menus hidden
        logoPanel.anchoredPosition = new Vector2(0f, 0f);
        leftPanel.anchoredPosition = new Vector2(0f, 1080f);
        rightPanel.anchoredPosition = new Vector2(0f, -1080f);

        // Start faded out
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
    }

    void Update()
    {
        if (!canStart || isAnimating)
            return;

        if (!isMenuOpen && Input.GetKeyDown(KeyCode.Space) && !isSubmenuOpen)
            StartCoroutine(ToggleMenu(true));

        if (isMenuOpen && Input.GetKeyDown(KeyCode.Escape) && !isSubmenuOpen)
            StartCoroutine(ToggleMenu(false));

        if (isSubmenuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            OnClick_ReturnFromSingleplayer();
        }
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
        isMenuOpen = open;

        Vector2 logoTarget = open
            ? new Vector2(-730f, 0f)
            : new Vector2(0f, 0f);

        Vector2 leftTarget = open
            ? new Vector2(0f, 0f)
            : new Vector2(0f, 1080f);

        Vector2 rightTarget = open
            ? new Vector2(0f, 0f)
            : new Vector2(0f, -1080f);

        if (open)
        {
            startGameSound.Play();

            // Slide in first
            Coroutine c1 = StartCoroutine(MoveUI(logoPanel, logoTarget, logoMoveTime));
            Coroutine c2 = StartCoroutine(MoveUI(leftPanel, leftTarget, sideMoveTime));
            Coroutine c3 = StartCoroutine(MoveUI(rightPanel, rightTarget, sideMoveTime));

            yield return c1;
            yield return c2; // Wait for left panel to reach 0
            yield return c3;

            // Then fade in
            if (menuButtonsCanvasGroup != null)
                yield return StartCoroutine(FadeCanvas(menuButtonsCanvasGroup, 1f, fadeTime, true));
        }
        else
        {
            returnSound.Play();

            // Fade out first
            if (menuButtonsCanvasGroup != null)
                yield return StartCoroutine(FadeCanvas(menuButtonsCanvasGroup, 0f, fadeTime, false));

            // Then slide out
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
        // Fade out current menu
        if (from != null)
            yield return StartCoroutine(FadeCanvas(from, 0f, fadeTime, false));

        // Fade in new menu
        if (to != null)
            yield return StartCoroutine(FadeCanvas(to, 1f, fadeTime, true));
    }

    public void OnClick_Singleplayer()
    {
        buttonSelectSound.Play();
        StartCoroutine(SwitchMenu(menuButtonsCanvasGroup, singleplayerCanvasGroup));
        isSubmenuOpen = true;
    }

    public void OnClick_Multiplayer()
    {
        buttonSelectSound.Play();
        Debug.Log("Multiplayer button clicked!");
    }

    public void OnClick_Settings()
    {
        buttonSelectSound.Play();
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
        Debug.Log("Returning from Singleplayer menu");

        StartCoroutine(SwitchMenu(singleplayerCanvasGroup, menuButtonsCanvasGroup));
        isSubmenuOpen = false;
    }
}