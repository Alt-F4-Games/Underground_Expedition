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
    
    // ==================================================================
    // ADD
    // ==================================================================

    public void RequestAddItem(int itemId, int qty, SlotType slotType)
    {
        if (!Object.HasInputAuthority) return;
        RPC_RequestAddItem(itemId, qty, slotType);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestAddItem(int itemId, int qty, SlotType slotType)
    {
        if (!Object.HasStateAuthority) return;

        var item = ItemDatabase.Instance.GetItemById(itemId);
        if (item == null) return;

        inventorySystem.TryAddItem(item, qty, slotType);
    }

    // ==================================================================
    // REMOVE
    // ==================================================================

    public void RequestRemoveItem(int itemId, int qty, SlotType slotType)
    {
        if (!Object.HasInputAuthority) return;
        RPC_RequestRemoveItem(itemId, qty, slotType);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestRemoveItem(int itemId, int qty, SlotType slotType)
    {
        if (!Object.HasStateAuthority) return;

        var item = ItemDatabase.Instance.GetItemById(itemId);
        if (item == null) return;

        inventorySystem.TryRemoveQuantity(item, qty, slotType);
    }

    // ==================================================================
    // MOVE
    // ==================================================================

    public void RequestMoveItem(SlotType from, int fromIndex, SlotType to, int toIndex)
    {
        if (!Object.HasInputAuthority) return;
        RPC_RequestMoveItem(from, fromIndex, to, toIndex);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestMoveItem(SlotType from, int fromIndex, SlotType to, int toIndex)
    {
        if (!Object.HasStateAuthority) return;

        inventorySystem.MoveItem(from, fromIndex, to, toIndex);
    }

    // ==================================================================
    // UI
    // ==================================================================
    
    public List<InventorySlot> GetSlots(SlotType type) =>
        inventorySystem != null ? inventorySystem.GetOrderedSlots(type) : null;
}
