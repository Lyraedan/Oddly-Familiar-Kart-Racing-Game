using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform logoPanel;
    public RectTransform leftPanel;
    public RectTransform rightPanel;
    public GameObject startTextObject;

    [Header("Animation Speeds")]
    public float logoMoveTime = 0.35f;
    public float sideMoveTime = 0.15f;

    bool canStart = false;
    bool isMenuOpen = false;
    bool isAnimating = false;

    void Start()
    {
        // Initial state → Logo shown, menus hidden
        logoPanel.anchoredPosition = new Vector2(0f, 0f);
        leftPanel.anchoredPosition = new Vector2(0f, 1080f);
        rightPanel.anchoredPosition = new Vector2(0f, -1080f);

        StartCoroutine(WaitToStart());
    }

    void Update()
    {
        if (!canStart || isAnimating)
            return;

        if (!isMenuOpen && Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(ToggleMenu(true));

        if (isMenuOpen && Input.GetKeyDown(KeyCode.Escape))
            StartCoroutine(ToggleMenu(false));
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

        // Different speeds here
        Coroutine c1 = StartCoroutine(MoveUI(logoPanel, logoTarget, logoMoveTime));
        Coroutine c2 = StartCoroutine(MoveUI(leftPanel, leftTarget, sideMoveTime));
        Coroutine c3 = StartCoroutine(MoveUI(rightPanel, rightTarget, sideMoveTime));

        yield return c1;
        yield return c2;
        yield return c3;

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
}