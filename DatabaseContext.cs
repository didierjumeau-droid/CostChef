using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;

namespace CostChef
{
    public static class DatabaseContext
    {
        private static string ConnectionString => "Data Source=costchef.db";

        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // Create ingredients table
            var createIngredientsTable = @"
                CREATE TABLE IF NOT EXISTS ingredients (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE,
                    unit TEXT NOT NULL,
                    unit_price REAL NOT NULL,
                    category TEXT
                )";

            // Create recipes table
            var createRecipesTable = @"
                CREATE TABLE IF NOT EXISTS recipes (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    description TEXT,
                    batch_yield INTEGER NOT NULL DEFAULT 1,
                    target_food_cost_percentage REAL NOT NULL DEFAULT 0.3,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP,
                    modified_date TEXT DEFAULT CURRENT_TIMESTAMP
                )";

            // Create recipe_ingredients table
            var createRecipeIngredientsTable = @"
                CREATE TABLE IF NOT EXISTS recipe_ingredients (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    recipe_id INTEGER NOT NULL,
                    ingredient_id INTEGER NOT NULL,
                    quantity REAL NOT NULL,
                    FOREIGN KEY (recipe_id) REFERENCES recipes (id) ON DELETE CASCADE,
                    FOREIGN KEY (ingredient_id) REFERENCES ingredients (id) ON DELETE CASCADE,
                    UNIQUE(recipe_id, ingredient_id)
                )";

            using (var command = new SqliteCommand(createIngredientsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createRecipesTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand(createRecipeIngredientsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            // Insert sample ingredients if empty
            if (GetIngredientsCount() == 0)
            {
                InsertSampleIngredients();
            }
        }

        private static int GetIngredientsCount()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand("SELECT COUNT(*) FROM ingredients", connection);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        private static void InsertSampleIngredients()
        {
            var sampleIngredients = new List<Ingredient>
            {
                new Ingredient { Name = "All-purpose Flour", Unit = "gram", UnitPrice = 0.1m },
                new Ingredient { Name = "Bacon", Unit = "gram", UnitPrice = 0.9m },
                new Ingredient { Name = "Beef Striploin", Unit = "gram", UnitPrice = 1.2m },
                new Ingredient { Name = "Black Pepper", Unit = "gram", UnitPrice = 1.5m },
                new Ingredient { Name = "Breadcrumbs", Unit = "gram", UnitPrice = 0.35m },
                new Ingredient { Name = "Brown Sugar", Unit = "gram", UnitPrice = 0.1m },
                new Ingredient { Name = "Butter", Unit = "gram", UnitPrice = 1.2m },
                new Ingredient { Name = "Cabbage", Unit = "gram", UnitPrice = 0.2m },
                new Ingredient { Name = "Calamansi", Unit = "piece", UnitPrice = 2.5m },
                new Ingredient { Name = "Carrot", Unit = "piece", UnitPrice = 10m },
                new Ingredient { Name = "Cheddar Cheese", Unit = "gram", UnitPrice = 0.9m },
                new Ingredient { Name = "Cheese Sauce", Unit = "tablespoon", UnitPrice = 5m },
                new Ingredient { Name = "Chicken Breast", Unit = "gram", UnitPrice = 0.32m },
                new Ingredient { Name = "Chicken Stock (house)", Unit = "ml", UnitPrice = 0.2m },
                new Ingredient { Name = "Chicken Thigh", Unit = "gram", UnitPrice = 0.4m },
                new Ingredient { Name = "Cilantro", Unit = "gram", UnitPrice = 1m },
                new Ingredient { Name = "Condensed Milk (can)", Unit = "can (390 g)", UnitPrice = 60m },
                new Ingredient { Name = "Cooking Oil", Unit = "ml", UnitPrice = 0.12m },
                new Ingredient { Name = "Egg", Unit = "piece", UnitPrice = 10m },
                new Ingredient { Name = "Egg Yolk", Unit = "piece", UnitPrice = 6.5m },
                new Ingredient { Name = "Evaporated Milk (can)", Unit = "can (370 ml)", UnitPrice = 60m },
                new Ingredient { Name = "Fish Sauce (Patis)", Unit = "tablespoon", UnitPrice = 1.2m },
                new Ingredient { Name = "French Fries (prepped, frozen)", Unit = "gram", UnitPrice = 0.17m },
                new Ingredient { Name = "Fresh Milk", Unit = "ml", UnitPrice = 0.12m },
                new Ingredient { Name = "Garlic", Unit = "clove", UnitPrice = 2m },
                new Ingredient { Name = "Green Chili", Unit = "piece", UnitPrice = 3m },
                new Ingredient { Name = "Ground Beef 80/20", Unit = "gram", UnitPrice = 0.5m },
                new Ingredient { Name = "Hamburger Bun", Unit = "piece", UnitPrice = 5m },
                new Ingredient { Name = "Jasmine Rice (raw)", Unit = "gram", UnitPrice = 0.06m },
                new Ingredient { Name = "Ketchup", Unit = "tablespoon", UnitPrice = 1.5m },
                new Ingredient { Name = "Lemon", Unit = "piece", UnitPrice = 25m },
                new Ingredient { Name = "Lettuce", Unit = "leaf", UnitPrice = 4m },
                new Ingredient { Name = "Mahi-Mahi (Dorado)", Unit = "gram", UnitPrice = 0.75m },
                new Ingredient { Name = "Mayonnaise (house)", Unit = "tablespoon", UnitPrice = 7m },
                new Ingredient { Name = "Mozzarella", Unit = "gram", UnitPrice = 0.652m },
                new Ingredient { Name = "Olive Oil", Unit = "ml", UnitPrice = 0.4m },
                new Ingredient { Name = "Onion", Unit = "piece", UnitPrice = 10m },
                new Ingredient { Name = "Oyster Sauce", Unit = "tablespoon", UnitPrice = 1.8m },
                new Ingredient { Name = "Pancit Canton Noodles", Unit = "gram", UnitPrice = 0.2m },
                new Ingredient { Name = "Parmesan", Unit = "gram", UnitPrice = 1m },
                new Ingredient { Name = "Pasta Fettuccine", Unit = "gram", UnitPrice = 0.13m },
                new Ingredient { Name = "Pasta Spaghetti", Unit = "gram", UnitPrice = 0.13m },
                new Ingredient { Name = "Pickles", Unit = "slice", UnitPrice = 1.5m },
                new Ingredient { Name = "Pumpkin", Unit = "gram", UnitPrice = 0.15m },
                new Ingredient { Name = "Salsa (house)", Unit = "tablespoon", UnitPrice = 4m },
                new Ingredient { Name = "Salt", Unit = "gram", UnitPrice = 0.03m },
                new Ingredient { Name = "Shrimp (medium)", Unit = "piece", UnitPrice = 7m },
                new Ingredient { Name = "Sinigang Mix (Tamarind)", Unit = "tablespoon", UnitPrice = 3m },
                new Ingredient { Name = "Soy Sauce", Unit = "tablespoon", UnitPrice = 1m },
                new Ingredient { Name = "Spinach", Unit = "gram", UnitPrice = 0.6m },
                new Ingredient { Name = "Tempura Flour", Unit = "gram", UnitPrice = 0.5m },
                new Ingredient { Name = "Tomato Paste", Unit = "tablespoon", UnitPrice = 2m },
                new Ingredient { Name = "Tomato Sauce", Unit = "ml", UnitPrice = 0.2m },
                new Ingredient { Name = "Tortilla (8-inch)", Unit = "piece", UnitPrice = 12m },
                new Ingredient { Name = "Tortilla Chips", Unit = "gram", UnitPrice = 0.4m },
                new Ingredient { Name = "Tuna Loin", Unit = "gram", UnitPrice = 0.7m },
                new Ingredient { Name = "Vanilla Extract", Unit = "teaspoon", UnitPrice = 10m },
                new Ingredient { Name = "White Sugar", Unit = "gram", UnitPrice = 0.08m },
                new Ingredient { Name = "Yellow Mustard", Unit = "teaspoon", UnitPrice = 1m },
                new Ingredient { Name = "mushrooms", Unit = "piece", UnitPrice = 2.5m }
            };

            foreach (var ingredient in sampleIngredients)
            {
                InsertIngredient(ingredient);
            }
        }

        // Ingredient methods
        public static List<Ingredient> GetAllIngredients()
        {
            var ingredients = new List<Ingredient>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand("SELECT * FROM ingredients ORDER BY name", connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                ingredients.Add(new Ingredient
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Unit = reader.GetString("unit"),
                    UnitPrice = reader.GetDecimal("unit_price"),
                    Category = reader.IsDBNull("category") ? "" : reader.GetString("category")
                });
            }
            
            return ingredients;
        }

        public static void InsertIngredient(Ingredient ingredient)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand(
                "INSERT INTO ingredients (name, unit, unit_price, category) VALUES (@name, @unit, @unitPrice, @category)",
                connection);
            
            command.Parameters.AddWithValue("@name", ingredient.Name);
            command.Parameters.AddWithValue("@unit", ingredient.Unit);
            command.Parameters.AddWithValue("@unitPrice", ingredient.UnitPrice);
            command.Parameters.AddWithValue("@category", ingredient.Category ?? "");
            
            command.ExecuteNonQuery();
        }

        public static void UpdateIngredient(Ingredient ingredient)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand(
                "UPDATE ingredients SET name = @name, unit = @unit, unit_price = @unitPrice, category = @category WHERE id = @id",
                connection);
            
            command.Parameters.AddWithValue("@id", ingredient.Id);
            command.Parameters.AddWithValue("@name", ingredient.Name);
            command.Parameters.AddWithValue("@unit", ingredient.Unit);
            command.Parameters.AddWithValue("@unitPrice", ingredient.UnitPrice);
            command.Parameters.AddWithValue("@category", ingredient.Category ?? "");
            
            command.ExecuteNonQuery();
        }

        public static void DeleteIngredient(int id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand("DELETE FROM ingredients WHERE id = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        public static Ingredient GetIngredientByName(string name)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand("SELECT * FROM ingredients WHERE name = @name", connection);
            command.Parameters.AddWithValue("@name", name);
            
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Ingredient
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Unit = reader.GetString("unit"),
                    UnitPrice = reader.GetDecimal("unit_price"),
                    Category = reader.IsDBNull("category") ? "" : reader.GetString("category")
                };
            }
            
            return null;
        }

        // Recipe methods
        public static List<Recipe> GetAllRecipes()
        {
            var recipes = new List<Recipe>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand("SELECT * FROM recipes ORDER BY name", connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                var recipe = new Recipe
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Description = reader.IsDBNull("description") ? "" : reader.GetString("description"),
                    BatchYield = reader.GetInt32("batch_yield"),
                    TargetFoodCostPercentage = reader.GetDecimal("target_food_cost_percentage")
                };
                
                // Load recipe ingredients
                recipe.Ingredients = GetRecipeIngredients(recipe.Id);
                
                recipes.Add(recipe);
            }
            
            return recipes;
        }

        public static int InsertRecipe(Recipe recipe)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand(
                "INSERT INTO recipes (name, description, batch_yield, target_food_cost_percentage) VALUES (@name, @description, @batchYield, @targetFoodCost); SELECT last_insert_rowid();",
                connection);
            
            command.Parameters.AddWithValue("@name", recipe.Name);
            command.Parameters.AddWithValue("@description", recipe.Description ?? "");
            command.Parameters.AddWithValue("@batchYield", recipe.BatchYield);
            command.Parameters.AddWithValue("@targetFoodCost", recipe.TargetFoodCostPercentage);
            
            var newId = Convert.ToInt32(command.ExecuteScalar());
            
            // Insert recipe ingredients
            foreach (var ingredient in recipe.Ingredients)
            {
                InsertRecipeIngredient(newId, ingredient);
            }
            
            return newId;
        }

        public static void UpdateRecipe(Recipe recipe)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            // Update recipe
            using var command = new SqliteCommand(
                "UPDATE recipes SET name = @name, description = @description, batch_yield = @batchYield, target_food_cost_percentage = @targetFoodCost, modified_date = CURRENT_TIMESTAMP WHERE id = @id",
                connection);
            
            command.Parameters.AddWithValue("@id", recipe.Id);
            command.Parameters.AddWithValue("@name", recipe.Name);
            command.Parameters.AddWithValue("@description", recipe.Description ?? "");
            command.Parameters.AddWithValue("@batchYield", recipe.BatchYield);
            command.Parameters.AddWithValue("@targetFoodCost", recipe.TargetFoodCostPercentage);
            
            command.ExecuteNonQuery();
            
            // Update recipe ingredients (delete all and reinsert)
            DeleteRecipeIngredients(recipe.Id);
            foreach (var ingredient in recipe.Ingredients)
            {
                InsertRecipeIngredient(recipe.Id, ingredient);
            }
        }

        public static void DeleteRecipe(int id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            // Recipe ingredients will be deleted due to CASCADE
            using var command = new SqliteCommand("DELETE FROM recipes WHERE id = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        // Recipe ingredient methods
        private static List<RecipeIngredient> GetRecipeIngredients(int recipeId)
        {
            var ingredients = new List<RecipeIngredient>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand(
                "SELECT ri.*, i.name as ingredient_name, i.unit, i.unit_price " +
                "FROM recipe_ingredients ri " +
                "JOIN ingredients i ON ri.ingredient_id = i.id " +
                "WHERE ri.recipe_id = @recipeId", 
                connection);
            
            command.Parameters.AddWithValue("@recipeId", recipeId);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ingredients.Add(new RecipeIngredient
                {
                    IngredientId = reader.GetInt32("ingredient_id"),
                    Quantity = reader.GetDecimal("quantity"),
                    IngredientName = reader.GetString("ingredient_name"),
                    Unit = reader.GetString("unit"),
                    UnitPrice = reader.GetDecimal("unit_price")
                });
            }
            
            return ingredients;
        }

        private static void InsertRecipeIngredient(int recipeId, RecipeIngredient ingredient)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand(
                "INSERT INTO recipe_ingredients (recipe_id, ingredient_id, quantity) VALUES (@recipeId, @ingredientId, @quantity)",
                connection);
            
            command.Parameters.AddWithValue("@recipeId", recipeId);
            command.Parameters.AddWithValue("@ingredientId", ingredient.IngredientId);
            command.Parameters.AddWithValue("@quantity", ingredient.Quantity);
            
            command.ExecuteNonQuery();
        }

        private static void DeleteRecipeIngredients(int recipeId)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand("DELETE FROM recipe_ingredients WHERE recipe_id = @recipeId", connection);
            command.Parameters.AddWithValue("@recipeId", recipeId);
            command.ExecuteNonQuery();
        }
    }
}