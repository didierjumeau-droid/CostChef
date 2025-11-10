using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

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
        private TextBox txtSearch;

        public SupplierManagementForm()
        {
            InitializeComponent();
            LoadSuppliers();
        }

        private void InitializeComponent()
        {
            this.dataGridViewSuppliers = new DataGridView();
            this.btnAddSupplier = new Button();
            this.btnEditSupplier = new Button();
            this.btnDeleteSupplier = new Button();
            this.btnViewIngredients = new Button();
            this.btnClose = new Button();
            this.txtSearch = new TextBox();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(700, 500);
            this.Text = "Manage Suppliers";
            this.StartPosition = FormStartPosition.CenterParent;

            // Search
            var lblSearch = new Label { Text = "Search:", Location = new System.Drawing.Point(12, 15), AutoSize = true };
            this.txtSearch.Location = new System.Drawing.Point(60, 12);
            this.txtSearch.Size = new System.Drawing.Size(150, 20);
            this.txtSearch.TextChanged += (s, e) => LoadSuppliers();

            // DataGrid
            this.dataGridViewSuppliers.Location = new System.Drawing.Point(12, 40);
            this.dataGridViewSuppliers.Size = new System.Drawing.Size(676, 350);
            this.dataGridViewSuppliers.ReadOnly = true;
            this.dataGridViewSuppliers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewSuppliers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Buttons
            this.btnAddSupplier.Location = new System.Drawing.Point(12, 410);
            this.btnAddSupplier.Size = new System.Drawing.Size(100, 30);
            this.btnAddSupplier.Text = "Add Supplier";
            this.btnAddSupplier.Click += (s, e) => AddSupplier();

            this.btnEditSupplier.Location = new System.Drawing.Point(122, 410);
            this.btnEditSupplier.Size = new System.Drawing.Size(100, 30);
            this.btnEditSupplier.Text = "Edit Supplier";
            this.btnEditSupplier.Click += (s, e) => EditSupplier();

            this.btnDeleteSupplier.Location = new System.Drawing.Point(232, 410);
            this.btnDeleteSupplier.Size = new System.Drawing.Size(100, 30);
            this.btnDeleteSupplier.Text = "Delete Supplier";
            this.btnDeleteSupplier.Click += (s, e) => DeleteSupplier();

            this.btnViewIngredients.Location = new System.Drawing.Point(342, 410);
            this.btnViewIngredients.Size = new System.Drawing.Size(120, 30);
            this.btnViewIngredients.Text = "View Ingredients";
            this.btnViewIngredients.Click += (s, e) => ViewSupplierIngredients();

            this.btnClose.Location = new System.Drawing.Point(588, 410);
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, dataGridViewSuppliers, btnAddSupplier, btnEditSupplier,
                btnDeleteSupplier, btnViewIngredients, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = DatabaseContext.GetAllSuppliers();
                
                var searchTerm = txtSearch.Text.ToLower();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    suppliers = suppliers.Where(s => 
                        s.Name.ToLower().Contains(searchTerm) ||
                        s.ContactPerson.ToLower().Contains(searchTerm) ||
                        s.Email.ToLower().Contains(searchTerm)
                    ).ToList();
                }

                dataGridViewSuppliers.DataSource = suppliers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddSupplier()
        {
            try
            {
                using (var form = new SupplierEditForm())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadSuppliers();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding supplier: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSupplier()
        {
            if (dataGridViewSuppliers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a supplier to edit.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var supplier = (Supplier)dataGridViewSuppliers.SelectedRows[0].DataBoundItem;
                using (var form = new SupplierEditForm(supplier))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadSuppliers();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing supplier: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSupplier()
        {
            if (dataGridViewSuppliers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a supplier to delete.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var supplier = (Supplier)dataGridViewSuppliers.SelectedRows[0].DataBoundItem;
                var result = MessageBox.Show($"Are you sure you want to delete {supplier.Name}?", 
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    DatabaseContext.DeleteSupplier(supplier.Id);
                    LoadSuppliers();
                    MessageBox.Show("Supplier deleted successfully.", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting supplier: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewSupplierIngredients()
        {
            if (dataGridViewSuppliers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a supplier to view ingredients.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var supplier = (Supplier)dataGridViewSuppliers.SelectedRows[0].DataBoundItem;
                
                // FIXED: Now passes the selected supplier to the form
                using (var form = new SupplierIngredientsForm(supplier))
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing supplier ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}