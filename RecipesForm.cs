using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace CostChef
{
    public partial class RecipesForm : Form
    {
        private DataGridView dataGridViewRecipes;
        private DataGridView dataGridViewIngredients;
        private TextBox txtSearch;
        private Button btnAddRecipe;
        private Button btnEditRecipe;
        private Button btnDeleteRecipe;
        private Button btnClose;
        private Label lblCostSummary;
        private ComboBox cmbCategoryFilter;
        private TextBox txtTagsFilter;

        private string currencySymbol => AppSettings.CurrencySymbol;
        private List<Recipe> _allRecipes = new List<Recipe>();
        private Recipe _currentRecipe = null;

        public RecipesForm()
        {
            InitializeComponent();
            LoadCategories();
            LoadRecipes();
        }

        private void InitializeComponent()
        {
            this.dataGridViewRecipes = new DataGridView();
            this.dataGridViewIngredients = new DataGridView();
            this.txtSearch = new TextBox();
            this.btnAddRecipe = new Button();
            this.btnEditRecipe = new Button();
            this.btnDeleteRecipe = new Button();
            this.btnClose = new Button();
            this.lblCostSummary = new Label();
            this.cmbCategoryFilter = new ComboBox();
            this.txtTagsFilter = new TextBox();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Text = "Manage Recipes";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            // Search and Filter
            var lblSearch = new Label { Text = "Search:", Location = new System.Drawing.Point(12, 12), AutoSize = true };
            this.txtSearch.Location = new System.Drawing.Point(60, 9);
            this.txtSearch.Size = new System.Drawing.Size(150, 20);
            this.txtSearch.TextChanged += (s, e) => LoadRecipes();

            var lblCategory = new Label { Text = "Category:", Location = new System.Drawing.Point(220, 12), AutoSize = true };
            this.cmbCategoryFilter.Location = new System.Drawing.Point(280, 9);
            this.cmbCategoryFilter.Size = new System.Drawing.Size(150, 20);
            this.cmbCategoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbCategoryFilter.SelectedIndexChanged += (s, e) => LoadRecipes();

            var lblTags = new Label { Text = "Tags:", Location = new System.Drawing.Point(440, 12), AutoSize = true };
            this.txtTagsFilter.Location = new System.Drawing.Point(480, 9);
            this.txtTagsFilter.Size = new System.Drawing.Size(150, 20);
            this.txtTagsFilter.PlaceholderText = "Filter by tags...";
            this.txtTagsFilter.TextChanged += (s, e) => LoadRecipes();

            // Recipes DataGrid
            var lblRecipes = new Label { Text = "Recipes:", Location = new System.Drawing.Point(12, 35), AutoSize = true, Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold) };
            this.dataGridViewRecipes.Location = new System.Drawing.Point(12, 55);
            this.dataGridViewRecipes.Size = new System.Drawing.Size(400, 200);
            this.dataGridViewRecipes.ReadOnly = true;
            this.dataGridViewRecipes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewRecipes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewRecipes.RowHeadersVisible = false;
            this.dataGridViewRecipes.SelectionChanged += (s, e) => LoadSelectedRecipe();

            // Ingredients DataGrid
            var lblIngredients = new Label { Text = "Recipe Ingredients:", Location = new System.Drawing.Point(12, 260), AutoSize = true, Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold) };
            this.dataGridViewIngredients.Location = new System.Drawing.Point(12, 280);
            this.dataGridViewIngredients.Size = new System.Drawing.Size(600, 150);
            this.dataGridViewIngredients.ReadOnly = true;
            this.dataGridViewIngredients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewIngredients.RowHeadersVisible = false;

            // Cost Summary - FIXED: No line breaks, single line display
            this.lblCostSummary.Location = new System.Drawing.Point(12, 440);
            this.lblCostSummary.Size = new System.Drawing.Size(800, 40);
            this.lblCostSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblCostSummary.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblCostSummary.Text = "Select a recipe to view cost breakdown";

            // Buttons
            this.btnAddRecipe.Location = new System.Drawing.Point(12, 490);
            this.btnAddRecipe.Size = new System.Drawing.Size(90, 30);
            this.btnAddRecipe.Text = "Add Recipe";
            this.btnAddRecipe.Click += (s, e) => AddRecipe();

            this.btnEditRecipe.Location = new System.Drawing.Point(112, 490);
            this.btnEditRecipe.Size = new System.Drawing.Size(90, 30);
            this.btnEditRecipe.Text = "Edit Recipe";
            this.btnEditRecipe.Click += (s, e) => EditRecipe();

            this.btnDeleteRecipe.Location = new System.Drawing.Point(212, 490);
            this.btnDeleteRecipe.Size = new System.Drawing.Size(90, 30);
            this.btnDeleteRecipe.Text = "Delete Recipe";
            this.btnDeleteRecipe.Click += (s, e) => DeleteRecipe();

            this.btnClose.Location = new System.Drawing.Point(798, 490);
            this.btnClose.Size = new System.Drawing.Size(90, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, lblCategory, cmbCategoryFilter, lblTags, txtTagsFilter,
                lblRecipes, dataGridViewRecipes, lblIngredients, dataGridViewIngredients,
                lblCostSummary, btnAddRecipe, btnEditRecipe, btnDeleteRecipe, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadCategories()
        {
            try
            {
                var categories = DatabaseContext.GetRecipeCategories();
                cmbCategoryFilter.Items.Clear();
                cmbCategoryFilter.Items.Add("All Categories");
                
                foreach (var category in categories)
                {
                    cmbCategoryFilter.Items.Add(category);
                }
                
                cmbCategoryFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRecipes()
        {
            try
            {
                var searchTerm = txtSearch.Text.ToLower();
                var categoryFilter = cmbCategoryFilter.SelectedIndex > 0 ? cmbCategoryFilter.SelectedItem.ToString() : "";
                var tagsFilter = txtTagsFilter.Text.ToLower();

                _allRecipes = DatabaseContext.GetAllRecipes();
                
                var filteredRecipes = _allRecipes
                    .Where(r => (string.IsNullOrEmpty(searchTerm) || r.Name.ToLower().Contains(searchTerm)) &&
                               (string.IsNullOrEmpty(categoryFilter) || r.Category == categoryFilter) &&
                               (string.IsNullOrEmpty(tagsFilter) || (r.Tags != null && r.Tags.Any(t => t.ToLower().Contains(tagsFilter)))))
                    .OrderBy(r => r.Name)
                    .ToList();

                dataGridViewRecipes.DataSource = filteredRecipes;
                
                if (dataGridViewRecipes.Columns.Count > 0)
                {
                    dataGridViewRecipes.Columns["Id"].Visible = false;
                    dataGridViewRecipes.Columns["Description"].Visible = false;
                    dataGridViewRecipes.Columns["Tags"].Visible = false;
                    dataGridViewRecipes.Columns["BatchYield"].Visible = false;
                    dataGridViewRecipes.Columns["TargetFoodCostPercentage"].Visible = false;
                    dataGridViewRecipes.Columns["Ingredients"].Visible = false;
                    
                    dataGridViewRecipes.Columns["Name"].HeaderText = "Recipe Name";
                    dataGridViewRecipes.Columns["Category"].HeaderText = "Category";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recipes: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSelectedRecipe()
        {
            if (dataGridViewRecipes.SelectedRows.Count > 0)
            {
                var recipeId = (int)dataGridViewRecipes.SelectedRows[0].Cells["Id"].Value;
                _currentRecipe = DatabaseContext.GetRecipeById(recipeId);
                
                if (_currentRecipe != null)
                {
                    // Load ingredients
                    dataGridViewIngredients.DataSource = _currentRecipe.Ingredients;
                    
                    if (dataGridViewIngredients.Columns.Count > 0)
                    {
                        dataGridViewIngredients.Columns["IngredientId"].Visible = false;
                        
                        dataGridViewIngredients.Columns["UnitPrice"].DefaultCellStyle.Format = $"{currencySymbol} 0.00";
                        dataGridViewIngredients.Columns["LineCost"].DefaultCellStyle.Format = $"{currencySymbol} 0.00";
                        
                        dataGridViewIngredients.Columns["IngredientName"].HeaderText = "Ingredient";
                        dataGridViewIngredients.Columns["Unit"].HeaderText = "Unit";
                        dataGridViewIngredients.Columns["Quantity"].HeaderText = "Quantity";
                        dataGridViewIngredients.Columns["UnitPrice"].HeaderText = $"Unit Price ({currencySymbol})";
                        dataGridViewIngredients.Columns["LineCost"].HeaderText = $"Line Cost ({currencySymbol})";
                        dataGridViewIngredients.Columns["Supplier"].HeaderText = "Supplier";
                    }
                    
                    UpdateCostSummary();
                }
            }
        }

        private void UpdateCostSummary()
        {
            if (_currentRecipe == null) return;

            try
            {
                decimal totalCost = _currentRecipe.Ingredients.Sum(i => i.LineCost);
                decimal costPerServing = _currentRecipe.BatchYield > 0 ? totalCost / _currentRecipe.BatchYield : 0;

                // FIXED: Single line display without line breaks
                string costSummary = $"Recipe: {_currentRecipe.Name} | " +
                                   $"Total Cost: {currencySymbol}{totalCost:F2} | " +
                                   $"Cost per Serving: {currencySymbol}{costPerServing:F2} | " +
                                   $"Target Food Cost: {_currentRecipe.TargetFoodCostPercentage:P0}";

                // Calculate suggested prices
                if (costPerServing > 0)
                {
                    decimal suggestedPrice25 = Math.Round((costPerServing / 0.25m) / 5, 0) * 5;
                    decimal suggestedPrice30 = Math.Round((costPerServing / 0.30m) / 5, 0) * 5;
                    decimal suggestedPrice35 = Math.Round((costPerServing / 0.35m) / 5, 0) * 5;
                    decimal targetPrice = Math.Round((costPerServing / _currentRecipe.TargetFoodCostPercentage) / 5, 0) * 5;

                    string priceSuggestions = $" | Suggested Prices: 25%: {currencySymbol}{suggestedPrice25} | " +
                                            $"30%: {currencySymbol}{suggestedPrice30} | " +
                                            $"35%: {currencySymbol}{suggestedPrice35} | " +
                                            $"Target: {currencySymbol}{targetPrice}";

                    costSummary += priceSuggestions;
                }

                lblCostSummary.Text = costSummary;
            }
            catch (Exception ex)
            {
                lblCostSummary.Text = $"Error calculating costs: {ex.Message}";
            }
        }

        private void AddRecipe()
        {
            MessageBox.Show("Add Recipe functionality would be implemented here with RecipeEditForm", "Info", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EditRecipe()
        {
            if (_currentRecipe != null)
            {
                MessageBox.Show($"Edit Recipe functionality for '{_currentRecipe.Name}' would be implemented here with RecipeEditForm", "Info", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please select a recipe to edit.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteRecipe()
        {
            if (_currentRecipe != null)
            {
                var result = MessageBox.Show($"Delete {_currentRecipe.Name}?", "Confirm Delete", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        DatabaseContext.DeleteRecipe(_currentRecipe.Id);
                        _currentRecipe = null;
                        LoadRecipes();
                        dataGridViewIngredients.DataSource = null;
                        lblCostSummary.Text = "Select a recipe to view cost breakdown";
                        MessageBox.Show("Recipe deleted successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting recipe: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a recipe to delete.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}