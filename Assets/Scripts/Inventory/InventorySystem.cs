using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private Dictionary<ItemSO, InventorySlot> baseSlots   = new Dictionary<ItemSO, InventorySlot>();
    private Dictionary<ItemSO, InventorySlot> hotbarSlots = new Dictionary<ItemSO, InventorySlot>();
    private Dictionary<ItemSO, InventorySlot> equipSlots  = new Dictionary<ItemSO, InventorySlot>();

    private int baseCapacity   = 3;
    private int hotbarCapacity = 3;
    private int equipCapacity  = 3;
}