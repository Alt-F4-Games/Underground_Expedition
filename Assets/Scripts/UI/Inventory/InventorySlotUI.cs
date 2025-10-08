using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class InventorySlotUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image frameImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI qtyText;

    [Header("Drop Settings")]
    [SerializeField] private KeyCode dropKey = KeyCode.Q;   
    [SerializeField] private GameObject worldItemPrefab;    
    

    private ItemSO currentItem;
    private int currentQty;
    private bool isHovered;

    public int SlotIndex { get; set; }
    public SlotType SlotType { get; set; }
    public InventoryManager Manager { get; set; }

    private void Update()
    {
        if (isHovered && HasItem && Input.GetKeyDown(dropKey))
        {
            DropOneItem();
        }
    }

    private void DropOneItem()
    {
        
        ItemSO itemToDrop = currentItem;

       
        bool removed = Manager.RemoveQuantity(itemToDrop, 1, SlotType);
        if (!removed) return;

        
        if (worldItemPrefab != null )
        {

            var go = Instantiate(worldItemPrefab, Vector3.zero, Quaternion.identity);

            var pickup = go.GetComponent<WorldItemPickup>();
            if (pickup != null)
                pickup.Setup(itemToDrop, 1); 
        }

        Debug.Log($"[InventorySlotUI] Dropped 1x {itemToDrop.itemName}");
    }

    public void Setup(ItemSO item, int quantity)
    {
        currentItem = item;
        currentQty = Mathf.Max(0, quantity);

        if (iconImage != null)
            iconImage.enabled = false;
        if (qtyText != null)
            qtyText.text = string.Empty;

        if (item != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = item.icon;
                iconImage.enabled = true;
                iconImage.color = Color.white;
            }

            if (qtyText != null)
                qtyText.text = quantity > 1 ? quantity.ToString() : string.Empty;
        }
        else
        {
            if (iconImage != null)
                iconImage.sprite = null;

            if (qtyText != null)
                qtyText.text = string.Empty;
        }
    }

    public void Clear() => Setup(null, 0);

    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => isHovered = false;

    public ItemSO CurrentItem => currentItem;
    public int CurrentQuantity => currentQty;
    public bool HasItem => currentItem != null;
}