using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI controller for the player's Hotbar.
/// Displays each hotbar slot and reacts to item changes,
/// selection changes, and mouse-scroll input.
/// </summary>

public class HotbarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform hotbarContainer;
    [SerializeField] private GameObject slotPrefab;

    // Internal runtime data
    private List<InventorySlotUI> _slots = new();
    private NetworkInventoryManager _currentManager;

    // =====================================================================
    // Lifecycle
    // =====================================================================
    private void Start()
    {
        if (NetworkInventoryManager.Local != null) ConnectToLocalPlayer();  // If a local player already exists, bind immediately
        // Subscribe to global events so we can connect later
        NetworkInventoryManager.OnLocalPlayerSpawned += ConnectToLocalPlayer;
        NetworkInventoryManager.OnHotbarIndexChanged += Refresh;
    }

    private void OnDestroy()
    {
        // Clean global subscriptions
        NetworkInventoryManager.OnLocalPlayerSpawned -= ConnectToLocalPlayer;
        NetworkInventoryManager.OnHotbarIndexChanged -= Refresh;

        // Clean local manager subscriptions
        if (_currentManager != null)
        {
            var sys = _currentManager.GetComponent<NetworkInventorySystem>();
            if (sys != null)
                sys.OnInventoryChanged -= Refresh;
        }
    }

    // =====================================================================
    // Connection / Initialization
    // =====================================================================

    private void ConnectToLocalPlayer() // Called when the local player spawns. This binds the HotbarUI to the player's NetworkInventorySystem and creates the UI slots.
    {
        if (NetworkInventoryManager.Local == null)
            return;
        _currentManager = NetworkInventoryManager.Local;    // Store reference
        var sys = _currentManager.GetComponent<NetworkInventorySystem>();   // Subscribe to inventory change events
        sys.OnInventoryChanged += Refresh;
        CreateSlots(sys.GetCapacity(SlotType.Hotbar));  // Create visual hotbar slots
        Refresh();  // Initial UI update
    }

    private void CreateSlots(int count) // Creates the visual slot objects based on inventory capacity. Clears any previous UI elements.
    {
        foreach (Transform child in hotbarContainer) Destroy(child.gameObject); // Remove old slots
        _slots.Clear();

        for (int i = 0; i < count; i++)     // Instantiate slot UI objects
        {
            GameObject go = Instantiate(slotPrefab, hotbarContainer);
            var ui = go.GetComponent<InventorySlotUI>();
            
            ui.SlotIndex = i;
            ui.SlotType = SlotType.Hotbar;
            ui.Manager = _currentManager;
            
            _slots.Add(ui);
        }
    }

    // =====================================================================
    // UI Refresh
    // =====================================================================
    public void Refresh()   // Updates all hotbar slots to match the networked inventory state. Also highlights the currently selected hotbar slot.
    {
        if (_currentManager == null) return;
        var sys = _currentManager.GetComponent<NetworkInventorySystem>();
        
        for(int i = 0; i < _slots.Count; i++)
        {
            _slots[i].Refresh(sys.HotbarSlots[i]);  // Update the icon and quantity
            
            bool isSelected = (_currentManager.SelectedHotbarIndex == i);   // Highlight if selected
            _slots[i].SetHighlight(isSelected);
        }
    }
    
    // =====================================================================
    // Hotbar Input (mouse scroll wheel)
    // =====================================================================
    private void Update()
    {
        if (_currentManager == null || !Application.isFocused) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.01f)
            return;

        HandleScrollInput(scroll);
    }
    
    private void HandleScrollInput(float scroll)    // Interprets scroll direction and sends a hotbar selection request.
    {
        int current = _currentManager.SelectedHotbarIndex;
        int max = _slots.Count;

        // Circular index navigation
        current += (scroll > 0 ? -1 : 1);

        if (current < 0) current = max - 1;
        if (current >= max) current = 0;

        // Notify the network manager → server → update selection
        _currentManager.Input_SetSelectedHotbar(current);
    }
}