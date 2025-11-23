using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public partial class InventoryReportsForm : Form
    {
        private DataGridView dataGridViewReport;
        private Button btnClose;
        private Button btnGenerate;
        private Button btnTakeSnapshot;
        private Label lblTitle;
        private ComboBox cmbReportType;
        private Label lblSummary;
        private Label lblSnapshotInfo;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;

        public InventoryReportsForm()
        {
            InitializeComponent();
            LoadHeaderInfo();
            LoadReportData();
        }

        private void InitializeComponent()
        {
            this.dataGridViewReport = new DataGridView();
            this.btnClose = new Button();
            this.btnGenerate = new Button();
            this.btnTakeSnapshot = new Button();
            this.lblTitle = new Label();
            this.cmbReportType = new ComboBox();
            this.lblSummary = new Label();
            this.lblSnapshotInfo = new Label();
            this.dtpStart = new DateTimePicker();
            this.dtpEnd = new DateTimePicker();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(920, 540);
            this.Text = "Inventory Reports & Snapshots - CostChef v3.0";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Text = "Inventory Reports & Snapshots";

            // Report type
            var lblReportType = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(22, 60),
                Text = "Report:"
            };

            this.cmbReportType.Location = new System.Drawing.Point(80, 57);
            this.cmbReportType.Size = new System.Drawing.Size(220, 21);
            this.cmbReportType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbReportType.Items.AddRange(new object[]
            {
                "Current Inventory",
                "Low Stock Items",
                "High Value Items",
                "Monthly Comparison"
            });
            this.cmbReportType.SelectedIndex = 0;
            this.cmbReportType.SelectedIndexChanged += cmbReportType_SelectedIndexChanged;

            // Summary
            this.lblSummary.AutoSize = true;
            this.lblSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblSummary.Location = new System.Drawing.Point(22, 90);
            this.lblSummary.Text = "Total Inventory Value: Calculating...";

            // Snapshot info
            this.lblSnapshotInfo.AutoSize = true;
            this.lblSnapshotInfo.Location = new System.Drawing.Point(22, 115);
            this.lblSnapshotInfo.Text = "Last snapshot: Not available";

            // Date range
            var lblFrom = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(340, 60),
                Text = "From:"
            };
            this.dtpStart.Location = new System.Drawing.Point(380, 57);
            this.dtpStart.Format = DateTimePickerFormat.Short;
            this.dtpStart.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var lblTo = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(560, 60),
                Text = "To:"
            };
            this.dtpEnd.Location = new System.Drawing.Point(590, 57);
            this.dtpEnd.Format = DateTimePickerFormat.Short;
            this.dtpEnd.Value = DateTime.Now;

            // DataGridView
            this.dataGridViewReport.Location = new System.Drawing.Point(20, 140);
            this.dataGridViewReport.Size = new System.Drawing.Size(880, 330);
            this.dataGridViewReport.ReadOnly = true;
            this.dataGridViewReport.AllowUserToAddRows = false;
            this.dataGridViewReport.AllowUserToDeleteRows = false;
            this.dataGridViewReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewReport.MultiSelect = false;
            this.dataGridViewReport.RowHeadersVisible = false;
            this.dataGridViewReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Buttons
            this.btnTakeSnapshot.Location = new System.Drawing.Point(20, 485);
            this.btnTakeSnapshot.Size = new System.Drawing.Size(140, 35);
            this.btnTakeSnapshot.Text = "Take Snapshot";
            this.btnTakeSnapshot.Click += btnTakeSnapshot_Click;

            this.btnGenerate.Location = new System.Drawing.Point(170, 485);
            this.btnGenerate.Size = new System.Drawing.Size(140, 35);
            this.btnGenerate.Text = "Generate PDF";
            this.btnGenerate.Click += btnGenerate_Click;

            this.btnClose.Location = new System.Drawing.Point(760, 485);
            this.btnClose.Size = new System.Drawing.Size(140, 35);
            this.btnClose.Text = "Close";
            this.btnClose.Click += btnClose_Click;

            // Add controls
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(lblReportType);
            this.Controls.Add(this.cmbReportType);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.lblSnapshotInfo);
            this.Controls.Add(lblFrom);
            this.Controls.Add(this.dtpStart);
            this.Controls.Add(lblTo);
            this.Controls.Add(this.dtpEnd);
            this.Controls.Add(this.dataGridViewReport);
            this.Controls.Add(this.btnTakeSnapshot);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnClose);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadHeaderInfo()
        {
            try
            {
                decimal totalValue = DatabaseContext.GetTotalInventoryValue();

                string currencySymbol = AppSettings.CurrencySymbol ?? "$";
                var nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                nfi.CurrencySymbol = currencySymbol;

                this.lblSummary.Text = $"Total Inventory Value: {totalValue.ToString("C2", nfi)}";

                var snapshot = DatabaseContext.GetLatestSnapshotSummary();
                if (snapshot == null || snapshot.SnapshotDate == DateTime.MinValue)
                {
                    this.lblSnapshotInfo.Text = "Last snapshot: Not available";
                }
                else
                {
                    this.lblSnapshotInfo.Text =
                        $"Last snapshot: {snapshot.SnapshotDate:g} | " +
                        $"Items: {snapshot.IngredientCount} | " +
                        $"Value: {snapshot.TotalValue.ToString("C2", nfi)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading header info: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadReportData()
        {
            try
            {
                string reportType = cmbReportType.SelectedItem?.ToString() ?? "Current Inventory";

                switch (reportType)
                {
                    case "Current Inventory":
                        LoadCurrentInventoryReport();
                        break;
                    case "Low Stock Items":
                        LoadLowStockReport();
                        break;
                    case "High Value Items":
                        LoadHighValueReport();
                        break;
                    case "Monthly Comparison":
                        LoadMonthlyComparisonReport();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureCurrencyColumns(params string[] columnNames)
        {
            string currencySymbol = AppSettings.CurrencySymbol ?? "$";
            var currencyFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            currencyFormat.CurrencySymbol = currencySymbol;

            foreach (var name in columnNames)
            {
                if (this.dataGridViewReport.Columns.Contains(name))
                {
                    var col = this.dataGridViewReport.Columns[name];
                    col.DefaultCellStyle.Format = "C2";
                    col.DefaultCellStyle.FormatProvider = currencyFormat;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private void ConfigureNumericColumns(params string[] columnNames)
        {
            foreach (var name in columnNames)
            {
                if (this.dataGridViewReport.Columns.Contains(name))
                {
                    var col = this.dataGridViewReport.Columns[name];
                    col.DefaultCellStyle.Format = "0.##";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        // ----------------------------------------------------
        // Individual reports
        // ----------------------------------------------------

        private void LoadCurrentInventoryReport()
        {
            var levels = DatabaseContext.GetInventoryLevels();

            this.dataGridViewReport.Columns.Clear();

            this.dataGridViewReport.Columns.Add("IngredientName", "Ingredient");
            this.dataGridViewReport.Columns.Add("CurrentStock", "Stock");
            this.dataGridViewReport.Columns.Add("Unit", "Unit");
            this.dataGridViewReport.Columns.Add("MinimumStock", "Min");
            this.dataGridViewReport.Columns.Add("MaximumStock", "Max");
            this.dataGridViewReport.Columns.Add("UnitCost", "Unit Cost");
            this.dataGridViewReport.Columns.Add("TotalValue", "Total Value");
            this.dataGridViewReport.Columns.Add("Status", "Status");

            foreach (var item in levels.OrderByDescending(l => l.TotalValue))
            {
                int row = this.dataGridViewReport.Rows.Add(
                    item.IngredientName,
                    item.CurrentStock,
                    item.Unit,
                    item.MinimumStock,
                    item.MaximumStock,
                    item.UnitCost,
                    item.TotalValue,
                    item.Status
                );

                if (item.IsLowStock)
                    this.dataGridViewReport.Rows[row].DefaultCellStyle.BackColor = Color.LightCoral;
                else if (item.IsOverstocked)
                    this.dataGridViewReport.Rows[row].DefaultCellStyle.BackColor = Color.LightYellow;
            }

            ConfigureNumericColumns("CurrentStock", "MinimumStock", "MaximumStock");
            ConfigureCurrencyColumns("UnitCost", "TotalValue");
        }

        private void LoadLowStockReport()
        {
            var levels = DatabaseContext.GetLowStockItems();

            this.dataGridViewReport.Columns.Clear();
            this.dataGridViewReport.Columns.Add("IngredientName", "Ingredient");
            this.dataGridViewReport.Columns.Add("CurrentStock", "Stock");
            this.dataGridViewReport.Columns.Add("MinimumStock", "Minimum");
            this.dataGridViewReport.Columns.Add("Unit", "Unit");
            this.dataGridViewReport.Columns.Add("UnitCost", "Unit Cost");
            this.dataGridViewReport.Columns.Add("TotalValue", "Total Value");

            foreach (var item in levels)
            {
                int row = this.dataGridViewReport.Rows.Add(
                    item.IngredientName,
                    item.CurrentStock,
                    item.MinimumStock,
                    item.Unit,
                    item.UnitCost,
                    item.TotalValue
                );
                this.dataGridViewReport.Rows[row].DefaultCellStyle.BackColor = Color.LightCoral;
            }

            ConfigureNumericColumns("CurrentStock", "MinimumStock");
            ConfigureCurrencyColumns("UnitCost", "TotalValue");
        }

        private void LoadHighValueReport()
        {
            var levels = DatabaseContext.GetInventoryLevels()
                .OrderByDescending(l => l.TotalValue)
                .Take(50)
                .ToList();

            this.dataGridViewReport.Columns.Clear();
            this.dataGridViewReport.Columns.Add("IngredientName", "Ingredient");
            this.dataGridViewReport.Columns.Add("CurrentStock", "Stock");
            this.dataGridViewReport.Columns.Add("Unit", "Unit");
            this.dataGridViewReport.Columns.Add("UnitCost", "Unit Cost");
            this.dataGridViewReport.Columns.Add("TotalValue", "Total Value");

            foreach (var item in levels)
            {
                int row = this.dataGridViewReport.Rows.Add(
                    item.IngredientName,
                    item.CurrentStock,
                    item.Unit,
                    item.UnitCost,
                    item.TotalValue
                );
            }

            ConfigureNumericColumns("CurrentStock");
            ConfigureCurrencyColumns("UnitCost", "TotalValue");
        }

        private void LoadMonthlyComparisonReport()
        {
            DateTime start = dtpStart.Value.Date;
            DateTime end = dtpEnd.Value.Date;

            // Define previous period as the same length immediately before
            TimeSpan span = end - start;
            DateTime prevStart = start.AddMonths(-1);
            DateTime prevEnd = prevStart + span;

            // Aggregate value changes from history
            var levels = DatabaseContext.GetInventoryLevels();

            var currentChanges = new Dictionary<int, decimal>();
            var previousChanges = new Dictionary<int, decimal>();

            foreach (var level in levels)
            {
                var curr = DatabaseContext.GetInventoryHistory(level.IngredientId, "All", start, end);
                var prev = DatabaseContext.GetInventoryHistory(level.IngredientId, "All", prevStart, prevEnd);

                decimal currValue = curr.Sum(h => h.ValueChange);
                decimal prevValue = prev.Sum(h => h.ValueChange);

                currentChanges[level.IngredientId] = currValue;
                previousChanges[level.IngredientId] = prevValue;
            }

            this.dataGridViewReport.Columns.Clear();
            this.dataGridViewReport.Columns.Add("IngredientName", "Ingredient");
            this.dataGridViewReport.Columns.Add("PreviousValue", "Previous Period");
            this.dataGridViewReport.Columns.Add("CurrentValue", "Current Period");
            this.dataGridViewReport.Columns.Add("Change", "Change");
            this.dataGridViewReport.Columns.Add("ChangePercent", "Change %");

            foreach (var level in levels)
            {
                decimal prevValue = previousChanges[level.IngredientId];
                decimal currValue = currentChanges[level.IngredientId];
                decimal change = currValue - prevValue;
                decimal pct = prevValue == 0 ? 0 : change / prevValue;

                int row = this.dataGridViewReport.Rows.Add(
                    level.IngredientName,
                    prevValue,
                    currValue,
                    change,
                    pct
                );

                if (change > 0)
                    this.dataGridViewReport.Rows[row].DefaultCellStyle.BackColor = Color.LightGreen;
                else if (change < 0)
                    this.dataGridViewReport.Rows[row].DefaultCellStyle.BackColor = Color.LightCoral;
            }

            ConfigureCurrencyColumns("PreviousValue", "CurrentValue", "Change");
            if (this.dataGridViewReport.Columns.Contains("ChangePercent"))
            {
                var col = this.dataGridViewReport.Columns["ChangePercent"];
                col.DefaultCellStyle.Format = "P1";
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        // ----------------------------------------------------
        // Events
        // ----------------------------------------------------

        private void cmbReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadReportData();
        }

        private void btnTakeSnapshot_Click(object sender, EventArgs e)
        {
            try
            {
                int id = DatabaseContext.TakeInventorySnapshot();
                LoadHeaderInfo();
                MessageBox.Show($"Snapshot #{id} saved successfully.", "Snapshot",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error taking snapshot: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Placeholder – future PDF export
            MessageBox.Show("PDF export will be implemented in a future version.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
