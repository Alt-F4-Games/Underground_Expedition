using UnityEngine;

public enum ItemType
{
    Pickup,
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

    [TextArea]
    public string description;

    private void OnValidate()
    {
        switch (itemType)
        {
            case ItemType.Pickup:
                maxStack = 32;
                break;
            
            case ItemType.Consumable:
                maxStack = 8;
                break;

            case ItemType.KeyItem:
                maxStack = 1;
                break;
        }
    }
    
    public bool IsPickup => itemType == ItemType.Pickup;
    public bool IsConsumable => itemType == ItemType.Consumable;
    public bool IsKeyItem => itemType == ItemType.KeyItem;
}
