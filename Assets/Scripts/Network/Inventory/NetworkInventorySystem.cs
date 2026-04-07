using Fusion;
using UnityEngine;
using System;

/// <summary>
/// NETWORKED INVENTORY SYSTEM (Fusion)
/// --------------------------------------
/// This script stores and synchronizes the 3 categories of inventory:
/// - BaseSlots   → general inventory items
/// - EquipSlots  → equipped items (armor, tools, weapons)
/// - HotbarSlots → quick access bar (visible slots)
///
/// NOTE FOR NON-PROGRAMMERS:
/// This script ensures everyone sees the same inventory while playing online.
/// This script handles NO UI and NO graphics — only synchronized data.
///
/// NOTE FOR PROGRAMMERS:
/// Only the server/state authority is allowed to modify the arrays
/// (Server_* methods). Clients only read them through Fusion replication.
/// </summary>

public class NetworkInventorySystem : NetworkBehaviour
{
    // Fusion requires fixed sizes for NetworkArray
    private const int MAX_BASE = 3;
    private const int MAX_EQUIP = 3;
    private const int MAX_HOTBAR = 3;

    // ----------------------------
    //        NETWORKED DATA
    // ----------------------------

    [Networked, Capacity(MAX_BASE)]
    public NetworkArray<NetworkInventorySlot> BaseSlots { get; }

    [Networked, Capacity(MAX_EQUIP)]
    public NetworkArray<NetworkInventorySlot> EquipSlots { get; }

    [Networked, Capacity(MAX_HOTBAR)]
    public NetworkArray<NetworkInventorySlot> HotbarSlots { get; }

    // ----------------------------
    //         LOCAL EVENTS
    // ----------------------------

    public event Action OnInventoryChanged; // UI systems subscribe to refresh when the inventory changes.

    private ChangeDetector _changes;

    public override void Spawned()
    {
        // Trigger initial refresh so UI can display current data.
        OnInventoryChanged?.Invoke();
    }

    // ------------------------------------------------------------
    // Fusion calls Render() every frame on each client.
    // Here we detect actual changes in the Networked arrays.
    // ------------------------------------------------------------
    public override void Render()
    {
        if (_changes == null)
            _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(BaseSlots) ||
                change == nameof(EquipSlots) ||
                change == nameof(HotbarSlots))
            {
                OnInventoryChanged?.Invoke();
                return; // avoid notifying multiple times in the same frame
            }
        }
    }

    // =====================================================================
    //                        SERVER-SIDE LOGIC
    // =====================================================================

    public bool Server_TryAddItem(int itemId, int quantity, SlotType type)  // Adds an item respecting stack limits. Can only be executed by State Authority (server/host).
    {
        if (!HasStateAuthority || quantity <= 0)
            return false;

        var slots = GetArrayByType(type);

        // 1) Try stacking into existing compatible slots
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (!s.IsEmpty && s.ItemId == itemId)
            {
                s.Quantity += quantity;
                slots.Set(i, s);
                return true;
            }
        }

        // 2) Find empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots.Set(i, new NetworkInventorySlot(itemId, quantity));
                return true;
            }
        }

        return false;
    }

    public bool Server_TryRemoveItem(int itemId, int quantity, SlotType type)   // Removes items from slots. Returns false if not enough quantity is found.
    {
        if (!HasStateAuthority)
            return false;

        var array = GetArrayByType(type);
        int remaining = quantity;

        for (int i = 0; i < array.Length; i++)
        {
            var slot = array[i];

            if (slot.ItemId != itemId)
                continue;

            if (slot.Quantity >= remaining)
            {
                slot.Quantity -= remaining;
                if (slot.Quantity <= 0)
                    slot = new NetworkInventorySlot(0, 0);

                array.Set(i, slot);
                return true;
            }

            // Empty slot and continue removing from others
            remaining -= slot.Quantity;
            array.Set(i, new NetworkInventorySlot(0, 0));
        }

        return false;
    }

    public void Server_MoveItem(SlotType fromType, int fromIdx, SlotType toType, int toIdx)     // Moves or combines items between any slots.
    {
        if (!HasStateAuthority)
            return;

        var fromArray = GetArrayByType(fromType);
        var toArray = GetArrayByType(toType);

        if (!IndexValid(fromArray, fromIdx) || !IndexValid(toArray, toIdx))
            return;

        var from = fromArray[fromIdx];
        var to = toArray[toIdx];

        if (from.IsEmpty)
            return;

        // If same item type → try stacking
        if (to.ItemId == from.ItemId)
        {
            var itemData = ItemDatabase.Instance.GetItemById(from.ItemId);
            int maxStack = itemData?.maxStack ?? 99;

            int space = maxStack - to.Quantity;
            if (space > 0)
            {
                int moved = Mathf.Min(space, from.Quantity);

                to.Quantity += moved;
                from.Quantity -= moved;

                if (from.Quantity <= 0)
                    from = new NetworkInventorySlot(0, 0);

                toArray.Set(toIdx, to);
                fromArray.Set(fromIdx, from);
                return;
            }
        }

        // Simple swap
        fromArray.Set(fromIdx, to);
        toArray.Set(toIdx, from);
    }

    // =====================================================================
    //                             HELPERS
    // =====================================================================

    private bool IndexValid(NetworkArray<NetworkInventorySlot> arr, int index)
        => index >= 0 && index < arr.Length;

    private NetworkArray<NetworkInventorySlot> GetArrayByType(SlotType type)
    {
        return type switch
        {
            SlotType.Base => BaseSlots,
            SlotType.Equip => EquipSlots,
            SlotType.Hotbar => HotbarSlots,
            _ => BaseSlots
        };
    }

    public NetworkInventorySlot GetSlotData(SlotType type, int index)
    {
        var arr = GetArrayByType(type);
        return IndexValid(arr, index) ? arr[index] : new NetworkInventorySlot(0, 0);
    }

    public int GetCapacity(SlotType type) => GetArrayByType(type).Length;

    // =====================================================================
    //                             SAVE / LOAD
    // =====================================================================

    public SavedInventoryData ToSavedData()
    {
        var data = new SavedInventoryData();

        // Copy BaseSlots
        for (int i = 0; i < BaseSlots.Length; i++)
            data.baseSlots.Add(new SavedSlot(BaseSlots[i].ItemId, BaseSlots[i].Quantity));

        // Copy EquipSlots
        for (int i = 0; i < EquipSlots.Length; i++)
            data.equipSlots.Add(new SavedSlot(EquipSlots[i].ItemId, EquipSlots[i].Quantity));

        // Copy HotbarSlots
        for (int i = 0; i < HotbarSlots.Length; i++)
            data.hotbarSlots.Add(new SavedSlot(HotbarSlots[i].ItemId, HotbarSlots[i].Quantity));

        return data;
    }

    public void LoadFromSavedData(SavedInventoryData data)
    {
        if (!HasStateAuthority)
            return;

        // Base
        for (int i = 0; i < BaseSlots.Length && i < data.baseSlots.Count; i++)
            BaseSlots.Set(i, new NetworkInventorySlot(data.baseSlots[i].itemId, data.baseSlots[i].quantity));

        // Equip
        for (int i = 0; i < EquipSlots.Length && i < data.equipSlots.Count; i++)
            EquipSlots.Set(i, new NetworkInventorySlot(data.equipSlots[i].itemId, data.equipSlots[i].quantity));

        // Hotbar
        for (int i = 0; i < HotbarSlots.Length && i < data.hotbarSlots.Count; i++)
            HotbarSlots.Set(i, new NetworkInventorySlot(data.hotbarSlots[i].itemId, data.hotbarSlots[i].quantity));
    }
}
