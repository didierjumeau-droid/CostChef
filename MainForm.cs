using System;
using System.Windows.Forms;

namespace CostChef
{
    public partial class MainForm : Form
    {
        private Button btnIngredients;
        private Button btnRecipes;
        private Button btnImportExport;
        private Button btnExit;
        private Button btnSettings;
        private Button btnSupplierManagement;
        private Button btnSupplierReports;
        private Button btnPriceHistory;
        private Button btnInventory;
        private Button btnMenuProfitability;   // ✅ NEW
        private Label lblTitle;
        private Label lblSubtitle;

        public MainForm()
        {
            InitializeComponent();

            // Initialize database on app startup
            DatabaseContext.InitializeDatabase();
        }

        private void InitializeComponent()
        {
            this.btnIngredients = new Button();
            this.btnRecipes = new Button();
            this.btnImportExport = new Button();
            this.btnExit = new Button();
            this.btnSettings = new Button();
            this.btnSupplierManagement = new Button();
            this.btnSupplierReports = new Button();
            this.btnPriceHistory = new Button();
            this.btnInventory = new Button();
            this.btnMenuProfitability = new Button();  // ✅ NEW
            this.lblTitle = new Label();
            this.lblSubtitle = new Label();

            // Main Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(400, 580);
            this.Text = "CostChef - Menu Costing & Inventory";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font(
                "Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(80, 40);
            this.lblTitle.Size = new System.Drawing.Size(240, 29);
            this.lblTitle.Text = "CostChef V3.0";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblSubtitle
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font(
                "Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular);
            this.lblSubtitle.Location = new System.Drawing.Point(80, 80);
            this.lblSubtitle.Size = new System.Drawing.Size(240, 17);
            this.lblSubtitle.Text = "Simple, Affordable Menu Costing";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // btnIngredients
            this.btnIngredients.Location = new System.Drawing.Point(80, 120);
            this.btnIngredients.Size = new System.Drawing.Size(240, 40);
            this.btnIngredients.Text = "📦 Manage Ingredients";
            this.btnIngredients.Click += new EventHandler(this.btnIngredients_Click);

            // btnRecipes
            this.btnRecipes.Location = new System.Drawing.Point(80, 170);
            this.btnRecipes.Size = new System.Drawing.Size(240, 40);
            this.btnRecipes.Text = "🍳 Manage Recipes";
            this.btnRecipes.Click += new EventHandler(this.btnRecipes_Click);

            // Supplier Management Button
            this.btnSupplierManagement.Location = new System.Drawing.Point(80, 220);
            this.btnSupplierManagement.Size = new System.Drawing.Size(240, 40);
            this.btnSupplierManagement.Text = "🏪 Manage Suppliers";
            this.btnSupplierManagement.Click += new EventHandler(this.btnSupplierManagement_Click);

           
            // btnSupplierReports
            this.btnSupplierReports.Location = new System.Drawing.Point(80, 270);
            this.btnSupplierReports.Size = new System.Drawing.Size(240, 40);
            this.btnSupplierReports.Text = "📊 Supplier Reports";
            this.btnSupplierReports.Click += new EventHandler(this.btnSupplierReports_Click);

            // btnPriceHistory
            this.btnPriceHistory.Location = new System.Drawing.Point(80, 370);
            this.btnPriceHistory.Size = new System.Drawing.Size(240, 40);
            this.btnPriceHistory.Text = "📈 Price History";
            this.btnPriceHistory.Click += new EventHandler(this.btnPriceHistory_Click);

            // ✅ NEW: Menu Profitability Button
            this.btnMenuProfitability.Location = new System.Drawing.Point(80, 420);
            this.btnMenuProfitability.Size = new System.Drawing.Size(240, 40);
            this.btnMenuProfitability.Text = "📊 Menu Profitability";
            this.btnMenuProfitability.Click += new EventHandler(this.btnMenuProfitability_Click);

            // ✅ Inventory button (shifted down)
            this.btnInventory.Location = new System.Drawing.Point(80, 320);
            this.btnInventory.Size = new System.Drawing.Size(240, 40);
            this.btnInventory.Text = "📦 Inventory Management";
            this.btnInventory.Click += new EventHandler(this.btnInventory_Click);

 // btnImportExport
            this.btnImportExport.Location = new System.Drawing.Point(80, 470);
            this.btnImportExport.Size = new System.Drawing.Size(240, 40);
            this.btnImportExport.Text = "📤 Import / Export Data";
            this.btnImportExport.Click += new EventHandler(this.btnImportExport_Click);

            // btnExit (shifted down)
            this.btnExit.Location = new System.Drawing.Point(80, 520);
            this.btnExit.Size = new System.Drawing.Size(240, 40);
            this.btnExit.Text = "Exit";
            this.btnExit.Click += new EventHandler(this.btnExit_Click);

            // Settings Button (small gear in top-right)
            this.btnSettings.Location = new System.Drawing.Point(350, 12);
            this.btnSettings.Size = new System.Drawing.Size(30, 30);
            this.btnSettings.Text = "⚙️";
            this.btnSettings.Font = new System.Drawing.Font(
                "Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular);
            this.btnSettings.Cursor = Cursors.Hand;
            this.btnSettings.FlatStyle = FlatStyle.Flat;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.Click += (s, e) =>
            {
                var settingsForm = new SettingsForm();
                settingsForm.ShowDialog();
            };

            // Add controls to form
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.btnIngredients);
            this.Controls.Add(this.btnRecipes);
            this.Controls.Add(this.btnSupplierManagement);
            this.Controls.Add(this.btnImportExport);
            this.Controls.Add(this.btnSupplierReports);
            this.Controls.Add(this.btnPriceHistory);
            this.Controls.Add(this.btnMenuProfitability);  // ✅ NEW
            this.Controls.Add(this.btnInventory);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnSettings);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // ===== Click Handlers =====

        private void btnIngredients_Click(object sender, EventArgs e)
        {
            var ingredientsForm = new IngredientsForm();
            ingredientsForm.ShowDialog();
        }

        private void btnRecipes_Click(object sender, EventArgs e)
        {
            var recipesForm = new RecipesForm();
            recipesForm.ShowDialog();
        }

        private void btnSupplierManagement_Click(object sender, EventArgs e)
        {
            var supplierManagementForm = new SupplierManagementForm();
            supplierManagementForm.ShowDialog();
        }

        private void btnImportExport_Click(object sender, EventArgs e)
        {
            var importExportForm = new ImportExportForm();
            importExportForm.ShowDialog();
        }

        private void btnSupplierReports_Click(object sender, EventArgs e)
        {
            var supplierReportsForm = new SupplierReportsForm();
            supplierReportsForm.ShowDialog();
        }

        private void btnPriceHistory_Click(object sender, EventArgs e)
        {
            var priceHistoryForm = new PriceHistoryForm();
            priceHistoryForm.ShowDialog();
        }

        private void btnMenuProfitability_Click(object sender, EventArgs e)
        {
            using (var f = new MenuProfitabilityForm())
            {
                f.ShowDialog();
            }
        }

        // Inventory click handler
        private void btnInventory_Click(object sender, EventArgs e)
        {
            var inventoryForm = new InventoryForm();
            inventoryForm.ShowDialog();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit CostChef?",
                "Exit Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
