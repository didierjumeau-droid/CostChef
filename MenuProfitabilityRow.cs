using System;

namespace CostChef
{
    /// <summary>
    /// One row in the Menu Profitability Dashboard.
    /// All values are per serving, unless stated otherwise.
    /// Percentages are stored as fractions (0.35 = 35%).
    /// </summary>
    public class MenuProfitabilityRow
    {
        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string Category { get; set; }

        public decimal TotalCost { get; set; }          // Total cost for the batch
        public int BatchYield { get; set; }             // Number of portions the batch yields

        public decimal CostPerServing { get; set; }     // TotalCost / BatchYield
        public decimal SalesPrice { get; set; }         // Selling price per serving

        public decimal GrossProfitPerServing { get; set; }   // SalesPrice - CostPerServing

        public decimal FoodCostPercentage { get; set; }      // CostPerServing / SalesPrice (0..1)
        public decimal TargetFoodCostPercentage { get; set; }// recipes.target_food_cost_percentage (0..1)
        public decimal VarianceFoodCostPercentage { get; set;}// FoodCost% - TargetFoodCost%

        public decimal MarginPercentage { get; set; }        // GrossProfit / SalesPrice (0..1)

        public string Status { get; set; }                   // e.g. "Above Target", "On Target", etc.
    }
}
