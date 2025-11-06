namespace WinCheck.Core.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Application name
    /// </summary>
    public const string ApplicationName = "WinCheck";

    /// <summary>
    /// Application folder name in LocalAppData
    /// </summary>
    public const string AppDataFolderName = "WinCheck";

    /// <summary>
    /// Settings file name
    /// </summary>
    public const string SettingsFileName = "settings.json";

    /// <summary>
    /// Log file name pattern
    /// </summary>
    public const string LogFilePattern = "wincheck-{Date}.log";

    /// <summary>
    /// Crash dump folder name
    /// </summary>
    public const string CrashDumpFolderName = "CrashDumps";

    /// <summary>
    /// Backups folder name
    /// </summary>
    public const string BackupsFolderName = "Backups";

    /// <summary>
    /// Logs folder name
    /// </summary>
    public const string LogsFolderName = "Logs";

    /// <summary>
    /// Maximum number of crash dumps to retain
    /// </summary>
    public const int MaxCrashDumps = 10;

    /// <summary>
    /// Maximum number of errors in history
    /// </summary>
    public const int MaxErrorHistory = 100;

    /// <summary>
    /// Log retention days
    /// </summary>
    public const int LogRetentionDays = 7;
}

/// <summary>
/// AI Provider related constants
/// </summary>
public static class AIProviderConstants
{
    /// <summary>
    /// OpenAI API endpoint
    /// </summary>
    public const string OpenAIApiEndpoint = "https://api.openai.com/v1/chat/completions";

    /// <summary>
    /// Claude API endpoint
    /// </summary>
    public const string ClaudeApiEndpoint = "https://api.anthropic.com/v1/messages";

    /// <summary>
    /// Gemini API endpoint template
    /// </summary>
    public const string GeminiApiEndpointTemplate = "https://generativelanguage.googleapis.com/v1/models/{0}:generateContent?key={1}";

    /// <summary>
    /// Default OpenAI model
    /// </summary>
    public const string DefaultOpenAIModel = "gpt-4";

    /// <summary>
    /// Default Claude model
    /// </summary>
    public const string DefaultClaudeModel = "claude-3-sonnet-20240229";

    /// <summary>
    /// Default Gemini model
    /// </summary>
    public const string DefaultGeminiModel = "gemini-pro";

    /// <summary>
    /// Default system prompt for AI assistants
    /// </summary>
    public const string DefaultSystemPrompt = "You are a helpful Windows system optimization assistant.";

    /// <summary>
    /// Claude API version header
    /// </summary>
    public const string ClaudeApiVersion = "2023-06-01";

    /// <summary>
    /// API request timeout in seconds
    /// </summary>
    public const int ApiTimeoutSeconds = 60;

    /// <summary>
    /// Minimum API key length
    /// </summary>
    public const int MinApiKeyLength = 20;
}

/// <summary>
/// Registry related constants
/// </summary>
public static class RegistryConstants
{
    /// <summary>
    /// Current user run registry path
    /// </summary>
    public const string CurrentUserRunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Local machine run registry path
    /// </summary>
    public const string LocalMachineRunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Registry backup file extension
    /// </summary>
    public const string RegistryBackupExtension = ".reg";

    /// <summary>
    /// Registry backup file pattern
    /// </summary>
    public const string RegistryBackupPattern = "registry_{0:yyyyMMdd_HHmmss}.reg";

    /// <summary>
    /// Services backup file pattern
    /// </summary>
    public const string ServicesBackupPattern = "services_{0:yyyyMMdd_HHmmss}.reg";
}

/// <summary>
/// File path constants
/// </summary>
public static class FilePathConstants
{
    /// <summary>
    /// Windows temp folder environment variable
    /// </summary>
    public const string WindowsTempEnvVar = "TEMP";

    /// <summary>
    /// Windows system temp path
    /// </summary>
    public const string WindowsSystemTemp = @"C:\Windows\Temp";

    /// <summary>
    /// Windows update cache path
    /// </summary>
    public const string WindowsUpdateCache = @"C:\Windows\SoftwareDistribution";

    /// <summary>
    /// Recycle bin path pattern
    /// </summary>
    public const string RecycleBinPattern = @"{0}:\$Recycle.Bin";

    /// <summary>
    /// Startup folder relative path
    /// </summary>
    public const string StartupFolderPath = @"Microsoft\Windows\Start Menu\Programs\Startup";

    /// <summary>
    /// Thumbnail cache folder
    /// </summary>
    public const string ThumbnailCacheFolder = @"Microsoft\Windows\Explorer";
}

/// <summary>
/// Monitoring constants
/// </summary>
public static class MonitoringConstants
{
    /// <summary>
    /// Default monitoring interval in seconds
    /// </summary>
    public const int DefaultMonitoringIntervalSeconds = 5;

    /// <summary>
    /// Maximum number of processes to display
    /// </summary>
    public const int MaxProcessesToDisplay = 50;

    /// <summary>
    /// High CPU usage threshold percentage
    /// </summary>
    public const double HighCpuThreshold = 80.0;

    /// <summary>
    /// High memory usage threshold percentage
    /// </summary>
    public const double HighMemoryThreshold = 50.0;

    /// <summary>
    /// Process monitoring update interval in milliseconds
    /// </summary>
    public const int ProcessMonitorUpdateIntervalMs = 2000;
}

/// <summary>
/// UI Theme constants
/// </summary>
public static class ThemeConstants
{
    /// <summary>
    /// Light theme name
    /// </summary>
    public const string LightTheme = "Light";

    /// <summary>
    /// Dark theme name
    /// </summary>
    public const string DarkTheme = "Dark";

    /// <summary>
    /// System theme name (follows Windows theme)
    /// </summary>
    public const string SystemTheme = "System";

    /// <summary>
    /// Default theme
    /// </summary>
    public const string DefaultTheme = DarkTheme;
}

/// <summary>
/// Validation constants
/// </summary>
public static class ValidationConstants
{
    /// <summary>
    /// Minimum drive letter
    /// </summary>
    public const char MinDriveLetter = 'A';

    /// <summary>
    /// Maximum drive letter
    /// </summary>
    public const char MaxDriveLetter = 'Z';

    /// <summary>
    /// Minimum port number
    /// </summary>
    public const int MinPortNumber = 1;

    /// <summary>
    /// Maximum port number
    /// </summary>
    public const int MaxPortNumber = 65535;

    /// <summary>
    /// Minimum percentage value
    /// </summary>
    public const double MinPercentage = 0.0;

    /// <summary>
    /// Maximum percentage value
    /// </summary>
    public const double MaxPercentage = 100.0;
}

/// <summary>
/// Cache constants
/// </summary>
public static class CacheConstants
{
    /// <summary>
    /// Default cache expiration in minutes
    /// </summary>
    public const int DefaultCacheExpirationMinutes = 5;

    /// <summary>
    /// System info cache key
    /// </summary>
    public const string SystemInfoCacheKey = "SystemInfo";

    /// <summary>
    /// Hardware cache key
    /// </summary>
    public const string HardwareCacheKey = "Hardware";

    /// <summary>
    /// Service list cache key
    /// </summary>
    public const string ServiceListCacheKey = "ServiceList";

    /// <summary>
    /// Process list cache key
    /// </summary>
    public const string ProcessListCacheKey = "ProcessList";
}
