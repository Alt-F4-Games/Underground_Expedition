using UnityEngine;

namespace Network.Crafting
{
    public static class CraftingService
    {
        public static bool CanCraft(NetworkInventorySystem inventory, CraftingRecipeSO recipe)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                int owned = inventory.CountItem(ingredient.itemId);

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

                bool canStore = inventory.CanAddItemGlobal(
                    recipe.resultItemId,
                    recipe.resultQuantity);

                if (!canStore)
                {
                    Debug.Log("INVENTORY FULL");
                    return false;
                }

                // Consume ingredients
                foreach (var ingredient in recipe.ingredients)
                {
                    bool removed = inventory.Server_ConsumeItemGlobal(
                        ingredient.itemId,
                        ingredient.quantity);

                    if (!removed)
                        return false;
                }

                bool added = inventory.Server_AddItemGlobal(
                    recipe.resultItemId,
                    recipe.resultQuantity);

                return added;
            
        }
    }
}