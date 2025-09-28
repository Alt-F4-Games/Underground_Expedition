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

        public ItemSO Item => item;
        public int Quantity => quantity;
    }
    
    
}
