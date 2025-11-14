using System;
using System.Windows.Forms;
using System.Linq;

namespace CostChef
{
    public partial class RecipesForm : Form
    {
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
                    
                    // AUTOMATICALLY OPEN SAVE AS DIALOG FOR DUPLICATES
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
                            return; // Let user try again with a different name
                        }
                        
                        // Use the new name and continue with normal save flow
                        currentRecipe.Name = newName;
                        txtRecipeName.Text = currentRecipe.Name;
                        
                        // Force "Save As" behavior - treat as new recipe
                        currentRecipe.Id = 0;
                    }
                    else
                    {
                        return; // User cancelled the Save As dialog
                    }
                }

                // Update current recipe with form data
                currentRecipe.Name = txtRecipeName.Text.Trim();
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

                // Only show save options if not already in "Save As" mode from duplicate handling
                if (currentRecipe.Id != 0)
                {
                    // Show save options dialog
                    var saveResult = MessageBox.Show(
                        $"Recipe: {currentRecipe.Name}\n\n" +
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
                        isNewRecipe = false;
                    }
                    else if (saveResult == DialogResult.No)
                    {
                        // Save as copy - create new recipe (clone)
                        // Use a loop to handle duplicate names and keep showing the Save As dialog
                        bool validNameEntered = false;
                        
                        while (!validNameEntered)
                        {
                            string newName = ShowSaveAsDialog(currentRecipe.Name);
                            if (string.IsNullOrEmpty(newName))
                            {
                                shouldSave = false; // User cancelled
                                break;
                            }
                            
                            // Check if the new name is also a duplicate
                            var newNameDuplicate = allRecipes.Any(r => 
                                r.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));
                            
                            if (newNameDuplicate)
                            {
                                MessageBox.Show($"A recipe named '{newName}' already exists.\n\nPlease choose a different name.", 
                                    "Duplicate Recipe Name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                // Continue the loop to show the Save As dialog again
                                continue;
                            }
                            
                            // Valid non-duplicate name entered
                            currentRecipe.Name = newName;
                            txtRecipeName.Text = currentRecipe.Name;
                            currentRecipe.Id = 0;
                            isNewRecipe = true;
                            validNameEntered = true;
                        }
                    }
                    else
                    {
                        shouldSave = false; // User cancelled
                    }
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