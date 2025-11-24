using System;
using System.Data.SQLite;

namespace CostChef
{
    public static partial class DatabaseContext
    {
        /// <summary>
        /// Update ingredient unit price based on a purchase.
        /// - Only updates price if supplier matches, or ingredient has no supplier yet.
        /// - Writes to price_history if price actually changed.
        /// </summary>
        public static void UpdateIngredientPriceFromPurchase(
            int ingredientId,
            decimal newUnitPrice,
            int? supplierId,
            string? reason = null)
        {
            if (newUnitPrice <= 0)
                return;

            using (var connection = GetConnection())
            {
                connection.Open();

                decimal oldPrice = 0m;
                int? existingSupplierId = null;

                // Get current price and supplier
                using (var cmd = new SQLiteCommand(
                    "SELECT unit_price, supplier_id FROM ingredients WHERE id = @id",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@id", ingredientId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            // Ingredient not found
                            return;
                        }

                        oldPrice = reader.IsDBNull(0) ? 0m : Convert.ToDecimal(reader.GetValue(0));
                        if (!reader.IsDBNull(1))
                            existingSupplierId = Convert.ToInt32(reader.GetValue(1));
                    }
                }

                // Supplier logic:
                // - If no supplier is selected, we still allow price update.
                // - If ingredient has no supplier yet and a supplier is selected -> assign it.
                // - If ingredient has a supplier and it differs from selected supplier -> DO NOT update price.
                bool canUpdatePrice = true;
                int? newSupplierId = existingSupplierId;

                if (supplierId.HasValue && supplierId.Value > 0)
                {
                    if (!existingSupplierId.HasValue || existingSupplierId.Value == 0)
                    {
                        // First supplier assignment for this ingredient
                        newSupplierId = supplierId.Value;
                    }
                    else if (existingSupplierId.Value != supplierId.Value)
                    {
                        // Different supplier -> respect your rule: do NOT override price
                        // You will handle this by creating a new ingredient if needed.
                        canUpdatePrice = false;
                    }
                }

                if (!canUpdatePrice)
                    return;

                // If price and supplier are unchanged, nothing to do
                if (oldPrice == newUnitPrice &&
                    (!supplierId.HasValue || existingSupplierId == supplierId))
                {
                    return;
                }

                // Update ingredient price (and supplier if needed)
                string updateSql = "UPDATE ingredients SET unit_price = @price";
                if (newSupplierId.HasValue && newSupplierId.Value > 0)
                    updateSql += ", supplier_id = @supplierId";
                updateSql += " WHERE id = @id";

                using (var updateCmd = new SQLiteCommand(updateSql, connection))
                {
                    updateCmd.Parameters.AddWithValue("@price", newUnitPrice);
                    updateCmd.Parameters.AddWithValue("@id", ingredientId);

                    if (newSupplierId.HasValue && newSupplierId.Value > 0)
                        updateCmd.Parameters.AddWithValue("@supplierId", newSupplierId.Value);

                    updateCmd.ExecuteNonQuery();
                }

                // Only log history if the price actually changed
                if (oldPrice != newUnitPrice)
                {
                    using (var historyCmd = new SQLiteCommand(
                        @"INSERT INTO price_history 
                          (ingredient_id, old_price, new_price, change_date, changed_by, reason)
                          VALUES (@ingredientId, @oldPrice, @newPrice, CURRENT_TIMESTAMP, 'PurchaseEntry', @reason)",
                        connection))
                    {
                        historyCmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                        historyCmd.Parameters.AddWithValue("@oldPrice", oldPrice);
                        historyCmd.Parameters.AddWithValue("@newPrice", newUnitPrice);
                        historyCmd.Parameters.AddWithValue("@reason", (object?)reason ?? DBNull.Value);
                        historyCmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
