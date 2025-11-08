using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace CostChef
{
    public partial class SupplierReportsForm : Form
    {
        private DataGridView dataGridViewReports;
        private Button btnClose;
        private Button btnExportToCsv;
        private Button btnExportToJson;

        public SupplierReportsForm()
        {
            InitializeComponent();
            LoadSupplierReports();
        }

        private void InitializeComponent()
        {
            this.dataGridViewReports = new DataGridView();
            this.btnClose = new Button();
            this.btnExportToCsv = new Button();
            this.btnExportToJson = new Button();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(700, 500);
            this.Text = "Supplier Reports";
            this.StartPosition = FormStartPosition.CenterParent;

            // DataGrid
            this.dataGridViewReports.Location = new System.Drawing.Point(12, 12);
            this.dataGridViewReports.Size = new System.Drawing.Size(676, 350);
            this.dataGridViewReports.ReadOnly = true;
            this.dataGridViewReports.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Export Buttons
            this.btnExportToCsv.Location = new System.Drawing.Point(12, 380);
            this.btnExportToCsv.Size = new System.Drawing.Size(120, 30);
            this.btnExportToCsv.Text = "Export to CSV";
            this.btnExportToCsv.Click += (s, e) => ExportSupplierReportsToCsv();

            this.btnExportToJson.Location = new System.Drawing.Point(142, 380);
            this.btnExportToJson.Size = new System.Drawing.Size(120, 30);
            this.btnExportToJson.Text = "Export to JSON";
            this.btnExportToJson.Click += (s, e) => ExportSupplierReportsToJson();

            // Close Button
            this.btnClose.Location = new System.Drawing.Point(588, 380);
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                dataGridViewReports, btnExportToCsv, btnExportToJson, btnClose
            });

            this.ResumeLayout(false);
        }

        private void LoadSupplierReports()
        {
            try
            {
                var supplierStats = DatabaseContext.GetSupplierStatistics();
                dataGridViewReports.DataSource = supplierStats;
                
                if (dataGridViewReports.Columns.Count > 0)
                {
                    dataGridViewReports.Columns["TotalInventoryValue"].DefaultCellStyle.Format = $"{AppSettings.CurrencySymbol}0.00";
                    dataGridViewReports.Columns["AveragePrice"].DefaultCellStyle.Format = $"{AppSettings.CurrencySymbol}0.0000";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading supplier reports: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportSupplierReportsToCsv()
        {
            try
            {
                var supplierStats = DatabaseContext.GetSupplierStatistics();
                if (supplierStats == null || supplierStats.Count == 0)
                {
                    MessageBox.Show("No supplier data to export.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = "Export Supplier Reports to CSV";
                    saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    saveDialog.FileName = $"supplier_reports_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    saveDialog.InitialDirectory = AppSettings.ExportLocation;
                    saveDialog.OverwritePrompt = true;
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var lines = new List<string>();
                        // Header
                        lines.Add("SupplierName,IngredientCount,TotalInventoryValue,AveragePrice");
                        
                        foreach (var stat in supplierStats)
                        {
                            lines.Add($"\"{stat.SupplierName}\",{stat.IngredientCount},{stat.TotalInventoryValue:F2},{stat.AveragePrice:F4}");
                        }
                        
                        File.WriteAllLines(saveDialog.FileName, lines);
                        
                        MessageBox.Show($"Successfully exported {supplierStats.Count} supplier reports to CSV.", 
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting supplier reports: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportSupplierReportsToJson()
        {
            try
            {
                var supplierStats = DatabaseContext.GetSupplierStatistics();
                if (supplierStats == null || supplierStats.Count == 0)
                {
                    MessageBox.Show("No supplier data to export.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = "Export Supplier Reports to JSON";
                    saveDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveDialog.FileName = $"supplier_reports_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    saveDialog.InitialDirectory = AppSettings.ExportLocation;
                    saveDialog.OverwritePrompt = true;
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string json = JsonSerializer.Serialize(supplierStats, options);
                        File.WriteAllText(saveDialog.FileName, json);
                        
                        MessageBox.Show($"Successfully exported {supplierStats.Count} supplier reports to JSON.", 
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting supplier reports: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}