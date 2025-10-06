using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemSO item;
    [SerializeField, Min(1)] private int quantity = 1;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var inventoryManager = other.GetComponent<InventoryManager>();
        if (inventoryManager == null) return;

        bool added = inventoryManager.AddItem(item, quantity, SlotType.Base);
        if (added)
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"Could not pick up {item.itemName} x{quantity}: inventory full or invalid slot.");
        }
    }
}