using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public partial class SupplierReportsForm : Form
    {
        private List<Supplier> suppliers;
        private List<Ingredient> ingredients;

        public SupplierReportsForm()
        {
            InitializeComponent();
            LoadSuppliers();
            LoadIngredients();
            SetupDataGridView();
        }

        private void InitializeComponent()
        {
            this.Text = "Supplier Reports";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Main layout panel
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            this.Controls.Add(mainPanel);

            // Title
            var titleLabel = new Label
            {
                Text = "Supplier Reports",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            mainPanel.Controls.Add(titleLabel);

            // Supplier selection
            var supplierLabel = new Label
            {
                Text = "Select Supplier:",
                Location = new Point(10, 50),
                AutoSize = true
            };
            mainPanel.Controls.Add(supplierLabel);

            var supplierComboBox = new ComboBox
            {
                Location = new Point(120, 47),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            supplierComboBox.SelectedIndexChanged += (s, e) => LoadSupplierReport();
            mainPanel.Controls.Add(supplierComboBox);

            // Report type selection
            var reportTypeLabel = new Label
            {
                Text = "Report Type:",
                Location = new Point(350, 50),
                AutoSize = true
            };
            mainPanel.Controls.Add(reportTypeLabel);

            var reportTypeComboBox = new ComboBox
            {
                Location = new Point(450, 47),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            reportTypeComboBox.Items.AddRange(new[] { "All Suppliers", "Selected Supplier" });
            reportTypeComboBox.SelectedIndex = 0;
            reportTypeComboBox.SelectedIndexChanged += (s, e) => UpdateReport();
            mainPanel.Controls.Add(reportTypeComboBox);

            // DataGridView for reports
            var dataGridView = new DataGridView
            {
                Location = new Point(10, 90),
                Size = new Size(760, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            mainPanel.Controls.Add(dataGridView);

            // Close button
            var closeButton = new Button
            {
                Text = "Close",
                Location = new Point(680, 500),
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            closeButton.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(closeButton);

            // Initialize controls
            this.suppliersComboBox = supplierComboBox;
            this.reportTypeComboBox = reportTypeComboBox;
            this.reportsDataGridView = dataGridView;
        }

        private ComboBox suppliersComboBox;
        private ComboBox reportTypeComboBox;
        private DataGridView reportsDataGridView;

        private void LoadSuppliers()
        {
            suppliers = DatabaseContext.GetAllSuppliers();
            suppliersComboBox.Items.Clear();
            suppliersComboBox.Items.AddRange(suppliers.Select(s => s.Name).ToArray());
            if (suppliersComboBox.Items.Count > 0)
                suppliersComboBox.SelectedIndex = 0;
        }

        private void LoadIngredients()
        {
            ingredients = DatabaseContext.GetAllIngredients();
        }

        private void SetupDataGridView()
        {
            reportsDataGridView.Columns.Clear();
        }

        private void UpdateReport()
        {
            if (reportTypeComboBox.SelectedItem?.ToString() == "All Suppliers")
            {
                LoadAllSuppliersReport();
            }
            else
            {
                LoadSupplierReport();
            }
        }

        private void LoadAllSuppliersReport()
        {
            reportsDataGridView.Columns.Clear();
            reportsDataGridView.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Supplier", DataPropertyName = "SupplierName", FillWeight = 25 },
                new DataGridViewTextBoxColumn { HeaderText = "Ingredients", DataPropertyName = "IngredientCount", FillWeight = 15 },
                new DataGridViewTextBoxColumn { HeaderText = "Total Value", DataPropertyName = "TotalValue", FillWeight = 20 },
                new DataGridViewTextBoxColumn { HeaderText = "Contact", DataPropertyName = "Contact", FillWeight = 20 },
                new DataGridViewTextBoxColumn { HeaderText = "Phone", DataPropertyName = "Phone", FillWeight = 20 }
            );

            var reportData = new List<dynamic>();
            foreach (var supplier in suppliers)
            {
                var supplierIngredients = ingredients.Where(i => i.SupplierId == supplier.Id).ToList();
                var stats = DatabaseContext.GetSupplierStatistics(supplier.Id);
                
                reportData.Add(new
                {
                    SupplierName = supplier.Name,
                    IngredientCount = supplierIngredients.Count,
                    TotalValue = supplierIngredients.Sum(i => i.UnitPrice).ToString("C2"),
                    Contact = supplier.ContactPerson ?? "N/A",
                    Phone = supplier.Phone ?? "N/A"
                });
            }

            reportsDataGridView.DataSource = reportData;
        }

        private void LoadSupplierReport()
        {
            if (suppliersComboBox.SelectedItem == null) return;

            var selectedSupplierName = suppliersComboBox.SelectedItem.ToString();
            var supplier = suppliers.FirstOrDefault(s => s.Name == selectedSupplierName);
            if (supplier == null) return;

            reportsDataGridView.Columns.Clear();
            reportsDataGridView.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Ingredient", DataPropertyName = "Name", FillWeight = 30 },
                new DataGridViewTextBoxColumn { HeaderText = "Unit", DataPropertyName = "Unit", FillWeight = 15 },
                new DataGridViewTextBoxColumn { HeaderText = "Unit Price", DataPropertyName = "UnitPrice", FillWeight = 20 },
                new DataGridViewTextBoxColumn { HeaderText = "Category", DataPropertyName = "Category", FillWeight = 20 },
                new DataGridViewTextBoxColumn { HeaderText = "Supplier", DataPropertyName = "SupplierName", FillWeight = 15 }
            );

            var supplierIngredients = ingredients.Where(i => i.SupplierId == supplier.Id).ToList();
            reportsDataGridView.DataSource = supplierIngredients;
        }
    }
}