using UnityEngine;

/// <summary>
/// NetworkInventorySystem
/// Handles networked slot data (IDs + quantities).
/// </summary>

public class NetworkInventorySystem : MonoBehaviour
{
    [Header("Slot Config")]
    [SerializeField] private int baseCapacity = 3;
    [SerializeField] private int equipCapacity = 3;
    [SerializeField] private int hotbarCapacity = 3;

    /// <summary>
    /// Placeholder local-only slot struct.
    /// This will later be replaced by Fusion's networked structs.
    /// </summary>
    [System.Serializable]
    public struct LocalSlot
    {
        public int itemId;
        public int quantity;
    }

    // Placeholder arrays (non-networked)
    public LocalSlot[] baseSlots;
    public LocalSlot[] equipSlots;
    public LocalSlot[] hotbarSlots;

    private void Awake()
    {
        // initialize empty placeholder arrays
        baseSlots = new LocalSlot[baseCapacity];
        equipSlots = new LocalSlot[equipCapacity];
        hotbarSlots = new LocalSlot[hotbarCapacity];
    }
}
