using UnityEngine;
using UnityEngine.UI;

public class ItemListPopulator : MonoBehaviour
{
    public int MAX_ITEMS = 43;
    public GameObject iconPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Populate();
    }

    private void Populate()
    {
        int items_count = ItemManager.Instance.items.Count;
        for (int i = 0; i < MAX_ITEMS; i++)
        {
            int index = i % items_count;
            ItemManager.Item item = ItemManager.Instance.items[index];
            GameObject iconObject = Instantiate(iconPrefab, transform);
            iconObject.name = "ItemIcon_" + item.itemPrefab.name;
            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = item.itemGraphic;

            iconObject.transform.localPosition += new Vector3(0f, -69f * i, 0f);
        }
    }

}
