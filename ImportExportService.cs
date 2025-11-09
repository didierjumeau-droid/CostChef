using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CsvHelper;
using System.Globalization;

namespace CostChef
{
    public static class ImportExportService
    {
        public static void ExportIngredientsToCsv(string filePath, List<Ingredient> ingredients)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ingredients);
            }
        }

        public static List<Ingredient> ImportIngredientsFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<Ingredient>().ToList();
            }
        }

        public static void ExportIngredientsToJson(string filePath, List<Ingredient> ingredients)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(ingredients, options);
            File.WriteAllText(filePath, json);
        }

        public static List<Ingredient> ImportIngredientsFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Ingredient>>(json) ?? new List<Ingredient>();
        }

        public static void ExportRecipesToJson(string filePath, List<Recipe> recipes)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(recipes, options);
            File.WriteAllText(filePath, json);
        }

        public static List<Recipe> ImportRecipesFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Recipe>>(json) ?? new List<Recipe>();
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

        public static void ExportSuppliersToJson(string filePath, List<Supplier> suppliers)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(suppliers, options);
            File.WriteAllText(filePath, json);
        }

        public static List<Supplier> ImportSuppliersFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Supplier>>(json) ?? new List<Supplier>();
        }
    }
}