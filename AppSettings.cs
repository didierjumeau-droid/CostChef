// [file name]: AppSettings.cs
// [file content begin]
using System;
using System.IO;
using System.Collections.Generic;

namespace CostChef
{
    public static class AppSettings
    {
        public static string CurrencySymbol { get; private set; } = "$";
        public static string CurrencyCode { get; private set; } = "USD";
        public static int DecimalPlaces { get; private set; } = 2;
        public static bool AutoSave { get; private set; } = true;
        
        // NEW: Export location
        public static string ExportLocation { get; private set; }

        static AppSettings()
        {
            LoadSettings();
        }

        public static void LoadSettings()
        {
            try
            {
                var settings = DatabaseContext.GetAllSettings();
                
                if (settings.ContainsKey("CurrencySymbol"))
                    CurrencySymbol = settings["CurrencySymbol"];
                
                if (settings.ContainsKey("CurrencyCode"))
                    CurrencyCode = settings["CurrencyCode"];
                
                if (settings.ContainsKey("DecimalPlaces") && int.TryParse(settings["DecimalPlaces"], out int decimalPlaces))
                    DecimalPlaces = decimalPlaces;
                
                if (settings.ContainsKey("AutoSave"))
                    AutoSave = settings["AutoSave"] == "true";

                // NEW: Load export location with default fallback
                if (settings.ContainsKey("ExportLocation") && !string.IsNullOrEmpty(settings["ExportLocation"]))
                {
                    ExportLocation = settings["ExportLocation"];
                }
                else
                {
                    // Set default export location
                    ExportLocation = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "CostChef",
                        "Exports"
                    );
                }

                // Ensure the export directory exists
                try
                {
                    if (!Directory.Exists(ExportLocation))
                    {
                        Directory.CreateDirectory(ExportLocation);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating export directory: {ex.Message}");
                    // Fallback to desktop if the default location fails
                    ExportLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                // Set safe defaults
                CurrencySymbol = "$";
                CurrencyCode = "USD";
                DecimalPlaces = 2;
                AutoSave = true;
                ExportLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
        }

        // NEW: Method to get a safe file path for export
        public static string GetExportFilePath(string defaultFileName, string fileType = "CSV")
        {
            try
            {
                // Ensure export directory exists
                if (!Directory.Exists(ExportLocation))
                {
                    Directory.CreateDirectory(ExportLocation);
                }

                string fileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}";
                string extension = fileType.ToLower() switch
                {
                    "csv" => ".csv",
                    "json" => ".json",
                    _ => ".txt"
                };

                return Path.Combine(ExportLocation, fileName + extension);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting export file path: {ex.Message}");
                // Fallback to desktop
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                );
            }
        }
    }
}
// [file content end]