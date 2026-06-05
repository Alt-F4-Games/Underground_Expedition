using Network.Inventory;
using UnityEngine;

namespace Network.Crafting
{
    public static class CraftingService
    {
        public static bool CanCraft(NetworkInventorySystem inventory, CraftingRecipeSO recipe)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                int networkId =
                    ItemDatabase.Instance.GetNetworkId(ingredient.itemId);

                int owned = inventory.CountItem(networkId);

                if (owned < ingredient.quantity)
                    return false;
            }

            return true;
        }

        public static bool Craft(NetworkInventorySystem inventory, CraftingRecipeSO recipe)
        {
            if (!inventory.HasStateAuthority)
                return false;

            if (!CanCraft(inventory, recipe))
                return false;

            int resultNetworkId = ItemDatabase.Instance.GetNetworkId(recipe.resultItemId);

            bool canStore = inventory.CanAddItemGlobal(resultNetworkId, recipe.resultQuantity);

            if (!canStore)
                return false;

            foreach (var ingredient in recipe.ingredients)
            {
                int ingredientNetworkId = ItemDatabase.Instance.GetNetworkId(ingredient.itemId);

                bool removed = inventory.Server_ConsumeItemGlobal(ingredientNetworkId, ingredient.quantity);

                if (!removed)
                    return false;
            }

            return inventory.Server_AddItemGlobal(resultNetworkId, recipe.resultQuantity);
        }
    }
}