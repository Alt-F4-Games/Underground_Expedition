using System.Collections;
using System.Collections.Generic;
using Network.Crafting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Crafting
{
    public class CraftingUIController : MonoBehaviour
{
    public static CraftingUIController Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Recipe List")]
    [SerializeField] private Transform recipeListContainer;
    [SerializeField] private RecipeEntryUI recipeEntryPrefab;

    [Header("Recipe Details")]
    [SerializeField] private Image resultIcon;
    [SerializeField] private TMP_Text resultName;
    [SerializeField] private TMP_Text resultDescription;

    [Header("Ingredients")]
    [SerializeField] private Transform ingredientsContainer;
    [SerializeField] private IngredientEntryUI ingredientPrefab;

    [Header("Buttons")]
    [SerializeField] private Button craftButton;
    
    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    private readonly List<RecipeEntryUI> _recipeEntries = new();
    private readonly List<IngredientEntryUI> _ingredientEntries = new();

    private CraftingRecipeSO _selectedRecipe;
    private NetworkInventoryManager _localManager;

    private void Awake()
    {
        Instance = this;

        root.SetActive(false);
        
        feedbackText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!root.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }
    
    // =========================================================
    // OPEN / CLOSE
    // =========================================================

    public void Open(NetworkPlayerController player)
    {
        if (root.activeSelf)
            return;
        
        _localManager = player.GetComponent<NetworkInventoryManager>();

        if (_localManager == null)
            return;

        root.SetActive(true);

        InputManager.SetMode(InputMode.UI);

        RefreshRecipeList();

        _localManager.inventorySystem.OnInventoryChanged += RefreshCurrentRecipe;
    }

    public void Close()
    {
        if (_localManager)
        {
            _localManager.inventorySystem.OnInventoryChanged -= RefreshCurrentRecipe;
        }

        _selectedRecipe = null;

        ClearIngredients();
        
        root.SetActive(false);

        InputManager.SetMode(InputMode.Game);
    }

    public void OnCloseButtonPressed()
    {
        Close();
    }
    
    // =========================================================
    // RECIPE LIST
    // =========================================================

    private void RefreshRecipeList()
    {
        ClearRecipeList();

        var recipes = CraftingDatabase.Instance.GetAllRecipes();

        foreach (var recipe in recipes)
        {
            var entry = Instantiate(recipeEntryPrefab, recipeListContainer);

            bool unlocked = CraftingUnlockSystem.IsUnlocked(
                _localManager.inventorySystem,
                recipe);

            entry.Setup(recipe, this, unlocked);

            _recipeEntries.Add(entry);
        }
        
        if (recipes.Count > 0)
        {
            SelectRecipe(recipes[0]);
        }
        else
        {
            ClearRecipeDetails();
        }
    }
    
    private void ClearRecipeDetails()
    {
        resultIcon.sprite = null;
        resultName.text = "";
        resultDescription.text = "";

        craftButton.interactable = false;
    }

    private void ClearRecipeList()
    {
        foreach (var entry in _recipeEntries)
        {
            Destroy(entry.gameObject);
        }

        _recipeEntries.Clear();
    }

    // =========================================================
    // SELECT RECIPE
    // =========================================================

    public void SelectRecipe(CraftingRecipeSO recipe)
    {
        _selectedRecipe = recipe;

        RefreshCurrentRecipe();
    }

    private void RefreshCurrentRecipe()
    {
        if (_selectedRecipe == null)
            return;

        var item = ItemDatabase.Instance.GetItemById(
            _selectedRecipe.resultItemId);

        resultIcon.sprite = item.icon;
        resultName.text = item.itemName;
        resultDescription.text = item.description;

        RefreshIngredients();

        bool canCraft = CraftingService.CanCraft(
            _localManager.inventorySystem,
            _selectedRecipe);

        craftButton.interactable = canCraft;

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(CraftSelectedRecipe);
    }

    // =========================================================
    // INGREDIENTS
    // =========================================================

    private void RefreshIngredients()
    {
        ClearIngredients();

        foreach (var ingredient in _selectedRecipe.ingredients)
        {
            int owned = _localManager.inventorySystem.CountItem(
                ingredient.itemId);

            var entry = Instantiate(
                ingredientPrefab,
                ingredientsContainer);

            entry.Setup(
                ingredient.itemId,
                ingredient.quantity,
                owned);

            _ingredientEntries.Add(entry);
        }
    }

    private void ClearIngredients()
    {
        foreach (var entry in _ingredientEntries)
        {
            Destroy(entry.gameObject);
        }

        _ingredientEntries.Clear();
    }

    // =========================================================
    // CRAFT
    // =========================================================

    private void CraftSelectedRecipe()
    {
        if (_selectedRecipe == null)
            return;

        bool canAdd = _localManager.inventorySystem.CanAddItemGlobal(
            _selectedRecipe.resultItemId,
            _selectedRecipe.resultQuantity);

        if (!canAdd)
        {
            ShowMessage("Inventory FULL");
            return;
        }

        _localManager.Input_Craft(_selectedRecipe.resultItemId);
        ShowMessage("Item Added To Your Bag!");
    }
    
    // =========================================================
    // FEEDBACK
    // =========================================================
    
    public void ShowMessage(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;

        yield return new WaitForSeconds(2f);

        feedbackText.gameObject.SetActive(false);
    }

}
}