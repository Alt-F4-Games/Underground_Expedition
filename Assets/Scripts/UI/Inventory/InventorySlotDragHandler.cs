using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(InventorySlotUI))]
public class InventorySlotDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private InventorySlotUI slotUI;
    private CanvasGroup canvasGroup;
    
    // Variables estáticas para manejar el arrastre globalmente
    private static GameObject dragIcon;
    private static RectTransform dragIconRect;
    private static InventorySlotUI dragSourceSlot;
    private static Canvas dragCanvas;

    private void Awake()
    {
        slotUI = GetComponent<InventorySlotUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!slotUI.HasItem) return;

        dragSourceSlot = slotUI;
        
        // Encontrar el Canvas raíz para dibujar el icono flotante
        if (dragCanvas == null) dragCanvas = GetComponentInParent<Canvas>();

        // Crear icono visual temporal
        dragIcon = new GameObject("DragIcon", typeof(RectTransform), typeof(Image));
        dragIcon.transform.SetParent(dragCanvas.transform, false);
        dragIcon.transform.SetAsLastSibling(); // Asegurar que se dibuje encima de todo

        // Obtener imagen desde la base de datos para asegurar que es correcta
        var itemSO = ItemDatabase.Instance.GetItemById(slotUI.CurrentItemId);
        var img = dragIcon.GetComponent<Image>();
        img.sprite = itemSO != null ? itemSO.icon : null;
        img.preserveAspect = true;
        img.raycastTarget = false; // Importante para que el Raycast atraviese el icono y detecte el slot destino

        dragIconRect = dragIcon.GetComponent<RectTransform>();
        dragIconRect.sizeDelta = new Vector2(50, 50); // Tamaño fijo o dinámico
        
        canvasGroup.blocksRaycasts = false; // Permitir que el raycast pase a través del slot origen
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
        // Si no hay origen o el origen soy yo mismo, cancelar
        if (dragSourceSlot == null || dragSourceSlot == slotUI) return;

        // Ejecutar lógica de movimiento
        TryMoveItem(dragSourceSlot, slotUI);
    }

    private void TryMoveItem(InventorySlotUI from, InventorySlotUI to)
    {
        // Verificamos que ambos pertenezcan al mismo manager (el local)
        if (from.Manager != to.Manager) return;
        if (from.Manager == null) return;

        // Solicitamos al Manager enviar el RPC
        from.Manager.Input_MoveItem(from.SlotType, from.SlotIndex, to.SlotType, to.SlotIndex);
    }

    private void SafeDestroyDragIcon()
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
        }
    }
}