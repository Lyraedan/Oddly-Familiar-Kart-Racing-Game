using UnityEngine;

public class MultiUseItem : ItemBase
{
    public ItemBase baseItem;
    public int uses = 3;

    public override void Use(bool forward, GameObject user)
    {
        if (uses <= 0) return;

        baseItem.Use(forward, user);
        uses--;

        if (uses <= 0)
            itemManager.ConsumeItem();
    }
}