using System.Globalization;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;


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

            // Sales Price Controls
            this.lblSalesPrice = new Label(); 
            this.txtSalesPrice = new TextBox(); 

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
            this.ClientSize = new System.Drawing.Size(900, 650); 
            this.Text = "Recipe Costing Calculator";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            // Existing Recipes (Moved to the far right)
            this.lblExistingRecipes.Location = new System.Drawing.Point(550, 15);
            this.lblExistingRecipes.Size = new System.Drawing.Size(80, 20);
            this.lblExistingRecipes.Text = "Load Recipe:";
            
            this.cmbExistingRecipes.Location = new System.Drawing.Point(630, 12);
            this.cmbExistingRecipes.Size = new System.Drawing.Size(180, 20);
            this.cmbExistingRecipes.DropDownStyle = ComboBoxStyle.DropDownList;

            this.btnLoadRecipe.Location = new System.Drawing.Point(820, 10);
            this.btnLoadRecipe.Size = new System.Drawing.Size(60, 25);
            this.btnLoadRecipe.Text = "Load";
            this.btnLoadRecipe.Click += (s, e) => LoadSelectedRecipe();

            // Refresh Recipes Button (Moved position)
            this.btnRefreshRecipes.Location = new System.Drawing.Point(750, 42); 
            this.btnRefreshRecipes.Size = new System.Drawing.Size(80, 25);
            this.btnRefreshRecipes.Text = "Refresh";
            this.btnRefreshRecipes.Click += (s, e) => LoadExistingRecipes();

            // Search Controls
            this.txtSearchRecipes.Location = new System.Drawing.Point(550, 45);
            this.txtSearchRecipes.Size = new System.Drawing.Size(150, 20);
            this.txtSearchRecipes.PlaceholderText = "Search recipes...";
            this.txtSearchRecipes.KeyPress += (s, e) => 
            {
                if (e.KeyChar == (char)Keys.Enter)
                    SearchRecipes();
            };

            this.btnSearchRecipes.Location = new System.Drawing.Point(700, 42);
            this.btnSearchRecipes.Size = new System.Drawing.Size(50, 25);
            this.btnSearchRecipes.Text = "Search";
            this.btnSearchRecipes.Click += (s, e) => SearchRecipes();

            this.btnClearSearch.Location = new System.Drawing.Point(830, 42); 
            this.btnClearSearch.Size = new System.Drawing.Size(60, 25);
            this.btnClearSearch.Text = "Clear";
            this.btnClearSearch.Click += (s, e) => ClearSearch();

            // Recipe Name
            this.lblRecipeName.Location = new System.Drawing.Point(12, 15);
            this.lblRecipeName.Size = new System.Drawing.Size(100, 20);
            this.lblRecipeName.Text = "Recipe Name:";
            
            this.txtRecipeName.Location = new System.Drawing.Point(120, 12);
            this.txtRecipeName.Size = new System.Drawing.Size(300, 20);
            this.txtRecipeName.Text = "New Recipe";
            this.txtRecipeName.TextChanged += (s, e) => 
            {
                currentRecipe.Name = txtRecipeName.Text;
                CheckRecipeNameAvailability();
            };

            // Sales Price (IMPROVED POSITIONING)
            this.lblSalesPrice.Location = new System.Drawing.Point(12, 45);
            this.lblSalesPrice.Size = new System.Drawing.Size(100, 20);
            this.lblSalesPrice.Text = "Sales Price:";

            this.txtSalesPrice.Location = new System.Drawing.Point(120, 42);
            this.txtSalesPrice.Size = new System.Drawing.Size(100, 20);
            this.txtSalesPrice.Text = "0.00";
            this.txtSalesPrice.TextChanged += (s, e) => 
            {
                if (decimal.TryParse(txtSalesPrice.Text, out decimal price) && price >= 0)
                {
                    currentRecipe.SalesPrice = price;
                    CalculateCost(); // Recalculate to show updated profitability
                }
                else
                {
                    // Reset to previous valid value or 0
                    txtSalesPrice.Text = currentRecipe.SalesPrice.ToString("F2");
                }
            };
            
            // Batch Yield (ADJUSTED POSITION)
            this.lblBatchYield.Location = new System.Drawing.Point(240, 45); 
            this.lblBatchYield.Size = new System.Drawing.Size(100, 20);
            this.lblBatchYield.Text = "Batch Yield:";
            
            this.txtBatchYield.Location = new System.Drawing.Point(340, 42); 
            this.txtBatchYield.Size = new System.Drawing.Size(100, 20);
            this.txtBatchYield.Text = "1";
            this.txtBatchYield.TextChanged += (s, e) => 
            {
                if (int.TryParse(txtBatchYield.Text, out int yield))
                {
                    var oldYield = currentRecipe.BatchYield;
                    currentRecipe.BatchYield = yield;
                    
                    if (currentRecipe.Id > 0 && oldYield != yield)
                    {
                        CreateVersionForChange($"Batch yield changed from {oldYield} to {yield}");
                    }
                }
            };

            // Food Cost Target (ADJUSTED POSITION)
            this.lblFoodCost.Location = new System.Drawing.Point(460, 45); 
            this.lblFoodCost.Size = new System.Drawing.Size(100, 20);
            this.lblFoodCost.Text = "Target FC %:"; 
            
            this.cmbFoodCost.Location = new System.Drawing.Point(560, 42); 
            this.cmbFoodCost.Size = new System.Drawing.Size(80, 20);
            
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
                        
                        if (currentRecipe.Id > 0 && Math.Abs(oldPercentage - currentRecipe.TargetFoodCostPercentage) > 0.01m)
                        {
                            CreateVersionForChange($"Food cost target changed from {oldPercentage*100:0}% to {foodCost}%");
                        }
                    }
                }
            };

            // Ingredients ComboBox (Adjusted Y position)
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
            this.dataGridViewIngredients.Size = new System.Drawing.Size(876, 250); 
            this.dataGridViewIngredients.ReadOnly = false;
            this.dataGridViewIngredients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewIngredients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewIngredients.CellEndEdit += new DataGridViewCellEventHandler(this.dataGridViewIngredients_CellEndEdit);

            // Category Label
            this.lblCategory.Location = new System.Drawing.Point(12, 370); 
            this.lblCategory.Size = new System.Drawing.Size(100, 20);
            this.lblCategory.Text = "Category:";
            
            // Category ComboBox
            this.cmbCategory.Location = new System.Drawing.Point(120, 367); 
            this.cmbCategory.Size = new System.Drawing.Size(150, 20);
            this.cmbCategory.DropDownStyle = ComboBoxStyle.DropDown;
            this.cmbCategory.TextChanged += (s, e) => currentRecipe.Category = cmbCategory.Text;

            // New Category TextBox (for adding new categories)
            this.txtNewCategory.Location = new System.Drawing.Point(280, 367); 
            this.txtNewCategory.Size = new System.Drawing.Size(120, 20);
            this.txtNewCategory.PlaceholderText = "New category...";
            this.txtNewCategory.Visible = false;

            // Manage Categories Button
            this.btnManageCategories.Location = new System.Drawing.Point(410, 365); 
            this.btnManageCategories.Size = new System.Drawing.Size(120, 25);
            this.btnManageCategories.Text = "Manage Categories";
            this.btnManageCategories.Click += (s, e) => ShowCategoryManager();

            // Tags Label
            this.lblTags.Location = new System.Drawing.Point(12, 400); 
            this.lblTags.Size = new System.Drawing.Size(100, 20);
            this.lblTags.Text = "Tags:";
            
            // Tags TextBox
            this.txtTags.Location = new System.Drawing.Point(120, 397); 
            this.txtTags.Size = new System.Drawing.Size(300, 20);
            this.txtTags.PlaceholderText = "comma, separated, tags";
            this.txtTags.TextChanged += (s, e) => UpdateRecipeTags();

            // Cost Summary
            this.lblCostSummary.Location = new System.Drawing.Point(12, 430); 
            this.lblCostSummary.Size = new System.Drawing.Size(876, 100); 
            this.lblCostSummary.Text = "Add ingredients to calculate cost...";
            this.lblCostSummary.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblCostSummary.BorderStyle = BorderStyle.FixedSingle;
            this.lblCostSummary.Font = new System.Drawing.Font("Consolas", 9);

            // Calculate Button
            this.btnCalculate.Location = new System.Drawing.Point(12, 545); 
            this.btnCalculate.Size = new System.Drawing.Size(100, 30);
            this.btnCalculate.Text = "Calculate";
            this.btnCalculate.Click += (s, e) => CalculateCost();

            // Save Recipe Button
            this.btnSaveRecipe.Location = new System.Drawing.Point(122, 545); 
            this.btnSaveRecipe.Size = new System.Drawing.Size(100, 30);
            this.btnSaveRecipe.Text = "Save Recipe";
            this.btnSaveRecipe.Click += (s, e) => SaveRecipe();

            // Delete Recipe Button
            this.btnDeleteRecipe.Location = new System.Drawing.Point(232, 545); 
            this.btnDeleteRecipe.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteRecipe.Text = "Delete Recipe";
            this.btnDeleteRecipe.Click += (s, e) => DeleteRecipe();

            // Version History Button
            this.btnVersionHistory.Location = new System.Drawing.Point(342, 545); 
            this.btnVersionHistory.Size = new System.Drawing.Size(120, 30);
            this.btnVersionHistory.Text = "Version History";
            this.btnVersionHistory.Click += (s, e) => ShowVersionHistory();
            this.btnVersionHistory.Enabled = false; // Disabled until a recipe is loaded

            // Close Button
            this.btnClose.Location = new System.Drawing.Point(828, 545); 
            this.btnClose.Size = new System.Drawing.Size(60, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            // Add all controls
            this.Controls.AddRange(new Control[] {
                lblRecipeName, txtRecipeName, lblSalesPrice, txtSalesPrice, // Sales Price now present
                lblBatchYield, txtBatchYield, lblFoodCost, cmbFoodCost, lblExistingRecipes, cmbExistingRecipes,
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
        // Determine current currency symbol from settings
        string currencySymbol = AppSettings.CurrencySymbol ?? "$";

        // Build a NumberFormatInfo that uses our app's currency symbol
        var currencyFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        currencyFormat.CurrencySymbol = currencySymbol;

        // Clear existing columns
        dataGridViewIngredients.Columns.Clear();

        // Ingredient name
        var colIngredientName = new DataGridViewTextBoxColumn
        {
            Name = "IngredientName",
            HeaderText = "Ingredient",
            DataPropertyName = "IngredientName",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        };

        // Quantity
        var colQuantity = new DataGridViewTextBoxColumn
        {
            Name = "Quantity",
            HeaderText = "Quantity",
            DataPropertyName = "Quantity",
            ReadOnly = false
        };

        // Unit  ➜ right-aligned
        var colUnit = new DataGridViewTextBoxColumn
        {
            Name = "Unit",
            HeaderText = "Unit",
            DataPropertyName = "Unit",
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        };

        // Unit price – currency formatted with app symbol, 2 decimals, right-aligned
        var colUnitPrice = new DataGridViewTextBoxColumn
        {
            Name = "UnitPrice",
            HeaderText = $"Unit Price ({currencySymbol})",
            DataPropertyName = "UnitPrice",
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "C2",                  // was "C4"
                FormatProvider = currencyFormat,
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        };

        // Line cost – currency formatted with app symbol
        var colLineCost = new DataGridViewTextBoxColumn
        {
            Name = "LineCost",
            HeaderText = "Line Cost",
            DataPropertyName = "LineCost",
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "C2",
                FormatProvider = currencyFormat,
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        };

        // Supplier
        var colSupplier = new DataGridViewTextBoxColumn
        {
            Name = "Supplier",
            HeaderText = "Supplier",
            DataPropertyName = "Supplier",
            ReadOnly = true
        };

        dataGridViewIngredients.Columns.AddRange(new DataGridViewColumn[]
        {
            colIngredientName,
            colQuantity,
            colUnit,
            colUnitPrice,
            colLineCost,
            colSupplier
        });

        dataGridViewIngredients.AutoGenerateColumns = false;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error configuring ingredients grid: {ex.Message}", "Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}



    }
}