using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace CostChef
{
    public partial class IngredientEditForm : Form
    {
        private TextBox txtName;
        private ComboBox cmbUnit;
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
            LoadUnits();
        }

        public IngredientEditForm(Ingredient ingredient)
        {
            currentIngredient = ingredient;
            isEditMode = true;
            InitializeComponent();
            LoadSuppliers();
            LoadCategories();
            LoadUnits();
            LoadIngredientData();
        }

        private void InitializeComponent()
        {
            this.txtName = new TextBox();
            this.cmbUnit = new ComboBox();
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

            // Unit
            var lblUnit = new Label { Text = "Unit:", Location = new System.Drawing.Point(20, 50), AutoSize = true };
            this.cmbUnit.Location = new System.Drawing.Point(120, 47);
            this.cmbUnit.Size = new System.Drawing.Size(250, 20);
            this.cmbUnit.DropDownStyle = ComboBoxStyle.DropDownList;

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
            this.btnSave.Location = new System.Drawing.Point(120, 180);
            this.btnSave.Size = new System.Drawing.Size(80, 30);
            this.btnSave.Click += (s, e) => SaveIngredient();

            this.btnCancel.Text = "Cancel";
            this.btnCancel.Location = new System.Drawing.Point(210, 180);
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] {
                lblName, txtName, lblUnit, cmbUnit, lblUnitPrice, txtUnitPrice,
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
            txtUnitPrice.Text = currentIngredient.UnitPrice.ToString("F4");
            cmbCategory.Text = currentIngredient.Category;

            // Select unit
            if (!string.IsNullOrEmpty(currentIngredient.Unit))
            {
                cmbUnit.SelectedItem = currentIngredient.Unit;
            }

            // Select supplier
            if (currentIngredient.SupplierId.HasValue)
            {
                var supplier = cmbSupplier.Items.Cast<Supplier>().FirstOrDefault(s => s.Id == currentIngredient.SupplierId.Value);
                if (supplier != null)
                {
                    cmbSupplier.SelectedItem = supplier;
                }
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = DatabaseContext.GetAllSuppliers();
                cmbSupplier.Items.Clear();
                
                // Add empty option
                cmbSupplier.Items.Add(new Supplier { Id = 0, Name = "(No Supplier)" });
                
                // Add suppliers
                foreach (var supplier in suppliers)
                {
                    cmbSupplier.Items.Add(supplier);
                }
                
                cmbSupplier.DisplayMember = "Name";
                cmbSupplier.ValueMember = "Id";
                cmbSupplier.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = DatabaseContext.GetIngredientCategories();
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add("");
                foreach (var category in categories)
                {
                    cmbCategory.Items.Add(category);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUnits()
        {
            try
            {
                cmbUnit.Items.Clear();
                
                // Common units
                var commonUnits = new List<string> { "gram", "kg", "piece", "ml", "liter", "cup", "tbsp", "tsp", "oz", "lb" };
                foreach (var unit in commonUnits)
                {
                    cmbUnit.Items.Add(unit);
                }

                // Database units
                try
                {
                    var dbUnits = DatabaseContext.GetIngredientUnits();
                    foreach (var unit in dbUnits)
                    {
                        if (!cmbUnit.Items.Contains(unit))
                            cmbUnit.Items.Add(unit);
                    }
                }
                catch { }

                cmbUnit.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading units: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveIngredient()
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter an ingredient name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cmbUnit.SelectedItem == null)
            {
                MessageBox.Show("Please select a unit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice < 0)
            {
                MessageBox.Show("Please enter a valid unit price.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                currentIngredient.Name = txtName.Text.Trim();
                currentIngredient.Unit = cmbUnit.SelectedItem.ToString();
                currentIngredient.UnitPrice = unitPrice;
                currentIngredient.Category = cmbCategory.Text.Trim();

                // Supplier
                if (cmbSupplier.SelectedItem is Supplier selectedSupplier && selectedSupplier.Id > 0)
                {
                    currentIngredient.SupplierId = selectedSupplier.Id;
                }
                else
                {
                    currentIngredient.SupplierId = null;
                }

                // Save
                if (isEditMode)
                {
                    DatabaseContext.UpdateIngredient(currentIngredient);
                    MessageBox.Show("Updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DatabaseContext.InsertIngredient(currentIngredient);
                    MessageBox.Show("Added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}