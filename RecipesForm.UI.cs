using System;
using System.Windows.Forms;
using System.Drawing;

namespace CostChef
{
    public partial class RecipesForm : Form
    {
        private void InitializeComponent()
        {
            this.dataGridViewIngredients = new DataGridView();
            this.btnAddIngredient = new Button();
            this.btnRemoveIngredient = new Button();
            this.btnCalculate = new Button();
            this.btnClose = new Button();
            this.btnSaveRecipe = new Button();
            this.btnLoadRecipe = new Button();
            this.btnDeleteRecipe = new Button();
            this.lblCostSummary = new Label();
            this.cmbIngredients = new ComboBox();
            this.txtQuantity = new TextBox();
            this.lblRecipeName = new Label();
            this.txtRecipeName = new TextBox();
            this.lblBatchYield = new Label();
            this.txtBatchYield = new TextBox();
            this.lblFoodCost = new Label();
            this.cmbFoodCost = new ComboBox();
            this.cmbExistingRecipes = new ComboBox();
            this.lblExistingRecipes = new Label();
            this.btnRefreshRecipes = new Button();
            this.txtSearchRecipes = new TextBox();
            this.btnSearchRecipes = new Button();
            this.btnClearSearch = new Button();

            // Category and Tags Controls
            this.lblCategory = new Label();
            this.cmbCategory = new ComboBox();
            this.txtNewCategory = new TextBox();
            this.lblTags = new Label();
            this.txtTags = new TextBox();
            this.btnManageCategories = new Button();

            // NEW: Unit display label
            this.lblUnitDisplay = new Label();

            // Version History button
            this.btnVersionHistory = new Button();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Recipe Costing Calculator";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            // Existing Recipes
            this.lblExistingRecipes.Location = new System.Drawing.Point(450, 15);
            this.lblExistingRecipes.Size = new System.Drawing.Size(100, 20);
            this.lblExistingRecipes.Text = "Load Recipe:";
            
            this.cmbExistingRecipes.Location = new System.Drawing.Point(550, 12);
            this.cmbExistingRecipes.Size = new System.Drawing.Size(150, 20);
            this.cmbExistingRecipes.DropDownStyle = ComboBoxStyle.DropDownList;

            this.btnLoadRecipe.Location = new System.Drawing.Point(710, 10);
            this.btnLoadRecipe.Size = new System.Drawing.Size(80, 25);
            this.btnLoadRecipe.Text = "Load";
            this.btnLoadRecipe.Click += (s, e) => LoadSelectedRecipe();

            // Refresh Recipes Button
            this.btnRefreshRecipes.Location = new System.Drawing.Point(620, 10);
            this.btnRefreshRecipes.Size = new System.Drawing.Size(80, 25);
            this.btnRefreshRecipes.Text = "Refresh";
            this.btnRefreshRecipes.Click += (s, e) => LoadExistingRecipes();

            // Search Controls
            this.txtSearchRecipes.Location = new System.Drawing.Point(450, 45);
            this.txtSearchRecipes.Size = new System.Drawing.Size(150, 20);
            this.txtSearchRecipes.PlaceholderText = "Search recipes...";
            this.txtSearchRecipes.KeyPress += (s, e) => 
            {
                if (e.KeyChar == (char)Keys.Enter)
                    SearchRecipes();
            };

            this.btnSearchRecipes.Location = new System.Drawing.Point(610, 42);
            this.btnSearchRecipes.Size = new System.Drawing.Size(60, 25);
            this.btnSearchRecipes.Text = "Search";
            this.btnSearchRecipes.Click += (s, e) => SearchRecipes();

            this.btnClearSearch.Location = new System.Drawing.Point(680, 42);
            this.btnClearSearch.Size = new System.Drawing.Size(60, 25);
            this.btnClearSearch.Text = "Clear";
            this.btnClearSearch.Click += (s, e) => ClearSearch();

            // Recipe Name
            this.lblRecipeName.Location = new System.Drawing.Point(12, 15);
            this.lblRecipeName.Size = new System.Drawing.Size(100, 20);
            this.lblRecipeName.Text = "Recipe Name:";
            
            this.txtRecipeName.Location = new System.Drawing.Point(120, 12);
            this.txtRecipeName.Size = new System.Drawing.Size(200, 20);
            this.txtRecipeName.Text = "New Recipe";
            this.txtRecipeName.TextChanged += (s, e) => 
            {
                currentRecipe.Name = txtRecipeName.Text;
                CheckRecipeNameAvailability();
            };

            // Batch Yield
            this.lblBatchYield.Location = new System.Drawing.Point(12, 45);
            this.lblBatchYield.Size = new System.Drawing.Size(100, 20);
            this.lblBatchYield.Text = "Batch Yield:";
            
            this.txtBatchYield.Location = new System.Drawing.Point(120, 42);
            this.txtBatchYield.Size = new System.Drawing.Size(100, 20);
            this.txtBatchYield.Text = "1";
            this.txtBatchYield.TextChanged += (s, e) => 
            {
                if (int.TryParse(txtBatchYield.Text, out int yield))
                {
                    var oldYield = currentRecipe.BatchYield;
                    currentRecipe.BatchYield = yield;
                    
                    // Create version when batch yield changes significantly
                    if (currentRecipe.Id > 0 && oldYield != yield)
                    {
                        CreateVersionForChange($"Batch yield changed from {oldYield} to {yield}");
                    }
                }
            };

            // Food Cost Target
            this.lblFoodCost.Location = new System.Drawing.Point(240, 45);
            this.lblFoodCost.Size = new System.Drawing.Size(100, 20);
            this.lblFoodCost.Text = "Food Cost %:";
            
            this.cmbFoodCost.Location = new System.Drawing.Point(340, 42);
            this.cmbFoodCost.Size = new System.Drawing.Size(100, 20);
// In RecipesForm.cs - update the cmbFoodCost setup:
this.cmbFoodCost.Items.AddRange(new object[] { "25%", "30%", "35%", "40%" });
this.cmbFoodCost.SelectedIndex = 1; // Default to 30%
            this.cmbFoodCost.SelectedIndexChanged += (s, e) => 
            {
                if (cmbFoodCost.SelectedItem != null)
                {
                    var percent = cmbFoodCost.SelectedItem.ToString().Replace("%", "");
                    if (decimal.TryParse(percent, out decimal foodCost))
                    {
                        var oldPercentage = currentRecipe.TargetFoodCostPercentage;
                        currentRecipe.TargetFoodCostPercentage = foodCost / 100m;
                        
                        // Create version when food cost % changes significantly
                        if (currentRecipe.Id > 0 && Math.Abs(oldPercentage - currentRecipe.TargetFoodCostPercentage) > 0.01m)
                        {
                            CreateVersionForChange($"Food cost target changed from {oldPercentage*100:0}% to {foodCost}%");
                        }
                    }
                }
            };

            // Ingredients ComboBox
            this.cmbIngredients.Location = new System.Drawing.Point(12, 75);
            this.cmbIngredients.Size = new System.Drawing.Size(200, 20);
            this.cmbIngredients.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbIngredients.SelectedIndexChanged += (s, e) => UpdateUnitDisplay();

            // Unit Display Label
            this.lblUnitDisplay.Location = new System.Drawing.Point(220, 75);
            this.lblUnitDisplay.Size = new System.Drawing.Size(60, 20);
            this.lblUnitDisplay.Text = "grams";
            this.lblUnitDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblUnitDisplay.BorderStyle = BorderStyle.FixedSingle;

            // Quantity
            this.txtQuantity.Location = new System.Drawing.Point(290, 75);
            this.txtQuantity.Size = new System.Drawing.Size(80, 20);
            this.txtQuantity.Text = "100";
            this.txtQuantity.PlaceholderText = "Quantity";

            // Add Ingredient Button
            this.btnAddIngredient.Location = new System.Drawing.Point(380, 73);
            this.btnAddIngredient.Size = new System.Drawing.Size(80, 25);
            this.btnAddIngredient.Text = "Add";
            this.btnAddIngredient.Click += (s, e) => AddIngredientToRecipe();

            // Remove Ingredient Button
            this.btnRemoveIngredient.Location = new System.Drawing.Point(470, 73);
            this.btnRemoveIngredient.Size = new System.Drawing.Size(80, 25);
            this.btnRemoveIngredient.Text = "Remove";
            this.btnRemoveIngredient.Click += (s, e) => RemoveIngredient();

            // Ingredients DataGrid
            this.dataGridViewIngredients.Location = new System.Drawing.Point(12, 105);
            this.dataGridViewIngredients.Size = new System.Drawing.Size(776, 200);
            this.dataGridViewIngredients.ReadOnly = false;
            this.dataGridViewIngredients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewIngredients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewIngredients.CellEndEdit += new DataGridViewCellEventHandler(this.dataGridViewIngredients_CellEndEdit);

            // Category Label
            this.lblCategory.Location = new System.Drawing.Point(12, 320);
            this.lblCategory.Size = new System.Drawing.Size(100, 20);
            this.lblCategory.Text = "Category:";
            
            // Category ComboBox
            this.cmbCategory.Location = new System.Drawing.Point(120, 317);
            this.cmbCategory.Size = new System.Drawing.Size(150, 20);
            this.cmbCategory.DropDownStyle = ComboBoxStyle.DropDown;
            this.cmbCategory.TextChanged += (s, e) => currentRecipe.Category = cmbCategory.Text;

            // New Category TextBox (for adding new categories)
            this.txtNewCategory.Location = new System.Drawing.Point(280, 317);
            this.txtNewCategory.Size = new System.Drawing.Size(120, 20);
            this.txtNewCategory.PlaceholderText = "New category...";
            this.txtNewCategory.Visible = false;

            // Manage Categories Button
            this.btnManageCategories.Location = new System.Drawing.Point(410, 315);
            this.btnManageCategories.Size = new System.Drawing.Size(120, 25);
            this.btnManageCategories.Text = "Manage Categories";
            this.btnManageCategories.Click += (s, e) => ShowCategoryManager();

            // Tags Label
            this.lblTags.Location = new System.Drawing.Point(12, 350);
            this.lblTags.Size = new System.Drawing.Size(100, 20);
            this.lblTags.Text = "Tags:";
            
            // Tags TextBox
            this.txtTags.Location = new System.Drawing.Point(120, 347);
            this.txtTags.Size = new System.Drawing.Size(300, 20);
            this.txtTags.PlaceholderText = "comma, separated, tags";
            this.txtTags.TextChanged += (s, e) => UpdateRecipeTags();

            // Cost Summary
            this.lblCostSummary.Location = new System.Drawing.Point(12, 380);
            this.lblCostSummary.Size = new System.Drawing.Size(776, 80);
            this.lblCostSummary.Text = "Add ingredients to calculate cost...";
            this.lblCostSummary.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblCostSummary.BorderStyle = BorderStyle.FixedSingle;
            this.lblCostSummary.Font = new System.Drawing.Font("Consolas", 9);

            // Calculate Button
            this.btnCalculate.Location = new System.Drawing.Point(12, 470);
            this.btnCalculate.Size = new System.Drawing.Size(100, 30);
            this.btnCalculate.Text = "Calculate";
            this.btnCalculate.Click += (s, e) => CalculateCost();

            // Save Recipe Button
            this.btnSaveRecipe.Location = new System.Drawing.Point(122, 470);
            this.btnSaveRecipe.Size = new System.Drawing.Size(100, 30);
            this.btnSaveRecipe.Text = "Save Recipe";
            this.btnSaveRecipe.Click += (s, e) => SaveRecipe();

            // Delete Recipe Button
            this.btnDeleteRecipe.Location = new System.Drawing.Point(232, 470);
            this.btnDeleteRecipe.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteRecipe.Text = "Delete Recipe";
            this.btnDeleteRecipe.Click += (s, e) => DeleteRecipe();

            // Version History Button
            this.btnVersionHistory.Location = new System.Drawing.Point(342, 470);
            this.btnVersionHistory.Size = new System.Drawing.Size(120, 30);
            this.btnVersionHistory.Text = "Version History";
            this.btnVersionHistory.Click += (s, e) => ShowVersionHistory();
            this.btnVersionHistory.Enabled = false; // Disabled until a recipe is loaded

            // Close Button
            this.btnClose.Location = new System.Drawing.Point(688, 470);
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            // Add all controls
            this.Controls.AddRange(new Control[] {
                lblRecipeName, txtRecipeName, lblBatchYield, txtBatchYield,
                lblFoodCost, cmbFoodCost, lblExistingRecipes, cmbExistingRecipes,
                btnLoadRecipe, btnRefreshRecipes, txtSearchRecipes, btnSearchRecipes, btnClearSearch,
                cmbIngredients, lblUnitDisplay, txtQuantity, btnAddIngredient, 
                btnRemoveIngredient, dataGridViewIngredients, 
                lblCategory, cmbCategory, txtNewCategory, lblTags, txtTags, btnManageCategories,
                lblCostSummary, btnCalculate, btnSaveRecipe, btnDeleteRecipe, btnVersionHistory, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ConfigureIngredientsGrid()
        {
            try
            {
                // Clear existing columns
                dataGridViewIngredients.Columns.Clear();
                
                // Add columns manually for better control
                var columns = new[]
                {
                    new DataGridViewTextBoxColumn { Name = "IngredientName", HeaderText = "Ingredient", DataPropertyName = "IngredientName", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                    new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Quantity", DataPropertyName = "Quantity", ReadOnly = false },
                    new DataGridViewTextBoxColumn { Name = "Unit", HeaderText = "Unit", DataPropertyName = "Unit", ReadOnly = true },
                    new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "Unit Price", DataPropertyName = "UnitPrice", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = $"{currencySymbol}0.0000" } },
                    new DataGridViewTextBoxColumn { Name = "LineCost", HeaderText = "Line Cost", DataPropertyName = "LineCost", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = $"{currencySymbol}0.00" } },
                    new DataGridViewTextBoxColumn { Name = "Supplier", HeaderText = "Supplier", DataPropertyName = "Supplier", ReadOnly = true }
                };
                
                dataGridViewIngredients.Columns.AddRange(columns);
                
                // Hide ID columns - they'll be in the data source but not visible
                dataGridViewIngredients.AutoGenerateColumns = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error configuring ingredients grid: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateUnitDisplay()
        {
            try
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
            catch (Exception ex)
            {
                lblUnitDisplay.Text = "grams";
            }
        }
    }
}