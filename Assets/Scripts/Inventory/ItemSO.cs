using UnityEngine;

public enum ItemType
{
    Pickup,
    ToolActive,
    ToolPassive,
    KeyItem
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/New Item")]
public class ItemSO :  ScriptableObject
{
    [Header("General Info")]
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public int maxStack = 1;
    
    [Header("Tool Properties")]
    public bool hasDurability;
    public int maxDurability;

    [TextArea]
    public string description;

    private void OnValidate()
    {
        switch (itemType)
        {
            case ItemType.Pickup:
                maxStack = 16;
                hasDurability = false;
                break;

            case ItemType.ToolActive:
            case ItemType.ToolPassive:
                maxStack = 1;
                hasDurability = true;
                break;

            case ItemType.KeyItem:
                maxStack = 1;
                hasDurability = false;
                break;
        }
    }

}
