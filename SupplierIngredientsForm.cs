using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CostChef
{
    public partial class SupplierIngredientsForm : Form
    {
        private DataGridView dataGridViewIngredients;
        private Button btnClose;
        private Button btnExportToCsv;
        private ComboBox cmbSuppliers;
        private Label lblSupplier;

        public SupplierIngredientsForm()
        {
            InitializeComponent();
            LoadSuppliers();
        }

        // ENHANCED CONSTRUCTOR: Now properly auto-loads the supplier's ingredients
        public SupplierIngredientsForm(Supplier supplier)
        {
            InitializeComponent();
            LoadSuppliers();
            
            // Auto-select and auto-load the provided supplier
            if (supplier != null)
            {
                // Find and select the supplier in the combobox
                foreach (Supplier item in cmbSuppliers.Items)
                {
                    if (item.Id == supplier.Id)
                    {
                        cmbSuppliers.SelectedItem = item;
                        break;
                    }
                }
                
                // NEW: Immediately load the supplier's ingredients
                LoadSupplierIngredients();
            }
        }

        private void InitializeComponent()
        {
            this.dataGridViewIngredients = new DataGridView();
            this.btnClose = new Button();
            this.btnExportToCsv = new Button();
            this.cmbSuppliers = new ComboBox();
            this.lblSupplier = new Label();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(700, 500);
            this.Text = "Supplier Ingredients";
            this.StartPosition = FormStartPosition.CenterParent;

            // Supplier Selection
            this.lblSupplier.Location = new System.Drawing.Point(12, 15);
            this.lblSupplier.Size = new System.Drawing.Size(100, 20);
            this.lblSupplier.Text = "Select Supplier:";
            this.lblSupplier.AutoSize = true;

            this.cmbSuppliers.Location = new System.Drawing.Point(120, 12);
            this.cmbSuppliers.Size = new System.Drawing.Size(200, 20);
            this.cmbSuppliers.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbSuppliers.SelectedIndexChanged += (s, e) => LoadSupplierIngredients();

            // DataGrid
            this.dataGridViewIngredients.Location = new System.Drawing.Point(12, 45);
            this.dataGridViewIngredients.Size = new System.Drawing.Size(676, 350);
            this.dataGridViewIngredients.ReadOnly = true;
            this.dataGridViewIngredients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Export Button
            this.btnExportToCsv.Location = new System.Drawing.Point(12, 410);
            this.btnExportToCsv.Size = new System.Drawing.Size(120, 30);
            this.btnExportToCsv.Text = "Export to CSV";
            this.btnExportToCsv.Click += (s, e) => ExportSupplierIngredientsToCsv();

            // Close Button
            this.btnClose.Location = new System.Drawing.Point(588, 410);
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblSupplier, cmbSuppliers, dataGridViewIngredients, btnExportToCsv, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = DatabaseContext.GetAllSuppliers();
                cmbSuppliers.DataSource = suppliers;
                cmbSuppliers.DisplayMember = "Name";
                cmbSuppliers.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSupplierIngredients()
        {
            try
            {
                if (cmbSuppliers.SelectedItem is Supplier selectedSupplier)
                {
                    var ingredients = DatabaseContext.GetIngredientsBySupplier(selectedSupplier.Id);
                    dataGridViewIngredients.DataSource = ingredients;
                    
                    if (dataGridViewIngredients.Columns.Count > 0)
                    {
                        dataGridViewIngredients.Columns["UnitPrice"].DefaultCellStyle.Format = $"{AppSettings.CurrencySymbol}0.0000";
                    }
                    
                    // Update window title to show supplier name and ingredient count
                    this.Text = $"Supplier Ingredients - {selectedSupplier.Name} ({ingredients.Count} items)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading supplier ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportSupplierIngredientsToCsv()
        {
            try
            {
                if (cmbSuppliers.SelectedItem is not Supplier selectedSupplier)
                {
                    MessageBox.Show("Please select a supplier first.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var ingredients = DatabaseContext.GetIngredientsBySupplier(selectedSupplier.Id);
                if (ingredients == null || ingredients.Count == 0)
                {
                    MessageBox.Show("No ingredients found for this supplier.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = $"Export {selectedSupplier.Name} Ingredients to CSV";
                    saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    saveDialog.FileName = $"{selectedSupplier.Name}_ingredients_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    saveDialog.InitialDirectory = AppSettings.ExportLocation;
                    saveDialog.OverwritePrompt = true;
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var lines = new List<string>();
                        // Header
                        lines.Add("Name,Unit,UnitPrice,Category,SupplierName");
                        
                        foreach (var ingredient in ingredients)
                        {
                            lines.Add($"\"{ingredient.Name}\",\"{ingredient.Unit}\",{ingredient.UnitPrice:F4},\"{ingredient.Category}\",\"{ingredient.SupplierName}\"");
                        }
                        
                        File.WriteAllLines(saveDialog.FileName, lines);
                        
                        MessageBox.Show($"Successfully exported {ingredients.Count} ingredients to CSV.", 
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting supplier ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}