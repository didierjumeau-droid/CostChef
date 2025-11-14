using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CostChef
{
    public static partial class DatabaseContext
    {
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
    }
}