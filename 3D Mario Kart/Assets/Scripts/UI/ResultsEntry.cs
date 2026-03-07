using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResultsEntry : MonoBehaviour
{
    public Text Position;
    public Text CharacterName;
    public Image Icon;

    public void UpdateEntry(int position, string characterName, Sprite icon)
    {
        Position.text = position.ToString();
        CharacterName.text = characterName;
        Icon.sprite = icon;
        StartCoroutine(UtilityFunctions.FadeCanvasGroup(GetComponent<CanvasGroup>(), 1, 0.25f)); // Fade in the results entry
    }
}
