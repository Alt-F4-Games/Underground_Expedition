using System;
using System.Collections.Generic;

[Serializable]
public class SavedInventoryData
{
    public List<SavedSlot> baseSlots = new();
    public List<SavedSlot> equipSlots = new();
    public List<SavedSlot> hotbarSlots = new();
}

[Serializable]
public class SavedSlot
{
    public int itemId;
    public int quantity;

    public SavedSlot(int id, int qty)
    {
        itemId = id;
        quantity = qty;
    }
}