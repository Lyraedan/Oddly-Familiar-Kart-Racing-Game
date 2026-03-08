using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CustomizableSprite : MonoBehaviour
{
    public CustomizableSpriteLibrary.ReplaceableElement element;
    public Image image;
    public Color tint = Color.white;
    public int index;

    public void SetSprite(Sprite sprite)
    {
        if(image == null)
        {
            Debug.LogWarning("CustomizableSprite on " + gameObject.name + " has no Image component reference set.");
            return;
        }

        image.sprite = sprite;
    }

    public void SetTint(Color newTint)
    {
        if (image == null)
        {
            Debug.LogWarning("CustomizableSprite on " + gameObject.name + " has no Image component reference set.");
            return;
        }
        tint = newTint;
        image.color = tint;
    }
}