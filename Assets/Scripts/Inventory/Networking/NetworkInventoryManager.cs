using Fusion;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// InventoryManager will act as the bridge between UI and the future
/// networked inventory system. For now, it remains as a placeholder.
/// </summary>

public class NetworkInventoryManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkInventorySystem inventorySystem;

    public NetworkInventorySystem Inventory => inventorySystem;

    [Header("Events")]
    public Action OnInventoryChanged;

    private void Awake()
    {
        if (inventorySystem == null)
            Debug.LogError("[NetworkInventoryManager] Missing inventory system reference!");
    }
    
    private void OnEnable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged += HandleInventoryChanged;
    }

    private void OnDisable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged -= HandleInventoryChanged;
    }

    private void HandleInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }
    
    public List<InventorySlot> GetSlots(SlotType type) =>
        inventorySystem != null ? inventorySystem.GetOrderedSlots(type) : null;
}
