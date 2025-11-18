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

        var itemSO = ItemDatabase.Instance.GetItemById(itemId);
        if (itemSO == null) return false;

        var array = GetArrayByType(type);
        int maxStack = itemSO.maxStack;

        // 1. Intentar apilar en slots existentes
        for (int i = 0; i < array.Length; i++)
        {
            NetworkInventorySlot slot = array[i];
            
            if (slot.ItemId == itemId && slot.Quantity < maxStack)
            {
                int space = maxStack - slot.Quantity;
                int amountToAdd = Mathf.Min(space, quantity);
                
                slot.Quantity += amountToAdd;
                array.Set(i, slot); // Actualizamos el array de red

                quantity -= amountToAdd;
                if (quantity <= 0) return true;
            }
        }

        // 2. Buscar slot vacío
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].IsEmpty)
            {
                int amountToAdd = Mathf.Min(maxStack, quantity);
                
                array.Set(i, new NetworkInventorySlot(itemId, amountToAdd));
                
                quantity -= amountToAdd;
                if (quantity <= 0) return true;
            }
        }

        // Si llegamos aqui, sobraron items (inventario lleno)
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
    
    public int GetCapacity(SlotType type) => GetArrayByType(type).Length;
}