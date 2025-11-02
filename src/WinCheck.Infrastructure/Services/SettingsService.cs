using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;
using WinCheck.Core.Services.AI;

namespace WinCheck.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private AppSettings? _cachedSettings;
    private readonly JsonSerializerOptions _jsonOptions;

    public SettingsService()
    {
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinCheck"
        );

        Directory.CreateDirectory(appDataFolder);
        _settingsFilePath = Path.Combine(appDataFolder, "settings.json");

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
                    return _cachedSettings;
                }
            }
        }
        catch (Exception)
        {
            // If loading fails, return defaults
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
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);
            _cachedSettings = settings;
            return true;
        }
        catch (Exception)
        {
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
        catch
        {
            // Ignore deletion errors
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
        catch
        {
            return false;
        }
    }

    public AppSettings GetCurrentSettings()
    {
        return _cachedSettings ?? new AppSettings();
    }
}
