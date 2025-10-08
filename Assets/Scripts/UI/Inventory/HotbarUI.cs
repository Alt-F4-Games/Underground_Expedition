using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform hotbarContainer;
    [SerializeField] private GameObject slotPrefab;

    private List<InventorySlotUI> hotbarSlots = new List<InventorySlotUI>();
    private int lastCapacity = -1;

    private void Start()
    {
        CreateSlots();
        Refresh();
    }

    private void OnEnable()
    {
        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged.AddListener(Refresh);
    }

    private void OnDisable()
    {
        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged.RemoveListener(Refresh);
    }

    private void CreateSlots()
    {
        if (inventoryManager == null || slotPrefab == null || hotbarContainer == null) return;

        var invSys = inventoryManager.GetComponent<InventorySystem>();
        int capacity = invSys != null ? invSys.GetCapacityPublic(SlotType.Hotbar) : 3;

        for (int i = hotbarContainer.childCount - 1; i >= 0; i--)
            Destroy(hotbarContainer.GetChild(i).gameObject);
        hotbarSlots.Clear();

        for (int i = 0; i < capacity; i++)
        {
            GameObject go = Instantiate(slotPrefab, hotbarContainer, false);
            var slotUI = go.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.Clear();
                slotUI.SlotIndex = i;
                slotUI.SlotType = SlotType.Hotbar;
                slotUI.Manager = inventoryManager;
                hotbarSlots.Add(slotUI);
            }
        }

        lastCapacity = capacity;
        LayoutRebuilder.ForceRebuildLayoutImmediate(hotbarContainer as RectTransform);
    }

}