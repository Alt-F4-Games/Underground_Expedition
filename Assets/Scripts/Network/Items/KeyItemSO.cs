using UnityEngine;

namespace Network.Items
{
    [CreateAssetMenu(fileName = "NewKeyItem", menuName = "Inventory/Items/Key Item")]
    public class KeyItemSO : ItemSo
    {
        protected override void OnValidate()
        {
            itemType = ItemType.KeyItem;
            maxStack = 1;
        }

        public override bool UseItem(NetworkPlayerController player)
        {
            // Its presence is validated when interacting with world objects (e.g. Respawn Tables)
            Debug.Log($"[SERVER] {itemName} is a key item. It requires physical interaction with a map target.");
            return false;
        }
    }
}