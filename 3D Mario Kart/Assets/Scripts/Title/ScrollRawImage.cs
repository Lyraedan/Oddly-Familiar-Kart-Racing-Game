using UnityEngine;
using UnityEngine.UI;

public class ScrollRawImage : MonoBehaviour
{
    public RawImage rawImage;

    [Header("Scroll speed in UV units per second")]
    public Vector2 scrollSpeed = new Vector2(-0.2f, 0.2f); // X = left, Y = up

    private Rect uvRect;

    void Start()
    {
        if (rawImage != null)
            uvRect = rawImage.uvRect;
    }

    void Update()
    {
        if (rawImage == null) return;

        uvRect.x += scrollSpeed.x * Time.deltaTime;
        uvRect.y += scrollSpeed.y * Time.deltaTime;

        // Wrap to keep neat values
        uvRect.x %= 1f;
        uvRect.y %= 1f;

        rawImage.uvRect = uvRect;
    }
}