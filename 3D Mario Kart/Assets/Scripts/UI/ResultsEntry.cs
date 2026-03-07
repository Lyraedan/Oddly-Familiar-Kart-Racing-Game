using System;
using System.Collections;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ResultsEntry : MonoBehaviour
{
    public LapCounter assignedRacer;
    public Text Position;
    public Text CharacterName;
    public Image Icon;

    public void AssignRacer(LapCounter player)
    {
        assignedRacer = player;
        assignedRacer.onPositionChanged += (position) => 
        {
            if (!assignedRacer.RaceComplete)
                transform.SetSiblingIndex(position - 1); // When the position changes, move the entry to the correct spot in the list
            else
                transform.SetSiblingIndex(player.RaceEndPosition - 1); // Fix where the racer finished
        };

        StartCoroutine(HideUntilFinished());
    }

    public void UpdateEntry(int position, string characterName, Sprite icon)
    {
        Position.text = position.ToString();
        CharacterName.text = characterName;
        Icon.sprite = icon;
    }

    IEnumerator HideUntilFinished()
    {
        // Wait for them to finish the race
        while (!assignedRacer.RaceComplete)
        {
            yield return null;
        }
        // Wait for the LocalPlayer to be finished
        yield return new WaitUntil(() => RaceManager.Instance.LocalPlayerLap.RaceComplete);
        MKWKartCustomization customization = assignedRacer.GetComponent<MKWKartCustomization>();
        RacerConfig config = customization.CurrentRacerConfig;
        int resultsIndex = assignedRacer.RaceEndPosition - 1;

        UpdateEntry(assignedRacer.RaceEndPosition, config.CharacterName, config.CharacterIcon);
        transform.SetSiblingIndex(resultsIndex); // Assign the entry to the correct spot in the list based on their finishing position

        // Fade the results entry in
        StartCoroutine(UtilityFunctions.FadeCanvasGroup(GetComponent<CanvasGroup>(), 1, 0.25f));
    }
}
