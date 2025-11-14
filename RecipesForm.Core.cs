using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace CostChef
{
    public partial class RecipesForm : Form
    {
        private DataGridView dataGridViewIngredients;
        private Button btnAddIngredient;
        private Button btnRemoveIngredient;
        private Button btnCalculate;
        private Button btnClose;
        private Button btnSaveRecipe;
        private Button btnLoadRecipe;
        private Button btnDeleteRecipe;
        private Label lblCostSummary;
        private ComboBox cmbIngredients;
        private TextBox txtQuantity;
        private Label lblRecipeName;
        private TextBox txtRecipeName;
        private Label lblBatchYield;
        private TextBox txtBatchYield;
        private Label lblFoodCost;
        private ComboBox cmbFoodCost;
        private ComboBox cmbExistingRecipes;
        private Label lblExistingRecipes;
        private Button btnRefreshRecipes;
        private TextBox txtSearchRecipes;
        private Button btnSearchRecipes;
        private Button btnClearSearch;

        // Category and tags controls
        private Label lblCategory;
        private ComboBox cmbCategory;
        private TextBox txtNewCategory;
        private Label lblTags;
        private TextBox txtTags;
        private Button btnManageCategories;

        // NEW: Unit display label
        private Label lblUnitDisplay;

        // Version History button
        private Button btnVersionHistory;

        private Recipe currentRecipe = new Recipe();
        private List<RecipeIngredient> currentIngredients = new List<RecipeIngredient>();
        private List<Recipe> allRecipes = new List<Recipe>();
        
        // Currency symbol - now uses AppSettings
        private string currencySymbol => AppSettings.CurrencySymbol;

        // Add this event for recipe updates
        public event Action RecipesUpdated;

        public RecipesForm()
        {
            InitializeComponent();
            LoadIngredientsComboBox();
            LoadExistingRecipes();
            InitializeNewRecipe();
            LoadCategories();
            ConfigureIngredientsGrid();
        }

        // ========== VERSIONING INTEGRATION METHODS ==========
        
        private void CreateVersionForChange(string changeDescription)
        {
            if (currentRecipe.Id > 0)
            {
                try
                {
                    RecipeVersioningService.CreateVersion(
                        currentRecipe.Id,
                        $"Auto {DateTime.Now:yyyy-MM-dd HH:mm}",
                        changeDescription,
                        "System"
                    );
                }
                catch (Exception ex)
                {
                    // Silent fail - versioning shouldn't break the main functionality
                    System.Diagnostics.Debug.WriteLine($"Auto-version failed: {ex.Message}");
                }
            }
        }

        private void CreateInitialVersion()
        {
            if (currentRecipe.Id > 0)
            {
                try
                {
                    RecipeVersioningService.CreateVersion(
                        currentRecipe.Id,
                        "Initial Version",
                        "Recipe created",
                        "System"
                    );
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Initial version creation failed: {ex.Message}");
                }
            }
        }

        private void ShowVersionHistory()
        {
            if (currentRecipe.Id > 0)
            {
                var versionForm = new RecipeVersionHistoryForm(currentRecipe.Id, currentRecipe.Name);
                versionForm.ShowDialog();
                
                // Refresh the recipe after version operations
                if (versionForm.DialogResult == DialogResult.OK)
                {
                    LoadSelectedRecipe(); // Reload the current recipe
                }
            }
            else
            {
                MessageBox.Show("Please load or save a recipe first to access version history.", 
                    "No Recipe Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}