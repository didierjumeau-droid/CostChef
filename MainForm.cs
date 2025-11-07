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
        private Button btnSettings; // ✅ ADDED THIS LINE
        private Label lblTitle;
        private Label lblSubtitle;

        public MainForm()
        {
            InitializeComponent();
            
            // Initialize database
            DatabaseContext.InitializeDatabase();
        }

        private void InitializeComponent()
        {
            this.btnIngredients = new Button();
            this.btnRecipes = new Button();
            this.btnImportExport = new Button();
            this.btnExit = new Button();
            this.btnSettings = new Button(); // ✅ ADDED THIS LINE
            this.lblTitle = new Label();
            this.lblSubtitle = new Label();
            
            // Main Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(400, 350);
            this.Text = "CostChef v1.1 - Simple Menu Costing";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(120, 40);
            this.lblTitle.Size = new System.Drawing.Size(160, 29);
            this.lblTitle.Text = "CostChef v1.1";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            
            // lblSubtitle
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
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

            // btnImportExport
            this.btnImportExport.Location = new System.Drawing.Point(80, 220);
            this.btnImportExport.Size = new System.Drawing.Size(240, 40);
            this.btnImportExport.Text = "📤 Import/Export Data";
            this.btnImportExport.Click += new EventHandler(this.btnImportExport_Click);
            
            // btnExit
            this.btnExit.Location = new System.Drawing.Point(80, 270);
            this.btnExit.Size = new System.Drawing.Size(240, 40);
            this.btnExit.Text = "Exit";
            this.btnExit.Click += new EventHandler(this.btnExit_Click);

            // ✅ SETTINGS BUTTON - ADDED THIS WHOLE SECTION
            this.btnSettings.Location = new System.Drawing.Point(350, 12);
            this.btnSettings.Size = new System.Drawing.Size(30, 30);
            this.btnSettings.Text = "⚙️";
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI Emoji", 10F);
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
            this.Controls.Add(this.btnImportExport);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnSettings); // ✅ ADDED THIS LINE
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }

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

        private void btnImportExport_Click(object sender, EventArgs e)
        {
            var importExportForm = new ImportExportForm();
            importExportForm.ShowDialog();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit CostChef?", 
                "Exit Confirmation", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }

    // Data classes
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public int BatchYield { get; set; } = 1;
        public decimal TargetFoodCostPercentage { get; set; } = 0.3m;
        public List<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    }

    public class RecipeIngredient
    {
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        
        // Display properties (not stored in database)
        public string IngredientName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal LineCost => Quantity * UnitPrice;
    }
}