using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private ItemSO item;
    [SerializeField] private int quantity = 1;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        InventoryManager inventoryManager = other.GetComponent<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.AddItem(item, quantity);
            Destroy(gameObject); 
        }
    }
}
