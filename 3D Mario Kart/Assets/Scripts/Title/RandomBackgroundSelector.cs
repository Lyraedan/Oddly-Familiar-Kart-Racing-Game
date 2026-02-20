using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomBackgroundSelector : MonoBehaviour
{
    public Image imageA;
    public Image imageB;
    public List<Sprite> backgrounds = new List<Sprite>();
    public float changeInterval = 5f;
    public float fadeDuration = 1f;

    private Image currentImage;
    private Image nextImage;
    private Sprite lastSprite = null;

    private void Start()
    {
        if (backgrounds.Count == 0) return;

        currentImage = imageA;
        nextImage = imageB;

        lastSprite = backgrounds[Random.Range(0, backgrounds.Count)];
        currentImage.sprite = lastSprite;
        currentImage.color = new Color(1f, 1f, 1f, 1f);
        nextImage.color = new Color(1f, 1f, 1f, 0f);

        StartCoroutine(BackgroundChanger());
    }

    private IEnumerator BackgroundChanger()
    {
        while (true)
        {
            Sprite newSprite;
            do
            {
                newSprite = backgrounds[Random.Range(0, backgrounds.Count)];
            } while (newSprite == lastSprite && backgrounds.Count > 1);

            nextImage.sprite = newSprite;

            yield return StartCoroutine(CrossFade(currentImage, nextImage));

            Image temp = currentImage;
            currentImage = nextImage;
            nextImage = temp;

            lastSprite = newSprite;

            yield return new WaitForSeconds(changeInterval);
        }
    }

    private IEnumerator CrossFade(Image fromImage, Image toImage)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            fromImage.color = new Color(1f, 1f, 1f, 1f - t);
            toImage.color = new Color(1f, 1f, 1f, t);

            yield return null;
        }

        fromImage.color = new Color(1f, 1f, 1f, 0f);
        toImage.color = new Color(1f, 1f, 1f, 1f);
    }
}