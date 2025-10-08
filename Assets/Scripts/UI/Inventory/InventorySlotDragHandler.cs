using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(InventorySlotUI))]
public class InventorySlotDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private InventorySlotUI slotUI;
    private static GameObject dragIcon;
    private static InventorySlotUI dragSourceSlot;

    private void Awake()
    {
        slotUI = GetComponent<InventorySlotUI>();
    }

   
}