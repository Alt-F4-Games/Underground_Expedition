using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Inventory inventory = new Inventory();
    public UnityEvent OnInventoryChanged;
    
    public void AddItem(ItemSO item, int quantity = 1)
    {
        if (inventory.AddItem(item, quantity))
        {
            OnInventoryChanged.Invoke();
            Debug.Log($"Added {item.itemName} x{quantity} to inventory");
        }
        else
        {
            Debug.Log("Inventory full or invalid item");
        }
    }
    
    public void RemoveItem(ItemSO item, int quantity = 1)
    {
        if (inventory.RemoveItem(item, quantity))
        {
            OnInventoryChanged.Invoke();
            Debug.Log($"Removed {quantity} x {item.itemName} from inventory.");
        }
        else
        {
            Debug.Log("Failed to remove item. Insufficient quantity.");
        }
    }
    
    public Inventory GetInventory() => inventory;
}
