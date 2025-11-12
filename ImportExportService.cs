using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Windows.Forms;

namespace CostChef
{
    public static class ImportExportService
    {
        public static void ExportIngredientsToCsv(string filePath, List<Ingredient> ingredients)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Use mapping for consistent export
                csv.Context.RegisterClassMap<IngredientMap>();
                csv.WriteRecords(ingredients);
            }
        }

        public static List<Ingredient> ImportIngredientsFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Configure CSV reader to handle mapping
                csv.Context.RegisterClassMap<IngredientMap>();
                
                var records = csv.GetRecords<Ingredient>().ToList();
                return records;
            }
        }

        public static void ExportRecipesToCsv(string filePath, List<Recipe> recipes)
        {
            // For simplicity, we'll export basic recipe info
            var exportData = recipes.Select(r => new
            {
                r.Name,
                r.Description,
                r.Category,
                r.Tags,
                r.BatchYield,
                r.TargetFoodCostPercentage
            });

            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(exportData);
            }
        }

        public static List<Recipe> ImportRecipesFromCsv(string filePath)
        {
            // This is a simplified import - you might want to enhance this
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<dynamic>().ToList();
                var recipes = new List<Recipe>();

                foreach (var record in records)
                {
                    recipes.Add(new Recipe
                    {
                        Name = record.Name,
                        Description = record.Description,
                        Category = record.Category,
                        Tags = record.Tags,
                        BatchYield = int.TryParse(record.BatchYield?.ToString(), out int yield) ? yield : 1,
                        TargetFoodCostPercentage = decimal.TryParse(record.TargetFoodCostPercentage?.ToString(), out decimal cost) ? cost : 30.0m
                    });
                }

                return recipes;
            }
        }

        public static void ExportSuppliersToCsv(string filePath, List<Supplier> suppliers)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(suppliers);
            }
        }

        public static List<Supplier> ImportSuppliersFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<Supplier>().ToList();
            }
        }

        // CSV mapping class for Ingredient
        private sealed class IngredientMap : ClassMap<Ingredient>
        {
            public IngredientMap()
            {
                Map(m => m.Id).Name("ID").Optional();
                Map(m => m.Name).Name("Name");
                Map(m => m.Unit).Name("Unit");
                Map(m => m.UnitPrice).Name("UnitPrice");
                Map(m => m.Category).Name("Category");
                Map(m => m.SupplierId).Name("SupplierID").Optional(); // Map to "SupplierID" in CSV
                // SupplierName column will be automatically ignored since it's not in the model
            }
        }

        // NEW: Import with duplicate handling
        public static (int imported, int updated, int skipped) ImportIngredientsWithDuplicateHandling(string filePath)
        {
            var importedIngredients = ImportIngredientsFromCsv(filePath);
            int importedCount = 0;
            int updatedCount = 0;
            int skippedCount = 0;

            // Get all existing ingredients for duplicate checking
            var existingIngredients = DatabaseContext.GetAllIngredients();
            var existingNames = existingIngredients.Select(i => i.Name.ToLowerInvariant()).ToHashSet();

            foreach (var ingredient in importedIngredients)
            {
                // Check if ingredient already exists (case-insensitive)
                bool exists = existingNames.Contains(ingredient.Name.ToLowerInvariant());
                
                if (exists)
                {
                    // Update existing ingredient
                    var existingIngredient = existingIngredients.First(i => 
                        i.Name.Equals(ingredient.Name, StringComparison.OrdinalIgnoreCase));
                    
                    // Update the existing ingredient with new data
                    existingIngredient.Unit = ingredient.Unit;
                    existingIngredient.UnitPrice = ingredient.UnitPrice;
                    existingIngredient.Category = ingredient.Category;
                    existingIngredient.SupplierId = ingredient.SupplierId;
                    
                    DatabaseContext.UpdateIngredient(existingIngredient);
                    updatedCount++;
                }
                else
                {
                    // Insert new ingredient
                    DatabaseContext.InsertIngredient(ingredient);
                    importedCount++;
                }
            }

            return (importedCount, updatedCount, skippedCount);
        }

        // NEW: Import with user choice for duplicates
        public static (int imported, int updated, int skipped) ImportIngredientsWithUserChoice(string filePath, Func<string, DialogResult> duplicateHandler)
        {
            var importedIngredients = ImportIngredientsFromCsv(filePath);
            int importedCount = 0;
            int updatedCount = 0;
            int skippedCount = 0;

            // Get all existing ingredients for duplicate checking
            var existingIngredients = DatabaseContext.GetAllIngredients();
            var existingNames = existingIngredients.Select(i => i.Name.ToLowerInvariant()).ToHashSet();

            foreach (var ingredient in importedIngredients)
            {
                // Check if ingredient already exists (case-insensitive)
                bool exists = existingNames.Contains(ingredient.Name.ToLowerInvariant());
                
                if (exists)
                {
                    // Ask user what to do with duplicate
                    var result = duplicateHandler?.Invoke(ingredient.Name);
                    
                    switch (result)
                    {
                        case DialogResult.Yes: // Update
                            var existingIngredient = existingIngredients.First(i => 
                                i.Name.Equals(ingredient.Name, StringComparison.OrdinalIgnoreCase));
                            
                            existingIngredient.Unit = ingredient.Unit;
                            existingIngredient.UnitPrice = ingredient.UnitPrice;
                            existingIngredient.Category = ingredient.Category;
                            existingIngredient.SupplierId = ingredient.SupplierId;
                            
                            DatabaseContext.UpdateIngredient(existingIngredient);
                            updatedCount++;
                            break;
                            
                        case DialogResult.No: // Skip
                            skippedCount++;
                            break;
                            
                        case DialogResult.Cancel: // Cancel entire import
                            return (importedCount, updatedCount, skippedCount);
                            
                        default: // Default to skip
                            skippedCount++;
                            break;
                    }
                }
                else
                {
                    // Insert new ingredient
                    DatabaseContext.InsertIngredient(ingredient);
                    importedCount++;
                }
            }

            return (importedCount, updatedCount, skippedCount);
        }

        // NEW: Basic duplicate handling (skip only)
        public static void ImportIngredientsWithBasicDuplicateHandling(string filePath)
        {
            try
            {
                var importedIngredients = ImportIngredientsFromCsv(filePath);
                var existingIngredients = DatabaseContext.GetAllIngredients();
                var existingNames = existingIngredients.Select(i => i.Name.ToLowerInvariant()).ToHashSet();

                foreach (var ingredient in importedIngredients)
                {
                    if (existingNames.Contains(ingredient.Name.ToLowerInvariant()))
                    {
                        // Skip duplicate
                        continue;
                    }
                    else
                    {
                        // Insert new ingredient
                        DatabaseContext.InsertIngredient(ingredient);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error importing ingredients: {ex.Message}", ex);
            }
        }
    }
}