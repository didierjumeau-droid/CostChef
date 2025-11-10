using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace CostChef
{
    public partial class IngredientsForm : Form
    {
        private DataGridView dataGridViewIngredients;
        private Button btnAddIngredient;
        private Button btnEditIngredient;
        private Button btnDeleteIngredient;
        private Button btnClose;
        private TextBox txtSearch;
        private ComboBox cmbCategoryFilter;
        private Button btnRefresh;

        public IngredientsForm()
        {
            InitializeComponent();
            LoadIngredients();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            this.dataGridViewIngredients = new DataGridView();
            this.btnAddIngredient = new Button();
            this.btnEditIngredient = new Button();
            this.btnDeleteIngredient = new Button();
            this.btnClose = new Button();
            this.txtSearch = new TextBox();
            this.cmbCategoryFilter = new ComboBox();
            this.btnRefresh = new Button();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(700, 500);
            this.Text = "Manage Ingredients";
            this.StartPosition = FormStartPosition.CenterParent;

            // Search
            var lblSearch = new Label { Text = "Search:", Location = new System.Drawing.Point(12, 15), AutoSize = true };
            this.txtSearch.Location = new System.Drawing.Point(60, 12);
            this.txtSearch.Size = new System.Drawing.Size(150, 20);
            this.txtSearch.TextChanged += (s, e) => LoadIngredients();
// In IngredientsForm, add to the button panel or as a context menu
// In IngredientsForm, replace the btnViewPriceHistory code with:

// In IngredientsForm, add this button to the button panel:
var btnViewPriceHistory = new Button
{
    Text = "View Price History",
    Location = new Point(342, 410), // Adjust position as needed
    Size = new Size(120, 30)
};
btnViewPriceHistory.Click += (s, e) => 
{
    if (dataGridViewIngredients.SelectedRows.Count > 0)
    {
        var ingredient = dataGridViewIngredients.SelectedRows[0].DataBoundItem as Ingredient;
        if (ingredient != null)
        {
            // FIXED: Pass the ingredient to the price history form
            var priceHistoryForm = new PriceHistoryForm(ingredient);
            priceHistoryForm.ShowDialog();
        }
    }
    else
    {
        MessageBox.Show("Please select an ingredient to view price history.", 
            "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
};
this.Controls.Add(btnViewPriceHistory);

            // Category Filter
            var lblCategory = new Label { Text = "Category:", Location = new System.Drawing.Point(220, 15), AutoSize = true };
            this.cmbCategoryFilter.Location = new System.Drawing.Point(280, 12);
            this.cmbCategoryFilter.Size = new System.Drawing.Size(150, 20);
            this.cmbCategoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbCategoryFilter.SelectedIndexChanged += (s, e) => LoadIngredients();

            // Refresh Button
            this.btnRefresh.Location = new System.Drawing.Point(440, 10);
            this.btnRefresh.Size = new System.Drawing.Size(80, 25);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += (s, e) => LoadIngredients();

            // DataGrid
            this.dataGridViewIngredients.Location = new System.Drawing.Point(12, 40);
            this.dataGridViewIngredients.Size = new System.Drawing.Size(676, 350);
            this.dataGridViewIngredients.ReadOnly = true;
            this.dataGridViewIngredients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewIngredients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Buttons
            this.btnAddIngredient.Location = new System.Drawing.Point(12, 410);
            this.btnAddIngredient.Size = new System.Drawing.Size(100, 30);
            this.btnAddIngredient.Text = "Add Ingredient";
            this.btnAddIngredient.Click += (s, e) => AddIngredient();

            this.btnEditIngredient.Location = new System.Drawing.Point(122, 410);
            this.btnEditIngredient.Size = new System.Drawing.Size(100, 30);
            this.btnEditIngredient.Text = "Edit Ingredient";
            this.btnEditIngredient.Click += (s, e) => EditIngredient();

            this.btnDeleteIngredient.Location = new System.Drawing.Point(232, 410);
            this.btnDeleteIngredient.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteIngredient.Text = "Delete Ingredient";
            this.btnDeleteIngredient.Click += (s, e) => DeleteIngredient();

            this.btnClose.Location = new System.Drawing.Point(588, 410);
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, lblCategory, cmbCategoryFilter, btnRefresh,
                dataGridViewIngredients, btnAddIngredient, btnEditIngredient,
                btnDeleteIngredient, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadIngredients()
        {
            try
            {
                var ingredients = DatabaseContext.GetAllIngredients();
                
                // Apply search filter
                var searchTerm = txtSearch.Text.ToLower();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    ingredients = ingredients.Where(i => 
                        i.Name.ToLower().Contains(searchTerm) ||
                        i.Category.ToLower().Contains(searchTerm) ||
                        (i.SupplierName ?? "").ToLower().Contains(searchTerm)
                    ).ToList();
                }

                // Apply category filter
                if (cmbCategoryFilter.SelectedItem != null && !string.IsNullOrEmpty(cmbCategoryFilter.SelectedItem.ToString()))
                {
                    ingredients = ingredients.Where(i => 
                        i.Category == cmbCategoryFilter.SelectedItem.ToString()
                    ).ToList();
                }

                dataGridViewIngredients.DataSource = ingredients;
                
                // Format currency column
                if (dataGridViewIngredients.Columns.Count > 0)
                {
                    var unitPriceColumn = dataGridViewIngredients.Columns.Cast<DataGridViewColumn>()
                        .FirstOrDefault(c => c.Name == "UnitPrice" || c.DataPropertyName == "UnitPrice");
                    if (unitPriceColumn != null)
                    {
                        unitPriceColumn.DefaultCellStyle.Format = $"{AppSettings.CurrencySymbol}0.0000";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = DatabaseContext.GetIngredientCategories();
                cmbCategoryFilter.Items.Clear();
                cmbCategoryFilter.Items.Add(""); // Empty option for "All Categories"
                foreach (var category in categories)
                {
                    cmbCategoryFilter.Items.Add(category);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddIngredient()
        {
            try
            {
                using (var form = new IngredientEditForm())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadIngredients();
                        LoadCategories();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding ingredient: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditIngredient()
        {
            if (dataGridViewIngredients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an ingredient to edit.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var ingredient = (Ingredient)dataGridViewIngredients.SelectedRows[0].DataBoundItem;
                using (var form = new IngredientEditForm(ingredient))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadIngredients();
                        LoadCategories();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing ingredient: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteIngredient()
        {
            if (dataGridViewIngredients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an ingredient to delete.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var ingredient = (Ingredient)dataGridViewIngredients.SelectedRows[0].DataBoundItem;
                var result = MessageBox.Show($"Are you sure you want to delete {ingredient.Name}?", 
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    DatabaseContext.DeleteIngredient(ingredient.Id);
                    LoadIngredients();
                    LoadCategories();
                    MessageBox.Show("Ingredient deleted successfully.", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting ingredient: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}