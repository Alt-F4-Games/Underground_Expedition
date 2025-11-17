using UnityEngine;

/// <summary>
/// InventoryManager will act as the bridge between UI and the future
/// networked inventory system. For now, it remains as a placeholder.
/// </summary>

public class NetworkInventoryManager : MonoBehaviour
{
    [Header("Future References")]
    [SerializeField] private MonoBehaviour futureInventorySystem; // to be replaced

    [Header("Events")]
    public System.Action OnInventoryChanged;

    // --- Public API to be implemented later ---
    public void RequestAddPlaceholder()
    {
        Debug.Log("[InventoryManager] Add request placeholder");
    }

    public void RequestRemovePlaceholder()
    {
        Debug.Log("[InventoryManager] Remove request placeholder");
    }

    public void RequestMovePlaceholder()
    {
        Debug.Log("[InventoryManager] Move request placeholder");
    }
}
