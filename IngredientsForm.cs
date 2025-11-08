using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

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
        private Button btnManageSuppliers;
        private Label lblCount;
        private ComboBox cmbSupplierFilter;

        private string currencySymbol => AppSettings.CurrencySymbol;
        private List<Ingredient> _currentIngredients = new List<Ingredient>();

        public IngredientsForm()
        {
            InitializeComponent();
            LoadSupplierFilter();
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
            this.btnManageSuppliers = new Button();
            this.lblCount = new Label();
            this.cmbSupplierFilter = new ComboBox();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "Manage Ingredients";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            // Search and Filter
            this.txtSearch.Location = new System.Drawing.Point(12, 12);
            this.txtSearch.Size = new System.Drawing.Size(200, 20);
            this.txtSearch.PlaceholderText = "Search ingredients...";
            this.txtSearch.TextChanged += (s, e) => LoadIngredients();

            this.cmbSupplierFilter.Location = new System.Drawing.Point(220, 12);
            this.cmbSupplierFilter.Size = new System.Drawing.Size(150, 20);
            this.cmbSupplierFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbSupplierFilter.SelectedIndexChanged += (s, e) => LoadIngredients();

            this.lblCount.Location = new System.Drawing.Point(380, 12);
            this.lblCount.Size = new System.Drawing.Size(200, 20);
            this.lblCount.Text = "Total: 0 ingredients";

            this.dataGridView.Location = new System.Drawing.Point(12, 40);
            this.dataGridView.Size = new System.Drawing.Size(776, 350);
            this.dataGridView.ReadOnly = false;
            this.dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.CellEndEdit += DataGridView_CellEndEdit;
            this.dataGridView.DataError += DataGridView_DataError;
            this.dataGridView.AllowUserToAddRows = false;

            this.btnAdd.Location = new System.Drawing.Point(12, 400);
            this.btnAdd.Size = new System.Drawing.Size(80, 30);
            this.btnAdd.Text = "Add";
            this.btnAdd.Click += (s, e) => AddIngredient();

            this.btnEdit.Location = new System.Drawing.Point(102, 400);
            this.btnEdit.Size = new System.Drawing.Size(80, 30);
            this.btnEdit.Text = "Edit Price";
            this.btnEdit.Click += (s, e) => EditSelectedIngredientPrice();

            this.btnDelete.Location = new System.Drawing.Point(192, 400);
            this.btnDelete.Size = new System.Drawing.Size(80, 30);
            this.btnDelete.Text = "Delete";
            this.btnDelete.Click += (s, e) => DeleteIngredient();

            this.btnManageSuppliers.Location = new System.Drawing.Point(282, 400);
            this.btnManageSuppliers.Size = new System.Drawing.Size(100, 30);
            this.btnManageSuppliers.Text = "Suppliers";
            this.btnManageSuppliers.Click += (s, e) => ManageSuppliers();

            this.btnClose.Location = new System.Drawing.Point(708, 400);
            this.btnClose.Size = new System.Drawing.Size(80, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                txtSearch, cmbSupplierFilter, lblCount, dataGridView, btnAdd, btnEdit, btnDelete, 
                btnManageSuppliers, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSupplierFilter()
        {
            try
            {
                var suppliers = DatabaseContext.GetAllSuppliers();
                cmbSupplierFilter.Items.Clear();
                cmbSupplierFilter.Items.Add("All Suppliers");
                
                foreach (var supplier in suppliers)
                {
                    cmbSupplierFilter.Items.Add(supplier.Name);
                }
                
                cmbSupplierFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading supplier filter: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception is FormatException)
            {
                MessageBox.Show($"Please enter a valid numeric value for the price (without currency symbol).\n\nExample: 0.85 instead of {currencySymbol}0.85", 
                    "Invalid Price Format", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                e.ThrowException = false;
            }
            else
            {
                MessageBox.Show($"Data error: {e.Exception.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.ThrowException = false;
            }
        }

        private void LoadIngredients()
        {
            try
            {
                // Suspend layout to prevent reentrant calls
                dataGridView.SuspendLayout();
                dataGridView.DataSource = null;

                var searchTerm = txtSearch.Text.ToLower();
                var allIngredients = DatabaseContext.GetAllIngredients();
                
                var filteredIngredients = allIngredients
                    .Where(i => string.IsNullOrEmpty(searchTerm) || 
                               i.Name.ToLower().Contains(searchTerm))
                    .ToList();

                // Apply supplier filter
                if (cmbSupplierFilter.SelectedIndex > 0)
                {
                    var selectedSupplier = cmbSupplierFilter.SelectedItem.ToString();
                    filteredIngredients = filteredIngredients
                        .Where(i => i.SupplierName == selectedSupplier)
                        .ToList();
                }

                filteredIngredients = filteredIngredients
                    .OrderBy(i => i.Name)
                    .ToList();

                // Store current ingredients for reference
                _currentIngredients = filteredIngredients;

                // Create a DataTable for safer binding
                var table = new System.Data.DataTable();
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("Unit", typeof(string));
                table.Columns.Add("UnitPrice", typeof(decimal));
                table.Columns.Add("SupplierName", typeof(string));

                foreach (var ingredient in filteredIngredients)
                {
                    table.Rows.Add(ingredient.Id, ingredient.Name, ingredient.Unit, ingredient.UnitPrice, ingredient.SupplierName);
                }

                dataGridView.DataSource = table;
                
                // Configure columns after data binding
                if (dataGridView.Columns.Count > 0)
                {
                    dataGridView.Columns["Id"].Visible = false;
                    
                    dataGridView.Columns["UnitPrice"].DefaultCellStyle.Format = "0.00";
                    dataGridView.Columns["UnitPrice"].HeaderText = $"Price/Unit ({currencySymbol})";
                    dataGridView.Columns["Name"].HeaderText = "Ingredient Name";
                    dataGridView.Columns["Unit"].HeaderText = "Unit";
                    dataGridView.Columns["SupplierName"].HeaderText = "Supplier";
                    
                    dataGridView.Columns["Name"].ReadOnly = true;
                    dataGridView.Columns["Unit"].ReadOnly = true;
                    dataGridView.Columns["UnitPrice"].ReadOnly = false;
                    dataGridView.Columns["SupplierName"].ReadOnly = false;
                    
                    dataGridView.Columns["Name"].DisplayIndex = 0;
                    dataGridView.Columns["Unit"].DisplayIndex = 1;
                    dataGridView.Columns["UnitPrice"].DisplayIndex = 2;
                    dataGridView.Columns["SupplierName"].DisplayIndex = 3;

                    // Make supplier column a dropdown
                    ConfigureSupplierColumn();
                }
                
                dataGridView.ResumeLayout();
                lblCount.Text = $"Total: {filteredIngredients.Count} ingredients";
            }
            catch (Exception ex)
            {
                dataGridView.ResumeLayout();
                MessageBox.Show($"Error loading ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureSupplierColumn()
        {
            var supplierColumn = dataGridView.Columns["SupplierName"];
            if (supplierColumn != null)
            {
                // Create a combo box column for suppliers
                var comboColumn = new DataGridViewComboBoxColumn();
                comboColumn.HeaderText = "Supplier";
                comboColumn.DataPropertyName = "SupplierName";
                comboColumn.Name = "SupplierName";
                comboColumn.FlatStyle = FlatStyle.Flat;
                
                // Add supplier options
                comboColumn.Items.Add(""); // Empty option
                var suppliers = DatabaseContext.GetAllSuppliers();
                foreach (var supplier in suppliers)
                {
                    comboColumn.Items.Add(supplier.Name);
                }

                // Replace the textbox column with combobox column
                int columnIndex = supplierColumn.Index;
                dataGridView.Columns.Remove(supplierColumn);
                dataGridView.Columns.Insert(columnIndex, comboColumn);
                comboColumn.DisplayIndex = 3;
            }
        }

        private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView.Rows.Count)
            {
                try
                {
                    var row = dataGridView.Rows[e.RowIndex];
                    int ingredientId = (int)row.Cells["Id"].Value;
                    var ingredient = _currentIngredients.FirstOrDefault(i => i.Id == ingredientId);
                    
                    if (ingredient != null)
                    {
                        if (e.ColumnIndex == dataGridView.Columns["UnitPrice"].Index)
                        {
                            if (row.Cells[e.ColumnIndex].Value != null && 
                                decimal.TryParse(row.Cells[e.ColumnIndex].Value.ToString(), out decimal newPrice))
                            {
                                ingredient.UnitPrice = newPrice;
                                DatabaseContext.UpdateIngredient(ingredient);
                                
                                // Refresh the display
                                dataGridView.InvalidateRow(e.RowIndex);
                            }
                            else
                            {
                                MessageBox.Show("Please enter a valid numeric price (without currency symbol).\n\nExample: 0.85 instead of $0.85", "Invalid Price Format", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                // Restore original value
                                row.Cells[e.ColumnIndex].Value = ingredient.UnitPrice;
                            }
                        }
                        else if (dataGridView.Columns[e.ColumnIndex] is DataGridViewComboBoxColumn)
                        {
                            var newSupplierName = row.Cells[e.ColumnIndex].Value?.ToString() ?? "";
                            ingredient.SupplierName = newSupplierName;
                            
                            // Try to find supplier ID
                            var supplier = DatabaseContext.GetSupplierByName(newSupplierName);
                            if (supplier != null)
                            {
                                ingredient.SupplierId = supplier.Id;
                            }
                            else
                            {
                                ingredient.SupplierId = null;
                            }
                            
                            DatabaseContext.UpdateIngredient(ingredient);
                            
                            // Show confirmation
                            if (!string.IsNullOrEmpty(newSupplierName))
                            {
                                MessageBox.Show($"Supplier updated to: {newSupplierName}", "Supplier Updated", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Supplier removed from ingredient", "Supplier Updated", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating ingredient: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadIngredients(); // Reload to reset any inconsistent state
                }
            }
        }

        private void ManageSuppliers()
        {
            using (var form = new SupplierManagementForm())
            {
                form.ShowDialog();
                LoadSupplierFilter();
                LoadIngredients();
            }
        }

        private void EditSelectedIngredientPrice()
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView.SelectedRows[0];
                int ingredientId = (int)selectedRow.Cells["Id"].Value;
                var ingredient = _currentIngredients.FirstOrDefault(i => i.Id == ingredientId);
                
                if (ingredient != null)
                {
                    using (var form = new Form())
                    {
                        form.Text = $"Update Price - {ingredient.Name}";
                        form.Size = new System.Drawing.Size(400, 350);
                        form.StartPosition = FormStartPosition.CenterParent;
                        form.FormBorderStyle = FormBorderStyle.FixedDialog;
                        form.MaximizeBox = false;
                        
                        var lblCurrent = new Label { 
                            Text = $"Current: {currencySymbol} {ingredient.UnitPrice:0.00} per {ingredient.Unit}", 
                            Location = new System.Drawing.Point(20, 20), 
                            AutoSize = true 
                        };
                        
                        var grpReceipt = new GroupBox();
                        grpReceipt.Text = "Receipt Information (Optional)";
                        grpReceipt.Location = new System.Drawing.Point(20, 50);
                        grpReceipt.Size = new System.Drawing.Size(350, 120);
                        
                        var lblShopPrice = new Label { 
                            Text = $"Shop Price ({currencySymbol}):", 
                            Location = new System.Drawing.Point(15, 25), 
                            AutoSize = true 
                        };
                        var txtShopPrice = new TextBox { 
                            Text = "", 
                            Location = new System.Drawing.Point(120, 22), 
                            Size = new System.Drawing.Size(100, 20),
                            PlaceholderText = "175.00"
                        };
                        
                        var lblQuantityBought = new Label { 
                            Text = "Quantity Bought:", 
                            Location = new System.Drawing.Point(15, 55), 
                            AutoSize = true 
                        };
                        var txtQuantityBought = new TextBox { 
                            Text = "", 
                            Location = new System.Drawing.Point(120, 52), 
                            Size = new System.Drawing.Size(100, 20),
                            PlaceholderText = "200"
                        };
                        
                        var lblBoughtUnit = new Label { 
                            Text = "Unit:", 
                            Location = new System.Drawing.Point(15, 85), 
                            AutoSize = true 
                        };
                        var txtBoughtUnit = new TextBox { 
                            Text = ingredient.Unit,
                            Location = new System.Drawing.Point(120, 82), 
                            Size = new System.Drawing.Size(100, 20),
                            PlaceholderText = "g, kg, ml, etc."
                        };
                        
                        var btnCalculate = new Button { 
                            Text = "Calculate", 
                            Location = new System.Drawing.Point(230, 50), 
                            Size = new System.Drawing.Size(80, 25)
                        };
                        
                        grpReceipt.Controls.AddRange(new Control[] {
                            lblShopPrice, txtShopPrice, lblQuantityBought, txtQuantityBought,
                            lblBoughtUnit, txtBoughtUnit, btnCalculate
                        });
                        
                        var lblCalculated = new Label { 
                            Text = "Calculated unit price will appear here", 
                            Location = new System.Drawing.Point(20, 180), 
                            Size = new System.Drawing.Size(350, 20),
                            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                            BorderStyle = BorderStyle.FixedSingle,
                            BackColor = System.Drawing.Color.LightYellow
                        };
                        
                        var lblNew = new Label { 
                            Text = "New Unit Price:", 
                            Location = new System.Drawing.Point(20, 210), 
                            AutoSize = true 
                        };
                        var txtNewPrice = new TextBox { 
                            Text = ingredient.UnitPrice.ToString("0.00"), 
                            Location = new System.Drawing.Point(120, 207), 
                            Size = new System.Drawing.Size(100, 20) 
                        };
                        
                        var btnUpdate = new Button { 
                            Text = "Update", 
                            DialogResult = DialogResult.OK, 
                            Location = new System.Drawing.Point(120, 240),
                            Size = new System.Drawing.Size(80, 30)
                        };
                        var btnCancel = new Button { 
                            Text = "Cancel", 
                            DialogResult = DialogResult.Cancel, 
                            Location = new System.Drawing.Point(210, 240),
                            Size = new System.Drawing.Size(80, 30)
                        };
                        
                        btnCalculate.Click += (s, e) =>
                        {
                            if (decimal.TryParse(txtShopPrice.Text, out decimal shopPrice) && 
                                decimal.TryParse(txtQuantityBought.Text, out decimal quantity) && 
                                quantity > 0)
                            {
                                decimal calculatedUnitPrice = shopPrice / quantity;
                                txtNewPrice.Text = calculatedUnitPrice.ToString("0.00");
                                lblCalculated.Text = $"Calculated: {currencySymbol} {shopPrice:0.00} ÷ {quantity} {txtBoughtUnit.Text} = {currencySymbol} {calculatedUnitPrice:0.00} per {ingredient.Unit}";
                                lblCalculated.BackColor = System.Drawing.Color.LightGreen;
                            }
                            else
                            {
                                lblCalculated.Text = "Please enter valid numbers for shop price and quantity";
                                lblCalculated.BackColor = System.Drawing.Color.LightCoral;
                            }
                        };
                        
                        form.Controls.AddRange(new Control[] {
                            lblCurrent, grpReceipt, lblCalculated, lblNew, txtNewPrice, btnUpdate, btnCancel
                        });
                        
                        form.AcceptButton = btnUpdate;
                        form.CancelButton = btnCancel;
                        
                        if (form.ShowDialog() == DialogResult.OK && 
                            decimal.TryParse(txtNewPrice.Text, out decimal newPrice))
                        {
                            ingredient.UnitPrice = newPrice;
                            DatabaseContext.UpdateIngredient(ingredient);
                            LoadIngredients(); // Reload to refresh display
                            MessageBox.Show($"Price updated to {currencySymbol} {newPrice:0.00} per {ingredient.Unit}!", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
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
                form.Size = new System.Drawing.Size(350, 280);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                
                var lblName = new Label { Text = "Name:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
                var txtName = new TextBox { Location = new System.Drawing.Point(120, 17), Size = new System.Drawing.Size(150, 20) };
                var lblUnit = new Label { Text = "Unit:", Location = new System.Drawing.Point(20, 50), AutoSize = true };
                var txtUnit = new TextBox { Location = new System.Drawing.Point(120, 47), Size = new System.Drawing.Size(150, 20) };
                var lblPrice = new Label { Text = $"Price ({currencySymbol}):", Location = new System.Drawing.Point(20, 80), AutoSize = true };
                var txtPrice = new TextBox { Text = "0", Location = new System.Drawing.Point(120, 77), Size = new System.Drawing.Size(150, 20) };
                
                var lblSupplier = new Label { Text = "Supplier:", Location = new System.Drawing.Point(20, 110), AutoSize = true };
                var cmbSupplier = new ComboBox { Location = new System.Drawing.Point(120, 107), Size = new System.Drawing.Size(150, 20), DropDownStyle = ComboBoxStyle.DropDownList };
                
                // Load suppliers into combo box
                try
                {
                    var suppliers = DatabaseContext.GetAllSuppliers();
                    cmbSupplier.Items.Add(""); // Empty option for no supplier
                    foreach (var supplier in suppliers)
                    {
                        cmbSupplier.Items.Add(supplier.Name);
                    }
                    cmbSupplier.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                var btnOk = new Button { Text = "Add", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(120, 150) };
                var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(200, 150) };
                
                form.Controls.AddRange(new Control[] { 
                    lblName, txtName, lblUnit, txtUnit, lblPrice, txtPrice, 
                    lblSupplier, cmbSupplier,
                    btnOk, btnCancel 
                });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;
                
                if (form.ShowDialog() == DialogResult.OK && 
                    !string.IsNullOrWhiteSpace(txtName.Text) &&
                    !string.IsNullOrWhiteSpace(txtUnit.Text) &&
                    decimal.TryParse(txtPrice.Text, out decimal price))
                {
                    try
                    {
                        var newIngredient = new Ingredient
                        {
                            Name = txtName.Text.Trim(),
                            Unit = txtUnit.Text.Trim(),
                            UnitPrice = price,
                            SupplierName = cmbSupplier.SelectedItem?.ToString() ?? ""
                        };
                        
                        // Set supplier ID if available
                        if (!string.IsNullOrEmpty(newIngredient.SupplierName))
                        {
                            var supplier = DatabaseContext.GetSupplierByName(newIngredient.SupplierName);
                            if (supplier != null)
                            {
                                newIngredient.SupplierId = supplier.Id;
                            }
                        }
                        
                        DatabaseContext.InsertIngredient(newIngredient);
                        LoadIngredients();
                        MessageBox.Show("Ingredient added successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error adding ingredient: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DeleteIngredient()
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView.SelectedRows[0];
                int ingredientId = (int)selectedRow.Cells["Id"].Value;
                var ingredient = _currentIngredients.FirstOrDefault(i => i.Id == ingredientId);
                
                if (ingredient != null)
                {
                    var result = MessageBox.Show($"Delete {ingredient.Name}?", "Confirm Delete", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            DatabaseContext.DeleteIngredient(ingredient.Id);
                            LoadIngredients();
                            MessageBox.Show("Ingredient deleted successfully!", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting ingredient: {ex.Message}", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
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