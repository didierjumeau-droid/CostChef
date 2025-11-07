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

            // UPDATED: Create recipes table with category and tags
            var createRecipesTable = @"
                CREATE TABLE IF NOT EXISTS recipes (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE,
                    description TEXT,
                    category TEXT,
                    tags TEXT,
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

            // 🆕 CREATE SETTINGS TABLE
            var createSettingsTable = @"
                CREATE TABLE IF NOT EXISTS settings (
                    key TEXT PRIMARY KEY,
                    value TEXT
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

            using (var command = new SqliteCommand(createSettingsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            // NEW: Update existing tables to add missing columns
            UpdateExistingTables(connection);

            // NEW: Initialize default settings
            InitializeDefaultSettings(connection);

            // Clean up any existing duplicate recipes
            CleanDuplicateRecipes();

            // Insert minimal sample ingredients if empty
            if (GetIngredientsCount() == 0)
            {
                InsertSampleIngredients();
            }
        }

        // NEW: Initialize default settings
        private static void InitializeDefaultSettings(SqliteConnection connection)
        {
            try
            {
                // Check if we already have settings
                var checkSettings = "SELECT COUNT(*) FROM settings";
                using var checkCommand = new SqliteCommand(checkSettings, connection);
                var hasSettings = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

                if (!hasSettings)
                {
                    // Insert default settings
                    var defaultSettings = new Dictionary<string, string>
                    {
                        {"CurrencySymbol", "$"},
                        {"CurrencyCode", "USD"},
                        {"DecimalPlaces", "2"},
                        {"AutoSave", "true"}
                    };

                    foreach (var setting in defaultSettings)
                    {
                        var insertSql = "INSERT INTO settings (key, value) VALUES (@key, @value)";
                        using var insertCommand = new SqliteCommand(insertSql, connection);
                        insertCommand.Parameters.AddWithValue("@key", setting.Key);
                        insertCommand.Parameters.AddWithValue("@value", setting.Value);
                        insertCommand.ExecuteNonQuery();
                    }
                    
                    System.Diagnostics.Debug.WriteLine("Initialized default settings");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Could not initialize settings: {ex.Message}");
            }
        }

        // NEW: Settings management methods
        public static string GetSetting(string key, string defaultValue = "")
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                
                using var command = new SqliteCommand("SELECT value FROM settings WHERE key = @key", connection);
                command.Parameters.AddWithValue("@key", key);
                
                var result = command.ExecuteScalar();
                return result?.ToString() ?? defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting setting {key}: {ex.Message}");
                return defaultValue;
            }
        }

        public static void SetSetting(string key, string value)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                
                using var command = new SqliteCommand(
                    "INSERT OR REPLACE INTO settings (key, value) VALUES (@key, @value)",
                    connection);
                
                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@value", value);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting setting {key}: {ex.Message}");
            }
        }

        // NEW: Get all settings as dictionary
        public static Dictionary<string, string> GetAllSettings()
        {
            var settings = new Dictionary<string, string>();
            
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                
                using var command = new SqliteCommand("SELECT key, value FROM settings", connection);
                using var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    settings[reader.GetString("key")] = reader.GetString("value");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all settings: {ex.Message}");
            }
            
            return settings;
        }

        // NEW: Method to update existing tables with new columns
        private static void UpdateExistingTables(SqliteConnection connection)
        {
            try
            {
                // Check if category column exists in recipes table
                var checkCategoryColumn = @"
                    SELECT COUNT(*) FROM pragma_table_info('recipes') 
                    WHERE name = 'category'";
                
                using var checkCommand = new SqliteCommand(checkCategoryColumn, connection);
                var hasCategoryColumn = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

                if (!hasCategoryColumn)
                {
                    var addCategoryColumn = "ALTER TABLE recipes ADD COLUMN category TEXT";
                    using var alterCommand = new SqliteCommand(addCategoryColumn, connection);
                    alterCommand.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("Added 'category' column to recipes table");
                }

                // Check if tags column exists in recipes table
                var checkTagsColumn = @"
                    SELECT COUNT(*) FROM pragma_table_info('recipes') 
                    WHERE name = 'tags'";
                
                using var checkTagsCommand = new SqliteCommand(checkTagsColumn, connection);
                var hasTagsColumn = Convert.ToInt32(checkTagsCommand.ExecuteScalar()) > 0;

                if (!hasTagsColumn)
                {
                    var addTagsColumn = "ALTER TABLE recipes ADD COLUMN tags TEXT";
                    using var alterCommand = new SqliteCommand(addTagsColumn, connection);
                    alterCommand.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("Added 'tags' column to recipes table");
                }

                // Check if description column exists in recipes table
                var checkDescriptionColumn = @"
                    SELECT COUNT(*) FROM pragma_table_info('recipes') 
                    WHERE name = 'description'";
                
                using var checkDescCommand = new SqliteCommand(checkDescriptionColumn, connection);
                var hasDescriptionColumn = Convert.ToInt32(checkDescCommand.ExecuteScalar()) > 0;

                if (!hasDescriptionColumn)
                {
                    var addDescriptionColumn = "ALTER TABLE recipes ADD COLUMN description TEXT";
                    using var alterCommand = new SqliteCommand(addDescriptionColumn, connection);
                    alterCommand.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("Added 'description' column to recipes table");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Could not update table schema: {ex.Message}");
            }
        }

        private static void CleanDuplicateRecipes()
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                
                var cleanDuplicatesSql = @"
                    DELETE FROM recipes 
                    WHERE id NOT IN (
                        SELECT MIN(id) 
                        FROM recipes 
                        GROUP BY name
                    )";
                
                using var command = new SqliteCommand(cleanDuplicatesSql, connection);
                int duplicatesRemoved = command.ExecuteNonQuery();
                
                if (duplicatesRemoved > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Cleaned up {duplicatesRemoved} duplicate recipes");
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Could not clean duplicate recipes");
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
            var essentialIngredients = new List<Ingredient>
            {
                new Ingredient { Name = "Salt", Unit = "gram", UnitPrice = 0.03m, Category = "Seasoning" },
                new Ingredient { Name = "Black Pepper", Unit = "gram", UnitPrice = 1.5m, Category = "Seasoning" },
                new Ingredient { Name = "Cooking Oil", Unit = "ml", UnitPrice = 0.12m, Category = "Oil" },
                new Ingredient { Name = "Water", Unit = "ml", UnitPrice = 0.0m, Category = "Liquid" },
                new Ingredient { Name = "All-purpose Flour", Unit = "gram", UnitPrice = 0.1m, Category = "Dry Goods" },
                new Ingredient { Name = "Sugar", Unit = "gram", UnitPrice = 0.08m, Category = "Dry Goods" }
            };

            foreach (var ingredient in essentialIngredients)
            {
                try
                {
                    InsertIngredient(ingredient);
                }
                catch (Exception)
                {
                    System.Diagnostics.Debug.WriteLine($"Sample ingredient already exists: {ingredient.Name}");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"Added {essentialIngredients.Count} essential sample ingredients");
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

        // UPDATED: Recipe methods to handle category and tags
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
                    Category = reader.IsDBNull("category") ? "" : reader.GetString("category"),
                    BatchYield = reader.GetInt32("batch_yield"),
                    TargetFoodCostPercentage = reader.GetDecimal("target_food_cost_percentage")
                };
                
                // Load tags from comma-separated string
                if (!reader.IsDBNull("tags"))
                {
                    var tagsString = reader.GetString("tags");
                    recipe.Tags = tagsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .ToList();
                }
                
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
            
            // UPDATED: Include category and tags in insert
            using var command = new SqliteCommand(
                "INSERT INTO recipes (name, description, category, tags, batch_yield, target_food_cost_percentage) " +
                "VALUES (@name, @description, @category, @tags, @batchYield, @targetFoodCost); " +
                "SELECT last_insert_rowid();",
                connection);
            
            command.Parameters.AddWithValue("@name", recipe.Name);
            command.Parameters.AddWithValue("@description", recipe.Description ?? "");
            command.Parameters.AddWithValue("@category", recipe.Category ?? "");
            command.Parameters.AddWithValue("@tags", string.Join(",", recipe.Tags ?? new List<string>()));
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
            
            // UPDATED: Include category and tags in update
            using var command = new SqliteCommand(
                "UPDATE recipes SET name = @name, description = @description, category = @category, " +
                "tags = @tags, batch_yield = @batchYield, target_food_cost_percentage = @targetFoodCost, " +
                "modified_date = CURRENT_TIMESTAMP WHERE id = @id",
                connection);
            
            command.Parameters.AddWithValue("@id", recipe.Id);
            command.Parameters.AddWithValue("@name", recipe.Name);
            command.Parameters.AddWithValue("@description", recipe.Description ?? "");
            command.Parameters.AddWithValue("@category", recipe.Category ?? "");
            command.Parameters.AddWithValue("@tags", string.Join(",", recipe.Tags ?? new List<string>()));
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

        // NEW: Get all unique categories from recipes
        public static List<string> GetRecipeCategories()
        {
            var categories = new List<string>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand(
                "SELECT DISTINCT category FROM recipes WHERE category IS NOT NULL AND category != '' ORDER BY category",
                connection);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(reader.GetString("category"));
            }
            
            return categories;
        }

        // NEW: Get all unique tags from recipes
        public static List<string> GetAllTags()
        {
            var tags = new List<string>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = new SqliteCommand(
                "SELECT tags FROM recipes WHERE tags IS NOT NULL AND tags != ''",
                connection);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var tagsString = reader.GetString("tags");
                var recipeTags = tagsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t));
                
                tags.AddRange(recipeTags);
            }
            
            return tags.Distinct().OrderBy(t => t).ToList();
        }
    }
}