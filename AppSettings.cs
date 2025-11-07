using System;
using System.Collections.Generic;

namespace CostChef
{
    public static class AppSettings
    {
        private static Dictionary<string, string> _settings = new Dictionary<string, string>();

        public static string CurrencySymbol => Get("CurrencySymbol", "$");
        public static string CurrencyCode => Get("CurrencyCode", "USD");
        public static int DecimalPlaces => int.TryParse(Get("DecimalPlaces", "2"), out int result) ? result : 2;
        public static bool AutoSave => Get("AutoSave", "true").ToLower() == "true";

        static AppSettings()
        {
            LoadSettings();
        }

        public static void LoadSettings()
        {
            _settings = DatabaseContext.GetAllSettings();
        }

        public static void SaveSettings()
        {
            foreach (var setting in _settings)
            {
                DatabaseContext.SetSetting(setting.Key, setting.Value);
            }
        }

        public static string Get(string key, string defaultValue = "")
        {
            return _settings.ContainsKey(key) ? _settings[key] : defaultValue;
        }

        public static void Set(string key, string value)
        {
            _settings[key] = value;
            DatabaseContext.SetSetting(key, value);
        }

        public static void UpdateCurrency(string currencyCode, string currencySymbol)
        {
            Set("CurrencyCode", currencyCode);
            Set("CurrencySymbol", currencySymbol);
        }
    }
}