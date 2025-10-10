using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private MonoBehaviour playerCameraController; 

    [Header("Slots Containers")]
    [SerializeField] private Transform equipContainer;    
    [SerializeField] private Transform backpackContainer; 

    [Header("Slot prefab")]
    [SerializeField] private GameObject slotPrefab;

    [Header("UI root / Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;
    [SerializeField] private GameObject inventoryPanelRoot; 

    [Header("Options")]
    [SerializeField] private int defaultBackpackCapacity = 20;
    [SerializeField] private int defaultEquipCapacity = 3;

    private List<InventorySlotUI> _equipSlots = new List<InventorySlotUI>();
    private List<InventorySlotUI> _backpackSlots = new List<InventorySlotUI>();

    private bool _initialized = false;
    private int _lastBackpackCap = -1;
    private int _lastEquipCap = -1;

    private void Start()
    {
        EnsureInitialized();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleVisibility();
    }

    private void OnEnable()
    {
        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged.AddListener(RefreshAll);
        RefreshAll();
    }

    private void OnDisable()
    {
        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged.RemoveListener(RefreshAll);
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;
        if (inventoryManager == null || slotPrefab == null || equipContainer == null || backpackContainer == null)
        {
            Debug.LogWarning("[InventoryUI] Faltan referencias (InventoryManager / SlotPrefab / Containers).");
            return;
        }

        var invSys = inventoryManager.GetComponent<InventorySystem>();
        int equipCap = invSys != null ? invSys.GetCapacityPublic(SlotType.Equip) : defaultEquipCapacity;
        int backCap  = invSys != null ? invSys.GetCapacityPublic(SlotType.Base) : defaultBackpackCapacity;

        CreateSlotsForContainer(equipContainer, _equipSlots, equipCap, SlotType.Equip);
        CreateSlotsForContainer(backpackContainer, _backpackSlots, backCap, SlotType.Base);

        _lastBackpackCap = backCap;
        _lastEquipCap = equipCap;
        _initialized = true;
    }

    private void CreateSlotsForContainer(Transform container, List<InventorySlotUI> list, int count, SlotType slotType)
    {
        if (container == null || slotPrefab == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);

        list.Clear();

        
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, container, false);
            var slotUI = go.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.Clear();
                slotUI.SlotIndex = i;
                slotUI.SlotType = slotType;
                slotUI.Manager = inventoryManager;

                
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    slotUI.playerTransform = player.transform;

                list.Add(slotUI);
            }
        }
    }

    public void RefreshAll()
    {
        if (!_initialized) EnsureInitialized();
        if (!_initialized) return;

        var invSys = inventoryManager.GetComponent<InventorySystem>();
        List<InventorySlot> equipData = invSys != null ? invSys.GetOrderedSlots(SlotType.Equip) : new List<InventorySlot>(new InventorySlot[_lastEquipCap > 0 ? _lastEquipCap : defaultEquipCapacity]);
        List<InventorySlot> baseData  = invSys != null ? invSys.GetOrderedSlots(SlotType.Base)  : new List<InventorySlot>(new InventorySlot[_lastBackpackCap > 0 ? _lastBackpackCap : defaultBackpackCapacity]);

        int newEquipCap = equipData.Count;
        int newBackCap = baseData.Count;
        if (newEquipCap != _lastEquipCap)
        {
            CreateSlotsForContainer(equipContainer, _equipSlots, newEquipCap, SlotType.Equip);
            _lastEquipCap = newEquipCap;
        }
        if (newBackCap != _lastBackpackCap)
        {
            CreateSlotsForContainer(backpackContainer, _backpackSlots, newBackCap, SlotType.Base);
            _lastBackpackCap = newBackCap;
        }

        FillSlotsList(_equipSlots, equipData);
        FillSlotsList(_backpackSlots, baseData);
    }

    private void FillSlotsList(List<InventorySlotUI> uiSlots, List<InventorySlot> data)
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < data.Count && data[i] != null && data[i].item != null)
                uiSlots[i].Setup(data[i].item, data[i].quantity);
            else
                uiSlots[i].Clear();
        }
    }

    public void ToggleVisibility()
    {
        if (inventoryPanelRoot == null) return;
        bool next = !inventoryPanelRoot.activeSelf;
        inventoryPanelRoot.SetActive(next);

        if (next)
        {
            RefreshAll();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerCameraController != null)
                playerCameraController.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerCameraController != null)
                playerCameraController.enabled = true;
        }
    }
    
    public bool IsVisible() => inventoryPanelRoot != null && inventoryPanelRoot.activeSelf;
}