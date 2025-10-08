using System.Collections.Generic;
using UnityEngine;

public enum SlotType { Base, Equip, Hotbar }

[System.Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int quantity;
}

public class InventorySystem : MonoBehaviour
{
    [Header("Capacities")]
    [SerializeField] private int baseCapacity = 3;   
    [SerializeField] private int equipCapacity = 3;  
    [SerializeField] private int hotbarCapacity = 3; 
    [SerializeField] private int maxBaseCapacity = 9; 

    private List<InventorySlot> baseSlots = new();
    private List<InventorySlot> equipSlots = new();
    private List<InventorySlot> hotbarSlots = new();

    public System.Action OnInventoryChanged;

    private void Awake()
    {
        EnsureCapacity(baseSlots, baseCapacity);
        EnsureCapacity(equipSlots, equipCapacity);
        EnsureCapacity(hotbarSlots, hotbarCapacity);
    }

    private void EnsureCapacity(List<InventorySlot> list, int capacity)
    {
        while (list.Count < capacity)
            list.Add(new InventorySlot());
    }

    public int GetCapacityPublic(SlotType type) => type switch
    {
        SlotType.Base => baseCapacity,
        SlotType.Equip => equipCapacity,
        SlotType.Hotbar => hotbarCapacity,
        _ => 0
    };

    private List<InventorySlot> GetList(SlotType type) => type switch
    {
        SlotType.Base => baseSlots,
        SlotType.Equip => equipSlots,
        SlotType.Hotbar => hotbarSlots,
        _ => baseSlots
    };

    public List<InventorySlot> GetOrderedSlots(SlotType type)
    {
        var list = GetList(type);
        int capacity = GetCapacityPublic(type);
        var result = new List<InventorySlot>(capacity);

        for (int i = 0; i < capacity; i++)
            result.Add(i < list.Count ? list[i] : new InventorySlot());

        return result;
    }

    public bool IsValidSlotType(ItemSO item, SlotType targetSlot)
    {
        if (item == null) return false;
        return item.itemType switch
        {
            ItemType.Pickup => targetSlot == SlotType.Base,
            ItemType.ToolActive => targetSlot == SlotType.Base || targetSlot == SlotType.Hotbar,
            ItemType.ToolPassive => targetSlot == SlotType.Base || targetSlot == SlotType.Equip,
            _ => false
        };
    }

    public bool TryAddItem(ItemSO item, int qty, SlotType slotType, bool ignoreValidation = false)
    {
        if (item == null || qty <= 0) return false;
        if (!ignoreValidation && !IsValidSlotType(item, slotType)) return false;

        var list = GetList(slotType);
        int capacity = GetCapacityPublic(slotType);

        for (int i = 0; i < list.Count; i++)
        {
            var slot = list[i];
            if (slot.item == item)
            {
                if (slot.quantity >= item.maxStack)
                    return false;

                int space = item.maxStack - slot.quantity;
                int add = Mathf.Min(space, qty);
                slot.quantity += add;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        for (int i = 0; i < capacity; i++)
        {
            if (i >= list.Count) list.Add(new InventorySlot());
            var slot = list[i];
            if (slot.item == null)
            {
                int add = Mathf.Min(qty, item.maxStack);
                slot.item = item;
                slot.quantity = add;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public bool TryRemoveQuantity(ItemSO item, int qty, SlotType slotType)
    {
        var list = GetList(slotType);
        for (int i = 0; i < list.Count; i++)
        {
            var slot = list[i];
            if (slot.item == item)
            {
                if (slot.quantity < qty) return false;
                slot.quantity -= qty;
                if (slot.quantity == 0) slot.item = null;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    
}