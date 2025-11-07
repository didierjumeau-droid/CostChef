using System;
using System.Windows.Forms;

namespace CostChef
{
    public partial class RestoreIngredientsForm : Form
    {
        private Button btnRestore;
        private Button btnClose;
        private Label lblInfo;

        public RestoreIngredientsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.btnRestore = new Button();
            this.btnClose = new Button();
            this.lblInfo = new Label();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Text = "Restore Ingredients";
            this.StartPosition = FormStartPosition.CenterParent;

            this.lblInfo.Text = "Your ingredients database was reset.\n\nClick 'Restore Ingredients' to add back all the default ingredients with their prices.";
            this.lblInfo.Location = new System.Drawing.Point(20, 20);
            this.lblInfo.Size = new System.Drawing.Size(360, 80);
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.btnRestore.Text = "Restore Ingredients";
            this.btnRestore.Location = new System.Drawing.Point(100, 120);
            this.btnRestore.Size = new System.Drawing.Size(120, 30);
            this.btnRestore.Click += (s, e) => RestoreIngredients();

            this.btnClose.Text = "Close";
            this.btnClose.Location = new System.Drawing.Point(230, 120);
            this.btnClose.Size = new System.Drawing.Size(80, 30);
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblInfo, btnRestore, btnClose });
            this.ResumeLayout(false);
        }

        private void RestoreIngredients()
        {
            try
            {
                // Restore all the default ingredients
                var defaultIngredients = new[]
                {
                    new Ingredient { Name = "All-purpose Flour", Unit = "gram", UnitPrice = 0.3m },
                    new Ingredient { Name = "Bacon", Unit = "gram", UnitPrice = 0.9m },
                    new Ingredient { Name = "Beef Striploin", Unit = "gram", UnitPrice = 1.2m },
                    new Ingredient { Name = "Black Pepper", Unit = "gram", UnitPrice = 1.5m },
                    new Ingredient { Name = "Breadcrumbs", Unit = "gram", UnitPrice = 0.35m },
                    new Ingredient { Name = "Brown Sugar", Unit = "gram", UnitPrice = 0.1m },
                    new Ingredient { Name = "Butter", Unit = "gram", UnitPrice = 1.2m },
                    new Ingredient { Name = "Cabbage", Unit = "gram", UnitPrice = 0.2m },
                    new Ingredient { Name = "Calamansi", Unit = "piece", UnitPrice = 2.5m },
                    new Ingredient { Name = "Carrot", Unit = "piece", UnitPrice = 10m },
                    new Ingredient { Name = "Cheddar Cheese", Unit = "gram", UnitPrice = 0.9m },
                    new Ingredient { Name = "Cheese Sauce", Unit = "tablespoon", UnitPrice = 5m },
                    new Ingredient { Name = "Chicken Breast", Unit = "gram", UnitPrice = 0.32m },
                    new Ingredient { Name = "Chicken Stock (house)", Unit = "ml", UnitPrice = 0.2m },
                    new Ingredient { Name = "Chicken Thigh", Unit = "gram", UnitPrice = 0.4m },
                    new Ingredient { Name = "Cilantro", Unit = "gram", UnitPrice = 1m },
                    new Ingredient { Name = "Condensed Milk (can)", Unit = "can (390 g)", UnitPrice = 60m },
                    new Ingredient { Name = "Cooking Oil", Unit = "ml", UnitPrice = 0.12m },
                    new Ingredient { Name = "Egg", Unit = "piece", UnitPrice = 10m },
                    new Ingredient { Name = "Egg Yolk", Unit = "piece", UnitPrice = 6.5m },
                    new Ingredient { Name = "Evaporated Milk (can)", Unit = "can (370 ml)", UnitPrice = 60m },
                    new Ingredient { Name = "Fish Sauce (Patis)", Unit = "tablespoon", UnitPrice = 1.2m },
                    new Ingredient { Name = "French Fries (prepped, frozen)", Unit = "gram", UnitPrice = 0.17m },
                    new Ingredient { Name = "Fresh Milk", Unit = "ml", UnitPrice = 0.12m },
                    new Ingredient { Name = "Garlic", Unit = "clove", UnitPrice = 2m },
                    new Ingredient { Name = "Green Chili", Unit = "piece", UnitPrice = 3m },
                    new Ingredient { Name = "Ground Beef 80/20", Unit = "gram", UnitPrice = 0.5m },
                    new Ingredient { Name = "Hamburger Bun", Unit = "piece", UnitPrice = 5m },
                    new Ingredient { Name = "Jasmine Rice (raw)", Unit = "gram", UnitPrice = 0.06m },
                    new Ingredient { Name = "Ketchup", Unit = "tablespoon", UnitPrice = 1.5m },
                    new Ingredient { Name = "Lemon", Unit = "piece", UnitPrice = 25m },
                    new Ingredient { Name = "Lettuce", Unit = "leaf", UnitPrice = 4m },
                    new Ingredient { Name = "Mahi-Mahi (Dorado)", Unit = "gram", UnitPrice = 0.75m },
                    new Ingredient { Name = "Mayonnaise (house)", Unit = "tablespoon", UnitPrice = 7m },
                    new Ingredient { Name = "Mozzarella", Unit = "gram", UnitPrice = 0.652m },
                    new Ingredient { Name = "Olive Oil", Unit = "ml", UnitPrice = 0.4m },
                    new Ingredient { Name = "Onion", Unit = "piece", UnitPrice = 10m },
                    new Ingredient { Name = "Oyster Sauce", Unit = "tablespoon", UnitPrice = 1.8m },
                    new Ingredient { Name = "Pancit Canton Noodles", Unit = "gram", UnitPrice = 0.2m },
                    new Ingredient { Name = "Parmesan", Unit = "gram", UnitPrice = 1m },
                    new Ingredient { Name = "Pasta Fettuccine", Unit = "gram", UnitPrice = 0.13m },
                    new Ingredient { Name = "Pasta Spaghetti", Unit = "gram", UnitPrice = 0.13m },
                    new Ingredient { Name = "Pickles", Unit = "slice", UnitPrice = 1.5m },
                    new Ingredient { Name = "Pumpkin", Unit = "gram", UnitPrice = 0.15m },
                    new Ingredient { Name = "Salsa (house)", Unit = "tablespoon", UnitPrice = 4m },
                    new Ingredient { Name = "Salt", Unit = "gram", UnitPrice = 0.03m },
                    new Ingredient { Name = "Shrimp (medium)", Unit = "piece", UnitPrice = 7m },
                    new Ingredient { Name = "Sinigang Mix (Tamarind)", Unit = "tablespoon", UnitPrice = 3m },
                    new Ingredient { Name = "Soy Sauce", Unit = "tablespoon", UnitPrice = 1m },
                    new Ingredient { Name = "Spinach", Unit = "gram", UnitPrice = 0.6m },
                    new Ingredient { Name = "Tempura Flour", Unit = "gram", UnitPrice = 0.5m },
                    new Ingredient { Name = "Tomato Paste", Unit = "tablespoon", UnitPrice = 2m },
                    new Ingredient { Name = "Tomato Sauce", Unit = "ml", UnitPrice = 0.2m },
                    new Ingredient { Name = "Tortilla (8-inch)", Unit = "piece", UnitPrice = 12m },
                    new Ingredient { Name = "Tortilla Chips", Unit = "gram", UnitPrice = 0.4m },
                    new Ingredient { Name = "Tuna Loin", Unit = "gram", UnitPrice = 0.7m },
                    new Ingredient { Name = "Vanilla Extract", Unit = "teaspoon", UnitPrice = 10m },
                    new Ingredient { Name = "White Sugar", Unit = "gram", UnitPrice = 0.08m },
                    new Ingredient { Name = "Yellow Mustard", Unit = "teaspoon", UnitPrice = 1m },
                    new Ingredient { Name = "mushrooms", Unit = "piece", UnitPrice = 2.5m }
                };

                int count = 0;
                foreach (var ingredient in defaultIngredients)
                {
                    DatabaseContext.InsertIngredient(ingredient);
                    count++;
                }

                MessageBox.Show($"Successfully restored {count} ingredients!", "Restore Complete", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}