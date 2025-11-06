using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;
using WinCheck.Core.Services.AI;
using WinCheck.Core.Helpers;
using WinCheck.Core.Constants;

namespace WinCheck.Infrastructure.Services;

/// <summary>
/// Service for managing application settings with encrypted API key storage
/// </summary>
/// <remarks>
/// API keys are automatically encrypted using Windows DPAPI when saved.
/// Encryption scope: CurrentUser only.
/// Backward compatible with plaintext API keys from previous versions.
/// </remarks>
public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private AppSettings? _cachedSettings;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger? _logger;

    public SettingsService(ILogger? logger = null)
    {
        _logger = logger;

        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppConstants.AppDataFolderName
        );

        Directory.CreateDirectory(appDataFolder);
        _settingsFilePath = Path.Combine(appDataFolder, AppConstants.SettingsFileName);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                _cachedSettings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);

                if (_cachedSettings != null)
                {
                    // Decrypt API keys if they are encrypted
                    _cachedSettings.OpenAIApiKey = DecryptApiKeyIfNeeded(_cachedSettings.OpenAIApiKey);
                    _cachedSettings.ClaudeApiKey = DecryptApiKeyIfNeeded(_cachedSettings.ClaudeApiKey);
                    _cachedSettings.GeminiApiKey = DecryptApiKeyIfNeeded(_cachedSettings.GeminiApiKey);

                    return _cachedSettings;
                }
            }
        }
        catch (Exception ex)
        {
            // If loading fails, return defaults
            _logger?.Log($"Failed to load settings: {ex.Message}", "SettingsService");
        }

        // Return default settings
        _cachedSettings = new AppSettings();
        return _cachedSettings;
    }

    public async Task<bool> SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            settings.LastModified = DateTime.Now;

            // Create a copy to avoid modifying the original
            var settingsToSave = new AppSettings
            {
                SelectedAIProvider = settings.SelectedAIProvider,
                OpenAIApiKey = EncryptionHelper.Encrypt(settings.OpenAIApiKey),
                ClaudeApiKey = EncryptionHelper.Encrypt(settings.ClaudeApiKey),
                GeminiApiKey = EncryptionHelper.Encrypt(settings.GeminiApiKey),
                OpenAIModel = settings.OpenAIModel,
                ClaudeModel = settings.ClaudeModel,
                GeminiModel = settings.GeminiModel,
                StartWithWindows = settings.StartWithWindows,
                MinimizeToTray = settings.MinimizeToTray,
                ShowNotifications = settings.ShowNotifications,
                AutoCheckUpdates = settings.AutoCheckUpdates,
                EnableProcessMonitoring = settings.EnableProcessMonitoring,
                EnableNetworkMonitoring = settings.EnableNetworkMonitoring,
                MonitoringIntervalSeconds = settings.MonitoringIntervalSeconds,
                ShowSuspiciousProcessAlert = settings.ShowSuspiciousProcessAlert,
                AutoCleanupOnStartup = settings.AutoCleanupOnStartup,
                CreateBackupBeforeCleanup = settings.CreateBackupBeforeCleanup,
                CleanupScheduleDays = settings.CleanupScheduleDays,
                EnableHardwareAcceleration = settings.EnableHardwareAcceleration,
                MaxConcurrentOperations = settings.MaxConcurrentOperations,
                SendAnonymousUsageData = settings.SendAnonymousUsageData,
                LogDetailedErrors = settings.LogDetailedErrors,
                Theme = settings.Theme,
                Language = settings.Language,
                ShowAdvancedOptions = settings.ShowAdvancedOptions,
                LastModified = settings.LastModified
            };

            var json = JsonSerializer.Serialize(settingsToSave, _jsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);
            _cachedSettings = settings; // Cache the unencrypted version
            return true;
        }
        catch (Exception ex)
        {
            _logger?.Log($"Failed to save settings: {ex.Message}", "SettingsService");
            return false;
        }
    }

    public async Task<AppSettings> ResetSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                File.Delete(_settingsFilePath);
            }
        }
        catch (Exception ex)
        {
            // Ignore deletion errors but log them
            _logger?.Log($"Failed to delete settings file: {ex.Message}", "SettingsService");
        }

        _cachedSettings = new AppSettings();
        await SaveSettingsAsync(_cachedSettings);
        return _cachedSettings;
    }

    public async Task<bool> ValidateApiKeyAsync(AIProviderType provider, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        try
        {
            IAIProvider aiProvider = provider switch
            {
                AIProviderType.OpenAI => new AI.OpenAIProvider(apiKey),
                AIProviderType.Claude => new AI.ClaudeProvider(apiKey),
                AIProviderType.Gemini => new AI.GeminiProvider(apiKey),
                _ => throw new ArgumentException($"Unknown provider: {provider}")
            };

            return await aiProvider.TestConnectionAsync();
        }
        catch (Exception ex)
        {
            _logger?.Log($"API key validation failed for {provider}: {ex.Message}", "SettingsService");
            return false;
        }
    }

    public AppSettings GetCurrentSettings()
    {
        return _cachedSettings ?? new AppSettings();
    }

    /// <summary>
    /// Decrypts API key if it's encrypted, otherwise returns as-is
    /// This allows backward compatibility with old plaintext settings
    /// </summary>
    private string DecryptApiKeyIfNeeded(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return string.Empty;
        }

        // Check if it looks encrypted (Base64 format, not starting with known prefixes)
        if (EncryptionHelper.IsEncrypted(apiKey))
        {
            var decrypted = EncryptionHelper.Decrypt(apiKey);
            return decrypted;
        }

        // Return as-is (plaintext, for backward compatibility)
        return apiKey;
    }
}
