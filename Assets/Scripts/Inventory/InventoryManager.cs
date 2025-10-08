using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InventorySystem))]
public class InventoryManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventorySystem inventorySystem;

    [Header("Events")]
    public UnityEvent OnInventoryChanged;

    private void Awake()
    {
        if (inventorySystem == null)
            inventorySystem = GetComponent<InventorySystem>();
    }

    public bool AddItem(ItemSO item, int qty = 1, SlotType slotType = SlotType.Base)
    {
        bool added = inventorySystem.TryAddItem(item, qty, slotType);
        if (added)
        {
            OnInventoryChanged?.Invoke();
            Debug.Log($"[InventoryManager] Added {item.itemName} x{qty} to {slotType}");
        }
        else
        {
            Debug.LogWarning($"[InventoryManager] Failed to add {item.itemName} x{qty} to {slotType}");
        }
        return added;
    }

    public bool RemoveQuantity(ItemSO item, int qty = 1, SlotType slotType = SlotType.Base)
    {
        bool removed = inventorySystem.TryRemoveQuantity(item, qty, slotType);
        if (removed)
        {
            OnInventoryChanged?.Invoke();
            Debug.Log($"[InventoryManager] Removed {item.itemName} x{qty} from {slotType}");
        }
        else
        {
            Debug.LogWarning($"[InventoryManager] Failed to remove {item.itemName} x{qty} from {slotType}");
        }
        return removed;
    }

    public bool MoveItem(SlotType fromType, int fromIndex, SlotType toType, int toIndex)
    {
        bool moved = inventorySystem.MoveItem(fromType, fromIndex, toType, toIndex);
        if (moved)
        {
            OnInventoryChanged?.Invoke();
            Debug.Log($"[InventoryManager] Moved item from {fromType}[{fromIndex}] to {toType}[{toIndex}]");
        }
        else
        {
            Debug.LogWarning($"[InventoryManager] Failed to move item from {fromType}[{fromIndex}] to {toType}[{toIndex}]");
        }
        return moved;
    }

    public List<InventorySlot> GetSlots(SlotType slotType)
    {
        return inventorySystem.GetOrderedSlots(slotType);
    }
}