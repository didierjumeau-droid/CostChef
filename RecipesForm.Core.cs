using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

namespace CostChef
{
    public partial class RecipesForm : Form
    {
        // Control declarations remain here (shared across all partial files)
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

        // Sales Price Controls - ADDED
        private Label lblSalesPrice;
        private TextBox txtSalesPrice;

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

        // Data fields
        private Recipe currentRecipe = new Recipe();
        private List<RecipeIngredient> currentIngredients = new List<RecipeIngredient>();
        private List<Recipe> allRecipes = new List<Recipe>();
        
        // Currency symbol - now uses AppSettings
        private string currencySymbol => AppSettings.CurrencySymbol;

        // Add this event for recipe updates
        public event Action RecipesUpdated;

        private void LoadIngredientsComboBox()
        {
            try
            {
                var ingredients = DatabaseContext.GetAllIngredients();
                cmbIngredients.DataSource = ingredients;
                cmbIngredients.DisplayMember = "Name";
                cmbIngredients.ValueMember = "Id";
                Debug.WriteLine($"Loaded {ingredients.Count} ingredients into combo box");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading ingredients combo box: {ex.Message}");
                MessageBox.Show($"Error loading ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeNewRecipe()
        {
            try
            {
                currentRecipe = new Recipe();
                currentIngredients = new List<RecipeIngredient>();
                txtRecipeName.Text = "New Recipe";
                txtBatchYield.Text = "1";
                txtSalesPrice.Text = "0.00"; // ADDED SALES PRICE INITIALIZATION
                // NOTE: cmbFoodCost needs its items added in InitializeComponent() in RecipesForm.UI.cs
                if (cmbFoodCost.Items.Count > 1)
                    cmbFoodCost.SelectedIndex = 1; // 30% default
                txtTags.Text = string.Empty;
                cmbCategory.Text = string.Empty;
                RefreshIngredientsGrid();
                
                Debug.WriteLine("Initialized new recipe");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing new recipe: {ex.Message}");
            }
        }

        // FIX: Update RecipesForm_Load to call the new methods (CS0103 fix)
        private void RecipesForm_Load(object sender, EventArgs e)
        {
            try
            {
                LoadIngredientsComboBox();
                LoadExistingRecipes();
                InitializeNewRecipe();
                // Assuming ConfigureIngredientsGrid is in RecipesForm.UI.cs, it's called in LoadSelectedRecipe
                LoadCategories(); // Defined in RecipesForm.Categories.cs
                Debug.WriteLine("Recipes form loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading recipes form: {ex.Message}");
                MessageBox.Show($"Error loading recipes form: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }       
         
        public RecipesForm()
        {
            try
            {
                InitializeComponent();
                LoadIngredientsComboBox();
                LoadExistingRecipes();
                InitializeNewRecipe();
                LoadCategories();
                ConfigureIngredientsGrid();
                Debug.WriteLine("Recipes form constructor completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RecipesForm constructor: {ex.Message}");
                MessageBox.Show($"Error initializing recipes form: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== VERSIONING INTEGRATION METHODS ==========
        // These stay in Core since they're fundamental to recipe management
        
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
                    Debug.WriteLine($"Created version for change: {changeDescription}");
                }
                catch (Exception ex)
                {
                    // Silent fail - versioning shouldn't break the main functionality
                    Debug.WriteLine($"Auto-version failed: {ex.Message}");
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
                    Debug.WriteLine("Created initial version");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Initial version creation failed: {ex.Message}");
                }
            }
        }

        private void ShowVersionHistory()
        {
            if (currentRecipe.Id > 0)
            {
                try
                {
                    var versionForm = new RecipeVersionHistoryForm(currentRecipe.Id, currentRecipe.Name);
                    versionForm.ShowDialog();
                    
                    // Refresh the recipe after version operations
                    if (versionForm.DialogResult == DialogResult.OK)
                    {
                        LoadSelectedRecipe(); // Reload the current recipe
                    }
                    Debug.WriteLine("Version history form closed");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error showing version history: {ex.Message}");
                    MessageBox.Show($"Error showing version history: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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