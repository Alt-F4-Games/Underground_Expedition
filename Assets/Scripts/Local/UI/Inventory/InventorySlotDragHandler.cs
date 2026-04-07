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

    private CanvasGroup GetOrCreateCanvasGroup()    // Ensures this GameObject always has a CanvasGroup. Prevents MissingComponentException during drag end.
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

        dragSourceSlot = slotUI;

        AcquireDragCanvas();
        CreateDragIcon(slotUI.CurrentItemId);
        // Disable raycasts so the drop target receives the event
        GetOrCreateCanvasGroup().blocksRaycasts = false;
    }

    // ============================================================
    // Drag Update
    // ============================================================

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconRect != null)
            dragIconRect.position = eventData.position;
    }

    // ============================================================
    // Drag End
    // ============================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        CleanupDragVisuals();
        
        // Re-enable raycasts safely
        GetOrCreateCanvasGroup().blocksRaycasts = true;
        
        dragSourceSlot = null;
    }

    // ============================================================
    // Drop Handling
    // ============================================================

    public void OnDrop(PointerEventData eventData)
    {
        // No source OR dropped onto itself → ignore
        if (dragSourceSlot == null || dragSourceSlot == slotUI) return;

        TryMoveItem(dragSourceSlot, slotUI);
    }

    // ============================================================
    // Item Move Logic
    // ============================================================

    private void TryMoveItem(InventorySlotUI from, InventorySlotUI to)
    {
        // Must belong to the same local Manager
        if (from.Manager != to.Manager || from.Manager == null) return;

        from.Manager.Input_MoveItem(from.SlotType, from.SlotIndex, to.SlotType, to.SlotIndex);
    }

    // ============================================================
    // Drag Icon Helpers
    // ============================================================
    
    private void AcquireDragCanvas()    // Ensures we have a root canvas to draw the drag icon on top.
    {
        if (dragCanvas == null)
            dragCanvas = GetComponentInParent<Canvas>();
    }
    
    private void CreateDragIcon(int itemId) // Creates the floating icon that follows the mouse while dragging.
    {
        dragIcon = new GameObject("DragIcon", typeof(RectTransform), typeof(Image));
        dragIcon.transform.SetParent(dragCanvas.transform, false);
        dragIcon.transform.SetAsLastSibling(); // render on top

        dragIconRect = dragIcon.GetComponent<RectTransform>();
        dragIconRect.sizeDelta = new Vector2(50, 50);

        var img = dragIcon.GetComponent<Image>();
        img.sprite = GetItemIcon(itemId);
        img.preserveAspect = true;
        img.raycastTarget = false; // allow raycasts to UI below
    }
    
    private void CleanupDragVisuals()   // Safely removes the floating icon instance.
    {
        if (dragIcon != null)
            Destroy(dragIcon);

        dragIcon = null;
        dragIconRect = null;
    }

    // ============================================================
    // Utility
    // ============================================================
    
    private Sprite GetItemIcon(int itemId)  // Returns the icon sprite associated with an item ID.
    {
        var itemSO = ItemDatabase.Instance.GetItemById(itemId);
        return itemSO != null ? itemSO.icon : null;
    }
    
    private bool IsLeftClick(PointerEventData e)    // Checks if the drag started with left-click only.
        => e.button == PointerEventData.InputButton.Left;
}
