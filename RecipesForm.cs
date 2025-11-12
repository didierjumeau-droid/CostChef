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

        private void InitializeComponent()
        {
            this.dataGridViewIngredients = new DataGridView();
            this.btnAddIngredient = new Button();
            this.btnRemoveIngredient = new Button();
            this.btnCalculate = new Button();
            this.btnClose = new Button();
            this.btnSaveRecipe = new Button();
            this.btnLoadRecipe = new Button();
            this.btnDeleteRecipe = new Button();
            this.lblCostSummary = new Label();
            this.cmbIngredients = new ComboBox();
            this.txtQuantity = new TextBox();
            this.lblRecipeName = new Label();
            this.txtRecipeName = new TextBox();
            this.lblBatchYield = new Label();
            this.txtBatchYield = new TextBox();
            this.lblFoodCost = new Label();
            this.cmbFoodCost = new ComboBox();
            this.cmbExistingRecipes = new ComboBox();
            this.lblExistingRecipes = new Label();
            this.btnRefreshRecipes = new Button();
            this.txtSearchRecipes = new TextBox();
            this.btnSearchRecipes = new Button();
            this.btnClearSearch = new Button();

            // Category and Tags Controls
            this.lblCategory = new Label();
            this.cmbCategory = new ComboBox();
            this.txtNewCategory = new TextBox();
            this.lblTags = new Label();
            this.txtTags = new TextBox();
            this.btnManageCategories = new Button();

            // NEW: Unit display label
            this.lblUnitDisplay = new Label();

            // Version History button
            this.btnVersionHistory = new Button();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Recipe Costing Calculator";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            // Existing Recipes
            this.lblExistingRecipes.Location = new System.Drawing.Point(450, 15);
            this.lblExistingRecipes.Size = new System.Drawing.Size(100, 20);
            this.lblExistingRecipes.Text = "Load Recipe:";
            
            this.cmbExistingRecipes.Location = new System.Drawing.Point(550, 12);
            this.cmbExistingRecipes.Size = new System.Drawing.Size(150, 20);
            this.cmbExistingRecipes.DropDownStyle = ComboBoxStyle.DropDownList;

            this.btnLoadRecipe.Location = new System.Drawing.Point(710, 10);
            this.btnLoadRecipe.Size = new System.Drawing.Size(80, 25);
            this.btnLoadRecipe.Text = "Load";
            this.btnLoadRecipe.Click += (s, e) => LoadSelectedRecipe();

            // Refresh Recipes Button
            this.btnRefreshRecipes.Location = new System.Drawing.Point(620, 10);
            this.btnRefreshRecipes.Size = new System.Drawing.Size(80, 25);
            this.btnRefreshRecipes.Text = "Refresh";
            this.btnRefreshRecipes.Click += (s, e) => LoadExistingRecipes();

            // Search Controls
            this.txtSearchRecipes.Location = new System.Drawing.Point(450, 45);
            this.txtSearchRecipes.Size = new System.Drawing.Size(150, 20);
            this.txtSearchRecipes.PlaceholderText = "Search recipes...";
            this.txtSearchRecipes.KeyPress += (s, e) => 
            {
                if (e.KeyChar == (char)Keys.Enter)
                    SearchRecipes();
            };

            this.btnSearchRecipes.Location = new System.Drawing.Point(610, 42);
            this.btnSearchRecipes.Size = new System.Drawing.Size(60, 25);
            this.btnSearchRecipes.Text = "Search";
            this.btnSearchRecipes.Click += (s, e) => SearchRecipes();

            this.btnClearSearch.Location = new System.Drawing.Point(680, 42);
            this.btnClearSearch.Size = new System.Drawing.Size(60, 25);
            this.btnClearSearch.Text = "Clear";
            this.btnClearSearch.Click += (s, e) => ClearSearch();

            // Recipe Name
            this.lblRecipeName.Location = new System.Drawing.Point(12, 15);
            this.lblRecipeName.Size = new System.Drawing.Size(100, 20);
            this.lblRecipeName.Text = "Recipe Name:";
            
            this.txtRecipeName.Location = new System.Drawing.Point(120, 12);
            this.txtRecipeName.Size = new System.Drawing.Size(200, 20);
            this.txtRecipeName.Text = "New Recipe";
            this.txtRecipeName.TextChanged += (s, e) => 
            {
                currentRecipe.Name = txtRecipeName.Text;
                CheckRecipeNameAvailability();
            };

            // Batch Yield
            this.lblBatchYield.Location = new System.Drawing.Point(12, 45);
            this.lblBatchYield.Size = new System.Drawing.Size(100, 20);
            this.lblBatchYield.Text = "Batch Yield:";
            
            this.txtBatchYield.Location = new System.Drawing.Point(120, 42);
            this.txtBatchYield.Size = new System.Drawing.Size(100, 20);
            this.txtBatchYield.Text = "1";
            this.txtBatchYield.TextChanged += (s, e) => 
            {
                if (int.TryParse(txtBatchYield.Text, out int yield))
                {
                    var oldYield = currentRecipe.BatchYield;
                    currentRecipe.BatchYield = yield;
                    
                    // Create version when batch yield changes significantly
                    if (currentRecipe.Id > 0 && oldYield != yield)
                    {
                        CreateVersionForChange($"Batch yield changed from {oldYield} to {yield}");
                    }
                }
            };

            // Food Cost Target
            this.lblFoodCost.Location = new System.Drawing.Point(240, 45);
            this.lblFoodCost.Size = new System.Drawing.Size(100, 20);
            this.lblFoodCost.Text = "Food Cost %:";
            
            this.cmbFoodCost.Location = new System.Drawing.Point(340, 42);
            this.cmbFoodCost.Size = new System.Drawing.Size(100, 20);
            this.cmbFoodCost.Items.AddRange(new object[] { "25%", "30%", "35%" });
            this.cmbFoodCost.SelectedIndex = 1; // Default to 30%
            this.cmbFoodCost.SelectedIndexChanged += (s, e) => 
            {
                if (cmbFoodCost.SelectedItem != null)
                {
                    var percent = cmbFoodCost.SelectedItem.ToString().Replace("%", "");
                    if (decimal.TryParse(percent, out decimal foodCost))
                    {
                        var oldPercentage = currentRecipe.TargetFoodCostPercentage;
                        currentRecipe.TargetFoodCostPercentage = foodCost / 100m;
                        
                        // Create version when food cost % changes significantly
                        if (currentRecipe.Id > 0 && Math.Abs(oldPercentage - currentRecipe.TargetFoodCostPercentage) > 0.01m)
                        {
                            CreateVersionForChange($"Food cost target changed from {oldPercentage*100:0}% to {foodCost}%");
                        }
                    }
                }
            };

            // Ingredients ComboBox
            this.cmbIngredients.Location = new System.Drawing.Point(12, 75);
            this.cmbIngredients.Size = new System.Drawing.Size(200, 20);
            this.cmbIngredients.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbIngredients.SelectedIndexChanged += (s, e) => UpdateUnitDisplay();

            // Unit Display Label
            this.lblUnitDisplay.Location = new System.Drawing.Point(220, 75);
            this.lblUnitDisplay.Size = new System.Drawing.Size(60, 20);
            this.lblUnitDisplay.Text = "grams";
            this.lblUnitDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblUnitDisplay.BorderStyle = BorderStyle.FixedSingle;

            // Quantity
            this.txtQuantity.Location = new System.Drawing.Point(290, 75);
            this.txtQuantity.Size = new System.Drawing.Size(80, 20);
            this.txtQuantity.Text = "100";
            this.txtQuantity.PlaceholderText = "Quantity";

            // Add Ingredient Button
            this.btnAddIngredient.Location = new System.Drawing.Point(380, 73);
            this.btnAddIngredient.Size = new System.Drawing.Size(80, 25);
            this.btnAddIngredient.Text = "Add";
            this.btnAddIngredient.Click += (s, e) => AddIngredientToRecipe();

            // Remove Ingredient Button
            this.btnRemoveIngredient.Location = new System.Drawing.Point(470, 73);
            this.btnRemoveIngredient.Size = new System.Drawing.Size(80, 25);
            this.btnRemoveIngredient.Text = "Remove";
            this.btnRemoveIngredient.Click += (s, e) => RemoveIngredient();

            // Ingredients DataGrid
            this.dataGridViewIngredients.Location = new System.Drawing.Point(12, 105);
            this.dataGridViewIngredients.Size = new System.Drawing.Size(776, 200);
            this.dataGridViewIngredients.ReadOnly = false;
            this.dataGridViewIngredients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewIngredients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewIngredients.CellEndEdit += new DataGridViewCellEventHandler(this.dataGridViewIngredients_CellEndEdit);

            // Category Label
            this.lblCategory.Location = new System.Drawing.Point(12, 320);
            this.lblCategory.Size = new System.Drawing.Size(100, 20);
            this.lblCategory.Text = "Category:";
            
            // Category ComboBox
            this.cmbCategory.Location = new System.Drawing.Point(120, 317);
            this.cmbCategory.Size = new System.Drawing.Size(150, 20);
            this.cmbCategory.DropDownStyle = ComboBoxStyle.DropDown;
            this.cmbCategory.TextChanged += (s, e) => currentRecipe.Category = cmbCategory.Text;

            // New Category TextBox (for adding new categories)
            this.txtNewCategory.Location = new System.Drawing.Point(280, 317);
            this.txtNewCategory.Size = new System.Drawing.Size(120, 20);
            this.txtNewCategory.PlaceholderText = "New category...";
            this.txtNewCategory.Visible = false;

            // Manage Categories Button
            this.btnManageCategories.Location = new System.Drawing.Point(410, 315);
            this.btnManageCategories.Size = new System.Drawing.Size(120, 25);
            this.btnManageCategories.Text = "Manage Categories";
            this.btnManageCategories.Click += (s, e) => ShowCategoryManager();

            // Tags Label
            this.lblTags.Location = new System.Drawing.Point(12, 350);
            this.lblTags.Size = new System.Drawing.Size(100, 20);
            this.lblTags.Text = "Tags:";
            
            // Tags TextBox
            this.txtTags.Location = new System.Drawing.Point(120, 347);
            this.txtTags.Size = new System.Drawing.Size(300, 20);
            this.txtTags.PlaceholderText = "comma, separated, tags";
            this.txtTags.TextChanged += (s, e) => UpdateRecipeTags();

            // Cost Summary
            this.lblCostSummary.Location = new System.Drawing.Point(12, 380);
            this.lblCostSummary.Size = new System.Drawing.Size(776, 80);
            this.lblCostSummary.Text = "Add ingredients to calculate cost...";
            this.lblCostSummary.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblCostSummary.BorderStyle = BorderStyle.FixedSingle;
            this.lblCostSummary.Font = new System.Drawing.Font("Consolas", 9);

            // Calculate Button
            this.btnCalculate.Location = new System.Drawing.Point(12, 470);
            this.btnCalculate.Size = new System.Drawing.Size(100, 30);
            this.btnCalculate.Text = "Calculate";
            this.btnCalculate.Click += (s, e) => CalculateCost();

            // Save Recipe Button
            this.btnSaveRecipe.Location = new System.Drawing.Point(122, 470);
            this.btnSaveRecipe.Size = new System.Drawing.Size(100, 30);
            this.btnSaveRecipe.Text = "Save Recipe";
            this.btnSaveRecipe.Click += (s, e) => SaveRecipe();

            // Delete Recipe Button
            this.btnDeleteRecipe.Location = new System.Drawing.Point(232, 470);
            this.btnDeleteRecipe.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteRecipe.Text = "Delete Recipe";
            this.btnDeleteRecipe.Click += (s, e) => DeleteRecipe();

            // Version History Button
            this.btnVersionHistory.Location = new System.Drawing.Point(342, 470);
            this.btnVersionHistory.Size = new System.Drawing.Size(120, 30);
            this.btnVersionHistory.Text = "Version History";
            this.btnVersionHistory.Click += (s, e) => ShowVersionHistory();
            this.btnVersionHistory.Enabled = false; // Disabled until a recipe is loaded

            // Close Button
            this.btnClose.Location = new System.Drawing.Point(688, 470);
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            // Add all controls
            this.Controls.AddRange(new Control[] {
                lblRecipeName, txtRecipeName, lblBatchYield, txtBatchYield,
                lblFoodCost, cmbFoodCost, lblExistingRecipes, cmbExistingRecipes,
                btnLoadRecipe, btnRefreshRecipes, txtSearchRecipes, btnSearchRecipes, btnClearSearch,
                cmbIngredients, lblUnitDisplay, txtQuantity, btnAddIngredient, 
                btnRemoveIngredient, dataGridViewIngredients, 
                lblCategory, cmbCategory, txtNewCategory, lblTags, txtTags, btnManageCategories,
                lblCostSummary, btnCalculate, btnSaveRecipe, btnDeleteRecipe, btnVersionHistory, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
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

        // ========== END VERSIONING INTEGRATION ==========

        private void CheckRecipeNameAvailability()
        {
            string currentName = txtRecipeName.Text.Trim();
            
            if (string.IsNullOrEmpty(currentName))
                return;
                
            // Check if the name already exists (excluding current recipe)
            var exists = allRecipes.Any(r => 
                r.Name.Equals(currentName, StringComparison.OrdinalIgnoreCase) && 
                r.Id != currentRecipe.Id);
            
            if (exists)
            {
                txtRecipeName.ForeColor = Color.Red;
            }
            else
            {
                txtRecipeName.ForeColor = SystemColors.WindowText;
            }
        }

        // Update the LoadSelectedRecipe method to enable the Version History button
        private void LoadSelectedRecipe()
        {
            try
            {
                if (cmbExistingRecipes.SelectedItem is Recipe selectedRecipe)
                {
                    currentRecipe = selectedRecipe;
                    
                    // Load recipe ingredients from database
                    currentIngredients = DatabaseContext.GetRecipeIngredients(currentRecipe.Id);

                    txtRecipeName.Text = currentRecipe.Name ?? "Unnamed Recipe";
                    txtBatchYield.Text = currentRecipe.BatchYield.ToString();
                    
                    // Load category
                    cmbCategory.Text = currentRecipe.Category ?? "";
                    
                    // Load tags - convert from comma-separated string to display format
                    if (!string.IsNullOrEmpty(currentRecipe.Tags))
                    {
                        txtTags.Text = currentRecipe.Tags.Replace(",", ", ");
                    }
                    else
                    {
                        txtTags.Text = "";
                    }
                    
                    // Validate and fix food cost percentage if needed
                    if (currentRecipe.TargetFoodCostPercentage <= 0 || currentRecipe.TargetFoodCostPercentage > 1)
                    {
                        currentRecipe.TargetFoodCostPercentage = 0.30m; // Default to 30%
                    }
                    
                    var foodCostPercent = (currentRecipe.TargetFoodCostPercentage * 100).ToString("0");
                    var matchingItem = cmbFoodCost.Items.Cast<string>()
                        .FirstOrDefault(item => item.Replace("%", "") == foodCostPercent);
                    
                    if (matchingItem != null)
                        cmbFoodCost.SelectedItem = matchingItem;
                    else
                        cmbFoodCost.SelectedIndex = 1; // Default to 30%
                    
                    RefreshIngredientsGrid();
                    CalculateCost();
                    UpdateRecipeCountDisplay();
                    
                    // Enable version history button for loaded recipes
                    btnVersionHistory.Enabled = true;
                    
                    // Update duplicate checking
                    CheckRecipeNameAvailability();
                    
                    // Show category and tags in status
                    var categoryInfo = string.IsNullOrEmpty(currentRecipe.Category) ? "" : $" | Category: {currentRecipe.Category}";
                    var tagsInfo = !string.IsNullOrEmpty(currentRecipe.Tags) ? $" | Tags: {currentRecipe.Tags}" : "";
                    
                    MessageBox.Show($"Loaded recipe: {currentRecipe.Name}{categoryInfo}{tagsInfo}\nIngredients loaded: {currentIngredients.Count}", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Please select a valid recipe to load.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recipe: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update the InitializeNewRecipe method to disable the Version History button
        private void InitializeNewRecipe()
        {
            currentRecipe = new Recipe { 
                Name = "New Recipe", 
                Category = "",
                Tags = "",
                BatchYield = 1, 
                TargetFoodCostPercentage = 0.3m 
            };
            currentIngredients.Clear();
            cmbCategory.Text = "";
            txtTags.Text = "";
            RefreshIngredientsGrid();
            UpdateRecipeCountDisplay();
            btnVersionHistory.Enabled = false; // Disable version history button for new recipes
            CheckRecipeNameAvailability();
        }

        private void SaveRecipe()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtRecipeName.Text))
                {
                    MessageBox.Show("Please enter a recipe name.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtRecipeName.Focus();
                    return;
                }

                if (currentIngredients.Count == 0)
                {
                    MessageBox.Show("Please add at least one ingredient to the recipe.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // === STRICT DUPLICATE CHECK ===
                string newRecipeName = txtRecipeName.Text.Trim();
                
                // Check for duplicates (exclude current recipe from the check)
                var duplicateRecipe = allRecipes.FirstOrDefault(r => 
                    r.Name.Equals(newRecipeName, StringComparison.OrdinalIgnoreCase) && 
                    r.Id != currentRecipe.Id);
                
                if (duplicateRecipe != null)
                {
                    MessageBox.Show($"A recipe named '{newRecipeName}' already exists.\n\nPlease choose a different name.", 
                        "Duplicate Recipe Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtRecipeName.Focus();
                    txtRecipeName.SelectAll();
                    return;
                }
                // === END STRICT DUPLICATE CHECK ===

                // Update current recipe with form data
                currentRecipe.Name = newRecipeName;
                currentRecipe.Category = cmbCategory.Text.Trim();
                UpdateRecipeTags();

                // Update batch yield and food cost from form
                if (int.TryParse(txtBatchYield.Text, out int batchYield))
                    currentRecipe.BatchYield = batchYield;

                if (cmbFoodCost.SelectedItem != null)
                {
                    var percent = cmbFoodCost.SelectedItem.ToString().Replace("%", "");
                    if (decimal.TryParse(percent, out decimal foodCost))
                        currentRecipe.TargetFoodCostPercentage = foodCost / 100m;
                }

                bool shouldSave = true;
                bool isNewRecipe = currentRecipe.Id == 0;

                // Show save options dialog
                var saveResult = MessageBox.Show(
                    $"Recipe: {newRecipeName}\n\n" +
                    "How would you like to save?\n\n" +
                    "• Yes: Save/Update this recipe\n" +
                    "• No: Save as a copy with different name\n" +
                    "• Cancel: Go back to editing",
                    "Save Recipe",
                    MessageBoxButtons.YesNoCancel, 
                    MessageBoxIcon.Question);

                if (saveResult == DialogResult.Yes)
                {
                    // Save/Update the recipe
                    if (currentRecipe.Id == 0)
                    {
                        isNewRecipe = true;
                    }
                    else
                    {
                        isNewRecipe = false;
                    }
                }
                else if (saveResult == DialogResult.No)
                {
                    // Save as copy - create new recipe (clone)
                    currentRecipe.Id = 0;
                    isNewRecipe = true;
                    
                    // Always prompt for new name when cloning
                    string newName = ShowSaveAsDialog(newRecipeName);
                    if (!string.IsNullOrEmpty(newName))
                    {
                        // Check if the new name is also a duplicate
                        var newNameDuplicate = allRecipes.Any(r => 
                            r.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));
                        
                        if (newNameDuplicate)
                        {
                            MessageBox.Show($"A recipe named '{newName}' already exists.\n\nPlease choose a different name.", 
                                "Duplicate Recipe Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        
                        currentRecipe.Name = newName;
                        txtRecipeName.Text = currentRecipe.Name;
                    }
                    else
                    {
                        shouldSave = false; // User cancelled
                    }
                }
                else
                {
                    shouldSave = false; // User cancelled
                }

                if (shouldSave)
                {
                    if (isNewRecipe)
                    {
                        // Insert new recipe
                        DatabaseContext.InsertRecipe(currentRecipe);
                        
                        // Get the newly created recipe ID
                        var allRecipesAfterInsert = DatabaseContext.GetAllRecipes();
                        var newRecipe = allRecipesAfterInsert
                            .FirstOrDefault(r => r.Name.Equals(currentRecipe.Name, StringComparison.OrdinalIgnoreCase));
                        
                        if (newRecipe != null)
                        {
                            currentRecipe.Id = newRecipe.Id;
                            
                            // Create initial version for new recipes
                            CreateInitialVersion();
                        }
                    }
                    else
                    {
                        // Update existing recipe
                        DatabaseContext.UpdateRecipe(currentRecipe);
                        
                        // Create version for recipe updates
                        CreateVersionForChange("Recipe updated");
                    }

                    // Save ingredients
                    SaveRecipeIngredients();

                    string message = isNewRecipe ? 
                        $"New recipe '{currentRecipe.Name}' saved successfully!" : 
                        $"Recipe '{currentRecipe.Name}' updated successfully!";
                        
                    MessageBox.Show(message, "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh the recipes list and trigger update event
                    LoadExistingRecipes();
                    RecipesUpdated?.Invoke();
                    UpdateRecipeCountDisplay();
                    btnVersionHistory.Enabled = true; // Enable version history button after saving
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving recipe: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureIngredientsGrid()
        {
            try
            {
                // Clear existing columns
                dataGridViewIngredients.Columns.Clear();
                
                // Add columns manually for better control
                var columns = new[]
                {
                    new DataGridViewTextBoxColumn { Name = "IngredientName", HeaderText = "Ingredient", DataPropertyName = "IngredientName", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                    new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Quantity", DataPropertyName = "Quantity", ReadOnly = false },
                    new DataGridViewTextBoxColumn { Name = "Unit", HeaderText = "Unit", DataPropertyName = "Unit", ReadOnly = true },
                    new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "Unit Price", DataPropertyName = "UnitPrice", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = $"{currencySymbol}0.0000" } },
                    new DataGridViewTextBoxColumn { Name = "LineCost", HeaderText = "Line Cost", DataPropertyName = "LineCost", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = $"{currencySymbol}0.00" } },
                    new DataGridViewTextBoxColumn { Name = "Supplier", HeaderText = "Supplier", DataPropertyName = "Supplier", ReadOnly = true }
                };
                
                dataGridViewIngredients.Columns.AddRange(columns);
                
                // Hide ID columns - they'll be in the data source but not visible
                dataGridViewIngredients.AutoGenerateColumns = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error configuring ingredients grid: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateUnitDisplay()
        {
            try
            {
                if (cmbIngredients.SelectedItem is Ingredient selectedIngredient)
                {
                    lblUnitDisplay.Text = selectedIngredient.Unit ?? "grams";
                }
                else
                {
                    lblUnitDisplay.Text = "grams"; // Default unit
                }
            }
            catch (Exception ex)
            {
                lblUnitDisplay.Text = "grams";
            }
        }

        private void dataGridViewIngredients_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == GetColumnIndex("Quantity") && e.RowIndex >= 0)
            {
                UpdateIngredientQuantity(e.RowIndex);
            }
        }

        private int GetColumnIndex(string columnName)
        {
            foreach (DataGridViewColumn column in dataGridViewIngredients.Columns)
            {
                if (column.HeaderText == columnName)
                    return column.Index;
            }
            return -1;
        }

        private void UpdateIngredientQuantity(int rowIndex)
        {
            if (rowIndex < currentIngredients.Count)
            {
                var row = dataGridViewIngredients.Rows[rowIndex];
                var ingredient = currentIngredients[rowIndex];
                
                if (row.Cells["Quantity"].Value != null && 
                    decimal.TryParse(row.Cells["Quantity"].Value.ToString(), out decimal newQuantity))
                {
                    var oldQuantity = ingredient.Quantity;
                    ingredient.Quantity = newQuantity;
                    
                    // Create version when quantity changes significantly
                    if (currentRecipe.Id > 0 && Math.Abs(oldQuantity - newQuantity) > 0.1m)
                    {
                        CreateVersionForChange($"Changed {ingredient.IngredientName} quantity from {oldQuantity} to {newQuantity} {ingredient.Unit}");
                    }
                    
                    this.BeginInvoke(new Action(() =>
                    {
                        RefreshIngredientsGrid();
                        CalculateCost();
                    }));
                }
            }
        }

        private void LoadExistingRecipes()
        {
            try
            {
                allRecipes = DatabaseContext.GetAllRecipes();
                
                cmbExistingRecipes.DataSource = null;
                cmbExistingRecipes.Items.Clear();
                
                if (allRecipes != null && allRecipes.Count > 0)
                {
                    var displayRecipes = allRecipes
                        .Where(r => !string.IsNullOrEmpty(r.Name))
                        .OrderBy(r => r.Name)
                        .ToList();
                    
                    cmbExistingRecipes.DataSource = displayRecipes;
                    cmbExistingRecipes.DisplayMember = "Name";
                    cmbExistingRecipes.ValueMember = "Id";
                    
                    if (cmbExistingRecipes.SelectedIndex == -1 && cmbExistingRecipes.Items.Count > 0)
                    {
                        cmbExistingRecipes.SelectedIndex = 0;
                    }
                }
                else
                {
                    cmbExistingRecipes.Text = "No recipes available";
                }
                
                UpdateRecipeCountDisplay();
                
                // Refresh duplicate checking after loading recipes
                CheckRecipeNameAvailability();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recipes: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbExistingRecipes.Text = "Error loading recipes";
            }
        }

        private void UpdateRecipeCountDisplay()
        {
            int totalRecipes = allRecipes?.Count ?? 0;
            int currentIngredientCount = currentIngredients?.Count ?? 0;
            
            this.Text = $"Recipe Costing Calculator - {totalRecipes} recipes, {currentIngredientCount} ingredients in current recipe";
        }

        private void ClearSearch()
        {
            txtSearchRecipes.Text = "";
            LoadExistingRecipes();
            lblExistingRecipes.Text = "Load Recipe:";
        }

        private void LoadCategories()
        {
            try
            {
                // Clear existing items
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add(""); // Empty option
                
                // Add common categories first (these should always be available)
                var commonCategories = new List<string>
                {
                    "Main Course", "Appetizer", "Dessert", "Side Dish", 
                    "Breakfast", "Lunch", "Dinner", "Beverage",
                    "Soup", "Salad", "Snack", "Sauce"
                };
                
                foreach (var category in commonCategories)
                {
                    if (!cmbCategory.Items.Contains(category))
                        cmbCategory.Items.Add(category);
                }
                
                // Then add any additional categories from the database
                var recipeCategories = DatabaseContext.GetRecipeCategories();
                foreach (var category in recipeCategories)
                {
                    if (!string.IsNullOrEmpty(category) && !cmbCategory.Items.Contains(category))
                        cmbCategory.Items.Add(category);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateRecipeTags()
        {
            if (string.IsNullOrEmpty(txtTags.Text))
            {
                currentRecipe.Tags = "";
                return;
            }

            var tags = txtTags.Text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
            
            currentRecipe.Tags = string.Join(",", tags);
        }

        private void ShowCategoryManager()
        {
            using (var form = new Form())
            {
                form.Text = "Manage Categories";
                form.Size = new System.Drawing.Size(300, 400);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;

                var lstCategories = new ListBox();
                lstCategories.Location = new System.Drawing.Point(20, 20);
                lstCategories.Size = new System.Drawing.Size(240, 200);
                
                var txtNewCategory = new TextBox();
                txtNewCategory.Location = new System.Drawing.Point(20, 240);
                txtNewCategory.Size = new System.Drawing.Size(240, 20);
                txtNewCategory.PlaceholderText = "New category name...";
                
                var btnAdd = new Button();
                btnAdd.Location = new System.Drawing.Point(20, 270);
                btnAdd.Size = new System.Drawing.Size(75, 30);
                btnAdd.Text = "Add";
                
                var btnDelete = new Button();
                btnDelete.Location = new System.Drawing.Point(105, 270);
                btnDelete.Size = new System.Drawing.Size(75, 30);
                btnDelete.Text = "Delete";
                
                var btnClose = new Button();
                btnClose.Location = new System.Drawing.Point(185, 270);
                btnClose.Size = new System.Drawing.Size(75, 30);
                btnClose.Text = "Close";
                btnClose.DialogResult = DialogResult.OK;

                // Define default categories that cannot be deleted
                var defaultCategories = new List<string>
                {
                    "Main Course", "Appetizer", "Dessert", "Side Dish", 
                    "Breakfast", "Lunch", "Dinner", "Beverage",
                    "Soup", "Salad", "Snack", "Sauce"
                };

                // Load all categories (default + custom from database)
                try
                {
                    var allCategories = new List<string>();
                    
                    // Add default categories
                    allCategories.AddRange(defaultCategories);
                    
                    // Add custom categories from database
                    var customCategories = DatabaseContext.GetRecipeCategories();
                    foreach (var category in customCategories)
                    {
                        if (!string.IsNullOrEmpty(category) && !allCategories.Contains(category))
                            allCategories.Add(category);
                    }
                    
                    lstCategories.Items.Clear();
                    foreach (var category in allCategories.OrderBy(c => c))
                    {
                        lstCategories.Items.Add(category);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading categories: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                btnAdd.Click += (s, e) =>
                {
                    var newCategory = txtNewCategory.Text.Trim();
                    if (!string.IsNullOrEmpty(newCategory))
                    {
                        // Check if category already exists (case-insensitive)
                        if (!lstCategories.Items.Cast<string>().Any(c => 
                            c.Equals(newCategory, StringComparison.OrdinalIgnoreCase)))
                        {
                            lstCategories.Items.Add(newCategory);
                            txtNewCategory.Text = "";
                            
                            // Also add to the main form's category list immediately
                            if (!cmbCategory.Items.Cast<string>().Any(c => 
                                c.Equals(newCategory, StringComparison.OrdinalIgnoreCase)))
                            {
                                cmbCategory.Items.Add(newCategory);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Category already exists.", "Information", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                };

                btnDelete.Click += (s, e) =>
                {
                    if (lstCategories.SelectedItem != null)
                    {
                        var selected = lstCategories.SelectedItem.ToString();
                        
                        // Check if it's a default category (cannot be deleted)
                        if (defaultCategories.Contains(selected))
                        {
                            MessageBox.Show($"Cannot delete default category '{selected}'.", "Information", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        
                        var result = MessageBox.Show($"Delete category '{selected}'?\n\nNote: This won't remove the category from existing recipes.", 
                            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        
                        if (result == DialogResult.Yes)
                        {
                            lstCategories.Items.Remove(selected);
                            
                            // Also remove from main form's category list if it exists
                            var itemToRemove = cmbCategory.Items.Cast<string>()
                                .FirstOrDefault(c => c.Equals(selected, StringComparison.OrdinalIgnoreCase));
                            if (itemToRemove != null)
                            {
                                cmbCategory.Items.Remove(itemToRemove);
                            }
                        }
                    }
                };

                form.Controls.AddRange(new Control[] {
                    lstCategories, txtNewCategory, btnAdd, btnDelete, btnClose
                });
                
                form.AcceptButton = btnClose;
                
                form.ShowDialog();
                
                // Refresh category list in main form
                LoadCategories();
            }
        }

        private void SearchRecipes()
        {
            try
            {
                var searchTerm = txtSearchRecipes.Text.Trim().ToLowerInvariant();
                
                if (string.IsNullOrEmpty(searchTerm))
                {
                    LoadExistingRecipes();
                    return;
                }

                var allRecipes = DatabaseContext.GetAllRecipes();
                var filteredRecipes = allRecipes
                    .Where(recipe =>
                        (recipe.Name ?? "").ToLowerInvariant().Contains(searchTerm) ||
                        (recipe.Category ?? "").ToLowerInvariant().Contains(searchTerm) ||
                        ((recipe.Tags ?? "").ToLowerInvariant().Contains(searchTerm))
                    )
                    .OrderBy(r => r.Name)
                    .ToList();

                cmbExistingRecipes.DataSource = null;
                cmbExistingRecipes.Items.Clear();
                
                if (filteredRecipes.Count > 0)
                {
                    cmbExistingRecipes.DataSource = filteredRecipes;
                    cmbExistingRecipes.DisplayMember = "Name";
                    cmbExistingRecipes.ValueMember = "Id";
                    
                    lblExistingRecipes.Text = $"Search Results ({filteredRecipes.Count} recipes):";
                }
                else
                {
                    cmbExistingRecipes.Text = "No recipes found";
                    lblExistingRecipes.Text = "Search Results (0 recipes):";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching recipes: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadIngredientsComboBox()
        {
            try
            {
                var ingredients = DatabaseContext.GetAllIngredients();
                if (ingredients != null && ingredients.Count > 0)
                {
                    cmbIngredients.DataSource = ingredients;
                    cmbIngredients.DisplayMember = "Name";
                    cmbIngredients.ValueMember = "Id";
                    
                    // Set initial unit display
                    UpdateUnitDisplay();
                }
                else
                {
                    cmbIngredients.Text = "No ingredients available";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddIngredientToRecipe()
        {
            if (cmbIngredients.SelectedItem is Ingredient selectedIngredient && 
                decimal.TryParse(txtQuantity.Text, out decimal quantity))
            {
                var recipeIngredient = new RecipeIngredient
                {
                    IngredientId = selectedIngredient.Id,
                    Quantity = quantity,
                    IngredientName = selectedIngredient.Name,
                    Unit = selectedIngredient.Unit,
                    UnitPrice = selectedIngredient.UnitPrice,
                    Supplier = selectedIngredient.SupplierName
                };

                currentIngredients.Add(recipeIngredient);
                RefreshIngredientsGrid();
                CalculateCost();
                UpdateRecipeCountDisplay();
                
                // Create version when ingredient is added
                if (currentRecipe.Id > 0)
                {
                    CreateVersionForChange($"Added {quantity} {selectedIngredient.Unit} of {selectedIngredient.Name}");
                }
                
                txtQuantity.Text = "100";
                UpdateUnitDisplay();
            }
            else
            {
                MessageBox.Show("Please enter a valid quantity.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveIngredient()
        {
            if (dataGridViewIngredients.SelectedRows.Count > 0)
            {
                var selected = (RecipeIngredient)dataGridViewIngredients.SelectedRows[0].DataBoundItem;
                currentIngredients.Remove(selected);
                RefreshIngredientsGrid();
                CalculateCost();
                UpdateRecipeCountDisplay();
                
                // Create version when ingredient is removed
                if (currentRecipe.Id > 0)
                {
                    CreateVersionForChange($"Removed {selected.IngredientName}");
                }
            }
        }

        private void RefreshIngredientsGrid()
        {
            try
            {
                dataGridViewIngredients.SuspendLayout();
                dataGridViewIngredients.DataSource = null;
                dataGridViewIngredients.DataSource = currentIngredients.ToList();
                
                // Auto-size columns for better display
                dataGridViewIngredients.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                dataGridViewIngredients.ResumeLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing ingredients grid: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveRecipeIngredients()
        {
            try
            {
                if (currentRecipe.Id <= 0)
                {
                    throw new InvalidOperationException("Recipe ID is not valid. Cannot save ingredients.");
                }

                // Delete ALL existing ingredients for this recipe first
                var existingIngredients = DatabaseContext.GetRecipeIngredients(currentRecipe.Id);
                foreach (var existing in existingIngredients)
                {
                    DatabaseContext.DeleteRecipeIngredient(existing.Id);
                }

                // Now save all current ingredients with the correct RecipeId
                foreach (var recipeIngredient in currentIngredients)
                {
                    // Use the DatabaseContext method to add the ingredient
                    DatabaseContext.AddRecipeIngredient(new RecipeIngredient
                    {
                        RecipeId = currentRecipe.Id,
                        IngredientId = recipeIngredient.IngredientId,
                        Quantity = recipeIngredient.Quantity
                    });
                }

                // Refresh the current ingredients from database to get proper IDs
                currentIngredients = DatabaseContext.GetRecipeIngredients(currentRecipe.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving recipe ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void DeleteRecipe()
        {
            try
            {
                if (currentRecipe.Id == 0)
                {
                    MessageBox.Show("No recipe loaded to delete.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var result = MessageBox.Show($"Are you sure you want to delete recipe '{currentRecipe.Name}'? This action cannot be undone.", 
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    DatabaseContext.DeleteRecipe(currentRecipe.Id);
                    MessageBox.Show("Recipe deleted successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Refresh and reset form
                    LoadExistingRecipes();
                    InitializeNewRecipe();
                    RecipesUpdated?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting recipe: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateCost()
        {
            if (currentIngredients.Count == 0)
            {
                lblCostSummary.Text = "Add ingredients to calculate cost...";
                return;
            }

            decimal totalCost = currentIngredients.Sum(i => i.LineCost);
            decimal costPerServing = currentRecipe.BatchYield > 0 ? totalCost / currentRecipe.BatchYield : totalCost;

            // Calculate suggested prices
            decimal suggestedPrice25 = costPerServing > 0 ? Math.Round((costPerServing / 0.25m) / 5, 0) * 5 : 0;
            decimal suggestedPrice30 = costPerServing > 0 ? Math.Round((costPerServing / 0.30m) / 5, 0) * 5 : 0;
            decimal suggestedPrice35 = costPerServing > 0 ? Math.Round((costPerServing / 0.35m) / 5, 0) * 5 : 0;
            
            // Ensure target food cost percentage is valid and calculate target price
            decimal targetPrice = 0;
            decimal targetFoodCostPercentage = currentRecipe.TargetFoodCostPercentage;
            
            // Validate and fix the target food cost percentage if needed
            if (targetFoodCostPercentage <= 0 || targetFoodCostPercentage > 1)
            {
                // Default to 30% if invalid
                targetFoodCostPercentage = 0.30m;
                currentRecipe.TargetFoodCostPercentage = targetFoodCostPercentage;
            }
            
            if (costPerServing > 0 && targetFoodCostPercentage > 0)
            {
                targetPrice = Math.Round((costPerServing / targetFoodCostPercentage) / 5, 0) * 5;
            }

            // Format the percentage correctly (0.30 becomes 30%, not 3000%)
            string foodCostPercentDisplay = (targetFoodCostPercentage * 100).ToString("0") + "%";

            // Using regular string concatenation to avoid any verbatim string issues
            string summary = "Recipe: " + currentRecipe.Name + Environment.NewLine +
                            "Total Cost: " + currencySymbol + totalCost.ToString("F2") + " | Cost per Serving: " + currencySymbol + costPerServing.ToString("F2") + Environment.NewLine +
                            "Suggested Prices: 25%: " + currencySymbol + suggestedPrice25 + " | 30%: " + currencySymbol + suggestedPrice30 + " | 35%: " + currencySymbol + suggestedPrice35 + Environment.NewLine +
                            "Target Price (" + foodCostPercentDisplay + "): " + currencySymbol + targetPrice;

            lblCostSummary.Text = summary;
        }

        private string ShowSaveAsDialog(string currentName)
        {
            using (var form = new Form())
            {
                form.Text = "Save Recipe As Copy";
                form.Size = new System.Drawing.Size(400, 200);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;

                var lblPrompt = new Label();
                lblPrompt.Location = new System.Drawing.Point(20, 20);
                lblPrompt.Size = new System.Drawing.Size(350, 40);
                lblPrompt.Text = "Enter a name for the recipe copy:";
                
                var txtNewName = new TextBox();
                txtNewName.Location = new System.Drawing.Point(20, 60);
                txtNewName.Size = new System.Drawing.Size(350, 20);
                txtNewName.Text = currentName + " Copy";
                txtNewName.SelectAll();
                
                var btnOK = new Button();
                btnOK.Location = new System.Drawing.Point(200, 100);
                btnOK.Size = new System.Drawing.Size(80, 30);
                btnOK.Text = "OK";
                btnOK.DialogResult = DialogResult.OK;
                
                var btnCancel = new Button();
                btnCancel.Location = new System.Drawing.Point(290, 100);
                btnCancel.Size = new System.Drawing.Size(80, 30);
                btnCancel.Text = "Cancel";
                btnCancel.DialogResult = DialogResult.Cancel;

                form.Controls.AddRange(new Control[] {
                    lblPrompt, txtNewName, btnOK, btnCancel
                });

                form.AcceptButton = btnOK;
                form.CancelButton = btnCancel;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    var newName = txtNewName.Text.Trim();
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        MessageBox.Show("Please enter a valid recipe name.", "Validation Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return ShowSaveAsDialog(currentName);
                    }
                    return newName;
                }
                
                return null; // User cancelled
            }
        }
    }
}