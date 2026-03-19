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

    // =====================================================================
    // Unity
    // =====================================================================

    private void Update()
    {
        if (_isHovered && HasItem && Input.GetKeyDown(dropKey))
            DropOneItem();
    }

    // =====================================================================
    // Item Drop
    // =====================================================================


    private void DropOneItem()  // Requests the network manager to drop a single item from this slot.
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
    
    public void Refresh(NetworkInventorySlot slotData)  // Updates UI visuals based on the given network slot data.
    {
        _currentItemId = slotData.ItemId;
        _currentQty = slotData.Quantity;

        if (HasItem)
            ApplyItemVisuals(_currentItemId, _currentQty);
        else
            Clear();
    }
    
    private void ApplyItemVisuals(int itemId, int quantity) // Applies item icon and quantity text to the slot.
    {
        var itemSo = ItemDatabase.Instance.GetItemById(itemId);
        if (!itemSo) { Clear(); return; }

        // Icon
        if (iconImage)
        {
            iconImage.sprite = itemSo.icon;
            iconImage.enabled = true;
            iconImage.color = Color.white;
        }

        // Quantity label
        if (qtyText)
            qtyText.text = quantity > 1 ? quantity.ToString() : string.Empty;
    }
    
    public void Clear() // Clears all visual content from this UI slot.
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
    
    public void SetHighlight(bool active)   // Enables or disables the “selected” highlight frame.
    {
        if (highlightFrame != null)
            highlightFrame.SetActive(active);
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    public bool HasItem => _currentItemId > 0 && _currentQty > 0;
    public int CurrentItemId => _currentItemId;

    // =====================================================================
    // Pointer Events (hover + clicks)
    // =====================================================================

    public void OnPointerEnter(PointerEventData eventData) => _isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => _isHovered = false;

    // No action needed for click, but implemented for interface completeness
    public void OnPointerClick(PointerEventData eventData) { }
}
