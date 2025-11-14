using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public static partial class DatabaseContext
    {
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
                            var ingredient = new Ingredient
                            {
                                Id = SafeGetInt(reader, "id"),
                                Name = SafeGetString(reader, "name"),
                                Unit = SafeGetString(reader, "unit"),
                                UnitPrice = SafeGetDecimal(reader, "unit_price"),
                                Category = SafeGetString(reader, "category"),
                                SupplierId = SafeGetNullableInt(reader, "supplier_id"),
                                SupplierName = SafeGetString(reader, "supplier_name"),
                                // NEW: Load yield percentage with safe default
                                YieldPercentage = SafeGetDecimal(reader, "yield_percentage") > 0 ? 
                                                 SafeGetDecimal(reader, "yield_percentage") : 1.0m
                            };
                            
                            ingredients.Add(ingredient);
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
                string query = @"INSERT INTO ingredients (name, unit, unit_price, category, supplier_id, yield_percentage) 
                               VALUES (@name, @unit, @unit_price, @category, @supplier_id, @yield_percentage)";

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
                    command.Parameters.AddWithValue("@yield_percentage", ingredient.YieldPercentage); // NEW
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
                                   category = @category, supplier_id = @supplier_id, yield_percentage = @yield_percentage 
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
                    command.Parameters.AddWithValue("@yield_percentage", ingredient.YieldPercentage); // NEW
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

        public static void CleanupOrphanedSupplierRelationships()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    
                    // Find and fix ingredients with supplier_id that don't exist in suppliers table
                    string cleanupQuery = @"
                        UPDATE ingredients 
                        SET supplier_id = NULL 
                        WHERE supplier_id IS NOT NULL 
                        AND supplier_id NOT IN (SELECT id FROM suppliers)";
                    
                    using (var command = new SQLiteCommand(cleanupQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cleaning up supplier relationships: {ex.Message}");
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
    }
}