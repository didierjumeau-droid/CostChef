using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace CostChef
{
    public static partial class DatabaseContext
    {
        /// <summary>
        /// Makes sure inventory-related tables/columns exist and match what the app expects.
        /// Safe to call multiple times.
        /// </summary>
        private static void EnsureInventorySchema(SQLiteConnection connection)
        {
            // Ensure unit_cost columns exist on existing databases
            EnsureColumnExists(connection, "inventory_levels", "unit_cost", "REAL DEFAULT 0");
            EnsureColumnExists(connection, "inventory_history", "unit_cost", "REAL DEFAULT 0");

            // Ensure snapshot tables exist (full per-item snapshots)
            using (var cmd = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS inventory_snapshots (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    snapshot_date TEXT NOT NULL
                );
            ", connection))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS inventory_snapshot_items (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    snapshot_id INTEGER NOT NULL,
                    ingredient_id INTEGER NOT NULL,
                    stock REAL NOT NULL,
                    unit_cost REAL NOT NULL,
                    total_value REAL NOT NULL,
                    FOREIGN KEY (snapshot_id) REFERENCES inventory_snapshots(id)
                );
            ", connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void EnsureColumnExists(SQLiteConnection connection, string table, string column, string typeDefinition)
        {
            bool exists = false;

            using (var pragma = new SQLiteCommand($"PRAGMA table_info({table});", connection))
            using (var reader = pragma.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader["name"]?.ToString();
                    if (string.Equals(name, column, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
            }

            if (!exists)
            {
                using (var alter = new SQLiteCommand(
                    $"ALTER TABLE {table} ADD COLUMN {column} {typeDefinition};", connection))
                {
                    alter.ExecuteNonQuery();
                }

                // Backfill unit_cost from ingredients.unit_price when possible
                if (table == "inventory_levels" && column == "unit_cost")
                {
                    using (var backfill = new SQLiteCommand(@"
                        UPDATE inventory_levels
                        SET unit_cost = (
                            SELECT unit_price
                            FROM ingredients
                            WHERE ingredients.id = inventory_levels.ingredient_id
                        )
                        WHERE unit_cost IS NULL OR unit_cost = 0;
                    ", connection))
                    {
                        backfill.ExecuteNonQuery();
                    }
                }
            }
        }

        // --------------------------------------------------------------------
        // NEW: Ensure every ingredient has an inventory row
        // --------------------------------------------------------------------
        private static void EnsureInventoryRowsForAllIngredients(SQLiteConnection connection)
        {
            const string sql = @"
                INSERT INTO inventory_levels
                    (ingredient_id, current_stock, minimum_stock, maximum_stock, last_updated, unit_cost)
                SELECT
                    i.id,
                    0,
                    NULL,
                    NULL,
                    CURRENT_TIMESTAMP,
                    COALESCE(i.unit_price, 0)
                FROM ingredients i
                LEFT JOIN inventory_levels il
                    ON il.ingredient_id = i.id
                WHERE il.ingredient_id IS NULL;
            ";

            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        // --------------------------------------------------------------------
        // CORE INVENTORY QUERIES
        // --------------------------------------------------------------------

        public static List<InventoryLevel> GetInventoryLevels()
        {
            var result = new List<InventoryLevel>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                EnsureInventorySchema(connection);

                // NEW: make sure there is one row per ingredient
                EnsureInventoryRowsForAllIngredients(connection);

                const string sql = @"
                    SELECT
                        il.id,
                        il.ingredient_id,
                        i.name AS ingredient_name,
                        i.unit AS unit,
                        il.current_stock,
                        il.minimum_stock,
                        il.maximum_stock,
                        il.last_updated,
                        COALESCE(il.unit_cost, i.unit_price, 0) AS unit_cost
                    FROM inventory_levels il
                    INNER JOIN ingredients i ON i.id = il.ingredient_id
                    ORDER BY i.name;
                ";

                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var level = new InventoryLevel
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                            IngredientName = reader["ingredient_name"]?.ToString() ?? string.Empty,
                            Unit = reader["unit"]?.ToString() ?? string.Empty,
                            CurrentStock = reader["current_stock"] == DBNull.Value
                                ? 0m
                                : Convert.ToDecimal(reader["current_stock"]),
                            MinimumStock = reader["minimum_stock"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["minimum_stock"]),
                            MaximumStock = reader["maximum_stock"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["maximum_stock"]),
                            LastUpdated = reader["last_updated"] == DBNull.Value
                                ? DateTime.MinValue
                                : Convert.ToDateTime(reader["last_updated"]),
                            UnitCost = reader["unit_cost"] == DBNull.Value
                                ? 0m
                                : Convert.ToDecimal(reader["unit_cost"])
                        };

                        result.Add(level);
                    }
                }
            }

            return result;
        }

        public static List<InventoryLevel> GetLowStockItems()
        {
            var result = new List<InventoryLevel>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                EnsureInventorySchema(connection);

                const string sql = @"
                    SELECT
                        il.id,
                        il.ingredient_id,
                        i.name AS ingredient_name,
                        i.unit AS unit,
                        il.current_stock,
                        il.minimum_stock,
                        il.maximum_stock,
                        il.last_updated,
                        COALESCE(il.unit_cost, i.unit_price, 0) AS unit_cost
                    FROM inventory_levels il
                    INNER JOIN ingredients i ON i.id = il.ingredient_id
                    WHERE il.minimum_stock IS NOT NULL
                      AND il.current_stock <= il.minimum_stock
                    ORDER BY i.name;
                ";

                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var level = new InventoryLevel
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                            IngredientName = reader["ingredient_name"]?.ToString() ?? string.Empty,
                            Unit = reader["unit"]?.ToString() ?? string.Empty,
                            CurrentStock = reader["current_stock"] == DBNull.Value
                                ? 0m
                                : Convert.ToDecimal(reader["current_stock"]),
                            MinimumStock = reader["minimum_stock"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["minimum_stock"]),
                            MaximumStock = reader["maximum_stock"] == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(reader["maximum_stock"]),
                            LastUpdated = reader["last_updated"] == DBNull.Value
                                ? DateTime.MinValue
                                : Convert.ToDateTime(reader["last_updated"]),
                            UnitCost = reader["unit_cost"] == DBNull.Value
                                ? 0m
                                : Convert.ToDecimal(reader["unit_cost"])
                        };

                        result.Add(level);
                    }
                }
            }

            return result;
        }

        public static decimal GetTotalInventoryValue()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                EnsureInventorySchema(connection);

                const string sql = @"
                    SELECT SUM(current_stock * unit_cost) AS total_value
                    FROM inventory_levels;
                ";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var obj = cmd.ExecuteScalar();
                    if (obj == null || obj == DBNull.Value)
                        return 0m;

                    return Convert.ToDecimal(obj);
                }
            }
        }

        // --------------------------------------------------------------------
        // UPDATE + HISTORY
        // --------------------------------------------------------------------

        public static void UpdateInventoryLevel(
            int ingredientId,
            decimal newStock,
            decimal? minStock = null,
            decimal? maxStock = null,
            string changeType = "adjustment",
            string? reason = null,
            int? recipeId = null)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                EnsureInventorySchema(connection);

                decimal previousStock = 0m;
                decimal unitCost = 0m;
                bool exists = false;

                // Get existing stock and unit_cost (or ingredient.unit_price)
                const string getSql = @"
                    SELECT il.current_stock,
                           COALESCE(il.unit_cost, i.unit_price, 0) AS unit_cost
                    FROM inventory_levels il
                    INNER JOIN ingredients i ON i.id = il.ingredient_id
                    WHERE il.ingredient_id = @ingredientId;
                ";

                using (var getCmd = new SQLiteCommand(getSql, connection))
                {
                    getCmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                    using (var reader = getCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            exists = true;
                            previousStock = reader["current_stock"] == DBNull.Value
                                ? 0m
                                : Convert.ToDecimal(reader["current_stock"]);
                            unitCost = reader["unit_cost"] == DBNull.Value
                                ? 0m
                                : Convert.ToDecimal(reader["unit_cost"]);
                        }
                    }
                }

                // If missing, create inventory_levels row
                if (!exists)
                {
                    const string priceSql = "SELECT unit_price FROM ingredients WHERE id = @ingredientId;";
                    using (var pcmd = new SQLiteCommand(priceSql, connection))
                    {
                        pcmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                        var priceObj = pcmd.ExecuteScalar();
                        if (priceObj != null && priceObj != DBNull.Value)
                            unitCost = Convert.ToDecimal(priceObj);
                    }

                    const string insertSql = @"
                        INSERT INTO inventory_levels
                            (ingredient_id, current_stock, minimum_stock, maximum_stock, last_updated, unit_cost)
                        VALUES
                            (@ingredientId, @currentStock, @minStock, @maxStock, CURRENT_TIMESTAMP, @unitCost);
                    ";

                    using (var insertCmd = new SQLiteCommand(insertSql, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                        insertCmd.Parameters.AddWithValue("@currentStock", newStock);
                        insertCmd.Parameters.AddWithValue("@minStock", (object?)minStock ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@maxStock", (object?)maxStock ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@unitCost", unitCost);
                        insertCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    const string updateSql = @"
                        UPDATE inventory_levels
                        SET current_stock = @newStock,
                            minimum_stock = COALESCE(@minStock, minimum_stock),
                            maximum_stock = COALESCE(@maxStock, maximum_stock),
                            last_updated  = CURRENT_TIMESTAMP,
                            unit_cost     = COALESCE(unit_cost, @unitCost)
                        WHERE ingredient_id = @ingredientId;
                    ";

                    using (var updateCmd = new SQLiteCommand(updateSql, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@newStock", newStock);
                        updateCmd.Parameters.AddWithValue("@minStock", (object?)minStock ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@maxStock", (object?)maxStock ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@unitCost", unitCost);
                        updateCmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                // Insert history row
                decimal changeAmount = newStock - previousStock;

                const string historySql = @"
                    INSERT INTO inventory_history
                        (ingredient_id, previous_stock, new_stock, change_amount,
                         change_type, change_date, reason, recipe_id, unit_cost)
                    VALUES
                        (@ingredientId, @previousStock, @newStock, @changeAmount,
                         @changeType, CURRENT_TIMESTAMP, @reason, @recipeId, @unitCost);
                ";

                using (var hcmd = new SQLiteCommand(historySql, connection))
                {
                    hcmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                    hcmd.Parameters.AddWithValue("@previousStock", previousStock);
                    hcmd.Parameters.AddWithValue("@newStock", newStock);
                    hcmd.Parameters.AddWithValue("@changeAmount", changeAmount);
                    hcmd.Parameters.AddWithValue("@changeType", changeType);
                    hcmd.Parameters.AddWithValue("@reason", (object?)reason ?? DBNull.Value);
                    hcmd.Parameters.AddWithValue("@recipeId", (object?)recipeId ?? DBNull.Value);
                    hcmd.Parameters.AddWithValue("@unitCost", unitCost);
                    hcmd.ExecuteNonQuery();
                }
            }
        }

        public static List<InventoryHistory> GetInventoryHistory(
            int ingredientId,
            string filterType = "All",
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var result = new List<InventoryHistory>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                EnsureInventorySchema(connection);

                var sql = @"
                    SELECT
                        ih.id,
                        ih.ingredient_id,
                        i.name AS ingredient_name,
                        ih.previous_stock,
                        ih.new_stock,
                        ih.change_amount,
                        ih.change_type,
                        ih.change_date,
                        ih.reason,
                        ih.recipe_id,
                        COALESCE(ih.unit_cost, ing.unit_price, 0) AS unit_cost
                    FROM inventory_history ih
                    INNER JOIN ingredients i   ON i.id   = ih.ingredient_id
                    INNER JOIN ingredients ing ON ing.id = ih.ingredient_id
                    WHERE ih.ingredient_id = @ingredientId
                ";

                if (!string.Equals(filterType, "All", StringComparison.OrdinalIgnoreCase))
                {
                    sql += " AND ih.change_type = @changeType";
                }

                if (startDate.HasValue)
                {
                    sql += " AND DATE(ih.change_date) >= DATE(@startDate)";
                }

                if (endDate.HasValue)
                {
                    sql += " AND DATE(ih.change_date) <= DATE(@endDate)";
                }

                sql += " ORDER BY ih.change_date DESC;";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                    if (!string.Equals(filterType, "All", StringComparison.OrdinalIgnoreCase))
                        cmd.Parameters.AddWithValue("@changeType", filterType);
                    if (startDate.HasValue)
                        cmd.Parameters.AddWithValue("@startDate", startDate.Value);
                    if (endDate.HasValue)
                        cmd.Parameters.AddWithValue("@endDate", endDate.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new InventoryHistory
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                IngredientId = Convert.ToInt32(reader["ingredient_id"]),
                                IngredientName = reader["ingredient_name"]?.ToString() ?? string.Empty,
                                PreviousStock = reader["previous_stock"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(reader["previous_stock"]),
                                NewStock = reader["new_stock"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(reader["new_stock"]),
                                ChangeAmount = reader["change_amount"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(reader["change_amount"]),
                                ChangeType = reader["change_type"]?.ToString() ?? string.Empty,
                                ChangeDate = reader["change_date"] == DBNull.Value
                                    ? DateTime.MinValue
                                    : Convert.ToDateTime(reader["change_date"]),
                                Reason = reader["reason"] == DBNull.Value
                                    ? string.Empty
                                    : reader["reason"]?.ToString() ?? string.Empty,
                                RecipeId = reader["recipe_id"] == DBNull.Value
                                    ? (int?)null
                                    : Convert.ToInt32(reader["recipe_id"]),
                                UnitCost = reader["unit_cost"] == DBNull.Value
                                    ? 0m
                                    : Convert.ToDecimal(reader["unit_cost"])
                            };

                            result.Add(item);
                        }
                    }
                }
            }

            return result;
        }

        // --------------------------------------------------------------------
        // SNAPSHOTS
        // --------------------------------------------------------------------

        public static int TakeInventorySnapshot()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                EnsureInventorySchema(connection);

                // 1. Create snapshot row
                int snapshotId;
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO inventory_snapshots (snapshot_date) VALUES (CURRENT_TIMESTAMP);", connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SQLiteCommand("SELECT last_insert_rowid();", connection))
                {
                    snapshotId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 2. Insert per-item details
                const string itemsSql = @"
                    INSERT INTO inventory_snapshot_items
                        (snapshot_id, ingredient_id, stock, unit_cost, total_value)
                    SELECT
                        @snapshotId,
                        il.ingredient_id,
                        il.current_stock,
                        il.unit_cost,
                        (il.current_stock * il.unit_cost)
                    FROM inventory_levels il;
                ";

                using (var cmd = new SQLiteCommand(itemsSql, connection))
                {
                    cmd.Parameters.AddWithValue("@snapshotId", snapshotId);
                    cmd.ExecuteNonQuery();
                }

                return snapshotId;
            }
        }

        public static InventorySnapshot? GetLatestSnapshotSummary()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                EnsureInventorySchema(connection);

                const string sql = @"
                    SELECT
                        s.id,
                        s.snapshot_date,
                        COALESCE(SUM(isi.total_value), 0) AS total_value,
                        COUNT(DISTINCT isi.ingredient_id) AS ingredient_count
                    FROM inventory_snapshots s
                    LEFT JOIN inventory_snapshot_items isi
                        ON isi.snapshot_id = s.id
                    GROUP BY s.id, s.snapshot_date
                    ORDER BY s.snapshot_date DESC
                    LIMIT 1;
                ";

                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new InventorySnapshot
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        SnapshotDate = reader["snapshot_date"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(reader["snapshot_date"]),
                        TotalValue = reader["total_value"] == DBNull.Value
                            ? 0m
                            : Convert.ToDecimal(reader["total_value"]),
                        IngredientCount = reader["ingredient_count"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["ingredient_count"])
                    };
                }
            }
        }
    }
}
