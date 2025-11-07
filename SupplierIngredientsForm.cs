using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace CostChef
{
    public partial class SupplierIngredientsForm : Form
    {
        private DataGridView dataGridViewIngredients;
        private Label lblTitle;
        private Button btnClose;
        private Button btnExport;
        private Label lblSummary;

        private Supplier currentSupplier;
        private List<Ingredient> supplierIngredients;
        private string currencySymbol => AppSettings.CurrencySymbol;

        public SupplierIngredientsForm(Supplier supplier)
        {
            currentSupplier = supplier;
            InitializeComponent();
            LoadSupplierIngredients();
        }

        private void InitializeComponent()
        {
            this.dataGridViewIngredients = new DataGridView();
            this.lblTitle = new Label();
            this.btnClose = new Button();
            this.btnExport = new Button();
            this.lblSummary = new Label();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(700, 450);
            this.Text = $"Ingredients from {currentSupplier.Name}";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            // Title
            this.lblTitle.Text = $"Ingredients supplied by: {currentSupplier.Name}";
            this.lblTitle.Location = new System.Drawing.Point(20, 15);
            this.lblTitle.Size = new System.Drawing.Size(400, 20);
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // Summary
            this.lblSummary.Location = new System.Drawing.Point(20, 45);
            this.lblSummary.Size = new System.Drawing.Size(400, 20);
            this.lblSummary.Text = "Loading...";

            // DataGrid
            this.dataGridViewIngredients.Location = new System.Drawing.Point(20, 75);
            this.dataGridViewIngredients.Size = new System.Drawing.Size(660, 280);
            this.dataGridViewIngredients.ReadOnly = true;
            this.dataGridViewIngredients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewIngredients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewIngredients.RowHeadersVisible = false;

            // Buttons
            this.btnExport.Location = new System.Drawing.Point(20, 370);
            this.btnExport.Size = new System.Drawing.Size(100, 30);
            this.btnExport.Text = "Export to CSV";
            this.btnExport.Click += (s, e) => ExportToCsv();

            this.btnClose.Location = new System.Drawing.Point(580, 370);
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblTitle, lblSummary, dataGridViewIngredients, btnExport, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSupplierIngredients()
        {
            try
            {
                supplierIngredients = DatabaseContext.GetIngredientsBySupplier(currentSupplier.Id);
                
                dataGridViewIngredients.DataSource = supplierIngredients;
                
                if (dataGridViewIngredients.Columns.Count > 0)
                {
                    dataGridViewIngredients.Columns["Id"].Visible = false;
                    dataGridViewIngredients.Columns["SupplierId"].Visible = false;
                    dataGridViewIngredients.Columns["SupplierName"].Visible = false;
                    dataGridViewIngredients.Columns["Category"].Visible = false;
                    
                    dataGridViewIngredients.Columns["UnitPrice"].DefaultCellStyle.Format = $"{currencySymbol} 0.00";
                    dataGridViewIngredients.Columns["UnitPrice"].HeaderText = $"Price/Unit ({currencySymbol})";
                    dataGridViewIngredients.Columns["Name"].HeaderText = "Ingredient Name";
                    dataGridViewIngredients.Columns["Unit"].HeaderText = "Unit";
                }

                // Update summary
                int ingredientCount = supplierIngredients.Count;
                decimal totalValue = supplierIngredients.Sum(i => i.UnitPrice);
                decimal averagePrice = ingredientCount > 0 ? totalValue / ingredientCount : 0;
                
                lblSummary.Text = $"{ingredientCount} ingredients | Total Value: {currencySymbol}{totalValue:F2} | Average Price: {currencySymbol}{averagePrice:F2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsv()
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV Files (*.csv)|*.csv";
                    saveDialog.Title = "Export Supplier Ingredients";
                    saveDialog.FileName = $"{currentSupplier.Name.Replace(" ", "_")}_Ingredients_{DateTime.Now:yyyyMMdd}.csv";
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var importExportService = new ImportExportService();
                        bool success = importExportService.ExportIngredientsToCsv(supplierIngredients, saveDialog.FileName);
                        
                        if (success)
                        {
                            MessageBox.Show($"Ingredients exported successfully!\n\n{saveDialog.FileName}", "Export Complete", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting ingredients: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}