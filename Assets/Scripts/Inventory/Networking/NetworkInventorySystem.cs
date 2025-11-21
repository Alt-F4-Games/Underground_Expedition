using Fusion;
using UnityEngine;
using System;

/// <summary>
/// SISTEMA DE INVENTARIO EN RED (Fusion)
/// --------------------------------------
/// Este script guarda y sincroniza las 3 categorías de inventario:
/// - BaseSlots   → ítems del inventario general
/// - EquipSlots  → ítems equipados (armaduras, herramientas)
/// - HotbarSlots → barra rápida (slots visibles)
///
/// NOTA PARA NO PROGRAMADORES:
/// Este script se encarga de que todos vean el inventario igual
/// cuando se juega online. Aquí NO hay gráficos ni UI,
/// solo datos sincronizados.
///
/// NOTA PARA PROGRAMADORES:
/// La lógica del servidor es la única que modifica los arrays
/// (Server_* methods). Los clientes solo los leen gracias a Fusion.
/// </summary>

public class NetworkInventorySystem : NetworkBehaviour
{
    // Fusion requiere tamaños fijos para NetworkArray
    private const int MAX_BASE = 3;
    private const int MAX_EQUIP = 3;
    private const int MAX_HOTBAR = 3;

    // ----------------------------
    //        NETWORKED DATA
    // ----------------------------

    [Networked, Capacity(MAX_BASE)]
    public NetworkArray<NetworkInventorySlot> BaseSlots { get; }

    [Networked, Capacity(MAX_EQUIP)]
    public NetworkArray<NetworkInventorySlot> EquipSlots { get; }

    [Networked, Capacity(MAX_HOTBAR)]
    public NetworkArray<NetworkInventorySlot> HotbarSlots { get; }

    // ----------------------------
    //         LOCAL EVENTS
    // ----------------------------
    /// <summary>
    /// La UI se suscribe a este evento para refrescarse
    /// cuando cambian los datos del inventario.
    /// </summary>
    public event Action OnInventoryChanged;

    private ChangeDetector _changes;

    public override void Spawned()
    {
        // Llamamos a un refresh inicial para que la UI cargue los datos.
        OnInventoryChanged?.Invoke();
    }

    // ------------------------------------------------------------
    //  Fusion llama Render() cada frame en el cliente.
    //  Aquí detectamos cambios reales en los arrays Networked.
    // ------------------------------------------------------------
    public override void Render()
    {
        if (_changes == null)
            _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        foreach (var change in _changes.DetectChanges(this))
        {
            if (change == nameof(BaseSlots) ||
                change == nameof(EquipSlots) ||
                change == nameof(HotbarSlots))
            {
                OnInventoryChanged?.Invoke();
                return; // evitamos avisar múltiples veces
            }
        }
    }

    // =====================================================================
    //                         LÓGICA DE SERVIDOR
    // =====================================================================

    /// <summary>
    /// Agrega un ítem a un inventario respetando stacks.
    /// Solo se puede llamar desde el dueño del estado (Host/Server).
    /// </summary>
    public bool Server_TryAddItem(int itemId, int quantity, SlotType type)
    {
        if (!HasStateAuthority || quantity <= 0)
            return false;

        var slots = GetArrayByType(type);

        // 1) Intentar apilar en slots compatibles
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (!s.IsEmpty && s.ItemId == itemId)
            {
                s.Quantity += quantity;
                slots.Set(i, s);
                return true;
            }
        }

        // 2) Buscar slot vacío
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots.Set(i, new NetworkInventorySlot(itemId, quantity));
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Remueve ítems de los slots. Si no se encuentra la cantidad suficiente → false.
    /// </summary>
    public bool Server_TryRemoveItem(int itemId, int quantity, SlotType type)
    {
        if (!HasStateAuthority)
            return false;

        var array = GetArrayByType(type);
        int remaining = quantity;

        for (int i = 0; i < array.Length; i++)
        {
            var slot = array[i];

            if (slot.ItemId != itemId)
                continue;

            if (slot.Quantity >= remaining)
            {
                slot.Quantity -= remaining;
                if (slot.Quantity <= 0)
                    slot = new NetworkInventorySlot(0, 0);

                array.Set(i, slot);
                return true;
            }

            // Vaciar slot y restar lo excedente
            remaining -= slot.Quantity;
            array.Set(i, new NetworkInventorySlot(0, 0));
        }

        return false;
    }

    /// <summary>
    /// Mueve o combina ítems entre slots cualquiera.
    /// </summary>
    public void Server_MoveItem(SlotType fromType, int fromIdx, SlotType toType, int toIdx)
    {
        if (!HasStateAuthority)
            return;

        var fromArray = GetArrayByType(fromType);
        var toArray = GetArrayByType(toType);

        if (!IndexValid(fromArray, fromIdx) || !IndexValid(toArray, toIdx))
            return;

        var from = fromArray[fromIdx];
        var to = toArray[toIdx];

        if (from.IsEmpty)
            return;

        // Si ambos son el mismo item → combinar
        if (to.ItemId == from.ItemId)
        {
            var itemData = ItemDatabase.Instance.GetItemById(from.ItemId);
            int maxStack = itemData?.maxStack ?? 99;

            int space = maxStack - to.Quantity;
            if (space > 0)
            {
                int moved = Mathf.Min(space, from.Quantity);

                to.Quantity += moved;
                from.Quantity -= moved;

                if (from.Quantity <= 0)
                    from = new NetworkInventorySlot(0, 0);

                toArray.Set(toIdx, to);
                fromArray.Set(fromIdx, from);
                return;
            }
        }

        // Swap simple
        fromArray.Set(fromIdx, to);
        toArray.Set(toIdx, from);
    }

    // =====================================================================
    //                           HELPERS
    // =====================================================================

    private bool IndexValid(NetworkArray<NetworkInventorySlot> arr, int index)
        => index >= 0 && index < arr.Length;

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

    public NetworkInventorySlot GetSlotData(SlotType type, int index)
    {
        var arr = GetArrayByType(type);
        return IndexValid(arr, index) ? arr[index] : new NetworkInventorySlot(0, 0);
    }

    public int GetCapacity(SlotType type) => GetArrayByType(type).Length;

    // =====================================================================
    //                           SAVE / LOAD
    // =====================================================================

    public SavedInventoryData ToSavedData()
    {
        var data = new SavedInventoryData();

        // Copiar BaseSlots
        for (int i = 0; i < BaseSlots.Length; i++)
            data.baseSlots.Add(new SavedSlot(BaseSlots[i].ItemId, BaseSlots[i].Quantity));

        // Copiar EquipSlots
        for (int i = 0; i < EquipSlots.Length; i++)
            data.equipSlots.Add(new SavedSlot(EquipSlots[i].ItemId, EquipSlots[i].Quantity));

        // Copiar HotbarSlots
        for (int i = 0; i < HotbarSlots.Length; i++)
            data.hotbarSlots.Add(new SavedSlot(HotbarSlots[i].ItemId, HotbarSlots[i].Quantity));

        return data;
    }

    public void LoadFromSavedData(SavedInventoryData data)
    {
        if (!HasStateAuthority)
            return;

        // Base
        for (int i = 0; i < BaseSlots.Length && i < data.baseSlots.Count; i++)
            BaseSlots.Set(i, new NetworkInventorySlot(data.baseSlots[i].itemId, data.baseSlots[i].quantity));

        // Equip
        for (int i = 0; i < EquipSlots.Length && i < data.equipSlots.Count; i++)
            EquipSlots.Set(i, new NetworkInventorySlot(data.equipSlots[i].itemId, data.equipSlots[i].quantity));

        // Hotbar
        for (int i = 0; i < HotbarSlots.Length && i < data.hotbarSlots.Count; i++)
            HotbarSlots.Set(i, new NetworkInventorySlot(data.hotbarSlots[i].itemId, data.hotbarSlots[i].quantity));
    }
}
