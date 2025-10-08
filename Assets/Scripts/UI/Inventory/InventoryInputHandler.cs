using UnityEngine;

public class InventoryInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Input Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private void Reset()
    {
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            inventoryUI.ToggleVisibility();
        }
    }
}