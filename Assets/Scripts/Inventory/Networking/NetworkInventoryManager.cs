using UnityEngine;

/// <summary>
/// InventoryManager will act as the bridge between UI and the future
/// networked inventory system. For now, it remains as a placeholder.
/// </summary>

public class NetworkInventoryManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkInventorySystem inventorySystem;

    public NetworkInventorySystem Inventory => inventorySystem;

    [Header("Events")]
    public System.Action OnInventoryChanged;

    private void Awake()
    {
        if (inventorySystem == null)
            Debug.LogError("[NetworkInventoryManager] Missing inventory system reference!");
    }
    
    public bool AddItem(ItemSO item, int qty, SlotType slotType = SlotType.Base)
    {
        if (inventorySystem == null) return false;
        return inventorySystem.TryAddItem(item, qty, slotType);
    }
    
    public bool RemoveItem(ItemSO item, int qty, SlotType slotType = SlotType.Base)
    {
        if (inventorySystem == null) return false;
        return inventorySystem.TryRemoveQuantity(item, qty, slotType);
    }

    public bool MoveItem(SlotType from, int fromIndex, SlotType to, int toIndex)
    {
        if (inventorySystem == null) return false;
        return inventorySystem.MoveItem(from, fromIndex, to, toIndex);
    }
}
