using System;
using System.IO;
using WinCheck.Core.Interfaces;

namespace WinCheck.Infrastructure.Logging;

/// <summary>
/// File-based logger implementation
/// </summary>
public class FileLogger : ILogger
{
    private readonly string _logFilePath;
    private readonly object _lock = new();
    private readonly LogLevel _minLogLevel;

    public FileLogger(LogLevel minLogLevel = LogLevel.Information)
    {
        _minLogLevel = minLogLevel;

        var logFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinCheck",
            "Logs"
        );

        Directory.CreateDirectory(logFolder);

        _logFilePath = Path.Combine(
            logFolder,
            $"wincheck_{DateTime.Now:yyyyMMdd}.log"
        );
    }

    public void LogDebug(string message)
    {
        Log(LogLevel.Debug, message);
    }

    public void LogInformation(string message)
    {
        Log(LogLevel.Information, message);
    }

    public void LogWarning(string message)
    {
        Log(LogLevel.Warning, message);
    }

    public void LogError(string message)
    {
        Log(LogLevel.Error, message);
    }

    public void LogError(Exception exception, string message)
    {
        Log(LogLevel.Error, $"{message}\nException: {exception.GetType().Name}\nMessage: {exception.Message}\nStackTrace: {exception.StackTrace}");
    }

    public void LogCritical(string message)
    {
        Log(LogLevel.Critical, message);
    }

    public void LogCritical(Exception exception, string message)
    {
        Log(LogLevel.Critical, $"{message}\nException: {exception.GetType().Name}\nMessage: {exception.Message}\nStackTrace: {exception.StackTrace}");
    }

    private void Log(LogLevel level, string message)
    {
        if (level < _minLogLevel)
            return;

        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] [{level,-11}] {message}";

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }

            // Also output to Debug
            System.Diagnostics.Debug.WriteLine(logEntry);
        }
        catch
        {
            // Fail silently to avoid breaking application
        }
    }

    /// <summary>
    /// Cleans up old log files (older than 7 days)
    /// </summary>
    public static void CleanOldLogs()
    {
        try
        {
            var logFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WinCheck",
                "Logs"
            );

            if (!Directory.Exists(logFolder))
                return;

            var cutoffDate = DateTime.Now.AddDays(-7);
            var files = Directory.GetFiles(logFolder, "wincheck_*.log");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(file);
                }
            }
        }
        catch
        {
            // Fail silently
        }
    }
}

public enum LogLevel
{
    Debug = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}
