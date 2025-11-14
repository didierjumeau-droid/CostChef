using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace CostChef
{
    public static partial class DatabaseContext
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