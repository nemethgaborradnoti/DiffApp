using DiffApp.Models;

namespace DiffApp.Services.Interfaces
{
    public interface ISettingsService
    {
        AppSettings LoadSettings();
        void SaveSettings(AppSettings settings);
        void ResetToDefaults();
    }
}