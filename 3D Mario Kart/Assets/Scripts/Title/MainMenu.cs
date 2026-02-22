using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject startTextObject;
    public GameObject logoPanel;
    public GameObject mainMenuPanel;

    bool canStart = false;
    bool startPressed = false;

    [Header("Start Game Sequence")]
    public Animator logoPanelAnimator;
    public Animator mainMenuLeftAnimator;
    public Animator mainMenuRightAnimator;

    void Start()
    {
        StartCoroutine(WaitToStart());
    }

    void Update()
    {
        if(canStart && Input.GetKeyDown(KeyCode.Space))
        {
            startPressed = true;
            mainMenuLeftAnimator.SetBool("Returned", false);
            mainMenuRightAnimator.SetBool("Returned", false);
            logoPanelAnimator.SetBool("Returned", false);

            logoPanelAnimator.SetBool("StartPressed", true);
        }

        if (startPressed && Input.GetKeyDown(KeyCode.Escape))
        {
            startPressed = false;
            logoPanelAnimator.SetBool("StartPressed", false);
            mainMenuLeftAnimator.SetBool("ShouldShow", false);
            mainMenuRightAnimator.SetBool("ShouldShow", false);

            logoPanelAnimator.SetBool("Returned", true);
            mainMenuLeftAnimator.SetBool("Returned", true);
            mainMenuRightAnimator.SetBool("Returned", true);
        }
    }

    IEnumerator WaitToStart()
    {
        canStart = false;
        startTextObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        canStart = true;
        startTextObject.SetActive(true);
        yield return new WaitUntil(() => startPressed);
        // Play sound
        mainMenuLeftAnimator.SetBool("ShouldShow", true);
        mainMenuRightAnimator.SetBool("ShouldShow", true);
    }
}
