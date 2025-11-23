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
        private CheckBox chkAutoConvertUnits;
        private ComboBox cmbDefaultFoodCost;

        // Unit Settings
        private ComboBox cmbWeightUnit;
        private ComboBox cmbVolumeUnit;
        private ComboBox cmbCountUnit;

        // Category Settings
        private ListBox lstIngredientCategories;
        private ListBox lstRecipeCategories;
        private TextBox txtNewIngredientCategory;
        private TextBox txtNewRecipeCategory;
        private Button btnAddIngredientCategory;
        private Button btnRemoveIngredientCategory;
        private Button btnAddRecipeCategory;
        private Button btnRemoveRecipeCategory;
        private ComboBox cmbDefaultIngredientCategory;
        private ComboBox cmbDefaultRecipeCategory;

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
            // Load units and categories first so combos have items
            LoadUnits();
            LoadCategories();
            LoadSettings();
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

            // TabControl
            tabControl.Dock = DockStyle.Top;
            tabControl.Height = 400;

            // Tabs
            var tabGeneral = new TabPage("General");
            var tabUnits = new TabPage("Units");
            var tabCategories = new TabPage("Categories");

            tabControl.TabPages.Add(tabGeneral);
            tabControl.TabPages.Add(tabUnits);
            tabControl.TabPages.Add(tabCategories);

            // ========== GENERAL TAB ==========
            InitializeGeneralTab(tabGeneral);

            // ========== UNITS TAB ==========
            InitializeUnitsTab(tabUnits);

            // ========== CATEGORIES TAB ==========
            InitializeCategoriesTab(tabCategories);

            // Buttons
            btnSave.Text = "Save";
            btnSave.Location = new System.Drawing.Point(350, 425);
            btnSave.Size = new System.Drawing.Size(75, 30);
            btnSave.Click += (s, e) => SaveSettings();

            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(435, 425);
            btnCancel.Size = new System.Drawing.Size(75, 30);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            btnApply.Text = "Apply";
            btnApply.Location = new System.Drawing.Point(520, 425);
            btnApply.Size = new System.Drawing.Size(75, 30);
            btnApply.Click += (s, e) => ApplySettings();

            // Add controls to form
            this.Controls.Add(tabControl);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.Controls.Add(btnApply);

            this.ResumeLayout(false);
        }

        private void InitializeGeneralTab(TabPage tab)
        {
            int yPos = 20;

            // Currency selection
            var lblCurrency = new Label
            {
                Text = "Currency:",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };
            cmbCurrencyCode = new ComboBox
            {
                Location = new System.Drawing.Point(150, yPos - 3),
                Size = new System.Drawing.Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCurrencyCode.Items.AddRange(new object[]
            {
                "USD", "EUR", "JPY", "GBP", "CNY", "CHF", "AUD", "CAD",
                "HKD", "SGD", "INR", "KRW", "SEK", "MXN", "NZD", "NOK",
                "TWD", "BRL", "ZAR", "PLN", "PHP"
            });
            cmbCurrencyCode.SelectedIndexChanged += (s, e) => UpdateCurrencySymbol();

            // Currency Symbol Display
            var lblSymbol = new Label
            {
                Text = "Symbol:",
                Location = new System.Drawing.Point(260, yPos),
                AutoSize = true
            };
            lblCurrencySymbol = new Label
            {
                Text = "$",
                Location = new System.Drawing.Point(310, yPos),
                Size = new System.Drawing.Size(50, 20),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            yPos += 35;

            // Decimal Places
            var lblDecimalPlaces = new Label
            {
                Text = "Decimal Places:",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };
            cmbDecimalPlaces = new ComboBox
            {
                Location = new System.Drawing.Point(150, yPos - 3),
                Size = new System.Drawing.Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDecimalPlaces.Items.AddRange(new object[]
            {
                "0 (Whole)",
                "1 (Tenth)",
                "2 (Money)",
                "3 (Thousandth)",
                "4",
                "5"
            });

            yPos += 35;

            // Boolean settings
            chkConfirmDeletes = new CheckBox
            {
                Text = "Confirm before deleting items",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };
            yPos += 25;

            chkAutoCalculate = new CheckBox
            {
                Text = "Automatically calculate costs on changes",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };
            yPos += 25;

            chkAutoConvertUnits = new CheckBox
            {
                Text = "Automatically convert units where possible",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };
            yPos += 35;

            // Default Food Cost %
            var lblDefaultFoodCost = new Label
            {
                Text = "Default Food Cost %:",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };
            cmbDefaultFoodCost = new ComboBox
            {
                Location = new System.Drawing.Point(150, yPos - 3),
                Size = new System.Drawing.Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDefaultFoodCost.Items.AddRange(new object[]
            {
                "25%", "30%", "35%", "40%", "45%", "50%"
            });

            tab.Controls.Add(lblCurrency);
            tab.Controls.Add(cmbCurrencyCode);
            tab.Controls.Add(lblSymbol);
            tab.Controls.Add(lblCurrencySymbol);
            tab.Controls.Add(lblDecimalPlaces);
            tab.Controls.Add(cmbDecimalPlaces);
            tab.Controls.Add(chkConfirmDeletes);
            tab.Controls.Add(chkAutoCalculate);
            tab.Controls.Add(chkAutoConvertUnits);
            tab.Controls.Add(lblDefaultFoodCost);
            tab.Controls.Add(cmbDefaultFoodCost);
        }

        private void InitializeUnitsTab(TabPage tab)
        {
            int yPos = 20;

            // Weight unit
            var lblWeightUnit = new Label
            {
                Text = "Default Weight Unit:",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };
            cmbWeightUnit = new ComboBox
            {
                Location = new System.Drawing.Point(180, yPos - 3),
                Size = new System.Drawing.Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            yPos += 35;

            // Volume unit
            var lblVolumeUnit = new Label
            {
                Text = "Default Volume Unit:",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };
            cmbVolumeUnit = new ComboBox
            {
                Location = new System.Drawing.Point(180, yPos - 3),
                Size = new System.Drawing.Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            yPos += 35;

            // Count unit
            var lblCountUnit = new Label
            {
                Text = "Default Count Unit:",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };
            cmbCountUnit = new ComboBox
            {
                Location = new System.Drawing.Point(180, yPos - 3),
                Size = new System.Drawing.Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            tab.Controls.Add(lblWeightUnit);
            tab.Controls.Add(cmbWeightUnit);
            tab.Controls.Add(lblVolumeUnit);
            tab.Controls.Add(cmbVolumeUnit);
            tab.Controls.Add(lblCountUnit);
            tab.Controls.Add(cmbCountUnit);
        }

        private void InitializeCategoriesTab(TabPage tab)
        {
            int yPos = 20;

            // Ingredient Categories Section
            var lblIngredientCategories = new Label
            {
                Text = "Ingredient Categories:",
                Location = new System.Drawing.Point(20, yPos),
                AutoSize = true
            };

            lstIngredientCategories = new ListBox
            {
                Location = new System.Drawing.Point(20, yPos + 25),
                Size = new System.Drawing.Size(200, 120)
            };

            txtNewIngredientCategory = new TextBox
            {
                Location = new System.Drawing.Point(20, yPos + 150),
                Size = new System.Drawing.Size(120, 20),
                PlaceholderText = "New category..."
            };

            btnAddIngredientCategory = new Button
            {
                Text = "Add",
                Location = new System.Drawing.Point(150, yPos + 148),
                Size = new System.Drawing.Size(70, 25)
            };
            btnAddIngredientCategory.Click += (s, e) => AddIngredientCategory();

            btnRemoveIngredientCategory = new Button
            {
                Text = "Remove",
                Location = new System.Drawing.Point(20, yPos + 180),
                Size = new System.Drawing.Size(200, 25)
            };
            btnRemoveIngredientCategory.Click += (s, e) => RemoveIngredientCategory();

            // Recipe Categories Section
            var lblRecipeCategories = new Label
            {
                Text = "Recipe Categories:",
                Location = new System.Drawing.Point(300, yPos),
                AutoSize = true
            };

            lstRecipeCategories = new ListBox
            {
                Location = new System.Drawing.Point(300, yPos + 25),
                Size = new System.Drawing.Size(200, 120)
            };

            txtNewRecipeCategory = new TextBox
            {
                Location = new System.Drawing.Point(300, yPos + 150),
                Size = new System.Drawing.Size(120, 20),
                PlaceholderText = "New category..."
            };

            btnAddRecipeCategory = new Button
            {
                Text = "Add",
                Location = new System.Drawing.Point(430, yPos + 148),
                Size = new System.Drawing.Size(70, 25)
            };
            btnAddRecipeCategory.Click += (s, e) => AddRecipeCategory();

            btnRemoveRecipeCategory = new Button
            {
                Text = "Remove",
                Location = new System.Drawing.Point(300, yPos + 180),
                Size = new System.Drawing.Size(200, 25)
            };
            btnRemoveRecipeCategory.Click += (s, e) => RemoveRecipeCategory();

            // Default Ingredient Category
            var lblDefaultIngredientCategory = new Label
            {
                Text = "Default Ingredient Category:",
                Location = new System.Drawing.Point(20, yPos + 220),
                AutoSize = true
            };
            cmbDefaultIngredientCategory = new ComboBox
            {
                Location = new System.Drawing.Point(20, yPos + 245),
                Size = new System.Drawing.Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Default Recipe Category
            var lblDefaultRecipeCategory = new Label
            {
                Text = "Default Recipe Category:",
                Location = new System.Drawing.Point(300, yPos + 220),
                AutoSize = true
            };
            cmbDefaultRecipeCategory = new ComboBox
            {
                Location = new System.Drawing.Point(300, yPos + 245),
                Size = new System.Drawing.Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            tab.Controls.Add(lblIngredientCategories);
            tab.Controls.Add(lstIngredientCategories);
            tab.Controls.Add(txtNewIngredientCategory);
            tab.Controls.Add(btnAddIngredientCategory);
            tab.Controls.Add(btnRemoveIngredientCategory);

            tab.Controls.Add(lblRecipeCategories);
            tab.Controls.Add(lstRecipeCategories);
            tab.Controls.Add(txtNewRecipeCategory);
            tab.Controls.Add(btnAddRecipeCategory);
            tab.Controls.Add(btnRemoveRecipeCategory);

            tab.Controls.Add(lblDefaultIngredientCategory);
            tab.Controls.Add(cmbDefaultIngredientCategory);
            tab.Controls.Add(lblDefaultRecipeCategory);
            tab.Controls.Add(cmbDefaultRecipeCategory);
        }

        private void UpdateCurrencySymbol()
        {
            if (cmbCurrencyCode.SelectedItem != null)
            {
                string currencyCode = cmbCurrencyCode.SelectedItem.ToString();
                if (currencyMap.TryGetValue(currencyCode, out var symbol))
                {
                    lblCurrencySymbol.Text = symbol;
                }
                else
                {
                    lblCurrencySymbol.Text = "$";
                }
            }
        }

        private void LoadSettings()
        {
            try
            {
                var settings = DatabaseContext.GetAllSettings();

                // --- Currency code + symbol ---
                string currencyCode;
                if (!settings.TryGetValue("CurrencyCode", out currencyCode) ||
                    string.IsNullOrWhiteSpace(currencyCode))
                {
                    currencyCode = "USD"; // default
                }

                if (cmbCurrencyCode.Items.Contains(currencyCode))
                {
                    cmbCurrencyCode.SelectedItem = currencyCode;
                }
                else
                {
                    cmbCurrencyCode.SelectedItem = "USD";
                    currencyCode = "USD";
                }

                // Update label + AppSettings
                UpdateCurrencySymbol();
                if (currencyMap.TryGetValue(currencyCode, out var symbol))
                {
                    AppSettings.CurrencySymbol = symbol;
                }

                // --- Decimal places (store as selected index) ---
                int decimalIndex = 2; // default "2 (Money)"
                if (settings.TryGetValue("DecimalPlaces", out var decimalIndexRaw) &&
                    int.TryParse(decimalIndexRaw, out var idx) &&
                    idx >= 0 && idx < cmbDecimalPlaces.Items.Count)
                {
                    decimalIndex = idx;
                }
                cmbDecimalPlaces.SelectedIndex = decimalIndex;

                // --- Booleans ---
                chkConfirmDeletes.Checked = GetBoolSetting(settings, "ConfirmDeletes", true);
                chkAutoCalculate.Checked = GetBoolSetting(settings, "AutoCalculate", true);
                chkAutoConvertUnits.Checked = GetBoolSetting(settings, "AutoConvertUnits", true);

                // --- Units ---
                SetComboFromSetting(cmbWeightUnit, settings, "PreferredWeightUnit");
                SetComboFromSetting(cmbVolumeUnit, settings, "PreferredVolumeUnit");
                SetComboFromSetting(cmbCountUnit, settings, "PreferredCountUnit");

                // --- Default food cost ---
                string defaultFoodCost = "30%";
                if (settings.TryGetValue("DefaultFoodCost", out var foodCostRaw) &&
                    !string.IsNullOrWhiteSpace(foodCostRaw))
                {
                    defaultFoodCost = foodCostRaw;
                }

                if (cmbDefaultFoodCost.Items.Contains(defaultFoodCost))
                    cmbDefaultFoodCost.SelectedItem = defaultFoodCost;
                else
                    cmbDefaultFoodCost.SelectedItem = "30%";

                // --- Default categories (after LoadCategories has filled the combos) ---
                if (settings.TryGetValue("DefaultIngredientCategory", out var defIngCat) &&
                    !string.IsNullOrWhiteSpace(defIngCat) &&
                    cmbDefaultIngredientCategory.Items.Contains(defIngCat))
                {
                    cmbDefaultIngredientCategory.SelectedItem = defIngCat;
                }

                if (settings.TryGetValue("DefaultRecipeCategory", out var defRecCat) &&
                    !string.IsNullOrWhiteSpace(defRecCat) &&
                    cmbDefaultRecipeCategory.Items.Contains(defRecCat))
                {
                    cmbDefaultRecipeCategory.SelectedItem = defRecCat;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool GetBoolSetting(Dictionary<string, string> settings, string key, bool defaultValue)
        {
            if (settings.TryGetValue(key, out var raw) && bool.TryParse(raw, out var value))
            {
                return value;
            }
            return defaultValue;
        }

        private void SetComboFromSetting(ComboBox combo, Dictionary<string, string> settings, string key)
        {
            if (combo == null) return;

            if (settings.TryGetValue(key, out var value) &&
                !string.IsNullOrWhiteSpace(value) &&
                combo.Items.Contains(value))
            {
                combo.SelectedItem = value;
            }
        }

        private List<string> GetDefaultIngredientCategories()
        {
            return new List<string> {
                "Bakery", "Beverages", "Canned & Packaged", "Dairy & Eggs",
                "Frozen Goods", "Fruit", "Grains", "Herbs", "Ingredients Base",
                "Meat & Poultry", "Oils & Fats", "Pasta", "Sauces & Condiments",
                "Seafood", "Spices", "Vegetables"
            };
        }

        private List<string> GetDefaultRecipeCategories()
        {
            return new List<string> {
                "Appetizer", "Soup & Salad", "Entrée", "Side Dish",
                "Dessert", "Baking", "Breakfast/Brunch", "Beverages", "Hot Drink"
            };
        }

        private void LoadCategories()
        {
            try
            {
                var defaultIngredientCategories = GetDefaultIngredientCategories();
                var defaultRecipeCategories = GetDefaultRecipeCategories();

                // Categories already in use in the DB
                var existingIngredientCategories = DatabaseContext.GetIngredientCategories();
                var existingRecipeCategories = DatabaseContext.GetRecipeCategories();

                // Custom categories from the settings table
                var settings = DatabaseContext.GetAllSettings();

                var customIngredientCategories = new List<string>();
                if (settings.TryGetValue("CustomIngredientCategories", out var ingRaw) &&
                    !string.IsNullOrWhiteSpace(ingRaw))
                {
                    customIngredientCategories = ingRaw
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrEmpty(c))
                        .ToList();
                }

                var customRecipeCategories = new List<string>();
                if (settings.TryGetValue("CustomRecipeCategories", out var recRaw) &&
                    !string.IsNullOrWhiteSpace(recRaw))
                {
                    customRecipeCategories = recRaw
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrEmpty(c))
                        .ToList();
                }

                // Final sorted, case-insensitive sets
                var ingredientCategoriesSet = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in defaultIngredientCategories) ingredientCategoriesSet.Add(c);
                foreach (var c in existingIngredientCategories ?? Enumerable.Empty<string>())
                    if (!string.IsNullOrWhiteSpace(c)) ingredientCategoriesSet.Add(c);
                foreach (var c in customIngredientCategories) ingredientCategoriesSet.Add(c);

                var recipeCategoriesSet = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in defaultRecipeCategories) recipeCategoriesSet.Add(c);
                foreach (var c in existingRecipeCategories ?? Enumerable.Empty<string>())
                    if (!string.IsNullOrWhiteSpace(c)) recipeCategoriesSet.Add(c);
                foreach (var c in customRecipeCategories) recipeCategoriesSet.Add(c);

                // Ingredient categories listbox
                lstIngredientCategories.Items.Clear();
                foreach (var c in ingredientCategoriesSet)
                    lstIngredientCategories.Items.Add(c);

                // Recipe categories listbox
                lstRecipeCategories.Items.Clear();
                foreach (var c in recipeCategoriesSet)
                    lstRecipeCategories.Items.Add(c);

                // Default category dropdowns
                cmbDefaultIngredientCategory.Items.Clear();
                cmbDefaultIngredientCategory.Items.Add(""); // empty option
                foreach (var c in ingredientCategoriesSet)
                    cmbDefaultIngredientCategory.Items.Add(c);

                cmbDefaultRecipeCategory.Items.Clear();
                cmbDefaultRecipeCategory.Items.Add(""); // empty option
                foreach (var c in recipeCategoriesSet)
                    cmbDefaultRecipeCategory.Items.Add(c);
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
                cmbWeightUnit.Items.AddRange(new object[] { "g", "kg", "oz", "lb" });
                cmbWeightUnit.SelectedIndex = 1; // Default to kg

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
                    MessageBox.Show($"Category '{newCategory}' already exists.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RemoveIngredientCategory()
        {
            if (lstIngredientCategories.SelectedItem != null)
            {
                var selected = lstIngredientCategories.SelectedItem.ToString();

                // Default ingredient categories
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

                    // Also remove from default dropdown if present
                    if (cmbDefaultIngredientCategory.Items.Contains(selected))
                    {
                        cmbDefaultIngredientCategory.Items.Remove(selected);
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
                    MessageBox.Show($"Category '{newCategory}' already exists.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RemoveRecipeCategory()
        {
            if (lstRecipeCategories.SelectedItem != null)
            {
                var selected = lstRecipeCategories.SelectedItem.ToString();

                // Default recipe categories
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

                    // Also remove from default dropdown if present
                    if (cmbDefaultRecipeCategory.Items.Contains(selected))
                    {
                        cmbDefaultRecipeCategory.Items.Remove(selected);
                    }
                }
            }
        }

        private void ApplySettings()
        {
            SaveSettings();
            MessageBox.Show("Settings applied successfully.", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveCategoriesToSettings()
        {
            try
            {
                var defaultIngredientCategories = GetDefaultIngredientCategories();
                var defaultRecipeCategories = GetDefaultRecipeCategories();

                // Only store non-default categories as "custom"
                var customIngredientCategories = lstIngredientCategories.Items.Cast<string>()
                    .Where(c => !defaultIngredientCategories.Contains(c, StringComparer.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c)
                    .ToList();

                var customRecipeCategories = lstRecipeCategories.Items.Cast<string>()
                    .Where(c => !defaultRecipeCategories.Contains(c, StringComparer.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c)
                    .ToList();

                DatabaseContext.SetSetting("CustomIngredientCategories",
                    string.Join(";", customIngredientCategories));
                DatabaseContext.SetSetting("CustomRecipeCategories",
                    string.Join(";", customRecipeCategories));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving categories: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                // --- Currency ---
                string currencyCode = cmbCurrencyCode.SelectedItem?.ToString() ?? "USD";
                string currencySymbol = currencyMap.ContainsKey(currencyCode) ? currencyMap[currencyCode] : "$";

                // Update AppSettings for immediate use
                AppSettings.CurrencySymbol = currencySymbol;

                // Persist currency in DB
                DatabaseContext.SetSetting("CurrencyCode", currencyCode);
                DatabaseContext.SetSetting("CurrencySymbol", currencySymbol);

                // --- Decimal places (store selected index) ---
                DatabaseContext.SetSetting("DecimalPlaces", cmbDecimalPlaces.SelectedIndex.ToString());

                // --- Booleans ---
                DatabaseContext.SetSetting("ConfirmDeletes", chkConfirmDeletes.Checked.ToString());
                DatabaseContext.SetSetting("AutoCalculate", chkAutoCalculate.Checked.ToString());
                DatabaseContext.SetSetting("AutoConvertUnits", chkAutoConvertUnits.Checked.ToString());

                // --- Units ---
                DatabaseContext.SetSetting("PreferredWeightUnit", cmbWeightUnit.SelectedItem?.ToString());
                DatabaseContext.SetSetting("PreferredVolumeUnit", cmbVolumeUnit.SelectedItem?.ToString());
                DatabaseContext.SetSetting("PreferredCountUnit", cmbCountUnit.SelectedItem?.ToString());

                // --- Default values ---
                DatabaseContext.SetSetting("DefaultFoodCost", cmbDefaultFoodCost.SelectedItem?.ToString());
                DatabaseContext.SetSetting("DefaultIngredientCategory", cmbDefaultIngredientCategory.Text ?? string.Empty);
                DatabaseContext.SetSetting("DefaultRecipeCategory", cmbDefaultRecipeCategory.Text ?? string.Empty);

                // --- Custom categories ---
                SaveCategoriesToSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
