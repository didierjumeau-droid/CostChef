using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace CostChef
{
    public static class DatabaseContext
    {
        private static string _connectionString = "Data Source=costchef.db;Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists("costchef.db"))
            {
                SQLiteConnection.CreateFile("costchef.db");
            }

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    // Create ingredients table
                    string createIngredientsTable = @"
                        CREATE TABLE IF NOT EXISTS ingredients (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NOT NULL UNIQUE,
                            unit TEXT NOT NULL,
                            unit_price REAL NOT NULL,
                            category TEXT,
                            supplier_id INTEGER
                        )";

                    // Create price_history table
                    string createPriceHistoryTable = @"
                        CREATE TABLE IF NOT EXISTS price_history (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ingredient_id INTEGER NOT NULL,
                            old_price REAL NOT NULL,
                            new_price REAL NOT NULL,
                            change_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                            changed_by TEXT DEFAULT 'System',
                            reason TEXT,
                            FOREIGN KEY (ingredient_id) REFERENCES ingredients (id)
                        )";

                    // Create recipes table
                    string createRecipesTable = @"
                        CREATE TABLE IF NOT EXISTS recipes (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NOT NULL,
                            description TEXT,
                            category TEXT,
                            tags TEXT,
                            batch_yield INTEGER DEFAULT 1,
                            target_food_cost_percentage REAL DEFAULT 30.0
                        )";

                    // Create recipe_versions table
                    string createRecipeVersionsTable = @"
                        CREATE TABLE IF NOT EXISTS recipe_versions (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            recipe_id INTEGER NOT NULL,
                            version_number INTEGER NOT NULL,
                            version_name TEXT,
                            version_notes TEXT,
                            created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                            created_by TEXT DEFAULT 'System',
                            is_current BOOLEAN DEFAULT 0,
                            recipe_data TEXT NOT NULL,
                            FOREIGN KEY (recipe_id) REFERENCES recipes (id)
                        )";

                    // Create recipe_ingredients table
                    string createRecipeIngredientsTable = @"
                        CREATE TABLE IF NOT EXISTS recipe_ingredients (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            recipe_id INTEGER NOT NULL,
                            ingredient_id INTEGER NOT NULL,
                            quantity REAL NOT NULL,
                            FOREIGN KEY (recipe_id) REFERENCES recipes (id),
                            FOREIGN KEY (ingredient_id) REFERENCES ingredients (id)
                        )";

                    // Create suppliers table
                    string createSuppliersTable = @"
                        CREATE TABLE IF NOT EXISTS suppliers (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NOT NULL,
                            contact_person TEXT,
                            phone TEXT,
                            email TEXT,
                            address TEXT,
                            created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";

                    // Create settings table
                    string createSettingsTable = @"
                        CREATE TABLE IF NOT EXISTS settings (
                            key TEXT PRIMARY KEY,
                            value TEXT
                        )";

                    // Execute all table creation commands
                    command.CommandText = createIngredientsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createPriceHistoryTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createRecipesTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createRecipeVersionsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createRecipeIngredientsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createSuppliersTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createSettingsTable;
                    command.ExecuteNonQuery();
                }
            }
        }

        // ========== PRICE HISTORY METHODS ==========

        public static void RecordPriceChange(int ingredientId, decimal oldPrice, decimal newPrice, string changedBy = "System", string reason = "")
        {
            if (oldPrice == newPrice) return;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    INSERT INTO price_history (ingredient_id, old_price, new_price, changed_by, reason) 
                    VALUES (@ingredient_id, @old_price, @new_price, @changed_by, @reason)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ingredient_id", ingredientId);
                    command.Parameters.AddWithValue("@old_price", oldPrice);
                    command.Parameters.AddWithValue("@new_price", newPrice);
                    command.Parameters.AddWithValue("@changed_by", changedBy);
                    command.Parameters.AddWithValue("@reason", reason ?? "");
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<PriceHistory> GetPriceHistory(int ingredientId)
        {
            var priceHistory = new List<PriceHistory>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT * FROM price_history 
                    WHERE ingredient_id = @ingredient_id 
                    ORDER BY change_date DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ingredient_id", ingredientId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            priceHistory.Add(new PriceHistory
                            {
                                Id = SafeGetInt(reader, "id"),
                                IngredientId = SafeGetInt(reader, "ingredient_id"),
                                OldPrice = SafeGetDecimal(reader, "old_price"),
                                NewPrice = SafeGetDecimal(reader, "new_price"),
                                ChangeDate = DateTime.Parse(SafeGetString(reader, "change_date")),
                                ChangedBy = SafeGetString(reader, "changed_by"),
                                Reason = SafeGetString(reader, "reason")
                            });
                        }
                    }
                }
            }

            return priceHistory;
        }

        public static List<PriceHistory> GetRecentPriceChanges(int limit = 50)
        {
            var priceHistory = new List<PriceHistory>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT ph.*, i.name as ingredient_name 
                    FROM price_history ph
                    INNER JOIN ingredients i ON ph.ingredient_id = i.id
                    ORDER BY ph.change_date DESC 
                    LIMIT @limit";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@limit", limit);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var history = new PriceHistory
                            {
                                Id = SafeGetInt(reader, "id"),
                                IngredientId = SafeGetInt(reader, "ingredient_id"),
                                OldPrice = SafeGetDecimal(reader, "old_price"),
                                NewPrice = SafeGetDecimal(reader, "new_price"),
                                ChangeDate = DateTime.Parse(SafeGetString(reader, "change_date")),
                                ChangedBy = SafeGetString(reader, "changed_by"),
                                Reason = SafeGetString(reader, "reason")
                            };
                            
                            priceHistory.Add(history);
                        }
                    }
                }
            }

            return priceHistory;
        }

        // ========== SETTINGS METHODS ==========

        public static Dictionary<string, string> GetAllSettings()
        {
            var settings = new Dictionary<string, string>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM settings";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        settings[reader["key"].ToString()] = reader["value"]?.ToString();
                    }
                }
            }

            return settings;
        }

        public static void SetSetting(string key, string value)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT OR REPLACE INTO settings (key, value) VALUES (@key, @value)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@key", key);
                    command.Parameters.AddWithValue("@value", value);
                    command.ExecuteNonQuery();
                }
            }
        }

        // ========== SUPPLIER METHODS ==========

        public static List<Supplier> GetAllSuppliers()
        {
            var suppliers = new List<Supplier>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    
                    string query = @"SELECT id, name, contact_person, phone, email, address, 
                                   COALESCE(created_at, datetime('now')) as created_at 
                                   FROM suppliers ORDER BY name";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var supplier = new Supplier
                            {
                                Id = SafeGetInt(reader, "id"),
                                Name = SafeGetString(reader, "name"),
                                ContactPerson = SafeGetString(reader, "contact_person"),
                                Phone = SafeGetString(reader, "phone"),
                                Email = SafeGetString(reader, "email"),
                                Address = SafeGetString(reader, "address"),
                                CreatedAt = SafeGetString(reader, "created_at")
                            };
                            
                            suppliers.Add(supplier);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading suppliers: {ex.Message}");
                return new List<Supplier>();
            }

            return suppliers ?? new List<Supplier>();
        }

        public static void InsertSupplier(Supplier supplier)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO suppliers (name, contact_person, phone, email, address) 
                               VALUES (@name, @contact_person, @phone, @email, @address)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", supplier.Name);
                    command.Parameters.AddWithValue("@contact_person", supplier.ContactPerson ?? "");
                    command.Parameters.AddWithValue("@phone", supplier.Phone ?? "");
                    command.Parameters.AddWithValue("@email", supplier.Email ?? "");
                    command.Parameters.AddWithValue("@address", supplier.Address ?? "");
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateSupplier(Supplier supplier)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"UPDATE suppliers SET name = @name, contact_person = @contact_person, 
                               phone = @phone, email = @email, address = @address WHERE id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", supplier.Name);
                    command.Parameters.AddWithValue("@contact_person", supplier.ContactPerson ?? "");
                    command.Parameters.AddWithValue("@phone", supplier.Phone ?? "");
                    command.Parameters.AddWithValue("@email", supplier.Email ?? "");
                    command.Parameters.AddWithValue("@address", supplier.Address ?? "");
                    command.Parameters.AddWithValue("@id", supplier.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteSupplier(int supplierId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                // Remove supplier references from ingredients
                string updateIngredients = "UPDATE ingredients SET supplier_id = NULL WHERE supplier_id = @supplierId";
                using (var command = new SQLiteCommand(updateIngredients, connection))
                {
                    command.Parameters.AddWithValue("@supplierId", supplierId);
                    command.ExecuteNonQuery();
                }

                // Delete supplier
                string deleteSupplier = "DELETE FROM suppliers WHERE id = @id";
                using (var command = new SQLiteCommand(deleteSupplier, connection))
                {
                    command.Parameters.AddWithValue("@id", supplierId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static Supplier GetSupplierByName(string name)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM suppliers WHERE name = @name";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Supplier
                            {
                                Id = SafeGetInt(reader, "id"),
                                Name = SafeGetString(reader, "name"),
                                ContactPerson = SafeGetString(reader, "contact_person"),
                                Phone = SafeGetString(reader, "phone"),
                                Email = SafeGetString(reader, "email"),
                                Address = SafeGetString(reader, "address"),
                                CreatedAt = SafeGetString(reader, "created_at")
                            };
                        }
                    }
                }
            }

            return null;
        }

        public static List<Ingredient> GetIngredientsBySupplier(int supplierId)
        {
            var ingredients = new List<Ingredient>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM ingredients WHERE supplier_id = @supplierId ORDER BY name";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@supplierId", supplierId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ingredients.Add(new Ingredient
                                {
                                    Id = SafeGetInt(reader, "id"),
                                    Name = SafeGetString(reader, "name"),
                                    Unit = SafeGetString(reader, "unit"),
                                    UnitPrice = SafeGetDecimal(reader, "unit_price"),
                                    Category = SafeGetString(reader, "category"),
                                    SupplierId = SafeGetNullableInt(reader, "supplier_id")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading supplier ingredients: {ex.Message}");
            }

            return ingredients ?? new List<Ingredient>();
        }

        public static dynamic GetSupplierStatistics(int supplierId)
        {
            var ingredients = GetIngredientsBySupplier(supplierId);
            return new
            {
                IngredientCount = ingredients?.Count ?? 0,
                TotalValue = ingredients?.Sum(i => i.UnitPrice) ?? 0
            };
        }

        // ========== INGREDIENT METHODS ==========

        public static List<Ingredient> GetAllIngredients()
        {
            var ingredients = new List<Ingredient>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"SELECT i.*, s.name as supplier_name 
                               FROM ingredients i 
                               LEFT JOIN suppliers s ON i.supplier_id = s.id 
                               ORDER BY i.name";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ingredients.Add(new Ingredient
                            {
                                Id = SafeGetInt(reader, "id"),
                                Name = SafeGetString(reader, "name"),
                                Unit = SafeGetString(reader, "unit"),
                                UnitPrice = SafeGetDecimal(reader, "unit_price"),
                                Category = SafeGetString(reader, "category"),
                                SupplierId = SafeGetNullableInt(reader, "supplier_id"),
                                SupplierName = SafeGetString(reader, "supplier_name")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading ingredients: {ex.Message}");
            }

            return ingredients ?? new List<Ingredient>();
        }

        public static void InsertIngredient(Ingredient ingredient)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO ingredients (name, unit, unit_price, category, supplier_id) 
                               VALUES (@name, @unit, @unit_price, @category, @supplier_id)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", ingredient.Name);
                    command.Parameters.AddWithValue("@unit", ingredient.Unit);
                    command.Parameters.AddWithValue("@unit_price", ingredient.UnitPrice);
                    command.Parameters.AddWithValue("@category", ingredient.Category ?? "");
                    command.Parameters.AddWithValue("@supplier_id", 
                        ingredient.SupplierId.HasValue && ingredient.SupplierId.Value > 0 
                            ? (object)ingredient.SupplierId.Value 
                            : DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateIngredient(Ingredient ingredient)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                // Get old price for history
                decimal oldPrice = 0;
                string getOldPriceQuery = "SELECT unit_price FROM ingredients WHERE id = @id";
                using (var getPriceCommand = new SQLiteCommand(getOldPriceQuery, connection))
                {
                    getPriceCommand.Parameters.AddWithValue("@id", ingredient.Id);
                    var result = getPriceCommand.ExecuteScalar();
                    if (result != null)
                    {
                        oldPrice = Convert.ToDecimal(result);
                    }
                }

                string query = @"UPDATE ingredients 
                               SET name = @name, unit = @unit, unit_price = @unit_price, 
                                   category = @category, supplier_id = @supplier_id 
                               WHERE id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", ingredient.Name);
                    command.Parameters.AddWithValue("@unit", ingredient.Unit);
                    command.Parameters.AddWithValue("@unit_price", ingredient.UnitPrice);
                    command.Parameters.AddWithValue("@category", ingredient.Category ?? "");
                    command.Parameters.AddWithValue("@supplier_id", 
                        ingredient.SupplierId.HasValue && ingredient.SupplierId.Value > 0 
                            ? (object)ingredient.SupplierId.Value 
                            : DBNull.Value);
                    command.Parameters.AddWithValue("@id", ingredient.Id);
                    command.ExecuteNonQuery();
                }

                // Record price change if needed
                if (oldPrice != ingredient.UnitPrice)
                {
                    RecordPriceChange(ingredient.Id, oldPrice, ingredient.UnitPrice, "User", "Price update");
                }
            }
        }

        public static void DeleteIngredient(int ingredientId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Delete from recipe_ingredients first
                string deleteRecipeIngredients = "DELETE FROM recipe_ingredients WHERE ingredient_id = @ingredientId";
                using (var command = new SQLiteCommand(deleteRecipeIngredients, connection))
                {
                    command.Parameters.AddWithValue("@ingredientId", ingredientId);
                    command.ExecuteNonQuery();
                }

                // Delete the ingredient
                string deleteIngredient = "DELETE FROM ingredients WHERE id = @id";
                using (var command = new SQLiteCommand(deleteIngredient, connection))
                {
                    command.Parameters.AddWithValue("@id", ingredientId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<string> GetIngredientCategories()
        {
            var categories = new List<string>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT category FROM ingredients WHERE category IS NOT NULL AND category != '' ORDER BY category";

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
                System.Diagnostics.Debug.WriteLine($"Error loading ingredient categories: {ex.Message}");
            }

            return categories ?? new List<string>();
        }

        public static List<string> GetIngredientUnits()
        {
            var units = new List<string>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT unit FROM ingredients WHERE unit IS NOT NULL AND unit != '' ORDER BY unit";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            units.Add(SafeGetString(reader, "unit"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading ingredient units: {ex.Message}");
            }

            return units ?? new List<string>();
        }

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

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        // ========== SAFE DATA READER METHODS ==========

        private static string SafeGetString(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static int SafeGetInt(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static decimal SafeGetDecimal(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? 0 : reader.GetDecimal(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static int? SafeGetNullableInt(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? (int?)null : reader.GetInt32(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}