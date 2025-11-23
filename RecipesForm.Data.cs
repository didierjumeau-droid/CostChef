using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CostChef
{
    public partial class RecipesForm : Form
    {
        // ===========================================================
        // MAIN SAVE ENTRY POINT (btnSaveRecipe → SaveRecipe)
        // ===========================================================
        private void SaveRecipe()
        {
            try
            {
                string name = txtRecipeName.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Please enter a recipe name.", "Missing Name",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (currentIngredients == null)
                    currentIngredients = new List<RecipeIngredient>();

                bool editingExisting = currentRecipe != null && currentRecipe.Id > 0;

                // =======================================================
                // CASE 1: EXISTING RECIPE (option 1)
                // =======================================================
                if (editingExisting)
                {
                    var result = MessageBox.Show(
                        "You are modifying an existing recipe.\n\n" +
                        "YES    = Overwrite the current recipe\n" +
                        "NO     = Save a new copy under a different name\n" +
                        "CANCEL = Return without saving\n\n" +
                        $"Do you want to save changes to '{currentRecipe.Name}'?",
                        "Save Recipe",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Cancel)
                        return;

                    if (result == DialogResult.Yes)
                    {
                        // Overwrite existing recipe
                        ApplyFormValuesToCurrentRecipe();
                        DatabaseContext.UpdateRecipe(currentRecipe);
                        SaveRecipeIngredients(currentRecipe.Id);

                        LoadExistingRecipes();
                        SelectRecipeInComboById(currentRecipe.Id);

                        MessageBox.Show("Recipe saved.", "Saved",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (result == DialogResult.No)
                    {
                        // Save As – propose "<name> - Copy" and let user edit
                        string newName = ShowSaveAsDialog(currentRecipe.Name);
                        if (newName == null)
                            return; // user cancelled

                        var newRecipe = CreateRecipeFromForm(newName);
                        int newId = DatabaseContext.InsertRecipe(newRecipe);
                        SaveRecipeIngredients(newId);

                        newRecipe.Id = newId;
                        currentRecipe = newRecipe;

                        LoadExistingRecipes();
                        SelectRecipeInComboById(newId);

                        MessageBox.Show($"Saved as '{newName}'.", "Saved As",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    return;
                }

                // =======================================================
                // CASE 2: NO CURRENT RECIPE (fallback)
                // Should not happen in Option 1, but kept safe.
                // =======================================================
                var recipeToCreate = CreateRecipeFromForm(name);
                int createdId = DatabaseContext.InsertRecipe(recipeToCreate);
                SaveRecipeIngredients(createdId);

                recipeToCreate.Id = createdId;
                currentRecipe = recipeToCreate;

                LoadExistingRecipes();
                SelectRecipeInComboById(createdId);

                MessageBox.Show("Recipe created.", "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving recipe:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===========================================================
        // APPLY FORM DATA TO EXISTING CURRENT RECIPE
        // ===========================================================
        private void ApplyFormValuesToCurrentRecipe()
        {
            if (currentRecipe == null)
                currentRecipe = new Recipe();

            currentRecipe.Name = txtRecipeName.Text?.Trim() ?? string.Empty;
            currentRecipe.Category = cmbCategory.Text?.Trim() ?? string.Empty;

            // Tags
            var rawTags = txtTags.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(rawTags))
            {
                var tags = rawTags
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => t.Length > 0);
                currentRecipe.Tags = string.Join(",", tags);
            }
            else
            {
                currentRecipe.Tags = string.Empty;
            }

            // Batch yield
            if (!int.TryParse(txtBatchYield.Text, out int batchYield) || batchYield <= 0)
                batchYield = 1;
            currentRecipe.BatchYield = batchYield;

            // Target food cost %
            decimal targetFoodCost = 0.30m; // default 30%
            if (cmbFoodCost.SelectedItem is string fc)
            {
                var numericPart = fc.Replace("%", "").Trim();
                if (decimal.TryParse(numericPart, out var percentValue) && percentValue > 0)
                    targetFoodCost = percentValue / 100m;
            }
            currentRecipe.TargetFoodCostPercentage = targetFoodCost;
        }

        // ===========================================================
        // BUILD A NEW RECIPE FROM FORM VALUES (USED FOR "SAVE AS")
        // ===========================================================
        private Recipe CreateRecipeFromForm(string recipeName)
        {
            var recipe = new Recipe
            {
                Name = recipeName?.Trim() ?? string.Empty,
                Description = string.Empty,
                Category = cmbCategory.Text?.Trim() ?? string.Empty,
                Tags = string.Empty,
                BatchYield = 1,
                TargetFoodCostPercentage = 0.30m,
                SalesPrice = 0m
            };

            // Tags
            var rawTags = txtTags.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(rawTags))
            {
                var tags = rawTags
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => t.Length > 0);
                recipe.Tags = string.Join(",", tags);
            }

            // Batch yield
            if (int.TryParse(txtBatchYield.Text, out int batchYield) && batchYield > 0)
                recipe.BatchYield = batchYield;

            // Target food cost %
            decimal targetFoodCost = 0.30m;
            if (cmbFoodCost.SelectedItem is string fc)
            {
                var numericPart = fc.Replace("%", "").Trim();
                if (decimal.TryParse(numericPart, out var percentValue) && percentValue > 0)
                    targetFoodCost = percentValue / 100m;
            }
            recipe.TargetFoodCostPercentage = targetFoodCost;

            return recipe;
        }

        // ===========================================================
        // SAVE INGREDIENTS FOR THE GIVEN RECIPE ID
        // ===========================================================
        private void SaveRecipeIngredients(int recipeId)
        {
            if (currentIngredients == null)
                currentIngredients = new List<RecipeIngredient>();

            foreach (var ing in currentIngredients)
                ing.RecipeId = recipeId;

            DatabaseContext.UpdateRecipeIngredients(recipeId, currentIngredients);
        }

        // ===========================================================
        // SELECT A RECIPE IN THE COMBO BY ID
        // ===========================================================
        private void SelectRecipeInComboById(int recipeId)
        {
            try
            {
                if (cmbExistingRecipes?.DataSource is IEnumerable<Recipe> ds)
                {
                    var list = ds.ToList();
                    var match = list.FirstOrDefault(r => r.Id == recipeId);
                    if (match != null)
                    {
                        cmbExistingRecipes.SelectedItem = match;
                        return;
                    }
                }

                if (allRecipes != null)
                {
                    var match = allRecipes.FirstOrDefault(r => r.Id == recipeId);
                    if (match != null)
                    {
                        cmbExistingRecipes.SelectedItem = match;
                    }
                }
            }
            catch
            {
                // Non-critical
            }
        }

        // ===========================================================
        // INLINE "SAVE AS" DIALOG
        // ===========================================================
        private string ShowSaveAsDialog(string currentName)
        {
            using (var form = new Form())
            {
                form.Text = "Save Recipe As";
                form.ClientSize = new System.Drawing.Size(400, 150);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.ShowInTaskbar = false;

                var lblPrompt = new Label
                {
                    Text = "Enter the new name for the recipe:",
                    Location = new System.Drawing.Point(12, 12),
                    Size = new System.Drawing.Size(360, 20)
                };

                var txtNewName = new TextBox
                {
                    Location = new System.Drawing.Point(12, 40),
                    Size = new System.Drawing.Size(366, 20),
                    Text = (currentName ?? string.Empty) + " - Copy"
                };
                txtNewName.SelectAll();

                var btnOK = new Button
                {
                    Location = new System.Drawing.Point(200, 100),
                    Size = new System.Drawing.Size(80, 30),
                    Text = "OK",
                    DialogResult = DialogResult.OK
                };

                var btnCancel = new Button
                {
                    Location = new System.Drawing.Point(290, 100),
                    Size = new System.Drawing.Size(80, 30),
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel
                };

                form.Controls.Add(lblPrompt);
                form.Controls.Add(txtNewName);
                form.Controls.Add(btnOK);
                form.Controls.Add(btnCancel);

                form.AcceptButton = btnOK;
                form.CancelButton = btnCancel;

                if (form.ShowDialog(this) == DialogResult.OK)
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

                return null; // user cancelled
            }
        }
    }
}
