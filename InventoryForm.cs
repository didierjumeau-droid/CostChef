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
        private Button btnEditItem;
        private Label lblSummary;

        private List<InventoryLevel> _inventory = new List<InventoryLevel>();

        public InventoryForm()
        {
            InitializeComponent();
            LoadInventoryData();
        }

        private void InitializeComponent()
        {
            this.dataGridViewInventory = new DataGridView();
            this.btnClose = new Button();
            this.btnRefresh = new Button();
            this.btnAdjustStock = new Button();
            this.btnViewHistory = new Button();
            this.btnGenerateReport = new Button();
            this.btnEditItem = new Button();
            this.lblSummary = new Label();

            this.SuspendLayout();

            // DataGridView
            this.dataGridViewInventory.Location = new Point(20, 20);
            this.dataGridViewInventory.Size = new Size(860, 420);
            this.dataGridViewInventory.ReadOnly = true;
            this.dataGridViewInventory.AllowUserToAddRows = false;
            this.dataGridViewInventory.AllowUserToDeleteRows = false;
            this.dataGridViewInventory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewInventory.MultiSelect = false;
            this.dataGridViewInventory.AutoGenerateColumns = false;
            this.dataGridViewInventory.RowHeadersVisible = false;
            this.dataGridViewInventory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            ConfigureGridColumns();

            // Summary label
            this.lblSummary.AutoSize = true;
            this.lblSummary.Location = new Point(20, 450);
            this.lblSummary.Text = "Total Value: 0 | Items: 0 | Low Stock: 0";

            // Refresh
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Location = new Point(20, 480);
            this.btnRefresh.Click += btnRefresh_Click;

            // Quick Adjust
            this.btnAdjustStock.Text = "Quick Adjust";
            this.btnAdjustStock.Location = new Point(120, 480);
            this.btnAdjustStock.Click += btnAdjustStock_Click;

            // Edit Item
            this.btnEditItem.Text = "Edit Item";
            this.btnEditItem.Location = new Point(240, 480);
            this.btnEditItem.Click += btnEditItem_Click;

            // View History
            this.btnViewHistory.Text = "View History";
            this.btnViewHistory.Location = new Point(360, 480);
            this.btnViewHistory.Click += btnViewHistory_Click;

            // Reports / Snapshots
            this.btnGenerateReport.Text = "Reports && Snapshots";
            this.btnGenerateReport.Location = new Point(480, 480);
            this.btnGenerateReport.Click += btnGenerateReport_Click;

            // Close
            this.btnClose.Text = "Close";
            this.btnClose.Location = new Point(760, 480);
            this.btnClose.Click += btnClose_Click;

            // Form
            this.ClientSize = new Size(900, 550);
            this.Controls.Add(this.dataGridViewInventory);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnAdjustStock);
            this.Controls.Add(this.btnEditItem);
            this.Controls.Add(this.btnViewHistory);
            this.Controls.Add(this.btnGenerateReport);
            this.Controls.Add(this.btnClose);
            this.Text = "Inventory - CostChef";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ConfigureGridColumns()
        {
            string currencySymbol = AppSettings.CurrencySymbol ?? "$";
            var currency = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            currency.CurrencySymbol = currencySymbol;

            dataGridViewInventory.Columns.Clear();

            dataGridViewInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IngredientName",
                HeaderText = "Ingredient",
                DataPropertyName = "IngredientName",
                ReadOnly = true
            });

            dataGridViewInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CurrentStock",
                HeaderText = "Stock",
                DataPropertyName = "CurrentStock",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "0.##",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dataGridViewInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Unit",
                HeaderText = "Unit",
                DataPropertyName = "Unit",
                ReadOnly = true
            });

            dataGridViewInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MinimumStock",
                HeaderText = "Min", // ✅ FIX: no HeaderHeaderText
                DataPropertyName = "MinimumStock",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "0.##",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dataGridViewInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaximumStock",
                HeaderText = "Max",
                DataPropertyName = "MaximumStock",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "0.##",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dataGridViewInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UnitCost",
                HeaderText = $"Unit Cost ({currencySymbol})",
                DataPropertyName = "UnitCost",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C4",
                    FormatProvider = currency,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dataGridViewInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalValue",
                HeaderText = "Total Value",
                DataPropertyName = "TotalValue",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    FormatProvider = currency,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dataGridViewInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                DataPropertyName = "Status",
                ReadOnly = true
            });
        }

        private void LoadInventoryData()
        {
            _inventory = DatabaseContext.GetInventoryLevels();  // expects List<InventoryLevel>

            dataGridViewInventory.DataSource = null;
            dataGridViewInventory.DataSource = _inventory;

            ApplyRowColors();

            decimal totalValue = _inventory.Sum(i => i.TotalValue);
            int totalItems = _inventory.Count;
            int lowStock = _inventory.Count(i => i.IsLowStock);

            var nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            nfi.CurrencySymbol = AppSettings.CurrencySymbol ?? "$";

            lblSummary.Text =
                $"Total Value: {totalValue.ToString("C2", nfi)} | Items: {totalItems} | Low Stock: {lowStock}";
        }

        private void ApplyRowColors()
        {
            foreach (DataGridViewRow row in dataGridViewInventory.Rows)
            {
                if (row.DataBoundItem is InventoryLevel level)
                {
                    if (level.IsLowStock)
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                    else if (level.IsOverstocked)
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                    else
                        row.DefaultCellStyle.BackColor = Color.White;
                }
            }
        }

        private InventoryLevel GetSelectedItem()
        {
            if (dataGridViewInventory.SelectedRows.Count == 0)
                return null;

            return dataGridViewInventory.SelectedRows[0].DataBoundItem as InventoryLevel;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadInventoryData();
        }

        private void btnAdjustStock_Click(object sender, EventArgs e)
        {
            var level = GetSelectedItem();
            if (level == null)
            {
                MessageBox.Show("Please select an item to adjust.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var f = new InventoryAdjustForm(level))
            {
                if (f.ShowDialog() == DialogResult.OK)
                    LoadInventoryData();
            }
        }

        private void btnEditItem_Click(object sender, EventArgs e)
        {
            var level = GetSelectedItem();
            if (level == null)
            {
                MessageBox.Show("Please select an item to edit.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var f = new InventoryEditForm(level.IngredientId))
            {
                if (f.ShowDialog() == DialogResult.OK)
                    LoadInventoryData();
            }
        }

        private void btnViewHistory_Click(object sender, EventArgs e)
        {
            var level = GetSelectedItem();
            if (level == null)
            {
                MessageBox.Show("Please select an item to view history.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var f = new InventoryHistoryForm(level.IngredientId))
            {
                f.ShowDialog();
            }
        }

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            using (var f = new InventoryReportsForm())
            {
                f.ShowDialog();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
