using System;
using Fusion;
using Local.Inventory;
using Network.Crafting;
using Network.Inventory;
using Network.Items;
using Network.Quests;
using Network.Quests.Enums;
using Tools.EventSystem;
using UnityEngine;
using Network;

/// <summary>
/// NetworkInventoryManager 
///
/// Main responsibilities:
///  - Expose input methods for the UI (Input_SetSelectedHotbar, Input_MoveItem, Input_DropItem).
///  - Send RPCs to the server so it can modify the inventory state (server-authoritative).
///  - Local persistence (saving/loading the player's inventory as JSON).
///
/// Notes for non-programmers:
///  - This script is the “bridge” between the UI/local player and the networked inventory.
///  - The actual inventory changes happen on the server; this script requests those changes.
/// </summary>

[RequireComponent(typeof(NetworkInventorySystem))]
public class NetworkInventoryManager : NetworkBehaviour
{
    // --------------------------- Inspector  --------------------------
    [Header("References")]
    [HideInInspector] public NetworkInventorySystem inventorySystem;

    // -------------------- Networked / Public API ----------------------
    [Networked] public int SelectedHotbarIndex { get; set; }
    public static NetworkInventoryManager Local { get; private set; }

    // -------------------- Internal state -----------------------------
    private NetworkPlayerController _playerController;

    private ChangeDetector _managerChanges;
    private ChangeDetector _invChanges;

    //-------------------------- Events ----------------------------
    public static event Action OnLocalPlayerSpawned;
    
    // -------------------------- LIFECYCLE ----------------------------------
    public override void Spawned()
    {
        SafeAssignInventorySystem();
        _playerController = GetComponent<NetworkPlayerController>();
        
        if (HasStateAuthority)
        {
            SelectedHotbarIndex = 1;
        }

        if (HasInputAuthority)
        {
            Local = this;
            OnLocalPlayerSpawned?.Invoke();
        }

        _managerChanges = null;
        Debug.Log($"{Object.Id}: Spawned at {transform.position}");
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority && Local == this)
            Local = null;
    }

    // =====================================================================
    // CORE DE INVENTARIO
    // =====================================================================
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputPlayer input) && HasStateAuthority)
        {
            if (input.Buttons.IsSet(NetworkInputPlayer.DROP_BUTTON))
            {
                Server_DropCurrentItem();
            }

            if (input.Buttons.IsSet(NetworkInputPlayer.USE_ITEM_BUTTON))
            {
                Server_UseCurrentItem();
            }
        }
    }

    private void Server_DropCurrentItem()
    {
        var slot = inventorySystem.GetSlotData(SlotType.Hotbar, SelectedHotbarIndex);
        if (slot.ItemId <= 0 || slot.Quantity <= 0) return;

        bool removed = inventorySystem.Server_TryRemoveItem(slot.ItemId, 1, SlotType.Hotbar);
        if (!removed) return;

        NetworkPrefabRef globalWorldItemPrefab = ItemDatabase.Instance.WorldItemPrefab;
        if (globalWorldItemPrefab.IsValid)
        {
            Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up;
            Runner.Spawn(globalWorldItemPrefab, spawnPos, Quaternion.identity, null, (runner, obj) => 
            {
                if (obj.TryGetComponent(out NetworkWorldItem pickupScript))
                    pickupScript.Init(slot.ItemId, 1);
            });
        }
    }

    private void Server_UseCurrentItem()
    {
        var slot = inventorySystem.GetSlotData(SlotType.Hotbar, SelectedHotbarIndex);
        if (slot.ItemId <= 0 || slot.Quantity <= 0) return;

        var itemData = ItemDatabase.Instance.GetItemByNetworkId(slot.ItemId);
        
        if (itemData != null && _playerController != null)
        {
            bool success = itemData.UseItem(_playerController);
            if (success)
            {
                inventorySystem.Server_TryRemoveItem(slot.ItemId, 1, SlotType.Hotbar);
            }
        }
    }
    // =====================================================================

    public override void Render()
    {
        if (HasInputAuthority && Local != this)
        {
            Local = this;
            OnLocalPlayerSpawned?.Invoke();
        }

        SafeAssignInventorySystem();
    }

    // -------------------- PUBLIC INPUT METHODS --------------

    public void Input_RotateHotbar(bool rotateRight)
    {
        if (!HasInputAuthority) return;
        RPC_RotateHotbar(rotateRight);
    }

    public void Input_MoveItem(SlotType fromType, int fromIdx, SlotType toType, int toIdx)
    {
        if (!HasInputAuthority) return;
        RPC_MoveItem(fromType, fromIdx, toType, toIdx);
    }

    public void Input_DropItem(SlotType type, int index)
    {
        if (!HasInputAuthority) return;
        RPC_DropItem(type, index);
    }
    
    public void Input_Craft(string resultItemId)
    {
        if (!HasInputAuthority) return;
        RPC_RequestCraft(resultItemId);
    }

    // -------------------- RPCs (Client -> Server) -------------------------

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RotateHotbar(bool rotateRight)
    {
        inventorySystem.Server_RotateHotbar(rotateRight);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_MoveItem(SlotType fromType, int fromIdx, SlotType toType, int toIdx)
    {
        inventorySystem.Server_MoveItem(fromType, fromIdx, toType, toIdx);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_DropItem(SlotType type, int index)
    {
        var slot = inventorySystem.GetSlotData(type, index);
        if (slot.ItemId <= 0) return;

        bool removed = inventorySystem.Server_TryRemoveItem(slot.ItemId, slot.Quantity, type);
        if (!removed) return;

        NetworkPrefabRef globalWorldItemPrefab = ItemDatabase.Instance.WorldItemPrefab;
        if (globalWorldItemPrefab.IsValid)
        {
            Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up;
            Runner.Spawn(globalWorldItemPrefab, spawnPos, Quaternion.identity, null, (runner, obj) => 
            {
                if (obj.TryGetComponent(out NetworkWorldItem pickupScript))
                    pickupScript.Init(slot.ItemId, slot.Quantity);
            });
        }
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestCraft(string resultItemId)
    {
        var recipe = CraftingDatabase.Instance.GetRecipeByResult(resultItemId);

        if (recipe == null) return;

        bool crafted = CraftingService.Craft(inventorySystem, recipe);

        if (crafted)
        {
            Debug.Log("Craft success");
        }
    }

    // -------------------- PICKUPS (Client requests pickup, server validates) --------------
    
    public void RequestPickupItem(NetworkWorldItem item)    
    {
        if (!HasInputAuthority) return;
        if (item == null || !item.Object.IsValid) return;
        RPC_RequestPickup(item);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestPickup(NetworkWorldItem item)
    {
        if (!HasStateAuthority) return;

        if (item == null || !item.Object.IsValid)
        {
            RPC_PickupResult(false, item != null ? item.Object : null);
            return;
        }

        var worldItem = item.GetComponent<NetworkWorldItem>();
        if (worldItem == null)
        {
            RPC_PickupResult(false, item.Object);
            return;
        }

        const float maxDist = 3f;
        if (Vector3.Distance(transform.position, worldItem.transform.position) > maxDist)
        {
            RPC_PickupResult(false, item.Object);
            return;
        }

        bool added = inventorySystem.Server_AddItemGlobal(worldItem.ItemId, worldItem.Quantity);
        if (added)
        {
            Runner.Despawn(item.Object);
            RPC_PickupResult(true, item.Object);
        }
        else
        {
            RPC_PickupResult(false, item.Object);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_PickupResult(bool success, NetworkObject itemNetObj)
    {
        if (itemNetObj == null) return;
        var worldItem = itemNetObj.GetComponent<NetworkWorldItem>();
        if (worldItem == null) return;

        if (!success)
        {
            worldItem.ResetPickupRequest();
        }
    }
    
    // -------------------- UTILITIES ---------------------------------------
    private void SafeAssignInventorySystem()    
    {
        inventorySystem = GetComponent<NetworkInventorySystem>();
    }
}