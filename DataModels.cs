using System;
using System.Collections.Generic;
using System.Linq;

namespace CostChef
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public string Category { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
    }

    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public int BatchYield { get; set; } = 1;
        public decimal TargetFoodCostPercentage { get; set; } = 30.0m;
        
        // Computed properties
        public List<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
        public decimal TotalCost => Ingredients.Sum(i => i.LineCost);
        public decimal CostPerServing => BatchYield > 0 ? TotalCost / BatchYield : 0;
        public decimal FoodCostPercentage => CostPerServing > 0 ? (CostPerServing / CostPerServing) * 100 : 0;
    }

    public class RecipeIngredient
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public string IngredientName { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public string Supplier { get; set; }
        public decimal LineCost => UnitPrice * Quantity;
    }

    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string CreatedAt { get; set; }
    }
}