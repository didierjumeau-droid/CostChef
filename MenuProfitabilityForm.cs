using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public class MenuProfitabilityForm : Form
    {
        private DataGridView dataGridViewMenu;
        private ComboBox cboCategory;
        private TextBox txtSearch;
        private CheckBox chkAboveTargetOnly;
        private Button btnRefresh;
        private Button btnClose;
        private Label lblSummary;

        private readonly BindingSource _binding = new BindingSource();
        private List<MenuProfitabilityRow> _allRows = new List<MenuProfitabilityRow>();

        public MenuProfitabilityForm()
        {
            InitializeComponent();
            LoadMenuData();
        }

        private void InitializeComponent()
        {
            this.dataGridViewMenu = new DataGridView();
            this.cboCategory = new ComboBox();
            this.txtSearch = new TextBox();
            this.chkAboveTargetOnly = new CheckBox();
            this.btnRefresh = new Button();
            this.btnClose = new Button();
            this.lblSummary = new Label();

            this.SuspendLayout();

            // Form
            this.Text = "Menu Profitability Dashboard";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(1000, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Category filter
            this.cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboCategory.Location = new Point(20, 15);
            this.cboCategory.Size = new Size(180, 24);
            this.cboCategory.Items.Add("All Categories");
            this.cboCategory.SelectedIndex = 0;
            this.cboCategory.SelectedIndexChanged += (s, e) => ApplyFilters();

            // Search box
            this.txtSearch.Location = new Point(220, 15);
            this.txtSearch.Size = new Size(220, 24);
            this.txtSearch.PlaceholderText = "Search recipe name...";
            this.txtSearch.TextChanged += (s, e) => ApplyFilters();

            // Above target only
            this.chkAboveTargetOnly.Text = "Only above target";
            this.chkAboveTargetOnly.AutoSize = true;
            this.chkAboveTargetOnly.Location = new Point(460, 17);
            this.chkAboveTargetOnly.CheckedChanged += (s, e) => ApplyFilters();

            // Refresh button
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Location = new Point(620, 12);
            this.btnRefresh.Size = new Size(90, 28);
            this.btnRefresh.Click += (s, e) => LoadMenuData();

            // Close button
            this.btnClose.Text = "Close";
            this.btnClose.Location = new Point(880, 12);
            this.btnClose.Size = new Size(90, 28);
            this.btnClose.Click += (s, e) => this.Close();

            // DataGridView
            this.dataGridViewMenu.Location = new Point(20, 50);
            this.dataGridViewMenu.Size = new Size(950, 480);
            this.dataGridViewMenu.ReadOnly = true;
            this.dataGridViewMenu.AllowUserToAddRows = false;
            this.dataGridViewMenu.AllowUserToDeleteRows = false;
            this.dataGridViewMenu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewMenu.MultiSelect = false;
            this.dataGridViewMenu.AutoGenerateColumns = false;
            this.dataGridViewMenu.RowHeadersVisible = false;
            this.dataGridViewMenu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewMenu.CellDoubleClick += DataGridViewMenu_CellDoubleClick;

            ConfigureGridColumns();

            // Summary label
            this.lblSummary.Location = new Point(20, 540);
            this.lblSummary.AutoSize = true;
            this.lblSummary.Text = "Items: 0 | Average Margin: 0% | Average Food Cost: 0%";

            // Add controls
            this.Controls.Add(this.dataGridViewMenu);
            this.Controls.Add(this.cboCategory);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.chkAboveTargetOnly);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblSummary);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ConfigureGridColumns()
        {
            string currencySymbol = AppSettings.CurrencySymbol ?? "$";
            var currency = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            currency.CurrencySymbol = currencySymbol;

            dataGridViewMenu.Columns.Clear();

            dataGridViewMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RecipeName",
                HeaderText = "Recipe",
                DataPropertyName = "RecipeName",
                ReadOnly = true,
                FillWeight = 150
            });

            dataGridViewMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Category",
                HeaderText = "Category",
                DataPropertyName = "Category",
                ReadOnly = true,
                FillWeight = 100
            });

            dataGridViewMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CostPerServing",
                HeaderText = $"Cost / Serving ({currencySymbol})",
                DataPropertyName = "CostPerServing",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    FormatProvider = currency,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                },
                FillWeight = 100
            });

            dataGridViewMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SalesPrice",
                HeaderText = $"Price / Serving ({currencySymbol})",
                DataPropertyName = "SalesPrice",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    FormatProvider = currency,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                },
                FillWeight = 100
            });

            dataGridViewMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FoodCostPercentage",
                HeaderText = "Food Cost %",
                DataPropertyName = "FoodCostPercentage",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "P1", // 0.35 -> 35.0 %
                    Alignment = DataGridViewContentAlignment.MiddleRight
                },
                FillWeight = 80
            });

            dataGridViewMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TargetFoodCostPercentage",
                HeaderText = "Target FC %",
                DataPropertyName = "TargetFoodCostPercentage",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "P1",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                },
                FillWeight = 80
            });

            dataGridViewMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "VarianceFoodCostPercentage",
                HeaderText = "Variance FC %",
                DataPropertyName = "VarianceFoodCostPercentage",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "P1",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                },
                FillWeight = 80
            });

            dataGridViewMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "GrossProfitPerServing",
                HeaderText = $"Gross Profit / Serving ({currencySymbol})",
                DataPropertyName = "GrossProfitPerServing",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    FormatProvider = currency,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                },
                FillWeight = 120
            });

            dataGridViewMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MarginPercentage",
                HeaderText = "Margin %",
                DataPropertyName = "MarginPercentage",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "P1",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                },
                FillWeight = 80
            });

            dataGridViewMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                DataPropertyName = "Status",
                ReadOnly = true,
                FillWeight = 90
            });
        }

        private void LoadMenuData()
        {
            _allRows = DatabaseContext.GetMenuProfitabilityRows();
            PopulateCategoryFilter();
            ApplyFilters();
        }

        private void PopulateCategoryFilter()
        {
            var categories = _allRows
                .Select(r => r.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            cboCategory.Items.Clear();
            cboCategory.Items.Add("All Categories");
            foreach (var c in categories)
                cboCategory.Items.Add(c);

            cboCategory.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            IEnumerable<MenuProfitabilityRow> filtered = _allRows;

            // Category filter
            if (cboCategory.SelectedIndex > 0)
            {
                string selectedCategory = cboCategory.SelectedItem?.ToString() ?? "";
                filtered = filtered.Where(r =>
                    string.Equals(r.Category, selectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            // Search by recipe name
            string search = txtSearch.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(r =>
                    r.RecipeName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Only above target
            if (chkAboveTargetOnly.Checked)
            {
                filtered = filtered.Where(r =>
                    r.SalesPrice > 0m &&
                    r.TotalCost > 0m &&
                    r.FoodCostPercentage > r.TargetFoodCostPercentage + 0.02m);
            }

            var list = filtered.ToList();
            _binding.DataSource = list;
            dataGridViewMenu.DataSource = _binding;

            ApplyRowColors(list);
            UpdateSummary(list);
        }

        private void ApplyRowColors(List<MenuProfitabilityRow> rows)
        {
            const decimal varianceTolerance = 0.02m; // ≈ 2 percentage points
            const decimal marginHigh = 0.70m;
            const decimal marginLow = 0.60m;

            for (int i = 0; i < dataGridViewMenu.Rows.Count; i++)
            {
                var gridRow = dataGridViewMenu.Rows[i];
                var item = gridRow.DataBoundItem as MenuProfitabilityRow;
                if (item == null)
                    continue;

                // Default
                gridRow.DefaultCellStyle.BackColor = Color.White;

                if (item.SalesPrice <= 0m || item.TotalCost <= 0m)
                {
                    // Grey out incomplete data (no price or no cost)
                    gridRow.DefaultCellStyle.BackColor = Color.LightGray;
                    continue;
                }

                bool aboveTarget = item.FoodCostPercentage > item.TargetFoodCostPercentage + varianceTolerance;
                bool belowTarget = item.FoodCostPercentage < item.TargetFoodCostPercentage - varianceTolerance;

                if (aboveTarget)
                {
                    gridRow.DefaultCellStyle.BackColor = Color.LightCoral; // too expensive to make
                }
                else if (belowTarget)
                {
                    gridRow.DefaultCellStyle.BackColor = Color.LightGreen; // good performer
                }
                else
                {
                    // within target: check margin for nuance
                    if (item.MarginPercentage < marginLow)
                        gridRow.DefaultCellStyle.BackColor = Color.MistyRose;
                    else if (item.MarginPercentage > marginHigh)
                        gridRow.DefaultCellStyle.BackColor = Color.LightGreen;
                    else
                        gridRow.DefaultCellStyle.BackColor = Color.White;
                }
            }
        }

        private void UpdateSummary(List<MenuProfitabilityRow> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                lblSummary.Text = "Items: 0 | Average Margin: 0% | Average Food Cost: 0%";
                return;
            }

            var withPrice = rows.Where(r => r.SalesPrice > 0m && r.TotalCost > 0m).ToList();
            if (withPrice.Count == 0)
            {
                lblSummary.Text = $"Items: {rows.Count} | Average Margin: N/A | Average Food Cost: N/A";
                return;
            }

            decimal avgMargin = withPrice.Average(r => r.MarginPercentage);
            decimal avgFoodCost = withPrice.Average(r => r.FoodCostPercentage);

            lblSummary.Text =
                $"Items: {rows.Count} | Average Margin: {avgMargin:P1} | Average Food Cost: {avgFoodCost:P1}";
        }

        private void DataGridViewMenu_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            // For now, just open the RecipesForm.
            // (Later we can navigate directly to the selected recipe if needed.)
            using (var f = new RecipesForm())
            {
                f.ShowDialog();
            }
        }
    }
}
