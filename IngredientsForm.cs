using System;
using System.Windows.Forms;
using System.Linq;

namespace CostChef
{
    public partial class IngredientsForm : Form
    {
        private DataGridView dataGridView;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnClose;
        private Label lblCount;

        // Currency symbol - changed from £ to ₱
        private string currencySymbol = "₱";

        public IngredientsForm()
        {
            InitializeComponent();
            LoadIngredients();
        }

        private void InitializeComponent()
        {
            this.dataGridView = new DataGridView();
            this.txtSearch = new TextBox();
            this.btnAdd = new Button();
            this.btnEdit = new Button();
            this.btnDelete = new Button();
            this.btnClose = new Button();
            this.lblCount = new Label();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Text = "Manage Ingredients";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            // Search Box
            this.txtSearch.Location = new System.Drawing.Point(12, 12);
            this.txtSearch.Size = new System.Drawing.Size(200, 20);
            this.txtSearch.PlaceholderText = "Search ingredients...";
            this.txtSearch.TextChanged += (s, e) => LoadIngredients();

            // Count Label
            this.lblCount.Location = new System.Drawing.Point(220, 12);
            this.lblCount.Size = new System.Drawing.Size(200, 20);
            this.lblCount.Text = "Total: 0 ingredients";

            // DataGridView - NOW EDITABLE!
            this.dataGridView.Location = new System.Drawing.Point(12, 40);
            this.dataGridView.Size = new System.Drawing.Size(576, 300);
            this.dataGridView.ReadOnly = false;
            this.dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.CellEndEdit += DataGridView_CellEndEdit;
            this.dataGridView.DataError += DataGridView_DataError;

            // Buttons
            this.btnAdd.Location = new System.Drawing.Point(12, 350);
            this.btnAdd.Size = new System.Drawing.Size(80, 30);
            this.btnAdd.Text = "Add";
            this.btnAdd.Click += (s, e) => AddIngredient();

            this.btnEdit.Location = new System.Drawing.Point(102, 350);
            this.btnEdit.Size = new System.Drawing.Size(80, 30);
            this.btnEdit.Text = "Edit Price";
            this.btnEdit.Click += (s, e) => EditSelectedIngredientPrice();

            this.btnDelete.Location = new System.Drawing.Point(192, 350);
            this.btnDelete.Size = new System.Drawing.Size(80, 30);
            this.btnDelete.Text = "Delete";
            this.btnDelete.Click += (s, e) => DeleteIngredient();

            this.btnClose.Location = new System.Drawing.Point(508, 350);
            this.btnClose.Size = new System.Drawing.Size(80, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            // Add controls
            this.Controls.AddRange(new Control[] {
                txtSearch, lblCount, dataGridView, btnAdd, btnEdit, btnDelete, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void DataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception is FormatException)
            {
                MessageBox.Show("Please enter a valid numeric value for the price (without currency symbol).\n\nExample: 0.85 instead of P0.85", 
                    "Invalid Price Format", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                e.ThrowException = false;
            }
        }

        private void LoadIngredients()
        {
            try
            {
                var searchTerm = txtSearch.Text.ToLower();
                var allIngredients = DatabaseContext.GetAllIngredients();
                
                var filteredIngredients = allIngredients
                    .Where(i => string.IsNullOrEmpty(searchTerm) || 
                               i.Name.ToLower().Contains(searchTerm))
                    .OrderBy(i => i.Name)
                    .ToList();

                // Suspend layout to prevent reentrant calls
                dataGridView.SuspendLayout();
                dataGridView.DataSource = null;
                dataGridView.DataSource = filteredIngredients;
                
                if (dataGridView.Columns.Count > 0)
                {
                    dataGridView.Columns["Id"].Visible = false;
                    dataGridView.Columns["Category"].Visible = false;
                    
                    // Format columns
                    dataGridView.Columns["UnitPrice"].DefaultCellStyle.Format = "0.00";
                    dataGridView.Columns["UnitPrice"].HeaderText = $"Price/Unit ({currencySymbol})";
                    dataGridView.Columns["Name"].HeaderText = "Ingredient Name";
                    dataGridView.Columns["Unit"].HeaderText = "Unit";
                    
                    // Make only UnitPrice editable
                    dataGridView.Columns["Name"].ReadOnly = true;
                    dataGridView.Columns["Unit"].ReadOnly = true;
                    dataGridView.Columns["UnitPrice"].ReadOnly = false;
                }
                
                dataGridView.ResumeLayout();
                lblCount.Text = $"Total: {filteredIngredients.Count} ingredients";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView.Columns["UnitPrice"].Index)
            {
                try
                {
                    var ingredient = (Ingredient)dataGridView.Rows[e.RowIndex].DataBoundItem;
                    if (dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null && 
                        decimal.TryParse(dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), 
                        out decimal newPrice))
                    {
                        ingredient.UnitPrice = newPrice;
                        DatabaseContext.UpdateIngredient(ingredient);
                        
                        // Use BeginInvoke to avoid reentrant call
                        this.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show($"Price updated for {ingredient.Name}!", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            // Refresh the display without reloading entire DataSource
                            dataGridView.InvalidateRow(e.RowIndex);
                        }));
                    }
                    else
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("Please enter a valid numeric price (without currency symbol).", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Just refresh this row instead of entire grid
                            dataGridView.InvalidateRow(e.RowIndex);
                        }));
                    }
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show($"Error updating price: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            }
        }

        private void EditSelectedIngredientPrice()
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var ingredient = (Ingredient)dataGridView.SelectedRows[0].DataBoundItem;
                
                using (var form = new Form())
                {
                    form.Text = $"Edit Price - {ingredient.Name}";
                    form.Size = new System.Drawing.Size(300, 150);
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.MaximizeBox = false;
                    
                    var lblCurrent = new Label { Text = $"Current: {currencySymbol}{ingredient.UnitPrice:0.00} per {ingredient.Unit}", 
                        Location = new System.Drawing.Point(20, 20), AutoSize = true };
                    var lblNew = new Label { Text = "New Price:", 
                        Location = new System.Drawing.Point(20, 50), AutoSize = true };
                    var txtNewPrice = new TextBox { Text = ingredient.UnitPrice.ToString("0.00"), 
                        Location = new System.Drawing.Point(100, 47), Size = new System.Drawing.Size(100, 20) };
                    var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, 
                        Location = new System.Drawing.Point(120, 80) };
                    var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, 
                        Location = new System.Drawing.Point(200, 80) };
                    
                    form.Controls.AddRange(new Control[] { lblCurrent, lblNew, txtNewPrice, btnOk, btnCancel });
                    form.AcceptButton = btnOk;
                    form.CancelButton = btnCancel;
                    
                    if (form.ShowDialog() == DialogResult.OK && 
                        decimal.TryParse(txtNewPrice.Text, out decimal newPrice))
                    {
                        ingredient.UnitPrice = newPrice;
                        DatabaseContext.UpdateIngredient(ingredient);
                        
                        // Refresh just the display instead of reloading DataSource
                        dataGridView.Invalidate();
                        MessageBox.Show($"Price updated to {currencySymbol}{newPrice:0.00}!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an ingredient to edit.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddIngredient()
        {
            using (var form = new Form())
            {
                form.Text = "Add New Ingredient";
                form.Size = new System.Drawing.Size(300, 200);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                
                var lblName = new Label { Text = "Name:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
                var txtName = new TextBox { Location = new System.Drawing.Point(100, 17), Size = new System.Drawing.Size(150, 20) };
                var lblUnit = new Label { Text = "Unit:", Location = new System.Drawing.Point(20, 50), AutoSize = true };
                var txtUnit = new TextBox { Location = new System.Drawing.Point(100, 47), Size = new System.Drawing.Size(150, 20) };
                var lblPrice = new Label { Text = $"Price ({currencySymbol}):", Location = new System.Drawing.Point(20, 80), AutoSize = true };
                var txtPrice = new TextBox { Text = "0", Location = new System.Drawing.Point(100, 77), Size = new System.Drawing.Size(150, 20) };
                var btnOk = new Button { Text = "Add", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(120, 110) };
                var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(200, 110) };
                
                form.Controls.AddRange(new Control[] { lblName, txtName, lblUnit, txtUnit, lblPrice, txtPrice, btnOk, btnCancel });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;
                
                if (form.ShowDialog() == DialogResult.OK && 
                    !string.IsNullOrWhiteSpace(txtName.Text) &&
                    !string.IsNullOrWhiteSpace(txtUnit.Text) &&
                    decimal.TryParse(txtPrice.Text, out decimal price))
                {
                    var newIngredient = new Ingredient
                    {
                        Name = txtName.Text,
                        Unit = txtUnit.Text,
                        UnitPrice = price
                    };
                    
                    DatabaseContext.InsertIngredient(newIngredient);
                    // Reload the entire grid for new ingredient
                    LoadIngredients();
                    MessageBox.Show("Ingredient added successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void DeleteIngredient()
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var ingredient = (Ingredient)dataGridView.SelectedRows[0].DataBoundItem;
                var result = MessageBox.Show($"Delete {ingredient.Name}?", "Confirm Delete", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    DatabaseContext.DeleteIngredient(ingredient.Id);
                    // Reload the entire grid after deletion
                    LoadIngredients();
                    MessageBox.Show("Ingredient deleted successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select an ingredient to delete.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}