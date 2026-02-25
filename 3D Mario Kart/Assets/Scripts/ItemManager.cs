using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    public enum ItemSlot { Primary, Secondary }

    private Player player;

    [System.Serializable]
    public class ItemSlotItem
    {
        public ItemBase equippedItem;
        public bool selecting;
        public bool selected;
        public GameObject prefab;

        public bool HasVisual()
        {
            return prefab != null;
        }

        public bool HasItemEquipped() { 
            return equippedItem != null;
        }
    }

    public ItemSlot CurrentItemSlot = ItemSlot.Primary;
    private ItemSlotItem PrimarySlot = new();
    private ItemSlotItem SecondarySlot = new();

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

        if (Input.GetKeyDown(KeyCode.C))
        {
            ResetUI(ItemSlot.Secondary);
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
        ItemSlotItem itemSlotItem = GetItemSlotItem(slot);
        if (itemSlotItem.selecting || itemSlotItem.selected)
            return;

        StartCoroutine(GetRandomItem(slot));
    }

    public void SelectItemAuto()
    {
        // Prioritize Primary first
        if (!PrimarySlot.selected && !PrimarySlot.selected)
        {
            SelectItem(ItemSlot.Primary);
            return;
        }

        // If Primary is occupied, try Secondary
        if (!SecondarySlot.selected && !SecondarySlot.selecting)
        {
            SelectItem(ItemSlot.Secondary);
            return;
        }

        // Both slots are busy or filled — do nothing
    }

    IEnumerator GetRandomItem(ItemSlot slot)
    {
        ItemSlotItem itemSlotItem = GetItemSlotItem(slot);
        itemSlotItem.selecting = true;

        ItemUI ui = GetUI(slot);

        SelectSound.Play();

        int itemIndex = itemDistributionManager.getItemNumber();
        itemIndex = Mathf.Clamp(itemIndex, 0, items.Count - 1);

        ui.OurItem.sprite = items[itemIndex].itemGraphic;

        ui.Main.SetBool("StartSelecting", true);
        ui.List.SetBool("Scroll", true);

        yield return new WaitForSeconds(4f);

        itemSlotItem.selecting = false;

        GameObject selectedPrefab = items[itemIndex].itemPrefab;

        // Always store the prefab
        itemSlotItem.prefab = selectedPrefab;

        // ONLY equip if Primary
        if (slot == ItemSlot.Primary)
        {
            EquipItem(selectedPrefab);
        }

        SelectSound.Stop();
        ItemSelectedSound.Play();

        itemSlotItem.selected = true;
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

        PrimarySlot.equippedItem = item;

        player.hasitem = true;
        player.Driver.SetBool("hasItem", true);
    }

    public void UseItem(bool forward)
    {
        ItemSlot primary = ItemSlot.Primary;
        ItemSlot secondary = ItemSlot.Secondary;

        if (!PrimarySlot.HasItemEquipped())
            return;

        ItemBase primaryItem = PrimarySlot.equippedItem;
        if (primaryItem == null)
            return;

        primaryItem.Use(forward);

        ConsumeItem(primary);

        if (/*SecondarySlot.HasItemEquipped()*/ SecondarySlot.selected)
        {
            PromoteSecondaryToPrimary();
        }
    }

    private void ConsumePrimaryVisual()
    {
        if (PrimarySlot.HasVisual())
            Destroy(PrimarySlot.prefab);

        PrimarySlot.equippedItem = null;
    }

    public void ConsumeItem(ItemSlot slot = ItemSlot.Primary, bool shouldDestroy = true)
    {
        ItemSlotItem itemSlotItem = GetItemSlotItem(slot);
        // Safety check
        if (!itemSlotItem.HasItemEquipped())
            return;

        ItemBase item = itemSlotItem.equippedItem;
        if (item == null)
            return;

        if (shouldDestroy)
        {
            itemSlotItem.prefab = null; // Clear the prefab reference since it's being destroyed
            Destroy(item.gameObject);
        }

        itemSlotItem.equippedItem = null;

        // Reset UI + state
        itemSlotItem.selected = false;
        itemSlotItem.selecting = false;

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
        ItemSlotItem itemSlotPrimary = GetItemSlotItem(primary);
        ItemSlotItem itemSlotSecondary = GetItemSlotItem(secondary);

        ItemBase secondaryItem = itemSlotSecondary.equippedItem;

        if (secondaryItem == null)
            return;

        // Move reference
        itemSlotPrimary.equippedItem = secondaryItem;
        itemSlotSecondary.equippedItem = null;

        // Update state
        itemSlotPrimary.selected = true;
        itemSlotSecondary.selected = false;

        // Update UI animations
        Secondary.Main.SetBool("StartSelecting", false);
        Secondary.List.SetBool("Scroll", false);

        Primary.Main.SetBool("StartSelecting", true);
        Primary.List.SetBool("Scroll", true);

        // Update primary sprite with secondary and reset secondary
        Primary.OurItem.sprite = Secondary.OurItem.sprite;
        Secondary.OurItem.sprite = null;
    }

    public void ResetUI(ItemSlot slot)
    {
        ItemSlotItem itemSlotItem = GetItemSlotItem(slot);

        itemSlotItem.selected = false;
        itemSlotItem.selecting = false;

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

    public ItemSlotItem GetItemSlotItem(ItemSlot slot)
    {
        return slot == ItemSlot.Primary ? PrimarySlot : SecondarySlot;
    }
}