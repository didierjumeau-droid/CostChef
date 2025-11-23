using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text; // For StringBuilder
using System.Diagnostics;

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
                    // FIX: Get the FULL recipe data from database, not just the dropdown item
                    var fullRecipe = DatabaseContext.GetRecipeById(selectedRecipe.Id);
                    if (fullRecipe == null)
                    {
                        MessageBox.Show("Recipe not found in database.", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    currentRecipe = fullRecipe;
                    currentIngredients = fullRecipe.Ingredients ?? new List<RecipeIngredient>();

                    // Update all UI controls
                    txtRecipeName.Text = currentRecipe.Name ?? "Unnamed Recipe";
                    txtBatchYield.Text = currentRecipe.BatchYield.ToString();
                    
                    // Load target food cost
                    var targetPercent = (currentRecipe.TargetFoodCostPercentage * 100).ToString("F0") + "%";
                    cmbFoodCost.SelectedItem = targetPercent;
                    
                    // Load category and tags
                    cmbCategory.Text = currentRecipe.Category ?? "";
                    txtTags.Text = currentRecipe.Tags ?? "";
                    
                    // Load sales price
                    txtSalesPrice.Text = currentRecipe.SalesPrice > 0 ? currentRecipe.SalesPrice.ToString("F2") : "0.00";
                    
                    // FIX: Refresh the ingredients grid
                    RefreshIngredientsGrid();
                    CalculateCost();
                    
                    // Enable version history
                    btnVersionHistory.Enabled = true;
                    this.Text = $"Recipe Costing Calculator - {currentRecipe.Name}";
                    
                    Debug.WriteLine($"Loaded recipe: {currentRecipe.Name} with {currentIngredients.Count} ingredients");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recipe: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddIngredientToRecipe()
        {
            if (cmbIngredients.SelectedItem is Ingredient selectedIngredient && 
                decimal.TryParse(txtQuantity.Text, out decimal quantity) && quantity > 0)
            {
                // Check if ingredient is already in the list
                var existing = currentIngredients.FirstOrDefault(ri => ri.IngredientId == selectedIngredient.Id);

                if (existing != null)
                {
                    existing.Quantity += quantity;
                }
                else
                {
                    currentIngredients.Add(new RecipeIngredient
                    {
                        IngredientId = selectedIngredient.Id,
                        Quantity = quantity,
                        IngredientName = selectedIngredient.Name,
                        Unit = selectedIngredient.Unit,
                        UnitPrice = selectedIngredient.UnitPrice,
                        Supplier = selectedIngredient.SupplierName,
                        YieldPercentage = selectedIngredient.YieldPercentage 
                    });
                }
                
                RefreshIngredientsGrid();
                CalculateCost();
            }
            else
            {
                MessageBox.Show("Please select a valid ingredient and quantity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveIngredient()
        {
            if (dataGridViewIngredients.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridViewIngredients.SelectedRows[0];
                if (selectedRow.DataBoundItem is RecipeIngredient selectedIngredient)
                {
                    currentIngredients.Remove(selectedIngredient);
                    RefreshIngredientsGrid();
                    CalculateCost();
                }
            }
        }
        
        private void RefreshIngredientsGrid() 
        {
            try
            {
                // Clear and reload grid.
                var bindingSource = new BindingSource { DataSource = currentIngredients.ToList() };
                dataGridViewIngredients.DataSource = bindingSource;
                
                // Force cost recalculation
                CalculateCost();
                btnVersionHistory.Enabled = currentRecipe.Id > 0;
                
                Debug.WriteLine($"Refreshed ingredients grid with {currentIngredients.Count} items");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing ingredients grid: {ex.Message}");
            }
        }

        private void dataGridViewIngredients_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Logic to update ingredient quantity on cell edit
            if (e.ColumnIndex == GetColumnIndex("Quantity") && e.RowIndex >= 0)
            {
                if (dataGridViewIngredients.Rows[e.RowIndex].DataBoundItem is RecipeIngredient ingredientToUpdate)
                {
                    if (decimal.TryParse(dataGridViewIngredients.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString(), out decimal newQuantity))
                    {
                        UpdateIngredientQuantity(ingredientToUpdate, newQuantity);
                    }
                    else
                    {
                        // Revert change on invalid input
                        dataGridViewIngredients.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ingredientToUpdate.Quantity;
                        MessageBox.Show("Invalid quantity entered.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            
            CalculateCost();
        }

        private void UpdateIngredientQuantity(RecipeIngredient ingredient, decimal newQuantity)
        {
            var existing = currentIngredients.FirstOrDefault(ri => ri.IngredientId == ingredient.IngredientId);
            if (existing != null)
            {
                existing.Quantity = newQuantity;
                RefreshIngredientsGrid(); 
            }
        }

        private int GetColumnIndex(string columnName)
        {
            if (dataGridViewIngredients.Columns.Contains(columnName))
            {
                return dataGridViewIngredients.Columns[columnName].Index;
            }
            return -1;
        }

        private void CalculateCost()
        {
            try
            {
                // Implementation: Calculate total cost and update summary label
                decimal totalCost = currentIngredients.Sum(i => i.LineCost);
                
                int batchYield;
                if (!int.TryParse(txtBatchYield.Text, out batchYield) || batchYield <= 0)
                {
                    batchYield = 1;
                }
                
                decimal costPerServing = totalCost / batchYield;
                
                // Read target food cost
                decimal targetFoodCost = 0.30m; // Default
                if (cmbFoodCost.SelectedItem != null)
                {
                    var percent = cmbFoodCost.SelectedItem.ToString().Replace("%", "");
                    if (decimal.TryParse(percent, out decimal foodCost))
                    {
                        targetFoodCost = foodCost / 100m;
                    }
                }
                
                // Read Sales Price from the current recipe object
                decimal salesPrice = currentRecipe.SalesPrice; 

                // Calculate suggested Sales Price based on Target Food Cost
                decimal suggestedSalesPrice = targetFoodCost > 0 ? costPerServing / targetFoodCost : 0;
                
                // Update the current recipe object for saving
                currentRecipe.Ingredients = currentIngredients.ToList();
                currentRecipe.BatchYield = batchYield;
                currentRecipe.TargetFoodCostPercentage = targetFoodCost;
                
                // Compute current profitability
                decimal profitPerServing = salesPrice - costPerServing;
                decimal profitMargin = salesPrice > 0 ? (profitPerServing / salesPrice) : 0;
                decimal actualFoodCostPercentage = salesPrice > 0 ? (costPerServing / salesPrice) : 0;
                
                // Create Summary Text
                StringBuilder summary = new StringBuilder();
                summary.AppendLine($"Total Ingredient Cost: {AppSettings.FormatCurrency(totalCost)}");
                summary.AppendLine($"Cost Per Serving ({batchYield} yield): {AppSettings.FormatCurrency(costPerServing)}");
                summary.AppendLine($"Actual Food Cost %: {actualFoodCostPercentage * 100:F1}%");
                summary.AppendLine("--------------------------------------------------");
                summary.AppendLine($"Target Food Cost: {targetFoodCost * 100:F1}%");
                summary.AppendLine($"Suggested Sales Price (at Target FC): {AppSettings.FormatCurrency(suggestedSalesPrice)}");
                
                if (salesPrice > 0)
                {
                    summary.AppendLine($"Current Sales Price: {AppSettings.FormatCurrency(salesPrice)}");
                    summary.AppendLine($"Profit Per Serving: {AppSettings.FormatCurrency(profitPerServing)}");
                    summary.AppendLine($"Profit Margin: {profitMargin * 100:F1}%");
                }
                                
                lblCostSummary.Text = summary.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating cost: {ex.Message}");
                lblCostSummary.Text = "Error calculating cost. Please check your inputs.";
            }
        }

        private void LoadExistingRecipes()
        {
            try
            {
                allRecipes = DatabaseContext.GetAllRecipes();
                cmbExistingRecipes.DataSource = null; // Clear first
                cmbExistingRecipes.DataSource = allRecipes.ToList(); 
                cmbExistingRecipes.DisplayMember = "Name";
                cmbExistingRecipes.ValueMember = "Id";
                
                Debug.WriteLine($"Loaded {allRecipes.Count} recipes into dropdown");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recipes: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchRecipes()
        {
            var searchText = txtSearchRecipes.Text.Trim().ToLower();
            var filtered = allRecipes.Where(r => 
                r.Name.ToLower().Contains(searchText) ||
                (r.Tags ?? "").ToLower().Contains(searchText) ||
                (r.Category ?? "").ToLower().Contains(searchText)
            ).ToList();

            cmbExistingRecipes.DataSource = filtered;
            cmbExistingRecipes.DisplayMember = "Name";
            cmbExistingRecipes.ValueMember = "Id";
        }

        private void ClearSearch()
        {
            txtSearchRecipes.Text = string.Empty;
            LoadExistingRecipes();
        }

        private void UpdateUnitDisplay()
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

        private void DeleteRecipe()
        {
            if (currentRecipe.Id == 0)
            {
                MessageBox.Show("No recipe is currently loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete the recipe '{currentRecipe.Name}'?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                DatabaseContext.DeleteRecipe(currentRecipe.Id);
                LoadExistingRecipes();
                InitializeNewRecipe(); // Reset form
                MessageBox.Show("Recipe deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private void LoadRecipeIngredients(List<RecipeIngredient> ingredients)
        {
            // Sets up the DataGridView DataSource
            currentIngredients = ingredients.ToList();
            RefreshIngredientsGrid();
        }
    }
}