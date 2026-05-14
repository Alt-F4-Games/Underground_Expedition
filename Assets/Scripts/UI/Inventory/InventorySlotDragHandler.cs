using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(InventorySlotUI))]
public class InventorySlotDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private InventorySlotUI slotUI;
    private CanvasGroup canvasGroup;

    //------------ Global drag controls (shared by all slot handlers) ------------
    private static GameObject dragIcon;
    private static RectTransform dragIconRect;
    private static InventorySlotUI dragSourceSlot;
    private static Canvas dragCanvas;

    // ============================================================
    // Initialization
    // ============================================================

    private void Awake()
    {
        slotUI = GetComponent<InventorySlotUI>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    private CanvasGroup GetOrCreateCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }

    // ============================================================
    // Drag Start
    // ============================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsLeftClick(eventData)) return;
        if (!slotUI.HasItem) return;

        BeginDragFromSlot(slotUI);

        GetOrCreateCanvasGroup().blocksRaycasts = false;
    }

    // ============================================================
    // Drag Update
    // ============================================================

    public void OnDrag(PointerEventData eventData)
    {
        UpdateDragPosition(eventData.position);
    }

    // ============================================================
    // Drag End
    // ============================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragVisual();

        GetOrCreateCanvasGroup().blocksRaycasts = true;
    }

    // ============================================================
    // Drop Handling
    // ============================================================

    public void OnDrop(PointerEventData eventData)
    {
        if (dragSourceSlot == null || dragSourceSlot == slotUI) return;

        TryMoveItem(dragSourceSlot, slotUI);
    }

    // ============================================================
    // Shared Drag API (CLAVE PARA NO DUPLICAR)
    // ============================================================

    public static void BeginDragFromSlot(InventorySlotUI slot)
    {
        if (slot == null || !slot.HasItem) return;

        dragSourceSlot = slot;

        var handler = slot.GetComponent<InventorySlotDragHandler>();
        if (handler == null) return;

        handler.AcquireDragCanvas();
        handler.CreateDragIcon(slot.CurrentItemId);
    }

    public static void UpdateDragPosition(Vector2 position)
    {
        if (dragIconRect != null)
            dragIconRect.position = position;
    }

    public static void EndDragVisual()
    {
        if (dragIcon != null)
            Destroy(dragIcon);

        dragIcon = null;
        dragIconRect = null;
        dragSourceSlot = null;
    }

    // ============================================================
    // Item Move Logic
    // ============================================================

    private void TryMoveItem(InventorySlotUI from, InventorySlotUI to)
    {
        if (from.Manager != to.Manager || from.Manager == null) return;

        from.Manager.Input_MoveItem(from.SlotType, from.SlotIndex, to.SlotType, to.SlotIndex);
    }

    // ============================================================
    // Drag Icon Helpers
    // ============================================================

    private void AcquireDragCanvas()
    {
        if (dragCanvas == null)
            dragCanvas = GetComponentInParent<Canvas>();
    }

    private void CreateDragIcon(int itemId)
    {
        dragIcon = new GameObject("DragIcon", typeof(RectTransform), typeof(Image));
        dragIcon.transform.SetParent(dragCanvas.transform, false);
        dragIcon.transform.SetAsLastSibling();

        dragIconRect = dragIcon.GetComponent<RectTransform>();
        dragIconRect.sizeDelta = new Vector2(50, 50);

        var img = dragIcon.GetComponent<Image>();
        img.sprite = GetItemIcon(itemId);
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    private Sprite GetItemIcon(int itemId)
    {
        var itemSO = ItemDatabase.Instance.GetItemByNetworkId(itemId);
        return itemSO != null ? itemSO.icon : null;
    }

    private bool IsLeftClick(PointerEventData e)
        => e.button == PointerEventData.InputButton.Left;
}