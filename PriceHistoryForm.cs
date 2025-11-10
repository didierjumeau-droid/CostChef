// [file name]: PriceHistoryForm.cs
// [file content begin]
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public partial class PriceHistoryForm : Form
    {
        private TabControl tabControl;
        private DataGridView gridIngredientHistory;
        private DataGridView gridRecentChanges;
        private ComboBox cmbIngredients;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private Button btnFilter;
        private Button btnClose;

        public PriceHistoryForm()
        {
            InitializeComponent();
            LoadIngredients();
            LoadRecentPriceChanges();
        }

        // Constructor that accepts a specific ingredient
        public PriceHistoryForm(Ingredient selectedIngredient) : this()
        {
            // Auto-select the ingredient if provided
            if (selectedIngredient != null)
            {
                foreach (Ingredient ingredient in cmbIngredients.Items)
                {
                    if (ingredient.Id == selectedIngredient.Id)
                    {
                        cmbIngredients.SelectedItem = ingredient;
                        break;
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.tabControl = new TabControl();
            this.gridIngredientHistory = new DataGridView();
            this.gridRecentChanges = new DataGridView();
            this.cmbIngredients = new ComboBox();
            this.dtpStartDate = new DateTimePicker();
            this.dtpEndDate = new DateTimePicker();
            this.btnFilter = new Button();
            this.btnClose = new Button();

            // Form setup
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Text = "Price History Tracking";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(900, 600);

            // Main tab control
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Size = new System.Drawing.Size(860, 500);
            this.tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Tab 1: Specific Ingredient History
            var tabIngredientHistory = new TabPage("Ingredient Price History");
            tabIngredientHistory.Controls.Add(CreateIngredientHistoryTab());
            this.tabControl.TabPages.Add(tabIngredientHistory);

            // Tab 2: Recent Changes
            var tabRecentChanges = new TabPage("Recent Price Changes");
            tabRecentChanges.Controls.Add(CreateRecentChangesTab());
            this.tabControl.TabPages.Add(tabRecentChanges);

            // Close button
            this.btnClose.Text = "Close";
            this.btnClose.Location = new System.Drawing.Point(782, 520);
            this.btnClose.Size = new System.Drawing.Size(90, 30);
            this.btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { tabControl, btnClose });
            this.ResumeLayout(false);
        }

        private Panel CreateIngredientHistoryTab()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Filter controls
            var lblIngredient = new Label { Text = "Select Ingredient:", Location = new Point(10, 10), AutoSize = true };
            this.cmbIngredients.Location = new Point(120, 7);
            this.cmbIngredients.Size = new Size(200, 20);
            this.cmbIngredients.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbIngredients.SelectedIndexChanged += (s, e) => LoadIngredientPriceHistory();

            var lblStartDate = new Label { Text = "From:", Location = new Point(330, 10), AutoSize = true };
            this.dtpStartDate.Location = new Point(370, 7);
            this.dtpStartDate.Size = new Size(120, 20);
            this.dtpStartDate.Value = DateTime.Now.AddMonths(-1);

            var lblEndDate = new Label { Text = "To:", Location = new Point(500, 10), AutoSize = true };
            this.dtpEndDate.Location = new Point(520, 7);
            this.dtpEndDate.Size = new Size(120, 20);
            this.dtpEndDate.Value = DateTime.Now;

            this.btnFilter.Text = "Apply Filter";
            this.btnFilter.Location = new Point(650, 5);
            this.btnFilter.Size = new Size(80, 25);
            this.btnFilter.Click += (s, e) => LoadIngredientPriceHistory();

            // Data grid for ingredient history
            this.gridIngredientHistory.Location = new Point(10, 40);
            this.gridIngredientHistory.Size = new Size(820, 400);
            this.gridIngredientHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.gridIngredientHistory.ReadOnly = true;
            this.gridIngredientHistory.AutoGenerateColumns = false;
            this.gridIngredientHistory.AllowUserToAddRows = false;
            this.gridIngredientHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Configure columns for ingredient history
            this.gridIngredientHistory.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colChangeDate", HeaderText = "Change Date", DataPropertyName = "ChangeDate", Width = 120 
            });
            this.gridIngredientHistory.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colOldPrice", HeaderText = "Old Price", DataPropertyName = "OldPrice", Width = 100 
            });
            this.gridIngredientHistory.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colNewPrice", HeaderText = "New Price", DataPropertyName = "NewPrice", Width = 100 
            });
            this.gridIngredientHistory.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colPriceChange", HeaderText = "Change", DataPropertyName = "PriceChange", Width = 80 
            });
            this.gridIngredientHistory.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colPercentChange", HeaderText = "% Change", DataPropertyName = "PercentChange", Width = 80 
            });
            this.gridIngredientHistory.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colChangedBy", HeaderText = "Changed By", DataPropertyName = "ChangedBy", Width = 100 
            });
            this.gridIngredientHistory.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colReason", HeaderText = "Reason", DataPropertyName = "Reason", Width = 200 
            });

            panel.Controls.AddRange(new Control[] {
                lblIngredient, cmbIngredients, lblStartDate, dtpStartDate, 
                lblEndDate, dtpEndDate, btnFilter, gridIngredientHistory
            });

            return panel;
        }

        private Panel CreateRecentChangesTab()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Title
            var lblTitle = new Label { 
                Text = "Recent Price Changes (Last 50 Changes)", 
                Location = new Point(10, 10), 
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // Data grid for recent changes
            this.gridRecentChanges.Location = new Point(10, 40);
            this.gridRecentChanges.Size = new Size(820, 400);
            this.gridRecentChanges.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.gridRecentChanges.ReadOnly = true;
            this.gridRecentChanges.AutoGenerateColumns = false;
            this.gridRecentChanges.AllowUserToAddRows = false;
            this.gridRecentChanges.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Configure columns for recent changes
            this.gridRecentChanges.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colChangeDate", HeaderText = "Change Date", DataPropertyName = "ChangeDate", Width = 120 
            });
            this.gridRecentChanges.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colIngredient", HeaderText = "Ingredient", DataPropertyName = "IngredientName", Width = 150 
            });
            this.gridRecentChanges.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colOldPrice", HeaderText = "Old Price", DataPropertyName = "OldPrice", Width = 100 
            });
            this.gridRecentChanges.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colNewPrice", HeaderText = "New Price", DataPropertyName = "NewPrice", Width = 100 
            });
            this.gridRecentChanges.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colPriceChange", HeaderText = "Change", DataPropertyName = "PriceChange", Width = 80 
            });
            this.gridRecentChanges.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colChangedBy", HeaderText = "Changed By", DataPropertyName = "ChangedBy", Width = 100 
            });
            this.gridRecentChanges.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colReason", HeaderText = "Reason", DataPropertyName = "Reason", Width = 150 
            });

            panel.Controls.AddRange(new Control[] { lblTitle, gridRecentChanges });
            return panel;
        }

        private void LoadIngredients()
        {
            try
            {
                var ingredients = DatabaseContext.GetAllIngredients();
                
                // FIX: Filter out invalid ingredients that might cause errors
                var validIngredients = ingredients?.Where(i => i != null && !string.IsNullOrEmpty(i.Name)).ToList() ?? new List<Ingredient>();
                
                cmbIngredients.DataSource = validIngredients;
                cmbIngredients.DisplayMember = "Name";
                cmbIngredients.ValueMember = "Id";

                if (cmbIngredients.Items.Count > 0)
                {
                    cmbIngredients.SelectedIndex = 0;
                    LoadIngredientPriceHistory();
                }
                else
                {
                    // Show message if no ingredients available
                    gridIngredientHistory.DataSource = new List<object> { new { Message = "No ingredients available" } };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Set empty data source to prevent further errors
                gridIngredientHistory.DataSource = new List<object> { new { Error = "Failed to load ingredients" } };
            }
        }

        private void LoadIngredientPriceHistory()
        {
            if (cmbIngredients.SelectedItem == null) 
            {
                gridIngredientHistory.DataSource = new List<object> { new { Message = "No ingredient selected" } };
                return;
            }

            try
            {
                var selectedIngredient = cmbIngredients.SelectedItem as Ingredient;
                if (selectedIngredient == null) 
                {
                    gridIngredientHistory.DataSource = new List<object> { new { Message = "Invalid ingredient selected" } };
                    return;
                }
                
                int ingredientId = selectedIngredient.Id;
                var priceHistory = DatabaseContext.GetPriceHistory(ingredientId);

                // Apply date filter
                var filteredHistory = priceHistory.Where(ph => 
                    ph.ChangeDate >= dtpStartDate.Value.Date && 
                    ph.ChangeDate <= dtpEndDate.Value.Date.AddDays(1)
                ).ToList();

                if (filteredHistory.Count == 0)
                {
                    gridIngredientHistory.DataSource = new List<object> { new { Message = "No price history found for selected period" } };
                    return;
                }

                // Create display data with calculated fields
                var displayData = filteredHistory.Select(ph => new 
                {
                    ChangeDate = ph.ChangeDate.ToString("yyyy-MM-dd HH:mm"),
                    OldPrice = FormatPrice(ph.OldPrice),
                    NewPrice = FormatPrice(ph.NewPrice),
                    PriceChange = FormatPriceChange(ph.OldPrice, ph.NewPrice),
                    PercentChange = FormatPercentChange(ph.OldPrice, ph.NewPrice),
                    ChangedBy = ph.ChangedBy ?? "System",
                    Reason = ph.Reason ?? "Price update"
                }).ToList();

                gridIngredientHistory.DataSource = displayData;
                ApplyPriceChangeColorCoding(gridIngredientHistory, "PriceChange");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading price history: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                gridIngredientHistory.DataSource = new List<object> { new { Error = "Failed to load price history" } };
            }
        }

        private void LoadRecentPriceChanges()
        {
            try
            {
                var recentChanges = DatabaseContext.GetRecentPriceChanges(50);

                if (recentChanges == null || recentChanges.Count == 0)
                {
                    gridRecentChanges.DataSource = new List<object> { new { Message = "No recent price changes found" } };
                    return;
                }

                // Create display data with all required properties
                var displayData = recentChanges.Select(ph => new 
                {
                    ChangeDate = ph.ChangeDate.ToString("yyyy-MM-dd HH:mm"),
                    IngredientName = GetIngredientName(ph) ?? "Unknown Ingredient",
                    OldPrice = FormatPrice(ph.OldPrice),
                    NewPrice = FormatPrice(ph.NewPrice),
                    PriceChange = FormatPriceChange(ph.OldPrice, ph.NewPrice),
                    ChangedBy = ph.ChangedBy ?? "System",
                    Reason = ph.Reason ?? "Price update"
                }).ToList();

                gridRecentChanges.DataSource = displayData;
                ApplyPriceChangeColorCoding(gridRecentChanges, "PriceChange");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recent price changes: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                gridRecentChanges.DataSource = new List<object> { new { Error = "Failed to load recent changes" } };
            }
        }

        private string FormatPrice(decimal price)
        {
            return $"{AppSettings.CurrencySymbol}{price:F4}";
        }

        private string FormatPriceChange(decimal oldPrice, decimal newPrice)
        {
            var change = newPrice - oldPrice;
            var symbol = change >= 0 ? "+" : "";
            return $"{symbol}{AppSettings.CurrencySymbol}{Math.Abs(change):F4}";
        }

        private string FormatPercentChange(decimal oldPrice, decimal newPrice)
        {
            if (oldPrice == 0) return "N/A";
            var percentChange = ((newPrice - oldPrice) / oldPrice) * 100;
            var symbol = percentChange >= 0 ? "+" : "";
            return $"{symbol}{Math.Abs(percentChange):F1}%";
        }

        private string GetIngredientName(PriceHistory priceHistory)
        {
            try
            {
                // Use reflection to get IngredientName if available
                var prop = priceHistory.GetType().GetProperty("IngredientName");
                return prop?.GetValue(priceHistory)?.ToString() ?? "Unknown Ingredient";
            }
            catch
            {
                return "Unknown Ingredient";
            }
        }

        private void ApplyPriceChangeColorCoding(DataGridView grid, string columnName)
        {
            try
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.Cells[columnName]?.Value != null)
                    {
                        var changeText = row.Cells[columnName].Value.ToString();
                        if (changeText.StartsWith("+"))
                        {
                            row.Cells[columnName].Style.ForeColor = Color.Red;
                            row.Cells[columnName].Style.Font = new Font(grid.Font, FontStyle.Bold);
                        }
                        else if (changeText.StartsWith("-"))
                        {
                            row.Cells[columnName].Style.ForeColor = Color.Green;
                            row.Cells[columnName].Style.Font = new Font(grid.Font, FontStyle.Bold);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Silent fail - color coding is non-essential
                System.Diagnostics.Debug.WriteLine($"Color coding error: {ex.Message}");
            }
        }
    }
}
// [file content end]