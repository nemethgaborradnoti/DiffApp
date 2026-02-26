using DiffApp.Models;
using DiffApp.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace DiffApp.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;

        public SettingsService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiffApp");
            Directory.CreateDirectory(appDataPath);
            _settingsPath = Path.Combine(appDataPath, "settings.json");
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch
            {
                // Fallback to defaults on error
            }

            return new AppSettings();
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch
            {
                // Handle or log save error if needed
            }
        }

        public void ResetToDefaults()
        {
            var currentSettings = LoadSettings();
            var defaults = new AppSettings();

            defaults.WindowTop = currentSettings.WindowTop;
            defaults.WindowLeft = currentSettings.WindowLeft;
            defaults.WindowWidth = currentSettings.WindowWidth;
            defaults.WindowHeight = currentSettings.WindowHeight;
            defaults.WindowState = currentSettings.WindowState;
            defaults.IsSettingsPanelOpen = currentSettings.IsSettingsPanelOpen;

            SaveSettings(defaults);
        }
    }
}