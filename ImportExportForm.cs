using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace CostChef
{
    public partial class ImportExportForm : Form
    {
        private Button btnExportIngredients;
        private Button btnImportIngredients;
        private Button btnExportRecipes;
        private Button btnImportRecipes;
        private Button btnClose;
        private ListBox lstLog;
        private ImportExportService importExportService;

        public ImportExportForm()
        {
            InitializeComponent();
            importExportService = new ImportExportService();
        }

        private void InitializeComponent()
        {
            this.btnExportIngredients = new Button();
            this.btnImportIngredients = new Button();
            this.btnExportRecipes = new Button();
            this.btnImportRecipes = new Button();
            this.btnClose = new Button();
            this.lstLog = new ListBox();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(500, 400);
            this.Text = "Import/Export";
            this.StartPosition = FormStartPosition.CenterParent;

            // Export Ingredients Button
            this.btnExportIngredients.Location = new System.Drawing.Point(20, 20);
            this.btnExportIngredients.Size = new System.Drawing.Size(150, 30);
            this.btnExportIngredients.Text = "Export Ingredients to CSV";
            this.btnExportIngredients.Click += (s, e) => ExportIngredients();

            // Import Ingredients Button
            this.btnImportIngredients.Location = new System.Drawing.Point(20, 60);
            this.btnImportIngredients.Size = new System.Drawing.Size(150, 30);
            this.btnImportIngredients.Text = "Import Ingredients from CSV";
            this.btnImportIngredients.Click += (s, e) => ImportIngredients();

            // Export Recipes Button
            this.btnExportRecipes.Location = new System.Drawing.Point(20, 100);
            this.btnExportRecipes.Size = new System.Drawing.Size(150, 30);
            this.btnExportRecipes.Text = "Export Recipes to CSV";
            this.btnExportRecipes.Click += (s, e) => ExportRecipes();

            // Import Recipes Button
            this.btnImportRecipes.Location = new System.Drawing.Point(20, 140);
            this.btnImportRecipes.Size = new System.Drawing.Size(150, 30);
            this.btnImportRecipes.Text = "Import Recipes from CSV";
            this.btnImportRecipes.Click += (s, e) => ImportRecipes();

            // Close Button
            this.btnClose.Location = new System.Drawing.Point(20, 180);
            this.btnClose.Size = new System.Drawing.Size(150, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            // Log ListBox
            this.lstLog.Location = new System.Drawing.Point(200, 20);
            this.lstLog.Size = new System.Drawing.Size(280, 350);
            this.lstLog.HorizontalScrollbar = true;

            // Add controls
            this.Controls.AddRange(new Control[] {
                btnExportIngredients, btnImportIngredients, btnExportRecipes,
                btnImportRecipes, btnClose, lstLog
            });

            this.ResumeLayout(false);
        }

        private void ExportIngredients()
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV Files (*.csv)|*.csv";
                    saveDialog.Title = "Export Ingredients to CSV";
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var ingredients = DatabaseContext.GetAllIngredients();
                        bool success = importExportService.ExportIngredientsToCsv(ingredients, saveDialog.FileName);
                        
                        if (success)
                        {
                            lstLog.Items.Add($"✓ Ingredients exported to: {saveDialog.FileName}");
                            MessageBox.Show("Ingredients exported successfully!", "Export Complete", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lstLog.Items.Add($"✗ Export failed: {ex.Message}");
                MessageBox.Show($"Error exporting ingredients: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportIngredients()
        {
            try
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "CSV Files (*.csv)|*.csv";
                    openDialog.Title = "Import Ingredients from CSV";
                    
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        var importedIngredients = importExportService.ImportIngredientsFromCsv(openDialog.FileName);
                        
                        if (importedIngredients != null && importedIngredients.Count > 0)
                        {
                            // Save imported ingredients to database
                            foreach (var ingredient in importedIngredients)
                            {
                                DatabaseContext.InsertIngredient(ingredient);
                            }
                            
                            lstLog.Items.Add($"✓ {importedIngredients.Count} ingredients imported from: {openDialog.FileName}");
                            MessageBox.Show($"{importedIngredients.Count} ingredients imported successfully!", "Import Complete", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            lstLog.Items.Add($"✗ No ingredients found in: {openDialog.FileName}");
                            MessageBox.Show("No ingredients found in the selected file.", "Import Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lstLog.Items.Add($"✗ Import failed: {ex.Message}");
                MessageBox.Show($"Error importing ingredients: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportRecipes()
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV Files (*.csv)|*.csv";
                    saveDialog.Title = "Export Recipes to CSV";
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var recipes = DatabaseContext.GetAllRecipes();
                        bool success = true;
                        
                        foreach (var recipe in recipes)
                        {
                            success = importExportService.ExportRecipeToCsv(recipe, 
                                Path.Combine(Path.GetDirectoryName(saveDialog.FileName), 
                                $"{recipe.Name.Replace(" ", "_")}.csv")) && success;
                        }
                        
                        if (success)
                        {
                            lstLog.Items.Add($"✓ {recipes.Count} recipes exported");
                            MessageBox.Show($"{recipes.Count} recipes exported successfully!", "Export Complete", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lstLog.Items.Add($"✗ Export failed: {ex.Message}");
                MessageBox.Show($"Error exporting recipes: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportRecipes()
        {
            try
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "CSV Files (*.csv)|*.csv";
                    openDialog.Title = "Import Recipes from CSV";
                    openDialog.Multiselect = true;
                    
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        int totalImported = 0;
                        
                        foreach (string filePath in openDialog.FileNames)
                        {
                            var importedRecipes = importExportService.ImportRecipeFromCsv(filePath);
                            
                            if (importedRecipes != null && importedRecipes.Count > 0)
                            {
                                // Save imported recipes to database
                                foreach (var recipe in importedRecipes)
                                {
                                    DatabaseContext.InsertRecipe(recipe);
                                    totalImported++;
                                }
                                
                                lstLog.Items.Add($"✓ {importedRecipes.Count} recipes imported from: {Path.GetFileName(filePath)}");
                            }
                        }
                        
                        if (totalImported > 0)
                        {
                            MessageBox.Show($"{totalImported} recipes imported successfully!", "Import Complete", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No recipes found in the selected files.", "Import Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lstLog.Items.Add($"✗ Import failed: {ex.Message}");
                MessageBox.Show($"Error importing recipes: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Log(string message)
        {
            lstLog.Items.Add($"{DateTime.Now:HH:mm:ss} - {message}");
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }
    }
}