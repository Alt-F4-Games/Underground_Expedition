using UnityEngine;

namespace Network.Items
{
    public enum ItemType
    {
        Pickup,
        Consumable,
        KeyItem
    }

    public abstract class ItemSo : ScriptableObject
    {
        [Header("General Info")]
        public string itemName;
        public Sprite icon;
        public ItemType itemType;
        public int maxStack = 1;

        [TextArea]
        public string description;

        protected virtual void OnValidate()
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

        public virtual bool UseItem(NetworkPlayerController player)
        {
            Debug.LogWarning($"[SERVER] Base item {itemName} does not have a defined use method.");
            return false;
        }
    }
}