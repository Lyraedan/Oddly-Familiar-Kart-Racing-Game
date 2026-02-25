using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDistributionManager : MonoBehaviour
{

    /*Player: Bullet: 0, Banana: 1, Greenshell: 2, Coin: 3, Redshell: 4, TripBananas: 5, mushroom: 6, 
        tripGreenShell: 7, TripMushroom: 8, TripRedSHell: 9, Golden mushroom: 10, bobomb: 11, blueshell: 12, star: 13
    */





    /*Opponent: Greenshell: 0, Redhshell: 1, Banana: 2, Blueshell: 3, Bobomb: 4, Bullet: 5, coin: 6, 
        tripGreenShell: 7, TripRedSHell: 8, TripMushrooms: 9, TripBananas: 10, Golden Mushroom: 11, Mushroom: 12, star: 13
    */

    int position;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        position = GetComponent<LapCounter>().Position;
    }

    public int getItemNumber()
    {
        if(gameObject.tag == "Player")
        {
            int item = GetItemForPosition(position);
            return item;
        }
        else //opponent
        {
            int item = GetItemForOpponent(position);
            return item;
        }
    }

    int GetItemForPosition(int position)
    {
        int range;
        int[] probabilities;

        switch (position)
        {
            case 1:
                range = Random.Range(0, 6);
                probabilities = new int[] { 3, 3, 1, 1, 2, 2 }; // coin, coin, banana, banana, green, green
                break;
            case 2:
                range = Random.Range(0, 5);
                probabilities = new int[] { 1, 2, 2, 4, 4 }; // banana, green, green, red, red
                break;
            case 3:
                range = Random.Range(0, 6);
                probabilities = new int[] { 6, 6, 4, 4, 4, 11 }; // mushroom, mushroom, red, red, red, bobomb
                break;
            case 4:
                range = Random.Range(0, 6);
                probabilities = new int[] { 4, 11, 6, 7, 5, 5 }; // red, bobomb, mushroom, triple green, triple banana, triple banana
                break;
            case 5:
                range = Random.Range(0, 6);
                probabilities = new int[] { 7, 6, 9, 9, 8, 8 }; // triple banana, mushroom, triple red, triple red, triple mushroom, triple mushroom
                break;
            case 6:
                range = Random.Range(0, 7);
                probabilities = new int[] { 8, 8, 9, 9, 9, 12, 12 }; // triple mushroom x2, triple red x3, blue x2
                break;
            case 7:
                range = Random.Range(0, 8);
                probabilities = new int[] { 12, 12, 10, 10, 10, 13, 13, 13 }; // blue x2, golden x3, star x3
                break;
            default: // 8th place
                range = Random.Range(0, 8);
                probabilities = new int[] { 13, 13, 10, 10, 10, 0, 0, 0 }; // star x2, golden x3, bullet x3
                break;
        }

        return probabilities[range];
    }

    int GetItemForOpponent(int position)
    {
        int range;
        int[] probabilities;

        switch (position)
        {
            case 1:
                range = Random.Range(0, 5);
                probabilities = new int[] { 6, 6, 2, 2, 0 }; // coin x2, banana x2, green
                break;
            case 2:
                range = Random.Range(0, 5);
                probabilities = new int[] { 2, 0, 0, 1, 1 }; // banana, green x2, red x2
                break;
            case 3:
                range = Random.Range(0, 5);
                probabilities = new int[] { 12, 12, 1, 1, 4 }; // mushroom x2, red x2, bobomb
                break;
            case 4:
                range = Random.Range(0, 6);
                probabilities = new int[] { 1, 4, 12, 7, 10, 10 }; // red, bobomb, mushroom, triple green, triple banana x2
                break;
            case 5:
                range = Random.Range(0, 6);
                probabilities = new int[] { 7, 12, 8, 8, 9, 9 }; // triple banana, mushroom, triple red x2, triple mushroom x2
                break;
            case 6:
                range = Random.Range(0, 7);
                if (range < 3)
                    return 9; // triple mushroom
                else if (range < 5)
                    return 8; // triple red shell
                else
                {
                    // Blue shell logic
                    var rm = GameObject.Find("RaceManager").GetComponent<RACE_MANAGER>();
                    if (rm.currentBlueShellCount == 0)
                    {
                        rm.currentBlueShellCount = 1;
                        StartCoroutine(resetBlueShell());
                        return 3; // blue shell
                    }
                    else
                        return 8; // triple red shell
                }
            case 7:
                range = Random.Range(0, 8);
                if (range < 3)
                {
                    var rm = GameObject.Find("RaceManager").GetComponent<RACE_MANAGER>();
                    if (rm.currentBlueShellCount == 0)
                    {
                        rm.currentBlueShellCount = 1;
                        StartCoroutine(resetBlueShell());
                        return 3; // blue shell
                    }
                    else
                        return 13; // star
                }
                else if (range < 5)
                    return 11; // golden mushroom
                else
                    return 13; // star
            default: // 8th place
                range = Random.Range(0, 8);
                probabilities = new int[] { 13, 13, 11, 11, 11, 5, 5, 5 }; // star x2, golden x3, bullet x3
                break;
        }

        return probabilities[range];
    }

    IEnumerator resetBlueShell()
    {
        yield return new WaitForSeconds(10);
        GameObject.Find("RaceManager").GetComponent<RACE_MANAGER>().currentBlueShellCount = 0;
    }
}
