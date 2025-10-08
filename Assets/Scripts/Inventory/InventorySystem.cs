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

    
}