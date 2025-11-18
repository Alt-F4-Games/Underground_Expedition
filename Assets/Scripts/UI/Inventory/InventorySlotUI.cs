using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Quitamos la dependencia de RequireComponent(Button) si no es estricta, pero la dejamos si tu prefab lo usa.
public class InventorySlotUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI qtyText;
    [SerializeField] private GameObject highlightFrame;

    [Header("Settings")]
    [SerializeField] private KeyCode dropKey = KeyCode.Q;   

    // Propiedades públicas
    public int SlotIndex { get; set; }
    public SlotType SlotType { get; set; }
    public NetworkInventoryManager Manager { get; set; } // Referencia al script de red

    private int _currentItemId;
    private int _currentQty;
    private bool _isHovered;

    private void Update()
    {
        // Detectar input para tirar objeto (Q)
        if (_isHovered && HasItem && Input.GetKeyDown(dropKey))
        {
            DropOneItem();
        }
    }

    private void DropOneItem()
    {
        if (Manager != null)
        {
            // Llamamos al método que envía el RPC
            Manager.Input_DropItem(SlotType, SlotIndex);
            Debug.Log($"[SlotUI] Requesting drop from {SlotType}[{SlotIndex}]");
        }
    }

    // Método principal de actualización visual
    public void Refresh(NetworkInventorySlot slotData)
    {
        _currentItemId = slotData.ItemId;
        _currentQty = slotData.Quantity;

        ItemSO itemSO = ItemDatabase.Instance.GetItemById(_currentItemId);

        if (itemSO != null && _currentQty > 0)
        {
            if (iconImage != null)
            {
                iconImage.sprite = itemSO.icon;
                iconImage.enabled = true; 
                iconImage.color = Color.white;
            }

            if (qtyText != null)
                qtyText.text = _currentQty > 1 ? _currentQty.ToString() : string.Empty;
        }
        else
        {
            Clear(); 
        }
    }

    public void Clear()
    {
        _currentItemId = -1;
        _currentQty = 0;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false; 
        }

        if (qtyText != null)
            qtyText.text = string.Empty;
    }
    
    public void SetHighlight(bool active)
    {
        if (highlightFrame != null) highlightFrame.SetActive(active);
    }

    // Getters helpers
    public bool HasItem => _currentItemId > 0 && _currentQty > 0;
    public int CurrentItemId => _currentItemId;

    // Interfaces de puntero (Mouse)
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) => _isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => _isHovered = false;
}