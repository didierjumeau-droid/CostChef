using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace CostChef
{
    public partial class SupplierReportsForm : Form
    {
        private DataGridView dataGridViewStats;
        private Button btnClose;
        private Button btnExport;
        private Label lblTitle;
        private Label lblSummary;

        private string currencySymbol => AppSettings.CurrencySymbol;

        public SupplierReportsForm()
        {
            InitializeComponent();
            LoadSupplierStatistics();
        }

        private void InitializeComponent()
        {
            this.dataGridViewStats = new DataGridView();
            this.btnClose = new Button();
            this.btnExport = new Button();
            this.lblTitle = new Label();
            this.lblSummary = new Label();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Text = "Supplier Reports";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            // Title
            this.lblTitle.Text = "Supplier Statistics Report";
            this.lblTitle.Location = new System.Drawing.Point(20, 15);
            this.lblTitle.Size = new System.Drawing.Size(400, 20);
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);

            // Summary
            this.lblSummary.Location = new System.Drawing.Point(20, 45);
            this.lblSummary.Size = new System.Drawing.Size(400, 20);
            this.lblSummary.Text = "Loading supplier statistics...";

            // DataGrid
            this.dataGridViewStats.Location = new System.Drawing.Point(20, 75);
            this.dataGridViewStats.Size = new System.Drawing.Size(760, 350);
            this.dataGridViewStats.ReadOnly = true;
            this.dataGridViewStats.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewStats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewStats.RowHeadersVisible = false;
            this.dataGridViewStats.CellDoubleClick += (s, e) => ViewSupplierDetails();

            // Buttons
            this.btnExport.Location = new System.Drawing.Point(20, 440);
            this.btnExport.Size = new System.Drawing.Size(100, 30);
            this.btnExport.Text = "Export to CSV";
            this.btnExport.Click += (s, e) => ExportToCsv();

            this.btnClose.Location = new System.Drawing.Point(680, 440);
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblTitle, lblSummary, dataGridViewStats, btnExport, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSupplierStatistics()
        {
            try
            {
                var stats = DatabaseContext.GetSupplierStatistics();
                
                dataGridViewStats.DataSource = stats;
                
                if (dataGridViewStats.Columns.Count > 0)
                {
                    dataGridViewStats.Columns["SupplierId"].Visible = false;
                    
                    dataGridViewStats.Columns["TotalInventoryValue"].DefaultCellStyle.Format = $"{currencySymbol} 0.00";
                    dataGridViewStats.Columns["AveragePrice"].DefaultCellStyle.Format = $"{currencySymbol} 0.00";
                    
                    dataGridViewStats.Columns["SupplierName"].HeaderText = "Supplier Name";
                    dataGridViewStats.Columns["IngredientCount"].HeaderText = "Ingredients";
                    dataGridViewStats.Columns["TotalInventoryValue"].HeaderText = $"Total Value ({currencySymbol})";
                    dataGridViewStats.Columns["AveragePrice"].HeaderText = $"Avg Price ({currencySymbol})";
                }

                // Update summary
                int totalSuppliers = stats.Count;
                int totalIngredients = stats.Sum(s => s.IngredientCount);
                decimal totalValue = stats.Sum(s => s.TotalInventoryValue);
                
                lblSummary.Text = $"{totalSuppliers} suppliers | {totalIngredients} total ingredients | Total Inventory Value: {currencySymbol}{totalValue:F2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading supplier statistics: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblSummary.Text = "Error loading statistics";
            }
        }

        private void ViewSupplierDetails()
        {
            if (dataGridViewStats.SelectedRows.Count > 0)
            {
                var stat = (SupplierStats)dataGridViewStats.SelectedRows[0].DataBoundItem;
                var supplier = DatabaseContext.GetSupplierById(stat.SupplierId);
                
                if (supplier != null)
                {
                    using (var form = new SupplierIngredientsForm(supplier))
                    {
                        form.ShowDialog();
                    }
                }
            }
        }

        private void ExportToCsv()
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV Files (*.csv)|*.csv";
                    saveDialog.Title = "Export Supplier Statistics";
                    saveDialog.FileName = $"Supplier_Statistics_{DateTime.Now:yyyyMMdd}.csv";
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var stats = DatabaseContext.GetSupplierStatistics();
                        var importExportService = new ImportExportService();
                        
                        // Convert to list of ingredients for export (we'll create a custom format)
                        var exportData = new List<object>();
                        foreach (var stat in stats)
                        {
                            exportData.Add(new {
                                SupplierName = stat.SupplierName,
                                IngredientCount = stat.IngredientCount,
                                TotalInventoryValue = stat.TotalInventoryValue,
                                AveragePrice = stat.AveragePrice
                            });
                        }
                        
                        // Use JSON export for now, or create a custom CSV method
                        bool success = importExportService.ExportIngredientsToJson(exportData, saveDialog.FileName.Replace(".csv", ".json"));
                        
                        if (success)
                        {
                            MessageBox.Show($"Supplier statistics exported successfully!\n\n{saveDialog.FileName.Replace(".csv", ".json")}", "Export Complete", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting statistics: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}