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
        private Button btnManageSuppliers;
        private Label lblCount;

        private string currencySymbol => AppSettings.CurrencySymbol;

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
            this.btnManageSuppliers = new Button();
            this.lblCount = new Label();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(750, 400);
            this.Text = "Manage Ingredients";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            this.txtSearch.Location = new System.Drawing.Point(12, 12);
            this.txtSearch.Size = new System.Drawing.Size(200, 20);
            this.txtSearch.PlaceholderText = "Search ingredients...";
            this.txtSearch.TextChanged += (s, e) => LoadIngredients();

            this.lblCount.Location = new System.Drawing.Point(220, 12);
            this.lblCount.Size = new System.Drawing.Size(200, 20);
            this.lblCount.Text = "Total: 0 ingredients";

            this.dataGridView.Location = new System.Drawing.Point(12, 40);
            this.dataGridView.Size = new System.Drawing.Size(726, 300);
            this.dataGridView.ReadOnly = false;
            this.dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.CellEndEdit += DataGridView_CellEndEdit;
            this.dataGridView.DataError += DataGridView_DataError;

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

            this.btnManageSuppliers.Location = new System.Drawing.Point(282, 350);
            this.btnManageSuppliers.Size = new System.Drawing.Size(100, 30);
            this.btnManageSuppliers.Text = "Suppliers";
            this.btnManageSuppliers.Click += (s, e) => ManageSuppliers();

            this.btnClose.Location = new System.Drawing.Point(658, 350);
            this.btnClose.Size = new System.Drawing.Size(80, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                txtSearch, lblCount, dataGridView, btnAdd, btnEdit, btnDelete, 
                btnManageSuppliers, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
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

                dataGridView.SuspendLayout();
                dataGridView.DataSource = null;
                dataGridView.DataSource = filteredIngredients;
                
                if (dataGridView.Columns.Count > 0)
                {
                    dataGridView.Columns["Id"].Visible = false;
                    dataGridView.Columns["Category"].Visible = false;
                    dataGridView.Columns["SupplierId"].Visible = false;
                    dataGridView.Columns["SupplierName"].Visible = false;
                    
                    dataGridView.Columns["UnitPrice"].DefaultCellStyle.Format = "0.00";
                    dataGridView.Columns["UnitPrice"].HeaderText = $"Price/Unit ({currencySymbol})";
                    dataGridView.Columns["Name"].HeaderText = "Ingredient Name";
                    dataGridView.Columns["Unit"].HeaderText = "Unit";
                    
                    if (!dataGridView.Columns.Contains("SupplierDisplay"))
                    {
                        var supplierColumn = new DataGridViewTextBoxColumn
                        {
                            Name = "SupplierDisplay",
                            HeaderText = "Supplier",
                            ReadOnly = false,
                            DataPropertyName = "SupplierName"
                        };
                        dataGridView.Columns.Add(supplierColumn);
                    }
                    
                    dataGridView.Columns["Name"].ReadOnly = true;
                    dataGridView.Columns["Unit"].ReadOnly = true;
                    dataGridView.Columns["UnitPrice"].ReadOnly = false;
                    dataGridView.Columns["SupplierDisplay"].ReadOnly = false;
                    
                    dataGridView.Columns["Name"].DisplayIndex = 0;
                    dataGridView.Columns["Unit"].DisplayIndex = 1;
                    dataGridView.Columns["UnitPrice"].DisplayIndex = 2;
                    dataGridView.Columns["SupplierDisplay"].DisplayIndex = 3;
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
            if (e.RowIndex >= 0)
            {
                try
                {
                    var ingredient = (Ingredient)dataGridView.Rows[e.RowIndex].DataBoundItem;
                    
                    if (e.ColumnIndex == dataGridView.Columns["UnitPrice"].Index)
                    {
                        if (dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null && 
                            decimal.TryParse(dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), 
                            out decimal newPrice))
                        {
                            ingredient.UnitPrice = newPrice;
                            DatabaseContext.UpdateIngredient(ingredient);
                            dataGridView.InvalidateRow(e.RowIndex);
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid numeric price (without currency symbol).\n\nExample: 0.85 instead of RM0.85", "Invalid Price Format", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            dataGridView.CancelEdit();
                            dataGridView.InvalidateRow(e.RowIndex);
                        }
                    }
                    else if (e.ColumnIndex == dataGridView.Columns["SupplierDisplay"].Index)
                    {
                        var newSupplierName = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "";
                        ingredient.SupplierName = newSupplierName;
                        
                        DatabaseContext.UpdateIngredient(ingredient);
                        dataGridView.InvalidateRow(e.RowIndex);
                        
                        MessageBox.Show($"Supplier updated to: {newSupplierName}", "Supplier Updated", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating ingredient: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ManageSuppliers()
        {
            using (var form = new Form())
            {
                form.Text = "Manage Suppliers";
                form.Size = new System.Drawing.Size(500, 400);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;

                var dataGridView = new DataGridView();
                dataGridView.Location = new System.Drawing.Point(20, 50);
                dataGridView.Size = new System.Drawing.Size(440, 250);
                dataGridView.ReadOnly = false;
                dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                var suppliers = new[]
                {
                    new { Name = "Local Market", Contact = "John Doe", Phone = "555-0101" },
                    new { Name = "Wholesale Foods Inc", Contact = "Jane Smith", Phone = "555-0102" },
                    new { Name = "Fresh Produce Co", Contact = "Bob Wilson", Phone = "555-0103" },
                    new { Name = "Quality Meats Ltd", Contact = "Alice Brown", Phone = "555-0104" }
                };

                dataGridView.DataSource = suppliers.ToList();

                var lblTitle = new Label();
                lblTitle.Text = "Suppliers (Double-click to select)";
                lblTitle.Location = new System.Drawing.Point(20, 20);
                lblTitle.Size = new System.Drawing.Size(200, 20);
                lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

                var btnSelect = new Button();
                btnSelect.Text = "Select Supplier";
                btnSelect.Location = new System.Drawing.Point(20, 310);
                btnSelect.Size = new System.Drawing.Size(100, 30);
                btnSelect.Click += (s, e) =>
                {
                    if (dataGridView.SelectedRows.Count > 0)
                    {
                        var selectedSupplier = dataGridView.SelectedRows[0].Cells["Name"].Value.ToString();
                        MessageBox.Show($"Selected supplier: {selectedSupplier}\n\nThis would assign the supplier to the selected ingredient in the main form.", 
                            "Supplier Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };

                var btnClose = new Button();
                btnClose.Text = "Close";
                btnClose.Location = new System.Drawing.Point(360, 310);
                btnClose.Size = new System.Drawing.Size(100, 30);
                btnClose.DialogResult = DialogResult.OK;

                form.Controls.AddRange(new Control[] { lblTitle, dataGridView, btnSelect, btnClose });
                form.AcceptButton = btnClose;

                form.ShowDialog();
            }
        }

        private void EditSelectedIngredientPrice()
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var ingredient = (Ingredient)dataGridView.SelectedRows[0].DataBoundItem;
                
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
                        dataGridView.Invalidate();
                        MessageBox.Show($"Price updated to {currencySymbol} {newPrice:0.00} per {ingredient.Unit}!", "Success", 
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
                form.Size = new System.Drawing.Size(350, 250);
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
                var txtSupplier = new TextBox { Location = new System.Drawing.Point(120, 107), Size = new System.Drawing.Size(150, 20), PlaceholderText = "Optional" };
                
                var btnOk = new Button { Text = "Add", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(120, 140) };
                var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(200, 140) };
                
                form.Controls.AddRange(new Control[] { 
                    lblName, txtName, lblUnit, txtUnit, lblPrice, txtPrice, 
                    lblSupplier, txtSupplier,
                    btnOk, btnCancel 
                });
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
                        UnitPrice = price,
                        SupplierName = txtSupplier.Text
                    };
                    
                    DatabaseContext.InsertIngredient(newIngredient);
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