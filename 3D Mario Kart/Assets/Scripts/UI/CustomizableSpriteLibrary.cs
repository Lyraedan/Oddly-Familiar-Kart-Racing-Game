using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomizableSpriteLibrary : MonoBehaviour
{
    // TODO Update this from just updating sprites to also models and prefab replacements

    public static CustomizableSpriteLibrary Instance { get; private set; }

    public enum ReplaceableElement
    {
        PLACEMENT_NUMBER,
        ITEM_BOX_BORDER,
        ITEM_BOX_MASK
    }

    [System.Serializable]
    public class ElementSpriteSet
    {
        public ReplaceableElement element;
        public List<Sprite> sprites;
    }

    public List<ElementSpriteSet> spriteSets = new();

    private Dictionary<ReplaceableElement, List<Sprite>> spriteLookup;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        BuildLookup();
    }

    private void BuildLookup()
    {
        spriteLookup = new();

        foreach (var set in spriteSets)
        {
            spriteLookup[set.element] = set.sprites;
        }
    }

    public Sprite GetSprite(ReplaceableElement element, int index = 0)
    {
        if (!spriteLookup.ContainsKey(element))
            return null;

        var list = spriteLookup[element];

        if (index < 0 || index >= list.Count)
            return null;

        return list[index];
    }

    public void AssignSpritesInScene()
    {
        var sprites = FindObjectsOfType<CustomizableSprite>(true);
        Debug.Log("Found " + sprites.Length + " customizable sprites in scene.");

        foreach (CustomizableSprite sprite in sprites)
        {
            var s = GetSprite(sprite.element, sprite.index);

            if (s != null)
                sprite.SetSprite(s);

            sprite.SetTint(sprite.tint);
        }
    }
}