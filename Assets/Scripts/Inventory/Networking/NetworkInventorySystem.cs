using Fusion;
using UnityEngine;
using System;

public class NetworkInventorySystem : NetworkBehaviour
{
    // Definimos capacidades fijas para Fusion (NetworkArray requiere tamaño fijo)
    private const int MAX_BASE = 3;
    private const int MAX_EQUIP = 3;
    private const int MAX_HOTBAR = 3;

    // --- NETWORKED DATA ---
    // OnChanged se dispara cuando el servidor modifica estos datos
    [Networked, Capacity(MAX_BASE)] 
    public NetworkArray<NetworkInventorySlot> BaseSlots { get; }
    
    [Networked, Capacity(MAX_EQUIP)] 
    public NetworkArray<NetworkInventorySlot> EquipSlots { get; }
    
    [Networked, Capacity(MAX_HOTBAR)] 
    public NetworkArray<NetworkInventorySlot> HotbarSlots { get; }

    // --- EVENTS (Local) ---
    // La UI se suscribirá a esto
    public event Action OnInventoryChanged;

    public override void Spawned()
    {
        // Forzamos una actualización inicial
        OnInventoryChanged?.Invoke();
    }
    
    // Este método es llamado automáticamente por Fusion cuando cambian los datos
    // IMPORTANTE: El nombre debe coincidir con "Networked" properties si usas OnChanged personalizado,
    // pero Fusion 2 detecta cambios en NetworkArray automáticamente si usamos ChangeDetector.
    // Para simplificar en Fusion 2, usaremos Render().
    
    private ChangeDetector _changes;

    public override void Render()
    {
        // Detectamos cambios en los arrays para avisar a la UI
        if (_changes == null) _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        bool changed = false;
        
        // Revisamos cada array
        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(BaseSlots) || 
                change == nameof(EquipSlots) || 
                change == nameof(HotbarSlots))
            {
                changed = true;
            }
        }

        if (changed)
        {
            OnInventoryChanged?.Invoke();
        }
    }

    // ================================================================================================
    // LOGICA DE SERVIDOR (SOLO StateAuthority DEBE LLAMAR ESTO)
    // ================================================================================================

    public bool Server_TryAddItem(int itemId, int quantity, SlotType type)
    {
        if (!HasStateAuthority) return false;
        if (quantity <= 0) return false;
        
        // 1. Intentar apilar en slots existentes
        var slots = (type == SlotType.Base ? BaseSlots : (type == SlotType.Hotbar ? HotbarSlots : EquipSlots));
        // 1) intentamos stackear en slot existente
        for (int i = 0; i < GetCapacity(type); i++) {
            var s = slots[i];
            if (!s.IsEmpty && s.ItemId == itemId) {
                // actualizar quantity de forma directa en NetworkArray
                s.Quantity += quantity;
                slots[i] = s; // escritura
                return true;
            }
        }
        // 2) buscar slot vacío
        for (int i = 0; i < GetCapacity(type); i++) {
            var s = slots[i];
            if (s.IsEmpty) {
                slots[i] = new NetworkInventorySlot(itemId, quantity);
                return true;
            }
        }
        return false;
    }

    public bool Server_TryRemoveItem(int itemId, int quantity, SlotType type)
    {
        if (!HasStateAuthority) return false;
        
        var array = GetArrayByType(type);
        int remainingToRemove = quantity;

        // Iteramos al revés para quitar primero de los últimos slots (comportamiento común)
        // o normal. Haremos normal.
        for (int i = 0; i < array.Length; i++)
        {
            NetworkInventorySlot slot = array[i];
            
            if (slot.ItemId == itemId)
            {
                if (slot.Quantity >= remainingToRemove)
                {
                    slot.Quantity -= remainingToRemove;
                    if (slot.Quantity <= 0) slot = new NetworkInventorySlot(0, 0); // Vaciar
                    
                    array.Set(i, slot);
                    return true;
                }
                else
                {
                    remainingToRemove -= slot.Quantity;
                    array.Set(i, new NetworkInventorySlot(0, 0)); // Vaciar slot y seguir buscando
                }
            }
        }
        return false; // No tenía suficiente cantidad
    }

    public void Server_MoveItem(SlotType fromType, int fromIdx, SlotType toType, int toIdx)
    {
        if (!HasStateAuthority) return;

        var fromArray = GetArrayByType(fromType);
        var toArray = GetArrayByType(toType);

        // Validaciones de índice
        if (fromIdx < 0 || fromIdx >= fromArray.Length) return;
        if (toIdx < 0 || toIdx >= toArray.Length) return;

        NetworkInventorySlot fromSlot = fromArray[fromIdx];
        NetworkInventorySlot toSlot = toArray[toIdx];

        if (fromSlot.IsEmpty) return;

        // TODO: Validar tipos de item (Ej: No poner un arma en slot de casco) usando ItemDatabase
        
        // Lógica simple de intercambio (Swap)
        // Nota: Aquí podrias agregar lógica de stackear si son el mismo item
        
        if (fromType == toType && fromIdx == toIdx) return; // Mismo slot

        // Si son el mismo item, intentamos combinar
        if (toSlot.ItemId == fromSlot.ItemId)
        {
            var itemData = ItemDatabase.Instance.GetItemById(fromSlot.ItemId);
            int max = itemData != null ? itemData.maxStack : 99;

            int space = max - toSlot.Quantity;
            if (space > 0)
            {
                int moveAmount = Mathf.Min(space, fromSlot.Quantity);
                
                toSlot.Quantity += moveAmount;
                fromSlot.Quantity -= moveAmount;
                
                if (fromSlot.Quantity <= 0) fromSlot = new NetworkInventorySlot(0, 0);

                toArray.Set(toIdx, toSlot);
                fromArray.Set(fromIdx, fromSlot);
                return;
            }
        }

        // Intercambio directo
        fromArray.Set(fromIdx, toSlot);
        toArray.Set(toIdx, fromSlot);
    }

    // Helper para obtener el array correcto según el Enum
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
    
    // Helper público para la UI (lectura solamente)
    public NetworkInventorySlot GetSlotData(SlotType type, int index)
    {
        var arr = GetArrayByType(type);
        if (index >= 0 && index < arr.Length)
            return arr[index];
        return new NetworkInventorySlot(0,0);
    }
    
    /// <summary>
    /// ////////////////////////////////////////////////////////////////////
    ///
    ///  SAVE / LOAD LOGIC
    ///
    /// ////////////////////////////////////////////////////////////////////
    /// </summary>
   
    public SavedInventoryData ToSavedData()
    {
        var data = new SavedInventoryData();

        // Base slots
        for (int i = 0; i < BaseSlots.Length; i++)
        {
            var slot = BaseSlots[i];
            data.baseSlots.Add(new SavedSlot(slot.ItemId, slot.Quantity));
        }

        // Equip slots
        for (int i = 0; i < EquipSlots.Length; i++)
        {
            var slot = EquipSlots[i];
            data.equipSlots.Add(new SavedSlot(slot.ItemId, slot.Quantity));
        }

        // Hotbar slots
        for (int i = 0; i < HotbarSlots.Length; i++)
        {
            var slot = HotbarSlots[i];
            data.hotbarSlots.Add(new SavedSlot(slot.ItemId, slot.Quantity));
        }

        return data;
    }

    public void LoadFromSavedData(SavedInventoryData data)
    {
        if (!HasStateAuthority) return;

        // BASE
        for (int i = 0; i < BaseSlots.Length && i < data.baseSlots.Count; i++)
        {
            BaseSlots.Set(i, new NetworkInventorySlot(
                data.baseSlots[i].itemId,
                data.baseSlots[i].quantity));
        }

        // EQUIP
        for (int i = 0; i < EquipSlots.Length && i < data.equipSlots.Count; i++)
        {
            EquipSlots.Set(i, new NetworkInventorySlot(
                data.equipSlots[i].itemId,
                data.equipSlots[i].quantity));
        }

        // HOTBAR
        for (int i = 0; i < HotbarSlots.Length && i < data.hotbarSlots.Count; i++)
        {
            HotbarSlots.Set(i, new NetworkInventorySlot(
                data.hotbarSlots[i].itemId,
                data.hotbarSlots[i].quantity));
        }
    }
    public int GetCapacity(SlotType type) => GetArrayByType(type).Length;
}