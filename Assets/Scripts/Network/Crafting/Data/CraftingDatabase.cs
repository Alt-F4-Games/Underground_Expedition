using System.Collections.Generic;
using UnityEngine;

namespace Network.Crafting
{
    [CreateAssetMenu(menuName = "Crafting/Database")]
    public class CraftingDatabase : ScriptableObject
    {
        public static CraftingDatabase Instance { get; private set; }

        [SerializeField]
        private List<CraftingRecipeSO> recipes = new();

        private Dictionary<int, CraftingRecipeSO> _lookup = new();

        public void Initialize()
        {
            Instance = this;

            _lookup.Clear();

            foreach (var recipe in recipes)
            {
                _lookup.TryAdd(recipe.resultItemId, recipe);
            }
        }

        public List<CraftingRecipeSO> GetAllRecipes()
        {
            return recipes;
        }

        public CraftingRecipeSO GetRecipeByResult(int itemId)
        {
            return _lookup.GetValueOrDefault(itemId);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadDatabase()
        {
            var db = Resources.Load<CraftingDatabase>("CraftingDatabase");

            if (db != null)
                db.Initialize();
        }
    }
}