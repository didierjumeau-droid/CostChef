using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace CostChef
{
    public partial class RecipesForm : Form
    {
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

       // In RecipesForm.Logic.cs - Update AddIngredientToRecipe method
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
            Supplier = selectedIngredient.SupplierName,
            YieldPercentage = selectedIngredient.YieldPercentage // NEW: Store yield at recipe creation
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
    }
}