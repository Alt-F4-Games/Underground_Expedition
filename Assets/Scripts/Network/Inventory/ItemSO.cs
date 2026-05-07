using UnityEngine;

public enum ItemType
{
    Pickup,
    Tool,
    Consumable,
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
    public bool isTool;
    public bool hasDurability;
    public int maxDurability;

    [TextArea]
    public string description;

    private void OnValidate()
    {
        switch (itemType)
        {
            case ItemType.Pickup:
                maxStack = 32;
                hasDurability = false;
                break;

            case ItemType.Tool:
                maxStack = 1;
                hasDurability = true;
                break;
            
            case ItemType.Consumable:
                maxStack = 8;
                hasDurability = false;
                break;

            case ItemType.KeyItem:
                maxStack = 1;
                hasDurability = false;
                break;
        }
    }

    public bool IsTool => itemType == ItemType.Tool;
    public bool IsConsumable => itemType == ItemType.Consumable;
    public bool IsPickup => itemType == ItemType.Pickup;
    public bool IsKeyItem => itemType == ItemType.KeyItem;

}
