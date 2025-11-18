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

    private void Start()
    {
        // Intentar conectar si el jugador ya existe
        if (NetworkInventoryManager.Local != null)
        {
            ConnectToLocalPlayer();
        }
        
        // Suscribirse al evento estático por si el jugador aparece después
        NetworkInventoryManager.OnLocalPlayerSpawned += ConnectToLocalPlayer;
        
        inventoryPanelRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        NetworkInventoryManager.OnLocalPlayerSpawned -= ConnectToLocalPlayer;
        if (_currentManager != null)
        {
            var sys = _currentManager.GetComponent<NetworkInventorySystem>();
            if (sys != null) sys.OnInventoryChanged -= RefreshAll;
        }
    }

    private void ConnectToLocalPlayer()
    {
        _currentManager = NetworkInventoryManager.Local;
        if (_currentManager == null) return;

        var sys = _currentManager.GetComponent<NetworkInventorySystem>();
        if (sys != null)
        {
            sys.OnInventoryChanged += RefreshAll;
        }

        InitializeSlots();
        RefreshAll();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleVisibility();
        }
    }

    public void ToggleVisibility()
    {
        _isOpen = !_isOpen;
        inventoryPanelRoot.SetActive(_isOpen);

        if (_isOpen)
        {
            RefreshAll();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void InitializeSlots()
    {
        if (_currentManager == null) return;
        var sys = _currentManager.GetComponent<NetworkInventorySystem>();

        // Crear slots visuales basados en la capacidad del sistema de red
        CreateSlots(equipContainer, _equipSlotsUI, sys.GetCapacity(SlotType.Equip), SlotType.Equip);
        CreateSlots(backpackContainer, _baseSlotsUI, sys.GetCapacity(SlotType.Base), SlotType.Base);
    }

    private void CreateSlots(Transform container, List<InventorySlotUI> list, int count, SlotType type)
    {
        // Limpiar previos
        foreach (Transform child in container) Destroy(child.gameObject);
        list.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, container);
            var ui = go.GetComponent<InventorySlotUI>();
            
            ui.SlotIndex = i;
            ui.SlotType = type;
            ui.Manager = _currentManager; // Asignamos el manager local
            
            list.Add(ui);
        }
    }

    public void RefreshAll()
    {
        if (_currentManager == null || !_isOpen) return;

        var sys = _currentManager.GetComponent<NetworkInventorySystem>();

        RefreshList(_equipSlotsUI, sys.EquipSlots, sys.GetCapacity(SlotType.Equip));
        RefreshList(_baseSlotsUI, sys.BaseSlots, sys.GetCapacity(SlotType.Base));
    }
    
    // Helper para recorrer arrays de Fusion y actualizar UI
    private void RefreshList(List<InventorySlotUI> uiList, Fusion.NetworkArray<NetworkInventorySlot> dataArray, int capacity)
    {
        for (int i = 0; i < uiList.Count; i++)
        {
            if (i < capacity)
            {
                // Pasamos el struct NetworkInventorySlot
                uiList[i].Refresh(dataArray[i]);
            }
            else
            {
                uiList[i].Clear();
            }
        }
    }
    
    public bool IsVisible() => _isOpen;
}