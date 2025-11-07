using System;
using System.Windows.Forms;

namespace CostChef
{
    public partial class SupplierEditForm : Form
    {
        private TextBox txtName;
        private TextBox txtContactPerson;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtAddress;
        private Button btnSave;
        private Button btnCancel;

        private Supplier currentSupplier;

        public SupplierEditForm()
        {
            currentSupplier = new Supplier();
            InitializeComponent();
        }

        public SupplierEditForm(Supplier supplier)
        {
            currentSupplier = supplier;
            InitializeComponent();
            LoadSupplierData();
        }

        private void InitializeComponent()
        {
            this.txtName = new TextBox();
            this.txtContactPerson = new TextBox();
            this.txtPhone = new TextBox();
            this.txtEmail = new TextBox();
            this.txtAddress = new TextBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Text = currentSupplier.Id == 0 ? "Add New Supplier" : "Edit Supplier";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Labels and TextBoxes
            var lblName = new Label { Text = "Supplier Name:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            this.txtName.Location = new System.Drawing.Point(120, 17);
            this.txtName.Size = new System.Drawing.Size(250, 20);
            this.txtName.MaxLength = 100;

            var lblContact = new Label { Text = "Contact Person:", Location = new System.Drawing.Point(20, 50), AutoSize = true };
            this.txtContactPerson.Location = new System.Drawing.Point(120, 47);
            this.txtContactPerson.Size = new System.Drawing.Size(250, 20);
            this.txtContactPerson.MaxLength = 100;

            var lblPhone = new Label { Text = "Phone:", Location = new System.Drawing.Point(20, 80), AutoSize = true };
            this.txtPhone.Location = new System.Drawing.Point(120, 77);
            this.txtPhone.Size = new System.Drawing.Size(250, 20);
            this.txtPhone.MaxLength = 20;

            var lblEmail = new Label { Text = "Email:", Location = new System.Drawing.Point(20, 110), AutoSize = true };
            this.txtEmail.Location = new System.Drawing.Point(120, 107);
            this.txtEmail.Size = new System.Drawing.Size(250, 20);
            this.txtEmail.MaxLength = 100;

            var lblAddress = new Label { Text = "Address:", Location = new System.Drawing.Point(20, 140), AutoSize = true };
            this.txtAddress.Location = new System.Drawing.Point(120, 137);
            this.txtAddress.Size = new System.Drawing.Size(250, 60);
            this.txtAddress.Multiline = true;
            this.txtAddress.ScrollBars = ScrollBars.Vertical;
            this.txtAddress.MaxLength = 255;

            // Buttons
            this.btnSave.Text = "Save";
            this.btnSave.DialogResult = DialogResult.OK;
            this.btnSave.Location = new System.Drawing.Point(120, 220);
            this.btnSave.Size = new System.Drawing.Size(80, 30);
            this.btnSave.Click += (s, e) => SaveSupplier();

            this.btnCancel.Text = "Cancel";
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(210, 220);
            this.btnCancel.Size = new System.Drawing.Size(80, 30);

            this.Controls.AddRange(new Control[] {
                lblName, txtName, lblContact, txtContactPerson, lblPhone, txtPhone,
                lblEmail, txtEmail, lblAddress, txtAddress, btnSave, btnCancel
            });

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSupplierData()
        {
            txtName.Text = currentSupplier.Name;
            txtContactPerson.Text = currentSupplier.ContactPerson;
            txtPhone.Text = currentSupplier.Phone;
            txtEmail.Text = currentSupplier.Email;
            txtAddress.Text = currentSupplier.Address;
        }

        private void SaveSupplier()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a supplier name.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            try
            {
                currentSupplier.Name = txtName.Text.Trim();
                currentSupplier.ContactPerson = txtContactPerson.Text.Trim();
                currentSupplier.Phone = txtPhone.Text.Trim();
                currentSupplier.Email = txtEmail.Text.Trim();
                currentSupplier.Address = txtAddress.Text.Trim();

                if (currentSupplier.Id == 0)
                {
                    // New supplier
                    DatabaseContext.InsertSupplier(currentSupplier);
                    MessageBox.Show("Supplier added successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Update existing supplier
                    DatabaseContext.UpdateSupplier(currentSupplier);
                    MessageBox.Show("Supplier updated successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving supplier: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}