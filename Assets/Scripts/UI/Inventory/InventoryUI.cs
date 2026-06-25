using System.Collections.Generic;
using Local.Inventory;
using UI;
using UnityEngine;

public class InventoryUI : UIWindow
{
    [Header("Containers")]
    [SerializeField] private Transform backpackContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab;

    private List<InventorySlotUI> _baseSlotsUI = new();

    private NetworkInventoryManager _currentManager;

    
    private void OnEnable()
    {
        NetworkController.OnInventoryPressed += HandleInventoryToggle;
    }

    private void OnDisable()
    {
        NetworkController.OnInventoryPressed -= HandleInventoryToggle;
    }
    
    // =====================================================================
    // Unity Lifecycle
    // =====================================================================

    private void Start()
    {
        if (NetworkInventoryManager.Local != null)   // Connect immediately if the player already exists
            ConnectToLocalPlayer();
        
        NetworkInventoryManager.OnLocalPlayerSpawned += ConnectToLocalPlayer;   // Subscribe so we connect when the local player spawns

        root.SetActive(false);
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
    
    private void HandleInventoryToggle()
    {
        if (root.activeSelf)
        {
            Close();
            return;
        }

        Open();
    }
    
    public override void Open()
    {
        if (root.activeSelf)
            return;

        base.Open();

        if (root.activeSelf)
        {
            RefreshAll();
        }
    }

    public override void Close()
    {
        if (!root.activeSelf)
            return;

        base.Close();
    }

    // =====================================================================
    // Slot Initialization
    // =====================================================================
    
    private void InitializeSlots()  // Creates UI slots synced with the server-side inventory capacities.
    {
        if (_currentManager == null) return;

        var sys = _currentManager.GetComponent<NetworkInventorySystem>();

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
    
    public void RefreshAll()
    {
        if (!_currentManager || !root.activeSelf)
            return;

        var sys = _currentManager.GetComponent<NetworkInventorySystem>();

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
}
