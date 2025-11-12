using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace CostChef
{
    public partial class ImportExportForm : Form
    {
        public ImportExportForm()
        {
            InitializeComponent();
            SetupControls();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(400, 300);
            Name = "ImportExportForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Import/Export Data";
            ResumeLayout(false);
        }

        private void SetupControls()
        {
            Controls.Clear();
            
            var lblTitle = new Label 
            { 
                Text = "Data Management", 
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(120, 20),
                Size = new Size(200, 30)
            };
            
            // Main action buttons - only the 4 core options
            var btnExportRecipesCSV = new Button 
            { 
                Text = "Export Recipes to CSV",
                Location = new Point(50, 70),
                Size = new Size(300, 30)
            };
            btnExportRecipesCSV.Click += (s, e) => ShowRecipeSelection();
            
            var btnExportIngredientsCSV = new Button 
            { 
                Text = "Export Ingredients to CSV",
                Location = new Point(50, 110),
                Size = new Size(300, 30)
            };
            btnExportIngredientsCSV.Click += (s, e) => ExportAllIngredients();
            
            var btnImportRecipesCSV = new Button 
            { 
                Text = "Import Recipes from CSV",
                Location = new Point(50, 150),
                Size = new Size(300, 30)
            };
            btnImportRecipesCSV.Click += (s, e) => ImportRecipes();
            
            var btnImportIngredientsCSV = new Button 
            { 
                Text = "Import Ingredients from CSV",
                Location = new Point(50, 190),
                Size = new Size(300, 30)
            };
            btnImportIngredientsCSV.Click += (s, e) => ImportIngredients();
            
            var btnClose = new Button 
            { 
                Text = "Close",
                Location = new Point(150, 230),
                Size = new Size(100, 30)
            };
            btnClose.Click += (s, e) => Close();

            Controls.Add(lblTitle);
            Controls.Add(btnExportRecipesCSV);
            Controls.Add(btnExportIngredientsCSV);
            Controls.Add(btnImportRecipesCSV);
            Controls.Add(btnImportIngredientsCSV);
            Controls.Add(btnClose);
        }

        private void ShowRecipeSelection()
        {
            var recipes = DatabaseContext.GetAllRecipes();
            if (recipes.Count == 0)
            {
                MessageBox.Show("No recipes found to export.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectionForm = new Form
            {
                Text = "Select Recipes to Export",
                Size = new Size(400, 500),
                StartPosition = FormStartPosition.CenterParent
            };

            var checkedListBox = new CheckedListBox
            {
                Location = new Point(20, 20),
                Size = new Size(340, 350),
                CheckOnClick = true
            };

            foreach (var recipe in recipes)
            {
                checkedListBox.Items.Add(recipe.Name, true); // Check all by default
            }

            var btnSelectAll = new Button { Text = "Select All", Location = new Point(20, 380), Size = new Size(80, 30) };
            var btnSelectNone = new Button { Text = "Select None", Location = new Point(110, 380), Size = new Size(80, 30) };
            var btnExport = new Button { Text = "Export Selected", Location = new Point(200, 380), Size = new Size(100, 30) };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(310, 380), Size = new Size(60, 30) };

            btnSelectAll.Click += (s, e) => 
            {
                for (int i = 0; i < checkedListBox.Items.Count; i++)
                    checkedListBox.SetItemChecked(i, true);
            };

            btnSelectNone.Click += (s, e) => 
            {
                for (int i = 0; i < checkedListBox.Items.Count; i++)
                    checkedListBox.SetItemChecked(i, false);
            };

            btnExport.Click += (s, e) => 
            {
                var selectedRecipes = new List<string>();
                foreach (var item in checkedListBox.CheckedItems)
                {
                    selectedRecipes.Add(item.ToString());
                }

                if (selectedRecipes.Count == 0)
                {
                    MessageBox.Show("Please select at least one recipe to export.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ExportSelectedRecipes(selectedRecipes, recipes);
                selectionForm.Close();
            };

            btnCancel.Click += (s, e) => selectionForm.Close();

            selectionForm.Controls.AddRange(new Control[] {
                checkedListBox, btnSelectAll, btnSelectNone, btnExport, btnCancel
            });

            selectionForm.ShowDialog();
        }

        private void ExportSelectedRecipes(List<string> selectedRecipeNames, List<Recipe> allRecipes)
        {
            try
            {
                // Get export location from settings
                var settings = DatabaseContext.GetAllSettings();
                
                string exportLocation = "";
                if (settings.ContainsKey("ExportLocation") && !string.IsNullOrEmpty(settings["ExportLocation"]))
                {
                    exportLocation = settings["ExportLocation"].Trim();
                    Console.WriteLine($"Using ExportLocation from settings: {exportLocation}");
                }
                
                if (string.IsNullOrEmpty(exportLocation))
                {
                    // Use Documents folder as default
                    exportLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    Console.WriteLine($"No ExportLocation in settings, using default: {exportLocation}");
                }

                // Ensure directory exists
                if (!Directory.Exists(exportLocation))
                {
                    try
                    {
                        Directory.CreateDirectory(exportLocation);
                        Console.WriteLine($"Created directory: {exportLocation}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Cannot create export directory: {ex.Message}\nUsing Documents folder instead.", "Directory Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        exportLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }
                }

                int exportedCount = 0;
                List<string> exportedFiles = new List<string>();
                
                foreach (var recipeName in selectedRecipeNames)
                {
                    var recipe = allRecipes.Find(r => r.Name == recipeName);
                    if (recipe != null)
                    {
                        string fileName = $"{SanitizeFileName(recipe.Name)}.csv";
                        string filePath = Path.Combine(exportLocation, fileName);
                        
                        Console.WriteLine($"Exporting recipe '{recipe.Name}' to: {filePath}");
                        
                        if (ExportSingleRecipeToCsv(recipe, filePath))
                        {
                            exportedCount++;
                            exportedFiles.Add(fileName);
                        }
                    }
                }

                string message = $"Successfully exported {exportedCount} recipes to:\n{exportLocation}";
                if (exportedFiles.Count > 0)
                {
                    message += "\n\nFiles created:\n• " + string.Join("\n• ", exportedFiles);
                }
                
                MessageBox.Show(message, "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting recipes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportAllIngredients()
        {
            try
            {
                var ingredients = DatabaseContext.GetAllIngredients();
                if (ingredients.Count == 0)
                {
                    MessageBox.Show("No ingredients found to export.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Get export location from settings
                var settings = DatabaseContext.GetAllSettings();
                
                string exportLocation = "";
                if (settings.ContainsKey("ExportLocation") && !string.IsNullOrEmpty(settings["ExportLocation"]))
                {
                    exportLocation = settings["ExportLocation"].Trim();
                    Console.WriteLine($"Using ExportLocation from settings: {exportLocation}");
                }
                
                if (string.IsNullOrEmpty(exportLocation))
                {
                    // Use Documents folder as default
                    exportLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    Console.WriteLine($"No ExportLocation in settings, using default: {exportLocation}");
                }

                // Ensure directory exists
                if (!Directory.Exists(exportLocation))
                {
                    try
                    {
                        Directory.CreateDirectory(exportLocation);
                        Console.WriteLine($"Created directory: {exportLocation}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Cannot create export directory: {ex.Message}\nUsing Documents folder instead.", "Directory Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        exportLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }
                }

                string fileName = "ingredients.csv";
                string filePath = Path.Combine(exportLocation, fileName);

                Console.WriteLine($"Exporting all ingredients to: {filePath}");

                bool success = ExportAllIngredientsToCsv(ingredients, filePath);
                if (success)
                    MessageBox.Show($"All ingredients exported successfully to:\n{filePath}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Failed to export ingredients.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting ingredients: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ExportSingleRecipeToCsv(Recipe recipe, string filePath)
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                
                // Write recipe header and data
                writer.WriteLine("RecipeID,RecipeName,Description,Category,Tags,BatchYield,TargetFoodCostPercentage");
                string tags = recipe.Tags != null ? string.Join(",", recipe.Tags) : "";
                writer.WriteLine($"\"{recipe.Id}\",\"{recipe.Name}\",\"{recipe.Description}\",\"{recipe.Category}\",\"{tags}\",\"{recipe.BatchYield}\",\"{recipe.TargetFoodCostPercentage}\"");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export error for {recipe.Name}: {ex.Message}");
                return false;
            }
        }

        private bool ExportAllIngredientsToCsv(List<Ingredient> ingredients, string filePath)
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                
                // Write header
                writer.WriteLine("ID,Name,Unit,UnitPrice,Category,SupplierId,SupplierName");
                
                // Write all ingredients
                foreach (var ingredient in ingredients)
                {
                    writer.WriteLine($"\"{ingredient.Id}\",\"{ingredient.Name}\",\"{ingredient.Unit}\",\"{ingredient.UnitPrice}\",\"{ingredient.Category}\",\"{ingredient.SupplierId}\",\"{ingredient.SupplierName}\"");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export error for ingredients: {ex.Message}");
                return false;
            }
        }

        private void ImportRecipes()
        {
            try
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "CSV files (*.csv)|*.csv";
                    openDialog.Multiselect = true;
                    openDialog.Title = "Import Recipes from CSV";

                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        var recipes = ImportExportService.ImportRecipesFromCsv(openDialog.FileName);
                        if (recipes != null && recipes.Count > 0)
                        {
                            // Save recipes to database
                            foreach (var recipe in recipes)
                            {
                                DatabaseContext.InsertRecipe(recipe);
                            }
                            MessageBox.Show($"{recipes.Count} recipes imported successfully!", "Import Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No recipes found in the file or failed to import.", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing recipes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportIngredients()
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "CSV files (*.csv)|*.csv";
                openDialog.Multiselect = false; // Changed to single file selection for simplicity
                openDialog.Title = "Import Ingredients from CSV";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Use the new duplicate-handling method
                        var result = ImportExportService.ImportIngredientsWithDuplicateHandling(openDialog.FileName);
                        
                        string message = $"Import completed:\n" +
                                       $"{result.imported} new ingredients imported\n" +
                                       $"{result.updated} existing ingredients updated\n" +
                                       $"{result.skipped} duplicates skipped";
                        
                        MessageBox.Show(message, "Import Complete", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error importing ingredients: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }
}