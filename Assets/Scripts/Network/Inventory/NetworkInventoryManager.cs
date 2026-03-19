using System;
using Fusion;
using UnityEngine;

/// <summary>
/// NetworkInventoryManager 
///
/// Main responsibilities:
///  - Expose input methods for the UI (Input_SetSelectedHotbar, Input_MoveItem, Input_DropItem).
///  - Send RPCs to the server so it can modify the inventory state (server-authoritative).
///  - Handle the visual item model shown in the player's hand (equip visuals).
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
    [Header("Visuals")]
    [SerializeField] private Transform handTransform;

    [Header("References")]
    [HideInInspector] public NetworkInventorySystem inventorySystem;
    [SerializeField] private NetworkObject worldItemPrefab;

    // -------------------- Networked / Public API ----------------------
    [Networked] public int SelectedHotbarIndex { get; set; }
    public static NetworkInventoryManager Local { get; private set; }

    // -------------------- Internal state -----------------------------
    private int _currentVisualItemId = -1;
    private GameObject _currentHandModel;

    private ChangeDetector _managerChanges;
    private ChangeDetector _invChanges;

    //-------------------------- Events ----------------------------
    public static event Action OnLocalPlayerSpawned;
    public static event Action OnHotbarIndexChanged;

    //---------- Player identifier used for local persistence ----------
    public static string LocalPlayerId => SystemInfo.deviceUniqueIdentifier;

    // -------------------------- LIFECYCLE ----------------------------------
    public override void Spawned()
    {
        // Minimal work here: set local if we are input-authority and kick off loading.
        // Heavy lifting is delegated to helpers and Render where necessary.
        SafeAssignInventorySystem();
        UpdateHandVisuals();
        if (HasInputAuthority)
        {
            Local = this;
            OnLocalPlayerSpawned?.Invoke();

            // Client loads its local JSON and sends it to the server (via RPC string).
            LoadLocalAndSyncToServer();

            // Subscribe visuals update to inventory changes
            inventorySystem.OnInventoryChanged += UpdateHandVisuals;
            OnHotbarIndexChanged?.Invoke();
            UpdateHandVisuals();
        }

        // Prepare change detector lazily in Render.
        _managerChanges = null;

        Debug.Log($"{Object.Id}: Spawned at {transform.position}");
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // Save local inventory for the client that owns this object.
        if (HasInputAuthority)
            SaveLocalInventory();

        if (HasInputAuthority && Local == this)
            Local = null;
    }

    public override void Render()
    {
        // Ensure Local is set even if Spawned timing had race conditions.
        if (HasInputAuthority && Local != this)
        {
            Local = this;
            OnLocalPlayerSpawned?.Invoke();
        }

        SafeAssignInventorySystem();

        // Init change detectors lazily
        if (_managerChanges == null)
            _managerChanges = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (_invChanges == null && inventorySystem != null)
            _invChanges = inventorySystem.GetChangeDetector(ChangeDetector.Source.SimulationState);

        // Detect changes to this manager (SelectedHotbarIndex)
        foreach (var change in _managerChanges.DetectChanges(this))
        {
            if (change == nameof(SelectedHotbarIndex))
            {
                UpdateHandVisuals();
                OnHotbarIndexChanged?.Invoke();
            }
        }

        // Detect changes in the inventory system (HotbarSlots)
        if (_invChanges != null)
        {
            foreach (var change in _invChanges.DetectChanges(inventorySystem))
            {
                if (change == nameof(NetworkInventorySystem.HotbarSlots))
                {
                    UpdateHandVisuals();
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        // Ensure save on quit for local client
        if (HasInputAuthority)
            SaveLocalInventory();
    }

    // ----------------------- VISUALS / HAND MODEL ---------------------------
    private void UpdateHandVisuals()    // Update model shown in player's hand according to selected hotbar slot. Keep logic compact and avoid duplicate GetComponent calls.
    {
        SafeAssignInventorySystem();
        if (inventorySystem == null) return;

        int idx = SelectedHotbarIndex;
        if (idx < 0 || idx >= inventorySystem.GetCapacity(SlotType.Hotbar))
            return;

        var slot = inventorySystem.HotbarSlots[idx];

        if (slot.IsEmpty)
        {
            ClearHandModel();
            _currentVisualItemId = -1;
            return;
        }

        // If same item already shown, no-op
        if (_currentVisualItemId == slot.ItemId) return;

        ClearHandModel();

        var prefab = ItemDatabase.Instance.GetEquipPrefab(slot.ItemId);
        if (prefab != null)
        {
            _currentHandModel = Instantiate(prefab, handTransform);
            _currentVisualItemId = slot.ItemId;
        }
    }

    private void ClearHandModel()
    {
        if (_currentHandModel != null)
        {
            Destroy(_currentHandModel);
            _currentHandModel = null;
        }
    }

    // -------------------- PUBLIC INPUT METHODS (UI -> Manager) --------------

    public void Input_SetSelectedHotbar(int index)
    {
        if (!HasInputAuthority) return;
        SafeAssignInventorySystem();
        if (index < 0 || index >= inventorySystem.GetCapacity(SlotType.Hotbar)) return;

        RPC_SetSelectedHotbar(index);
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

    // -------------------- RPCs (Client -> Server) -------------------------

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetSelectedHotbar(int index)
    {
        SelectedHotbarIndex = index;
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

        if (worldItemPrefab)
        {
            Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up;
            var obj = Runner.Spawn(worldItemPrefab, spawnPos, Quaternion.identity);
            if (obj.TryGetComponent(out NetworkWorldItem pickupScript))
                pickupScript.Init(slot.ItemId, slot.Quantity);
        }
    }

    // -------------------- PICKUPS (Client requests pickup, server validates) --------------
    
    public void RequestPickupItem(NetworkWorldItem item)    // Called by NetworkWorldItem local when player attempts pickup. Sends the NetworkObject reference to server for validation.
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
            // Safety: inform client (optional)
            RPC_PickupResult(false, item != null ? item.Object : null);
            return;
        }

        var worldItem = item.GetComponent<NetworkWorldItem>();
        if (worldItem == null)
        {
            RPC_PickupResult(false, item.Object);
            return;
        }

        // Server-side distance check
        const float maxDist = 3f;
        if (Vector3.Distance(transform.position, worldItem.transform.position) > maxDist)
        {
            RPC_PickupResult(false, item.Object);
            return;
        }

        bool added = inventorySystem.Server_TryAddItem(worldItem.ItemId, worldItem.Quantity, SlotType.Base);
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
        // Runs on client that requested pickup
        if (itemNetObj == null) return;
        var worldItem = itemNetObj.GetComponent<NetworkWorldItem>();
        if (worldItem == null) return;

        if (!success)
        {
            // Reset local debounce to allow retry
            worldItem.ResetPickupRequest();
        }
        // else: success — server has despawned object
    }

    // -------------------- SAVED INVENTORY SYNC (client -> server) -----------
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SendSavedInventoryJson(string json)    // Client reads local JSON, serializes to string and sends to server via RPC.
    {
        if (!HasStateAuthority) return;

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[Inventory] Received empty JSON from client");
            return;
        }

        try
        {
            var saved = JsonUtility.FromJson<SavedInventoryData>(json);
            if (saved != null)
            {
                inventorySystem.LoadFromSavedData(saved);
                Debug.Log("[Inventory] Server applied saved inventory from client.");
            }
            else
            {
                Debug.LogWarning("[Inventory] JsonUtility.FromJson returned null");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Inventory] Failed to parse saved JSON: {ex.Message}");
        }
    }

    // -------------------- LOCAL PERSISTENCE HELPERS -------------------------
    private void LoadLocalAndSyncToServer()     // Load local saved inventory (client-only) and send it to server as JSON.
    {
        if (!HasInputAuthority) return;

        string id = LocalPlayerId;
        var saved = InventorySaveSystem.Load(id);
        if (saved == null)
        {
            Debug.Log($"[Inventory] No local save for {id}");
            return;
        }

        string json = JsonUtility.ToJson(saved);
        Debug.Log($"[Inventory] Sending saved JSON to server for {id} (len {json.Length})");
        RPC_SendSavedInventoryJson(json);
    }

    public void SaveLocalInventory()
    {
        string playerId = LocalPlayerId;
        var data = inventorySystem.ToSavedData();
        InventorySaveSystem.Save(playerId, data);
        Debug.Log($"[Inventory] Saved inventory for player {playerId}");
    }

    // -------------------- UTILITIES ---------------------------------------
    private void SafeAssignInventorySystem()    // Safe getter for inventorySystem to avoid repeated GetComponent calls.
    {
        if (inventorySystem == null)
            inventorySystem = GetComponent<NetworkInventorySystem>();
    }
}