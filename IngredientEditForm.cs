using System;
using System.Windows.Forms;
using System.Linq;

namespace CostChef
{
    public partial class IngredientEditForm : Form
    {
        private TextBox txtName;
        private TextBox txtUnit;
        private TextBox txtUnitPrice;
        private ComboBox cmbCategory;
        private ComboBox cmbSupplier;
        private Button btnSave;
        private Button btnCancel;

        private Ingredient currentIngredient;
        private bool isEditMode;

        public IngredientEditForm()
        {
            currentIngredient = new Ingredient();
            isEditMode = false;
            InitializeComponent();
            LoadSuppliers();
            LoadCategories();
        }

        public IngredientEditForm(Ingredient ingredient)
        {
            currentIngredient = ingredient;
            isEditMode = true;
            InitializeComponent();
            LoadSuppliers();
            LoadCategories();
            LoadIngredientData();
        }

        private void InitializeComponent()
        {
            this.txtName = new TextBox();
            this.txtUnit = new TextBox();
            this.txtUnitPrice = new TextBox();
            this.cmbCategory = new ComboBox();
            this.cmbSupplier = new ComboBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Text = isEditMode ? "Edit Ingredient" : "Add New Ingredient";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Name
            var lblName = new Label { Text = "Name:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            this.txtName.Location = new System.Drawing.Point(120, 17);
            this.txtName.Size = new System.Drawing.Size(250, 20);
            this.txtName.MaxLength = 100;

            // Unit
            var lblUnit = new Label { Text = "Unit:", Location = new System.Drawing.Point(20, 50), AutoSize = true };
            this.txtUnit.Location = new System.Drawing.Point(120, 47);
            this.txtUnit.Size = new System.Drawing.Size(250, 20);
            this.txtUnit.MaxLength = 20;
            this.txtUnit.PlaceholderText = "e.g., gram, piece, ml";

            // Unit Price
            var lblUnitPrice = new Label { Text = "Unit Price:", Location = new System.Drawing.Point(20, 80), AutoSize = true };
            this.txtUnitPrice.Location = new System.Drawing.Point(120, 77);
            this.txtUnitPrice.Size = new System.Drawing.Size(250, 20);
            this.txtUnitPrice.PlaceholderText = "0.00";

            // Category
            var lblCategory = new Label { Text = "Category:", Location = new System.Drawing.Point(20, 110), AutoSize = true };
            this.cmbCategory.Location = new System.Drawing.Point(120, 107);
            this.cmbCategory.Size = new System.Drawing.Size(250, 20);
            this.cmbCategory.DropDownStyle = ComboBoxStyle.DropDown;

            // Supplier
            var lblSupplier = new Label { Text = "Supplier:", Location = new System.Drawing.Point(20, 140), AutoSize = true };
            this.cmbSupplier.Location = new System.Drawing.Point(120, 137);
            this.cmbSupplier.Size = new System.Drawing.Size(250, 20);
            this.cmbSupplier.DropDownStyle = ComboBoxStyle.DropDownList;

            // Buttons
            this.btnSave.Text = "Save";
            this.btnSave.DialogResult = DialogResult.OK;
            this.btnSave.Location = new System.Drawing.Point(120, 180);
            this.btnSave.Size = new System.Drawing.Size(80, 30);
            this.btnSave.Click += (s, e) => SaveIngredient();

            this.btnCancel.Text = "Cancel";
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(210, 180);
            this.btnCancel.Size = new System.Drawing.Size(80, 30);

            this.Controls.AddRange(new Control[] {
                lblName, txtName, lblUnit, txtUnit, lblUnitPrice, txtUnitPrice,
                lblCategory, cmbCategory, lblSupplier, cmbSupplier,
                btnSave, btnCancel
            });

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadIngredientData()
        {
            txtName.Text = currentIngredient.Name;
            txtUnit.Text = currentIngredient.Unit;
            txtUnitPrice.Text = currentIngredient.UnitPrice.ToString("F4");
            cmbCategory.Text = currentIngredient.Category;

            // Select supplier if exists
            if (currentIngredient.SupplierId.HasValue)
            {
                foreach (Supplier supplier in cmbSupplier.Items)
                {
                    if (supplier.Id == currentIngredient.SupplierId.Value)
                    {
                        cmbSupplier.SelectedItem = supplier;
                        break;
                    }
                }
            }
        }

      private void LoadSuppliers()
{
    try
    {
        var suppliers = DatabaseContext.GetAllSuppliers();
        cmbSupplier.Items.Clear();
        
        // Add empty supplier option first - use a proper Supplier object
        cmbSupplier.Items.Add(new Supplier { Id = 0, Name = "" });
        
        // Add actual suppliers
        foreach (var supplier in suppliers)
        {
            cmbSupplier.Items.Add(supplier);
        }
        
        cmbSupplier.DisplayMember = "Name";
        cmbSupplier.ValueMember = "Id";

        if (cmbSupplier.Items.Count > 0)
            cmbSupplier.SelectedIndex = 0;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

        private void LoadCategories()
        {
            try
            {
                var categories = DatabaseContext.GetIngredientCategories();
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add(""); // Empty option
                foreach (var category in categories)
                {
                    cmbCategory.Items.Add(category);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveIngredient()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter an ingredient name.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                MessageBox.Show("Please enter a unit.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtUnit.Focus();
                return;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice < 0)
            {
                MessageBox.Show("Please enter a valid unit price.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtUnitPrice.Focus();
                return;
            }

            try
            {
                currentIngredient.Name = txtName.Text.Trim();
                currentIngredient.Unit = txtUnit.Text.Trim();
                currentIngredient.UnitPrice = unitPrice;
                currentIngredient.Category = cmbCategory.Text.Trim();

                // Set supplier (optional)
             // Set supplier (optional)
if (cmbSupplier.SelectedItem != null)
{
    // Check if it's a Supplier object (not the empty option)
    if (cmbSupplier.SelectedItem is Supplier selectedSupplier)
    {
        currentIngredient.SupplierId = selectedSupplier.Id;
    }
    else
    {
        // It's the empty option or some other non-supplier item
        currentIngredient.SupplierId = null;
    }
}
else
{
    currentIngredient.SupplierId = null;
}

                if (isEditMode)
                {
                    DatabaseContext.UpdateIngredient(currentIngredient);
                    MessageBox.Show("Ingredient updated successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DatabaseContext.InsertIngredient(currentIngredient);
                    MessageBox.Show("Ingredient added successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving ingredient: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}