using Local.Inventory;
using Network.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI qtyText;
    [SerializeField] private GameObject highlightFrame;

    [Header("Settings")]
    [SerializeField] private KeyCode dropKey = KeyCode.Q;

    //---------------- External references --------------------
    public int SlotIndex { get; set; }
    public SlotType SlotType { get; set; }
    public NetworkInventoryManager Manager { get; set; }

    //------------------- Internal state -----------------
    private int _currentItemId;
    private int _currentQty;
    private bool _isHovered;
    private static InventorySlotUI _selectedSlot;

    // =====================================================================
    // Unity
    // =====================================================================

    private void Update()
    {
        if (_isHovered && HasItem && Input.GetKeyDown(dropKey))
            DropOneItem();

        if (_selectedSlot == this)
        {
            InventorySlotDragHandler.UpdateDragPosition(Input.mousePosition);
        }
    }

    // =====================================================================
    // CLICK SYSTEM
    // =====================================================================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        HandleClickSelection();
    }

    private void HandleClickSelection()
    {
        if (!HasItem && _selectedSlot == null)
            return;

        if (_selectedSlot == null)
        {
            if (!HasItem) return;

            _selectedSlot = this;
            SetHighlight(true);

            InventorySlotDragHandler.BeginDragFromSlot(this);

            return;
        }

        if (_selectedSlot == this)
        {
            ClearSelection();
            return;
        }

        TryMoveItem(_selectedSlot, this);

        ClearSelection();
    }

    private void ClearSelection()
    {
        if (_selectedSlot != null)
            _selectedSlot.SetHighlight(false);

        InventorySlotDragHandler.EndDragVisual();

        _selectedSlot = null;
    }

    private void TryMoveItem(InventorySlotUI from, InventorySlotUI to)
    {
        if (from.Manager != to.Manager || from.Manager == null)
            return;

        from.Manager.Input_MoveItem(from.SlotType, from.SlotIndex, to.SlotType, to.SlotIndex);
    }

    // =====================================================================
    // Item Drop
    // =====================================================================

    private void DropOneItem()
    {
        if (!Manager) return;

        Manager.Input_DropItem(SlotType, SlotIndex);

#if UNITY_EDITOR
        Debug.Log($"[InventorySlotUI] Requested drop from {SlotType}[{SlotIndex}]");
#endif
    }

    // =====================================================================
    // Visual Refresh
    // =====================================================================

    public void Refresh(NetworkInventorySlot slotData)
    {
        _currentItemId = slotData.ItemId;
        _currentQty = slotData.Quantity;

        if (HasItem)
            ApplyItemVisuals(_currentItemId, _currentQty);
        else
            Clear();
    }

    private void ApplyItemVisuals(int itemId, int quantity)
    {
        var itemSo = ItemDatabase.Instance.GetItemByNetworkId(itemId);
        if (!itemSo) { Clear(); return; }

        if (iconImage)
        {
            iconImage.sprite = itemSo.icon;
            iconImage.enabled = true;
            iconImage.color = Color.white;
        }

        if (qtyText)
            qtyText.text = quantity > 1 ? quantity.ToString() : string.Empty;
    }

    public void Clear()
    {
        _currentItemId = -1;
        _currentQty = 0;

        if (iconImage)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (qtyText)
            qtyText.text = string.Empty;
    }

    // =====================================================================
    // Highlighting
    // =====================================================================

    public void SetHighlight(bool active)
    {
        if (highlightFrame != null)
            highlightFrame.SetActive(false);
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    public bool HasItem => _currentItemId > 0 && _currentQty > 0;
    public int CurrentItemId => _currentItemId;

    public RectTransform ItemIconRect
    {
        get
        {
            if (iconImage == null) return null;
            return iconImage.rectTransform;
        }
    }
    
    // =====================================================================
    // Pointer Events
    // =====================================================================

    public void OnPointerEnter(PointerEventData eventData) => _isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => _isHovered = false;
}