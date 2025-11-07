using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

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

        // NEW: Category and tags controls
        private Label lblCategory;
        private ComboBox cmbCategory;
        private TextBox txtNewCategory;
        private Label lblTags;
        private TextBox txtTags;
        private Button btnManageCategories;

        private Recipe currentRecipe = new Recipe();
        private List<RecipeIngredient> currentIngredients = new List<RecipeIngredient>();
        private List<Recipe> allRecipes = new List<Recipe>();
        
        // Currency symbol
        private string currencySymbol = AppSettings.CurrencySymbol;

        // Add this event for recipe updates
        public event Action RecipesUpdated;

        public RecipesForm()
        {
            InitializeComponent();
            LoadIngredientsComboBox();
            LoadExistingRecipes();
            InitializeNewRecipe();
            LoadCategories(); // NEW
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

            // NEW: Category and Tags Controls
            this.lblCategory = new Label();
            this.cmbCategory = new ComboBox();
            this.txtNewCategory = new TextBox();
            this.lblTags = new Label();
            this.txtTags = new TextBox();
            this.btnManageCategories = new Button();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Recipe Costing Calculator";
            this.StartPosition = FormStartPosition.CenterParent;

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
            this.txtRecipeName.TextChanged += (s, e) => currentRecipe.Name = txtRecipeName.Text;

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
                    currentRecipe.BatchYield = yield;
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
                        currentRecipe.TargetFoodCostPercentage = foodCost / 100m;
                }
            };

            // Ingredients ComboBox
            this.cmbIngredients.Location = new System.Drawing.Point(12, 75);
            this.cmbIngredients.Size = new System.Drawing.Size(250, 20);
            this.cmbIngredients.DropDownStyle = ComboBoxStyle.DropDownList;

            // Quantity
            this.txtQuantity.Location = new System.Drawing.Point(270, 75);
            this.txtQuantity.Size = new System.Drawing.Size(80, 20);
            this.txtQuantity.Text = "100";
            this.txtQuantity.PlaceholderText = "Quantity";

            // Add Ingredient Button
            this.btnAddIngredient.Location = new System.Drawing.Point(360, 73);
            this.btnAddIngredient.Size = new System.Drawing.Size(80, 25);
            this.btnAddIngredient.Text = "Add";
            this.btnAddIngredient.Click += (s, e) => AddIngredientToRecipe();

            // Remove Ingredient Button
            this.btnRemoveIngredient.Location = new System.Drawing.Point(450, 73);
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

            // NEW: Category Label
            this.lblCategory.Location = new System.Drawing.Point(12, 320);
            this.lblCategory.Size = new System.Drawing.Size(100, 20);
            this.lblCategory.Text = "Category:";
            
            // NEW: Category ComboBox
            this.cmbCategory.Location = new System.Drawing.Point(120, 317);
            this.cmbCategory.Size = new System.Drawing.Size(150, 20);
            this.cmbCategory.DropDownStyle = ComboBoxStyle.DropDown;
            this.cmbCategory.TextChanged += (s, e) => currentRecipe.Category = cmbCategory.Text;

            // NEW: New Category TextBox (for adding new categories)
            this.txtNewCategory.Location = new System.Drawing.Point(280, 317);
            this.txtNewCategory.Size = new System.Drawing.Size(120, 20);
            this.txtNewCategory.PlaceholderText = "New category...";
            this.txtNewCategory.Visible = false;

            // NEW: Manage Categories Button
            this.btnManageCategories.Location = new System.Drawing.Point(410, 315);
            this.btnManageCategories.Size = new System.Drawing.Size(120, 25);
            this.btnManageCategories.Text = "Manage Categories";
            this.btnManageCategories.Click += (s, e) => ShowCategoryManager();

            // NEW: Tags Label
            this.lblTags.Location = new System.Drawing.Point(12, 350);
            this.lblTags.Size = new System.Drawing.Size(100, 20);
            this.lblTags.Text = "Tags:";
            
            // NEW: Tags TextBox
            this.txtTags.Location = new System.Drawing.Point(120, 347);
            this.txtTags.Size = new System.Drawing.Size(300, 20);
            this.txtTags.PlaceholderText = "comma, separated, tags";
            this.txtTags.TextChanged += (s, e) => UpdateRecipeTags();

            // Cost Summary - Adjusted position to make room for new controls
            this.lblCostSummary.Location = new System.Drawing.Point(12, 380);
            this.lblCostSummary.Size = new System.Drawing.Size(776, 80);
            this.lblCostSummary.Text = "Add ingredients to calculate cost...";
            this.lblCostSummary.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblCostSummary.BorderStyle = BorderStyle.FixedSingle;
            this.lblCostSummary.Font = new System.Drawing.Font("Consolas", 9);

            // Calculate Button - Adjusted position
            this.btnCalculate.Location = new System.Drawing.Point(12, 470);
            this.btnCalculate.Size = new System.Drawing.Size(100, 30);
            this.btnCalculate.Text = "Calculate";
            this.btnCalculate.Click += (s, e) => CalculateCost();

            // Save Recipe Button - Adjusted position
            this.btnSaveRecipe.Location = new System.Drawing.Point(122, 470);
            this.btnSaveRecipe.Size = new System.Drawing.Size(100, 30);
            this.btnSaveRecipe.Text = "Save Recipe";
            this.btnSaveRecipe.Click += (s, e) => SaveRecipe();

            // Delete Recipe Button - Adjusted position
            this.btnDeleteRecipe.Location = new System.Drawing.Point(232, 470);
            this.btnDeleteRecipe.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteRecipe.Text = "Delete Recipe";
            this.btnDeleteRecipe.Click += (s, e) => DeleteRecipe();

            // Close Button - Adjusted position
            this.btnClose.Location = new System.Drawing.Point(688, 470);
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            // Add all controls
            this.Controls.AddRange(new Control[] {
                lblRecipeName, txtRecipeName, lblBatchYield, txtBatchYield,
                lblFoodCost, cmbFoodCost, lblExistingRecipes, cmbExistingRecipes,
                btnLoadRecipe, btnRefreshRecipes, txtSearchRecipes, btnSearchRecipes, btnClearSearch,
                cmbIngredients, txtQuantity, btnAddIngredient, 
                btnRemoveIngredient, dataGridViewIngredients, 
                lblCategory, cmbCategory, txtNewCategory, lblTags, txtTags, btnManageCategories, // NEW controls
                lblCostSummary, btnCalculate, btnSaveRecipe, btnDeleteRecipe, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
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
                    ingredient.Quantity = newQuantity;
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

        // ENHANCED: Search recipes by name, description, ingredients, categories, or tags
        private void SearchRecipes()
        {
            try
            {
                var searchTerm = txtSearchRecipes.Text.Trim().ToLower();
                
                if (string.IsNullOrEmpty(searchTerm))
                {
                    LoadExistingRecipes();
                    return;
                }

                var allRecipes = DatabaseContext.GetAllRecipes();
                var filteredRecipes = allRecipes
                    .Where(recipe =>
                        recipe.Name.ToLower().Contains(searchTerm) ||
                        recipe.Description.ToLower().Contains(searchTerm) ||
                        recipe.Category.ToLower().Contains(searchTerm) ||
                        (recipe.Tags != null && recipe.Tags.Any(tag => 
                            tag.ToLower().Contains(searchTerm))) ||
                        (recipe.Ingredients != null && recipe.Ingredients.Any(i => 
                            i.IngredientName.ToLower().Contains(searchTerm)))
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

        private void ClearSearch()
        {
            txtSearchRecipes.Text = "";
            LoadExistingRecipes();
            lblExistingRecipes.Text = "Load Recipe:";
        }

        // NEW: Load categories into combo box
        private void LoadCategories()
        {
            try
            {
                var categories = DatabaseContext.GetRecipeCategories();
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add(""); // Empty option
                
                // Add common categories
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
                
                // Add existing categories from database
                foreach (var category in categories)
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

        // NEW: Update recipe tags from textbox
        private void UpdateRecipeTags()
        {
            var tags = txtTags.Text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
            
            currentRecipe.Tags = tags;
        }

        // NEW: Category management form
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

                // Load existing categories
                try
                {
                    var categories = DatabaseContext.GetRecipeCategories();
                    lstCategories.Items.Clear();
                    foreach (var category in categories)
                    {
                        if (!string.IsNullOrEmpty(category))
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
                        if (!lstCategories.Items.Contains(newCategory))
                        {
                            lstCategories.Items.Add(newCategory);
                            txtNewCategory.Text = "";
                            
                            // Update the main form's category list
                            if (!cmbCategory.Items.Contains(newCategory))
                                cmbCategory.Items.Add(newCategory);
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
                        var result = MessageBox.Show($"Delete category '{selected}'?\n\nNote: This won't remove the category from existing recipes.", 
                            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        
                        if (result == DialogResult.Yes)
                        {
                            lstCategories.Items.Remove(selected);
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

        // UPDATED: Load selected recipe with categories and tags
        private void LoadSelectedRecipe()
        {
            try
            {
                if (cmbExistingRecipes.SelectedItem is Recipe selectedRecipe)
                {
                    currentRecipe = selectedRecipe;
                    currentIngredients = selectedRecipe.Ingredients ?? new List<RecipeIngredient>();

                    txtRecipeName.Text = currentRecipe.Name ?? "Unnamed Recipe";
                    txtBatchYield.Text = currentRecipe.BatchYield.ToString();
                    
                    // Load category
                    cmbCategory.Text = currentRecipe.Category ?? "";
                    
                    // Load tags
                    txtTags.Text = string.Join(", ", currentRecipe.Tags ?? new List<string>());
                    
                    var foodCostPercent = (currentRecipe.TargetFoodCostPercentage * 100).ToString("0");
                    var matchingItem = cmbFoodCost.Items.Cast<string>()
                        .FirstOrDefault(item => item.Replace("%", "") == foodCostPercent);
                    
                    if (matchingItem != null)
                        cmbFoodCost.SelectedItem = matchingItem;
                    
                    RefreshIngredientsGrid();
                    CalculateCost();
                    UpdateRecipeCountDisplay();
                    
                    // Show category and tags in status
                    var categoryInfo = string.IsNullOrEmpty(currentRecipe.Category) ? "" : $" | Category: {currentRecipe.Category}";
                    var tagsInfo = currentRecipe.Tags.Count > 0 ? $" | Tags: {string.Join(", ", currentRecipe.Tags)}" : "";
                    
                    MessageBox.Show($"Loaded recipe: {currentRecipe.Name}{categoryInfo}{tagsInfo}", "Success", 
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

        // UPDATED: Initialize new recipe with empty category and tags
        private void InitializeNewRecipe()
        {
            currentRecipe = new Recipe { 
                Name = "New Recipe", 
                Category = "",
                Tags = new List<string>(),
                BatchYield = 1, 
                TargetFoodCostPercentage = 0.3m 
            };
            currentIngredients.Clear();
            cmbCategory.Text = "";
            txtTags.Text = "";
            RefreshIngredientsGrid();
            UpdateRecipeCountDisplay();
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
                    UnitPrice = selectedIngredient.UnitPrice
                };

                currentIngredients.Add(recipeIngredient);
                RefreshIngredientsGrid();
                CalculateCost();
                UpdateRecipeCountDisplay();
                
                txtQuantity.Text = "100";
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
            }
        }

        private void RefreshIngredientsGrid()
        {
            try
            {
                dataGridViewIngredients.SuspendLayout();
                dataGridViewIngredients.DataSource = null;
                dataGridViewIngredients.DataSource = currentIngredients.ToList();
                
                if (dataGridViewIngredients.Columns.Count > 0)
                {
                    dataGridViewIngredients.Columns["LineCost"].DefaultCellStyle.Format = $"{currencySymbol} 0.00";
                    dataGridViewIngredients.Columns["UnitPrice"].DefaultCellStyle.Format = $"{currencySymbol} 0.0000";

// Add this after the format lines:
dataGridViewIngredients.Columns["UnitPrice"].HeaderText = $"Price/Unit ({currencySymbol})";
dataGridViewIngredients.Columns["LineCost"].HeaderText = $"Line Cost ({currencySymbol})";
dataGridViewIngredients.Columns["IngredientName"].HeaderText = "Ingredient";
dataGridViewIngredients.Columns["Quantity"].HeaderText = "Qty";

                    dataGridViewIngredients.Columns["IngredientId"].Visible = false;
                    
                    foreach (DataGridViewColumn column in dataGridViewIngredients.Columns)
                    {
                        column.ReadOnly = (column.HeaderText != "Quantity");
                    }
                }
                dataGridViewIngredients.ResumeLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing ingredients grid: {ex.Message}", "Error", 
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
            decimal costPerServing = totalCost / currentRecipe.BatchYield;
            
            decimal suggestedPrice25 = Math.Round((costPerServing / 0.25m) / 5, 0) * 5;
            decimal suggestedPrice30 = Math.Round((costPerServing / 0.30m) / 5, 0) * 5;
            decimal suggestedPrice35 = Math.Round((costPerServing / 0.35m) / 5, 0) * 5;
            decimal targetPrice = Math.Round((costPerServing / currentRecipe.TargetFoodCostPercentage) / 5, 0) * 5;

            string summary = $@"Recipe: {currentRecipe.Name}
Total Cost: {currencySymbol} {totalCost:F2} | Cost per Serving: {currencySymbol} {costPerServing:F2}
Suggested Prices: 25%: {currencySymbol} {suggestedPrice25} | 30%: {currencySymbol} {suggestedPrice30} | 35%: {currencySymbol} {suggestedPrice35}
Target Price ({currentRecipe.TargetFoodCostPercentage:P0}): {currencySymbol} {targetPrice}";

            lblCostSummary.Text = summary;
        }

        // UPDATED: Save recipe with category and tags
        private void SaveRecipe()
        {
            if (string.IsNullOrWhiteSpace(currentRecipe.Name))
            {
                MessageBox.Show("Please enter a recipe name.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            currentRecipe.Ingredients = currentIngredients ?? new List<RecipeIngredient>();

            try
            {
                bool isNewRecipe = (currentRecipe.Id == 0);
                
                if (isNewRecipe)
                {
                    currentRecipe.Id = DatabaseContext.InsertRecipe(currentRecipe);
                    MessageBox.Show($"Recipe '{currentRecipe.Name}' saved successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DatabaseContext.UpdateRecipe(currentRecipe);
                    MessageBox.Show($"Recipe '{currentRecipe.Name}' updated successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                // Add new category to available categories if it doesn't exist
                if (!string.IsNullOrEmpty(currentRecipe.Category) && 
                    !cmbCategory.Items.Contains(currentRecipe.Category))
                {
                    cmbCategory.Items.Add(currentRecipe.Category);
                }
                
                LoadExistingRecipes();
                
                if (allRecipes != null)
                {
                    var savedRecipe = allRecipes.FirstOrDefault(r => r.Id == currentRecipe.Id);
                    if (savedRecipe != null)
                    {
                        cmbExistingRecipes.SelectedItem = savedRecipe;
                    }
                }
                
                UpdateRecipeCountDisplay();
                RecipesUpdated?.Invoke();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving recipe: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteRecipe()
        {
            if (currentRecipe.Id == 0)
            {
                MessageBox.Show("No recipe loaded to delete.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete '{currentRecipe.Name}'?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    DatabaseContext.DeleteRecipe(currentRecipe.Id);
                    MessageBox.Show("Recipe deleted successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    LoadExistingRecipes();
                    InitializeNewRecipe();
                    
                    UpdateRecipeCountDisplay();
                    RecipesUpdated?.Invoke();
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting recipe: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}