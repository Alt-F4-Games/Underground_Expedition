using UnityEngine;

public class HotbarSelector : MonoBehaviour
{
    [SerializeField] private HotbarUI hotbarUI;
    [SerializeField] private InventoryUI inventoryUI; 

    private int selectedIndex = 0;

    private void Update()
    {
        if (inventoryUI != null && inventoryUI.IsVisible()) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int max = hotbarUI.SlotCount;
            selectedIndex = (selectedIndex - Mathf.RoundToInt(scroll * 10) + max) % max;
            hotbarUI.HighlightSlot(selectedIndex);
        }
    }
}
