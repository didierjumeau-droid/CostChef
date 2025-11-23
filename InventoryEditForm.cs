using System;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CostChef
{
    public partial class InventoryEditForm : Form
    {
        private InventoryLevel? _inventoryItem;
        private readonly int _ingredientId;

        private Label lblIngredientName;
        private Label lblCurrentStock;
        private TextBox txtCurrentStock;
        private TextBox txtMinimumStock;
        private TextBox txtMaximumStock;
        private Label lblUnit;
        private Label lblUnitCost;
        private Label lblTotalValue;
        private Label lblLastUpdated;
        private Button btnSave;
        private Button btnCancel;

        public InventoryEditForm(int ingredientId)
        {
            _ingredientId = ingredientId;
            InitializeComponent();
            LoadInventoryItem();
        }

        private void InitializeComponent()
        {
            this.lblIngredientName = new Label();
            this.lblCurrentStock = new Label();
            this.txtCurrentStock = new TextBox();
            this.txtMinimumStock = new TextBox();
            this.txtMaximumStock = new TextBox();
            this.lblUnit = new Label();
            this.lblUnitCost = new Label();
            this.lblTotalValue = new Label();
            this.lblLastUpdated = new Label();
            this.btnSave = new Button();
            this.btnCancel = new Button();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(520, 360);
            this.Text = "Edit Inventory - CostChef v3.0";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Ingredient Name
            this.lblIngredientName.AutoSize = true;
            this.lblIngredientName.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.lblIngredientName.Location = new Point(20, 20);
            this.lblIngredientName.Text = "Ingredient";

            // Current Stock
            this.lblCurrentStock.AutoSize = true;
            this.lblCurrentStock.Location = new Point(20, 60);
            this.lblCurrentStock.Text = "Current Stock:";
            this.txtCurrentStock.Location = new Point(150, 57);
            this.txtCurrentStock.Size = new Size(100, 20);

            // Minimum Stock
            var lblMin = new Label
            {
                AutoSize = true,
                Location = new Point(20, 90),
                Text = "Minimum Stock:"
            };
            this.txtMinimumStock.Location = new Point(150, 87);
            this.txtMinimumStock.Size = new Size(100, 20);

            // Maximum Stock
            var lblMax = new Label
            {
                AutoSize = true,
                Location = new Point(20, 120),
                Text = "Maximum Stock:"
            };
            this.txtMaximumStock.Location = new Point(150, 117);
            this.txtMaximumStock.Size = new Size(100, 20);

            // Unit
            this.lblUnit.AutoSize = true;
            this.lblUnit.Location = new Point(20, 150);
            this.lblUnit.Text = "Unit: -";

            // Unit Cost
            this.lblUnitCost.AutoSize = true;
            this.lblUnitCost.Location = new Point(20, 180);
            this.lblUnitCost.Text = "Unit Cost: -";

            // Total Value
            this.lblTotalValue.AutoSize = true;
            this.lblTotalValue.Location = new Point(20, 210);
            this.lblTotalValue.Text = "Total Value: -";

            // Last Updated
            this.lblLastUpdated.AutoSize = true;
            this.lblLastUpdated.Location = new Point(20, 240);
            this.lblLastUpdated.Text = "Last Updated: -";

            // Save button
            this.btnSave.Text = "Save";
            this.btnSave.Location = new Point(260, 290);
            this.btnSave.Size = new Size(80, 30);
            this.btnSave.Click += btnSave_Click;

            // Cancel button
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Location = new Point(350, 290);
            this.btnCancel.Size = new Size(80, 30);
            this.btnCancel.Click += btnCancel_Click;

            this.Controls.Add(this.lblIngredientName);
            this.Controls.Add(this.lblCurrentStock);
            this.Controls.Add(this.txtCurrentStock);
            this.Controls.Add(lblMin);
            this.Controls.Add(this.txtMinimumStock);
            this.Controls.Add(lblMax);
            this.Controls.Add(this.txtMaximumStock);
            this.Controls.Add(this.lblUnit);
            this.Controls.Add(this.lblUnitCost);
            this.Controls.Add(this.lblTotalValue);
            this.Controls.Add(this.lblLastUpdated);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);

            this.ResumeLayout(false);
        }

        private void LoadInventoryItem()
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                connection.Open();

                const string sql = @"
                    SELECT
                        il.ingredient_id,
                        i.name AS ingredient_name,
                        il.current_stock,
                        il.minimum_stock,
                        il.maximum_stock,
                        il.unit_cost,
                        il.last_updated,
                        i.unit AS unit
                    FROM inventory_levels il
                    INNER JOIN ingredients i ON i.id = il.ingredient_id
                    WHERE il.ingredient_id = @ingredientId;
                ";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ingredientId", _ingredientId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _inventoryItem = new InventoryLevel
                            {
                                IngredientId = _ingredientId,
                                IngredientName = reader["ingredient_name"]?.ToString() ?? string.Empty,
                                CurrentStock = reader["current_stock"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(reader["current_stock"]),
                                MinimumStock = reader["minimum_stock"] == DBNull.Value
                                    ? (decimal?)null
                                    : Convert.ToDecimal(reader["minimum_stock"]),
                                MaximumStock = reader["maximum_stock"] == DBNull.Value
                                    ? (decimal?)null
                                    : Convert.ToDecimal(reader["maximum_stock"]),
                                UnitCost = reader["unit_cost"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(reader["unit_cost"]),
                                LastUpdated = reader["last_updated"] == DBNull.Value
                                    ? DateTime.MinValue
                                    : Convert.ToDateTime(reader["last_updated"]),
                                Unit = reader["unit"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }

            if (_inventoryItem == null)
            {
                MessageBox.Show("Inventory item not found.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            var item = _inventoryItem;
            this.lblIngredientName.Text = item.IngredientName;
            this.txtCurrentStock.Text = item.CurrentStock.ToString("0.##");
            this.txtMinimumStock.Text = item.MinimumStock.HasValue
                ? item.MinimumStock.Value.ToString("0.##")
                : string.Empty;
            this.txtMaximumStock.Text = item.MaximumStock.HasValue
                ? item.MaximumStock.Value.ToString("0.##")
                : string.Empty;

            this.lblUnit.Text = $"Unit: {item.Unit}";

            var currency = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            currency.CurrencySymbol = AppSettings.CurrencySymbol ?? "$";

            this.lblUnitCost.Text = $"Unit Cost: {item.UnitCost.ToString("C4", currency)}";
            this.lblTotalValue.Text = $"Total Value: {item.TotalValue.ToString("C2", currency)}";
            this.lblLastUpdated.Text = item.LastUpdated == DateTime.MinValue
                ? "Last Updated: (not set)"
                : $"Last Updated: {item.LastUpdated:g}";

            this.Text = $"Edit {item.IngredientName} - CostChef v3.0";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_inventoryItem == null)
            {
                MessageBox.Show("No inventory item to save.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!decimal.TryParse(txtCurrentStock.Text.Trim(), out var newStock) || newStock < 0)
            {
                MessageBox.Show("Invalid current stock value.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal? minStock = null;
            decimal? maxStock = null;

            if (decimal.TryParse(txtMinimumStock.Text.Trim(), out var min))
                minStock = min;
            if (decimal.TryParse(txtMaximumStock.Text.Trim(), out var max))
                maxStock = max;

            if (minStock.HasValue && maxStock.HasValue && minStock.Value > maxStock.Value)
            {
                MessageBox.Show("Minimum stock cannot be greater than maximum stock.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DatabaseContext.UpdateInventoryLevel(
                ingredientId: _inventoryItem.IngredientId,
                newStock: newStock,
                minStock: minStock,
                maxStock: maxStock,
                changeType: "manual_edit",
                reason: "Manual edit from InventoryEditForm",
                recipeId: null
            );

            MessageBox.Show("Inventory saved successfully.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
