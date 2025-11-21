using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private Transform equipContainer;
    [SerializeField] private Transform backpackContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab;

    [Header("UI Root")]
    [SerializeField] private GameObject inventoryPanelRoot;
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private List<InventorySlotUI> _equipSlotsUI = new();
    private List<InventorySlotUI> _baseSlotsUI = new();

    private NetworkInventoryManager _currentManager;
    private bool _isOpen;

    // =====================================================================
    // Unity Lifecycle
    // =====================================================================

    private void Start()
    {
        if (NetworkInventoryManager.Local != null)   // Connect immediately if the player already exists
            ConnectToLocalPlayer();
        
        NetworkInventoryManager.OnLocalPlayerSpawned += ConnectToLocalPlayer;   // Subscribe so we connect when the local player spawns

        inventoryPanelRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        NetworkInventoryManager.OnLocalPlayerSpawned -= ConnectToLocalPlayer;

        if (_currentManager != null)
        {
            var sys = _currentManager.GetComponent<NetworkInventorySystem>();
            if (sys != null)
                sys.OnInventoryChanged -= RefreshAll;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleVisibility();
    }

    // =====================================================================
    // Player Linking
    // =====================================================================
    
    private void ConnectToLocalPlayer() // Connects this UI to the local player's inventory system.
    {
        _currentManager = NetworkInventoryManager.Local;
        if (_currentManager == null) return;

        var sys = _currentManager.GetComponent<NetworkInventorySystem>();
        if (sys != null)
            sys.OnInventoryChanged += RefreshAll;

        InitializeSlots();
        RefreshAll();
    }

    // =====================================================================
    // UI Visibility
    // =====================================================================
    
    public void ToggleVisibility()  // Shows or hides the inventory UI panel and configures cursor state.
    {
        _isOpen = !_isOpen;
        inventoryPanelRoot.SetActive(_isOpen);

        if (_isOpen)
        {
            RefreshAll();
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // =====================================================================
    // Slot Initialization
    // =====================================================================
    
    private void InitializeSlots()  // Creates UI slots synced with the server-side inventory capacities.
    {
        if (_currentManager == null) return;

        var sys = _currentManager.GetComponent<NetworkInventorySystem>();

        CreateSlots(equipContainer, _equipSlotsUI, sys.GetCapacity(SlotType.Equip), SlotType.Equip);
        CreateSlots(backpackContainer, _baseSlotsUI, sys.GetCapacity(SlotType.Base), SlotType.Base);
    }
    
    private void CreateSlots(Transform container, List<InventorySlotUI> uiList, int count, SlotType type)   // Creates N slots inside a container and binds them to the manager.
    {
        // Clear old children
        foreach (Transform child in container)
            Destroy(child.gameObject);

        uiList.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, container);
            var ui = go.GetComponent<InventorySlotUI>();

            ui.SlotIndex = i;
            ui.SlotType = type;
            ui.Manager = _currentManager;

            uiList.Add(ui);
        }
    }

    // =====================================================================
    // Refresh
    // =====================================================================
    
    public void RefreshAll()    // Refreshes all UI slot visuals if the panel is open.
    {
        if (!_currentManager || !_isOpen) return;

        var sys = _currentManager.GetComponent<NetworkInventorySystem>();

        RefreshList(_equipSlotsUI, sys.EquipSlots, sys.GetCapacity(SlotType.Equip));
        RefreshList(_baseSlotsUI, sys.BaseSlots, sys.GetCapacity(SlotType.Base));
    }
    
    
    // Updates a slot list with data from a Fusion NetworkArray.
    private void RefreshList(                               
        List<InventorySlotUI> uiList,
        Fusion.NetworkArray<NetworkInventorySlot> dataArray,
        int capacity)
    {
        for (int i = 0; i < uiList.Count; i++)
        {
            if (i < capacity)
                uiList[i].Refresh(dataArray[i]);
            else
                uiList[i].Clear();
        }
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    public bool IsVisible() => _isOpen;
}
