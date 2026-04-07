using UnityEngine;

/// <summary>
/// Handles player input for opening/closing the Inventory UI.
/// This script simply listens for a key press and requests the UI to toggle.
/// </summary>
public class InventoryInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Input Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    // =====================================================================
    // Unity Lifecycle
    // =====================================================================

    private void Awake()    // Ensure we have a valid reference before gameplay starts
    { AutoAssignInventoryUI(); }

    private void Update()
    {
        HandleToggleInput();
    }

    // =====================================================================
    // Input Handling
    // =====================================================================
    
    private void HandleToggleInput()    // Detects the toggle key and informs the UI to show/hide itself.
    {
        if (Input.GetKeyDown(toggleKey) && inventoryUI) inventoryUI.ToggleVisibility();
    }

    // =====================================================================
    // Helpers
    // =====================================================================
    
    private void AutoAssignInventoryUI()    // Assigns InventoryUI automatically if not assigned in inspector. Useful when dragging this script into a new scene.
    {
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>();
    }

    private void Reset()    // Unity calls this only in Editor when clicking "Reset".
    { AutoAssignInventoryUI(); }
}