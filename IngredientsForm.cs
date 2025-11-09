        private void SaveRecipe()
        {
            if (string.IsNullOrWhiteSpace(currentRecipe.Name))
            {
                MessageBox.Show("Please enter a recipe name.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                bool isNewRecipe = (currentRecipe.Id == 0);
                
                if (!isNewRecipe)
                {
                    // Recipe is being modified - ask user what they want to do
                    var result = MessageBox.Show(
                        $"Recipe '{currentRecipe.Name}' already exists.\n\n" +
                        "What would you like to do?\n\n" +
                        "• 'Yes' - Overwrite the existing recipe\n" +
                        "• 'No' - Save as a new recipe with a different name\n" +
                        "• 'Cancel' - Go back and make changes",
                        "Save Recipe Options",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button3); // Default to Cancel for safety

                    switch (result)
                    {
                        case DialogResult.Yes:
                            // Overwrite existing recipe
                            DatabaseContext.UpdateRecipe(currentRecipe);
                            MessageBox.Show($"Recipe '{currentRecipe.Name}' updated successfully!", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                            
                        case DialogResult.No:
                            // Save as new recipe - prompt for new name
                            using (var nameDialog = new Form())
                            {
                                nameDialog.Text = "Save As New Recipe";
                                nameDialog.Size = new System.Drawing.Size(350, 150);
                                nameDialog.StartPosition = FormStartPosition.CenterParent;
                                nameDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                                nameDialog.MaximizeBox = false;
                                
                                var lblNewName = new Label { Text = "New Recipe Name:", Location = new System.Drawing.Point(20, 20), Size = new System.Drawing.Size(100, 20) };
                                var txtNewName = new TextBox { Text = $"{currentRecipe.Name} (Copy)", Location = new System.Drawing.Point(120, 17), Size = new System.Drawing.Size(200, 20) };
                                var btnOk = new Button { Text = "OK", Location = new System.Drawing.Point(120, 50), Size = new System.Drawing.Size(75, 30), DialogResult = DialogResult.OK };
                                var btnCancel = new Button { Text = "Cancel", Location = new System.Drawing.Point(205, 50), Size = new System.Drawing.Size(75, 30), DialogResult = DialogResult.Cancel };
                                
                                nameDialog.Controls.AddRange(new Control[] { lblNewName, txtNewName, btnOk, btnCancel });
                                nameDialog.AcceptButton = btnOk;
                                nameDialog.CancelButton = btnCancel;
                                
                                if (nameDialog.ShowDialog() == DialogResult.OK)
                                {
                                    var newName = txtNewName.Text.Trim();
                                    if (!string.IsNullOrEmpty(newName))
                                    {
                                        // Create a copy of the current recipe with new name and ID
                                        var newRecipe = new Recipe
                                        {
                                            Name = newName,
                                            Description = currentRecipe.Description,
                                            Category = currentRecipe.Category,
                                            Tags = currentRecipe.Tags,
                                            BatchYield = currentRecipe.BatchYield,
                                            TargetFoodCostPercentage = currentRecipe.TargetFoodCostPercentage
                                        };
                                        
                                        // Save the new recipe
                                        DatabaseContext.InsertRecipe(newRecipe);
                                        
                                        // Now save all the ingredients for the new recipe
                                        foreach (var ingredient in currentIngredients)
                                        {
                                            var newRecipeIngredient = new RecipeIngredient
                                            {
                                                RecipeId = newRecipe.Id,
                                                IngredientId = ingredient.IngredientId,
                                                Quantity = ingredient.Quantity
                                            };
                                            DatabaseContext.AddRecipeIngredient(newRecipeIngredient);
                                        }
                                        
                                        // Update the current recipe to the new one
                                        currentRecipe = newRecipe;
                                        currentRecipe.Id = newRecipe.Id; // Update the ID
                                        
                                        MessageBox.Show($"Recipe '{newRecipe.Name}' saved successfully as a new recipe!", "Success", 
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Please enter a valid recipe name.", "Error", 
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                                else
                                {
                                    return; // User cancelled the name dialog
                                }
                            }
                            break;
                            
                        case DialogResult.Cancel:
                            return; // User cancelled the operation
                    }
                }
                else
                {
                    // This is a brand new recipe
                    DatabaseContext.InsertRecipe(currentRecipe);
                    MessageBox.Show($"Recipe '{currentRecipe.Name}' saved successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                // Add new category to available categories if it doesn't exist
                if (!string.IsNullOrEmpty(currentRecipe.Category) && 
                    !cmbCategory.Items.Cast<string>().Any(c => 
                        c.Equals(currentRecipe.Category, StringComparison.OrdinalIgnoreCase)))
                {
                    cmbCategory.Items.Add(currentRecipe.Category);
                }
                
                LoadExistingRecipes();
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
            // Add your delete recipe logic here
        }
    }
}