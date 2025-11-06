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

        // Recipe selection controls
        private CheckedListBox chkListRecipes;
        private Button btnSelectAll;
        private Button btnSelectNone;
        private Button btnExportSelected;
        private Button btnDeleteSelected; // NEW BUTTON
        private Panel pnlRecipeSelection;
        private Panel pnlMain;
        private Label lblSelectRecipes;

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
            this.btnDeleteSelected = new Button(); // NEW BUTTON
            this.pnlRecipeSelection = new Panel();
            this.pnlMain = new Panel();
            this.lblSelectRecipes = new Label();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(600, 450);
            this.Text = "Import/Export";
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
            this.btnImportRecipes.Click += (s, e) => ImportRecipes();

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
            this.lblSelectRecipes.Size = new System.Drawing.Size(200, 20);
            this.lblSelectRecipes.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // Recipe checklist
            this.chkListRecipes.Location = new System.Drawing.Point(20, 50);
            this.chkListRecipes.Size = new System.Drawing.Size(400, 250);
            this.chkListRecipes.CheckOnClick = true;

            // Select All button
            this.btnSelectAll.Location = new System.Drawing.Point(20, 310);
            this.btnSelectAll.Size = new System.Drawing.Size(80, 30);
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.Click += (s, e) => SelectAllRecipes();

            // Select None button
            this.btnSelectNone.Location = new System.Drawing.Point(110, 310);
            this.btnSelectNone.Size = new System.Drawing.Size(80, 30);
            this.btnSelectNone.Text = "Select None";
            this.btnSelectNone.Click += (s, e) => SelectNoRecipes();

            // Export Selected button
            this.btnExportSelected.Location = new System.Drawing.Point(200, 310);
            this.btnExportSelected.Size = new System.Drawing.Size(100, 30);
            this.btnExportSelected.Text = "Export Selected";
            this.btnExportSelected.BackColor = System.Drawing.Color.LightGreen;
            this.btnExportSelected.Click += (s, e) => ExportSelectedRecipes();

            // DELETE SELECTED BUTTON - NEW
            this.btnDeleteSelected.Location = new System.Drawing.Point(310, 310);
            this.btnDeleteSelected.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteSelected.Text = "Delete Selected";
            this.btnDeleteSelected.BackColor = System.Drawing.Color.LightCoral;
            this.btnDeleteSelected.Click += (s, e) => DeleteSelectedRecipes();

            // Back button
            var btnBack = new Button();
            btnBack.Location = new System.Drawing.Point(420, 310);
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
        }

        private void ShowRecipeSelection()
        {
            try
            {
                // Load recipes from database
                var recipes = DatabaseContext.GetAllRecipes();
                chkListRecipes.Items.Clear();

                if (recipes == null || recipes.Count == 0)
                {
                    MessageBox.Show("No recipes found to export. Please create some recipes first.", 
                        "No Recipes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (var recipe in recipes)
                {
                    chkListRecipes.Items.Add(recipe.Name, true);
                }

                pnlMain.Visible = false;
                pnlRecipeSelection.Visible = true;
                
                lblSelectRecipes.Text = $"Select Recipes to Export ({recipes.Count} recipes found):";
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

        // NEW METHOD: Delete selected recipes
        private void DeleteSelectedRecipes()
        {
            var selectedRecipeNames = new List<string>();
            
            foreach (string recipeName in chkListRecipes.CheckedItems)
            {
                selectedRecipeNames.Add(recipeName);
            }

            if (selectedRecipeNames.Count == 0)
            {
                MessageBox.Show("Please select at least one recipe to delete.", 
                    "No Recipes Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete {selectedRecipeNames.Count} recipe(s)?\n\nThis action cannot be undone!",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Get the actual recipe objects from database
                    var allRecipes = DatabaseContext.GetAllRecipes();
                    int deletedCount = 0;
                    
                    foreach (var recipeName in selectedRecipeNames)
                    {
                        var recipe = allRecipes.Find(r => r.Name == recipeName);
                        if (recipe != null)
                        {
                            DatabaseContext.DeleteRecipe(recipe.Id);
                            deletedCount++;
                            Log($"✓ Deleted: {recipeName}");
                        }
                    }
                    
                    MessageBox.Show($"Successfully deleted {deletedCount} recipe(s).", 
                        "Delete Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Refresh the recipe list
                    ShowRecipeSelection();
                }
                catch (Exception ex)
                {
                    Log($"✗ Error deleting recipes: {ex.Message}");
                    MessageBox.Show($"Error deleting recipes: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportSelectedRecipes()
        {
            var selectedRecipeNames = new List<string>();
            
            foreach (string recipeName in chkListRecipes.CheckedItems)
            {
                selectedRecipeNames.Add(recipeName);
            }

            if (selectedRecipeNames.Count == 0)
            {
                MessageBox.Show("Please select at least one recipe to export.", 
                    "No Recipes Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the actual recipe objects from database
            var allRecipes = DatabaseContext.GetAllRecipes();
            var selectedRecipes = new List<Recipe>();
            
            foreach (var recipeName in selectedRecipeNames)
            {
                var recipe = allRecipes.Find(r => r.Name == recipeName);
                if (recipe != null)
                {
                    selectedRecipes.Add(recipe);
                }
            }

            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select folder to save recipe CSV files";
                folderDialog.ShowNewFolderButton = true;
                
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    int successCount = 0;
                    
                    foreach (var recipe in selectedRecipes)
                    {
                        // Create a safe filename from the recipe name
                        string safeFileName = MakeValidFileName(recipe.Name);
                        string filePath = Path.Combine(folderPath, $"{safeFileName}.csv");
                        
                        if (ExportSingleRecipeToCsv(recipe, filePath))
                        {
                            successCount++;
                            Log($"✓ Exported: {recipe.Name}");
                        }
                        else
                        {
                            Log($"✗ Failed to export: {recipe.Name}");
                        }
                    }
                    
                    MessageBox.Show($"Successfully exported {successCount} out of {selectedRecipes.Count} recipes to:\n{folderPath}", 
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ShowMainPanel();
                }
            }
        }

        private bool ExportSingleRecipeToCsv(Recipe recipe, string filePath)
        {
            try
            {
                // Use the existing ImportExportService to export a single recipe
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
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
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
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action<string>(Log), message);
                return;
            }
            
            lstLog.Items.Add($"{DateTime.Now:HH:mm:ss} - {message}");
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }
    }
}