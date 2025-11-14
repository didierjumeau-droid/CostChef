using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace CostChef
{
    public static class DatabaseMigration
    {
        public static void MigrateToV2_2()
        {
            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    connection.Open();
                    
                    // Check if yield columns already exist
                    bool needsMigration = !ColumnExists(connection, "ingredients", "trim_waste_percentage");
                    
                    if (needsMigration)
                    {
                        MessageBox.Show("Updating database for Yield Management features...", "Database Update", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Add yield management columns to ingredients table
                        string[] migrationQueries = {
                            "ALTER TABLE ingredients ADD COLUMN trim_waste_percentage REAL DEFAULT 0",
                            "ALTER TABLE ingredients ADD COLUMN cooking_loss_percentage REAL DEFAULT 0", 
                            "ALTER TABLE ingredients ADD COLUMN is_multi_pack BOOLEAN DEFAULT 0",
                            "ALTER TABLE ingredients ADD COLUMN multi_pack_quantity INTEGER DEFAULT 1",
                            "ALTER TABLE ingredients ADD COLUMN multi_pack_price REAL DEFAULT 0"
                        };
                        
                        foreach (string query in migrationQueries)
                        {
                            using (var command = new SQLiteCommand(query, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                        
                        MessageBox.Show("Database updated successfully for Yield Management!", "Update Complete", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database migration error: {ex.Message}\n\nSome yield management features may not work correctly.", 
                    "Migration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private static bool ColumnExists(SQLiteConnection connection, string table, string column)
        {
            try
            {
                string query = $"PRAGMA table_info({table})";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["name"].ToString() == column)
                            return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}