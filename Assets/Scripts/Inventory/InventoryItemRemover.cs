using UnityEngine;

public class InventoryItemRemover : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ItemSO itemToRemove;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveSpecificItem();
        }
    }

    private void RemoveSpecificItem()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager not asigned to InventoryItemRemover.");
            return;
        }
        
        if (itemToRemove == null)
        {
            Debug.LogError("ItemToRemove not asigned to InventoryItemRemover.");
            return;
        }
        
        bool itemExists = false;
        foreach (var slot in inventoryManager.GetInventory().GetSlots())
        {
            if (slot.Item == itemToRemove && slot.Quantity > 0)
            {
                itemExists = true;
                break;
            }
        }

        
        if (itemExists && inventoryManager.RemoveItem(itemToRemove, 1))
        {
            Debug.Log($"Eliminated: {itemToRemove.itemName} x1 from inventory.");
        }
        else
        {
            Debug.Log($"Cannot eliminate {itemToRemove.itemName}: not found in inventory or insufficient quantity.");
        }
    }
}
