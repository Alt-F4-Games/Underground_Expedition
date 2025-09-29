using UnityEngine;

[System.Serializable]
public class Inventory
{
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    [SerializeField] private int maxSlots = 8;

    [System.Serializable]
    public class InventorySlot
    {
        [SerializeField] private ItemSO item;
        [SerializeField] private int quantity = 1;
        
        public InventorySlot(ItemSO item, int quantity)
        {
            this.item = item;
            this.quantity = quantity > 0 ? quantity : 1; 
        }

        public ItemSO Item => item;
        public int Quantity => quantity;
    }

    public bool AddItem(ItemSO item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        
        foreach (var slot in slots)
        {
            if (slot.Item == item && slot.Quantity < item.maxStack)
            {
                int spaceLeft = item.maxStack - slot.Quantity;
                int amountToAdd = Mathf.Min(quantity, spaceLeft);
                slot.quantity += amountToAdd;
                quantity -= amountToAdd;
                if (quantity == 0) return true;
            }
        }
        
        while (quantity > 0 && slots.Count < maxSlots)
        {
            int amountToAdd = Mathf.Min(quantity, item.maxStack);
            slots.Add(new InventorySlot(item, amountToAdd));
            quantity -= amountToAdd;
        }

        return quantity == 0;
    }
}
