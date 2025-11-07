using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;

namespace CostChef
{
    public class ImportExportService
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private string currencySymbol = AppSettings.CurrencySymbol;

        public ImportExportService()
        {
            _jsonOptions = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // CSV Export Methods
        public bool ExportIngredientsToCsv(List<Ingredient> ingredients, string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
                {
                    csv.WriteField("Name");
                    csv.WriteField("Unit");
                    csv.WriteField("Price");
                    csv.WriteField("Category");
                    csv.NextRecord();

                    foreach (var ingredient in ingredients)
                    {
                        csv.WriteField(ingredient.Name);
                        csv.WriteField(ingredient.Unit);
                        csv.WriteField(ingredient.UnitPrice);
                        csv.WriteField(ingredient.Category ?? "");
                        csv.NextRecord();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting ingredients: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool ExportRecipeToCsv(Recipe recipe, string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
                {
                    // Recipe header
                    csv.WriteField("Recipe");
                    csv.WriteField(recipe.Name);
                    csv.NextRecord();

                    csv.WriteField("Description");
                    csv.WriteField(recipe.Description ?? "");
                    csv.NextRecord();

                    csv.WriteField("Category");
                    csv.WriteField(recipe.Category ?? "");
                    csv.NextRecord();

                    csv.WriteField("Tags");
                    csv.WriteField(string.Join(",", recipe.Tags ?? new List<string>()));
                    csv.NextRecord();

                    csv.WriteField("Batch Yield");
                    csv.WriteField(recipe.BatchYield);
                    csv.NextRecord();

                    csv.WriteField("Target Food Cost %");
                    csv.WriteField(recipe.TargetFoodCostPercentage * 100);
                    csv.NextRecord();

                    // Empty line
                    csv.NextRecord();

                    // Ingredients header
                    csv.WriteField("Line");
                    csv.WriteField("Ingredient");
                    csv.WriteField("Unit");
                    csv.WriteField("Quantity");
                    csv.WriteField($"Cost/Unit ({currencySymbol})");
                    csv.WriteField($"Line Cost ({currencySymbol})");
                    csv.NextRecord();

                    // Ingredients
                    if (recipe.Ingredients != null)
                    {
                        int lineNumber = 1;
                        foreach (var ingredient in recipe.Ingredients)
                        {
                            csv.WriteField(lineNumber);
                            csv.WriteField(ingredient.IngredientName);
                            csv.WriteField(ingredient.Unit);
                            csv.WriteField(ingredient.Quantity);
                            csv.WriteField(ingredient.UnitPrice);
                            csv.WriteField(ingredient.LineCost);
                            csv.NextRecord();
                            lineNumber++;
                        }
                    }

                    // Cost summary
                    csv.NextRecord();
                    decimal totalCost = recipe.Ingredients?.Sum(i => i.LineCost) ?? 0;
                    decimal costPerServing = totalCost / recipe.BatchYield;

                    csv.WriteField("Total Batch Cost");
                    csv.WriteField($"{currencySymbol} {totalCost:F2}");
                    csv.NextRecord();

                    csv.WriteField("Cost per Serving");
                    csv.WriteField($"{currencySymbol} {costPerServing:F2}");
                    csv.NextRecord();

                    // Suggested prices
                    decimal suggestedPrice25 = Math.Round((costPerServing / 0.25m) / 5, 0) * 5;
                    decimal suggestedPrice30 = Math.Round((costPerServing / 0.30m) / 5, 0) * 5;
                    decimal suggestedPrice35 = Math.Round((costPerServing / 0.35m) / 5, 0) * 5;
                    decimal targetPrice = Math.Round((costPerServing / recipe.TargetFoodCostPercentage) / 5, 0) * 5;

                    csv.WriteField($"Suggested Price @25%");
                    csv.WriteField($"{currencySymbol} {suggestedPrice25}");
                    csv.NextRecord();

                    csv.WriteField($"Suggested Price @30%");
                    csv.WriteField($"{currencySymbol} {suggestedPrice30}");
                    csv.NextRecord();

                    csv.WriteField($"Suggested Price @35%");
                    csv.WriteField($"{currencySymbol} {suggestedPrice35}");
                    csv.NextRecord();

                    csv.WriteField($"Target Price @{recipe.TargetFoodCostPercentage:P0}");
                    csv.WriteField($"{currencySymbol} {targetPrice}");
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting recipe: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // CSV Import Methods
        public List<Ingredient> ImportIngredientsFromCsv(string filePath)
        {
            var ingredients = new List<Ingredient>();
            
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        try
                        {
                            var ingredient = new Ingredient
                            {
                                Name = csv.GetField("Name") ?? "",
                                Unit = csv.GetField("Unit") ?? "",
                                UnitPrice = (decimal)csv.GetField<double>("Price"),
                                Category = csv.GetField("Category") ?? ""
                            };

                            if (!string.IsNullOrEmpty(ingredient.Name))
                            {
                                ingredients.Add(ingredient);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error parsing ingredient row: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing ingredients: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return ingredients;
        }

        public List<Recipe> ImportRecipeFromCsv(string filePath)
        {
            var recipes = new List<Recipe>();
            
            try
            {
                var lines = File.ReadAllLines(filePath);
                Recipe currentRecipe = null;
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var fields = line.Split(',');
                    
                    // Skip empty lines
                    if (fields.All(f => string.IsNullOrWhiteSpace(f)))
                        continue;

                    // Look for recipe header
                    if (fields.Length > 1 && fields[0].Trim() == "Recipe")
                    {
                        // Save previous recipe if exists
                        if (currentRecipe != null)
                        {
                            recipes.Add(currentRecipe);
                        }
                        
                        // Create new recipe
                        currentRecipe = new Recipe
                        {
                            Name = fields[1].Trim(),
                            BatchYield = 1,
                            TargetFoodCostPercentage = 0.3m,
                            Ingredients = new List<RecipeIngredient>()
                        };
                    }
                    
                    // Look for description
                    if (currentRecipe != null && fields.Length > 1 && fields[0].Trim() == "Description")
                    {
                        currentRecipe.Description = fields[1].Trim();
                    }
                    
                    // Look for category
                    if (currentRecipe != null && fields.Length > 1 && fields[0].Trim() == "Category")
                    {
                        currentRecipe.Category = fields[1].Trim();
                    }
                    
                    // Look for tags
                    if (currentRecipe != null && fields.Length > 1 && fields[0].Trim() == "Tags")
                    {
                        var tagsString = fields[1].Trim();
                        if (!string.IsNullOrEmpty(tagsString))
                        {
                            currentRecipe.Tags = tagsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .ToList();
                        }
                    }
                    
                    // Look for batch yield
                    if (currentRecipe != null && fields.Length > 1 && fields[0].Trim() == "Batch Yield")
                    {
                        if (int.TryParse(fields[1].Trim(), out int yield))
                        {
                            currentRecipe.BatchYield = yield;
                        }
                    }
                    
                    // Look for food cost percentage
                    if (currentRecipe != null && fields.Length > 1 && fields[0].Trim() == "Target Food Cost %")
                    {
                        if (decimal.TryParse(fields[1].Trim(), out decimal foodCostPercent))
                        {
                            currentRecipe.TargetFoodCostPercentage = foodCostPercent / 100m;
                        }
                    }
                    
                    // Look for ingredient lines (they start with numbers)
                    if (currentRecipe != null && fields.Length >= 5 && 
                        int.TryParse(fields[0].Trim(), out int lineNumber))
                    {
                        var ingredientName = fields[1]?.Trim() ?? "";
                        var unit = fields[2]?.Trim() ?? "";
                        var quantity = decimal.TryParse(fields[3], out decimal qty) ? qty : 0;
                        var unitPrice = decimal.TryParse(fields[4], out decimal price) ? price : 0;
                        
                        if (!string.IsNullOrEmpty(ingredientName))
                        {
                            // Look up ingredient by name
                            var existingIngredient = DatabaseContext.GetIngredientByName(ingredientName);
                            
                            if (existingIngredient != null)
                            {
                                var recipeIngredient = new RecipeIngredient
                                {
                                    IngredientId = existingIngredient.Id,
                                    Quantity = quantity,
                                    IngredientName = ingredientName,
                                    Unit = unit,
                                    UnitPrice = unitPrice
                                };
                                
                                currentRecipe.Ingredients.Add(recipeIngredient);
                            }
                            else
                            {
                                // If ingredient doesn't exist, create a new one
                                var newIngredient = new Ingredient
                                {
                                    Name = ingredientName,
                                    Unit = unit,
                                    UnitPrice = unitPrice
                                };
                                
                                DatabaseContext.InsertIngredient(newIngredient);
                                
                                // Get the newly created ingredient with its ID
                                var createdIngredient = DatabaseContext.GetIngredientByName(ingredientName);
                                
                                if (createdIngredient != null)
                                {
                                    var recipeIngredient = new RecipeIngredient
                                    {
                                        IngredientId = createdIngredient.Id,
                                        Quantity = quantity,
                                        IngredientName = ingredientName,
                                        Unit = unit,
                                        UnitPrice = unitPrice
                                    };
                                    
                                    currentRecipe.Ingredients.Add(recipeIngredient);
                                }
                            }
                        }
                    }
                }
                
                // Add the last recipe
                if (currentRecipe != null)
                {
                    recipes.Add(currentRecipe);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing recipes: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return recipes;
        }

        // NEW: Enhanced import with duplicate handling options
        public (List<Recipe> imported, List<string> skipped, List<string> errors) ImportRecipesWithOptions(string filePath, bool overwriteDuplicates = false)
        {
            var imported = new List<Recipe>();
            var skipped = new List<string>();
            var errors = new List<string>();
            
            try
            {
                var recipesFromFile = ImportRecipeFromCsv(filePath);
                var existingRecipes = DatabaseContext.GetAllRecipes();
                
                foreach (var recipe in recipesFromFile)
                {
                    try
                    {
                        // Check if recipe already exists
                        var existingRecipe = existingRecipes.FirstOrDefault(r => 
                            r.Name.Equals(recipe.Name, StringComparison.OrdinalIgnoreCase));
                        
                        if (existingRecipe == null)
                        {
                            // New recipe - insert it
                            DatabaseContext.InsertRecipe(recipe);
                            imported.Add(recipe);
                        }
                        else if (overwriteDuplicates)
                        {
                            // Overwrite existing recipe
                            recipe.Id = existingRecipe.Id;
                            DatabaseContext.UpdateRecipe(recipe);
                            imported.Add(recipe);
                        }
                        else
                        {
                            // Skip duplicate
                            skipped.Add(recipe.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{recipe.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"File error: {ex.Message}");
            }
            
            return (imported, skipped, errors);
        }

        // JSON Methods
        public bool ExportRecipesToJson(object recipes, string filePath = "recipes.json")
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(recipes, _jsonOptions);
                File.WriteAllText(filePath, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting recipes: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool ExportIngredientsToJson(object ingredients, string filePath = "ingredients.json")
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(ingredients, _jsonOptions);
                File.WriteAllText(filePath, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting ingredients: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public T ImportFromJson<T>(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(jsonString, _jsonOptions);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing from {filePath}: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return default(T);
            }
        }
    }
}