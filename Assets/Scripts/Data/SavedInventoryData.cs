using System;
using System.Collections.Generic;

/// <summary>
/// Container for all inventory slots that must be saved locally.
/// This class is serialized to JSON and sent to the server when the player joins.
/// 
/// Contains:
/// - Base inventory slots
/// - Equipment slots
/// - Hotbar slots
///
/// Notes:
/// - Lists allow flexible size, but the NetworkInventorySystem
///   will clamp to its fixed slot capacity when loading.
/// </summary>

[Serializable]
public class SavedInventoryData
{
    public List<SavedSlot> baseSlots = new();   // Stored lists of saved slots (simple ID + quantity).
    public List<SavedSlot> equipSlots = new();  // These lists may be larger than the networked capacity,
    public List<SavedSlot> hotbarSlots = new(); // but the NetworkInventorySystem will only read up to its max size.
}

/// <summary>
/// Represents a single saved item stack.
/// Contains only the raw data needed to reconstruct inventory:
/// - itemId: The ID of the item (0 = empty)
/// - quantity: How many units of that item
/// </summary>

[Serializable]
public class SavedSlot
{
    public int itemId;
    public int quantity;

    public SavedSlot(int id, int qty)   // Creates a saved slot with the given item ID and quantity.
    {
        itemId = id;
        quantity = qty;
    }
}