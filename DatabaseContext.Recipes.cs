using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace CostChef
{
    public static partial class DatabaseContext
    {
        // ========== RECIPE METHODS ==========

        public static List<Recipe> GetAllRecipes()
        {
            var recipes = new List<Recipe>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM recipes ORDER BY name";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            recipes.Add(new Recipe
                            {
                                Id = SafeGetInt(reader, "id"),
                                Name = SafeGetString(reader, "name"),
                                Description = SafeGetString(reader, "description"),
                                Category = SafeGetString(reader, "category"),
                                Tags = SafeGetString(reader, "tags"),
                                BatchYield = SafeGetInt(reader, "batch_yield"),
                                TargetFoodCostPercentage = SafeGetDecimal(reader, "target_food_cost_percentage")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recipes: {ex.Message}");
            }

            return recipes ?? new List<Recipe>();
        }

        public static void InsertRecipe(Recipe recipe)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO recipes (name, description, category, tags, batch_yield, target_food_cost_percentage) 
                               VALUES (@name, @description, @category, @tags, @batch_yield, @target_food_cost_percentage)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", recipe.Name);
                    command.Parameters.AddWithValue("@description", recipe.Description ?? "");
                    command.Parameters.AddWithValue("@category", recipe.Category ?? "");
                    command.Parameters.AddWithValue("@tags", recipe.Tags ?? "");
                    command.Parameters.AddWithValue("@batch_yield", recipe.BatchYield);
                    command.Parameters.AddWithValue("@target_food_cost_percentage", recipe.TargetFoodCostPercentage);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateRecipe(Recipe recipe)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"UPDATE recipes 
                               SET name = @name, description = @description, category = @category, 
                                   tags = @tags, batch_yield = @batch_yield, target_food_cost_percentage = @target_food_cost_percentage 
                               WHERE id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", recipe.Name);
                    command.Parameters.AddWithValue("@description", recipe.Description ?? "");
                    command.Parameters.AddWithValue("@category", recipe.Category ?? "");
                    command.Parameters.AddWithValue("@tags", recipe.Tags ?? "");
                    command.Parameters.AddWithValue("@batch_yield", recipe.BatchYield);
                    command.Parameters.AddWithValue("@target_food_cost_percentage", recipe.TargetFoodCostPercentage);
                    command.Parameters.AddWithValue("@id", recipe.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteRecipe(int recipeId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Delete recipe ingredients first
                string deleteRecipeIngredients = "DELETE FROM recipe_ingredients WHERE recipe_id = @recipeId";
                using (var command = new SQLiteCommand(deleteRecipeIngredients, connection))
                {
                    command.Parameters.AddWithValue("@recipeId", recipeId);
                    command.ExecuteNonQuery();
                }

                // Delete the recipe
                string deleteRecipe = "DELETE FROM recipes WHERE id = @id";
                using (var command = new SQLiteCommand(deleteRecipe, connection))
                {
                    command.Parameters.AddWithValue("@id", recipeId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<string> GetRecipeCategories()
        {
            var categories = new List<string>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT category FROM recipes WHERE category IS NOT NULL AND category != '' ORDER BY category";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(SafeGetString(reader, "category"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recipe categories: {ex.Message}");
            }

            return categories ?? new List<string>();
        }

        public static List<RecipeIngredient> GetRecipeIngredients(int recipeId)
        {
            var recipeIngredients = new List<RecipeIngredient>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                    SELECT ri.*, i.name as ingredient_name, i.unit, i.unit_price, s.name as supplier_name
                    FROM recipe_ingredients ri 
                    INNER JOIN ingredients i ON ri.ingredient_id = i.id 
                    LEFT JOIN suppliers s ON i.supplier_id = s.id
                    WHERE ri.recipe_id = @recipeId";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@recipeId", recipeId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var unitPrice = SafeGetDecimal(reader, "unit_price");
                                var quantity = SafeGetDecimal(reader, "quantity");
                                
                                recipeIngredients.Add(new RecipeIngredient
                                {
                                    Id = SafeGetInt(reader, "id"),
                                    RecipeId = SafeGetInt(reader, "recipe_id"),
                                    IngredientId = SafeGetInt(reader, "ingredient_id"),
                                    Quantity = quantity,
                                    IngredientName = SafeGetString(reader, "ingredient_name"),
                                    Unit = SafeGetString(reader, "unit"),
                                    UnitPrice = unitPrice,
                                    Supplier = SafeGetString(reader, "supplier_name")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recipe ingredients: {ex.Message}");
            }

            return recipeIngredients ?? new List<RecipeIngredient>();
        }

        public static void AddRecipeIngredient(RecipeIngredient recipeIngredient)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO recipe_ingredients (recipe_id, ingredient_id, quantity) VALUES (@recipe_id, @ingredient_id, @quantity)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@recipe_id", recipeIngredient.RecipeId);
                    command.Parameters.AddWithValue("@ingredient_id", recipeIngredient.IngredientId);
                    command.Parameters.AddWithValue("@quantity", recipeIngredient.Quantity);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateRecipeIngredient(RecipeIngredient recipeIngredient)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "UPDATE recipe_ingredients SET quantity = @quantity WHERE id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@quantity", recipeIngredient.Quantity);
                    command.Parameters.AddWithValue("@id", recipeIngredient.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteRecipeIngredient(int recipeIngredientId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM recipe_ingredients WHERE id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", recipeIngredientId);
                    command.ExecuteNonQuery();
                }
            }
        }

        // ========== RECIPE VERSIONING METHODS ==========

        public static int GetRecipeVersionCount(int recipeId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM recipe_versions WHERE recipe_id = @recipeId";
                
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@recipeId", recipeId);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
    }
}