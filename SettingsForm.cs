using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace CostChef
{
    public partial class SettingsForm : Form
    {
        private TabControl tabControl;
        private Button btnSave;
        private Button btnCancel;
        private Button btnApply;

        // General Settings
        private ComboBox cmbCurrencyCode;
        private Label lblCurrencySymbol;
        private ComboBox cmbDecimalPlaces;
        private CheckBox chkConfirmDeletes;
        private CheckBox chkAutoCalculate;

        // Unit Preferences
        private ComboBox cmbWeightUnit;
        private ComboBox cmbVolumeUnit;
        private ComboBox cmbCountUnit;
        private CheckBox chkAutoConvertUnits;

        // Category Management
        private ListBox lstIngredientCategories;
        private ListBox lstRecipeCategories;
        private TextBox txtNewIngredientCategory;
        private TextBox txtNewRecipeCategory;
        private Button btnAddIngredientCategory;
        private Button btnAddRecipeCategory;
        private Button btnRemoveIngredientCategory;
        private Button btnRemoveRecipeCategory;

        // Default Values
        private ComboBox cmbDefaultIngredientCategory;
        private ComboBox cmbDefaultRecipeCategory;
        private ComboBox cmbDefaultFoodCost;

        // Currency mapping
        private Dictionary<string, string> currencyMap = new Dictionary<string, string>
        {
            { "USD", "$" }, { "EUR", "€" }, { "JPY", "¥" }, { "GBP", "£" },
            { "CNY", "¥" }, { "CHF", "CHF" }, { "AUD", "A$" }, { "CAD", "C$" },
            { "HKD", "HK$" }, { "SGD", "S$" }, { "INR", "₹" }, { "KRW", "₩" },
            { "SEK", "kr" }, { "MXN", "Mex$" }, { "NZD", "NZ$" }, { "NOK", "kr" },
            { "TWD", "NT$" }, { "BRL", "R$" }, { "ZAR", "R" }, { "PLN", "zł" },
            { "PHP", "₱" }
        };

        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
            LoadCategories();
            LoadUnits();
        }

        private void InitializeComponent()
        {
            this.tabControl = new TabControl();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.btnApply = new Button();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(600, 500);
            this.Text = "Settings";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Tab Control
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Size = new System.Drawing.Size(576, 400);
            this.tabControl.TabStop = false;

            // Create tabs
            CreateGeneralTab();
            CreateUnitsTab();
            CreateCategoriesTab();
            CreateDefaultsTab();

            // Buttons
            this.btnSave.Text = "Save";
            this.btnSave.Location = new System.Drawing.Point(350, 425);
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.Click += (s, e) => SaveSettings();

            this.btnCancel.Text = "Cancel";
            this.btnCancel.Location = new System.Drawing.Point(435, 425);
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.btnApply.Text = "Apply";
            this.btnApply.Location = new System.Drawing.Point(520, 425);
            this.btnApply.Size = new System.Drawing.Size(75, 30);
            this.btnApply.Click += (s, e) => ApplySettings();

            this.Controls.AddRange(new Control[] {
                tabControl, btnSave, btnCancel, btnApply
            });

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            this.ResumeLayout(false);
        }

        private void CreateGeneralTab()
        {
            var tab = new TabPage("General");
            tab.Size = new System.Drawing.Size(572, 370);

            int yPos = 20;

            // Currency Code
            var lblCurrencyCode = new Label { 
                Text = "Currency:", 
                Location = new System.Drawing.Point(20, yPos), 
                AutoSize = true 
            };
            cmbCurrencyCode = new ComboBox { 
                Location = new System.Drawing.Point(150, yPos - 3), 
                Size = new System.Drawing.Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // Currency codes for selection
            cmbCurrencyCode.Items.AddRange(new object[] { 
                "USD", "EUR", "JPY", "GBP", "CNY", "CHF", "AUD", "CAD", 
                "HKD", "SGD", "INR", "KRW", "SEK", "MXN", "NZD", "NOK", 
                "TWD", "BRL", "ZAR", "PLN", "PHP" 
            });
            cmbCurrencyCode.SelectedIndexChanged += (s, e) => UpdateCurrencySymbol();

            // Currency Symbol Display
            var lblSymbol = new Label { 
                Text = "Symbol:", 
                Location = new System.Drawing.Point(260, yPos), 
                AutoSize = true 
            };
            lblCurrencySymbol = new Label { 
                Text = "$",
                Location = new System.Drawing.Point(310, yPos),
                Size = new System.Drawing.Size(50, 20),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            yPos += 35;

            // Decimal Places
            var lblDecimalPlaces = new Label { 
                Text = "Decimal Places:", 
                Location = new System.Drawing.Point(20, yPos), 
                AutoSize = true 
            };
            cmbDecimalPlaces = new ComboBox { 
                Location = new System.Drawing.Point(150, yPos - 3), 
                Size = new System.Drawing.Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
  // In CreateGeneralTab() method - update the cmbDecimalPlaces setup:
cmbDecimalPlaces.Items.AddRange(new object[] { "0 (Whole numbers)", "1 (Tenths)", "2 (Money)" });
cmbDecimalPlaces.SelectedIndex = 2; // Default to 2 decimal places

            yPos += 35;

            // Confirm Deletes
            chkConfirmDeletes = new CheckBox { 
                Text = "Confirm before deleting items",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };

            yPos += 30;

            // Auto Calculate
            chkAutoCalculate = new CheckBox { 
                Text = "Auto-calculate costs when changes occur",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };

            tab.Controls.AddRange(new Control[] {
                lblCurrencyCode, cmbCurrencyCode, lblSymbol, lblCurrencySymbol,
                lblDecimalPlaces, cmbDecimalPlaces,
                chkConfirmDeletes, chkAutoCalculate
            });

            tabControl.TabPages.Add(tab);
        }

        private void CreateUnitsTab()
        {
            var tab = new TabPage("Units");
            tab.Size = new System.Drawing.Size(572, 370);

            int yPos = 20;

            // Weight Unit
            var lblWeightUnit = new Label { 
                Text = "Preferred Weight Unit:", 
                Location = new System.Drawing.Point(20, yPos), 
                AutoSize = true 
            };
            cmbWeightUnit = new ComboBox { 
                Location = new System.Drawing.Point(180, yPos - 3), 
                Size = new System.Drawing.Size(120, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            yPos += 35;

            // Volume Unit
            var lblVolumeUnit = new Label { 
                Text = "Preferred Volume Unit:", 
                Location = new System.Drawing.Point(20, yPos), 
                AutoSize = true 
            };
            cmbVolumeUnit = new ComboBox { 
                Location = new System.Drawing.Point(180, yPos - 3), 
                Size = new System.Drawing.Size(120, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            yPos += 35;

            // Count Unit
            var lblCountUnit = new Label { 
                Text = "Preferred Count Unit:", 
                Location = new System.Drawing.Point(20, yPos), 
                AutoSize = true 
            };
            cmbCountUnit = new ComboBox { 
                Location = new System.Drawing.Point(180, yPos - 3), 
                Size = new System.Drawing.Size(120, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            yPos += 35;

            // Auto Convert
            chkAutoConvertUnits = new CheckBox { 
                Text = "Auto-convert units when possible",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };

            yPos += 40;

            // Unit Help
            var lblUnitHelp = new Label { 
                Text = "Note: These preferences affect default units in forms and calculations.",
                Location = new System.Drawing.Point(20, yPos), 
                AutoSize = true,
                Font = new Font(this.Font, FontStyle.Italic),
                ForeColor = Color.Gray
            };

            tab.Controls.AddRange(new Control[] {
                lblWeightUnit, cmbWeightUnit,
                lblVolumeUnit, cmbVolumeUnit,
                lblCountUnit, cmbCountUnit,
                chkAutoConvertUnits, lblUnitHelp
            });

            tabControl.TabPages.Add(tab);
        }

        private void CreateCategoriesTab()
        {
            var tab = new TabPage("Categories");
            tab.Size = new System.Drawing.Size(572, 370);

            int yPos = 20;

            // Ingredient Categories Section
            var lblIngredientCategories = new Label { 
                Text = "Ingredient Categories:", 
                Location = new System.Drawing.Point(20, yPos), 
                AutoSize = true,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            yPos += 25;

            lstIngredientCategories = new ListBox { 
                Location = new System.Drawing.Point(20, yPos), 
                Size = new System.Drawing.Size(200, 120)
            };

            txtNewIngredientCategory = new TextBox { 
                Location = new System.Drawing.Point(20, yPos + 125), 
                Size = new System.Drawing.Size(120, 20),
                PlaceholderText = "New category..."
            };

            btnAddIngredientCategory = new Button { 
                Text = "Add",
                Location = new System.Drawing.Point(150, yPos + 123),
                Size = new System.Drawing.Size(70, 25)
            };
            btnAddIngredientCategory.Click += (s, e) => AddIngredientCategory();

            btnRemoveIngredientCategory = new Button { 
                Text = "Remove",
                Location = new System.Drawing.Point(20, yPos + 155),
                Size = new System.Drawing.Size(200, 25)
            };
            btnRemoveIngredientCategory.Click += (s, e) => RemoveIngredientCategory();

            // Recipe Categories Section
            var lblRecipeCategories = new Label { 
                Text = "Recipe Categories:", 
                Location = new System.Drawing.Point(250, 20), 
                AutoSize = true,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            lstRecipeCategories = new ListBox { 
                Location = new System.Drawing.Point(250, 45), 
                Size = new System.Drawing.Size(200, 120)
            };

            txtNewRecipeCategory = new TextBox { 
                Location = new System.Drawing.Point(250, 170), 
                Size = new System.Drawing.Size(120, 20),
                PlaceholderText = "New category..."
            };

            btnAddRecipeCategory = new Button { 
                Text = "Add",
                Location = new System.Drawing.Point(380, 168),
                Size = new System.Drawing.Size(70, 25)
            };
            btnAddRecipeCategory.Click += (s, e) => AddRecipeCategory();

            btnRemoveRecipeCategory = new Button { 
                Text = "Remove",
                Location = new System.Drawing.Point(250, 200),
                Size = new System.Drawing.Size(200, 25)
            };
            btnRemoveRecipeCategory.Click += (s, e) => RemoveRecipeCategory();

            // Help Text
            var lblCategoryHelp = new Label { 
                Text = "Note: Default categories cannot be removed. Changes affect dropdowns in forms.",
                Location = new System.Drawing.Point(20, 300), 
                AutoSize = true,
                Font = new Font(this.Font, FontStyle.Italic),
                ForeColor = Color.Gray
            };

            tab.Controls.AddRange(new Control[] {
                lblIngredientCategories, lstIngredientCategories,
                txtNewIngredientCategory, btnAddIngredientCategory, btnRemoveIngredientCategory,
                lblRecipeCategories, lstRecipeCategories,
                txtNewRecipeCategory, btnAddRecipeCategory, btnRemoveRecipeCategory,
                lblCategoryHelp
            });

            tabControl.TabPages.Add(tab);
        }

        private void CreateDefaultsTab()
        {
            var tab = new TabPage("Defaults");
            tab.Size = new System.Drawing.Size(572, 370);

            int yPos = 20;

            // Default Ingredient Category
            var lblDefaultIngredientCategory = new Label { 
                Text = "Default Ingredient Category:", 
                Location = new System.Drawing.Point(20, yPos), 
                AutoSize = true 
            };
            cmbDefaultIngredientCategory = new ComboBox { 
                Location = new System.Drawing.Point(200, yPos - 3), 
                Size = new System.Drawing.Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDown
            };

            yPos += 35;

            // Default Recipe Category
            var lblDefaultRecipeCategory = new Label { 
                Text = "Default Recipe Category:", 
                Location = new System.Drawing.Point(20, yPos), 
                AutoSize = true 
            };
            cmbDefaultRecipeCategory = new ComboBox { 
                Location = new System.Drawing.Point(200, yPos - 3), 
                Size = new System.Drawing.Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDown
            };

            yPos += 35;

            // Default Food Cost
            var lblDefaultFoodCost = new Label { 
                Text = "Default Food Cost %:", 
                Location = new System.Drawing.Point(20, yPos), 
                AutoSize = true 
            };
            cmbDefaultFoodCost = new ComboBox { 
                Location = new System.Drawing.Point(200, yPos - 3), 
                Size = new System.Drawing.Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDefaultFoodCost.Items.AddRange(new object[] { "25%", "30%", "35%", "40%" });

            yPos += 50;

            // Reset to Defaults Button
            var btnResetDefaults = new Button { 
                Text = "Reset All to Default Values",
                Location = new System.Drawing.Point(20, yPos),
                Size = new System.Drawing.Size(200, 30)
            };
            btnResetDefaults.Click += (s, e) => ResetToDefaults();

            tab.Controls.AddRange(new Control[] {
                lblDefaultIngredientCategory, cmbDefaultIngredientCategory,
                lblDefaultRecipeCategory, cmbDefaultRecipeCategory,
                lblDefaultFoodCost, cmbDefaultFoodCost,
                btnResetDefaults
            });

            tabControl.TabPages.Add(tab);
        }

        private void UpdateCurrencySymbol()
        {
            if (cmbCurrencyCode.SelectedItem != null)
            {
                string code = cmbCurrencyCode.SelectedItem.ToString();
                if (currencyMap.ContainsKey(code))
                {
                    lblCurrencySymbol.Text = currencyMap[code];
                }
            }
        }

        private void LoadSettings()
        {
            try
            {
                // Load current settings
                cmbCurrencyCode.Text = AppSettings.CurrencySymbol;
                UpdateCurrencySymbol(); // Update the symbol display
                cmbDecimalPlaces.SelectedIndex = 1; // Default to 3 decimal places
                chkConfirmDeletes.Checked = true;
                chkAutoCalculate.Checked = true;
                chkAutoConvertUnits.Checked = true;
                cmbDefaultFoodCost.SelectedIndex = 1; // Default to 30%
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCategories()
        {
            try
            {
                // Load default ingredient categories
                var defaultIngredientCategories = new List<string> { 
                    "Bakery", "Beverages", "Canned & Packaged", "Dairy & Eggs", 
                    "Frozen Goods", "Fruit", "Grains", "Herbs", "Ingredients Base", 
                    "Meat & Poultry", "Oils & Fats", "Pasta", "Sauces & Condiments", 
                    "Seafood", "Spices", "Vegetables" 
                };

                // Load default recipe categories
                var defaultRecipeCategories = new List<string> { 
                    "Appetizer", "Soup & Salad", "Entrée", "Side Dish", 
                    "Dessert", "Baking", "Breakfast/Brunch", "Beverages", "Hot Drink" 
                };

                // Load existing categories from database
                var existingIngredientCategories = DatabaseContext.GetIngredientCategories();
                var existingRecipeCategories = DatabaseContext.GetRecipeCategories();

                // Populate ingredient categories list
                lstIngredientCategories.Items.Clear();
                foreach (var category in defaultIngredientCategories)
                {
                    lstIngredientCategories.Items.Add(category);
                }
                foreach (var category in existingIngredientCategories)
                {
                    if (!string.IsNullOrEmpty(category) && !lstIngredientCategories.Items.Contains(category))
                        lstIngredientCategories.Items.Add(category);
                }

                // Populate recipe categories list
                lstRecipeCategories.Items.Clear();
                foreach (var category in defaultRecipeCategories)
                {
                    lstRecipeCategories.Items.Add(category);
                }
                foreach (var category in existingRecipeCategories)
                {
                    if (!string.IsNullOrEmpty(category) && !lstRecipeCategories.Items.Contains(category))
                        lstRecipeCategories.Items.Add(category);
                }

                // Populate default category dropdowns
                cmbDefaultIngredientCategory.Items.Clear();
                cmbDefaultIngredientCategory.Items.Add(""); // Empty option
                foreach (var category in defaultIngredientCategories)
                {
                    cmbDefaultIngredientCategory.Items.Add(category);
                }

                cmbDefaultRecipeCategory.Items.Clear();
                cmbDefaultRecipeCategory.Items.Add(""); // Empty option
                foreach (var category in defaultRecipeCategories)
                {
                    cmbDefaultRecipeCategory.Items.Add(category);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUnits()
        {
            try
            {
                // Weight units
                cmbWeightUnit.Items.Clear();
                cmbWeightUnit.Items.AddRange(new object[] { "gram", "kg", "oz", "lb" });
                cmbWeightUnit.SelectedIndex = 0; // Default to grams

                // Volume units
                cmbVolumeUnit.Items.Clear();
                cmbVolumeUnit.Items.AddRange(new object[] { "ml", "liter", "cup", "tbsp", "tsp" });
                cmbVolumeUnit.SelectedIndex = 0; // Default to ml

                // Count units
                cmbCountUnit.Items.Clear();
                cmbCountUnit.Items.AddRange(new object[] { "piece", "pack", "box", "case" });
                cmbCountUnit.SelectedIndex = 0; // Default to piece
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading units: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddIngredientCategory()
        {
            var newCategory = txtNewIngredientCategory.Text.Trim();
            if (!string.IsNullOrEmpty(newCategory))
            {
                if (!lstIngredientCategories.Items.Cast<string>().Any(c => 
                    c.Equals(newCategory, StringComparison.OrdinalIgnoreCase)))
                {
                    lstIngredientCategories.Items.Add(newCategory);
                    txtNewIngredientCategory.Text = "";
                    
                    // Also add to default category dropdown
                    if (!cmbDefaultIngredientCategory.Items.Cast<string>().Any(c => 
                        c.Equals(newCategory, StringComparison.OrdinalIgnoreCase)))
                    {
                        cmbDefaultIngredientCategory.Items.Add(newCategory);
                    }
                }
                else
                {
                    MessageBox.Show("Category already exists.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RemoveIngredientCategory()
        {
            if (lstIngredientCategories.SelectedItem != null)
            {
                var selected = lstIngredientCategories.SelectedItem.ToString();
                
                // UPDATED: Default ingredient categories
                var defaultCategories = new List<string> { 
                    "Bakery", "Beverages", "Canned & Packaged", "Dairy & Eggs", 
                    "Frozen Goods", "Fruit", "Grains", "Herbs", "Ingredients Base", 
                    "Meat & Poultry", "Oils & Fats", "Pasta", "Sauces & Condiments", 
                    "Seafood", "Spices", "Vegetables" 
                };
                
                if (defaultCategories.Contains(selected))
                {
                    MessageBox.Show($"Cannot remove default category '{selected}'.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                var result = MessageBox.Show($"Remove category '{selected}'?", "Confirm Remove", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    lstIngredientCategories.Items.Remove(selected);
                    
                    // Also remove from default category dropdown
                    var itemToRemove = cmbDefaultIngredientCategory.Items.Cast<string>()
                        .FirstOrDefault(c => c.Equals(selected, StringComparison.OrdinalIgnoreCase));
                    if (itemToRemove != null)
                    {
                        cmbDefaultIngredientCategory.Items.Remove(itemToRemove);
                    }
                }
            }
        }

        private void AddRecipeCategory()
        {
            var newCategory = txtNewRecipeCategory.Text.Trim();
            if (!string.IsNullOrEmpty(newCategory))
            {
                if (!lstRecipeCategories.Items.Cast<string>().Any(c => 
                    c.Equals(newCategory, StringComparison.OrdinalIgnoreCase)))
                {
                    lstRecipeCategories.Items.Add(newCategory);
                    txtNewRecipeCategory.Text = "";
                    
                    // Also add to default category dropdown
                    if (!cmbDefaultRecipeCategory.Items.Cast<string>().Any(c => 
                        c.Equals(newCategory, StringComparison.OrdinalIgnoreCase)))
                    {
                        cmbDefaultRecipeCategory.Items.Add(newCategory);
                    }
                }
                else
                {
                    MessageBox.Show("Category already exists.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RemoveRecipeCategory()
        {
            if (lstRecipeCategories.SelectedItem != null)
            {
                var selected = lstRecipeCategories.SelectedItem.ToString();
                
                // UPDATED: Default recipe categories
                var defaultCategories = new List<string> { 
                    "Appetizer", "Soup & Salad", "Entrée", "Side Dish", 
                    "Dessert", "Baking", "Breakfast/Brunch", "Beverages", "Hot Drink" 
                };
                
                if (defaultCategories.Contains(selected))
                {
                    MessageBox.Show($"Cannot remove default category '{selected}'.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                var result = MessageBox.Show($"Remove category '{selected}'?", "Confirm Remove", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    lstRecipeCategories.Items.Remove(selected);
                    
                    // Also remove from default category dropdown
                    var itemToRemove = cmbDefaultRecipeCategory.Items.Cast<string>()
                        .FirstOrDefault(c => c.Equals(selected, StringComparison.OrdinalIgnoreCase));
                    if (itemToRemove != null)
                    {
                        cmbDefaultRecipeCategory.Items.Remove(itemToRemove);
                    }
                }
            }
        }

        private void ResetToDefaults()
        {
            var result = MessageBox.Show(
                "Reset all settings to default values?\n\nThis will not affect your data.",
                "Reset Settings", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                LoadSettings();
                LoadUnits();
                MessageBox.Show("Settings reset to defaults.", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ApplySettings()
        {
            SaveSettings();
            MessageBox.Show("Settings applied successfully.", "Success", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveSettings()
        {
            try
            {
                // Save both currency code and symbol
                string currencyCode = cmbCurrencyCode.SelectedItem?.ToString() ?? "USD";
                string currencySymbol = currencyMap.ContainsKey(currencyCode) ? currencyMap[currencyCode] : "$";

                // Save to AppSettings
                AppSettings.CurrencySymbol = currencySymbol;

                // Save to database settings table for persistence
                DatabaseContext.SetSetting("CurrencyCode", currencyCode);
                DatabaseContext.SetSetting("CurrencySymbol", currencySymbol);
                DatabaseContext.SetSetting("ConfirmDeletes", chkConfirmDeletes.Checked.ToString());
                DatabaseContext.SetSetting("AutoCalculate", chkAutoCalculate.Checked.ToString());
                DatabaseContext.SetSetting("AutoConvertUnits", chkAutoConvertUnits.Checked.ToString());
                DatabaseContext.SetSetting("PreferredWeightUnit", cmbWeightUnit.SelectedItem?.ToString());
                DatabaseContext.SetSetting("PreferredVolumeUnit", cmbVolumeUnit.SelectedItem?.ToString());
                DatabaseContext.SetSetting("PreferredCountUnit", cmbCountUnit.SelectedItem?.ToString());
                DatabaseContext.SetSetting("DefaultFoodCost", cmbDefaultFoodCost.SelectedItem?.ToString());

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