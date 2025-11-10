// [file name]: RecipeVersionHistoryForm.cs
// [file content begin]
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
            this.lblVersionDetails.Text = "Select a version to view details or restore";
            this.lblVersionDetails.Location = new Point(20, 45);
            this.lblVersionDetails.Size = new Size(760, 20);
            this.lblVersionDetails.ForeColor = Color.Gray;

            // Versions grid
            this.gridVersions.Location = new Point(20, 75);
            this.gridVersions.Size = new Size(760, 300);
            this.gridVersions.ReadOnly = true;
            this.gridVersions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.gridVersions.AutoGenerateColumns = false;
            this.gridVersions.AllowUserToAddRows = false;
            this.gridVersions.MultiSelect = false;

            // Configure grid columns
            this.gridVersions.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colVersion", HeaderText = "Version", DataPropertyName = "DisplayName", Width = 150 
            });
            this.gridVersions.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colCreated", HeaderText = "Created", DataPropertyName = "CreatedDisplay", Width = 120 
            });
            this.gridVersions.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colCreatedBy", HeaderText = "By", DataPropertyName = "CreatedBy", Width = 100 
            });
            this.gridVersions.Columns.Add(new DataGridViewTextBoxColumn { 
                Name = "colNotes", HeaderText = "Notes", DataPropertyName = "VersionNotes", Width = 250 
            });
            this.gridVersions.Columns.Add(new DataGridViewCheckBoxColumn { 
                Name = "colCurrent", HeaderText = "Current", DataPropertyName = "IsCurrent", Width = 60 
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

            // Wire up selection changed
            this.gridVersions.SelectionChanged += (s, e) => UpdateButtonStates();

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
            btnCompare.Enabled = gridVersions.SelectedRows.Count >= 2;

            if (hasSelection && gridVersions.SelectedRows[0].DataBoundItem is RecipeVersion selectedVersion)
            {
                lblVersionDetails.Text = selectedVersion.VersionNotes;
                
                // Disable restore for current version
                btnRestoreVersion.Enabled = !selectedVersion.IsCurrent;
            }
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
            if (gridVersions.SelectedRows.Count < 2)
            {
                MessageBox.Show("Please select at least two versions to compare.", "Selection Required", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedVersions = new List<RecipeVersion>();
            foreach (DataGridViewRow row in gridVersions.SelectedRows)
            {
                if (row.DataBoundItem is RecipeVersion version)
                {
                    selectedVersions.Add(version);
                }
            }

            if (selectedVersions.Count >= 2)
            {
                // For now, show a simple comparison - we can enhance this later
                ShowSimpleComparison(selectedVersions);
            }
        }

        private void ShowSimpleComparison(List<RecipeVersion> versions)
        {
            var compareForm = new Form
            {
                Text = "Version Comparison",
                Size = new Size(700, 500),
                StartPosition = FormStartPosition.CenterParent
            };

            var comparisonText = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9)
            };

            string comparison = "VERSION COMPARISON\n";
            comparison += "===================\n\n";

            foreach (var version in versions.OrderBy(v => v.VersionNumber))
            {
                var data = version.GetVersionData();
                if (data != null)
                {
                    comparison += $"{version.DisplayName}:\n";
                    comparison += $"  Total Cost: {AppSettings.CurrencySymbol}{data.TotalCost:F2}\n";
                    comparison += $"  Cost/Serving: {AppSettings.CurrencySymbol}{data.CostPerServing:F2}\n";
                    comparison += $"  Ingredients: {data.Ingredients.Count}\n";
                    comparison += $"  Notes: {version.VersionNotes}\n\n";
                }
            }

            comparisonText.Text = comparison;
            compareForm.Controls.Add(comparisonText);

            compareForm.ShowDialog();
        }
    }
}
// [file content end]