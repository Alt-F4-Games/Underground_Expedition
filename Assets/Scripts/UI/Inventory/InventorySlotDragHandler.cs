using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(InventorySlotUI))]
public class InventorySlotDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private InventorySlotUI slotUI;

    private static GameObject dragIcon;
    private static RectTransform dragIconRect;
    private static InventorySlotUI dragSourceSlot;
    private static Canvas dragCanvas;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        slotUI = GetComponent<InventorySlotUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!slotUI.HasItem) return;

        dragSourceSlot = slotUI;

        if (dragCanvas == null)
            dragCanvas = GetComponentInParent<Canvas>();

        dragIcon = new GameObject("DragIcon", typeof(RectTransform), typeof(Image));
        dragIcon.transform.SetParent(dragCanvas.transform, false);

        var img = dragIcon.GetComponent<Image>();
        img.sprite = slotUI.CurrentItem.icon;
        img.preserveAspect = true;
        img.raycastTarget = false;

        dragIconRect = dragIcon.GetComponent<RectTransform>();
        dragIconRect.sizeDelta = new Vector2(64, 64);
        dragIconRect.position = eventData.position;

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconRect != null)
            dragIconRect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SafeDestroyDragIcon();
        canvasGroup.blocksRaycasts = true;
        dragSourceSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (dragSourceSlot == null || dragSourceSlot == slotUI) return;

        TryMoveItem(dragSourceSlot, slotUI);

        SafeDestroyDragIcon();

        var srcGroup = dragSourceSlot.GetComponent<CanvasGroup>();
        if (srcGroup != null) srcGroup.blocksRaycasts = true;

        dragSourceSlot = null;
    }

    private void TryMoveItem(InventorySlotUI from, InventorySlotUI to)
    {
        var invSys = from.Manager.GetComponent<InventorySystem>();
        if (invSys == null) return;

        bool ok = invSys.MoveItem(from.SlotType, from.SlotIndex, to.SlotType, to.SlotIndex);
        if (!ok)
            Debug.LogWarning("Invalid movement or destination slot busy/incompatible.");

        from.Manager.OnInventoryChanged?.Invoke();
    }

    private static void SafeDestroyDragIcon()
    {
        if (dragIcon != null)
        {
            Object.Destroy(dragIcon);
            dragIcon = null;
            dragIconRect = null;
        }
    }
}