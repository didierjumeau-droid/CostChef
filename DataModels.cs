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
        public decimal YieldPercentage { get; set; } = 1.0m;
    }

    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public int BatchYield { get; set; } = 1;
        public decimal TargetFoodCostPercentage { get; set; } = 0.30m; 
        
        // FIX: SalesPrice must be defined as it's used in RecipeProfitabilityForm.cs
        public decimal SalesPrice { get; set; } = 0.0m; 
        
        // Computed properties
        public List<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
        public decimal TotalCost => Ingredients.Sum(i => i.LineCost);
        public decimal CostPerServing => BatchYield > 0 ? TotalCost / BatchYield : 0;
        
        // FIX: Added computed properties used in RecipeProfitabilityForm.cs
        public decimal ActualFoodCostPercentage => SalesPrice > 0 ? (CostPerServing / SalesPrice) : 0;
        public decimal ProfitPerServing => SalesPrice - CostPerServing;
        public decimal ProfitMargin => SalesPrice > 0 ? (ProfitPerServing / SalesPrice) : 0;
        
        public decimal FoodCostPercentage => ActualFoodCostPercentage * 100; // For legacy use
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
        public decimal YieldPercentage { get; set; } = 1.0m;
        public decimal LineCost => UnitPrice * Quantity / (YieldPercentage > 0 ? YieldPercentage : 1.0m);
        public decimal BaseLineCost => UnitPrice * Quantity;
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
    
    // Inventory and Utility Models (included for completeness)
    public class InventoryLevel
    {
        public int Id { get; set; }
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string Unit { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal? MinimumStock { get; set; }
        public decimal? MaximumStock { get; set; }
        public DateTime LastUpdated { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue => CurrentStock * UnitCost;
        public bool IsLowStock => MinimumStock.HasValue && CurrentStock <= MinimumStock.Value;
        public bool IsOverstocked => MaximumStock.HasValue && CurrentStock >= MaximumStock.Value;
        public string Status => IsLowStock ? "Low Stock" : IsOverstocked ? "Overstocked" : "OK";
    }

    public class InventoryHistory
    {
        public int Id { get; set; }
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public decimal PreviousStock { get; set; }
        public decimal NewStock { get; set; }
        public decimal ChangeAmount { get; set; }
        public string ChangeType { get; set; }
        public DateTime ChangeDate { get; set; }
        public string Reason { get; set; }
        public int? RecipeId { get; set; }
        public decimal UnitCost { get; set; }
        public decimal ValueChange => ChangeAmount * UnitCost; 
    }

    public class InventorySnapshot
    {
        public int Id { get; set; }
        public DateTime SnapshotDate { get; set; }
        public decimal TotalValue { get; set; }
        public int IngredientCount { get; set; }
    }

    public class RecipeDisplayData
    {
        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public decimal TotalCost { get; set; }
        public decimal CostPerServing { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal ProfitPerServing { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal FoodCostPercentage { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
    }

    public class SafeImportResult 
    {
        public int Imported { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public int NewIngredients { get; set; }
        public int NewSuppliers { get; set; }
        public List<string> DuplicateNames { get; set; } = new List<string>();
        public List<Ingredient> IngredientsToImport { get; set; } = new List<Ingredient>();
        public List<Supplier> SuppliersToImport { get; set; } = new List<Supplier>();
    }
}