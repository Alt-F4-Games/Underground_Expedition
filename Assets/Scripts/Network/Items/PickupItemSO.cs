using UnityEngine;

namespace Network.Items
{
    [CreateAssetMenu(fileName = "NewPickup", menuName = "Inventory/Items/Pickup")]
    public class PickupItemSO : ItemSo
    {
        protected override void OnValidate()
        {
            itemType = ItemType.Pickup;
            maxStack = 32;
        }

        public override bool UseItem(NetworkPlayerController player)
        {
            // Materials are consumed through the CraftingService
            Debug.Log($"[SERVER] {itemName} is a crafting material. It cannot be consumed from the hotbar.");
            return false;
        }
    }
}