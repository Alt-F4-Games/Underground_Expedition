using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkInventorySystem))]
public class NetworkInventoryManager : NetworkBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Transform handTransform; // Asigna el hueso de la mano derecha aquí

    [Header("References")]
    [SerializeField] private NetworkInventorySystem inventorySystem;
    [SerializeField] private NetworkObject worldItemPrefab; // Prefab genérico para items tirados (ver siguiente script)

    // Variable sincronizada: Cuál slot de la hotbar tiene seleccionado el jugador
    [Networked] public int SelectedHotbarIndex { get; set; }
    public static NetworkInventoryManager Local { get; private set; }

    private int _currentVisualItemId = -1;
    private GameObject _currentHandModel;
    private ChangeDetector _changes;
    
    public static event System.Action OnLocalPlayerSpawned;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            Local = this;
        
            // Disparar evento para avisar a la UI que ya existimos
            OnLocalPlayerSpawned?.Invoke();
        }
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        UpdateHandVisuals(); // Actualización inicial
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Local == this) Local = null;
    }

    public override void Render()
    {
        // Detectamos si cambió el índice seleccionado o el contenido del inventario
        foreach (var change in _changes.DetectChanges(this, inventorySystem))
        {
            if (change == nameof(SelectedHotbarIndex) || 
                change == nameof(inventorySystem.HotbarSlots)) 
            {
                UpdateHandVisuals();
            }
        }
    }

    // --- VISUALS: Actualizar el modelo 3D en la mano ---
    private void UpdateHandVisuals()
    {
        if (handTransform == null) return;

        // 1. Obtener qué item hay en el slot seleccionado
        var slotData = inventorySystem.GetSlotData(SlotType.Hotbar, SelectedHotbarIndex);
        int newItemId = slotData.ItemId;

        // 2. Si es el mismo que ya tenemos, no hacer nada (optimización)
        if (newItemId == _currentVisualItemId) return;

        // 3. Destruir modelo anterior
        if (_currentHandModel != null) Destroy(_currentHandModel);

        _currentVisualItemId = newItemId;

        // 4. Instanciar nuevo modelo si hay item
        if (newItemId > 0) // 0 o -1 es vacío
        {
            GameObject prefab = ItemDatabase.Instance.GetEquipPrefab(newItemId);
            if (prefab != null)
            {
                _currentHandModel = Instantiate(prefab, handTransform);
                _currentHandModel.transform.localPosition = Vector3.zero;
                _currentHandModel.transform.localRotation = Quaternion.identity;
            }
        }
    }

    // =======================================================================
    // INPUTS (Métodos públicos llamados por tu UI local o Inputs)
    // =======================================================================

    public void Input_SetSelectedHotbar(int index)
    {
        if (!HasInputAuthority) return;
        // Validar rango
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
        // El servidor ejecuta la lógica segura del Sistema
        inventorySystem.Server_MoveItem(fromType, fromIdx, toType, toIdx);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_DropItem(SlotType type, int index)
    {
        // 1. Obtener info del item antes de borrarlo
        var slot = inventorySystem.GetSlotData(type, index);
        if (slot.ItemId <= 0) return; // Nada que tirar

        // 2. Intentar quitar del inventario
        // (Aquí asumimos tirar todo el stack, o puedes modificar para tirar 1)
        bool removed = inventorySystem.Server_TryRemoveItem(slot.ItemId, slot.Quantity, type);

        if (removed)
        {
            // 3. Spawnear el objeto en el mundo (NetworkObject)
            if (worldItemPrefab != null)
            {
                // Posición enfrente del jugador
                Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up;
                
                NetworkObject obj = Runner.Spawn(worldItemPrefab, spawnPos, Quaternion.identity);
                
                // Configurar el pickup
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
        
        // Enviamos al servidor la referencia del objeto que queremos recoger
        RPC_RequestPickup(item);
    }

    // 2. RPC que viaja al Servidor
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestPickup(NetworkWorldItem item)
    {
        // VALIDACIONES DE SEGURIDAD
        if (item == null) return; 
        
        // IMPORTANTE: Si el objeto no es valido (ya se despawneo) salimos
        if (!item.Object.IsValid) return; 

        // IMPORTANTE: Validamos distancia en el servidor para evitar trampas
        float distance = Vector3.Distance(transform.position, item.transform.position);
        if (distance > 3.0f) return; // Si está muy lejos, ignorar

        // Intentar añadir
        bool added = inventorySystem.Server_TryAddItem(item.ItemId, item.Quantity, SlotType.Base);

        if (added)
        {
            // Despawnear
            Runner.Despawn(item.Object);
            Debug.Log($"[Server] Item {item.ItemId} picked up and despawned.");
        }
    }
}