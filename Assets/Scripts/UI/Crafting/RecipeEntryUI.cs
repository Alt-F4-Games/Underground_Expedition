using Network.Crafting;
using Network.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Crafting
{
    public class RecipeEntryUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image icon;

        [SerializeField] private TMP_Text recipeName;

        [SerializeField] private Button button;

        [SerializeField] private CanvasGroup canvasGroup;

        private CraftingRecipeSO _recipe;

        private CraftingUIController _controller;

        public void Setup(
            CraftingRecipeSO recipe,
            CraftingUIController controller,
            bool unlocked)
        {
            _recipe = recipe;
            _controller = controller;

            var item =
                ItemDatabase.Instance.GetItemByGameplayId(
                    recipe.resultItemId);

            if (item == null)
                return;

            icon.sprite = item.icon;
            recipeName.text = item.itemName;

            canvasGroup.alpha =
                unlocked
                    ? 1f
                    : 0.45f;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _controller.SelectRecipe(_recipe);
        }
    }
}