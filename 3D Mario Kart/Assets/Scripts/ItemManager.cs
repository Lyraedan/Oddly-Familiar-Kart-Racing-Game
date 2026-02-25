using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    private Player player;

    private ItemBase currentItemInstance;

    [System.Serializable]
    public struct Item
    {
        public string name;
        public Sprite itemGraphic;
        public GameObject itemPrefab;
    }

    public List<Item> items = new();

    [Header("Item UI")]
    public ItemDistributionManager itemDistributionManager;
    public Animator ItemsUIMain;
    public Animator ItemsList;
    public Image OurItem;

    [Header("Sounds")]
    public AudioSource SelectSound;
    public AudioSource ItemSelectedSound;
    public AudioSource CoinSparkle;

    [Header("Power-ups")]
    public bool StarPowerUp;
    public Material starMat;
    public bool isBullet;
    public bool canUseBulletAntigravity;

    private GameObject CurrentItem;

    private bool itemSelecting = false;
    private bool itemSelected = false;

    public bool IsSelectingItem { get { return itemSelecting; } }
    public bool HasItemSelected { get { return itemSelected; } }

    void Start()
    {
        player = GetComponent<Player>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SelectItem();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EquipItem(items[0].itemPrefab);
            player.hasitem = true;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Use forward
            currentItemInstance?.Use(true);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            // Use backward
            currentItemInstance?.Use(false);
        }
        return;
        if (!player.hasitem) return;

        bool use = PlayerControls.GetButtonDown(PlayerControls.USE_ITEM);
        bool back = PlayerControls.GetButtonDown(PlayerControls.THROW_BACK);

        if (use || back)
        {
            currentItemInstance?.Use(use);
        }
    }

    public void SelectItem()
    {
        // Do nothing if we're already selecting or have selected an item
        if (IsSelectingItem || itemSelected)
            return;

        StartCoroutine(GetRandomItem());
    }

    IEnumerator GetRandomItem()
    {
        itemSelecting = true;
        SelectSound.Play();

        int itemIndex = itemDistributionManager.getItemNumber();
        itemIndex = Mathf.Clamp(itemIndex, 0, items.Count - 1); // Clamp to valid range

        OurItem.sprite = items[itemIndex].itemGraphic;

        ItemsUIMain.SetBool("StartSelecting", true);
        ItemsList.SetBool("Scroll", true);
        yield return new WaitForSeconds(4);

        itemSelecting = false;
        GameObject selected = items[itemIndex].itemPrefab;
        EquipItem(selected);
        if(selected.tag != "Non-Hold-Item")
        {
            player.Driver.SetBool("hasItem", true);
            player.has_item_hold = true;
            //tripleItemCount = 0;

            //if (selected.name == "GoldenMushroom")
            //{
            //    GoldenMushroomTimer = 10f;
            //}
        }
        else
        {
            //tripleItemCount = 3; //triple item
        }

        SelectSound.Stop();
        ItemSelectedSound.Play();

        itemSelected = true;
        player.hasitem = true;
        //item_decided = true;
    }

    public void EquipItem(GameObject itemPrefab)
    {
        if (currentItemInstance != null)
            Destroy(currentItemInstance.gameObject);

        GameObject instance = Instantiate(itemPrefab, player.ShellBack);
        instance.name = "Equipped_" + instance.name;
        currentItemInstance = instance.GetComponent<ItemBase>();

        // Zero in the item
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = currentItemInstance.spawnScale;

        if(!currentItemInstance)
        {
            Debug.LogError("The equipped item prefab does not have an ItemBase component.");
            Destroy(instance);
            return;
        }

        // Set spawn points for the item
        currentItemInstance.SetBackSpawn(player.ShellBack);
        currentItemInstance.SetFrontSpawn(player.ShellFront);
        currentItemInstance.SetHandSpawn(player.ItemHand);
        currentItemInstance.SetThrowSpawn(player.ThrowForward);

        currentItemInstance.Initialize(player, this);
        CurrentItem = instance;
    }

    public void ConsumeItem(bool ShouldDestroy = true)
    {
        ResetUI();

        if (currentItemInstance != null && ShouldDestroy)
            Destroy(currentItemInstance.gameObject);

        currentItemInstance = null;
    }

    public void ResetUI()
    {
        player.hasitem = false;
        player.has_item_hold = false;
        itemSelected = false;
        itemSelecting = false;
        ItemsUIMain.SetBool("StartSelecting", false);
        player.Driver.SetBool("hasItem", false);
        ItemsList.SetBool("Scroll", false);
    }
}