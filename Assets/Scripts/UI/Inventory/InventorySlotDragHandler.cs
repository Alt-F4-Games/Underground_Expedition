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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!slotUI.HasItem) return;

        dragSourceSlot = slotUI;

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(transform.root);
        var img = dragIcon.AddComponent<Image>();
        img.sprite = slotUI.CurrentItem.icon;
        img.raycastTarget = false;

        var rt = dragIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(64, 64);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            Destroy(dragIcon);

        dragSourceSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (dragSourceSlot == null || dragSourceSlot == slotUI) return;

        TryMoveItem(dragSourceSlot, slotUI);
    }

    private void TryMoveItem(InventorySlotUI from, InventorySlotUI to)
    {
        var invSys = from.Manager.GetComponent<InventorySystem>();
        if (invSys == null) return;

        Debug.Log($"Mover: {from.CurrentItem?.itemName} {from.SlotType}[{from.SlotIndex}] → {to.SlotType}[{to.SlotIndex}]");

        bool ok = invSys.MoveItem(from.SlotType, from.SlotIndex, to.SlotType, to.SlotIndex);
        if (!ok)
        {
            Debug.LogWarning("Invalid movement o destination slot busy/incompatible.");
        }

        from.Manager.OnInventoryChanged?.Invoke();
    }
}