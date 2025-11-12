using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace CostChef
{
    public partial class SupplierIngredientsForm : Form
    {
        private int? _selectedSupplierId;
        private ComboBox cmbSuppliers;
        private DataGridView dataGridView1;
        private Label lblTitle;
        private Button btnClose;
        private Button btnRemoveAssignment;
        private Label lblStatus;
        private System.ComponentModel.IContainer components = null;

        // Default constructor
        public SupplierIngredientsForm()
        {
            InitializeComponent();
        }

        // Constructor with supplier ID
        public SupplierIngredientsForm(int supplierId) : this()
        {
            _selectedSupplierId = supplierId;
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Supplier Ingredients";
            this.StartPosition = FormStartPosition.CenterParent;

            // Main layout panel
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            this.Controls.Add(mainPanel);

            // Title label
            lblTitle = new Label 
            { 
                Text = "Supplier Ingredients",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            mainPanel.Controls.Add(lblTitle);

            // Supplier selection
            var lblSupplier = new Label 
            { 
                Text = "Select Supplier:",
                Location = new Point(10, 50),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblSupplier);

            cmbSuppliers = new ComboBox
            {
                Location = new Point(120, 47),
                Size = new Size(300, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            mainPanel.Controls.Add(cmbSuppliers);

            // DataGridView for ingredients
            dataGridView1 = new DataGridView
            {
                Location = new Point(10, 90),
                Size = new Size(760, 350),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true
            };

            // Configure columns
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "colName", 
                HeaderText = "Ingredient Name", 
                DataPropertyName = "Name",
                Width = 200 
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "colUnit", 
                HeaderText = "Unit", 
                DataPropertyName = "Unit",
                Width = 80 
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "colPrice", 
                HeaderText = "Unit Price", 
                DataPropertyName = "UnitPrice",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F4" }
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "colCategory", 
                HeaderText = "Category", 
                DataPropertyName = "Category",
                Width = 150 
            });

            mainPanel.Controls.Add(dataGridView1);

            // Status label
            lblStatus = new Label
            {
                Location = new Point(10, 450),
                Size = new Size(400, 20),
                Text = "Select ingredients to remove supplier assignment"
            };
            mainPanel.Controls.Add(lblStatus);

            // Button panel
            var buttonPanel = new Panel
            {
                Location = new Point(10, 480),
                Size = new Size(760, 40)
            };
            mainPanel.Controls.Add(buttonPanel);

            // Remove Assignment button
            btnRemoveAssignment = new Button
            {
                Text = "Remove Supplier Assignment",
                Location = new Point(0, 5),
                Size = new Size(180, 30),
                Enabled = false
            };
            btnRemoveAssignment.Click += BtnRemoveAssignment_Click;
            buttonPanel.Controls.Add(btnRemoveAssignment);

            // Close button
            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(670, 5),
                Size = new Size(90, 30)
            };
            btnClose.Click += BtnClose_Click;
            buttonPanel.Controls.Add(btnClose);

            // Wire up events
            cmbSuppliers.SelectedIndexChanged += CmbSuppliers_SelectedIndexChanged;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            
            // Handle form load event
            this.Load += SupplierIngredientsForm_Load;
        }

        private void SupplierIngredientsForm_Load(object sender, EventArgs e)
        {
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            try
            {
                var suppliers = DatabaseContext.GetAllSuppliers();
                cmbSuppliers.DataSource = suppliers;
                cmbSuppliers.DisplayMember = "Name";
                cmbSuppliers.ValueMember = "Id";
                
                // Auto-select after ComboBox is populated
                if (_selectedSupplierId.HasValue)
                {
                    AutoSelectSupplier(_selectedSupplierId.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AutoSelectSupplier(int supplierId)
        {
            try
            {
                bool found = false;
                foreach (Supplier supplier in cmbSuppliers.Items)
                {
                    if (supplier.Id == supplierId)
                    {
                        cmbSuppliers.SelectedItem = supplier;
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    MessageBox.Show($"Supplier with ID {supplierId} not found.", "Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error auto-selecting supplier: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSupplierIngredients(int supplierId)
        {
            try
            {
                var ingredients = DatabaseContext.GetIngredientsBySupplier(supplierId);
                dataGridView1.DataSource = ingredients;

                // Update title to show supplier name
                if (cmbSuppliers.SelectedItem is Supplier selectedSupplier)
                {
                    lblTitle.Text = $"Ingredients from {selectedSupplier.Name}";
                    this.Text = $"Supplier Ingredients - {selectedSupplier.Name}";
                }

                UpdateStatusLabel(ingredients.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading supplier ingredients: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatusLabel(int ingredientCount)
        {
            lblStatus.Text = $"{ingredientCount} ingredients found. Select items to remove supplier assignment.";
        }

        private void CmbSuppliers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSuppliers.SelectedItem is Supplier selectedSupplier)
            {
                LoadSupplierIngredients(selectedSupplier.Id);
                btnRemoveAssignment.Enabled = false;
            }
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            btnRemoveAssignment.Enabled = dataGridView1.SelectedRows.Count > 0;
        }

        private void BtnRemoveAssignment_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select ingredients to remove supplier assignment.", 
                              "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbSuppliers.SelectedItem is not Supplier currentSupplier)
            {
                MessageBox.Show("No supplier selected.", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var selectedIngredients = new List<Ingredient>();
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                if (row.DataBoundItem is Ingredient ingredient)
                {
                    selectedIngredients.Add(ingredient);
                }
            }

            if (selectedIngredients.Count == 0) return;

            var result = MessageBox.Show(
                $"Remove supplier assignment from {selectedIngredients.Count} ingredient(s)?\n\n" +
                $"This will unlink these ingredients from {currentSupplier.Name} but keep the ingredients in your inventory.",
                "Confirm Remove Supplier Assignment",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int successCount = 0;
                    foreach (var ingredient in selectedIngredients)
                    {
                        ingredient.SupplierId = null;
                        DatabaseContext.UpdateIngredient(ingredient);
                        successCount++;
                    }

                    LoadSupplierIngredients(currentSupplier.Id);
                    
                    MessageBox.Show($"Successfully removed supplier assignment from {successCount} ingredient(s).",
                                  "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error removing supplier assignments: {ex.Message}", 
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
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