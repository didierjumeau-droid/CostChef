using System;
using System.Windows.Forms;

namespace CostChef
{
    public partial class ImportExportForm : Form
    {
        private Button btnExportRecipesCsv;
        private Button btnExportIngredientsCsv;
        private Button btnExportRecipesJson;
        private Button btnExportIngredientsJson;
        private Button btnImportRecipesCsv;
        private Button btnClose;

        public ImportExportForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.btnExportRecipesCsv = new Button();
            this.btnExportIngredientsCsv = new Button();
            this.btnExportRecipesJson = new Button();
            this.btnExportIngredientsJson = new Button();
            this.btnImportRecipesCsv = new Button();
            this.btnClose = new Button();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Text = "Import/Export Data";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Buttons
            this.btnExportRecipesCsv.Location = new System.Drawing.Point(50, 30);
            this.btnExportRecipesCsv.Size = new System.Drawing.Size(300, 35);
            this.btnExportRecipesCsv.Text = "Export Recipes to CSV";
            this.btnExportRecipesCsv.Click += (s, e) => ImportExportService.ExportRecipesToCsv();

            this.btnExportIngredientsCsv.Location = new System.Drawing.Point(50, 75);
            this.btnExportIngredientsCsv.Size = new System.Drawing.Size(300, 35);
            this.btnExportIngredientsCsv.Text = "Export Ingredients to CSV";
            this.btnExportIngredientsCsv.Click += (s, e) => ImportExportService.ExportIngredientsToCsv();

            this.btnExportRecipesJson.Location = new System.Drawing.Point(50, 120);
            this.btnExportRecipesJson.Size = new System.Drawing.Size(300, 35);
            this.btnExportRecipesJson.Text = "Export Recipes to JSON";
            this.btnExportRecipesJson.Click += (s, e) => ImportExportService.ExportRecipesToJson();

            this.btnExportIngredientsJson.Location = new System.Drawing.Point(50, 165);
            this.btnExportIngredientsJson.Size = new System.Drawing.Size(300, 35);
            this.btnExportIngredientsJson.Text = "Export Ingredients to JSON";
            this.btnExportIngredientsJson.Click += (s, e) => ImportExportService.ExportIngredientsToJson();

            this.btnImportRecipesCsv.Location = new System.Drawing.Point(50, 210);
            this.btnImportRecipesCsv.Size = new System.Drawing.Size(300, 35);
            this.btnImportRecipesCsv.Text = "Import Recipes from CSV";
            this.btnImportRecipesCsv.Click += (s, e) => ImportExportService.ImportRecipesFromCsv();

            this.btnClose.Location = new System.Drawing.Point(50, 255);
            this.btnClose.Size = new System.Drawing.Size(300, 35);
            this.btnClose.Text = "Close";
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                btnExportRecipesCsv, btnExportIngredientsCsv, btnExportRecipesJson,
                btnExportIngredientsJson, btnImportRecipesCsv, btnClose
            });

            this.ResumeLayout(false);
        }
    }
}