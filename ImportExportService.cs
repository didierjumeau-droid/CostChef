using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CsvHelper;
using CsvHelper.Configuration;

namespace CostChef
{
    public static class ImportExportService
    {
        public static void ExportRecipeToCsv(Recipe recipe, string filePath)
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

                // Write recipe header
                csv.WriteField("Recipe:");
                csv.WriteField(recipe.Name);
                csv.NextRecord();

                // Write batch yield
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("Batch yield (servings)");
                csv.WriteField(recipe.BatchYield);
                csv.NextRecord();

                // Write target food cost
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("Target Food Cost %");
                csv.WriteField(recipe.TargetFoodCostPercentage);
                csv.NextRecord();

                // Write loss percentage
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("Loss %");
                csv.WriteField("0");
                csv.NextRecord();

                // Write ingredients header
                csv.WriteField("Line");
                csv.WriteField("Ingredient");
                csv.WriteField("Unit");
                csv.WriteField("Qty");
                csv.WriteField("Cost/Unit (?)");
                csv.WriteField("Line Cost (?)");
                csv.NextRecord();

                // Write ingredients
                int lineNumber = 1;
                decimal totalCost = 0;

                foreach (var ingredient in recipe.Ingredients)
                {
                    var dbIngredient = DatabaseContext.GetAllIngredients()
                        .FirstOrDefault(i => i.Id == ingredient.IngredientId);

                    if (dbIngredient != null)
                    {
                        decimal lineCost = ingredient.Quantity * dbIngredient.UnitPrice;
                        totalCost += lineCost;

                        csv.WriteField(lineNumber);
                        csv.WriteField(dbIngredient.Name);
                        csv.WriteField(dbIngredient.Unit);
                        csv.WriteField(ingredient.Quantity);
                        csv.WriteField(dbIngredient.UnitPrice);
                        csv.WriteField(lineCost);
                        csv.NextRecord();
                        lineNumber++;
                    }
                }

                // Fill remaining lines up to 20
                for (int i = lineNumber; i <= 20; i++)
                {
                    csv.WriteField(i);
                    csv.NextRecord();
                }

                // Write cost summary
                decimal costPerServing = totalCost / recipe.BatchYield;
                decimal suggestedPrice = Math.Round((costPerServing / recipe.TargetFoodCostPercentage) / 5, 0) * 5;
                decimal price25 = Math.Round((costPerServing / 0.25m) / 5, 0) * 5;
                decimal price30 = Math.Round((costPerServing / 0.30m) / 5, 0) * 5;
                decimal price35 = Math.Round((costPerServing / 0.35m) / 5, 0) * 5;

                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("Total Batch Cost (?)");
                csv.WriteField(totalCost);
                csv.NextRecord();

                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("Cost per Serving (?)");
                csv.WriteField(costPerServing);
                csv.NextRecord();

                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("Suggested Price @FoodCost");
                csv.WriteField(suggestedPrice);
                csv.NextRecord();

                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("Suggested Price @25%");
                csv.WriteField(price25);
                csv.NextRecord();

                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("Suggested Price @30%");
                csv.WriteField(price30);
                csv.NextRecord();

                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("Suggested Price @35%");
                csv.WriteField(price35);
                csv.NextRecord();

                // Write description section
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("Description / Steps");
                csv.NextRecord();

                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("— Write the method here —");
                csv.NextRecord();

                // Write ingredient list for reference
                csv.WriteField("ING_LIST_AUTO");
                csv.NextRecord();

                var allIngredients = DatabaseContext.GetAllIngredients();
                foreach (var ing in allIngredients)
                {
                    csv.WriteField(ing.Name);
                    csv.NextRecord();
                }

                MessageBox.Show($"Recipe exported successfully to {filePath}", "Export Successful", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting recipe: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static Recipe ImportRecipeFromCsv(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

                var recipe = new Recipe();
                var ingredients = new List<RecipeIngredient>();

                // Read recipe name (first row, second column)
                if (csv.Read())
                {
                    csv.Read(); // Skip first column
                    recipe.Name = csv.GetField(1) ?? "Imported Recipe";
                }

                // Read batch yield (second row, eighth column)
                if (csv.Read())
                {
                    for (int i = 0; i < 6; i++) csv.Read(); // Skip to column H
                    if (int.TryParse(csv.GetField(7), out int yield))
                        recipe.BatchYield = yield;
                }

                // Read target food cost (third row, eighth column)
                if (csv.Read())
                {
                    for (int i = 0; i < 6; i++) csv.Read(); // Skip to column H
                    if (decimal.TryParse(csv.GetField(7), out decimal foodCost))
                        recipe.TargetFoodCostPercentage = foodCost;
                }

                // Skip loss percentage row
                csv.Read();

                // Skip ingredients header
                csv.Read();

                // Read ingredients until empty line
                while (csv.Read())
                {
                    var lineField = csv.GetField(0);
                    if (string.IsNullOrEmpty(lineField) || !int.TryParse(lineField, out _))
                        break;

                    var ingredientName = csv.GetField(1);
                    var unit = csv.GetField(2);
                    var quantityStr = csv.GetField(3);

                    if (!string.IsNullOrEmpty(ingredientName) && decimal.TryParse(quantityStr, out decimal quantity))
                    {
                        // Find or create ingredient
                        var ingredient = DatabaseContext.GetIngredientByName(ingredientName);
                        if (ingredient == null)
                        {
                            // Create new ingredient
                            ingredient = new Ingredient 
                            { 
                                Name = ingredientName, 
                                Unit = unit,
                                UnitPrice = 0 // Default price, can be updated later
                            };
                            DatabaseContext.InsertIngredient(ingredient);
                        }

                        ingredients.Add(new RecipeIngredient
                        {
                            IngredientId = ingredient.Id,
                            Quantity = quantity
                        });
                    }
                }

                recipe.Ingredients = ingredients;
                return recipe;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing recipe: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public static void ExportIngredientsToCsv(string filePath)
        {
            try
            {
                var ingredients = DatabaseContext.GetAllIngredients();
                
                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

                // Write header
                csv.WriteField("Name");
                csv.WriteField("Unit");
                csv.WriteField("UnitPrice");
                csv.WriteField("Category");
                csv.NextRecord();

                foreach (var ingredient in ingredients)
                {
                    csv.WriteField(ingredient.Name);
                    csv.WriteField(ingredient.Unit);
                    csv.WriteField(ingredient.UnitPrice);
                    csv.WriteField(ingredient.Category);
                    csv.NextRecord();
                }

                MessageBox.Show($"Ingredients exported successfully to {filePath}", "Export Successful", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting ingredients: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void ImportIngredientsFromCsv(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

                // Skip header
                csv.Read();

                int importedCount = 0;
                int updatedCount = 0;

                while (csv.Read())
                {
                    var name = csv.GetField(0);
                    var unit = csv.GetField(1);
                    var unitPriceStr = csv.GetField(2);
                    var category = csv.GetField(3);

                    if (!string.IsNullOrEmpty(name) && decimal.TryParse(unitPriceStr, out decimal unitPrice))
                    {
                        var existingIngredient = DatabaseContext.GetIngredientByName(name);
                        
                        if (existingIngredient != null)
                        {
                            // Update existing ingredient
                            existingIngredient.Unit = unit;
                            existingIngredient.UnitPrice = unitPrice;
                            existingIngredient.Category = category;
                            DatabaseContext.UpdateIngredient(existingIngredient);
                            updatedCount++;
                        }
                        else
                        {
                            // Create new ingredient
                            var newIngredient = new Ingredient
                            {
                                Name = name,
                                Unit = unit,
                                UnitPrice = unitPrice,
                                Category = category
                            };
                            DatabaseContext.InsertIngredient(newIngredient);
                            importedCount++;
                        }
                    }
                }

                MessageBox.Show($"Ingredients import completed:\n{importedCount} new ingredients imported\n{updatedCount} existing ingredients updated", 
                    "Import Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing ingredients: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}