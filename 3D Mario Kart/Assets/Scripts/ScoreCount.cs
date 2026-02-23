using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ScoreCount : MonoBehaviour
{
    public int COINCOUNT;

    public Color maxCoinColor;
    public Color regCoinColor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(COINCOUNT < 10)
        {
            IngameUIHolder.Instance.coinUI.UpdateText("0" + COINCOUNT);
            IngameUIHolder.Instance.coinUI.top.color = regCoinColor;

        }
        if (COINCOUNT == 10)
        {
            IngameUIHolder.Instance.coinUI.UpdateText(COINCOUNT + "");
            IngameUIHolder.Instance.coinUI.top.color = maxCoinColor;
        }

        if (COINCOUNT > 10)
        {
            COINCOUNT = 10;
        }
    }
}
