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

}