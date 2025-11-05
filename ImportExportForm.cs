using System;
using System.Windows.Forms;

namespace CostChef
{
    public partial class ImportExportForm : Form
    {
        private Button btnExportIngredients;
        private Button btnImportIngredients;
        private Button btnExportRecipe;
        private Button btnImportRecipe;
        private Button btnClose;
        private Label lblTitle;

        public ImportExportForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.btnExportIngredients = new Button();
            this.btnImportIngredients = new Button();
            this.btnExportRecipe = new Button();
            this.btnImportRecipe = new Button();
            this.btnClose = new Button();
            this.lblTitle = new Label();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Text = "Import/Export Data";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(120, 20);
            this.lblTitle.Size = new System.Drawing.Size(160, 24);
            this.lblTitle.Text = "Import & Export";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Export Ingredients Button
            this.btnExportIngredients.Location = new System.Drawing.Point(80, 60);
            this.btnExportIngredients.Size = new System.Drawing.Size(240, 35);
            this.btnExportIngredients.Text = "📤 Export Ingredients to CSV";
            this.btnExportIngredients.Click += (s, e) => ExportIngredients();

            // Import Ingredients Button
            this.btnImportIngredients.Location = new System.Drawing.Point(80, 105);
            this.btnImportIngredients.Size = new System.Drawing.Size(240, 35);
            this.btnImportIngredients.Text = "📥 Import Ingredients from CSV";
            this.btnImportIngredients.Click += (s, e) => ImportIngredients();

            // Export Recipe Button
            this.btnExportRecipe.Location = new System.Drawing.Point(80, 150);
            this.btnExportRecipe.Size = new System.Drawing.Size(240, 35);
            this.btnExportRecipe.Text = "📤 Export Recipe to CSV";
            this.btnExportRecipe.Click += (s, e) => ExportRecipe();

            // Import Recipe Button
            this.btnImportRecipe.Location = new System.Drawing.Point(80, 195);
            this.btnImportRecipe.Size = new System.Drawing.Size(240, 35);
            this.btnImportRecipe.Text = "📥 Import Recipe from CSV";
            this.btnImportRecipe.Click += (s, e) => ImportRecipe();

            // Close Button
            this.btnClose.Location = new System.Drawing.Point(80, 240);
            this.btnClose.Size = new System.Drawing.Size(240, 35);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            // Add controls
            this.Controls.AddRange(new Control[] {
                lblTitle, btnExportIngredients, btnImportIngredients,
                btnExportRecipe, btnImportRecipe, btnClose
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ExportIngredients()
        {
            using var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "CSV Files (*.csv)|*.csv";
            saveDialog.FileName = "costchef_ingredients.csv";
            
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                ImportExportService.ExportIngredientsToCsv(saveDialog.FileName);
            }
        }

        private void ImportIngredients()
        {
            using var openDialog = new OpenFileDialog();
            openDialog.Filter = "CSV Files (*.csv)|*.csv";
            
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                ImportExportService.ImportIngredientsFromCsv(openDialog.FileName);
            }
        }

        private void ExportRecipe()
        {
            var recipes = DatabaseContext.GetAllRecipes();
            if (recipes.Count == 0)
            {
                MessageBox.Show("No recipes available to export.", "Information", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Let user select which recipe to export
            using var form = new Form();
            form.Text = "Select Recipe to Export";
            form.Size = new System.Drawing.Size(300, 150);
            form.StartPosition = FormStartPosition.CenterParent;

            var lbl = new Label { Text = "Select Recipe:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            var cmb = new ComboBox { DataSource = recipes, DisplayMember = "Name", Location = new System.Drawing.Point(20, 45), Size = new System.Drawing.Size(240, 20) };
            var btnOk = new Button { Text = "Export", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(120, 80) };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(200, 80) };

            form.Controls.AddRange(new Control[] { lbl, cmb, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            if (form.ShowDialog() == DialogResult.OK && cmb.SelectedItem is Recipe selectedRecipe)
            {
                using var saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV Files (*.csv)|*.csv";
                saveDialog.FileName = $"{selectedRecipe.Name.Replace(" ", "_")}.csv";
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ImportExportService.ExportRecipeToCsv(selectedRecipe, saveDialog.FileName);
                }
            }
        }

        private void ImportRecipe()
        {
            using var openDialog = new OpenFileDialog();
            openDialog.Filter = "CSV Files (*.csv)|*.csv";
            
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                var recipe = ImportExportService.ImportRecipeFromCsv(openDialog.FileName);
                if (recipe != null)
                {
                    // Save the imported recipe
                    DatabaseContext.InsertRecipe(recipe);
                    MessageBox.Show($"Recipe '{recipe.Name}' imported successfully!", "Import Successful", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}