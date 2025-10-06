using System;
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

}