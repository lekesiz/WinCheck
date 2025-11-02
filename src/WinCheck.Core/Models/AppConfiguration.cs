using System;

namespace WinCheck.Core.Models;

/// <summary>
/// Application configuration model
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// Application version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Logging configuration
    /// </summary>
    public LoggingConfiguration Logging { get; set; } = new();

    /// <summary>
    /// Performance configuration
    /// </summary>
    public PerformanceConfiguration Performance { get; set; } = new();

    /// <summary>
    /// AI provider configuration
    /// </summary>
    public AIConfiguration AI { get; set; } = new();

    /// <summary>
    /// Monitoring configuration
    /// </summary>
    public MonitoringConfiguration Monitoring { get; set; } = new();

    /// <summary>
    /// Cleanup configuration
    /// </summary>
    public CleanupConfiguration Cleanup { get; set; } = new();
}

/// <summary>
/// Logging configuration
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// Minimum log level (0=Debug, 1=Info, 2=Warning, 3=Error, 4=Critical)
    /// </summary>
    public int MinLogLevel { get; set; } = 1; // Information

    /// <summary>
    /// Log retention days
    /// </summary>
    public int RetentionDays { get; set; } = 7;

    /// <summary>
    /// Enable file logging
    /// </summary>
    public bool EnableFileLogging { get; set; } = true;

    /// <summary>
    /// Enable console logging
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// Maximum log file size in MB
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 10;
}

/// <summary>
/// Performance configuration
/// </summary>
public class PerformanceConfiguration
{
    /// <summary>
    /// Enable caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache expiration minutes
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Enable performance monitoring
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = true;

    /// <summary>
    /// Maximum parallel operations
    /// </summary>
    public int MaxParallelOperations { get; set; } = 4;

    /// <summary>
    /// Process monitor update interval (seconds)
    /// </summary>
    public int ProcessMonitorIntervalSeconds { get; set; } = 2;
}

/// <summary>
/// AI provider configuration
/// </summary>
public class AIConfiguration
{
    /// <summary>
    /// Default AI provider (OpenAI, Claude, Gemini)
    /// </summary>
    public string DefaultProvider { get; set; } = "OpenAI";

    /// <summary>
    /// Request timeout seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable retry on failure
    /// </summary>
    public bool EnableRetry { get; set; } = true;

    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Enable rate limiting
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Maximum tokens per request
    /// </summary>
    public int MaxTokens { get; set; } = 2000;
}

/// <summary>
/// Monitoring configuration
/// </summary>
public class MonitoringConfiguration
{
    /// <summary>
    /// Enable process monitoring
    /// </summary>
    public bool EnableProcessMonitoring { get; set; } = true;

    /// <summary>
    /// Enable network monitoring
    /// </summary>
    public bool EnableNetworkMonitoring { get; set; } = false;

    /// <summary>
    /// Enable suspicious process detection
    /// </summary>
    public bool EnableSuspiciousDetection { get; set; } = true;

    /// <summary>
    /// CPU threshold for suspicious process (percentage)
    /// </summary>
    public double SuspiciousCpuThreshold { get; set; } = 80.0;

    /// <summary>
    /// Memory threshold for suspicious process (percentage)
    /// </summary>
    public double SuspiciousMemoryThreshold { get; set; } = 50.0;
}

/// <summary>
/// Cleanup configuration
/// </summary>
public class CleanupConfiguration
{
    /// <summary>
    /// Enable automatic cleanup on startup
    /// </summary>
    public bool AutoCleanupOnStartup { get; set; } = false;

    /// <summary>
    /// Cleanup temporary files
    /// </summary>
    public bool CleanTempFiles { get; set; } = true;

    /// <summary>
    /// Cleanup browser caches
    /// </summary>
    public bool CleanBrowserCaches { get; set; } = true;

    /// <summary>
    /// Cleanup Windows Update cache
    /// </summary>
    public bool CleanWindowsUpdateCache { get; set; } = false; // Requires admin

    /// <summary>
    /// Empty recycle bin
    /// </summary>
    public bool EmptyRecycleBin { get; set; } = false;

    /// <summary>
    /// Minimum free disk space warning (GB)
    /// </summary>
    public int MinFreeDiskSpaceGB { get; set; } = 10;
}

/// <summary>
/// Default configuration values
/// </summary>
public static class DefaultConfiguration
{
    /// <summary>
    /// Gets default configuration
    /// </summary>
    public static AppConfiguration Get()
    {
        return new AppConfiguration
        {
            Version = "1.0.0",
            Logging = new LoggingConfiguration
            {
                MinLogLevel = 1, // Information
                RetentionDays = 7,
                EnableFileLogging = true,
                EnableConsoleLogging = true,
                MaxFileSizeMB = 10
            },
            Performance = new PerformanceConfiguration
            {
                EnableCaching = true,
                CacheExpirationMinutes = 5,
                EnablePerformanceMonitoring = true,
                MaxParallelOperations = 4,
                ProcessMonitorIntervalSeconds = 2
            },
            AI = new AIConfiguration
            {
                DefaultProvider = "OpenAI",
                TimeoutSeconds = 30,
                EnableRetry = true,
                MaxRetryAttempts = 3,
                EnableRateLimiting = true,
                MaxTokens = 2000
            },
            Monitoring = new MonitoringConfiguration
            {
                EnableProcessMonitoring = true,
                EnableNetworkMonitoring = false,
                EnableSuspiciousDetection = true,
                SuspiciousCpuThreshold = 80.0,
                SuspiciousMemoryThreshold = 50.0
            },
            Cleanup = new CleanupConfiguration
            {
                AutoCleanupOnStartup = false,
                CleanTempFiles = true,
                CleanBrowserCaches = true,
                CleanWindowsUpdateCache = false,
                EmptyRecycleBin = false,
                MinFreeDiskSpaceGB = 10
            }
        };
    }
}
