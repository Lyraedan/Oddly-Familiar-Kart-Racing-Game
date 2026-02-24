using UnityEngine;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour
{
    public Sprite characterHeadSprite;

    public Image shadow;
    public Image characterHead;

    public bool IsOurIcon = false;

    void Start()
    {
        if (!IsOurIcon)
        {
            Color shadowColor = shadow.color;
            shadowColor.a = 0f; // Hide the shadow
            shadow.color = shadowColor;
        }
    }

    public void UpdateSprite(Sprite newSprite)
    {
        // No new sprite ignore.
        if (newSprite == null)
        {
            Debug.LogWarning("MinimapIcon: No new sprite provided for update. On: " + gameObject.name);
            return;
        }

        characterHead.sprite = newSprite;
        characterHeadSprite = newSprite;
    }
}
