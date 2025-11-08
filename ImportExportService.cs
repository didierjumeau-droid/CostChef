using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace CostChef
{
    public static class ImportExportService
    {
        public static bool ExportRecipesToCsv(string filePath)
        {
            try
            {
                var recipes = DatabaseContext.GetAllRecipes();
                using var writer = new StreamWriter(filePath);
                
                // Write header
                writer.WriteLine("ID,Name,Description,Category,Tags,BatchYield,TargetFoodCostPercentage");
                
                // Write data
                foreach (var recipe in recipes)
                {
                    string tags = recipe.Tags != null ? string.Join(",", recipe.Tags) : "";
                    writer.WriteLine($"\"{recipe.Id}\",\"{recipe.Name}\",\"{recipe.Description}\",\"{recipe.Category}\",\"{tags}\",\"{recipe.BatchYield}\",\"{recipe.TargetFoodCostPercentage}\"");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export error: {ex.Message}");
                return false;
            }
        }

        public static bool ExportIngredientsToCsv(string filePath)
        {
            try
            {
                var ingredients = DatabaseContext.GetAllIngredients();
                using var writer = new StreamWriter(filePath);
                
                // Write header
                writer.WriteLine("ID,Name,Unit,UnitPrice,Category,SupplierId,SupplierName");
                
                // Write data
                foreach (var ingredient in ingredients)
                {
                    writer.WriteLine($"\"{ingredient.Id}\",\"{ingredient.Name}\",\"{ingredient.Unit}\",\"{ingredient.UnitPrice}\",\"{ingredient.Category}\",\"{ingredient.SupplierId}\",\"{ingredient.SupplierName}\"");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export error: {ex.Message}");
                return false;
            }
        }

        public static bool ImportRecipesFromCsv(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                int importedCount = 0;
                
                // Skip header (line 0)
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    
                    var fields = ParseCsvLine(lines[i]);
                    if (fields.Length >= 6)
                    {
                        var recipe = new Recipe
                        {
                            Name = fields[1],
                            Description = fields[2],
                            Category = fields[3],
                            BatchYield = int.Parse(fields[5]),
                            TargetFoodCostPercentage = decimal.Parse(fields[6])
                        };
                        
                        // Handle tags
                        if (!string.IsNullOrEmpty(fields[4]))
                        {
                            recipe.Tags = new List<string>(fields[4].Split(',', StringSplitOptions.RemoveEmptyEntries));
                        }
                        
                        DatabaseContext.InsertRecipe(recipe);
                        importedCount++;
                    }
                }
                
                return importedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Import error: {ex.Message}");
                return false;
            }
        }

        public static bool ImportIngredientsFromCsv(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                int importedCount = 0;
                
                // Skip header (line 0)
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    
                    var fields = ParseCsvLine(lines[i]);
                    if (fields.Length >= 5)
                    {
                        var ingredient = new Ingredient
                        {
                            Name = fields[1],
                            Unit = fields[2],
                            UnitPrice = decimal.Parse(fields[3]),
                            Category = fields[4]
                        };
                        
                        // Handle supplier fields if present
                        if (fields.Length >= 7)
                        {
                            if (!string.IsNullOrEmpty(fields[5]) && int.TryParse(fields[5], out int supplierId))
                                ingredient.SupplierId = supplierId;
                            ingredient.SupplierName = fields[6];
                        }
                        
                        DatabaseContext.InsertIngredient(ingredient);
                        importedCount++;
                    }
                }
                
                return importedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Import error: {ex.Message}");
                return false;
            }
        }

        private static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            string currentField = "";
            
            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }
            
            fields.Add(currentField);
            return fields.ToArray();
        }
    }
}