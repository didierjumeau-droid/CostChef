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

                // Create ingredients table
                string createIngredientsTable = @"
                    CREATE TABLE IF NOT EXISTS ingredients (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        unit TEXT NOT NULL,
                        unit_price REAL NOT NULL,
                        category TEXT,
                        supplier_id INTEGER
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

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = createIngredientsTable;
                    command.ExecuteNonQuery();

                    command.CommandText = createRecipesTable;
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

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM suppliers ORDER BY name";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suppliers.Add(new Supplier
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString(),
                            ContactPerson = reader["contact_person"]?.ToString(),
                            Phone = reader["phone"]?.ToString(),
                            Email = reader["email"]?.ToString(),
                            Address = reader["address"]?.ToString(),
                            CreatedAt = reader["created_at"]?.ToString()
                        });
                    }
                }
            }

            return suppliers;
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
                
                string updateIngredients = "UPDATE ingredients SET supplier_id = NULL WHERE supplier_id = @supplierId";
                using (var command = new SQLiteCommand(updateIngredients, connection))
                {
                    command.Parameters.AddWithValue("@supplierId", supplierId);
                    command.ExecuteNonQuery();
                }

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
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                ContactPerson = reader["contact_person"]?.ToString(),
                                Phone = reader["phone"]?.ToString(),
                                Email = reader["email"]?.ToString(),
                                Address = reader["address"]?.ToString(),
                                CreatedAt = reader["created_at"]?.ToString()
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
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                Unit = reader["unit"].ToString(),
                                UnitPrice = Convert.ToDecimal(reader["unit_price"]),
                                Category = reader["category"]?.ToString(),
                                SupplierId = reader["supplier_id"] != DBNull.Value ? Convert.ToInt32(reader["supplier_id"]) : (int?)null
                            });
                        }
                    }
                }
            }

            return ingredients;
        }

        public static dynamic GetSupplierStatistics(int supplierId)
        {
            var ingredients = GetIngredientsBySupplier(supplierId);
            return new
            {
                IngredientCount = ingredients.Count,
                TotalValue = ingredients.Sum(i => i.UnitPrice)
            };
        }

        // ========== INGREDIENT METHODS ==========

        public static List<Ingredient> GetAllIngredients()
        {
            var ingredients = new List<Ingredient>();

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
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString(),
                            Unit = reader["unit"].ToString(),
                            UnitPrice = Convert.ToDecimal(reader["unit_price"]),
                            Category = reader["category"]?.ToString(),
                            SupplierId = reader["supplier_id"] != DBNull.Value ? Convert.ToInt32(reader["supplier_id"]) : (int?)null,
                            SupplierName = reader["supplier_name"]?.ToString()
                        });
                    }
                }
            }

            return ingredients;
        }

        public static void InsertIngredient(Ingredient ingredient)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO ingredients (name, unit, unit_price, category, supplier_id) VALUES (@name, @unit, @unit_price, @category, @supplier_id)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", ingredient.Name);
                    command.Parameters.AddWithValue("@unit", ingredient.Unit);
                    command.Parameters.AddWithValue("@unit_price", ingredient.UnitPrice);
                    command.Parameters.AddWithValue("@category", ingredient.Category ?? "");
                    command.Parameters.AddWithValue("@supplier_id", ingredient.SupplierId ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateIngredient(Ingredient ingredient)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "UPDATE ingredients SET name = @name, unit = @unit, unit_price = @unit_price, category = @category, supplier_id = @supplier_id WHERE id = @id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", ingredient.Name);
                    command.Parameters.AddWithValue("@unit", ingredient.Unit);
                    command.Parameters.AddWithValue("@unit_price", ingredient.UnitPrice);
                    command.Parameters.AddWithValue("@category", ingredient.Category ?? "");
                    command.Parameters.AddWithValue("@supplier_id", ingredient.SupplierId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@id", ingredient.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteIngredient(int ingredientId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string deleteRecipeIngredients = "DELETE FROM recipe_ingredients WHERE ingredient_id = @ingredientId";
                using (var command = new SQLiteCommand(deleteRecipeIngredients, connection))
                {
                    command.Parameters.AddWithValue("@ingredientId", ingredientId);
                    command.ExecuteNonQuery();
                }

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

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT DISTINCT category FROM ingredients WHERE category IS NOT NULL AND category != '' ORDER BY category";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(reader["category"].ToString());
                    }
                }
            }

            return categories;
        }

        // ========== RECIPE METHODS ==========

        public static List<Recipe> GetAllRecipes()
        {
            var recipes = new List<Recipe>();

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
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString(),
                            Description = reader["description"]?.ToString(),
                            Category = reader["category"]?.ToString(),
                            Tags = reader["tags"]?.ToString(),
                            BatchYield = Convert.ToInt32(reader["batch_yield"]),
                            TargetFoodCostPercentage = Convert.ToDecimal(reader["target_food_cost_percentage"])
                        });
                    }
                }
            }

            return recipes;
        }

        public static void InsertRecipe(Recipe recipe)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO recipes (name, description, category, tags, batch_yield, target_food_cost_percentage) VALUES (@name, @description, @category, @tags, @batch_yield, @target_food_cost_percentage)";

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
                string query = "UPDATE recipes SET name = @name, description = @description, category = @category, tags = @tags, batch_yield = @batch_yield, target_food_cost_percentage = @target_food_cost_percentage WHERE id = @id";

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

                string deleteRecipeIngredients = "DELETE FROM recipe_ingredients WHERE recipe_id = @recipeId";
                using (var command = new SQLiteCommand(deleteRecipeIngredients, connection))
                {
                    command.Parameters.AddWithValue("@recipeId", recipeId);
                    command.ExecuteNonQuery();
                }

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

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT DISTINCT category FROM recipes WHERE category IS NOT NULL AND category != '' ORDER BY category";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(reader["category"].ToString());
                    }
                }
            }

            return categories;
        }

        public static List<RecipeIngredient> GetRecipeIngredients(int recipeId)
        {
            var recipeIngredients = new List<RecipeIngredient>();

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
                            var unitPrice = Convert.ToDecimal(reader["unit_price"]);
                            var quantity = Convert.ToDecimal(reader["quantity"]);
                            
                            recipeIngredients.Add(new RecipeIngredient
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                RecipeId = Convert.ToInt32(reader["recipe_id"]),
                                IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                                Quantity = quantity,
                                IngredientName = reader["ingredient_name"].ToString(),
                                Unit = reader["unit"].ToString(),
                                UnitPrice = unitPrice,
                                Supplier = reader["supplier_name"]?.ToString(),
                                
                            });
                        }
                    }
                }
            }

            return recipeIngredients;
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
    }
}