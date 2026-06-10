using UnityEngine;
using System.Collections.Generic;

namespace Network.Items
{
    [CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Items/Consumable")]
    public class ConsumableItemSO : ItemSo
    {
        [Header("Modular Strategy Effects")]
        // List of modular strategy effects. Now uses drag-and-drop ScriptableObjects.
        public List<ItemEffect> effects = new List<ItemEffect>();

        protected override void OnValidate()
        {
            itemType = ItemType.Consumable;
            maxStack = 8;
        }

        public override bool UseItem(NetworkPlayerController player)
        {
            if (player == null) return false;

            bool appliedAtLeastOne = false;

            // The server iterates through each modular strategy injected into the item
            foreach (var effect in effects)
            {
                // Added a null check in case an empty element is left in the Inspector list
                if (effect != null && effect.Apply(player))
                {
                    appliedAtLeastOne = true;
                }
            }

            // If at least one effect was successfully executed, return true to consume one unit
            return appliedAtLeastOne;
        }
    }
}