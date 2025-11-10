//[file name]: SupplierReportsForm.cs
// [file content begin]
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public partial class SupplierReportsForm : Form
    {
        private List<Supplier> suppliers;
        private List<Ingredient> ingredients;

        // New analytics properties
        private Panel analyticsPanel;
        private Label summaryLabel;
        private Label insightsLabel;

        public SupplierReportsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Supplier Analytics Dashboard";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Main layout panel
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            this.Controls.Add(mainPanel);

            // Title
            var titleLabel = new Label
            {
                Text = "📊 Supplier Analytics Dashboard",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10),
                ForeColor = Color.DarkBlue
            };
            mainPanel.Controls.Add(titleLabel);

            // Analytics Summary Panel
            analyticsPanel = new Panel
            {
                Location = new Point(10, 45),
                Size = new Size(860, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.AliceBlue
            };
            mainPanel.Controls.Add(analyticsPanel);

            // Summary label
            summaryLabel = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(840, 25),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            analyticsPanel.Controls.Add(summaryLabel);

            // Insights label
            insightsLabel = new Label
            {
                Location = new Point(10, 35),
                Size = new Size(840, 35),
                Font = new Font("Arial", 9, FontStyle.Italic),
                ForeColor = Color.DarkGreen
            };
            analyticsPanel.Controls.Add(insightsLabel);

            // Controls panel
            var controlsPanel = new Panel
            {
                Location = new Point(10, 140),
                Size = new Size(860, 40),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(controlsPanel);

            // Supplier selection
            var supplierLabel = new Label
            {
                Text = "Select Supplier:",
                Location = new Point(10, 10),
                AutoSize = true
            };
            controlsPanel.Controls.Add(supplierLabel);

            var supplierComboBox = new ComboBox
            {
                Location = new Point(120, 7),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            supplierComboBox.SelectedIndexChanged += (s, e) => UpdateReport();
            controlsPanel.Controls.Add(supplierComboBox);

            // Report type selection
            var reportTypeLabel = new Label
            {
                Text = "Report Type:",
                Location = new Point(350, 10),
                AutoSize = true
            };
            controlsPanel.Controls.Add(reportTypeLabel);

            var reportTypeComboBox = new ComboBox
            {
                Location = new Point(450, 7),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            reportTypeComboBox.Items.AddRange(new[] { "All Suppliers", "Selected Supplier" });
            reportTypeComboBox.SelectedIndex = 0;
            reportTypeComboBox.SelectedIndexChanged += (s, e) => UpdateReport();
            controlsPanel.Controls.Add(reportTypeComboBox);

            // DataGridView for reports
            var dataGridView = new DataGridView
            {
                Location = new Point(10, 195),
                Size = new Size(860, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            mainPanel.Controls.Add(dataGridView);

            // Close button
            var closeButton = new Button
            {
                Text = "Close",
                Location = new Point(780, 610),
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            closeButton.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(closeButton);

            // Initialize controls
            this.suppliersComboBox = supplierComboBox;
            this.reportTypeComboBox = reportTypeComboBox;
            this.reportsDataGridView = dataGridView;
        }

        private ComboBox suppliersComboBox;
        private ComboBox reportTypeComboBox;
        private DataGridView reportsDataGridView;

        private void LoadData()
        {
            try
            {
                suppliers = DatabaseContext.GetAllSuppliers() ?? new List<Supplier>();
                ingredients = DatabaseContext.GetAllIngredients() ?? new List<Ingredient>();
                
                LoadSuppliers();
                SetupDataGridView();
                UpdateReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                suppliers = new List<Supplier>();
                ingredients = new List<Ingredient>();
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                suppliersComboBox.Items.Clear();
                
                if (suppliers != null && suppliers.Any())
                {
                    suppliersComboBox.Items.AddRange(suppliers.Select(s => s.Name).ToArray());
                    if (suppliersComboBox.Items.Count > 0)
                        suppliersComboBox.SelectedIndex = 0;
                }
                else
                {
                    suppliersComboBox.Items.Add("No suppliers available");
                    suppliersComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers list: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGridView()
        {
            reportsDataGridView.Columns.Clear();
        }

        private void UpdateReport()
        {
            try
            {
                if (reportTypeComboBox.SelectedItem?.ToString() == "All Suppliers")
                {
                    LoadAllSuppliersReport();
                }
                else
                {
                    LoadSupplierReport();
                }
                UpdateAnalyticsPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating report: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAllSuppliersReport()
        {
            try
            {
                reportsDataGridView.Columns.Clear();
                
                // Enhanced columns with analytics
                reportsDataGridView.Columns.AddRange(
                    new DataGridViewTextBoxColumn { HeaderText = "Supplier", DataPropertyName = "SupplierName", FillWeight = 20 },
                    new DataGridViewTextBoxColumn { HeaderText = "Ingredients", DataPropertyName = "IngredientCount", FillWeight = 10 },
                    new DataGridViewTextBoxColumn { HeaderText = "Total Value", DataPropertyName = "TotalValue", FillWeight = 15 },
                    new DataGridViewTextBoxColumn { HeaderText = "Avg Price", DataPropertyName = "AveragePrice", FillWeight = 12 },
                    new DataGridViewTextBoxColumn { HeaderText = "Performance", DataPropertyName = "Performance", FillWeight = 15 },
                    new DataGridViewTextBoxColumn { HeaderText = "Risk Level", DataPropertyName = "RiskLevel", FillWeight = 12 },
                    new DataGridViewTextBoxColumn { HeaderText = "Contact", DataPropertyName = "Contact", FillWeight = 16 }
                );

                var reportData = new List<SupplierAnalytics>();
                
                if (suppliers != null)
                {
                    foreach (var supplier in suppliers)
                    {
                        var supplierIngredients = ingredients?.Where(i => i.SupplierId == supplier.Id).ToList() ?? new List<Ingredient>();
                        var analytics = CreateSupplierAnalytics(supplier, supplierIngredients);
                        reportData.Add(analytics);
                    }
                }

                reportsDataGridView.DataSource = reportData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading all suppliers report: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSupplierReport()
        {
            try
            {
                if (suppliersComboBox.SelectedItem == null) return;

                var selectedSupplierName = suppliersComboBox.SelectedItem.ToString();
                var supplier = suppliers?.FirstOrDefault(s => s.Name == selectedSupplierName);
                if (supplier == null) return;

                reportsDataGridView.Columns.Clear();
                
                // Enhanced detailed view
                reportsDataGridView.Columns.AddRange(
                    new DataGridViewTextBoxColumn { HeaderText = "Ingredient", DataPropertyName = "Name", FillWeight = 25 },
                    new DataGridViewTextBoxColumn { HeaderText = "Unit", DataPropertyName = "Unit", FillWeight = 10 },
                    new DataGridViewTextBoxColumn { HeaderText = "Unit Price", DataPropertyName = "UnitPriceFormatted", FillWeight = 15 },
                    new DataGridViewTextBoxColumn { HeaderText = "Category", DataPropertyName = "Category", FillWeight = 20 },
                    new DataGridViewTextBoxColumn { HeaderText = "Value Contribution", DataPropertyName = "ValueContribution", FillWeight = 15 },
                    new DataGridViewTextBoxColumn { HeaderText = "Price Trend", DataPropertyName = "PriceTrend", FillWeight = 15 }
                );

                var supplierIngredients = ingredients?.Where(i => i.SupplierId == supplier.Id).ToList() ?? new List<Ingredient>();
                var totalValue = supplierIngredients.Sum(i => i.UnitPrice);
                
                var detailedData = supplierIngredients.Select(i => new 
                {
                    i.Name,
                    i.Unit,
                    UnitPriceFormatted = $"{AppSettings.CurrencySymbol}{i.UnitPrice:F2}",
                    i.Category,
                    ValueContribution = totalValue > 0 ? $"{((i.UnitPrice / totalValue) * 100):F1}%" : "0%",
                    PriceTrend = GetPriceTrendIndicator(i.UnitPrice)
                }).ToList();

                reportsDataGridView.DataSource = detailedData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading supplier report: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private SupplierAnalytics CreateSupplierAnalytics(Supplier supplier, List<Ingredient> supplierIngredients)
        {
            var ingredientCount = supplierIngredients.Count;
            var totalValue = supplierIngredients.Sum(i => i.UnitPrice);
            var averagePrice = ingredientCount > 0 ? totalValue / ingredientCount : 0;

            return new SupplierAnalytics
            {
                SupplierName = supplier.Name ?? "Unknown",
                IngredientCount = ingredientCount,
                TotalValue = $"{AppSettings.CurrencySymbol}{totalValue:F2}",
                AveragePrice = $"{AppSettings.CurrencySymbol}{averagePrice:F2}",
                Performance = CalculatePerformance(ingredientCount, totalValue),
                RiskLevel = CalculateRiskLevel(ingredientCount, totalValue),
                Contact = supplier.ContactPerson ?? "N/A"
            };
        }

        private string CalculatePerformance(int ingredientCount, decimal totalValue)
        {
            if (ingredientCount == 0) return "No Data";
            
            var score = (ingredientCount * 0.4) + ((double)totalValue / 100 * 0.6);
            
            if (score >= 8) return "⭐️⭐️⭐️⭐️⭐️";
            if (score >= 6) return "⭐️⭐️⭐️⭐️";
            if (score >= 4) return "⭐️⭐️⭐️";
            if (score >= 2) return "⭐️⭐️";
            return "⭐️";
        }

        private string CalculateRiskLevel(int ingredientCount, decimal totalValue)
        {
            if (ingredientCount == 0) return "No Data";
            if (ingredientCount <= 2 && totalValue > 100) return "🔴 High";
            if (ingredientCount <= 3) return "🟡 Medium";
            return "🟢 Low";
        }

        private string GetPriceTrendIndicator(decimal price)
        {
            if (price > 50) return "💰 High";
            if (price > 20) return "💲 Medium";
            return "💵 Low";
        }

        private void UpdateAnalyticsPanel()
        {
            try
            {
                if (reportTypeComboBox.SelectedItem?.ToString() == "All Suppliers")
                {
                    UpdateAllSuppliersAnalytics();
                }
                else
                {
                    UpdateSingleSupplierAnalytics();
                }
            }
            catch (Exception ex)
            {
                summaryLabel.Text = "Error loading analytics";
                insightsLabel.Text = ex.Message;
            }
        }

        private void UpdateAllSuppliersAnalytics()
        {
            var totalSuppliers = suppliers?.Count ?? 0;
            var totalIngredients = suppliers?.Sum(s => 
                ingredients?.Count(i => i.SupplierId == s.Id) ?? 0) ?? 0;
            var totalValue = suppliers?.Sum(s => 
                ingredients?.Where(i => i.SupplierId == s.Id).Sum(i => i.UnitPrice) ?? 0) ?? 0;

            var highPerformers = suppliers?.Count(s => {
                var supplierIngredients = ingredients?.Where(i => i.SupplierId == s.Id).ToList() ?? new List<Ingredient>();
                var performance = CalculatePerformance(supplierIngredients.Count, supplierIngredients.Sum(i => i.UnitPrice));
                return performance.Contains("⭐️⭐️⭐️⭐️") || performance.Contains("⭐️⭐️⭐️⭐️⭐️");
            }) ?? 0;

            summaryLabel.Text = $"📊 {totalSuppliers} Suppliers | 🛒 {totalIngredients} Ingredients | 💰 {AppSettings.CurrencySymbol}{totalValue:F2} Total Value | 🏆 {highPerformers} High Performers";

            // Generate insights
            var insights = new List<string>();
            if (totalSuppliers < 3) insights.Add("Consider diversifying supplier base");
            if (totalIngredients / (double)totalSuppliers < 3) insights.Add("Consolidate suppliers for efficiency");
            if (highPerformers >= totalSuppliers * 0.7) insights.Add("Strong supplier network");
            else insights.Add("Opportunity to improve supplier performance");

            insightsLabel.Text = "💡 " + string.Join(" • ", insights);
        }

        private void UpdateSingleSupplierAnalytics()
        {
            if (suppliersComboBox.SelectedItem == null) return;

            var selectedSupplierName = suppliersComboBox.SelectedItem.ToString();
            var supplier = suppliers?.FirstOrDefault(s => s.Name == selectedSupplierName);
            if (supplier == null) return;

            var supplierIngredients = ingredients?.Where(i => i.SupplierId == supplier.Id).ToList() ?? new List<Ingredient>();
            var ingredientCount = supplierIngredients.Count;
            var totalValue = supplierIngredients.Sum(i => i.UnitPrice);
            var averagePrice = ingredientCount > 0 ? totalValue / ingredientCount : 0;

            summaryLabel.Text = $"{supplier.Name} Analytics: {ingredientCount} Ingredients | {AppSettings.CurrencySymbol}{totalValue:F2} Total Value | {AppSettings.CurrencySymbol}{averagePrice:F2} Avg Price";

            // Generate insights for single supplier
            var insights = new List<string>();
            if (ingredientCount >= 10) insights.Add("Diverse ingredient portfolio");
            else if (ingredientCount >= 5) insights.Add("Moderate product range");
            else insights.Add("Limited product selection");

            if (averagePrice > 50) insights.Add("High-value ingredient specialist");
            else if (averagePrice > 20) insights.Add("Balanced value mix");
            else insights.Add("Cost-effective supplier");

            if (ingredientCount <= 2 && totalValue > 100) insights.Add("⚠️ High dependency risk");

            insightsLabel.Text = "💡 " + string.Join(" • ", insights);
        }
    }

    // Analytics data class
    public class SupplierAnalytics
    {
        public string SupplierName { get; set; }
        public int IngredientCount { get; set; }
        public string TotalValue { get; set; }
        public string AveragePrice { get; set; }
        public string Performance { get; set; }
        public string RiskLevel { get; set; }
        public string Contact { get; set; }
    }
}
// [file content end]