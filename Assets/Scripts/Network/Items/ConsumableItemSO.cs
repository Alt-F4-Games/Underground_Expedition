using UnityEngine;
using System.Collections.Generic;

namespace Network.Items
{
    [CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Items/Consumable")]
    public class ConsumableItemSO : ItemSo
    {
        [Header("Modular Strategy Effects")]
        // Enables Unity's native dynamic dropdown menu in the Inspector
        [SerializeReference] 
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
                if (effect.Apply(player))
                {
                    appliedAtLeastOne = true;
                }
            }

            // If at least one effect was successfully executed, return true to consume one unit
            return appliedAtLeastOne;
        }
    }
}