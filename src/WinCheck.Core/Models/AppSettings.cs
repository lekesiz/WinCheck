using System;

namespace WinCheck.Core.Models;

public class AppSettings
{
    // AI Provider Settings
    public AIProviderType SelectedAIProvider { get; set; } = AIProviderType.OpenAI;
    public string OpenAIApiKey { get; set; } = string.Empty;
    public string ClaudeApiKey { get; set; } = string.Empty;
    public string GeminiApiKey { get; set; } = string.Empty;
    public string OpenAIModel { get; set; } = "gpt-4";
    public string ClaudeModel { get; set; } = "claude-3-sonnet-20240229";
    public string GeminiModel { get; set; } = "gemini-pro";

    // Application Settings
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool ShowNotifications { get; set; } = true;
    public bool AutoCheckUpdates { get; set; } = true;

    // Monitoring Settings
    public bool EnableProcessMonitoring { get; set; } = true;
    public bool EnableNetworkMonitoring { get; set; } = true;
    public int MonitoringIntervalSeconds { get; set; } = 5;
    public bool ShowSuspiciousProcessAlert { get; set; } = true;

    // Cleanup Settings
    public bool AutoCleanupOnStartup { get; set; } = false;
    public bool CreateBackupBeforeCleanup { get; set; } = true;
    public int CleanupScheduleDays { get; set; } = 7;

    // Performance Settings
    public bool EnableHardwareAcceleration { get; set; } = true;
    public int MaxConcurrentOperations { get; set; } = 4;

    // Privacy Settings
    public bool SendAnonymousUsageData { get; set; } = false;
    public bool LogDetailedErrors { get; set; } = true;

    // UI Settings
    public string Theme { get; set; } = "Dark";
    public string Language { get; set; } = "en-US";
    public bool ShowAdvancedOptions { get; set; } = false;

    // Last Updated
    public DateTime LastModified { get; set; } = DateTime.Now;
}

public enum AIProviderType
{
    OpenAI,
    Claude,
    Gemini
}
