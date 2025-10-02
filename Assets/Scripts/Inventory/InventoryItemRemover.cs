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
    }
}
