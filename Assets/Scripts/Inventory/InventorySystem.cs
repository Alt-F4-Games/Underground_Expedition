using System;
using System.Collections.Generic;
using UnityEngine;

public enum SlotType { Base, Hotbar, Equip }

[Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int quantity;

    public InventorySlot(ItemSO item, int quantity)
    {
        this.item = item;
        this.quantity = Mathf.Max(quantity, 1);
    }
}

public class InventorySystem : MonoBehaviour
{
    public event Action OnInventoryChanged;
    
    
    [SerializeField] Dictionary<ItemSO, InventorySlot> _baseSlots   = new Dictionary<ItemSO, InventorySlot>();
    [SerializeField]private Dictionary<ItemSO, InventorySlot> _hotbarSlots = new Dictionary<ItemSO, InventorySlot>();
    [SerializeField]private Dictionary<ItemSO, InventorySlot> _equipSlots  = new Dictionary<ItemSO, InventorySlot>();
    
    
    private int _baseCapacity   = 3;
    private int _hotbarCapacity = 3;
    private int _equipCapacity  = 3;

    
    public bool TryAddItem(ItemSO item, int qty = 1, SlotType slotType = SlotType.Base)
    {
        if (item == null || qty <= 0) return false;
        if (!IsValidSlotType(item, slotType)) return false;

        var dict= GetDict(slotType);
        int capacity = GetCapacity(slotType);

        
        if (dict.TryGetValue(item, out var slot))
        {
            int space = Mathf.Min(qty, item.maxStack - slot.quantity);
            if (space <= 0) return false;
            slot.quantity += space;
            OnInventoryChanged?.Invoke();
            return true;
        }

        
        if (dict.Count < capacity)
        {
            dict[item] = new InventorySlot(item, Mathf.Min(qty, item.maxStack));
            OnInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }
    
    public bool TryRemoveItem(ItemSO item, SlotType slotType = SlotType.Base)
    {
        var dict = GetDict(slotType);
        bool removed = dict.Remove(item);
        if (removed) OnInventoryChanged?.Invoke();
        return removed;
    }

    public bool TryRemoveQuantity(ItemSO item, int qty = 1, SlotType slotType = SlotType.Base)
    {
        if (item == null || qty <= 0) return false;

        var dict = GetDict(slotType);
        if (!dict.TryGetValue(item, out var slot)) return false;
        if (slot.quantity < qty) return false;

        slot.quantity -= qty;
        if (slot.quantity == 0) dict.Remove(item);

        OnInventoryChanged?.Invoke();
        return true;
    }

    public void ExpandBaseCapacity(int newCapacity)
    {
        _baseCapacity = Mathf.Max(_baseCapacity, newCapacity);
        OnInventoryChanged?.Invoke();
    }

    public List<InventorySlot> GetSlots(SlotType slotType)
    {
        return new List<InventorySlot>(GetDict(slotType).Values);
    }

}