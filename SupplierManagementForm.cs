using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace CostChef
{
    public partial class SupplierManagementForm : Form
    {
        private DataGridView dataGridViewSuppliers;
        private Button btnAddSupplier;
        private Button btnEditSupplier;
        private Button btnDeleteSupplier;
        private Button btnViewIngredients;
        private Button btnClose;
        private TextBox txtSearchSuppliers;
        private Label lblSupplierStats;

        public SupplierManagementForm()
        {
            InitializeComponent();
            LoadSuppliers();
            LoadSupplierStatistics();
        }

        private void InitializeComponent()
        {
            this.dataGridViewSuppliers = new DataGridView();
            this.btnAddSupplier = new Button();
            this.btnEditSupplier = new Button();
            this.btnDeleteSupplier = new Button();
            this.btnViewIngredients = new Button();
            this.btnClose = new Button();
            this.txtSearchSuppliers = new TextBox();
            this.lblSupplierStats = new Label();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Text = "Supplier Management";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            // Search box
            this.txtSearchSuppliers.Location = new System.Drawing.Point(12, 12);
            this.txtSearchSuppliers.Size = new System.Drawing.Size(200, 20);
            this.txtSearchSuppliers.PlaceholderText = "Search suppliers...";
            this.txtSearchSuppliers.TextChanged += (s, e) => LoadSuppliers();

            // Stats label
            this.lblSupplierStats.Location = new System.Drawing.Point(220, 12);
            this.lblSupplierStats.Size = new System.Drawing.Size(400, 20);
            this.lblSupplierStats.Text = "Total: 0 suppliers";

            // DataGrid
            this.dataGridViewSuppliers.Location = new System.Drawing.Point(12, 40);
            this.dataGridViewSuppliers.Size = new System.Drawing.Size(776, 350);
            this.dataGridViewSuppliers.ReadOnly = true;
            this.dataGridViewSuppliers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewSuppliers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewSuppliers.RowHeadersVisible = false;
            this.dataGridViewSuppliers.CellDoubleClick += (s, e) => EditSelectedSupplier();

            // Buttons
            this.btnAddSupplier.Location = new System.Drawing.Point(12, 400);
            this.btnAddSupplier.Size = new System.Drawing.Size(100, 30);
            this.btnAddSupplier.Text = "Add Supplier";
            this.btnAddSupplier.Click += (s, e) => AddSupplier();

            this.btnEditSupplier.Location = new System.Drawing.Point(122, 400);
            this.btnEditSupplier.Size = new System.Drawing.Size(100, 30);
            this.btnEditSupplier.Text = "Edit Supplier";
            this.btnEditSupplier.Click += (s, e) => EditSelectedSupplier();

            this.btnDeleteSupplier.Location = new System.Drawing.Point(232, 400);
            this.btnDeleteSupplier.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteSupplier.Text = "Delete Supplier";
            this.btnDeleteSupplier.Click += (s, e) => DeleteSupplier();

            this.btnViewIngredients.Location = new System.Drawing.Point(342, 400);
            this.btnViewIngredients.Size = new System.Drawing.Size(120, 30);
            this.btnViewIngredients.Text = "View Ingredients";
            this.btnViewIngredients.Click += (s, e) => ViewSupplierIngredients();

            this.btnClose.Location = new System.Drawing.Point(708, 400);
            this.btnClose.Size = new System.Drawing.Size(80, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                txtSearchSuppliers, lblSupplierStats, dataGridViewSuppliers,
                btnAddSupplier, btnEditSupplier, btnDeleteSupplier, btnViewIngredients, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSuppliers()
        {
            try
            {
                var searchTerm = txtSearchSuppliers.Text.ToLower();
                var allSuppliers = DatabaseContext.GetAllSuppliers();
                
                var filteredSuppliers = allSuppliers
                    .Where(s => string.IsNullOrEmpty(searchTerm) || 
                               s.Name.ToLower().Contains(searchTerm) ||
                               s.ContactPerson.ToLower().Contains(searchTerm) ||
                               s.Phone.ToLower().Contains(searchTerm))
                    .OrderBy(s => s.Name)
                    .ToList();

                dataGridViewSuppliers.DataSource = filteredSuppliers;
                
                if (dataGridViewSuppliers.Columns.Count > 0)
                {
                    dataGridViewSuppliers.Columns["Id"].Visible = false;
                    dataGridViewSuppliers.Columns["Name"].HeaderText = "Supplier Name";
                    dataGridViewSuppliers.Columns["ContactPerson"].HeaderText = "Contact Person";
                    dataGridViewSuppliers.Columns["Phone"].HeaderText = "Phone";
                    dataGridViewSuppliers.Columns["Email"].HeaderText = "Email";
                    dataGridViewSuppliers.Columns["Address"].HeaderText = "Address";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSupplierStatistics()
        {
            try
            {
                var stats = DatabaseContext.GetSupplierStatistics();
                int totalSuppliers = stats.Count;
                int totalIngredients = stats.Sum(s => s.IngredientCount);
                decimal totalValue = stats.Sum(s => s.TotalInventoryValue);
                
                lblSupplierStats.Text = $"Total: {totalSuppliers} suppliers, {totalIngredients} ingredients, Total Value: {AppSettings.CurrencySymbol}{totalValue:F2}";
            }
            catch (Exception)
            {
                lblSupplierStats.Text = "Error loading statistics";
            }
        }

        private void AddSupplier()
        {
            using (var form = new SupplierEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadSuppliers();
                    LoadSupplierStatistics();
                }
            }
        }

        private void EditSelectedSupplier()
        {
            if (dataGridViewSuppliers.SelectedRows.Count > 0)
            {
                var supplier = (Supplier)dataGridViewSuppliers.SelectedRows[0].DataBoundItem;
                using (var form = new SupplierEditForm(supplier))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadSuppliers();
                        LoadSupplierStatistics();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a supplier to edit.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteSupplier()
        {
            if (dataGridViewSuppliers.SelectedRows.Count > 0)
            {
                var supplier = (Supplier)dataGridViewSuppliers.SelectedRows[0].DataBoundItem;
                
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{supplier.Name}'?\n\nThis will remove the supplier association from all ingredients but won't delete the ingredients themselves.",
                    "Confirm Delete", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        DatabaseContext.DeleteSupplier(supplier.Id);
                        LoadSuppliers();
                        LoadSupplierStatistics();
                        MessageBox.Show("Supplier deleted successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting supplier: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a supplier to delete.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ViewSupplierIngredients()
        {
            if (dataGridViewSuppliers.SelectedRows.Count > 0)
            {
                var supplier = (Supplier)dataGridViewSuppliers.SelectedRows[0].DataBoundItem;
                using (var form = new SupplierIngredientsForm(supplier))
                {
                    form.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Please select a supplier to view ingredients.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}