public class MultiUseItem : ItemBase
{
    public ItemBase baseItem;
    public int uses = 3;

    public override void Use(bool forward)
    {
        if (uses <= 0) return;

        baseItem.Use(forward);
        uses--;

        if (uses <= 0)
            itemManager.ConsumeItem();
    }
}