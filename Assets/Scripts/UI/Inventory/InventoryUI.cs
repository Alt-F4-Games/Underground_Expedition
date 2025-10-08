using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Slots Containers")]
    [SerializeField] private Transform equipContainer;    // Grid para Equipamiento
    [SerializeField] private Transform backpackContainer; // Grid para Mochila (Base)

    [Header("Slot prefab")]
    [SerializeField] private GameObject slotPrefab;
    

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

                // ✅ Asignar referencia al jugador en runtime
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    slotUI.playerTransform = player.transform;

                list.Add(slotUI);
            }
        }
    }

    
}