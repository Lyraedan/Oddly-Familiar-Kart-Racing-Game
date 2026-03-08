using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : NetworkBehaviour
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
            Destroy(this); // Remove the copy off this object
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        player = GetComponent<Player>();
    }

    void Update()
    {
        if (!player.IsMine) return;

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

    private IngameUIHolder.UIItem GetUI(ItemSlot slot)
    {
        return slot == ItemSlot.Primary ? IngameUIHolder.Instance.PrimaryItem : IngameUIHolder.Instance.SecondaryItem;
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
        //ItemSlotItem itemSlotItem = GetItemSlotItem(slot);
        //if (itemSlotItem.selecting || itemSlotItem.selected)
        //    return;

        //StartCoroutine(GetRandomItem(slot));

        SelectItemServerRPC((int) slot);
    }

    public void SelectItemAuto()
    {
        // Prioritize Primary first
        if (!PrimarySlot.selected && !PrimarySlot.selected)
        {
            SelectItem(ItemSlot.Primary);
            return;
        }

        /* Disable secondary for now
        // If Primary is occupied, try Secondary
        if (!SecondarySlot.selected && !SecondarySlot.selecting)
        {
            SelectItem(ItemSlot.Secondary);
            return;
        }*/

        // Both slots are busy or filled — do nothing
    }

    IEnumerator GetRandomItem(ItemSlot slot)
    {
        ItemSlotItem itemSlotItem = GetItemSlotItem(slot);
        itemSlotItem.selecting = true;

        IngameUIHolder.UIItem ui = GetUI(slot);

        SelectSound.Play();

        int itemIndex = 0; //itemDistributionManager.getItemNumber();

        //itemIndex = Mathf.Clamp(itemIndex, 0, items.Count - 1);

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
        if (!IsOwner) return;

        UseItemServerRpc(forward);

        /*
        if (!RaceManager.RACE_STARTED || RaceManager.RACE_COMPLETED)
            return;

        if (!PrimarySlot.HasItemEquipped())
            return;

        ItemBase primaryItem = PrimarySlot.equippedItem;
        if (primaryItem == null)
            return;

        primaryItem.Use(forward, gameObject);

        // Items should call Consume in Use when needed
        //ConsumeItem(primary);

        if (/*SecondarySlot.HasItemEquipped()*/ /*SecondarySlot.selected)
        {
            PromoteSecondaryToPrimary();
        }*/
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void UseItemServerRpc(bool forward)
    {
        if (!PrimarySlot.HasItemEquipped())
            return;

        ItemBase item = PrimarySlot.equippedItem;
        item.Use(forward, gameObject);
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
            item.networkedObject.Despawn();
        }

        itemSlotItem.equippedItem = null;

        // Reset UI + state
        itemSlotItem.selected = false;
        itemSlotItem.selecting = false;

        IngameUIHolder.UIItem ui = GetUI(slot);
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
        IngameUIHolder.Instance.SecondaryItem.Main.SetBool("StartSelecting", false);
        IngameUIHolder.Instance.SecondaryItem.List.SetBool("Scroll", false);

        IngameUIHolder.Instance.PrimaryItem.Main.SetBool("StartSelecting", true);
        IngameUIHolder.Instance.PrimaryItem.List.SetBool("Scroll", true);

        // Update primary sprite with secondary and reset secondary
        IngameUIHolder.Instance.PrimaryItem.OurItem.sprite = IngameUIHolder.Instance.SecondaryItem.OurItem.sprite;
        IngameUIHolder.Instance.SecondaryItem.OurItem.sprite = null;
    }

    public void ResetUI(ItemSlot slot)
    {
        ItemSlotItem itemSlotItem = GetItemSlotItem(slot);

        itemSlotItem.selected = false;
        itemSlotItem.selecting = false;

        IngameUIHolder.UIItem ui = GetUI(slot);

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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SelectItemServerRPC(int slot)
    {
        ItemSlot itemSlot = (ItemSlot)slot;
        StartCoroutine(GetRandomItemServer(itemSlot));
    }

    private IEnumerator GetRandomItemServer(ItemSlot slot)
    {
        ItemSlotItem itemSlotItem = GetItemSlotItem(slot);
        itemSlotItem.selecting = true;
        int itemIndex = 0;

        SelectItemClientRpc(slot, itemIndex);
        yield return new WaitForSeconds(4f);

        GameObject selectedPrefab = items[itemIndex].itemPrefab;

        if(slot == ItemSlot.Primary)
        {
            SpawnItemServer(selectedPrefab);
        }

        itemSlotItem.selected = true;
        SelectedItemClientRpc();
    }

    // Executed by server
    private void SpawnItemServer(GameObject prefab)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("SpawnItemServer called on client!");
            return;
        }

        GameObject instance = Instantiate(prefab);
        instance.name = $"Equipped_Primary_{instance.name}";

        NetworkObject networkObject = instance.GetComponent<NetworkObject>();
        if(networkObject == null)
        {
            Debug.LogError("Prefab missing NetworkObject.");
            Destroy(instance); // Destroy the instance since it can't be networked
            return;
        }

        networkObject.Spawn(true);

        networkObject.TrySetParent(transform, false);

        instance.transform.rotation = Quaternion.identity;

        ItemBase item = instance.GetComponent<ItemBase>();

        item.SetBackSpawn(player.ShellBack);
        item.SetFrontSpawn(player.ShellFront);
        item.SetHandSpawn(player.ItemHand);
        item.SetThrowSpawn(player.ThrowForward);

        item.UpdateHoldPoint(player.ShellBack);
        item.isHeld = true;

        item.Initialize(player, this);
        PrimarySlot.equippedItem = item;
    }

    [Rpc(SendTo.Owner)]
    public void SelectItemClientRpc(ItemSlot slot, int itemIndex)
    {
        IngameUIHolder.UIItem ui = GetUI(slot);
        ui.OurItem.sprite = items[itemIndex].itemGraphic;
        ui.Main.SetBool("StartSelecting", true);
        ui.List.SetBool("Scroll", true);
        SelectSound.Play();
    }

    [Rpc(SendTo.Owner)]
    public void SelectedItemClientRpc()
    {
        SelectSound.Stop();
        ItemSelectedSound.Play();
        player.Driver.SetBool("hasItem", true);
    }
}