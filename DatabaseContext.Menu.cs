using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CostChef
{
    /// <summary>
    /// Menu profitability / menu engineering queries.
    /// </summary>
    public static partial class DatabaseContext
    {
        /// <summary>
        /// Returns one row per recipe with computed menu profitability metrics.
        /// Uses:
        ///   - recipes
        ///   - recipe_ingredients
        ///   - ingredients
        /// </summary>
        public static List<MenuProfitabilityRow> GetMenuProfitabilityRows()
        {
            var result = new List<MenuProfitabilityRow>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                // Sum cost for each recipe based on recipe_ingredients × ingredient.unit_price
                // Batch yield and target food cost % are stored on recipes table.
                const string sql = @"
                    SELECT
                        r.id,
                        r.name,
                        r.category,
                        COALESCE(r.batch_yield, 1) AS batch_yield,
                        COALESCE(r.sales_price, 0.0) AS sales_price,
                        COALESCE(r.target_food_cost_percentage, 0.0) AS target_food_cost_percentage,
                        SUM(COALESCE(ri.quantity, 0) * COALESCE(i.unit_price, 0)) AS total_cost
                    FROM recipes r
                    LEFT JOIN recipe_ingredients ri ON ri.recipe_id = r.id
                    LEFT JOIN ingredients i ON i.id = ri.ingredient_id
                    GROUP BY
                        r.id,
                        r.name,
                        r.category,
                        r.batch_yield,
                        r.sales_price,
                        r.target_food_cost_percentage
                    ORDER BY r.name;
                ";

                using (var command = new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int recipeId = Convert.ToInt32(reader["id"]);
                        string name = reader["name"]?.ToString() ?? string.Empty;
                        string category = reader["category"]?.ToString() ?? string.Empty;

                        int batchYield = 1;
                        if (reader["batch_yield"] != DBNull.Value)
                        {
                            try
                            {
                                batchYield = Convert.ToInt32(reader["batch_yield"]);
                            }
                            catch
                            {
                                batchYield = 1;
                            }
                        }
                        if (batchYield <= 0)
                            batchYield = 1;

                        decimal salesPrice = reader["sales_price"] == DBNull.Value
                            ? 0m
                            : Convert.ToDecimal(reader["sales_price"]);

                        decimal targetFood = reader["target_food_cost_percentage"] == DBNull.Value
                            ? 0m
                            : Convert.ToDecimal(reader["target_food_cost_percentage"]);

                        decimal totalCost = reader["total_cost"] == DBNull.Value
                            ? 0m
                            : Convert.ToDecimal(reader["total_cost"]);

                        decimal costPerServing = totalCost;
                        if (batchYield > 0)
                            costPerServing = totalCost / batchYield;

                        decimal foodCostPct = 0m;
                        decimal marginPct = 0m;
                        decimal grossProfit = 0m;

                        if (salesPrice > 0m)
                        {
                            foodCostPct = costPerServing / salesPrice;
                            grossProfit = salesPrice - costPerServing;
                            marginPct = grossProfit / salesPrice;
                        }

                        decimal variance = foodCostPct - targetFood;

                        string status;
                        if (salesPrice <= 0m)
                        {
                            status = "No Price";
                        }
                        else if (totalCost == 0m)
                        {
                            status = "No Cost";
                        }
                        else if (foodCostPct > targetFood + 0.02m)
                        {
                            status = "Above Target";
                        }
                        else if (foodCostPct < targetFood - 0.02m)
                        {
                            status = "Below Target";
                        }
                        else
                        {
                            status = "On Target";
                        }

                        var row = new MenuProfitabilityRow
                        {
                            RecipeId = recipeId,
                            RecipeName = name,
                            Category = category,
                            TotalCost = totalCost,
                            BatchYield = batchYield,
                            CostPerServing = costPerServing,
                            SalesPrice = salesPrice,
                            GrossProfitPerServing = grossProfit,
                            FoodCostPercentage = foodCostPct,             // 0.35 = 35%
                            TargetFoodCostPercentage = targetFood,        // 0.30 = 30%
                            VarianceFoodCostPercentage = variance,        // e.g. +0.05
                            MarginPercentage = marginPct,                 // 0.70 = 70%
                            Status = status
                        };

                        result.Add(row);
                    }
                }
            }

            return result;
        }
    }
}
