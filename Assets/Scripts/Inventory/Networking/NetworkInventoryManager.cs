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
    [SerializeField] private Transform handTransform;

    [Networked] public int EquippedItemId { get; set; } = -1;
    
    private GameObject equippedObject;
    
    [Header("Events")]
    public Action OnInventoryChanged;
    public Action<int> OnEquippedItemChanged;

    private void Awake()
    {
        if (inventorySystem == null)
            Debug.LogError("[NetworkInventoryManager] Missing inventory system reference!");
        if (handTransform == null)
            Debug.LogWarning("[NetworkInventoryManager] No handTransform assigned (equip will still sync but not display).");
    }
    
    private void OnEnable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged += HandleInventoryChanged;
        
        OnEquippedItemChanged += HandleEquipChanged;
    }

    private void OnDisable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged -= HandleInventoryChanged;
        
        OnEquippedItemChanged -= HandleEquipChanged;
    }

    private void HandleInventoryChanged() => OnInventoryChanged?.Invoke();
    
    private void HandleEquipChanged(int id)
    {
        if (equippedObject != null)
            Destroy(equippedObject);

        if (id < 0) return;

        var prefab = ItemDatabase.Instance.GetEquipPrefabById(id);
        if (prefab == null) return;
        if (handTransform == null) return;

        equippedObject = Instantiate(prefab, handTransform);
        equippedObject.transform.localPosition = Vector3.zero;
        equippedObject.transform.localRotation = Quaternion.identity;
        equippedObject.transform.localScale = Vector3.one;
    }
    
    // ==================================================================
    // REQUESTS (equip + inventory)
    // ==================================================================

    public void RequestEquipItem(int itemId)
    {
        if (!Object.HasInputAuthority) return;
        RPC_RequestEquip(itemId);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestEquip(int itemId)
    {
        if (!Object.HasStateAuthority) return;

        EquippedItemId = itemId;
        RPC_NotifyEquipped(itemId);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotifyEquipped(int itemId)
    {
        OnEquippedItemChanged?.Invoke(itemId);
    }

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
        if (item != null)
            inventorySystem.TryAddItem(item, qty, slotType);
    }

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
        if (item != null)
            inventorySystem.TryRemoveQuantity(item, qty, slotType);
    }

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
    // UI ACCESS
    // ==================================================================

    public List<InventorySlot> GetSlots(SlotType type)
        => inventorySystem.GetOrderedSlots(type);

    // ==================================================================
    // SAVE / LOAD PLACEHOLDERS
    // ==================================================================

    public  PlayerInventoryData SaveInventory()
    {
        return inventorySystem.SerializeInventory(EquippedItemId);
    }

    public void LoadInventory(PlayerInventoryData data)
    {
        if (!Object.HasStateAuthority) return;
        inventorySystem.DeserializeInventory(data);
        EquippedItemId = data.equippedItemId;
        RPC_NotifyEquipped(EquippedItemId);
    }
}
