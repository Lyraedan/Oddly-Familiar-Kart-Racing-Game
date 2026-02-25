using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    public enum ItemSlot { Primary, Secondary }

    private Player player;

    private Dictionary<ItemSlot, ItemBase> equippedItems = new();
    private Dictionary<ItemSlot, bool> itemSelecting = new();
    private Dictionary<ItemSlot, bool> itemSelected = new();
    private Dictionary<ItemSlot, GameObject> storedItemPrefabs = new();

    public ItemSlot CurrentItemSlot = ItemSlot.Primary;

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

    [System.Serializable]
    public struct ItemUI
    {
        public Animator Main;
        public Animator List;
        public Image OurItem;
    }

    public ItemUI Primary;
    public ItemUI Secondary;

    [Header("Sounds")]
    public AudioSource SelectSound;
    public AudioSource ItemSelectedSound;
    public AudioSource CoinSparkle;

    [Header("Power-ups")]
    public bool StarPowerUp;
    public Material starMat;
    public bool isBullet;
    public bool canUseBulletAntigravity;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        player = GetComponent<Player>();

        foreach (ItemSlot slot in System.Enum.GetValues(typeof(ItemSlot)))
        {
            equippedItems[slot] = null;
            storedItemPrefabs[slot] = null;
            itemSelecting[slot] = false;
            itemSelected[slot] = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SelectItemAuto();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SelectItem(CurrentItemSlot);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchSlot();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseItem(true);
        }

        if (!player.hasitem) return;

        bool use = PlayerControls.GetButtonDown(PlayerControls.USE_ITEM);
        bool back = PlayerControls.GetButtonDown(PlayerControls.THROW_BACK);

        if (use || back)
        {
            UseItem(use);
        }
    }

    // -------------------------------------------------
    // SLOT HELPERS
    // -------------------------------------------------

    private ItemUI GetUI(ItemSlot slot)
    {
        return slot == ItemSlot.Primary ? Primary : Secondary;
    }

    public void SwitchSlot()
    {
        CurrentItemSlot = CurrentItemSlot == ItemSlot.Primary
            ? ItemSlot.Secondary
            : ItemSlot.Primary;
    }

    // -------------------------------------------------
    // ITEM SELECTION
    // -------------------------------------------------

    public void SelectItem(ItemSlot slot)
    {
        if (itemSelecting[slot] || itemSelected[slot])
            return;

        StartCoroutine(GetRandomItem(slot));
    }

    public void SelectItemAuto()
    {
        // Prioritize Primary first
        if (!itemSelected[ItemSlot.Primary] && !itemSelecting[ItemSlot.Primary])
        {
            SelectItem(ItemSlot.Primary);
            return;
        }

        // If Primary is occupied, try Secondary
        if (!itemSelected[ItemSlot.Secondary] && !itemSelecting[ItemSlot.Secondary])
        {
            SelectItem(ItemSlot.Secondary);
            return;
        }

        // Both slots are busy or filled — do nothing
    }

    IEnumerator GetRandomItem(ItemSlot slot)
    {
        itemSelecting[slot] = true;

        ItemUI ui = GetUI(slot);

        SelectSound.Play();

        int itemIndex = itemDistributionManager.getItemNumber();
        itemIndex = Mathf.Clamp(itemIndex, 0, items.Count - 1);

        ui.OurItem.sprite = items[itemIndex].itemGraphic;

        ui.Main.SetBool("StartSelecting", true);
        ui.List.SetBool("Scroll", true);

        yield return new WaitForSeconds(4f);

        itemSelecting[slot] = false;

        GameObject selectedPrefab = items[itemIndex].itemPrefab;

        // Always store the prefab
        storedItemPrefabs[slot] = selectedPrefab;

        // ONLY equip if Primary
        if (slot == ItemSlot.Primary)
        {
            EquipItem(selectedPrefab);
        }

        SelectSound.Stop();
        ItemSelectedSound.Play();

        itemSelected[slot] = true;
    }

    public void EquipItem(GameObject itemPrefab)
    {
        ConsumePrimaryVisual();

        GameObject instance = Instantiate(itemPrefab, player.ShellBack);
        instance.name = $"Equipped_Primary_{instance.name}";

        ItemBase item = instance.GetComponent<ItemBase>();

        if (!item)
        {
            Debug.LogError("Prefab missing ItemBase.");
            Destroy(instance);
            return;
        }

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = item.spawnScale;

        item.SetBackSpawn(player.ShellBack);
        item.SetFrontSpawn(player.ShellFront);
        item.SetHandSpawn(player.ItemHand);
        item.SetThrowSpawn(player.ThrowForward);

        item.Initialize(player, this);

        equippedItems[ItemSlot.Primary] = item;

        player.hasitem = true;
        player.Driver.SetBool("hasItem", true);
    }

    public void UseItem(bool forward)
    {
        ItemSlot primary = ItemSlot.Primary;
        ItemSlot secondary = ItemSlot.Secondary;

        // No primary item → nothing to use
        if (!equippedItems.ContainsKey(primary))
            return;

        ItemBase primaryItem = equippedItems[primary];
        if (primaryItem == null)
            return;

        // Use the primary item
        primaryItem.Use(forward);

        // Consume primary (this destroys it)
        ConsumeItem(primary);

        // If secondary exists, promote it
        if (equippedItems[secondary] != null)
        {
            PromoteSecondaryToPrimary();
        }
    }

    private void ConsumePrimaryVisual()
    {
        if (equippedItems[ItemSlot.Primary] != null)
            Destroy(equippedItems[ItemSlot.Primary].gameObject);

        equippedItems[ItemSlot.Primary] = null;
    }

    public void ConsumeItem(ItemSlot slot = ItemSlot.Primary, bool shouldDestroy = true)
    {
        // Safety check
        if (!equippedItems.ContainsKey(slot))
            return;

        ItemBase item = equippedItems[slot];
        if (item == null)
            return;

        // Destroy visual instance
        if (shouldDestroy)
            Destroy(item.gameObject);

        equippedItems[slot] = null;

        // Reset UI + state
        itemSelected[slot] = false;
        itemSelecting[slot] = false;

        ItemUI ui = GetUI(slot);
        ui.Main.SetBool("StartSelecting", false);
        ui.List.SetBool("Scroll", false);

        // Only Primary controls player animation flags
        if (slot == ItemSlot.Primary)
        {
            player.hasitem = false;
            player.has_item_hold = false;
            player.Driver.SetBool("hasItem", false);
        }
    }

    private void PromoteSecondaryToPrimary()
    {
        ItemSlot primary = ItemSlot.Primary;
        ItemSlot secondary = ItemSlot.Secondary;

        ItemBase secondaryItem = equippedItems[secondary];

        if (secondaryItem == null)
            return;

        // Move reference
        equippedItems[primary] = secondaryItem;
        equippedItems[secondary] = null;

        // Update state
        itemSelected[primary] = true;
        itemSelected[secondary] = false;

        // Update UI animations
        Secondary.Main.SetBool("StartSelecting", false);
        Secondary.List.SetBool("Scroll", false);

        Primary.Main.SetBool("StartSelecting", false);
        Primary.List.SetBool("Scroll", false);
    }

    public void ResetUI(ItemSlot slot)
    {
        itemSelected[slot] = false;
        itemSelecting[slot] = false;

        ItemUI ui = GetUI(slot);

        ui.Main.SetBool("StartSelecting", false);
        ui.List.SetBool("Scroll", false);

        if (slot == ItemSlot.Primary)
        {
            player.hasitem = false;
            player.has_item_hold = false;
            player.Driver.SetBool("hasItem", false);
        }
    }
}