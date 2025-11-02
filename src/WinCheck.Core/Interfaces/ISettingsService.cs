using System.Threading.Tasks;
using WinCheck.Core.Models;

namespace WinCheck.Core.Interfaces;

public interface ISettingsService
{
    /// <summary>
    /// Load settings from storage
    /// </summary>
    Task<AppSettings> LoadSettingsAsync();

    /// <summary>
    /// Save settings to storage
    /// </summary>
    Task<bool> SaveSettingsAsync(AppSettings settings);

    /// <summary>
    /// Reset settings to defaults
    /// </summary>
    Task<AppSettings> ResetSettingsAsync();

    /// <summary>
    /// Validate API key for selected provider
    /// </summary>
    Task<bool> ValidateApiKeyAsync(AIProviderType provider, string apiKey);

    /// <summary>
    /// Get current settings (cached)
    /// </summary>
    AppSettings GetCurrentSettings();
}
