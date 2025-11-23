using System;
using System.Windows.Forms;
using System.Data.SQLite;

namespace CostChef
{
    public static class DatabaseSchemaFix
    {
        public static void CheckAndFixSchema()
        {
            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    connection.Open();

                    // Ensure ingredients.yield_percentage exists
                    EnsureColumn(
                        connection,
                        "ingredients",
                        "yield_percentage",
                        "REAL NOT NULL DEFAULT 1.0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error checking/updating database schema:\n{ex.Message}",
                    "Database Schema Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Ensures a given column exists in a table, otherwise adds it.
        /// </summary>
        private static void EnsureColumn(
            SQLiteConnection connection,
            string table,
            string column,
            string typeDefinition)
        {
            if (!ColumnExists(connection, table, column))
            {
                AddColumn(connection, table, column, typeDefinition);
            }
        }

        /// <summary>
        /// Returns true if the given column exists in the specified table.
        /// </summary>
        private static bool ColumnExists(SQLiteConnection connection, string table, string column)
        {
            string pragma = $"PRAGMA table_info({table});";
            using (var cmd = new SQLiteCommand(pragma, connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var colName = reader["name"]?.ToString();
                    if (string.Equals(colName, column, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a column to the specified table.
        /// </summary>
        private static void AddColumn(SQLiteConnection connection, string table, string column, string typeDefinition)
        {
            string alterQuery = $"ALTER TABLE {table} ADD COLUMN {column} {typeDefinition};";
            using (var command = new SQLiteCommand(alterQuery, connection))
            {
                command.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"Added column {column} to {table}");
            }
        }
    }
}
