using System;

namespace DwarfClone.Inventory
{
    [Serializable]
    public class ItemStack
    {
        public ItemData item;
        public int count;

        public bool IsEmpty => item == null || count <= 0;

        public ItemStack(ItemData item, int count)
        {
            this.item = item;
            this.count = count;
        }

        public static ItemStack Empty => new ItemStack(null, 0);

        public void Clear()
        {
            item = null;
            count = 0;
        }
    }
}
