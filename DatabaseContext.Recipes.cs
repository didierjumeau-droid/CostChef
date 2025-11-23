using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Diagnostics;

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
                Debug.WriteLine("=== GET ALL RECIPES STARTED ===");
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            id,
                            name,
                            description,
                            category,
                            tags,
                            batch_yield,
                            target_food_cost_percentage,
                            COALESCE(sales_price, 0) AS sales_price
                        FROM recipes
                        ORDER BY name;
                    ";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var rawTarget = SafeGetDecimal(reader, "target_food_cost_percentage");

                            // Backward-compat: old DBs may store 30 instead of 0.30
                            decimal targetFoodCost;
                            if (rawTarget > 1m)
                                targetFoodCost = rawTarget / 100m;
                            else if (rawTarget <= 0m)
                                targetFoodCost = 0.30m;
                            else
                                targetFoodCost = rawTarget;

                            recipes.Add(new Recipe
                            {
                                Id = SafeGetInt(reader, "id"),
                                Name = SafeGetString(reader, "name"),
                                Description = SafeGetString(reader, "description"),
                                Category = SafeGetString(reader, "category"),
                                Tags = SafeGetString(reader, "tags"),
                                BatchYield = SafeGetInt(reader, "batch_yield"),
                                TargetFoodCostPercentage = targetFoodCost,
                                SalesPrice = SafeGetDecimal(reader, "sales_price")
                            });
                        }
                    }
                }

                Debug.WriteLine($"Found {recipes.Count} recipes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading recipes: {ex.Message}");
            }

            // Load ingredients for each recipe in a second pass
            foreach (var recipe in recipes)
            {
                recipe.Ingredients = GetRecipeIngredients(recipe.Id);
                Debug.WriteLine($"Recipe '{recipe.Name}' has {recipe.Ingredients.Count} ingredients");
            }

            Debug.WriteLine("=== GET ALL RECIPES COMPLETED ===");
            return recipes;
        }

        public static Recipe GetRecipeById(int id)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            id,
                            name,
                            description,
                            category,
                            tags,
                            batch_yield,
                            target_food_cost_percentage,
                            COALESCE(sales_price, 0) AS sales_price
                        FROM recipes
                        WHERE id = @id;
                    ";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var rawTarget = SafeGetDecimal(reader, "target_food_cost_percentage");

                                decimal targetFoodCost;
                                if (rawTarget > 1m)
                                    targetFoodCost = rawTarget / 100m;
                                else if (rawTarget <= 0m)
                                    targetFoodCost = 0.30m;
                                else
                                    targetFoodCost = rawTarget;

                                var recipe = new Recipe
                                {
                                    Id = SafeGetInt(reader, "id"),
                                    Name = SafeGetString(reader, "name"),
                                    Description = SafeGetString(reader, "description"),
                                    Category = SafeGetString(reader, "category"),
                                    Tags = SafeGetString(reader, "tags"),
                                    BatchYield = SafeGetInt(reader, "batch_yield"),
                                    TargetFoodCostPercentage = targetFoodCost,
                                    SalesPrice = SafeGetDecimal(reader, "sales_price")
                                };

                                recipe.Ingredients = GetRecipeIngredients(recipe.Id);
                                Debug.WriteLine($"Loaded recipe id {id}: '{recipe.Name}' with {recipe.Ingredients.Count} ingredients");
                                return recipe;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading recipe by id {id}: {ex.Message}");
            }

            return null;
        }

        public static int InsertRecipe(Recipe recipe)
        {
            int newId = 0;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO recipes
                        (name, description, category, tags, batch_yield, target_food_cost_percentage, sales_price)
                    VALUES
                        (@name, @description, @category, @tags, @batch_yield, @target_food_cost_percentage, @sales_price);
                    SELECT last_insert_rowid();
                ";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", recipe.Name);
                    command.Parameters.AddWithValue("@description", recipe.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@category", recipe.Category ?? string.Empty);
                    command.Parameters.AddWithValue("@tags", recipe.Tags ?? string.Empty);
                    command.Parameters.AddWithValue("@batch_yield", recipe.BatchYield);
                    command.Parameters.AddWithValue("@target_food_cost_percentage", recipe.TargetFoodCostPercentage);
                    command.Parameters.AddWithValue("@sales_price", recipe.SalesPrice);

                    newId = Convert.ToInt32(command.ExecuteScalar());
                    Debug.WriteLine($"Inserted recipe '{recipe.Name}' with ID: {newId}");
                }

                // Persist ingredients
                UpdateRecipeIngredients(newId, recipe.Ingredients ?? new List<RecipeIngredient>());
            }

            return newId;
        }

        public static void UpdateRecipe(Recipe recipe)
        {
            Debug.WriteLine($"Updating recipe ID: {recipe.Id}, Name: {recipe.Name}");

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    UPDATE recipes SET
                        name = @name,
                        description = @description,
                        category = @category,
                        tags = @tags,
                        batch_yield = @batch_yield,
                        target_food_cost_percentage = @target_food_cost_percentage,
                        sales_price = @sales_price
                    WHERE id = @id;
                ";

            using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", recipe.Name);
                    command.Parameters.AddWithValue("@description", recipe.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@category", recipe.Category ?? string.Empty);
                    command.Parameters.AddWithValue("@tags", recipe.Tags ?? string.Empty);
                    command.Parameters.AddWithValue("@batch_yield", recipe.BatchYield);
                    command.Parameters.AddWithValue("@target_food_cost_percentage", recipe.TargetFoodCostPercentage);
                    command.Parameters.AddWithValue("@sales_price", recipe.SalesPrice);
                    command.Parameters.AddWithValue("@id", recipe.Id);

                    int rows = command.ExecuteNonQuery();
                    Debug.WriteLine($"Update recipe rows affected: {rows}");
                }

                UpdateRecipeIngredients(recipe.Id, recipe.Ingredients ?? new List<RecipeIngredient>());
            }
        }

        public static void DeleteRecipe(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // First delete ingredients
                string deleteIngredients = "DELETE FROM recipe_ingredients WHERE recipe_id = @id;";
                using (var command = new SQLiteCommand(deleteIngredients, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }

                // TODO: optionally delete recipe_versions rows here as well

                string deleteRecipe = "DELETE FROM recipes WHERE id = @id;";
                using (var command = new SQLiteCommand(deleteRecipe, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<RecipeIngredient> GetRecipeIngredients(int recipeId)
        {
            var ingredients = new List<RecipeIngredient>();

            try
            {
                Debug.WriteLine($"Getting ingredients for recipe ID: {recipeId}");

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            ri.id,
                            ri.recipe_id,
                            ri.ingredient_id,
                            ri.quantity,
                            i.name AS ingredient_name,
                            i.unit,
                            i.unit_price,
                            s.name AS supplier_name
                        FROM recipe_ingredients ri
                        JOIN ingredients i ON ri.ingredient_id = i.id
                        LEFT JOIN suppliers s ON i.supplier_id = s.id
                        WHERE ri.recipe_id = @recipeId;
                    ";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@recipeId", recipeId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var yield = SafeGetDecimal(reader, "yield_percentage");
                                if (yield <= 0m) yield = 1.0m;

                                ingredients.Add(new RecipeIngredient
                                {
                                    Id = SafeGetInt(reader, "id"),
                                    RecipeId = SafeGetInt(reader, "recipe_id"),
                                    IngredientId = SafeGetInt(reader, "ingredient_id"),
                                    Quantity = SafeGetDecimal(reader, "quantity"),
                                    IngredientName = SafeGetString(reader, "ingredient_name"),
                                    Unit = SafeGetString(reader, "unit"),
                                    UnitPrice = SafeGetDecimal(reader, "unit_price"),
                                    Supplier = SafeGetString(reader, "supplier_name"),
                                    YieldPercentage = yield
                                });
                            }
                        }
                    }
                }

                Debug.WriteLine($"Found {ingredients.Count} ingredients for recipe ID: {recipeId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading recipe ingredients: {ex.Message}");
            }

            return ingredients;
        }

        public static void UpdateRecipeIngredients(int recipeId, List<RecipeIngredient> ingredients)
        {
            Debug.WriteLine($"Updating ingredients for recipe ID: {recipeId}, Count: {ingredients?.Count ?? 0}");

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string deleteQuery = "DELETE FROM recipe_ingredients WHERE recipe_id = @recipeId;";
                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@recipeId", recipeId);
                    int deletedRows = command.ExecuteNonQuery();
                    Debug.WriteLine($"Deleted {deletedRows} existing recipe ingredients");
                }

                if (ingredients == null || ingredients.Count == 0)
                {
                    return;
                }

                string insertQuery = @"
                    INSERT INTO recipe_ingredients (recipe_id, ingredient_id, quantity)
                    VALUES (@recipe_id, @ingredient_id, @quantity);
                ";

                foreach (var ingredient in ingredients)
                {
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@recipe_id", recipeId);
                        command.Parameters.AddWithValue("@ingredient_id", ingredient.IngredientId);
                        command.Parameters.AddWithValue("@quantity", ingredient.Quantity);
                        command.ExecuteNonQuery();

                        Debug.WriteLine($"Added ingredient ID: {ingredient.IngredientId}, Quantity: {ingredient.Quantity}");
                    }
                }

                Debug.WriteLine($"Finished updating ingredients for recipe ID: {recipeId}");
            }
        }

        public static void DeleteRecipeIngredient(int recipeIngredientId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "DELETE FROM recipe_ingredients WHERE id = @id;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", recipeIngredientId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void AddRecipeIngredient(RecipeIngredient ingredient)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO recipe_ingredients (recipe_id, ingredient_id, quantity)
                    VALUES (@recipe_id, @ingredient_id, @quantity);
                ";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@recipe_id", ingredient.RecipeId);
                    command.Parameters.AddWithValue("@ingredient_id", ingredient.IngredientId);
                    command.Parameters.AddWithValue("@quantity", ingredient.Quantity);
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

                    string query = @"
                        SELECT DISTINCT category
                        FROM recipes
                        WHERE category IS NOT NULL AND category <> ''
                        ORDER BY category;
                    ";

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
                Debug.WriteLine($"Error loading recipe categories: {ex.Message}");
            }

            return categories ?? new List<string>();
        }

        public static List<Recipe> GetRecipesByProfitAmount(decimal minProfit)
        {
            var allRecipes = GetAllRecipes();
            return allRecipes.Where(r => r.ProfitPerServing >= minProfit).ToList();
        }

        public static List<Recipe> GetRecipesByFoodCostPercentage(decimal maxFoodCost)
        {
            var allRecipes = GetAllRecipes();
            return allRecipes.Where(r => r.ActualFoodCostPercentage <= maxFoodCost).ToList();
        }

        public static List<Recipe> GetRecipesByProfitMargin(decimal minProfitMargin)
        {
            var allRecipes = GetAllRecipes();
            return allRecipes.Where(r => r.ProfitMargin >= minProfitMargin).ToList();
        }

        public static int GetRecipeVersionCount(int recipeId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM recipe_versions WHERE recipe_id = @recipeId;";

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
