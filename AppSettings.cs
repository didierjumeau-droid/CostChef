using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CostChef
{
    public static class AppSettings
    {
        private static Dictionary<string, string> _settings = new Dictionary<string, string>();
        private static string _settingsFile = "appsettings.json";

        // Default values
        public static string CurrencySymbol { get; set; } = "$";
        public static string CurrencyCode { get; set; } = "USD";
        public static int DecimalPlaces { get; set; } = 4;
        public static bool ConfirmBeforeDeleting { get; set; } = true;
        public static bool AutoCalculateOnChange { get; set; } = true;
        public static bool AutoConvertUnits { get; set; } = true;
        
        // Unit preferences
        public static string PreferredWeightUnit { get; set; } = "gram";
        public static string PreferredVolumeUnit { get; set; } = "ml";
        public static string PreferredCountUnit { get; set; } = "piece";
        
        // Default categories
        public static string DefaultIngredientCategory { get; set; } = "";
        public static string DefaultRecipeCategory { get; set; } = "";
        public static decimal DefaultFoodCostPercentage { get; set; } = 0.30m;
        
        // Import/Export settings
        public static string ExportPath { get; set; } = "exports/";
        public static string ImportPath { get; set; } = "imports/";

        static AppSettings()
        {
            LoadSettings();
        }

        public static void LoadSettings()
        {
            try
            {
                // First try to load from database
                LoadFromDatabase();
                
                // If database settings don't exist, use defaults
                if (_settings.Count == 0)
                {
                    SetDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                SetDefaultSettings();
            }
        }

        private static void LoadFromDatabase()
        {
            try
            {
                var dbSettings = DatabaseContext.GetAllSettings();
                if (dbSettings != null && dbSettings.Count > 0)
                {
                    _settings = dbSettings;

                    // Map database settings to properties
                    if (_settings.ContainsKey("CurrencySymbol"))
                        CurrencySymbol = _settings["CurrencySymbol"];
                    
                    if (_settings.ContainsKey("CurrencyCode"))
                        CurrencyCode = _settings["CurrencyCode"];
                    
                    if (_settings.ContainsKey("DecimalPlaces") && int.TryParse(_settings["DecimalPlaces"], out int decimalPlaces))
                        DecimalPlaces = decimalPlaces;
                    
                    if (_settings.ContainsKey("ConfirmDeletes") && bool.TryParse(_settings["ConfirmDeletes"], out bool confirmDeletes))
                        ConfirmBeforeDeleting = confirmDeletes;
                    
                    if (_settings.ContainsKey("AutoCalculate") && bool.TryParse(_settings["AutoCalculate"], out bool autoCalculate))
                        AutoCalculateOnChange = autoCalculate;
                    
                    if (_settings.ContainsKey("AutoConvertUnits") && bool.TryParse(_settings["AutoConvertUnits"], out bool autoConvert))
                        AutoConvertUnits = autoConvert;
                    
                    if (_settings.ContainsKey("PreferredWeightUnit"))
                        PreferredWeightUnit = _settings["PreferredWeightUnit"];
                    
                    if (_settings.ContainsKey("PreferredVolumeUnit"))
                        PreferredVolumeUnit = _settings["PreferredVolumeUnit"];
                    
                    if (_settings.ContainsKey("PreferredCountUnit"))
                        PreferredCountUnit = _settings["PreferredCountUnit"];
                    
                    if (_settings.ContainsKey("DefaultIngredientCategory"))
                        DefaultIngredientCategory = _settings["DefaultIngredientCategory"];
                    
                    if (_settings.ContainsKey("DefaultRecipeCategory"))
                        DefaultRecipeCategory = _settings["DefaultRecipeCategory"];
                    
                    if (_settings.ContainsKey("DefaultFoodCost"))
                    {
                        var foodCostStr = _settings["DefaultFoodCost"]?.Replace("%", "");
                        if (decimal.TryParse(foodCostStr, out decimal foodCost))
                            DefaultFoodCostPercentage = foodCost / 100m;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings from database: {ex.Message}");
            }
        }

        private static void SetDefaultSettings()
        {
            CurrencySymbol = "$";
            CurrencyCode = "USD";
            DecimalPlaces = 4;
            ConfirmBeforeDeleting = true;
            AutoCalculateOnChange = true;
            AutoConvertUnits = true;
            PreferredWeightUnit = "gram";
            PreferredVolumeUnit = "ml";
            PreferredCountUnit = "piece";
            DefaultIngredientCategory = "";
            DefaultRecipeCategory = "";
            DefaultFoodCostPercentage = 0.30m;
            ExportPath = "exports/";
            ImportPath = "imports/";
        }

        public static void SaveSettings()
        {
            try
            {
                // Save to database
                DatabaseContext.SetSetting("CurrencySymbol", CurrencySymbol);
                DatabaseContext.SetSetting("CurrencyCode", CurrencyCode);
                DatabaseContext.SetSetting("DecimalPlaces", DecimalPlaces.ToString());
                DatabaseContext.SetSetting("ConfirmDeletes", ConfirmBeforeDeleting.ToString());
                DatabaseContext.SetSetting("AutoCalculate", AutoCalculateOnChange.ToString());
                DatabaseContext.SetSetting("AutoConvertUnits", AutoConvertUnits.ToString());
                DatabaseContext.SetSetting("PreferredWeightUnit", PreferredWeightUnit);
                DatabaseContext.SetSetting("PreferredVolumeUnit", PreferredVolumeUnit);
                DatabaseContext.SetSetting("PreferredCountUnit", PreferredCountUnit);
                DatabaseContext.SetSetting("DefaultIngredientCategory", DefaultIngredientCategory);
                DatabaseContext.SetSetting("DefaultRecipeCategory", DefaultRecipeCategory);
                DatabaseContext.SetSetting("DefaultFoodCost", (DefaultFoodCostPercentage * 100).ToString("0") + "%");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public static string GetExportFilePath(string fileName)
        {
            try
            {
                // Ensure export directory exists
                if (!Directory.Exists(ExportPath))
                {
                    Directory.CreateDirectory(ExportPath);
                }

                return Path.Combine(ExportPath, fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting export file path: {ex.Message}");
                return fileName; // Fallback to current directory
            }
        }

        public static string GetImportFilePath(string fileName)
        {
            try
            {
                // Check if file exists in import directory
                var importFile = Path.Combine(ImportPath, fileName);
                if (File.Exists(importFile))
                {
                    return importFile;
                }

                // Fallback to current directory
                return File.Exists(fileName) ? fileName : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting import file path: {ex.Message}");
                return null;
            }
        }

        // Helper method to format currency
        public static string FormatCurrency(decimal amount)
        {
            return $"{CurrencySymbol}{amount.ToString($"F{DecimalPlaces}")}";
        }

        // Helper method to format percentage
        public static string FormatPercentage(decimal percentage)
        {
            return $"{percentage * 100:F1}%";
        }

        // Reset all settings to defaults
        public static void ResetToDefaults()
        {
            SetDefaultSettings();
            SaveSettings();
        }
    }
}