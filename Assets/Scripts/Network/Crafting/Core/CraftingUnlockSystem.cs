using Network.Inventory;
using Network.Items;

namespace Network.Crafting
{
    public static class CraftingUnlockSystem
    {
        public static bool IsUnlocked(NetworkInventorySystem inventory, CraftingRecipeSO recipe)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                int networkId = ItemDatabase.Instance.GetNetworkId(ingredient.itemId);

                if (inventory.CountItem(networkId) > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}