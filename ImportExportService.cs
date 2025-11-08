using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;

namespace CostChef
{
    public static class ImportExportService
    {
        // UPDATED: Export recipes with proper file dialog
        public static bool ExportRecipesToCsv()
        {
            try
            {
                var recipes = DatabaseContext.GetAllRecipes();
                if (recipes == null || recipes.Count == 0)
                {
                    MessageBox.Show("No recipes found to export.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = "Export Recipes to CSV";
                    saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    saveDialog.FileName = $"recipes_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    saveDialog.InitialDirectory = AppSettings.ExportLocation;
                    saveDialog.OverwritePrompt = true;
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var lines = new List<string>();
                        // Header
                        lines.Add("RecipeName,Category,Tags,BatchYield,TargetFoodCost%,TotalCost,CostPerServing,IngredientsCount");
                        
                        foreach (var recipe in recipes)
                        {
                            decimal totalCost = recipe.Ingredients?.Sum(i => i.LineCost) ?? 0;
                            decimal costPerServing = recipe.BatchYield > 0 ? totalCost / recipe.BatchYield : 0;
                            int ingredientCount = recipe.Ingredients?.Count ?? 0;
                            string tags = recipe.Tags != null ? string.Join(";", recipe.Tags) : "";
                            
                            lines.Add($"\"{recipe.Name}\",\"{recipe.Category}\",\"{tags}\",{recipe.BatchYield},{recipe.TargetFoodCostPercentage:P},{totalCost:F2},{costPerServing:F2},{ingredientCount}");
                        }
                        
                        File.WriteAllLines(saveDialog.FileName, lines);
                        
                        MessageBox.Show($"Successfully exported {recipes.Count} recipes to:{Environment.NewLine}{saveDialog.FileName}", 
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting recipes: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // UPDATED: Export ingredients with proper file dialog
        public static bool ExportIngredientsToCsv()
        {
            try
            {
                var ingredients = DatabaseContext.GetAllIngredients();
                if (ingredients == null || ingredients.Count == 0)
                {
                    MessageBox.Show("No ingredients found to export.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = "Export Ingredients to CSV";
                    saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    saveDialog.FileName = $"ingredients_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    saveDialog.InitialDirectory = AppSettings.ExportLocation;
                    saveDialog.OverwritePrompt = true;
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var lines = new List<string>();
                        // Header
                        lines.Add("Name,Unit,UnitPrice,Category,SupplierName");
                        
                        foreach (var ingredient in ingredients)
                        {
                            lines.Add($"\"{ingredient.Name}\",\"{ingredient.Unit}\",{ingredient.UnitPrice:F4},\"{ingredient.Category}\",\"{ingredient.SupplierName}\"");
                        }
                        
                        File.WriteAllLines(saveDialog.FileName, lines);
                        
                        MessageBox.Show($"Successfully exported {ingredients.Count} ingredients to:{Environment.NewLine}{saveDialog.FileName}", 
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting ingredients: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // UPDATED: Quick export to default location (for automated exports)
        public static bool QuickExportRecipes()
        {
            try
            {
                var recipes = DatabaseContext.GetAllRecipes();
                if (recipes == null || recipes.Count == 0)
                    return false;

                string filePath = AppSettings.GetExportFilePath("recipes_quick_export", "CSV");
                
                var lines = new List<string>();
                lines.Add("RecipeName,Category,Tags,BatchYield,TargetFoodCost%,TotalCost,CostPerServing");
                
                foreach (var recipe in recipes)
                {
                    decimal totalCost = recipe.Ingredients?.Sum(i => i.LineCost) ?? 0;
                    decimal costPerServing = recipe.BatchYield > 0 ? totalCost / recipe.BatchYield : 0;
                    string tags = recipe.Tags != null ? string.Join(";", recipe.Tags) : "";
                    
                    lines.Add($"\"{recipe.Name}\",\"{recipe.Category}\",\"{tags}\",{recipe.BatchYield},{recipe.TargetFoodCostPercentage:P},{totalCost:F2},{costPerServing:F2}");
                }
                
                File.WriteAllLines(filePath, lines);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Quick export failed: {ex.Message}");
                return false;
            }
        }

        // UPDATED: Import recipes with proper file dialog
        public static bool ImportRecipesFromCsv()
        {
            try
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Title = "Import Recipes from CSV";
                    openDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    openDialog.InitialDirectory = AppSettings.ExportLocation;
                    openDialog.CheckFileExists = true;
                    
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Implementation for CSV import would go here
                        MessageBox.Show("CSV import functionality would be implemented here.", "Info", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing recipes: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // JSON export methods (similar updates)
        public static bool ExportRecipesToJson()
        {
            try
            {
                var recipes = DatabaseContext.GetAllRecipes();
                if (recipes == null || recipes.Count == 0)
                {
                    MessageBox.Show("No recipes found to export.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = "Export Recipes to JSON";
                    saveDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveDialog.FileName = $"recipes_export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    saveDialog.InitialDirectory = AppSettings.ExportLocation;
                    saveDialog.OverwritePrompt = true;
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string json = JsonSerializer.Serialize(recipes, options);
                        File.WriteAllText(saveDialog.FileName, json);
                        
                        MessageBox.Show($"Successfully exported {recipes.Count} recipes to JSON.", 
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting recipes to JSON: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool ExportIngredientsToJson()
        {
            try
            {
                var ingredients = DatabaseContext.GetAllIngredients();
                if (ingredients == null || ingredients.Count == 0)
                {
                    MessageBox.Show("No ingredients found to export.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = "Export Ingredients to JSON";
                    saveDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                    saveDialog.FileName = $"ingredients_export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    saveDialog.InitialDirectory = AppSettings.ExportLocation;
                    saveDialog.OverwritePrompt = true;
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string json = JsonSerializer.Serialize(ingredients, options);
                        File.WriteAllText(saveDialog.FileName, json);
                        
                        MessageBox.Show($"Successfully exported {ingredients.Count} ingredients to JSON.", 
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting ingredients to JSON: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}