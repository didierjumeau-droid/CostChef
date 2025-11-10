using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public partial class SupplierManagementForm : Form
    {
        private List<Supplier> suppliers;
        private DataGridView dataGridViewSuppliers;
        private Button btnAddSupplier;
        private Button btnEditSupplier;
        private Button btnDeleteSupplier;
        private Button btnViewIngredients;
        private Button btnReports;
        private Button btnClose;
        private TextBox txtSearch;
        private Button btnSearch;
        private System.ComponentModel.IContainer components = null;

        public SupplierManagementForm()
        {
            InitializeComponent();
            LoadSuppliers();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Text = "Supplier Management";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(900, 600);

            // Main layout panel
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            this.Controls.Add(mainPanel);

            // Title
            var lblTitle = new Label 
            { 
                Text = "Supplier Management",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            mainPanel.Controls.Add(lblTitle);

            // Search panel
            var searchPanel = new Panel
            {
                Location = new Point(10, 50),
                Size = new System.Drawing.Size(860, 35),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(searchPanel);

            var lblSearch = new Label 
            { 
                Text = "Search:",
                Location = new Point(10, 8),
                AutoSize = true
            };
            searchPanel.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(60, 5),
                Size = new System.Drawing.Size(200, 20)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            searchPanel.Controls.Add(txtSearch);

            btnSearch = new Button
            {
                Text = "Clear",
                Location = new Point(270, 5),
                Size = new System.Drawing.Size(60, 23)
            };
            btnSearch.Click += BtnSearch_Click;
            searchPanel.Controls.Add(btnSearch);

            // DataGridView
            dataGridViewSuppliers = new DataGridView
            {
                Location = new Point(10, 100),
                Size = new System.Drawing.Size(860, 350),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            // Configure columns
            dataGridViewSuppliers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "colId", 
                HeaderText = "ID", 
                DataPropertyName = "Id",
                Width = 50 
            });
            dataGridViewSuppliers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "colName", 
                HeaderText = "Name", 
                DataPropertyName = "Name",
                Width = 150 
            });
            dataGridViewSuppliers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "colContact", 
                HeaderText = "Contact Person", 
                DataPropertyName = "ContactPerson",
                Width = 120 
            });
            dataGridViewSuppliers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "colPhone", 
                HeaderText = "Phone", 
                DataPropertyName = "Phone",
                Width = 100 
            });
            dataGridViewSuppliers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "colEmail", 
                HeaderText = "Email", 
                DataPropertyName = "Email",
                Width = 150 
            });
            dataGridViewSuppliers.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "colAddress", 
                HeaderText = "Address", 
                DataPropertyName = "Address",
                Width = 200 
            });

            mainPanel.Controls.Add(dataGridViewSuppliers);

            // Button panel
            var buttonPanel = new Panel
            {
                Location = new Point(10, 460),
                Size = new System.Drawing.Size(860, 40),
                Anchor = AnchorStyles.Bottom
            };
            mainPanel.Controls.Add(buttonPanel);

            btnAddSupplier = new Button
            {
                Text = "Add Supplier",
                Location = new Point(0, 5),
                Size = new System.Drawing.Size(100, 30)
            };
            btnAddSupplier.Click += BtnAddSupplier_Click;
            buttonPanel.Controls.Add(btnAddSupplier);

            btnEditSupplier = new Button
            {
                Text = "Edit Supplier",
                Location = new Point(110, 5),
                Size = new System.Drawing.Size(100, 30)
            };
            btnEditSupplier.Click += BtnEditSupplier_Click;
            buttonPanel.Controls.Add(btnEditSupplier);

            btnDeleteSupplier = new Button
            {
                Text = "Delete Supplier",
                Location = new Point(220, 5),
                Size = new System.Drawing.Size(100, 30)
            };
            btnDeleteSupplier.Click += BtnDeleteSupplier_Click;
            buttonPanel.Controls.Add(btnDeleteSupplier);

            btnViewIngredients = new Button
            {
                Text = "View Ingredients",
                Location = new Point(330, 5),
                Size = new System.Drawing.Size(120, 30)
            };
            btnViewIngredients.Click += BtnViewIngredients_Click;
            buttonPanel.Controls.Add(btnViewIngredients);

            btnReports = new Button
            {
                Text = "Reports",
                Location = new Point(460, 5),
                Size = new System.Drawing.Size(80, 30)
            };
            btnReports.Click += BtnReports_Click;
            buttonPanel.Controls.Add(btnReports);

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(770, 5),
                Size = new System.Drawing.Size(80, 30)
            };
            btnClose.Click += BtnClose_Click;
            buttonPanel.Controls.Add(btnClose);

            // Status label
            var lblStatus = new Label
            {
                Location = new Point(10, 510),
                Size = new System.Drawing.Size(400, 20),
                Anchor = AnchorStyles.Bottom,
                Text = "Select a supplier to manage"
            };
            mainPanel.Controls.Add(lblStatus);
        }

        private void LoadSuppliers()
        {
            try
            {
                suppliers = DatabaseContext.GetAllSuppliers();
                dataGridViewSuppliers.DataSource = suppliers;
                UpdateStatus($"Loaded {suppliers.Count} suppliers");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error loading suppliers");
            }
        }

        private void BtnAddSupplier_Click(object sender, EventArgs e)
        {
            var editForm = new SupplierEditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadSuppliers(); // Refresh the list
                UpdateStatus("Supplier added successfully");
            }
        }

        private void BtnEditSupplier_Click(object sender, EventArgs e)
        {
            if (dataGridViewSuppliers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a supplier to edit.", "Selection Required", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRow = dataGridViewSuppliers.SelectedRows[0];
            if (selectedRow.DataBoundItem is Supplier supplier)
            {
                var editForm = new SupplierEditForm(supplier);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadSuppliers(); // Refresh the list
                    UpdateStatus("Supplier updated successfully");
                }
            }
        }

        private void BtnDeleteSupplier_Click(object sender, EventArgs e)
        {
            if (dataGridViewSuppliers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a supplier to delete.", "Selection Required", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRow = dataGridViewSuppliers.SelectedRows[0];
            if (selectedRow.DataBoundItem is Supplier supplier)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete supplier '{supplier.Name}'?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        DatabaseContext.DeleteSupplier(supplier.Id);
                        LoadSuppliers(); // Refresh the list
                        UpdateStatus("Supplier deleted successfully");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting supplier: {ex.Message}", "Error", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnViewIngredients_Click(object sender, EventArgs e)
        {
            if (dataGridViewSuppliers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a supplier to view ingredients.", "Selection Required", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRow = dataGridViewSuppliers.SelectedRows[0];
            if (selectedRow.DataBoundItem is Supplier supplier)
            {
                // FIXED: Pass the supplier ID to the ingredients form
                var ingredientsForm = new SupplierIngredientsForm(supplier.Id);
                ingredientsForm.ShowDialog();
            }
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            var reportsForm = new SupplierReportsForm();
            reportsForm.ShowDialog();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterSuppliers();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            FilterSuppliers();
        }

        private void FilterSuppliers()
        {
            if (suppliers == null) return;

            var searchText = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                dataGridViewSuppliers.DataSource = suppliers;
            }
            else
            {
                var filtered = suppliers.Where(s =>
                    s.Name.ToLower().Contains(searchText) ||
                    (s.ContactPerson ?? "").ToLower().Contains(searchText) ||
                    (s.Phone ?? "").ToLower().Contains(searchText) ||
                    (s.Email ?? "").ToLower().Contains(searchText) ||
                    (s.Address ?? "").ToLower().Contains(searchText)
                ).ToList();

                dataGridViewSuppliers.DataSource = filtered;
            }

            UpdateStatus($"Displaying {dataGridViewSuppliers.Rows.Count} suppliers");
        }

        private void UpdateStatus(string message)
        {
            // Update status label if you added one
            // For now, we can set the form text or just ignore
            this.Text = $"Supplier Management - {message}";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}