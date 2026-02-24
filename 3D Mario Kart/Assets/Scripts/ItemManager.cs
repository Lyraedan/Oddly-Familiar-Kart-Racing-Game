using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

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

    [Header("Sounds")]
    public AudioSource PlaySelectsound;
    public AudioSource coinSparkle;

    [Header("Power-ups")]
    public bool StarPowerUp;
    public Material starMat;
    public bool isBullet;
    public bool canUseBulletAntigravity;

    void Start()
    {
        player = GetComponent<Player>();
    }

    void Update()
    {
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
    }

    public void ConsumeItem(bool ShouldDestroy = true)
    {
        player.hasitem = false;

        if (currentItemInstance != null && ShouldDestroy)
            Destroy(currentItemInstance.gameObject);

        currentItemInstance = null;
    }
}