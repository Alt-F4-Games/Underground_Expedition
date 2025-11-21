using System;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkInventorySystem))]
public class NetworkInventoryManager : NetworkBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Transform handTransform; 

    [Header("References")]
    [HideInInspector] public NetworkInventorySystem inventorySystem;
    [SerializeField] private NetworkObject worldItemPrefab; 
    
    [Networked] public int SelectedHotbarIndex { get; set; }
    public static NetworkInventoryManager Local { get; private set; }

    private int _currentVisualItemId = -1;
    private GameObject _currentHandModel;
    private ChangeDetector _managerChanges;
    private ChangeDetector _invChanges;
    
    public static event System.Action OnLocalPlayerSpawned;
    public static event Action OnHotbarIndexChanged;

    public static string LocalPlayerId =>
        SystemInfo.deviceUniqueIdentifier;
    
    public override void Spawned()
    {
        UpdateHandVisuals();
        if (HasInputAuthority)
        {
            Local = this;
            OnLocalPlayerSpawned?.Invoke();
            string playerId = LocalPlayerId;
            var saved = InventorySaveSystem.Load(playerId);

            if (saved != null)
            {
                Debug.Log("[Client] Sending saved inventory to server...");
                var json = JsonUtility.ToJson(typeof(SavedInventoryData));
                RPC_RequestApplySavedInventory(json);
            }
            else
            {
                Debug.Log("[Client] No saved data found locally.");
            }

            var sys = GetComponent<NetworkInventorySystem>();
            sys.OnInventoryChanged += UpdateHandVisuals;

            UpdateHandVisuals();
        }
        UpdateHandVisuals();
        Debug.Log($"{Object.Id}: Spawned at {transform.position}");
        _managerChanges = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority && Local == this) Local = null;
        if (hasState)
        {
            string playerId = LocalPlayerId;
            var data = inventorySystem.ToSavedData();
            InventorySaveSystem.Save(playerId, data);
            Debug.Log($"[Inventory] Saved inventory for player {playerId}");
        }
    }

    public override void Render()
    {
        if (inventorySystem == null)
            inventorySystem = GetComponent<NetworkInventorySystem>();

        if (_managerChanges == null)
            _managerChanges = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (_invChanges == null)
            _invChanges = inventorySystem.GetChangeDetector(ChangeDetector.Source.SimulationState);
        
        // Detect changes in Manager (SelectedHotbarIndex)
        foreach (var change in _managerChanges.DetectChanges(this))
        {
            if (change == nameof(SelectedHotbarIndex))
            {
                UpdateHandVisuals();
                OnHotbarIndexChanged?.Invoke();
            }
        }

        // Detect changes in InventorySystem (HotbarSlots)
        foreach (var change in _invChanges.DetectChanges(inventorySystem))
        {
            if (change == nameof(NetworkInventorySystem.HotbarSlots))
            {
                UpdateHandVisuals();
            }
        }
    }
    
    private void UpdateHandVisuals()
    {
        var sys = GetComponent<NetworkInventorySystem>();
        if (sys == null) return;

        int index = SelectedHotbarIndex;
        if (index < 0 || index >= sys.GetCapacity(SlotType.Hotbar))
            return;

        var slot = sys.HotbarSlots[index];

        // Si no hay item → destruir modelo en mano
        if (slot.IsEmpty)
        {
            if (_currentHandModel != null) Destroy(_currentHandModel);
            _currentVisualItemId = -1;
            return;
        }

        // Si ya está equipado, no hagas nada
        if (_currentVisualItemId == slot.ItemId)
            return;

        // Instanciar nuevo modelo
        if (_currentHandModel != null) Destroy(_currentHandModel);

        GameObject prefab = ItemDatabase.Instance.GetEquipPrefab(slot.ItemId);

        if (prefab != null)
        {
            _currentHandModel = Instantiate(prefab, handTransform);
            _currentVisualItemId = slot.ItemId;
        }
    }

    // =======================================================================
    // INPUTS (Métodos públicos llamados por tu UI local o Inputs)
    // =======================================================================

    public void Input_SetSelectedHotbar(int index)
    {
        if (!HasInputAuthority) return;
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

    // =======================================================================
    // RPCs (Cliente -> Servidor)
    // =======================================================================

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

        if (removed)
        {
            if (worldItemPrefab != null)
            {
                Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up;
                NetworkObject obj = Runner.Spawn(worldItemPrefab, spawnPos, Quaternion.identity);
                
                if (obj.TryGetComponent<NetworkWorldItem>(out var pickupScript))
                {
                    pickupScript.Init(slot.ItemId, slot.Quantity);
                }
            }
        }
    }
    
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
        if (item == null || !item.Object.IsValid) {
            // Opcional: enviar NACK al cliente
            RPC_PickupResult(false, item.Object);
            return;
        }

        var worldItem = item.GetComponent<NetworkWorldItem>();
        if (worldItem == null) {
            RPC_PickupResult(false, item.Object);
            return;
        }

        // Validar distancia usando la posición del StateAuthority (servidor)
        float maxDist = 3.0f;
        if (Vector3.Distance(transform.position, worldItem.transform.position) > maxDist) {
            RPC_PickupResult(false, item.Object);
            return;
        }
        bool added = inventorySystem.Server_TryAddItem(worldItem.ItemId, worldItem.Quantity, SlotType.Base);
        if (added) {
            Runner.Despawn(item.Object);
            RPC_PickupResult(true, item.Object);
        } else {
            RPC_PickupResult(false, item.Object);
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_PickupResult(bool success, NetworkObject itemNetObj) {
        if (itemNetObj == null) return;
        var worldItem = itemNetObj.GetComponent<NetworkWorldItem>();
        if (worldItem != null) {
            if (!success) {
                worldItem.ResetPickupRequest();
                
            } else {
                // Pickup OK: el objeto habrá sido despawneado por el servidor.
                // worldItem puede llegar a existir localmente hasta que la despawn propague; no hace falta Reset.
            }
        }
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestApplySavedInventory(string json)
    {
        if (!HasStateAuthority) return;
        var data = JsonUtility.FromJson<SavedInventoryData>(json);
        inventorySystem.LoadFromSavedData(data);
    }
}