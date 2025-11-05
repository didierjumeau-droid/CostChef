using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace CostChef
{
    public partial class RecipesForm : Form
    {
        private DataGridView dataGridViewIngredients;
        private Button btnAddIngredient;
        private Button btnRemoveIngredient;
        private Button btnCalculate;
        private Button btnClose;
        private Button btnSaveRecipe;
        private Button btnLoadRecipe;
        private Button btnDeleteRecipe;
        private Label lblCostSummary;
        private ComboBox cmbIngredients;
        private TextBox txtQuantity;
        private Label lblRecipeName;
        private TextBox txtRecipeName;
        private Label lblBatchYield;
        private TextBox txtBatchYield;
        private Label lblFoodCost;
        private ComboBox cmbFoodCost;
        private ComboBox cmbExistingRecipes;
        private Label lblExistingRecipes;

        private Recipe currentRecipe = new Recipe();
        private List<RecipeIngredient> currentIngredients = new List<RecipeIngredient>();
        private List<Recipe> allRecipes = new List<Recipe>();

        public RecipesForm()
        {
            InitializeComponent();
            LoadIngredientsComboBox();
            LoadExistingRecipes();
            InitializeNewRecipe();
        }

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

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Recipe Costing Calculator";
            this.StartPosition = FormStartPosition.CenterParent;

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

            // Recipe Name
            this.lblRecipeName.Location = new System.Drawing.Point(12, 15);
            this.lblRecipeName.Size = new System.Drawing.Size(100, 20);
            this.lblRecipeName.Text = "Recipe Name:";
            
            this.txtRecipeName.Location = new System.Drawing.Point(120, 12);
            this.txtRecipeName.Size = new System.Drawing.Size(200, 20);
            this.txtRecipeName.Text = "New Recipe";
            this.txtRecipeName.TextChanged += (s, e) => currentRecipe.Name = txtRecipeName.Text;

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
                    currentRecipe.BatchYield = yield;
            };

            // Food Cost Target
            this.lblFoodCost.Location = new System.Drawing.Point(240, 45);
            this.lblFoodCost.Size = new System.Drawing.Size(100, 20);
            this.lblFoodCost.Text = "Food Cost %:";
            
            this.cmbFoodCost.Location = new System.Drawing.Point(340, 42);
            this.cmbFoodCost.Size = new System.Drawing.Size(100, 20);
            this.cmbFoodCost.Items.AddRange(new object[] { "25%", "30%", "35%" });
            this.cmbFoodCost.SelectedIndex = 1; // Default to 30%
            this.cmbFoodCost.SelectedIndexChanged += (s, e) => 
            {
                if (cmbFoodCost.SelectedItem != null)
                {
                    var percent = cmbFoodCost.SelectedItem.ToString().Replace("%", "");
                    if (decimal.TryParse(percent, out decimal foodCost))
                        currentRecipe.TargetFoodCostPercentage = foodCost / 100m;
                }
            };

            // Ingredients ComboBox
            this.cmbIngredients.Location = new System.Drawing.Point(12, 75);
            this.cmbIngredients.Size = new System.Drawing.Size(250, 20);
            this.cmbIngredients.DropDownStyle = ComboBoxStyle.DropDownList;

            // Quantity
            this.txtQuantity.Location = new System.Drawing.Point(270, 75);
            this.txtQuantity.Size = new System.Drawing.Size(80, 20);
            this.txtQuantity.Text = "100";
            this.txtQuantity.PlaceholderText = "Quantity";

            // Add Ingredient Button
            this.btnAddIngredient.Location = new System.Drawing.Point(360, 73);
            this.btnAddIngredient.Size = new System.Drawing.Size(80, 25);
            this.btnAddIngredient.Text = "Add";
            this.btnAddIngredient.Click += (s, e) => AddIngredientToRecipe();

            // Remove Ingredient Button
            this.btnRemoveIngredient.Location = new System.Drawing.Point(450, 73);
            this.btnRemoveIngredient.Size = new System.Drawing.Size(80, 25);
            this.btnRemoveIngredient.Text = "Remove";
            this.btnRemoveIngredient.Click += (s, e) => RemoveIngredient();

            // Ingredients DataGrid
            this.dataGridViewIngredients.Location = new System.Drawing.Point(12, 105);
            this.dataGridViewIngredients.Size = new System.Drawing.Size(776, 200);
            this.dataGridViewIngredients.ReadOnly = true;
            this.dataGridViewIngredients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewIngredients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Cost Summary
            this.lblCostSummary.Location = new System.Drawing.Point(12, 315);
            this.lblCostSummary.Size = new System.Drawing.Size(776, 40);
            this.lblCostSummary.Text = "Add ingredients to calculate cost...";
            this.lblCostSummary.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblCostSummary.BorderStyle = BorderStyle.FixedSingle;

            // Calculate Button
            this.btnCalculate.Location = new System.Drawing.Point(12, 370);
            this.btnCalculate.Size = new System.Drawing.Size(100, 30);
            this.btnCalculate.Text = "Calculate";
            this.btnCalculate.Click += (s, e) => CalculateCost();

            // Save Recipe Button
            this.btnSaveRecipe.Location = new System.Drawing.Point(122, 370);
            this.btnSaveRecipe.Size = new System.Drawing.Size(100, 30);
            this.btnSaveRecipe.Text = "Save Recipe";
            this.btnSaveRecipe.Click += (s, e) => SaveRecipe();

            // Delete Recipe Button
            this.btnDeleteRecipe.Location = new System.Drawing.Point(232, 370);
            this.btnDeleteRecipe.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteRecipe.Text = "Delete Recipe";
            this.btnDeleteRecipe.Click += (s, e) => DeleteRecipe();

            // Close Button
            this.btnClose.Location = new System.Drawing.Point(688, 370);
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            // Add all controls
            this.Controls.AddRange(new Control[] {
                lblRecipeName, txtRecipeName, lblBatchYield, txtBatchYield,
                lblFoodCost, cmbFoodCost, lblExistingRecipes, cmbExistingRecipes,
                btnLoadRecipe, cmbIngredients, txtQuantity, btnAddIngredient, 
                btnRemoveIngredient, dataGridViewIngredients, lblCostSummary, 
                btnCalculate, btnSaveRecipe, btnDeleteRecipe, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadExistingRecipes()
        {
            allRecipes = DatabaseContext.GetAllRecipes();
            cmbExistingRecipes.DataSource = allRecipes;
            cmbExistingRecipes.DisplayMember = "Name";
            cmbExistingRecipes.ValueMember = "Id";
        }

        private void LoadSelectedRecipe()
        {
            if (cmbExistingRecipes.SelectedItem is Recipe selectedRecipe)
            {
                currentRecipe = selectedRecipe;
                currentIngredients = selectedRecipe.Ingredients;

                // Update UI
                txtRecipeName.Text = currentRecipe.Name;
                txtBatchYield.Text = currentRecipe.BatchYield.ToString();
                cmbFoodCost.SelectedItem = $"{currentRecipe.TargetFoodCostPercentage * 100}%";
                
                RefreshIngredientsGrid();
                CalculateCost();
            }
        }

        private void LoadIngredientsComboBox()
        {
            var ingredients = DatabaseContext.GetAllIngredients();
            cmbIngredients.DataSource = ingredients;
            cmbIngredients.DisplayMember = "Name";
            cmbIngredients.ValueMember = "Id";
        }

        private void InitializeNewRecipe()
        {
            currentRecipe = new Recipe { 
                Name = "New Recipe", 
                BatchYield = 1, 
                TargetFoodCostPercentage = 0.3m 
            };
            currentIngredients.Clear();
            RefreshIngredientsGrid();
        }

        private void AddIngredientToRecipe()
        {
            if (cmbIngredients.SelectedItem is Ingredient selectedIngredient && 
                decimal.TryParse(txtQuantity.Text, out decimal quantity))
            {
                var recipeIngredient = new RecipeIngredient
                {
                    IngredientId = selectedIngredient.Id,
                    Quantity = quantity,
                    IngredientName = selectedIngredient.Name,
                    Unit = selectedIngredient.Unit,
                    UnitPrice = selectedIngredient.UnitPrice
                };

                currentIngredients.Add(recipeIngredient);
                RefreshIngredientsGrid();
                CalculateCost();
                
                // Reset quantity for next entry
                txtQuantity.Text = "100";
            }
            else
            {
                MessageBox.Show("Please enter a valid quantity.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveIngredient()
        {
            if (dataGridViewIngredients.SelectedRows.Count > 0)
            {
                var selected = (RecipeIngredient)dataGridViewIngredients.SelectedRows[0].DataBoundItem;
                currentIngredients.Remove(selected);
                RefreshIngredientsGrid();
                CalculateCost();
            }
        }

        private void RefreshIngredientsGrid()
        {
            dataGridViewIngredients.DataSource = currentIngredients.ToList();
            
            if (dataGridViewIngredients.Columns.Count > 0)
            {
                dataGridViewIngredients.Columns["LineCost"].DefaultCellStyle.Format = "C2";
                dataGridViewIngredients.Columns["UnitPrice"].DefaultCellStyle.Format = "C4";
                dataGridViewIngredients.Columns["IngredientId"].Visible = false;
            }
        }

        private void CalculateCost()
        {
            if (currentIngredients.Count == 0)
            {
                lblCostSummary.Text = "Add ingredients to calculate cost...";
                return;
            }

            decimal totalCost = currentIngredients.Sum(i => i.LineCost);
            decimal costPerServing = totalCost / currentRecipe.BatchYield;
            
            // Calculate suggested prices for different food cost percentages
            decimal suggestedPrice25 = Math.Round((costPerServing / 0.25m) / 5, 0) * 5;
            decimal suggestedPrice30 = Math.Round((costPerServing / 0.30m) / 5, 0) * 5;
            decimal suggestedPrice35 = Math.Round((costPerServing / 0.35m) / 5, 0) * 5;
            decimal targetPrice = Math.Round((costPerServing / currentRecipe.TargetFoodCostPercentage) / 5, 0) * 5;

            string summary = $@"Recipe: {currentRecipe.Name}
Total Cost: ₱{totalCost:F2} | Cost per Serving: ₱{costPerServing:F2}
Suggested Prices: 25%: ₱{suggestedPrice25} | 30%: ₱{suggestedPrice30} | 35%: ₱{suggestedPrice35}
Target Price ({currentRecipe.TargetFoodCostPercentage:P0}): ₱{targetPrice}";

            lblCostSummary.Text = summary;
        }

        private void SaveRecipe()
        {
            if (string.IsNullOrWhiteSpace(currentRecipe.Name))
            {
                MessageBox.Show("Please enter a recipe name.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (currentIngredients.Count == 0)
            {
                MessageBox.Show("Please add at least one ingredient to the recipe.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Set the ingredients for the current recipe
            currentRecipe.Ingredients = currentIngredients;

            if (currentRecipe.Id == 0)
            {
                // New recipe
                currentRecipe.Id = DatabaseContext.InsertRecipe(currentRecipe);
                MessageBox.Show($"Recipe '{currentRecipe.Name}' saved successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Reload recipes list
                LoadExistingRecipes();
            }
            else
            {
                // Update existing recipe
                DatabaseContext.UpdateRecipe(currentRecipe);
                MessageBox.Show($"Recipe '{currentRecipe.Name}' updated successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Optionally: Initialize a new recipe for next entry
            InitializeNewRecipe();
        }

        private void DeleteRecipe()
        {
            if (currentRecipe.Id == 0)
            {
                MessageBox.Show("No recipe loaded to delete.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete '{currentRecipe.Name}'?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                DatabaseContext.DeleteRecipe(currentRecipe.Id);
                MessageBox.Show("Recipe deleted successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Reload recipes and initialize new recipe
                LoadExistingRecipes();
                InitializeNewRecipe();
            }
        }
    }
}