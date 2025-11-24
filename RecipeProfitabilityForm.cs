using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace CostChef
{
    public partial class RecipeProfitabilityForm : Form
    {
        private DataGridView dataGridViewRecipes;
        private Button btnClose;
        private Button btnRefresh;
        private Button btnExport;
        private Label lblTitle;
        private ComboBox cmbSortBy;
        private ComboBox cmbCategoryFilter;
        private Label lblSortBy;
        private Label lblCategoryFilter;
        private Label lblSummary;
        private Button btnOpenRecipe;

        public RecipeProfitabilityForm()
        {
            InitializeComponent();
            LoadRecipeData();
        }

        private void InitializeComponent()
        {
            this.dataGridViewRecipes = new DataGridView();
            this.btnClose = new Button();
            this.btnRefresh = new Button();
            this.btnExport = new Button();
            this.btnOpenRecipe = new Button();
            this.lblTitle = new Label();
            this.cmbSortBy = new ComboBox();
            this.cmbCategoryFilter = new ComboBox();
            this.lblSortBy = new Label();
            this.lblCategoryFilter = new Label();
            this.lblSummary = new Label();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Text = "Recipe Profitability Dashboard - CostChef v3.0";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Size = new System.Drawing.Size(350, 24);
            this.lblTitle.Text = "💵 Recipe Profitability Dashboard";

            // cmbSortBy
            this.cmbSortBy.Location = new System.Drawing.Point(20, 60);
            this.cmbSortBy.Size = new System.Drawing.Size(200, 21);
            this.cmbSortBy.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbSortBy.Items.AddRange(new object[] {
                "Profit Margin (High to Low)",
                "Profit Margin (Low to High)",
                "Food Cost % (High to Low)",
                "Profit Amount"
            });
            this.cmbSortBy.SelectedIndex = 0;
            this.cmbSortBy.SelectedIndexChanged += (s, e) => RefreshRecipeGrid();

            // cmbCategoryFilter
            this.cmbCategoryFilter.Location = new System.Drawing.Point(230, 60);
            this.cmbCategoryFilter.Size = new System.Drawing.Size(150, 21);
            this.cmbCategoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbCategoryFilter.SelectedIndexChanged += (s, e) => RefreshRecipeGrid();

            // lblSummary
            this.lblSummary.Location = new System.Drawing.Point(20, 95);
            this.lblSummary.Size = new System.Drawing.Size(960, 25);
            this.lblSummary.BorderStyle = BorderStyle.FixedSingle;
            this.lblSummary.Text = "Summary: Calculating...";
            this.lblSummary.TextAlign = ContentAlignment.MiddleLeft;

            // DataGridView
            this.dataGridViewRecipes.Location = new System.Drawing.Point(20, 130);
            this.dataGridViewRecipes.Size = new System.Drawing.Size(960, 400);
            this.dataGridViewRecipes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewRecipes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewRecipes.ReadOnly = true;
            this.dataGridViewRecipes.AutoGenerateColumns = false;
            this.dataGridViewRecipes.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "RecipeName", HeaderText = "Recipe", DataPropertyName = "RecipeName" },
                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", DataPropertyName = "Category" },
                new DataGridViewTextBoxColumn { Name = "CostPerServing", HeaderText = "Cost/Unit", DataPropertyName = "CostPerServing", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } },
                new DataGridViewTextBoxColumn { Name = "SalesPrice", HeaderText = "Price", DataPropertyName = "SalesPrice", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } },
                new DataGridViewTextBoxColumn { Name = "ProfitMargin", HeaderText = "Margin %", DataPropertyName = "ProfitMargin", DefaultCellStyle = new DataGridViewCellStyle { Format = "P1" } },
                new DataGridViewTextBoxColumn { Name = "FoodCostPercentage", HeaderText = "Food Cost %", DataPropertyName = "FoodCostPercentage", DefaultCellStyle = new DataGridViewCellStyle { Format = "F1" } },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status" },
                new DataGridViewTextBoxColumn { Name = "RecipeId", HeaderText = "ID", DataPropertyName = "RecipeId", Visible = false }
            });

            // btnOpenRecipe
            this.btnOpenRecipe.Location = new System.Drawing.Point(20, 545);
            this.btnOpenRecipe.Size = new System.Drawing.Size(120, 30);
            this.btnOpenRecipe.Text = "Open Recipe";
            this.btnOpenRecipe.Click += new EventHandler(this.btnOpenRecipe_Click);

            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(150, 545);
            this.btnRefresh.Size = new System.Drawing.Size(100, 30);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new EventHandler(this.btnRefresh_Click);

            // btnExport
            this.btnExport.Location = new System.Drawing.Point(260, 545);
            this.btnExport.Size = new System.Drawing.Size(100, 30);
            this.btnExport.Text = "Export CSV";
            this.btnExport.Click += new EventHandler(this.btnExport_Click);

            // btnClose
            this.btnClose.Location = new System.Drawing.Point(880, 545);
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new EventHandler(this.btnClose_Click);

            // lblSortBy
            this.lblSortBy.AutoSize = true;
            this.lblSortBy.Location = new System.Drawing.Point(20, 45);
            this.lblSortBy.Text = "Sort by:";

            // lblCategoryFilter
            this.lblCategoryFilter.AutoSize = true;
            this.lblCategoryFilter.Location = new System.Drawing.Point(230, 45);
            this.lblCategoryFilter.Text = "Category:";

            // Add controls
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSortBy);
            this.Controls.Add(this.cmbSortBy);
            this.Controls.Add(this.lblCategoryFilter);
            this.Controls.Add(this.cmbCategoryFilter);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.dataGridViewRecipes);
            this.Controls.Add(this.btnOpenRecipe);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnClose);

            this.ResumeLayout(false);
        }

        private void LoadRecipeData()
        {
            try
            {
                // Load categories for filter
                var categories = DatabaseContext.GetRecipeCategories();
                cmbCategoryFilter.Items.Clear();
                cmbCategoryFilter.Items.Add("All Categories");
                foreach (var category in categories)
                {
                    cmbCategoryFilter.Items.Add(category);
                }
                cmbCategoryFilter.SelectedIndex = 0;

                // Initial load
                RefreshRecipeGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recipe data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshRecipeGrid()
        {
            // FIX: Set required default values for the filtering parameters
            decimal minProfit = 0m;
            decimal maxFoodCost = 1.0m;
            decimal minProfitMargin = 0m;

            List<Recipe> recipes = DatabaseContext.GetAllRecipes();
            string sortOption = cmbSortBy.SelectedItem?.ToString() ?? "Profit Margin (High to Low)";
            string filterCategory = cmbCategoryFilter.SelectedItem?.ToString() ?? "All Categories";

            // Apply DB filtering based on sort option
            if (sortOption.Contains("Profit Amount"))
            {
                recipes = DatabaseContext.GetRecipesByProfitAmount(minProfit);
            }
            else if (sortOption.Contains("Food Cost %")) // FIXED: matches combo text
            {
                recipes = DatabaseContext.GetRecipesByFoodCostPercentage(maxFoodCost);
            }
            else if (sortOption.Contains("Profit Margin"))
            {
                recipes = DatabaseContext.GetRecipesByProfitMargin(minProfitMargin);
            }

            // Filter by category
            if (filterCategory != "All Categories")
            {
                recipes = recipes.Where(r =>
                    r.Category != null &&
                    r.Category.Equals(filterCategory, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Sort the list based on selection
            recipes = SortRecipes(recipes, sortOption);

            // Populate the grid
            PopulateRecipeGrid(recipes);
        }

        private List<Recipe> SortRecipes(List<Recipe> recipes, string sortOption)
        {
            return sortOption switch
            {
                "Name (A-Z)" => recipes.OrderBy(r => r.Name).ToList(),
                "Name (Z-A)" => recipes.OrderByDescending(r => r.Name).ToList(),
                "Profit Margin (High to Low)" => recipes.OrderByDescending(r => r.ProfitMargin).ToList(),
                "Profit Margin (Low to High)" => recipes.OrderBy(r => r.ProfitMargin).ToList(),
                "Food Cost % (High to Low)" => recipes.OrderByDescending(r => r.ActualFoodCostPercentage).ToList(),
                "Profit Amount" => recipes.OrderByDescending(r => r.ProfitPerServing).ToList(),
                _ => recipes.OrderByDescending(r => r.ProfitMargin).ToList(),
            };
        }

        private void PopulateRecipeGrid(List<Recipe> recipes)
        {
            var gridData = new List<RecipeDisplayData>();

            foreach (var recipe in recipes)
            {
                gridData.Add(new RecipeDisplayData
                {
                    RecipeId = recipe.Id,
                    RecipeName = recipe.Name,
                    TotalCost = recipe.TotalCost,
                    CostPerServing = recipe.CostPerServing,
                    SalesPrice = recipe.SalesPrice,
                    ProfitPerServing = recipe.ProfitPerServing,
                    ProfitMargin = recipe.ProfitMargin,
                    FoodCostPercentage = recipe.FoodCostPercentage,
                    Status = recipe.ActualFoodCostPercentage <= recipe.TargetFoodCostPercentage ? "OK" : "High FC",
                    Category = recipe.Category
                });
            }

            dataGridViewRecipes.DataSource = gridData;

            // Recalculate summary
            CalculateSummary(recipes);
        }

        private void CalculateSummary(List<Recipe> recipes)
        {
            if (!recipes.Any())
            {
                lblSummary.Text = "No recipes to display.";
                return;
            }

            var profitable = recipes.Count(r => r.ProfitPerServing > 0);
            var highFC = recipes.Count(r => r.ActualFoodCostPercentage > r.TargetFoodCostPercentage);
            var averageProfitMargin = recipes.Where(r => r.SalesPrice > 0).Average(r => r.ProfitMargin) * 100;

            lblSummary.Text = $"Total Recipes: {recipes.Count} | Profitable: {profitable} | High Food Cost: {highFC} | Avg Profit Margin: {averageProfitMargin:F1}%";
        }

        private void btnOpenRecipe_Click(object sender, EventArgs e)
        {
            if (dataGridViewRecipes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a recipe to open.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = dataGridViewRecipes.SelectedRows[0];
            if (selectedRow.DataBoundItem is RecipeDisplayData displayData)
            {
                // Load the full recipe using its ID
                var recipe = DatabaseContext.GetRecipeById(displayData.RecipeId);
                if (recipe != null)
                {
                    var recipeForm = new RecipesForm();
                    // TODO: Implement LoadRecipeById on RecipesForm and uncomment:
                    // recipeForm.LoadRecipeById(recipe.Id);
                    recipeForm.ShowDialog();
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshRecipeGrid();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV files (*.csv)|*.csv";
                    saveDialog.Title = "Export Recipe Profitability Data";
                    saveDialog.FileName = $"RecipeProfitability_{DateTime.Now:yyyyMMdd}.csv";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (var writer = new StreamWriter(saveDialog.FileName))
                        {
                            // Write header
                            writer.WriteLine("RecipeName,Category,TotalCost,CostPerServing,SalesPrice,ProfitPerServing,ProfitMargin,FoodCostPercentage,Status");

                            foreach (DataGridViewRow row in dataGridViewRecipes.Rows)
                            {
                                if (row.IsNewRow) continue;

                                if (row.DataBoundItem is RecipeDisplayData data)
                                {
                                    var line =
                                        "\"" + (data.RecipeName ?? string.Empty).Replace("\"", "\"\"") + "\"," +
                                        "\"" + (data.Category ?? string.Empty).Replace("\"", "\"\"") + "\"," +
                                        data.TotalCost + "," +
                                        data.CostPerServing + "," +
                                        data.SalesPrice + "," +
                                        data.ProfitPerServing + "," +
                                        data.ProfitMargin + "," +
                                        data.FoodCostPercentage + "," +
                                        "\"" + (data.Status ?? string.Empty).Replace("\"", "\"\"") + "\"";

                                    writer.WriteLine(line);
                                }
                            }
                        }

                        MessageBox.Show("Recipe profitability data exported to " + saveDialog.FileName, "Export Successful",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }


}
