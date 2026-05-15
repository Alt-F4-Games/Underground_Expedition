

namespace Network.Crafting
{
    public static class CraftingUnlockSystem
    {
        public static bool IsUnlocked(
            NetworkInventorySystem inventory,
            CraftingRecipeSO recipe)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                if (inventory.CountItem(ingredient.itemId) > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}