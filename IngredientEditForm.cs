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

        // New fields for purchase calculation
        private TextBox txtPurchasePrice;
        private TextBox txtPurchaseQuantity;
        private ComboBox cmbPurchaseUnit;
        private Button btnCalculate;

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
            LoadPurchaseUnits();
        }

        public IngredientEditForm(Ingredient ingredient)
        {
            currentIngredient = ingredient;
            isEditMode = true;
            InitializeComponent();
            LoadSuppliers();
            LoadCategories();
            LoadUnits();
            LoadPurchaseUnits();
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

    // New purchase calculation fields
    this.txtPurchasePrice = new TextBox();
    this.txtPurchaseQuantity = new TextBox();
    this.cmbPurchaseUnit = new ComboBox();
    this.btnCalculate = new Button();

    // Form - increased height to accommodate new fields
    this.SuspendLayout();
    this.ClientSize = new System.Drawing.Size(400, 400);
    this.Text = isEditMode ? "Edit Ingredient" : "Add New Ingredient";
    this.StartPosition = FormStartPosition.CenterParent;
    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    this.MaximizeBox = false;

    int yPos = 20;

    // Name
    var lblName = new Label { Text = "Name:", Location = new System.Drawing.Point(20, yPos), AutoSize = true };
    this.txtName.Location = new System.Drawing.Point(120, yPos - 3);
    this.txtName.Size = new System.Drawing.Size(250, 20);
    this.txtName.MaxLength = 100;

    yPos += 30;

    // Unit
    var lblUnit = new Label { Text = "Unit:", Location = new System.Drawing.Point(20, yPos), AutoSize = true };
    this.cmbUnit.Location = new System.Drawing.Point(120, yPos - 3);
    this.cmbUnit.Size = new System.Drawing.Size(250, 20);
    this.cmbUnit.DropDownStyle = ComboBoxStyle.DropDownList;

    yPos += 30;

    // Unit Price
    var lblUnitPrice = new Label { Text = "Unit Price:", Location = new System.Drawing.Point(20, yPos), AutoSize = true };
    this.txtUnitPrice.Location = new System.Drawing.Point(120, yPos - 3);
    this.txtUnitPrice.Size = new System.Drawing.Size(250, 20);
    this.txtUnitPrice.PlaceholderText = "0.0000";

    yPos += 30;

    // Category
    var lblCategory = new Label { Text = "Category:", Location = new System.Drawing.Point(20, yPos), AutoSize = true };
    this.cmbCategory.Location = new System.Drawing.Point(120, yPos - 3);
    this.cmbCategory.Size = new System.Drawing.Size(250, 20);
    this.cmbCategory.DropDownStyle = ComboBoxStyle.DropDown;

    yPos += 30;

    // Supplier
    var lblSupplier = new Label { Text = "Supplier:", Location = new System.Drawing.Point(20, yPos), AutoSize = true };
    this.cmbSupplier.Location = new System.Drawing.Point(120, yPos - 3);
    this.cmbSupplier.Size = new System.Drawing.Size(250, 20);
    this.cmbSupplier.DropDownStyle = ComboBoxStyle.DropDownList;

    yPos += 40;

    // Purchase Calculation Section
    var lblPurchaseSection = new Label 
    { 
        Text = "Purchase Calculation (from receipt):", 
        Location = new System.Drawing.Point(20, yPos), 
        AutoSize = true,
        Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold)
    };
    this.Controls.Add(lblPurchaseSection);

    yPos += 25;

    // Purchase Price
    var lblPurchasePrice = new Label { Text = "Purchase Price:", Location = new System.Drawing.Point(20, yPos), AutoSize = true };
    this.txtPurchasePrice.Location = new System.Drawing.Point(120, yPos - 3);
    this.txtPurchasePrice.Size = new System.Drawing.Size(100, 20);
    this.txtPurchasePrice.PlaceholderText = "0.00";

    yPos += 30;

    // Purchase Quantity - FIXED: Smaller font for better spacing
    var lblPurchaseQuantity = new Label 
    { 
        Text = "Purchase Quantity:", 
        Location = new System.Drawing.Point(20, yPos), 
        AutoSize = true,
        Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size - 1) // FIXED: Proper font constructor
    };
    this.txtPurchaseQuantity.Location = new System.Drawing.Point(120, yPos - 3);
    this.txtPurchaseQuantity.Size = new System.Drawing.Size(100, 20);
    this.txtPurchaseQuantity.PlaceholderText = "0";

    yPos += 30;

    // Purchase Unit
    var lblPurchaseUnit = new Label { Text = "Purchase Unit:", Location = new System.Drawing.Point(20, yPos), AutoSize = true };
    this.cmbPurchaseUnit.Location = new System.Drawing.Point(120, yPos - 3);
    this.cmbPurchaseUnit.Size = new System.Drawing.Size(100, 20);
    this.cmbPurchaseUnit.DropDownStyle = ComboBoxStyle.DropDownList;

    yPos += 30;

    // Calculate Button
    this.btnCalculate.Text = "Calculate";
    this.btnCalculate.Location = new System.Drawing.Point(120, yPos);
    this.btnCalculate.Size = new System.Drawing.Size(80, 25);
    this.btnCalculate.Click += (s, e) => CalculateUnitPrice();

    yPos += 40;

    // Buttons
    this.btnSave.Text = "Save";
    this.btnSave.Location = new System.Drawing.Point(120, yPos);
    this.btnSave.Size = new System.Drawing.Size(80, 30);
    this.btnSave.Click += (s, e) => SaveIngredient();

    this.btnCancel.Text = "Cancel";
    this.btnCancel.Location = new System.Drawing.Point(210, yPos);
    this.btnCancel.Size = new System.Drawing.Size(80, 30);
    this.btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

    this.Controls.AddRange(new Control[] {
        lblName, txtName, lblUnit, cmbUnit, lblUnitPrice, txtUnitPrice,
        lblCategory, cmbCategory, lblSupplier, cmbSupplier,
        lblPurchasePrice, txtPurchasePrice, 
        lblPurchaseQuantity, txtPurchaseQuantity,
        lblPurchaseUnit, cmbPurchaseUnit,
        btnCalculate, btnSave, btnCancel
    });

    this.AcceptButton = btnSave;
    this.CancelButton = btnCancel;

    this.ResumeLayout(false);
    this.PerformLayout();
}

        private void LoadPurchaseUnits()
        {
            try
            {
                cmbPurchaseUnit.Items.Clear();
                
                // Common purchase units
                var purchaseUnits = new List<string> { "gram", "kg", "piece", "ml", "liter", "oz", "lb", "pack", "box", "case" };
                foreach (var unit in purchaseUnits)
                {
                    cmbPurchaseUnit.Items.Add(unit);
                }

                if (cmbPurchaseUnit.Items.Count > 0)
                    cmbPurchaseUnit.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase units: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateUnitPrice()
        {
            // Validate purchase price
            if (!decimal.TryParse(txtPurchasePrice.Text, out decimal purchasePrice) || purchasePrice <= 0)
            {
                MessageBox.Show("Please enter a valid purchase price greater than 0.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPurchasePrice.Focus();
                txtPurchasePrice.SelectAll();
                return;
            }

            // Validate purchase quantity
            if (!decimal.TryParse(txtPurchaseQuantity.Text, out decimal purchaseQuantity) || purchaseQuantity <= 0)
            {
                MessageBox.Show("Please enter a valid purchase quantity greater than 0.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPurchaseQuantity.Focus();
                txtPurchaseQuantity.SelectAll();
                return;
            }

            // Validate purchase unit
            if (cmbPurchaseUnit.SelectedItem == null)
            {
                MessageBox.Show("Please select a purchase unit.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbPurchaseUnit.Focus();
                return;
            }

            try
            {
                string purchaseUnit = cmbPurchaseUnit.SelectedItem.ToString();
                
                // Auto-update the main unit field to match purchase unit
                bool unitChanged = false;
                if (cmbUnit.SelectedItem == null || cmbUnit.SelectedItem.ToString() != purchaseUnit)
                {
                    // Try to select the purchase unit in the main unit dropdown
                    foreach (string unit in cmbUnit.Items)
                    {
                        if (unit.Equals(purchaseUnit, StringComparison.OrdinalIgnoreCase))
                        {
                            cmbUnit.SelectedItem = unit;
                            unitChanged = true;
                            break;
                        }
                    }
                    
                    // If purchase unit doesn't exist in main units, add it
                    if (!unitChanged)
                    {
                        cmbUnit.Items.Add(purchaseUnit);
                        cmbUnit.SelectedItem = purchaseUnit;
                        unitChanged = true;
                    }
                }

                string targetUnit = cmbUnit.SelectedItem.ToString();
                
                // Calculate price per target unit
                decimal pricePerUnit = CalculatePriceConversion(purchasePrice, purchaseQuantity, purchaseUnit, targetUnit);
                
                // Update the unit price field
                txtUnitPrice.Text = pricePerUnit.ToString("F4");
                
                string message = $"Calculated unit price: {AppSettings.CurrencySymbol}{pricePerUnit:F4} per {targetUnit}";
                if (unitChanged)
                {
                    message += $"\n\nUnit automatically changed to: {targetUnit}";
                }
                
                MessageBox.Show(message, "Calculation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating unit price: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private decimal CalculatePriceConversion(decimal purchasePrice, decimal purchaseQuantity, string purchaseUnit, string targetUnit)
        {
            // Convert both units to base units for calculation
            decimal purchaseInBase = ConvertToBaseUnit(purchaseQuantity, purchaseUnit);
            decimal targetInBase = ConvertToBaseUnit(1, targetUnit);
            
            // Calculate price per target unit
            decimal pricePerBaseUnit = purchasePrice / purchaseInBase;
            decimal pricePerTargetUnit = pricePerBaseUnit * targetInBase;
            
            return pricePerTargetUnit;
        }

        private decimal ConvertToBaseUnit(decimal quantity, string unit)
        {
            // Convert various units to base units (grams for weight, ml for volume, pieces for count)
            switch (unit.ToLower())
            {
                // Weight conversions (base: grams)
                case "gram": return quantity;
                case "kg": return quantity * 1000;
                case "oz": return quantity * 28.3495m;
                case "lb": return quantity * 453.592m;
                
                // Volume conversions (base: ml)
                case "ml": return quantity;
                case "liter": return quantity * 1000;
                
                // Count-based items (base: pieces)
                case "piece":
                case "pack":
                case "box":
                case "case":
                    return quantity;
                    
                default:
                    // If unit is unknown, assume it's the same (1:1 conversion)
                    return quantity;
            }
        }

        private void LoadIngredientData()
        {
            try
            {
                txtName.Text = currentIngredient.Name;
                txtUnitPrice.Text = currentIngredient.UnitPrice.ToString("F4");
                cmbCategory.Text = currentIngredient.Category;

                // Select unit
                if (!string.IsNullOrEmpty(currentIngredient.Unit))
                {
                    foreach (string unit in cmbUnit.Items)
                    {
                        if (unit.Equals(currentIngredient.Unit, StringComparison.OrdinalIgnoreCase))
                        {
                            cmbUnit.SelectedItem = unit;
                            break;
                        }
                    }
                }

                // Select supplier
                if (currentIngredient.SupplierId.HasValue && currentIngredient.SupplierId.Value > 0)
                {
                    bool supplierFound = false;
                    foreach (object item in cmbSupplier.Items)
                    {
                        if (item is Supplier supplier && supplier.Id == currentIngredient.SupplierId.Value)
                        {
                            cmbSupplier.SelectedItem = item;
                            supplierFound = true;
                            break;
                        }
                    }
                    
                    if (!supplierFound && cmbSupplier.Items.Count > 0)
                    {
                        cmbSupplier.SelectedIndex = 0;
                    }
                }
                else
                {
                    // No supplier assigned
                    if (cmbSupplier.Items.Count > 0)
                        cmbSupplier.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ingredient data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = DatabaseContext.GetAllSuppliers();
                
                cmbSupplier.Items.Clear();
                
                // Add "No Supplier" option
                cmbSupplier.Items.Add(new Supplier { Id = 0, Name = "(No Supplier)" });
                
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
                cmbCategory.Items.Add("");
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

                // Try to load from database
                try
                {
                    var dbUnits = DatabaseContext.GetIngredientUnits();
                    foreach (var unit in dbUnits)
                    {
                        if (!cmbUnit.Items.Contains(unit))
                        {
                            cmbUnit.Items.Add(unit);
                        }
                    }
                }
                catch
                {
                    // If method doesn't exist, just use common units
                }

                if (cmbUnit.Items.Count > 0)
                    cmbUnit.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading units: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveIngredient()
        {
            // Validate name
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter an ingredient name.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            // Validate unit
            if (cmbUnit.SelectedItem == null)
            {
                MessageBox.Show("Please select a unit.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbUnit.Focus();
                return;
            }

            // Validate unit price
            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice < 0)
            {
                MessageBox.Show("Please enter a valid unit price.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtUnitPrice.Focus();
                txtUnitPrice.SelectAll();
                return;
            }

            try
            {
                currentIngredient.Name = txtName.Text.Trim();
                currentIngredient.Unit = cmbUnit.SelectedItem.ToString();
                currentIngredient.UnitPrice = unitPrice;
                currentIngredient.Category = cmbCategory.Text.Trim();

                // Handle supplier
                if (cmbSupplier.SelectedItem is Supplier selectedSupplier)
                {
                    if (selectedSupplier.Id > 0)
                    {
                        currentIngredient.SupplierId = selectedSupplier.Id;
                    }
                    else
                    {
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