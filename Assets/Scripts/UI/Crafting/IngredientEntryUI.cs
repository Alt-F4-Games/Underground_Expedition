using Network.Inventory;
using Network.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Crafting
{
    public class IngredientEntryUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text quantityText;

        public void Setup(
            string itemId,
            int required,
            int owned)
        {
            var item =
                ItemDatabase.Instance.GetItemByGameplayId(
                    itemId);

            if (item == null)
                return;

            icon.sprite = item.icon;

            quantityText.text = $"{owned}/{required}";

            quantityText.color =
                owned >= required
                    ? Color.green
                    : Color.red;
        }
    }
}