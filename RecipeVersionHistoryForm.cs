using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public partial class RecipeVersionHistoryForm : Form
    {
        private int currentRecipeId;
        private string currentRecipeName;
        private DataGridView gridVersions;
        private Button btnViewVersion;
        private Button btnRestoreVersion;
        private Button btnCompare;
        private Button btnClose;
        private Label lblRecipeName;
        private Label lblVersionDetails;

        public RecipeVersionHistoryForm(int recipeId, string recipeName)
        {
            currentRecipeId = recipeId;
            currentRecipeName = recipeName;
            InitializeComponent();
            LoadVersions();
        }

        private void InitializeComponent()
        {
            this.gridVersions = new DataGridView();
            this.btnViewVersion = new Button();
            this.btnRestoreVersion = new Button();
            this.btnCompare = new Button();
            this.btnClose = new Button();
            this.lblRecipeName = new Label();
            this.lblVersionDetails = new Label();

            // Form setup
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Text = "Recipe Version History";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(800, 500);

            // Recipe name label
            this.lblRecipeName.Text = $"Version History: {currentRecipeName}";
            this.lblRecipeName.Font = new Font("Arial", 14, FontStyle.Bold);
            this.lblRecipeName.Location = new Point(20, 15);
            this.lblRecipeName.Size = new Size(760, 25);
            this.lblRecipeName.TextAlign = ContentAlignment.MiddleLeft;

            // Version details label
            this.lblVersionDetails.Text = "Select versions to compare (use checkboxes for multiple selection)";
            this.lblVersionDetails.Location = new Point(20, 45);
            this.lblVersionDetails.Size = new Size(760, 20);
            this.lblVersionDetails.ForeColor = Color.Gray;

            // Versions grid - MAKE EDITABLE FOR CHECKBOXES
            this.gridVersions.Location = new Point(20, 75);
            this.gridVersions.Size = new Size(760, 300);
            this.gridVersions.ReadOnly = false; // CHANGED TO FALSE
            this.gridVersions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.gridVersions.AutoGenerateColumns = false;
            this.gridVersions.AllowUserToAddRows = false;
            this.gridVersions.MultiSelect = true; // CHANGED TO TRUE for checkbox functionality

            // Configure grid columns
            // Add selection checkbox column FIRST
            this.gridVersions.Columns.Add(new DataGridViewCheckBoxColumn { 
                Name = "colSelect", HeaderText = "Select", Width = 50,
                ReadOnly = false // This column is editable
            });
            
            this.gridVersions.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colVersion", HeaderText = "Version", DataPropertyName = "DisplayName", Width = 150,
                ReadOnly = true // Make text columns read-only
            });
            this.gridVersions.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colCreated", HeaderText = "Created", DataPropertyName = "CreatedDisplay", Width = 120,
                ReadOnly = true
            });
            this.gridVersions.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colCreatedBy", HeaderText = "By", DataPropertyName = "CreatedBy", Width = 100,
                ReadOnly = true
            });
            this.gridVersions.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colNotes", HeaderText = "Notes", DataPropertyName = "VersionNotes", Width = 250,
                ReadOnly = true
            });
            this.gridVersions.Columns.Add(new DataGridViewCheckBoxColumn { 
                Name = "colCurrent", HeaderText = "Current", DataPropertyName = "IsCurrent", Width = 60,
                ReadOnly = true // Keep current version checkbox read-only
            });

            // Buttons
            this.btnViewVersion.Text = "View Version";
            this.btnViewVersion.Location = new Point(20, 390);
            this.btnViewVersion.Size = new Size(120, 30);
            this.btnViewVersion.Click += (s, e) => ViewSelectedVersion();

            this.btnRestoreVersion.Text = "Restore Version";
            this.btnRestoreVersion.Location = new Point(150, 390);
            this.btnRestoreVersion.Size = new Size(120, 30);
            this.btnRestoreVersion.Click += (s, e) => RestoreSelectedVersion();

            this.btnCompare.Text = "Compare Versions";
            this.btnCompare.Location = new Point(280, 390);
            this.btnCompare.Size = new Size(120, 30);
            this.btnCompare.Click += (s, e) => CompareVersions();

            this.btnClose.Text = "Close";
            this.btnClose.Location = new Point(680, 390);
            this.btnClose.Size = new Size(100, 30);
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblRecipeName, lblVersionDetails, gridVersions,
                btnViewVersion, btnRestoreVersion, btnCompare, btnClose
            });

            // Wire up events
            this.gridVersions.SelectionChanged += (s, e) => UpdateButtonStates();
            this.gridVersions.CellValueChanged += (s, e) => UpdateButtonStates();
            
            // Add checkbox click handler
            this.gridVersions.CellContentClick += (s, e) => 
            {
                if (e.ColumnIndex == 0) // Checkbox column
                {
                    gridVersions.EndEdit();
                    UpdateButtonStates();
                }
            };

            this.ResumeLayout(false);
        }

        private void LoadVersions()
        {
            try
            {
                var versions = RecipeVersioningService.GetRecipeVersions(currentRecipeId);
                gridVersions.DataSource = versions;

                if (versions.Count > 0)
                {
                    gridVersions.ClearSelection();
                    UpdateButtonStates();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading versions: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = gridVersions.SelectedRows.Count > 0;
            btnViewVersion.Enabled = hasSelection;
            btnRestoreVersion.Enabled = hasSelection;
            
            // FIX: Use checkbox count instead of selected rows
            int checkedCount = GetCheckedVersionCount();
            btnCompare.Enabled = checkedCount >= 2;

            if (hasSelection && gridVersions.SelectedRows[0].DataBoundItem is RecipeVersion selectedVersion)
            {
                lblVersionDetails.Text = selectedVersion.VersionNotes;
                
                // Disable restore for current version
                btnRestoreVersion.Enabled = !selectedVersion.IsCurrent;
            }
        }

        // NEW METHOD: Count checked versions
        private int GetCheckedVersionCount()
        {
            int count = 0;
            foreach (DataGridViewRow row in gridVersions.Rows)
            {
                var checkboxCell = row.Cells["colSelect"] as DataGridViewCheckBoxCell;
                if (checkboxCell?.Value != null && (bool)checkboxCell.Value)
                {
                    count++;
                }
            }
            return count;
        }

        // NEW METHOD: Get checked versions for comparison
        private List<RecipeVersion> GetCheckedVersions()
        {
            var versions = new List<RecipeVersion>();
            foreach (DataGridViewRow row in gridVersions.Rows)
            {
                var checkboxCell = row.Cells["colSelect"] as DataGridViewCheckBoxCell;
                if (checkboxCell?.Value != null && (bool)checkboxCell.Value)
                {
                    if (row.DataBoundItem is RecipeVersion version)
                    {
                        versions.Add(version);
                    }
                }
            }
            return versions;
        }

        private void ViewSelectedVersion()
        {
            if (gridVersions.SelectedRows.Count == 0) return;

            var selectedVersion = gridVersions.SelectedRows[0].DataBoundItem as RecipeVersion;
            if (selectedVersion == null) return;

            var versionData = selectedVersion.GetVersionData();
            if (versionData == null) return;

            ShowVersionDetails(selectedVersion, versionData);
        }

        private void ShowVersionDetails(RecipeVersion version, RecipeVersioningService.RecipeVersionData data)
        {
            var detailForm = new Form
            {
                Text = $"Version Details: {version.DisplayName}",
                Size = new Size(600, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var txtDetails = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9)
            };

            // Build details text
            string details = $"RECIPE: {data.Recipe.Name}\n";
            details += $"Version: {version.DisplayName}\n";
            details += $"Created: {version.CreatedDisplay} by {version.CreatedBy}\n";
            details += $"Notes: {version.VersionNotes}\n\n";
            details += $"Batch Yield: {data.Recipe.BatchYield}\n";
            details += $"Target Food Cost: {data.Recipe.TargetFoodCostPercentage:P0}\n";
            details += $"Total Cost: {AppSettings.CurrencySymbol}{data.TotalCost:F2}\n";
            details += $"Cost per Serving: {AppSettings.CurrencySymbol}{data.CostPerServing:F2}\n\n";
            details += "INGREDIENTS:\n";
            details += "------------\n";

            foreach (var ingredient in data.Ingredients)
            {
                details += $"{ingredient.Quantity} {ingredient.Unit} {ingredient.IngredientName}";
                details += $" = {AppSettings.CurrencySymbol}{ingredient.LineCost:F2}\n";
            }

            txtDetails.Text = details;
            detailForm.Controls.Add(txtDetails);

            var btnClose = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.OK,
                Location = new Point(250, 320),
                Size = new Size(100, 30)
            };
            detailForm.Controls.Add(btnClose);
            detailForm.AcceptButton = btnClose;

            detailForm.ShowDialog();
        }

        private void RestoreSelectedVersion()
        {
            if (gridVersions.SelectedRows.Count == 0) return;

            var selectedVersion = gridVersions.SelectedRows[0].DataBoundItem as RecipeVersion;
            if (selectedVersion == null || selectedVersion.IsCurrent) return;

            var result = MessageBox.Show(
                $"Restore '{selectedVersion.DisplayName}' as the current version?\n\nThis will replace the current recipe data.",
                "Confirm Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var versionData = selectedVersion.GetVersionData();
                    if (versionData != null)
                    {
                        // Update the recipe
                        DatabaseContext.UpdateRecipe(versionData.Recipe);

                        // Clear current ingredients
                        var currentIngredients = DatabaseContext.GetRecipeIngredients(currentRecipeId);
                        foreach (var ingredient in currentIngredients)
                        {
                            DatabaseContext.DeleteRecipeIngredient(ingredient.Id);
                        }

                        // Add restored ingredients
                        foreach (var ingredient in versionData.Ingredients)
                        {
                            DatabaseContext.AddRecipeIngredient(new RecipeIngredient
                            {
                                RecipeId = currentRecipeId,
                                IngredientId = ingredient.IngredientId,
                                Quantity = ingredient.Quantity
                            });
                        }

                        // Create a new version for the restore action
                        RecipeVersioningService.CreateVersion(currentRecipeId, 
                            $"Restored from {selectedVersion.DisplayName}",
                            $"Restored version {selectedVersion.VersionNumber}",
                            "User");

                        MessageBox.Show("Version restored successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error restoring version: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CompareVersions()
        {
            // FIX: Use checkbox selection instead of row selection
            var selectedVersions = GetCheckedVersions();
            
            if (selectedVersions.Count < 2)
            {
                MessageBox.Show("Please check at least two versions to compare.", "Selection Required", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ShowSimpleComparison(selectedVersions);
        }

        private void ShowSimpleComparison(List<RecipeVersion> versions)
        {
            var compareForm = new Form
            {
                Text = $"Version Comparison - {currentRecipeName}",
                Size = new Size(900, 600),
                StartPosition = FormStartPosition.CenterParent,
                MinimumSize = new Size(900, 600)
            };

            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };

            // Summary Tab
            var summaryTab = new TabPage("Summary Comparison");
            CreateSummaryTab(summaryTab, versions);
            tabControl.TabPages.Add(summaryTab);

            // Detailed Tab
            var detailedTab = new TabPage("Detailed Comparison");
            CreateDetailedTab(detailedTab, versions);
            tabControl.TabPages.Add(detailedTab);

            // Ingredients Tab
            var ingredientsTab = new TabPage("Ingredients Comparison");
            CreateIngredientsTab(ingredientsTab, versions);
            tabControl.TabPages.Add(ingredientsTab);

            compareForm.Controls.Add(tabControl);
            compareForm.ShowDialog();
        }

      private void CreateSummaryTab(TabPage tab, List<RecipeVersion> versions)
{
    var panel = new Panel
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        Padding = new Padding(10)
    };

    int yPos = 10;
    var headerLabel = new Label
    {
        Text = "VERSION COMPARISON SUMMARY",
        Font = new Font("Segoe UI", 12, FontStyle.Bold),
        Location = new Point(10, yPos),
        Size = new Size(850, 25),
        ForeColor = Color.Navy
    };
    panel.Controls.Add(headerLabel);
    yPos += 35;

    // Create comparison table
    var tablePanel = new Panel
    {
        Location = new Point(10, yPos),
        Size = new Size(850, 200),
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.White
    };

    // Headers
    var headers = new[] { "Metric", "Current", "Comparison" };
    for (int i = 0; i < headers.Length; i++)
    {
        var header = new Label
        {
            Text = headers[i],
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(i * 280 + 10, 10),
            Size = new Size(270, 20),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.LightGray
        };
        tablePanel.Controls.Add(header);
    }

    // Data rows
    var currentVersion = versions.OrderByDescending(v => v.VersionNumber).First();
    var compareVersion = versions.OrderByDescending(v => v.VersionNumber).Last();
    
    var currentData = currentVersion.GetVersionData();
    var compareData = compareVersion.GetVersionData();

    if (currentData != null && compareData != null)
    {
        // FIX: Use explicit type instead of implicit array
        var metrics = new List<MetricComparison>
        {
            new MetricComparison { Name = "Total Cost", Current = currentData.TotalCost, Compare = compareData.TotalCost },
            new MetricComparison { Name = "Cost per Serving", Current = currentData.CostPerServing, Compare = compareData.CostPerServing },
            new MetricComparison { Name = "Number of Ingredients", Current = currentData.Ingredients.Count, Compare = compareData.Ingredients.Count },
            new MetricComparison { Name = "Batch Yield", Current = currentData.Recipe.BatchYield, Compare = compareData.Recipe.BatchYield },
            new MetricComparison { Name = "Target Food Cost %", Current = currentData.Recipe.TargetFoodCostPercentage, Compare = compareData.Recipe.TargetFoodCostPercentage }
        };

        for (int i = 0; i < metrics.Count; i++)
        {
            var metric = metrics[i];
            int rowY = 40 + i * 30;

            // Metric name
            var nameLabel = new Label
            {
                Text = metric.Name,
                Location = new Point(10, rowY),
                Size = new Size(270, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tablePanel.Controls.Add(nameLabel);

            // Current value
            var currentLabel = new Label
            {
                Text = FormatMetricValue(metric.Name, metric.Current),
                Location = new Point(280, rowY),
                Size = new Size(270, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tablePanel.Controls.Add(currentLabel);

            // Comparison value with color coding
            var compareLabel = new Label
            {
                Text = FormatMetricValue(metric.Name, metric.Compare),
                Location = new Point(550, rowY),
                Size = new Size(270, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Color code based on whether it's better (green) or worse (red)
            if (metric.Name.Contains("Cost") && metric.Current != metric.Compare)
            {
                compareLabel.ForeColor = metric.Compare < metric.Current ? Color.Green : Color.Red;
                compareLabel.Font = new Font(compareLabel.Font, FontStyle.Bold);
            }

            tablePanel.Controls.Add(compareLabel);
        }
    }

    panel.Controls.Add(tablePanel);
    yPos += 220;

    // Version details
    var detailsLabel = new Label
    {
        Text = "VERSION DETAILS:",
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        Location = new Point(10, yPos),
        Size = new Size(850, 20),
        ForeColor = Color.DarkBlue
    };
    panel.Controls.Add(detailsLabel);
    yPos += 30;

    foreach (var version in versions.OrderByDescending(v => v.VersionNumber))
    {
        var versionPanel = new Panel
        {
            Location = new Point(10, yPos),
            Size = new Size(850, 80),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = version.IsCurrent ? Color.LightYellow : Color.White
        };

        var versionHeader = new Label
        {
            Text = $"{version.DisplayName} {(version.IsCurrent ? "(CURRENT)" : "")}",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(10, 10),
            Size = new Size(400, 20)
        };
        versionPanel.Controls.Add(versionHeader);

        var createdLabel = new Label
        {
            Text = $"Created: {version.CreatedDisplay} by {version.CreatedBy}",
            Location = new Point(10, 35),
            Size = new Size(400, 20)
        };
        versionPanel.Controls.Add(createdLabel);

        var notesLabel = new Label
        {
            Text = $"Notes: {version.VersionNotes}",
            Location = new Point(10, 55),
            Size = new Size(800, 20),
            ForeColor = Color.DarkGray
        };
        versionPanel.Controls.Add(notesLabel);

        panel.Controls.Add(versionPanel);
        yPos += 90;
    }

    tab.Controls.Add(panel);
}

// ADD THIS CLASS at the end of the RecipeVersionHistoryForm class, just before the final closing brace
private class MetricComparison
{
    public string Name { get; set; }
    public decimal Current { get; set; }
    public decimal Compare { get; set; }
}

        private void CreateDetailedTab(TabPage tab, List<RecipeVersion> versions)
        {
            var textBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                BackColor = Color.White
            };

            var comparison = new System.Text.StringBuilder();
            comparison.AppendLine("DETAILED VERSION COMPARISON");
            comparison.AppendLine("============================");
            comparison.AppendLine();

            foreach (var version in versions.OrderBy(v => v.VersionNumber))
            {
                var data = version.GetVersionData();
                if (data != null)
                {
                    comparison.AppendLine($"{version.DisplayName} {(version.IsCurrent ? "(CURRENT)" : "")}");
                    comparison.AppendLine(new string('-', 50));
                    comparison.AppendLine($"Created: {version.CreatedDisplay} by {version.CreatedBy}");
                    comparison.AppendLine($"Notes: {version.VersionNotes}");
                    comparison.AppendLine();
                    comparison.AppendLine($"Batch Yield: {data.Recipe.BatchYield}");
                    comparison.AppendLine($"Target Food Cost: {data.Recipe.TargetFoodCostPercentage:P0}");
                    comparison.AppendLine($"Total Cost: {AppSettings.CurrencySymbol}{data.TotalCost:F2}");
                    comparison.AppendLine($"Cost per Serving: {AppSettings.CurrencySymbol}{data.CostPerServing:F2}");
                    comparison.AppendLine($"Ingredients: {data.Ingredients.Count}");
                    comparison.AppendLine();
                    
                    comparison.AppendLine("INGREDIENTS:");
                    comparison.AppendLine("------------");
                    foreach (var ingredient in data.Ingredients)
                    {
                        comparison.AppendLine($"  {ingredient.Quantity} {ingredient.Unit} {ingredient.IngredientName}");
                        comparison.AppendLine($"    = {AppSettings.CurrencySymbol}{ingredient.LineCost:F2}");
                    }
                    comparison.AppendLine();
                    comparison.AppendLine();
                }
            }

            textBox.Text = comparison.ToString();
            tab.Controls.Add(textBox);
        }

        private void CreateIngredientsTab(TabPage tab, List<RecipeVersion> versions)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            int yPos = 10;
            var headerLabel = new Label
            {
                Text = "INGREDIENTS COMPARISON",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, yPos),
                Size = new Size(850, 25),
                ForeColor = Color.Navy
            };
            panel.Controls.Add(headerLabel);
            yPos += 40;

            // Get all unique ingredients across all versions
            var allIngredients = new Dictionary<string, List<decimal>>();
            
            foreach (var version in versions.OrderByDescending(v => v.VersionNumber))
            {
                var data = version.GetVersionData();
                if (data != null)
                {
                    foreach (var ingredient in data.Ingredients)
                    {
                        var key = $"{ingredient.IngredientName} ({ingredient.Unit})";
                        if (!allIngredients.ContainsKey(key))
                        {
                            allIngredients[key] = new List<decimal>();
                        }
                        // Pad the list to align with version count
                        while (allIngredients[key].Count < versions.IndexOf(version))
                        {
                            allIngredients[key].Add(0);
                        }
                        allIngredients[key].Add(ingredient.Quantity);
                    }
                }
            }

            // Create table headers
            var versionHeaders = versions.Select(v => v.DisplayName).ToArray();
            
            // Header row
            var headerPanel = new Panel
            {
                Location = new Point(10, yPos),
                Size = new Size(850, 30),
                BackColor = Color.LightGray
            };

            var ingredientHeader = new Label
            {
                Text = "Ingredient",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(10, 5),
                Size = new Size(300, 20)
            };
            headerPanel.Controls.Add(ingredientHeader);

            for (int i = 0; i < versionHeaders.Length; i++)
            {
                var versionHeader = new Label
                {
                    Text = $"Ver {i+1}",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Location = new Point(320 + i * 120, 5),
                    Size = new Size(110, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                headerPanel.Controls.Add(versionHeader);
            }

            panel.Controls.Add(headerPanel);
            yPos += 35;

            // Data rows
            foreach (var ingredient in allIngredients)
            {
                var rowPanel = new Panel
                {
                    Location = new Point(10, yPos),
                    Size = new Size(850, 25),
                    BackColor = yPos % 2 == 0 ? Color.White : Color.Lavender
                };

                var nameLabel = new Label
                {
                    Text = ingredient.Key,
                    Location = new Point(10, 5),
                    Size = new Size(300, 20),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                rowPanel.Controls.Add(nameLabel);

                for (int i = 0; i < versionHeaders.Length; i++)
                {
                    var quantity = i < ingredient.Value.Count ? ingredient.Value[i] : 0;
                    var quantityLabel = new Label
                    {
                        Text = quantity > 0 ? quantity.ToString("F2") : "-",
                        Location = new Point(320 + i * 120, 5),
                        Size = new Size(110, 20),
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = quantity == 0 ? Color.Gray : Color.Black
                    };
                    rowPanel.Controls.Add(quantityLabel);
                }

                panel.Controls.Add(rowPanel);
                yPos += 30;
            }

            tab.Controls.Add(panel);
        }

private string FormatMetricValue(string metricName, decimal value)
{
    if (metricName.Contains("Cost"))
    {
        return $"{AppSettings.CurrencySymbol}{value:F2}";
    }
    else if (metricName.Contains("Percentage"))
    {
        return $"{value:P0}";
    }
    else if (metricName.Contains("Number of Ingredients") || metricName.Contains("Batch Yield"))
    {
        return $"{value:F0}";
    }
    return value.ToString("F2");
}
    }
}