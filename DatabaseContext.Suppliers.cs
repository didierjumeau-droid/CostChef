using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public static partial class DatabaseContext
    {
        // ========== SUPPLIER METHODS ==========

        public static List<Supplier> GetAllSuppliers()
        {
            var suppliers = new List<Supplier>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    
                    string query = @"SELECT id, name, contact_person, phone, email, address 
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
                                Address = SafeGetString(reader, "address")
                            };
                            
                            suppliers.Add(supplier);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                                Address = SafeGetString(reader, "address")
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
    }
}