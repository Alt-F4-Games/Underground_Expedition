using System.Collections.Generic;

namespace Network.Crafting
{
    public static class CraftingUnlockSystem
    {
        public static List<CraftingRecipeSO> GetUnlockedRecipes(NetworkInventorySystem inventory)
        {
            List<CraftingRecipeSO> unlocked = new();

            foreach (var recipe in CraftingDatabase.Instance.GetAllRecipes())
            {
                foreach (var ingredient in recipe.ingredients)
                {
                    if (inventory.CountItem(ingredient.itemId) > 0)
                    {
                        unlocked.Add(recipe);
                        break;
                    }
                }
            }

            return unlocked;
        }
    }
}