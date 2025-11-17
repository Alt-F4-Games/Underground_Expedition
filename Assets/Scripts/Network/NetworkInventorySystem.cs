using UnityEngine;

public class NetworkInventorySystem : MonoBehaviour
{
    [Header("Slot Config")]
    [SerializeField] private int baseCapacity = 3;
    [SerializeField] private int equipCapacity = 3;
    [SerializeField] private int hotbarCapacity = 3;

    [System.Serializable]
    public struct LocalSlot
    {
        public int itemId;
        public int quantity;
    }

    public LocalSlot[] baseSlots;
    public LocalSlot[] equipSlots;
    public LocalSlot[] hotbarSlots;

    private void Awake()
    {
        baseSlots = new LocalSlot[baseCapacity];
        equipSlots = new LocalSlot[equipCapacity];
        hotbarSlots = new LocalSlot[hotbarCapacity];

        Debug.Log("[NetworkInventorySystem] Initialized placeholder slot arrays.");
    }
    
    public LocalSlot GetBaseSlot(int i) => baseSlots[i];
    public LocalSlot GetEquipSlot(int i) => equipSlots[i];
    public LocalSlot GetHotbarSlot(int i) => hotbarSlots[i];
    
    // --- PLACEHOLDER METHODS (will be replaced with RPC logic later) ---
    public bool TryAddItemPlaceholder(int itemId, int qty)
    {
        Debug.Log($"[NetworkInventorySystem] Placeholder Add itemId={itemId} qty={qty}");
        return true;
    }

    public bool TryRemoveItemPlaceholder(int itemId, int qty)
    {
        Debug.Log($"[NetworkInventorySystem] Placeholder Remove itemId={itemId} qty={qty}");
        return true;
    }

    public bool TryMoveItemPlaceholder()
    {
        Debug.Log($"[NetworkInventorySystem] Placeholder Move");
        return true;
    }
}
