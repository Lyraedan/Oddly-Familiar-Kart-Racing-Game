using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class ResultsUI : MonoBehaviour
{
    private int playerResult;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
  
    }

    public void createResults(List<LapCounter> players)
    {
        foreach (LapCounter player in players)
        {
            MKWKartCustomization customization = player.GetComponent<MKWKartCustomization>();
            RacerConfig config = customization.CurrentRacerConfig;
            ResultsEntry entry = transform.GetChild(player.Position - 1).GetComponent<ResultsEntry>();
            entry.UpdateEntry(player.Position, config.CharacterName, config.CharacterIcon);

            Player p = player.GetComponent<Player>();
            if (p)
            {
                if(p.IsMine)
                    playerResult = player.Position - 1;
            }
        }

        GetComponent<Animator>().SetBool("RaceEnd", true);
        StartCoroutine(yellowPlayerResult());
    }

    public IEnumerator yellowPlayerResult()
    {
        yield return new WaitForSeconds(1);

        Color color = new Color(1, 1, 1, 1);
        color.a = 1;
        transform.GetChild(playerResult).GetComponent<Image>().color = color;
        transform.GetChild(playerResult).GetComponent<UIGradient>().enabled = true;
    }
}
