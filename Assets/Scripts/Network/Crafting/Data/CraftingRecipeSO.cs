using System.Collections.Generic;
using UnityEngine;

namespace Network.Crafting
{
    [CreateAssetMenu(menuName = "Crafting/Recipe")]
    public class CraftingRecipeSO : ScriptableObject
    {
        [Header("Recipe Info")]
        public string recipeName;
        
        [Header("Craft Result")]
        public int resultItemId;
        public int resultQuantity = 1;

        [Header("Ingredients")]
        public List<CraftingIngredient> ingredients = new();
    }
}