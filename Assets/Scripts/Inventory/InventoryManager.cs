using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Inventory inventory = new Inventory();
    public UnityEvent OnInventoryChanged;
    
    
}
