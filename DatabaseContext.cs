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

            // Create suppliers table FIRST
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS suppliers (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL UNIQUE,
                        contact_person TEXT,
                        phone TEXT,
                        email TEXT,
                        address TEXT
                    )";
                command.ExecuteNonQuery();
            }

            // Create ingredients table WITH SUPPLIER SUPPORT
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS ingredients (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL UNIQUE,
                        unit TEXT NOT NULL,
                        unit_price DECIMAL(10,4) NOT NULL,
                        category TEXT,
                        supplier_id INTEGER,
                        supplier_name TEXT,
                        FOREIGN KEY (supplier_id) REFERENCES suppliers (id) ON DELETE SET NULL
                    )";
                command.ExecuteNonQuery();
            }

            // Check if we need to migrate existing ingredients table
            MigrateIngredientsTable(connection);

            // Create recipes table
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS recipes (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        description TEXT,
                        category TEXT,
                        tags TEXT,
                        batch_yield INTEGER NOT NULL DEFAULT 1,
                        target_food_cost_percentage DECIMAL(5,4) NOT NULL DEFAULT 0.3
                    )";
                command.ExecuteNonQuery();
            }

            // Create recipe_ingredients table
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS recipe_ingredients (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        recipe_id INTEGER NOT NULL,
                        ingredient_id INTEGER NOT NULL,
                        quantity DECIMAL(10,4) NOT NULL,
                        FOREIGN KEY (recipe_id) REFERENCES recipes (id) ON DELETE CASCADE,
                        FOREIGN KEY (ingredient_id) REFERENCES ingredients (id) ON DELETE CASCADE
                    )";
                command.ExecuteNonQuery();
            }

            // Create settings table
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS settings (
                        key TEXT PRIMARY KEY,
                        value TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();
            }

            // Insert default settings if they don't exist
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT OR IGNORE INTO settings (key, value) VALUES 
                    ('CurrencySymbol', '$'),
                    ('CurrencyCode', 'USD'),
                    ('DecimalPlaces', '2'),
                    ('AutoSave', 'true')
                ";
                command.ExecuteNonQuery();
            }

            // Insert default suppliers if they don't exist
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT OR IGNORE INTO suppliers (name, contact_person, phone) VALUES 
                    ('Local Market', 'John Doe', '555-0101'),
                    ('Wholesale Foods Inc', 'Jane Smith', '555-0102'),
                    ('Fresh Produce Co', 'Bob Wilson', '555-0103'),
                    ('Quality Meats Ltd', 'Alice Brown', '555-0104')
                ";
                command.ExecuteNonQuery();
            }
        }

        // NEW: Migration method to add supplier columns to existing ingredients table
        private static void MigrateIngredientsTable(SqliteConnection connection)
        {
            try
            {
                // Check if supplier_id column exists
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) FROM pragma_table_info('ingredients') 
                        WHERE name = 'supplier_id'";
                    var columnExists = Convert.ToInt32(command.ExecuteScalar()) > 0;

                    if (!columnExists)
                    {
                        // Add supplier_id column
                        using (var alterCommand = connection.CreateCommand())
                        {
                            alterCommand.CommandText = "ALTER TABLE ingredients ADD COLUMN supplier_id INTEGER";
                            alterCommand.ExecuteNonQuery();
                        }

                        // Add supplier_name column  
                        using (var alterCommand = connection.CreateCommand())
                        {
                            alterCommand.CommandText = "ALTER TABLE ingredients ADD COLUMN supplier_name TEXT";
                            alterCommand.ExecuteNonQuery();
                        }

                        System.Diagnostics.Debug.WriteLine("Successfully migrated ingredients table with supplier columns");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Migration error: {ex.Message}");
            }
        }

        // SUPPLIER METHODS
        public static List<Supplier> GetAllSuppliers()
        {
            var suppliers = new List<Supplier>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM suppliers ORDER BY name";
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                suppliers.Add(new Supplier
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    ContactPerson = reader.IsDBNull("contact_person") ? "" : reader.GetString("contact_person"),
                    Phone = reader.IsDBNull("phone") ? "" : reader.GetString("phone"),
                    Email = reader.IsDBNull("email") ? "" : reader.GetString("email"),
                    Address = reader.IsDBNull("address") ? "" : reader.GetString("address")
                });
            }
            
            return suppliers;
        }

        public static Supplier GetSupplierById(int id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM suppliers WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);
            
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Supplier
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    ContactPerson = reader.IsDBNull("contact_person") ? "" : reader.GetString("contact_person"),
                    Phone = reader.IsDBNull("phone") ? "" : reader.GetString("phone"),
                    Email = reader.IsDBNull("email") ? "" : reader.GetString("email"),
                    Address = reader.IsDBNull("address") ? "" : reader.GetString("address")
                };
            }
            
            return null;
        }

        public static Supplier GetSupplierByName(string name)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM suppliers WHERE name = @name";
            command.Parameters.AddWithValue("@name", name);
            
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Supplier
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    ContactPerson = reader.IsDBNull("contact_person") ? "" : reader.GetString("contact_person"),
                    Phone = reader.IsDBNull("phone") ? "" : reader.GetString("phone"),
                    Email = reader.IsDBNull("email") ? "" : reader.GetString("email"),
                    Address = reader.IsDBNull("address") ? "" : reader.GetString("address")
                };
            }
            
            return null;
        }

        public static int InsertSupplier(Supplier supplier)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO suppliers (name, contact_person, phone, email, address) 
                VALUES (@name, @contact_person, @phone, @email, @address);
                SELECT last_insert_rowid();";
            
            command.Parameters.AddWithValue("@name", supplier.Name);
            command.Parameters.AddWithValue("@contact_person", (object)supplier.ContactPerson ?? DBNull.Value);
            command.Parameters.AddWithValue("@phone", (object)supplier.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@email", (object)supplier.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@address", (object)supplier.Address ?? DBNull.Value);
            
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public static void UpdateSupplier(Supplier supplier)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE suppliers 
                SET name = @name, contact_person = @contact_person, phone = @phone, 
                    email = @email, address = @address 
                WHERE id = @id";
            
            command.Parameters.AddWithValue("@id", supplier.Id);
            command.Parameters.AddWithValue("@name", supplier.Name);
            command.Parameters.AddWithValue("@contact_person", (object)supplier.ContactPerson ?? DBNull.Value);
            command.Parameters.AddWithValue("@phone", (object)supplier.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@email", (object)supplier.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@address", (object)supplier.Address ?? DBNull.Value);
            
            command.ExecuteNonQuery();
        }

        public static void DeleteSupplier(int id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            // First, remove supplier references from ingredients
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE ingredients SET supplier_id = NULL, supplier_name = NULL WHERE supplier_id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
            
            // Then delete the supplier
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM suppliers WHERE id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }

        // NEW: Get ingredients by supplier
        public static List<Ingredient> GetIngredientsBySupplier(int supplierId)
        {
            var ingredients = new List<Ingredient>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM ingredients WHERE supplier_id = @supplierId ORDER BY name";
            command.Parameters.AddWithValue("@supplierId", supplierId);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ingredients.Add(new Ingredient
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Unit = reader.GetString("unit"),
                    UnitPrice = reader.GetDecimal("unit_price"),
                    Category = reader.IsDBNull("category") ? "" : reader.GetString("category"),
                    SupplierId = reader.IsDBNull("supplier_id") ? null : reader.GetInt32("supplier_id"),
                    SupplierName = reader.IsDBNull("supplier_name") ? "" : reader.GetString("supplier_name")
                });
            }
            
            return ingredients;
        }

        // NEW: Get supplier statistics
        public static List<SupplierStats> GetSupplierStatistics()
        {
            var stats = new List<SupplierStats>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    s.id,
                    s.name,
                    COUNT(i.id) as ingredient_count,
                    COALESCE(SUM(i.unit_price), 0) as total_inventory_value,
                    COALESCE(AVG(i.unit_price), 0) as average_price
                FROM suppliers s
                LEFT JOIN ingredients i ON s.id = i.supplier_id
                GROUP BY s.id, s.name
                ORDER BY ingredient_count DESC";
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                stats.Add(new SupplierStats
                {
                    SupplierId = reader.GetInt32("id"),
                    SupplierName = reader.GetString("name"),
                    IngredientCount = reader.GetInt32("ingredient_count"),
                    TotalInventoryValue = reader.GetDecimal("total_inventory_value"),
                    AveragePrice = reader.GetDecimal("average_price")
                });
            }
            
            return stats;
        }

        // Ingredient methods - UPDATED FOR SUPPLIER SUPPORT
        public static List<Ingredient> GetAllIngredients()
        {
            var ingredients = new List<Ingredient>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT i.*, s.name as supplier_name 
                FROM ingredients i 
                LEFT JOIN suppliers s ON i.supplier_id = s.id 
                ORDER BY i.name";
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ingredients.Add(new Ingredient
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Unit = reader.GetString("unit"),
                    UnitPrice = reader.GetDecimal("unit_price"),
                    Category = reader.IsDBNull("category") ? "" : reader.GetString("category"),
                    SupplierId = reader.IsDBNull("supplier_id") ? null : reader.GetInt32("supplier_id"),
                    SupplierName = reader.IsDBNull("supplier_name") ? "" : reader.GetString("supplier_name")
                });
            }
            
            return ingredients;
        }

        public static Ingredient GetIngredientById(int id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT i.*, s.name as supplier_name 
                FROM ingredients i 
                LEFT JOIN suppliers s ON i.supplier_id = s.id 
                WHERE i.id = @id";
            command.Parameters.AddWithValue("@id", id);
            
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Ingredient
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Unit = reader.GetString("unit"),
                    UnitPrice = reader.GetDecimal("unit_price"),
                    Category = reader.IsDBNull("category") ? "" : reader.GetString("category"),
                    SupplierId = reader.IsDBNull("supplier_id") ? null : reader.GetInt32("supplier_id"),
                    SupplierName = reader.IsDBNull("supplier_name") ? "" : reader.GetString("supplier_name")
                };
            }
            
            return null;
        }

        public static Ingredient GetIngredientByName(string name)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT i.*, s.name as supplier_name 
                FROM ingredients i 
                LEFT JOIN suppliers s ON i.supplier_id = s.id 
                WHERE i.name = @name";
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
                    Category = reader.IsDBNull("category") ? "" : reader.GetString("category"),
                    SupplierId = reader.IsDBNull("supplier_id") ? null : reader.GetInt32("supplier_id"),
                    SupplierName = reader.IsDBNull("supplier_name") ? "" : reader.GetString("supplier_name")
                };
            }
            
            return null;
        }

        public static void InsertIngredient(Ingredient ingredient)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ingredients (name, unit, unit_price, category, supplier_id, supplier_name) 
                VALUES (@name, @unit, @unit_price, @category, @supplier_id, @supplier_name)";
            
            command.Parameters.AddWithValue("@name", ingredient.Name);
            command.Parameters.AddWithValue("@unit", ingredient.Unit);
            command.Parameters.AddWithValue("@unit_price", ingredient.UnitPrice);
            command.Parameters.AddWithValue("@category", (object)ingredient.Category ?? DBNull.Value);
            command.Parameters.AddWithValue("@supplier_id", (object)ingredient.SupplierId ?? DBNull.Value);
            command.Parameters.AddWithValue("@supplier_name", (object)ingredient.SupplierName ?? DBNull.Value);
            
            command.ExecuteNonQuery();
        }

        public static void UpdateIngredient(Ingredient ingredient)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE ingredients 
                SET name = @name, unit = @unit, unit_price = @unit_price, 
                    category = @category, supplier_id = @supplier_id, supplier_name = @supplier_name 
                WHERE id = @id";
            
            command.Parameters.AddWithValue("@id", ingredient.Id);
            command.Parameters.AddWithValue("@name", ingredient.Name);
            command.Parameters.AddWithValue("@unit", ingredient.Unit);
            command.Parameters.AddWithValue("@unit_price", ingredient.UnitPrice);
            command.Parameters.AddWithValue("@category", (object)ingredient.Category ?? DBNull.Value);
            command.Parameters.AddWithValue("@supplier_id", (object)ingredient.SupplierId ?? DBNull.Value);
            command.Parameters.AddWithValue("@supplier_name", (object)ingredient.SupplierName ?? DBNull.Value);
            
            command.ExecuteNonQuery();
        }

        public static void DeleteIngredient(int id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM ingredients WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);
            
            command.ExecuteNonQuery();
        }

        // Recipe methods (unchanged)
        public static List<Recipe> GetAllRecipes()
        {
            var recipes = new List<Recipe>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM recipes";
            
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
                
                // Get tags
                if (!reader.IsDBNull("tags"))
                {
                    var tagsString = reader.GetString("tags");
                    if (!string.IsNullOrEmpty(tagsString))
                    {
                        recipe.Tags = new List<string>(tagsString.Split(',', StringSplitOptions.RemoveEmptyEntries));
                    }
                }
                
                // Get ingredients
                recipe.Ingredients = GetRecipeIngredients(recipe.Id);
                recipes.Add(recipe);
            }
            
            return recipes;
        }

        public static Recipe GetRecipeById(int id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM recipes WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);
            
            using var reader = command.ExecuteReader();
            if (reader.Read())
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
                
                // Get tags
                if (!reader.IsDBNull("tags"))
                {
                    var tagsString = reader.GetString("tags");
                    if (!string.IsNullOrEmpty(tagsString))
                    {
                        recipe.Tags = new List<string>(tagsString.Split(',', StringSplitOptions.RemoveEmptyEntries));
                    }
                }
                
                // Get ingredients
                recipe.Ingredients = GetRecipeIngredients(id);
                return recipe;
            }
            
            return null;
        }

        private static List<RecipeIngredient> GetRecipeIngredients(int recipeId)
        {
            var ingredients = new List<RecipeIngredient>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ri.ingredient_id, ri.quantity, i.name, i.unit, i.unit_price, i.supplier_name
                FROM recipe_ingredients ri
                JOIN ingredients i ON ri.ingredient_id = i.id
                WHERE ri.recipe_id = @recipeId";
            command.Parameters.AddWithValue("@recipeId", recipeId);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ingredients.Add(new RecipeIngredient
                {
                    IngredientId = reader.GetInt32("ingredient_id"),
                    Quantity = reader.GetDecimal("quantity"),
                    IngredientName = reader.GetString("name"),
                    Unit = reader.GetString("unit"),
                    UnitPrice = reader.GetDecimal("unit_price"),
                    Supplier = reader.IsDBNull("supplier_name") ? "" : reader.GetString("supplier_name")
                });
            }
            
            return ingredients;
        }

        public static int InsertRecipe(Recipe recipe)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            try
            {
                // Insert recipe
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = @"
                        INSERT INTO recipes (name, description, category, tags, batch_yield, target_food_cost_percentage) 
                        VALUES (@name, @description, @category, @tags, @batch_yield, @target_food_cost_percentage);
                        SELECT last_insert_rowid();";
                    
                    command.Parameters.AddWithValue("@name", recipe.Name);
                    command.Parameters.AddWithValue("@description", (object)recipe.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@category", (object)recipe.Category ?? DBNull.Value);
                    command.Parameters.AddWithValue("@tags", recipe.Tags != null ? string.Join(",", recipe.Tags) : DBNull.Value);
                    command.Parameters.AddWithValue("@batch_yield", recipe.BatchYield);
                    command.Parameters.AddWithValue("@target_food_cost_percentage", recipe.TargetFoodCostPercentage);
                    
                    var recipeId = Convert.ToInt32(command.ExecuteScalar());

                    // Insert ingredients
                    if (recipe.Ingredients != null)
                    {
                        foreach (var ingredient in recipe.Ingredients)
                        {
                            using var ingredientCommand = connection.CreateCommand();
                            ingredientCommand.Transaction = transaction;
                            ingredientCommand.CommandText = @"
                                INSERT INTO recipe_ingredients (recipe_id, ingredient_id, quantity) 
                                VALUES (@recipe_id, @ingredient_id, @quantity)";
                            
                            ingredientCommand.Parameters.AddWithValue("@recipe_id", recipeId);
                            ingredientCommand.Parameters.AddWithValue("@ingredient_id", ingredient.IngredientId);
                            ingredientCommand.Parameters.AddWithValue("@quantity", ingredient.Quantity);
                            
                            ingredientCommand.ExecuteNonQuery();
                        }
                    }
                    
                    transaction.Commit();
                    return recipeId;
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static void UpdateRecipe(Recipe recipe)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            try
            {
                // Update recipe
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = @"
                        UPDATE recipes 
                        SET name = @name, description = @description, category = @category, 
                            tags = @tags, batch_yield = @batch_yield, target_food_cost_percentage = @target_food_cost_percentage 
                        WHERE id = @id";
                    
                    command.Parameters.AddWithValue("@id", recipe.Id);
                    command.Parameters.AddWithValue("@name", recipe.Name);
                    command.Parameters.AddWithValue("@description", (object)recipe.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@category", (object)recipe.Category ?? DBNull.Value);
                    command.Parameters.AddWithValue("@tags", recipe.Tags != null ? string.Join(",", recipe.Tags) : DBNull.Value);
                    command.Parameters.AddWithValue("@batch_yield", recipe.BatchYield);
                    command.Parameters.AddWithValue("@target_food_cost_percentage", recipe.TargetFoodCostPercentage);
                    
                    command.ExecuteNonQuery();
                }

                // Delete existing ingredients
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "DELETE FROM recipe_ingredients WHERE recipe_id = @id";
                    command.Parameters.AddWithValue("@id", recipe.Id);
                    command.ExecuteNonQuery();
                }

                // Insert updated ingredients
                if (recipe.Ingredients != null)
                {
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        using var ingredientCommand = connection.CreateCommand();
                        ingredientCommand.Transaction = transaction;
                        ingredientCommand.CommandText = @"
                            INSERT INTO recipe_ingredients (recipe_id, ingredient_id, quantity) 
                            VALUES (@recipe_id, @ingredient_id, @quantity)";
                        
                        ingredientCommand.Parameters.AddWithValue("@recipe_id", recipe.Id);
                        ingredientCommand.Parameters.AddWithValue("@ingredient_id", ingredient.IngredientId);
                        ingredientCommand.Parameters.AddWithValue("@quantity", ingredient.Quantity);
                        
                        ingredientCommand.ExecuteNonQuery();
                    }
                }
                
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static void DeleteRecipe(int id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM recipes WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);
            
            command.ExecuteNonQuery();
        }

        // Settings methods
        public static Dictionary<string, string> GetAllSettings()
        {
            var settings = new Dictionary<string, string>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT key, value FROM settings";
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                settings[reader.GetString("key")] = reader.GetString("value");
            }
            
            return settings;
        }

        public static void SetSetting(string key, string value)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT OR REPLACE INTO settings (key, value) VALUES (@key, @value)";
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@value", value);
            
            command.ExecuteNonQuery();
        }

        // Category methods
        public static List<string> GetRecipeCategories()
        {
            var categories = new List<string>();
            
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT DISTINCT category FROM recipes WHERE category IS NOT NULL AND category != '' ORDER BY category";
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(reader.GetString("category"));
            }
            
            return categories;
        }
    }

    // NEW: Supplier class
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    // NEW: Supplier statistics class
    public class SupplierStats
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int IngredientCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public decimal AveragePrice { get; set; }
    }
}