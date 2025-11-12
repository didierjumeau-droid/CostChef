using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Data.SQLite;

namespace CostChef
{
    public static class RecipeVersioningService
    {
        public class RecipeVersionData
        {
            public Recipe Recipe { get; set; }
            public List<RecipeIngredient> Ingredients { get; set; }
            public decimal TotalCost { get; set; }
            public decimal CostPerServing { get; set; }
        }

        public static void CreateVersion(int recipeId, string versionName = "", string versionNotes = "", string createdBy = "System")
        {
            try
            {
                // Get current recipe and ingredients
                var recipe = DatabaseContext.GetAllRecipes().FirstOrDefault(r => r.Id == recipeId);
                if (recipe == null) 
                {
                    System.Diagnostics.Debug.WriteLine($"Recipe not found: {recipeId}");
                    return;
                }

                var ingredients = DatabaseContext.GetRecipeIngredients(recipeId);
                
                // Calculate current version number
                int nextVersion = GetNextVersionNumber(recipeId);
                
                // Mark previous versions as not current
                MarkPreviousVersionsAsNotCurrent(recipeId);
                
                // Create version data
                var versionData = new RecipeVersionData
                {
                    Recipe = recipe,
                    Ingredients = ingredients,
                    TotalCost = ingredients.Sum(i => i.LineCost),
                    CostPerServing = recipe.BatchYield > 0 ? ingredients.Sum(i => i.LineCost) / recipe.BatchYield : 0
                };

                // Serialize to JSON
                string recipeJson = JsonSerializer.Serialize(versionData, new JsonSerializerOptions { WriteIndented = true });

                // Save to database
                SaveRecipeVersion(recipeId, nextVersion, versionName, versionNotes, createdBy, recipeJson, true);
                
                System.Diagnostics.Debug.WriteLine($"Version created for recipe {recipeId}: {versionName} (v{nextVersion})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating recipe version: {ex.Message}");
                // Don't throw - versioning shouldn't break the main functionality
            }
        }

        private static int GetNextVersionNumber(int recipeId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                connection.Open();
                string query = "SELECT COALESCE(MAX(version_number), 0) + 1 FROM recipe_versions WHERE recipe_id = @recipeId";
                
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@recipeId", recipeId);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        private static void MarkPreviousVersionsAsNotCurrent(int recipeId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                connection.Open();
                string query = "UPDATE recipe_versions SET is_current = 0 WHERE recipe_id = @recipeId";
                
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@recipeId", recipeId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void SaveRecipeVersion(int recipeId, int versionNumber, string versionName, string versionNotes, string createdBy, string recipeJson, bool isCurrent)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                connection.Open();
                string query = @"
                    INSERT INTO recipe_versions (recipe_id, version_number, version_name, version_notes, created_by, recipe_data, is_current)
                    VALUES (@recipeId, @versionNumber, @versionName, @versionNotes, @createdBy, @recipeData, @isCurrent)";
                
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@recipeId", recipeId);
                    command.Parameters.AddWithValue("@versionNumber", versionNumber);
                    command.Parameters.AddWithValue("@versionName", versionName ?? $"Version {versionNumber}");
                    command.Parameters.AddWithValue("@versionNotes", versionNotes ?? "");
                    command.Parameters.AddWithValue("@createdBy", createdBy);
                    command.Parameters.AddWithValue("@recipeData", recipeJson);
                    command.Parameters.AddWithValue("@isCurrent", isCurrent);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<RecipeVersion> GetRecipeVersions(int recipeId)
        {
            var versions = new List<RecipeVersion>();
            
            using (var connection = DatabaseContext.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT id, recipe_id, version_number, version_name, version_notes, created_date, created_by, is_current, recipe_data
                    FROM recipe_versions 
                    WHERE recipe_id = @recipeId 
                    ORDER BY version_number DESC";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@recipeId", recipeId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            versions.Add(new RecipeVersion
                            {
                                Id = SafeGetInt(reader, "id"),
                                RecipeId = SafeGetInt(reader, "recipe_id"),
                                VersionNumber = SafeGetInt(reader, "version_number"),
                                VersionName = SafeGetString(reader, "version_name"),
                                VersionNotes = SafeGetString(reader, "version_notes"),
                                CreatedDate = SafeGetDateTime(reader, "created_date"),
                                CreatedBy = SafeGetString(reader, "created_by"),
                                IsCurrent = SafeGetBool(reader, "is_current"),
                                RecipeData = SafeGetString(reader, "recipe_data")
                            });
                        }
                    }
                }
            }

            return versions;
        }

        public static RecipeVersionData RestoreVersion(int versionId)
        {
            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT recipe_data FROM recipe_versions WHERE id = @versionId";
                    
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@versionId", versionId);
                        var result = command.ExecuteScalar();
                        
                        if (result != null)
                        {
                            return JsonSerializer.Deserialize<RecipeVersionData>(result.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring recipe version: {ex.Message}");
            }

            return null;
        }

        // Helper methods for safe data reading
        private static string SafeGetString(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
            }
            catch
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
            catch
            {
                return 0;
            }
        }

        private static bool SafeGetBool(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? false : reader.GetBoolean(ordinal);
            }
            catch
            {
                return false;
            }
        }

        private static DateTime SafeGetDateTime(SQLiteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return DateTime.MinValue;
                
                var value = reader.GetValue(ordinal);
                if (value is DateTime dateTime)
                    return dateTime;
                if (value is string dateString && DateTime.TryParse(dateString, out DateTime parsedDate))
                    return parsedDate;
                    
                return DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }

    public class RecipeVersion
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int VersionNumber { get; set; }
        public string VersionName { get; set; }
        public string VersionNotes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public bool IsCurrent { get; set; }
        public string RecipeData { get; set; }

        // Computed properties
        public string DisplayName => $"{VersionName} (v{VersionNumber})";
        public string CreatedDisplay => CreatedDate.ToString("yyyy-MM-dd HH:mm");
        
        // ADD THIS MISSING PROPERTY - it's referenced in your DataGridView
        public string CreatedByDisplay => CreatedBy ?? "System";
        
        public RecipeVersioningService.RecipeVersionData GetVersionData()
        {
            try
            {
                return JsonSerializer.Deserialize<RecipeVersioningService.RecipeVersionData>(RecipeData);
            }
            catch
            {
                return null;
            }
        }
    }
}