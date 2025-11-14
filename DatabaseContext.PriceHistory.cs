using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CostChef
{
    public static partial class DatabaseContext
    {
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
    }
}