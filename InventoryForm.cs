using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public partial class InventoryForm : Form
    {
        private DataGridView dataGridViewInventory;
        private Button btnClose;
        private Button btnRefresh;
        private Button btnAdjustStock;
        private Button btnViewHistory;
        private Button btnGenerateReport;
        private Button btnRecordPurchase;
        private Button btnEditItem;
        private Label lblTitle;
        private Label lblSummary;
        private Label lblLowStockAlert;

        private List<InventoryLevel> _inventory = new List<InventoryLevel>();
        private NumberFormatInfo _currencyFormat;

        public InventoryForm()
        {
            InitializeComponent();
            InitializeCurrencyFormat();
            LoadInventoryData();
        }

        private void InitializeCurrencyFormat()
        {
            _currencyFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            if (!string.IsNullOrEmpty(AppSettings.CurrencySymbol))
            {
                _currencyFormat.CurrencySymbol = AppSettings.CurrencySymbol;
            }
        }

        private void InitializeComponent()
        {
            this.dataGridViewInventory = new DataGridView();
            this.btnClose = new Button();
            this.btnRefresh = new Button();
            this.btnAdjustStock = new Button();
            this.btnViewHistory = new Button();
            this.btnGenerateReport = new Button();
            this.btnRecordPurchase = new Button();
            this.btnEditItem = new Button();
            this.lblTitle = new Label();
            this.lblSummary = new Label();
            this.lblLowStockAlert = new Label();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(900, 580);
            this.Text = "Inventory - CostChef";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Text = "📊 Inventory Management";

            // lblSummary
            this.lblSummary.AutoSize = true;
            this.lblSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblSummary.Location = new System.Drawing.Point(22, 55);
            this.lblSummary.Text = "Total Value: 0 | Items: 0 | Low Stock: 0";

            // lblLowStockAlert
            this.lblLowStockAlert.AutoSize = true;
            this.lblLowStockAlert.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.lblLowStockAlert.ForeColor = Color.Red;
            this.lblLowStockAlert.Location = new System.Drawing.Point(22, 80);
            this.lblLowStockAlert.Text = "";
            this.lblLowStockAlert.Visible = false;

            // btnRecordPurchase
            this.btnRecordPurchase.Location = new System.Drawing.Point(600, 50);
            this.btnRecordPurchase.Size = new System.Drawing.Size(200, 30);
            this.btnRecordPurchase.Text = "🧾 Record Purchases";
            this.btnRecordPurchase.Click += new EventHandler(this.btnRecordPurchase_Click);

            // dataGridViewInventory
            this.dataGridViewInventory.Location = new System.Drawing.Point(25, 110);
            this.dataGridViewInventory.Size = new System.Drawing.Size(850, 360);
            this.dataGridViewInventory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewInventory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewInventory.ReadOnly = true;
            this.dataGridViewInventory.AllowUserToAddRows = false;
            this.dataGridViewInventory.AllowUserToDeleteRows = false;
            this.dataGridViewInventory.RowHeadersVisible = false;

            // Columns
            this.dataGridViewInventory.Columns.Add("IngredientName", "Ingredient");
            this.dataGridViewInventory.Columns.Add("CurrentStock", "Current Stock");
            this.dataGridViewInventory.Columns.Add("Unit", "Unit");
            this.dataGridViewInventory.Columns.Add("MinimumStock", "Min Stock");
            this.dataGridViewInventory.Columns.Add("MaximumStock", "Max Stock");
            this.dataGridViewInventory.Columns.Add("UnitCost", "Unit Cost");
            this.dataGridViewInventory.Columns.Add("TotalValue", "Total Value");
            this.dataGridViewInventory.Columns.Add("Status", "Status");

            this.dataGridViewInventory.Columns["CurrentStock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.dataGridViewInventory.Columns["MinimumStock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.dataGridViewInventory.Columns["MaximumStock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.dataGridViewInventory.Columns["UnitCost"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            this.dataGridViewInventory.Columns["TotalValue"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // btnEditItem
            this.btnEditItem.Location = new System.Drawing.Point(25, 490);
            this.btnEditItem.Size = new System.Drawing.Size(120, 35);
            this.btnEditItem.Text = "Edit Item";
            this.btnEditItem.Click += new EventHandler(this.btnEditItem_Click);

            // btnAdjustStock
            this.btnAdjustStock.Location = new System.Drawing.Point(155, 490);
            this.btnAdjustStock.Size = new System.Drawing.Size(120, 35);
            this.btnAdjustStock.Text = "Quick Adjust";
            this.btnAdjustStock.Click += new EventHandler(this.btnAdjustStock_Click);

            // btnViewHistory
            this.btnViewHistory.Location = new System.Drawing.Point(285, 490);
            this.btnViewHistory.Size = new System.Drawing.Size(120, 35);
            this.btnViewHistory.Text = "View History";
            this.btnViewHistory.Click += new EventHandler(this.btnViewHistory_Click);

            // btnGenerateReport
            this.btnGenerateReport.Location = new System.Drawing.Point(415, 490);
            this.btnGenerateReport.Size = new System.Drawing.Size(120, 35);
            this.btnGenerateReport.Text = "Reports";
            this.btnGenerateReport.Click += new EventHandler(this.btnGenerateReport_Click);

            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(630, 490);
            this.btnRefresh.Size = new System.Drawing.Size(120, 35);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new EventHandler(this.btnRefresh_Click);

            // btnClose
            this.btnClose.Location = new System.Drawing.Point(760, 490);
            this.btnClose.Size = new System.Drawing.Size(120, 35);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new EventHandler(this.btnClose_Click);

            // Add controls
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.lblLowStockAlert);
            this.Controls.Add(this.dataGridViewInventory);
            this.Controls.Add(this.btnEditItem);
            this.Controls.Add(this.btnAdjustStock);
            this.Controls.Add(this.btnViewHistory);
            this.Controls.Add(this.btnGenerateReport);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRecordPurchase);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadInventoryData()
        {
            try
            {
                _inventory = DatabaseContext.GetInventoryLevels();
                this.dataGridViewInventory.Rows.Clear();

                decimal totalValue = 0;

                foreach (var item in _inventory)
                {
                    decimal itemTotal = item.TotalValue;

                    int rowIndex = this.dataGridViewInventory.Rows.Add(
                        item.IngredientName,
                        item.CurrentStock,
                        item.Unit,
                        item.MinimumStock,
                        item.MaximumStock,
                        item.UnitCost.ToString("C2", _currencyFormat),
                        itemTotal.ToString("C2", _currencyFormat),
                        item.Status
                    );

                    totalValue += itemTotal;

                    if (item.IsLowStock)
                        this.dataGridViewInventory.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                    else if (item.IsOverstocked)
                        this.dataGridViewInventory.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                }

                int lowCount = _inventory.Count(i => i.IsLowStock);
                this.lblSummary.Text =
                    $"Total Value: {totalValue.ToString("C2", _currencyFormat)} | Items: {_inventory.Count} | Low Stock: {lowCount}";

                this.lblLowStockAlert.Visible = lowCount > 0;
                if (lowCount > 0)
                    this.lblLowStockAlert.Text = "⚠ Some items are below minimum stock!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inventory data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private InventoryLevel GetSelectedItem()
        {
            if (this.dataGridViewInventory.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select an inventory item.",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return null;
            }

            var row = this.dataGridViewInventory.SelectedRows[0];
            var ingredientNameObj = row.Cells["IngredientName"].Value;
            if (ingredientNameObj == null)
            {
                MessageBox.Show(
                    "Unable to determine the selected ingredient.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return null;
            }

            string ingredientName = ingredientNameObj.ToString();

            var level = _inventory.FirstOrDefault(i => i.IngredientName == ingredientName);
            if (level == null)
            {
                MessageBox.Show(
                    "Could not find inventory information for the selected ingredient.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return level;
        }

        private void btnEditItem_Click(object sender, EventArgs e)
        {
            var level = GetSelectedItem();
            if (level == null) return;

            using (var form = new InventoryEditForm(level.IngredientId))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadInventoryData();
                }
            }
        }

        private void btnAdjustStock_Click(object sender, EventArgs e)
        {
            var level = GetSelectedItem();
            if (level == null) return;

            using (var form = new InventoryAdjustForm(level))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadInventoryData();
                }
            }
        }

        private void btnViewHistory_Click(object sender, EventArgs e)
        {
            var level = GetSelectedItem();
            if (level == null) return;

            using (var historyForm = new InventoryHistoryForm(level.IngredientId))
            {
                historyForm.ShowDialog(this);
            }
        }

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            using (var reportsForm = new InventoryReportsForm())
            {
                reportsForm.ShowDialog(this);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadInventoryData();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnRecordPurchase_Click(object sender, EventArgs e)
        {
            using (var form = new PurchaseEntryForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadInventoryData();
                }
            }
        }
    }
}
