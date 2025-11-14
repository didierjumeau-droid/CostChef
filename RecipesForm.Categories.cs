using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace CostChef
{
    public partial class RecipesForm : Form
    {
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
    }
}