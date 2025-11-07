using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CostChef
{
    public partial class SettingsForm : Form
    {
        private ComboBox cmbCurrency;
        private ComboBox cmbDecimalPlaces;
        private CheckBox chkAutoSave;
        private Button btnSave;
        private Button btnCancel;
        private Label lblTitle;

        public SettingsForm()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.cmbCurrency = new ComboBox();
            this.cmbDecimalPlaces = new ComboBox();
            this.chkAutoSave = new CheckBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.lblTitle = new Label();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(350, 200);
            this.Text = "CostChef Settings";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title
            this.lblTitle.Text = "General Settings";
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Size = new System.Drawing.Size(200, 20);
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // Currency
            var lblCurrency = new Label { 
                Text = "Currency:", 
                Location = new System.Drawing.Point(20, 50), 
                AutoSize = true 
            };
            
            this.cmbCurrency.Location = new System.Drawing.Point(120, 47);
            this.cmbCurrency.Size = new System.Drawing.Size(200, 20);
            this.cmbCurrency.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbCurrency.DropDownWidth = 250;
            
            // Expanded currency list
            var currencies = new Dictionary<string, string>
            {
                {"USD", "US Dollar ($)"},
                {"CAD", "Canadian Dollar (C$)"},
                {"GBP", "British Pound (£)"},
                {"EUR", "Euro (€)"},
                {"AUD", "Australian Dollar (A$)"},
                {"NZD", "New Zealand Dollar (NZ$)"},
                {"JPY", "Japanese Yen (¥)"},
                {"CNY", "Chinese Yuan (¥)"},
                {"INR", "Indian Rupee (₹)"},
                {"PHP", "Philippine Peso (₱)"},
                {"SGD", "Singapore Dollar (S$)"},
                {"MYR", "Malaysian Ringgit (RM)"},
                {"THB", "Thai Baht (฿)"},
                {"IDR", "Indonesian Rupiah (Rp)"},
                {"ZAR", "South African Rand (R)"},
                {"BRL", "Brazilian Real (R$)"},
                {"MXN", "Mexican Peso ($)"},
                {"SAR", "Saudi Riyal (ر.س)"},
                {"AED", "UAE Dirham (د.إ)"},
                {"CHF", "Swiss Franc (CHF)"}
            };
            
            foreach (var currency in currencies)
            {
                cmbCurrency.Items.Add($"{currency.Key} - {currency.Value}");
            }

            // Decimal Places
            var lblDecimal = new Label { 
                Text = "Decimal Places:", 
                Location = new System.Drawing.Point(20, 80), 
                AutoSize = true 
            };
            
            this.cmbDecimalPlaces.Location = new System.Drawing.Point(120, 77);
            this.cmbDecimalPlaces.Size = new System.Drawing.Size(80, 20);
            this.cmbDecimalPlaces.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbDecimalPlaces.Items.AddRange(new object[] { "0", "1", "2", "3", "4" });

            // Auto-save
            this.chkAutoSave.Text = "Auto-save after edits";
            this.chkAutoSave.Location = new System.Drawing.Point(20, 110);
            this.chkAutoSave.Size = new System.Drawing.Size(200, 20);
            this.chkAutoSave.Checked = true;

            // Buttons
            this.btnSave.Text = "Save Settings";
            this.btnSave.Location = new System.Drawing.Point(120, 150);
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.Click += (s, e) => SaveSettings();

            this.btnCancel.Text = "Cancel";
            this.btnCancel.Location = new System.Drawing.Point(230, 150);
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.Click += (s, e) => this.Close();

            // Add controls
            this.Controls.AddRange(new Control[] {
                lblTitle, lblCurrency, cmbCurrency, lblDecimal, cmbDecimalPlaces,
                chkAutoSave, btnSave, btnCancel
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadCurrentSettings()
        {
            // Load current settings
            var currentCurrency = $"{AppSettings.CurrencyCode} - ";
            var decimalPlaces = AppSettings.DecimalPlaces.ToString();
            var autoSave = AppSettings.AutoSave;

            // Find and select current currency
            for (int i = 0; i < cmbCurrency.Items.Count; i++)
            {
                if (cmbCurrency.Items[i].ToString().StartsWith(currentCurrency))
                {
                    cmbCurrency.SelectedIndex = i;
                    break;
                }
            }

            // Select decimal places
            cmbDecimalPlaces.SelectedItem = decimalPlaces;
            chkAutoSave.Checked = autoSave;
        }

        private void SaveSettings()
        {
            try
            {
                // Parse currency selection
                if (cmbCurrency.SelectedItem != null)
                {
                    var selectedCurrency = cmbCurrency.SelectedItem.ToString();
                    var parts = selectedCurrency.Split(new[] { " - " }, StringSplitOptions.None);
                    if (parts.Length >= 1)
                    {
                        var currencyCode = parts[0];
                        // Extract symbol from the display text (it's in parentheses)
                        var symbol = "$"; // default
                        if (parts.Length > 1 && parts[1].Contains("("))
                        {
                            var symbolStart = parts[1].IndexOf('(') + 1;
                            var symbolEnd = parts[1].IndexOf(')');
                            if (symbolEnd > symbolStart)
                            {
                                symbol = parts[1].Substring(symbolStart, symbolEnd - symbolStart);
                            }
                        }
                        
                        AppSettings.UpdateCurrency(currencyCode, symbol);
                    }
                }

                // Save other settings
                if (cmbDecimalPlaces.SelectedItem != null)
                {
                    AppSettings.Set("DecimalPlaces", cmbDecimalPlaces.SelectedItem.ToString());
                }
                
                AppSettings.Set("AutoSave", chkAutoSave.Checked.ToString().ToLower());

                MessageBox.Show("Settings saved successfully!\n\nChanges will take effect immediately.", "Settings Saved", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
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