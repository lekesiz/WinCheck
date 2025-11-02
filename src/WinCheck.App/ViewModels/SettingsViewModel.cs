using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // AI Provider Settings
    [ObservableProperty]
    private AIProviderType _selectedAIProvider = AIProviderType.OpenAI;

    public int SelectedProviderIndex
    {
        get => (int)SelectedAIProvider;
        set => SelectedAIProvider = (AIProviderType)value;
    }

    [ObservableProperty]
    private string _openAIApiKey = string.Empty;

    [ObservableProperty]
    private string _claudeApiKey = string.Empty;

    [ObservableProperty]
    private string _geminiApiKey = string.Empty;

    [ObservableProperty]
    private string _openAIModel = "gpt-4";

    [ObservableProperty]
    private string _claudeModel = "claude-3-sonnet-20240229";

    [ObservableProperty]
    private string _geminiModel = "gemini-pro";

    [ObservableProperty]
    private bool _isValidatingOpenAI;

    [ObservableProperty]
    private bool _isValidatingClaude;

    [ObservableProperty]
    private bool _isValidatingGemini;

    [ObservableProperty]
    private bool _openAIKeyValid;

    [ObservableProperty]
    private bool _claudeKeyValid;

    [ObservableProperty]
    private bool _geminiKeyValid;

    // Application Settings
    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    [ObservableProperty]
    private bool _showNotifications = true;

    [ObservableProperty]
    private bool _autoCheckUpdates = true;

    // Monitoring Settings
    [ObservableProperty]
    private bool _enableProcessMonitoring = true;

    [ObservableProperty]
    private bool _enableNetworkMonitoring = true;

    [ObservableProperty]
    private int _monitoringIntervalSeconds = 5;

    [ObservableProperty]
    private bool _showSuspiciousProcessAlert = true;

    // Cleanup Settings
    [ObservableProperty]
    private bool _autoCleanupOnStartup;

    [ObservableProperty]
    private bool _createBackupBeforeCleanup = true;

    [ObservableProperty]
    private int _cleanupScheduleDays = 7;

    // Performance Settings
    [ObservableProperty]
    private bool _enableHardwareAcceleration = true;

    [ObservableProperty]
    private int _maxConcurrentOperations = 4;

    // Privacy Settings
    [ObservableProperty]
    private bool _sendAnonymousUsageData;

    [ObservableProperty]
    private bool _logDetailedErrors = true;

    // UI Settings
    [ObservableProperty]
    private string _theme = "Dark";

    [ObservableProperty]
    private string _language = "en-US";

    [ObservableProperty]
    private bool _showAdvancedOptions;

    public List<string> AvailableThemes { get; } = new() { "Light", "Dark", "System" };
    public List<string> AvailableLanguages { get; } = new() { "en-US", "tr-TR", "de-DE", "fr-FR", "es-ES" };
    public List<AIProviderType> AvailableProviders { get; } = new()
    {
        AIProviderType.OpenAI,
        AIProviderType.Claude,
        AIProviderType.Gemini
    };

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading settings...";

            var settings = await _settingsService.LoadSettingsAsync();
            MapSettingsToViewModel(settings);

            StatusMessage = "Settings loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading settings: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            IsSaving = true;
            StatusMessage = "Saving settings...";

            var settings = MapViewModelToSettings();
            var success = await _settingsService.SaveSettingsAsync(settings);

            if (success)
            {
                StatusMessage = "Settings saved successfully";
            }
            else
            {
                StatusMessage = "Failed to save settings";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        try
        {
            StatusMessage = "Resetting settings to defaults...";

            var settings = await _settingsService.ResetSettingsAsync();
            MapSettingsToViewModel(settings);

            StatusMessage = "Settings reset to defaults";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error resetting settings: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ValidateOpenAIKeyAsync()
    {
        if (string.IsNullOrWhiteSpace(OpenAIApiKey))
        {
            OpenAIKeyValid = false;
            return;
        }

        try
        {
            IsValidatingOpenAI = true;
            OpenAIKeyValid = await _settingsService.ValidateApiKeyAsync(AIProviderType.OpenAI, OpenAIApiKey);
            StatusMessage = OpenAIKeyValid ? "OpenAI API key is valid" : "OpenAI API key is invalid";
        }
        catch
        {
            OpenAIKeyValid = false;
            StatusMessage = "Error validating OpenAI API key";
        }
        finally
        {
            IsValidatingOpenAI = false;
        }
    }

    [RelayCommand]
    private async Task ValidateClaudeKeyAsync()
    {
        if (string.IsNullOrWhiteSpace(ClaudeApiKey))
        {
            ClaudeKeyValid = false;
            return;
        }

        try
        {
            IsValidatingClaude = true;
            ClaudeKeyValid = await _settingsService.ValidateApiKeyAsync(AIProviderType.Claude, ClaudeApiKey);
            StatusMessage = ClaudeKeyValid ? "Claude API key is valid" : "Claude API key is invalid";
        }
        catch
        {
            ClaudeKeyValid = false;
            StatusMessage = "Error validating Claude API key";
        }
        finally
        {
            IsValidatingClaude = false;
        }
    }

    [RelayCommand]
    private async Task ValidateGeminiKeyAsync()
    {
        if (string.IsNullOrWhiteSpace(GeminiApiKey))
        {
            GeminiKeyValid = false;
            return;
        }

        try
        {
            IsValidatingGemini = true;
            GeminiKeyValid = await _settingsService.ValidateApiKeyAsync(AIProviderType.Gemini, GeminiApiKey);
            StatusMessage = GeminiKeyValid ? "Gemini API key is valid" : "Gemini API key is invalid";
        }
        catch
        {
            GeminiKeyValid = false;
            StatusMessage = "Error validating Gemini API key";
        }
        finally
        {
            IsValidatingGemini = false;
        }
    }

    private void MapSettingsToViewModel(AppSettings settings)
    {
        // AI Settings
        SelectedAIProvider = settings.SelectedAIProvider;
        OpenAIApiKey = settings.OpenAIApiKey;
        ClaudeApiKey = settings.ClaudeApiKey;
        GeminiApiKey = settings.GeminiApiKey;
        OpenAIModel = settings.OpenAIModel;
        ClaudeModel = settings.ClaudeModel;
        GeminiModel = settings.GeminiModel;

        // Application Settings
        StartWithWindows = settings.StartWithWindows;
        MinimizeToTray = settings.MinimizeToTray;
        ShowNotifications = settings.ShowNotifications;
        AutoCheckUpdates = settings.AutoCheckUpdates;

        // Monitoring Settings
        EnableProcessMonitoring = settings.EnableProcessMonitoring;
        EnableNetworkMonitoring = settings.EnableNetworkMonitoring;
        MonitoringIntervalSeconds = settings.MonitoringIntervalSeconds;
        ShowSuspiciousProcessAlert = settings.ShowSuspiciousProcessAlert;

        // Cleanup Settings
        AutoCleanupOnStartup = settings.AutoCleanupOnStartup;
        CreateBackupBeforeCleanup = settings.CreateBackupBeforeCleanup;
        CleanupScheduleDays = settings.CleanupScheduleDays;

        // Performance Settings
        EnableHardwareAcceleration = settings.EnableHardwareAcceleration;
        MaxConcurrentOperations = settings.MaxConcurrentOperations;

        // Privacy Settings
        SendAnonymousUsageData = settings.SendAnonymousUsageData;
        LogDetailedErrors = settings.LogDetailedErrors;

        // UI Settings
        Theme = settings.Theme;
        Language = settings.Language;
        ShowAdvancedOptions = settings.ShowAdvancedOptions;
    }

    private AppSettings MapViewModelToSettings()
    {
        return new AppSettings
        {
            // AI Settings
            SelectedAIProvider = SelectedAIProvider,
            OpenAIApiKey = OpenAIApiKey,
            ClaudeApiKey = ClaudeApiKey,
            GeminiApiKey = GeminiApiKey,
            OpenAIModel = OpenAIModel,
            ClaudeModel = ClaudeModel,
            GeminiModel = GeminiModel,

            // Application Settings
            StartWithWindows = StartWithWindows,
            MinimizeToTray = MinimizeToTray,
            ShowNotifications = ShowNotifications,
            AutoCheckUpdates = AutoCheckUpdates,

            // Monitoring Settings
            EnableProcessMonitoring = EnableProcessMonitoring,
            EnableNetworkMonitoring = EnableNetworkMonitoring,
            MonitoringIntervalSeconds = MonitoringIntervalSeconds,
            ShowSuspiciousProcessAlert = ShowSuspiciousProcessAlert,

            // Cleanup Settings
            AutoCleanupOnStartup = AutoCleanupOnStartup,
            CreateBackupBeforeCleanup = CreateBackupBeforeCleanup,
            CleanupScheduleDays = CleanupScheduleDays,

            // Performance Settings
            EnableHardwareAcceleration = EnableHardwareAcceleration,
            MaxConcurrentOperations = MaxConcurrentOperations,

            // Privacy Settings
            SendAnonymousUsageData = SendAnonymousUsageData,
            LogDetailedErrors = LogDetailedErrors,

            // UI Settings
            Theme = Theme,
            Language = Language,
            ShowAdvancedOptions = ShowAdvancedOptions
        };
    }
}
