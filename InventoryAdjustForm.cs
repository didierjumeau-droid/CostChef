using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CostChef
{
    public partial class InventoryAdjustForm : Form
    {
        private InventoryLevel _inventoryItem;

        private Label lblIngredientName;
        private Label lblCurrentStock;
        private Label lblAdjustmentType;
        private ComboBox cmbAdjustmentType;
        private Label lblAmount;
        private TextBox txtAmount;
        private Label lblReason;
        private TextBox txtReason;
        private Button btnApply;
        private Button btnCancel;

        public InventoryAdjustForm(InventoryLevel item)
        {
            _inventoryItem = item;
            InitializeComponent();
            LoadItem();
        }

        private void InitializeComponent()
        {
            this.lblIngredientName = new Label();
            this.lblCurrentStock = new Label();
            this.lblAdjustmentType = new Label();
            this.cmbAdjustmentType = new ComboBox();
            this.lblAmount = new Label();
            this.txtAmount = new TextBox();
            this.lblReason = new Label();
            this.txtReason = new TextBox();
            this.btnApply = new Button();
            this.btnCancel = new Button();

            this.SuspendLayout();

            this.ClientSize = new System.Drawing.Size(480, 300);
            this.Text = "Adjust Inventory";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Ingredient name
            this.lblIngredientName.AutoSize = true;
            this.lblIngredientName.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.lblIngredientName.Location = new Point(20, 20);

            // Current stock
            this.lblCurrentStock.AutoSize = true;
            this.lblCurrentStock.Location = new Point(20, 55);

            // Adjustment type
            this.lblAdjustmentType.AutoSize = true;
            this.lblAdjustmentType.Location = new Point(20, 90);
            this.lblAdjustmentType.Text = "Adjustment Type:";

            this.cmbAdjustmentType.Location = new Point(150, 87);
            this.cmbAdjustmentType.Size = new Size(200, 21);
            this.cmbAdjustmentType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbAdjustmentType.Items.AddRange(new object[]
            {
                "addition",
                "removal",
                "waste",
                "correction"
            });
            this.cmbAdjustmentType.SelectedIndex = 0;

            // Amount
            this.lblAmount.AutoSize = true;
            this.lblAmount.Location = new Point(20, 125);
            this.lblAmount.Text = "Amount:";

            this.txtAmount.Location = new Point(150, 122);
            this.txtAmount.Size = new Size(100, 20);

            // Reason
            this.lblReason.AutoSize = true;
            this.lblReason.Location = new Point(20, 160);
            this.lblReason.Text = "Reason (optional):";

            this.txtReason.Location = new Point(150, 157);
            this.txtReason.Size = new Size(280, 20);

            // Apply
            this.btnApply.Text = "Apply";
            this.btnApply.Location = new Point(230, 210);
            this.btnApply.Size = new Size(80, 30);
            this.btnApply.Click += btnApply_Click;

            // Cancel
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Location = new Point(320, 210);
            this.btnCancel.Size = new Size(80, 30);
            this.btnCancel.Click += btnCancel_Click;

            this.Controls.Add(this.lblIngredientName);
            this.Controls.Add(this.lblCurrentStock);
            this.Controls.Add(this.lblAdjustmentType);
            this.Controls.Add(this.cmbAdjustmentType);
            this.Controls.Add(this.lblAmount);
            this.Controls.Add(this.txtAmount);
            this.Controls.Add(this.lblReason);
            this.Controls.Add(this.txtReason);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnCancel);

            this.ResumeLayout(false);
        }

        private void LoadItem()
        {
            this.lblIngredientName.Text = _inventoryItem.IngredientName;

            var currency = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            currency.CurrencySymbol = AppSettings.CurrencySymbol ?? "$";

            this.lblCurrentStock.Text =
                $"Current: {_inventoryItem.CurrentStock:0.##} {_inventoryItem.Unit} | " +
                $"Unit Cost: {_inventoryItem.UnitCost.ToString("C2", currency)}";
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtAmount.Text.Trim(), out decimal amount) || amount < 0)
            {
                MessageBox.Show("Please enter a valid positive amount.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string adjType = cmbAdjustmentType.SelectedItem?.ToString() ?? "adjustment";
            string reason = txtReason.Text.Trim();

            decimal newStock = _inventoryItem.CurrentStock;

            switch (adjType)
            {
                case "addition":
                    newStock += amount;
                    break;

                case "removal":
                case "waste":
                    newStock -= amount;
                    if (newStock < 0) newStock = 0;
                    break;

                case "correction":
                    newStock = amount;
                    break;
            }

            DatabaseContext.UpdateInventoryLevel(
                ingredientId: _inventoryItem.IngredientId,
                newStock: newStock,
                minStock: null,
                maxStock: null,
                changeType: adjType,
                reason: reason,
                recipeId: null
            );

            MessageBox.Show("Inventory updated successfully.", "Success",
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
