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
    private Dictionary<ItemSO, InventorySlot> baseSlots   = new Dictionary<ItemSO, InventorySlot>();
    private Dictionary<ItemSO, InventorySlot> hotbarSlots = new Dictionary<ItemSO, InventorySlot>();
    private Dictionary<ItemSO, InventorySlot> equipSlots  = new Dictionary<ItemSO, InventorySlot>();

    private int baseCapacity   = 3;
    private int hotbarCapacity = 3;
    private int equipCapacity  = 3;
}