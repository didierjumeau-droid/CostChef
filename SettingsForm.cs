//[file name]: SettingsForm.cs
//[file content begin]
using System;
using System.Windows.Forms;
using System.IO;

namespace CostChef
{
    public partial class SettingsForm : Form
    {
        private ComboBox cmbCurrencySymbol;
        private ComboBox cmbCurrencyCode;
        private NumericUpDown numDecimalPlaces;
        private CheckBox chkAutoSave;
        private Button btnSave;
        private Button btnCancel;
        
        // NEW: Export location controls
        private Label lblExportLocation;
        private TextBox txtExportLocation;
        private Button btnBrowseExportLocation;
        private Button btnResetExportLocation;

        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.cmbCurrencySymbol = new ComboBox();
            this.cmbCurrencyCode = new ComboBox();
            this.numDecimalPlaces = new NumericUpDown();
            this.chkAutoSave = new CheckBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            
            // NEW: Export location controls
            this.lblExportLocation = new Label();
            this.txtExportLocation = new TextBox();
            this.btnBrowseExportLocation = new Button();
            this.btnResetExportLocation = new Button();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(450, 280);
            this.Text = "Settings";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Currency Symbol
            var lblCurrencySymbol = new Label { 
                Text = "Currency Symbol:", 
                Location = new System.Drawing.Point(20, 20), 
                AutoSize = true 
            };
            
            this.cmbCurrencySymbol.Location = new System.Drawing.Point(150, 17);
            this.cmbCurrencySymbol.Size = new System.Drawing.Size(100, 20);
            this.cmbCurrencySymbol.Items.AddRange(new object[] { "$", "€", "£", "¥", "₹", "₽", "₩", "₺" });

            // Currency Code
            var lblCurrencyCode = new Label { 
                Text = "Currency Code:", 
                Location = new System.Drawing.Point(20, 50), 
                AutoSize = true 
            };
            
            this.cmbCurrencyCode.Location = new System.Drawing.Point(150, 47);
            this.cmbCurrencyCode.Size = new System.Drawing.Size(100, 20);
            this.cmbCurrencyCode.Items.AddRange(new object[] { "USD", "EUR", "GBP", "JPY", "INR", "RUB", "KRW", "TRY", "CAD", "AUD" });

            // Decimal Places
            var lblDecimalPlaces = new Label { 
                Text = "Decimal Places:", 
                Location = new System.Drawing.Point(20, 80), 
                AutoSize = true 
            };
            
            this.numDecimalPlaces.Location = new System.Drawing.Point(150, 77);
            this.numDecimalPlaces.Size = new System.Drawing.Size(100, 20);
            this.numDecimalPlaces.Minimum = 0;
            this.numDecimalPlaces.Maximum = 4;
            this.numDecimalPlaces.Value = 2;

            // Auto Save
            this.chkAutoSave.Location = new System.Drawing.Point(20, 110);
            this.chkAutoSave.Size = new System.Drawing.Size(200, 20);
            this.chkAutoSave.Text = "Enable Auto Save";

            // NEW: Export Location
            this.lblExportLocation.Location = new System.Drawing.Point(20, 140);
            this.lblExportLocation.Size = new System.Drawing.Size(120, 20);
            this.lblExportLocation.Text = "Export Location:";
            this.lblExportLocation.AutoSize = true;

            this.txtExportLocation.Location = new System.Drawing.Point(150, 137);
            this.txtExportLocation.Size = new System.Drawing.Size(200, 20);
            this.txtExportLocation.ReadOnly = true;

            this.btnBrowseExportLocation.Location = new System.Drawing.Point(360, 135);
            this.btnBrowseExportLocation.Size = new System.Drawing.Size(75, 25);
            this.btnBrowseExportLocation.Text = "Browse...";
            this.btnBrowseExportLocation.Click += (s, e) => BrowseExportLocation();

            this.btnResetExportLocation.Location = new System.Drawing.Point(150, 165);
            this.btnResetExportLocation.Size = new System.Drawing.Size(100, 25);
            this.btnResetExportLocation.Text = "Reset to Default";
            this.btnResetExportLocation.Click += (s, e) => ResetExportLocation();

            // Buttons
            this.btnSave.Location = new System.Drawing.Point(280, 220);
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.Text = "Save";
            this.btnSave.Click += (s, e) => SaveSettings();

            this.btnCancel.Location = new System.Drawing.Point(365, 220);
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] {
                lblCurrencySymbol, cmbCurrencySymbol,
                lblCurrencyCode, cmbCurrencyCode,
                lblDecimalPlaces, numDecimalPlaces,
                chkAutoSave,
                lblExportLocation, txtExportLocation, btnBrowseExportLocation, btnResetExportLocation,
                btnSave, btnCancel
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSettings()
        {
            try
            {
                var settings = DatabaseContext.GetAllSettings();
                
                if (settings.ContainsKey("CurrencySymbol"))
                    cmbCurrencySymbol.Text = settings["CurrencySymbol"];
                else
                    cmbCurrencySymbol.SelectedIndex = 0;
                
                if (settings.ContainsKey("CurrencyCode"))
                    cmbCurrencyCode.Text = settings["CurrencyCode"];
                else
                    cmbCurrencyCode.SelectedIndex = 0;
                
                if (settings.ContainsKey("DecimalPlaces") && int.TryParse(settings["DecimalPlaces"], out int decimalPlaces))
                    numDecimalPlaces.Value = decimalPlaces;
                
                if (settings.ContainsKey("AutoSave"))
                    chkAutoSave.Checked = settings["AutoSave"] == "true";

                // NEW: Load export location
                if (settings.ContainsKey("ExportLocation") && !string.IsNullOrEmpty(settings["ExportLocation"]))
                {
                    txtExportLocation.Text = settings["ExportLocation"];
                }
                else
                {
                    // Set default export location to Documents\CostChef\Exports
                    string defaultExportPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "CostChef",
                        "Exports"
                    );
                    txtExportLocation.Text = defaultExportPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BrowseExportLocation()
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select default export location for CSV files";
                folderDialog.SelectedPath = txtExportLocation.Text;
                
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtExportLocation.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void ResetExportLocation()
        {
            string defaultExportPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CostChef",
                "Exports"
            );
            txtExportLocation.Text = defaultExportPath;
        }

        private void SaveSettings()
        {
            try
            {
                // Validate export location
                if (!string.IsNullOrEmpty(txtExportLocation.Text))
                {
                    try
                    {
                        // Test if path is valid
                        Path.GetFullPath(txtExportLocation.Text);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("The export location path is not valid. Please select a valid folder.", 
                            "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                DatabaseContext.SetSetting("CurrencySymbol", cmbCurrencySymbol.Text);
                DatabaseContext.SetSetting("CurrencyCode", cmbCurrencyCode.Text);
                DatabaseContext.SetSetting("DecimalPlaces", numDecimalPlaces.Value.ToString());
                DatabaseContext.SetSetting("AutoSave", chkAutoSave.Checked ? "true" : "false");
                
                // NEW: Save export location
                DatabaseContext.SetSetting("ExportLocation", txtExportLocation.Text);

                // Update AppSettings
                AppSettings.LoadSettings();
                
                MessageBox.Show("Settings saved successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
// [file content end]