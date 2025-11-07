using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        // Recipe selection controls
        private CheckedListBox chkListRecipes;
        private Button btnSelectAll;
        private Button btnSelectNone;
        private Button btnExportSelected;
        private Button btnDeleteSelected;
        private Panel pnlRecipeSelection;
        private Panel pnlMain;
        private Label lblSelectRecipes;

        // Store recipe data with IDs to prevent duplicates
        private List<Recipe> _allRecipes = new List<Recipe>();

        public ImportExportForm()
        {
            InitializeComponent();
            importExportService = new ImportExportService();
            ShowMainPanel();
        }

        private void InitializeComponent()
        {
            this.btnExportIngredients = new Button();
            this.btnImportIngredients = new Button();
            this.btnExportRecipes = new Button();
            this.btnImportRecipes = new Button();
            this.btnClose = new Button();
            this.lstLog = new ListBox();

            // Recipe selection controls
            this.chkListRecipes = new CheckedListBox();
            this.btnSelectAll = new Button();
            this.btnSelectNone = new Button();
            this.btnExportSelected = new Button();
            this.btnDeleteSelected = new Button();
            this.pnlRecipeSelection = new Panel();
            this.pnlMain = new Panel();
            this.lblSelectRecipes = new Label();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(600, 450);
            this.Text = "Import/Export Data";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Main Panel
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Size = new System.Drawing.Size(600, 450);
            this.pnlMain.Visible = true;

            // Export Ingredients Button
            this.btnExportIngredients.Location = new System.Drawing.Point(20, 20);
            this.btnExportIngredients.Size = new System.Drawing.Size(200, 35);
            this.btnExportIngredients.Text = "Export Ingredients to CSV";
            this.btnExportIngredients.Click += (s, e) => ExportIngredients();

            // Import Ingredients Button
            this.btnImportIngredients.Location = new System.Drawing.Point(20, 65);
            this.btnImportIngredients.Size = new System.Drawing.Size(200, 35);
            this.btnImportIngredients.Text = "Import Ingredients from CSV";
            this.btnImportIngredients.Click += (s, e) => ImportIngredients();

            // Export Recipes Button
            this.btnExportRecipes.Location = new System.Drawing.Point(20, 110);
            this.btnExportRecipes.Size = new System.Drawing.Size(200, 35);
            this.btnExportRecipes.Text = "Export Recipes to CSV";
            this.btnExportRecipes.Click += (s, e) => ShowRecipeSelection();

            // Import Recipes Button
            this.btnImportRecipes.Location = new System.Drawing.Point(20, 155);
            this.btnImportRecipes.Size = new System.Drawing.Size(200, 35);
            this.btnImportRecipes.Text = "Import Recipes from CSV";
            this.btnImportRecipes.Click += (s, e) => ImportRecipesWithOptions();

            // Close Button
            this.btnClose.Location = new System.Drawing.Point(20, 200);
            this.btnClose.Size = new System.Drawing.Size(200, 35);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            // Log ListBox
            this.lstLog.Location = new System.Drawing.Point(240, 20);
            this.lstLog.Size = new System.Drawing.Size(340, 380);
            this.lstLog.HorizontalScrollbar = true;

            // Add controls to main panel
            this.pnlMain.Controls.AddRange(new Control[] {
                btnExportIngredients, btnImportIngredients, btnExportRecipes,
                btnImportRecipes, btnClose, lstLog
            });

            // Recipe Selection Panel
            this.pnlRecipeSelection.Location = new System.Drawing.Point(0, 0);
            this.pnlRecipeSelection.Size = new System.Drawing.Size(600, 450);
            this.pnlRecipeSelection.Visible = false;
            this.pnlRecipeSelection.BorderStyle = BorderStyle.FixedSingle;

            // Recipe selection title
            this.lblSelectRecipes.Text = "Select Recipes to Export:";
            this.lblSelectRecipes.Location = new System.Drawing.Point(20, 20);
            this.lblSelectRecipes.Size = new System.Drawing.Size(400, 20);
            this.lblSelectRecipes.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // Recipe checklist
            this.chkListRecipes.Location = new System.Drawing.Point(20, 50);
            this.chkListRecipes.Size = new System.Drawing.Size(500, 250);
            this.chkListRecipes.CheckOnClick = true;

            // Select All button
            this.btnSelectAll.Location = new System.Drawing.Point(20, 310);
            this.btnSelectAll.Size = new System.Drawing.Size(90, 30);
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.Click += (s, e) => SelectAllRecipes();

            // Select None button
            this.btnSelectNone.Location = new System.Drawing.Point(120, 310);
            this.btnSelectNone.Size = new System.Drawing.Size(90, 30);
            this.btnSelectNone.Text = "Select None";
            this.btnSelectNone.Click += (s, e) => SelectNoRecipes();

            // Export Selected button
            this.btnExportSelected.Location = new System.Drawing.Point(220, 310);
            this.btnExportSelected.Size = new System.Drawing.Size(100, 30);
            this.btnExportSelected.Text = "Export Selected";
            this.btnExportSelected.BackColor = System.Drawing.Color.LightGreen;
            this.btnExportSelected.Click += (s, e) => ExportSelectedRecipes();

            // Delete Selected button
            this.btnDeleteSelected.Location = new System.Drawing.Point(330, 310);
            this.btnDeleteSelected.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteSelected.Text = "Delete Selected";
            this.btnDeleteSelected.BackColor = System.Drawing.Color.LightCoral;
            this.btnDeleteSelected.Click += (s, e) => DeleteSelectedRecipes();

            // Back button
            var btnBack = new Button();
            btnBack.Location = new System.Drawing.Point(440, 310);
            btnBack.Size = new System.Drawing.Size(80, 30);
            btnBack.Text = "Back";
            btnBack.Click += (s, e) => ShowMainPanel();

            // Add controls to recipe selection panel
            this.pnlRecipeSelection.Controls.AddRange(new Control[] {
                lblSelectRecipes, chkListRecipes, btnSelectAll, btnSelectNone, 
                btnExportSelected, btnDeleteSelected, btnBack
            });

            // Add both panels to form
            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlRecipeSelection);

            this.ResumeLayout(false);
        }

        private void ShowMainPanel()
        {
            pnlMain.Visible = true;
            pnlRecipeSelection.Visible = false;
            ClearLog();
        }

        private void ShowRecipeSelection()
        {
            try
            {
                // Load recipes from database
                _allRecipes = DatabaseContext.GetAllRecipes();
                chkListRecipes.Items.Clear();

                if (_allRecipes == null || _allRecipes.Count == 0)
                {
                    MessageBox.Show("No recipes found to export. Please create some recipes first.", 
                        "No Recipes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Use distinct recipe names to prevent duplicates
                var distinctRecipes = _allRecipes
                    .GroupBy(r => r.Name)
                    .Select(g => g.First())
                    .OrderBy(r => r.Name)
                    .ToList();

                foreach (var recipe in distinctRecipes)
                {
                    chkListRecipes.Items.Add(new RecipeListItem(recipe), true);
                }

                pnlMain.Visible = false;
                pnlRecipeSelection.Visible = true;
                
                lblSelectRecipes.Text = $"Select Recipes to Export ({distinctRecipes.Count} recipes found):";
                Log($"Loaded {distinctRecipes.Count} recipes for export");
            }
            catch (Exception ex)
            {
                Log($"Error loading recipes: {ex.Message}");
                MessageBox.Show($"Error loading recipes: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectAllRecipes()
        {
            for (int i = 0; i < chkListRecipes.Items.Count; i++)
            {
                chkListRecipes.SetItemChecked(i, true);
            }
        }

        private void SelectNoRecipes()
        {
            for (int i = 0; i < chkListRecipes.Items.Count; i++)
            {
                chkListRecipes.SetItemChecked(i, false);
            }
        }

        private void DeleteSelectedRecipes()
        {
            var selectedItems = new List<RecipeListItem>();
            
            foreach (RecipeListItem item in chkListRecipes.CheckedItems)
            {
                selectedItems.Add(item);
            }

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one recipe to delete.", 
                    "No Recipes Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete {selectedItems.Count} recipe(s)?\n\nThis action cannot be undone!",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int deletedCount = 0;
                    int errorCount = 0;
                    
                    foreach (var item in selectedItems)
                    {
                        try
                        {
                            DatabaseContext.DeleteRecipe(item.RecipeId);
                            deletedCount++;
                            Log($"✓ Deleted: {item.DisplayName}");
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            Log($"✗ Error deleting {item.DisplayName}: {ex.Message}");
                        }
                    }
                    
                    string message = $"Successfully deleted {deletedCount} recipe(s).";
                    if (errorCount > 0)
                    {
                        message += $"\n{errorCount} recipe(s) could not be deleted.";
                    }
                    
                    MessageBox.Show(message, "Delete Complete", MessageBoxButtons.OK, 
                        errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                    
                    // Refresh the recipe list
                    ShowRecipeSelection();
                }
                catch (Exception ex)
                {
                    Log($"✗ Error during delete operation: {ex.Message}");
                    MessageBox.Show($"Error deleting recipes: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportSelectedRecipes()
        {
            var selectedItems = new List<RecipeListItem>();
            
            foreach (RecipeListItem item in chkListRecipes.CheckedItems)
            {
                selectedItems.Add(item);
            }

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one recipe to export.", 
                    "No Recipes Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select folder to save recipe CSV files";
                folderDialog.ShowNewFolderButton = true;
                
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    int successCount = 0;
                    int errorCount = 0;
                    
                    foreach (var item in selectedItems)
                    {
                        // Find the full recipe object
                        var recipe = _allRecipes.FirstOrDefault(r => r.Id == item.RecipeId);
                        if (recipe != null)
                        {
                            string safeFileName = MakeValidFileName(recipe.Name);
                            string filePath = Path.Combine(folderPath, $"{safeFileName}.csv");
                            
                            if (ExportSingleRecipeToCsv(recipe, filePath))
                            {
                                successCount++;
                                Log($"✓ Exported: {recipe.Name}");
                            }
                            else
                            {
                                errorCount++;
                                Log($"✗ Failed to export: {recipe.Name}");
                            }
                        }
                        else
                        {
                            errorCount++;
                            Log($"✗ Recipe not found: {item.DisplayName}");
                        }
                    }
                    
                    string message = $"Successfully exported {successCount} out of {selectedItems.Count} recipes to:\n{folderPath}";
                    if (errorCount > 0)
                    {
                        message += $"\n{errorCount} recipe(s) could not be exported.";
                    }
                    
                    MessageBox.Show(message, "Export Complete", MessageBoxButtons.OK, 
                        errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                    
                    if (successCount > 0)
                    {
                        ShowMainPanel();
                    }
                }
            }
        }

        private bool ExportSingleRecipeToCsv(Recipe recipe, string filePath)
        {
            try
            {
                return importExportService.ExportRecipeToCsv(recipe, filePath);
            }
            catch (Exception ex)
            {
                Log($"Error exporting recipe {recipe.Name}: {ex.Message}");
                return false;
            }
        }

        private string MakeValidFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "unnamed_recipe";
            
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            string safeName = System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
            
            // Trim to reasonable length
            if (safeName.Length > 50) safeName = safeName.Substring(0, 50);
            
            return safeName.Trim();
        }

        private void ExportIngredients()
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV Files (*.csv)|*.csv";
                    saveDialog.Title = "Export Ingredients to CSV";
                    saveDialog.FileName = $"CostChef_Ingredients_{DateTime.Now:yyyyMMdd}.csv";
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var ingredients = DatabaseContext.GetAllIngredients();
                        bool success = importExportService.ExportIngredientsToCsv(ingredients, saveDialog.FileName);
                        
                        if (success)
                        {
                            Log($"✓ Ingredients exported to: {Path.GetFileName(saveDialog.FileName)}");
                            MessageBox.Show($"Ingredients exported successfully!\n\n{saveDialog.FileName}", "Export Complete", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Export failed: {ex.Message}");
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
                            int importedCount = 0;
                            int skippedCount = 0;
                            
                            foreach (var ingredient in importedIngredients)
                            {
                                try
                                {
                                    // Check if ingredient already exists
                                    var existing = DatabaseContext.GetIngredientByName(ingredient.Name);
                                    if (existing == null)
                                    {
                                        DatabaseContext.InsertIngredient(ingredient);
                                        importedCount++;
                                    }
                                    else
                                    {
                                        skippedCount++;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log($"✗ Error importing {ingredient.Name}: {ex.Message}");
                                    skippedCount++;
                                }
                            }
                            
                            string message = $"{importedCount} ingredients imported successfully!";
                            if (skippedCount > 0)
                            {
                                message += $"\n{skippedCount} ingredients skipped (already exist or errors).";
                            }
                            
                            Log($"✓ {importedCount} ingredients imported from: {Path.GetFileName(openDialog.FileName)}");
                            MessageBox.Show(message, "Import Complete", 
                                MessageBoxButtons.OK, importedCount > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                        }
                        else
                        {
                            Log($"✗ No valid ingredients found in: {Path.GetFileName(openDialog.FileName)}");
                            MessageBox.Show("No valid ingredients found in the selected file.", "Import Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Import failed: {ex.Message}");
                MessageBox.Show($"Error importing ingredients: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportRecipesWithOptions()
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
                        // Ask user about duplicate handling
                        var duplicateOption = MessageBox.Show(
                            "How do you want to handle duplicate recipes?\n\n" +
                            "Yes: Overwrite existing recipes\n" +
                            "No: Skip duplicates (keep existing recipes)\n" +
                            "Cancel: Abort import",
                            "Duplicate Recipe Handling",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (duplicateOption == DialogResult.Cancel)
                        {
                            Log("Import cancelled by user");
                            return;
                        }

                        bool overwriteDuplicates = (duplicateOption == DialogResult.Yes);
                        int totalImported = 0;
                        int totalSkipped = 0;
                        int totalErrors = 0;

                        foreach (string filePath in openDialog.FileNames)
                        {
                            var result = importExportService.ImportRecipesWithOptions(filePath, overwriteDuplicates);
                            
                            totalImported += result.imported.Count;
                            totalSkipped += result.skipped.Count;
                            totalErrors += result.errors.Count;

                            // Log results
                            foreach (var recipe in result.imported)
                            {
                                Log($"✓ Imported: {recipe.Name}");
                            }
                            foreach (var skipped in result.skipped)
                            {
                                Log($"⏭ Skipped (duplicate): {skipped}");
                            }
                            foreach (var error in result.errors)
                            {
                                Log($"✗ Error: {error}");
                            }
                        }

                        string message = "";
                        if (totalImported > 0)
                        {
                            message += $"{totalImported} recipe(s) imported successfully!\n";
                        }
                        if (totalSkipped > 0)
                        {
                            message += $"{totalSkipped} recipe(s) skipped (duplicates).\n";
                        }
                        if (totalErrors > 0)
                        {
                            message += $"{totalErrors} recipe(s) had errors.\n";
                        }
                        if (totalImported == 0 && totalSkipped == 0 && totalErrors == 0)
                        {
                            message = "No recipes found to import.";
                        }

                        MessageBox.Show(message.Trim(), "Import Complete", 
                            MessageBoxButtons.OK, 
                            totalErrors > 0 ? MessageBoxIcon.Warning : 
                            totalImported > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Import failed: {ex.Message}");
                MessageBox.Show($"Error importing recipes: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Log(string message)
        {
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action<string>(Log), message);
                return;
            }
            
            lstLog.Items.Add($"{DateTime.Now:HH:mm:ss} - {message}");
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        private void ClearLog()
        {
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action(ClearLog));
                return;
            }
            
            lstLog.Items.Clear();
        }

        // Helper class to prevent duplicate display issues
        private class RecipeListItem
        {
            public int RecipeId { get; }
            public string DisplayName { get; }
            
            public RecipeListItem(Recipe recipe)
            {
                RecipeId = recipe.Id;
                DisplayName = $"{recipe.Name} (ID: {recipe.Id})";
            }
            
            public override string ToString() => DisplayName;
        }
    }
}