using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WinCheck.Core.Interfaces;

namespace WinCheck.Core.Services;

/// <summary>
/// Advanced error handling and crash reporting service
/// </summary>
public class ErrorHandlingService
{
    private readonly ILogger _logger;
    private readonly ConcurrentQueue<ErrorReport> _errorHistory;
    private readonly string _crashDumpPath;
    private const int MaxErrorHistory = 100;

    public ErrorHandlingService(ILogger logger)
    {
        _logger = logger;
        _errorHistory = new ConcurrentQueue<ErrorReport>();

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _crashDumpPath = Path.Combine(localAppData, "WinCheck", "CrashDumps");
        Directory.CreateDirectory(_crashDumpPath);

        // Register global exception handlers
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    /// <summary>
    /// Handles an exception with full context
    /// </summary>
    public void HandleException(Exception ex, string context, ErrorSeverity severity = ErrorSeverity.Error)
    {
        var report = new ErrorReport
        {
            Timestamp = DateTime.UtcNow,
            Exception = ex,
            Context = context,
            Severity = severity,
            StackTrace = ex.StackTrace ?? string.Empty,
            MachineName = Environment.MachineName,
            OSVersion = Environment.OSVersion.ToString(),
            ProcessId = Environment.ProcessId,
            ThreadId = Environment.CurrentManagedThreadId
        };

        // Add to history
        _errorHistory.Enqueue(report);
        while (_errorHistory.Count > MaxErrorHistory)
        {
            _errorHistory.TryDequeue(out _);
        }

        // Log the error
        _logger.Log($"[{severity}] {context}: {ex.Message}", "ErrorHandling");

        // Write crash dump for critical errors
        if (severity == ErrorSeverity.Critical)
        {
            WriteCrashDump(report);
        }
    }

    /// <summary>
    /// Gets recent error history
    /// </summary>
    public IEnumerable<ErrorReport> GetErrorHistory(int count = 50)
    {
        return _errorHistory.Reverse().Take(count);
    }

    /// <summary>
    /// Clears error history
    /// </summary>
    public void ClearHistory()
    {
        _errorHistory.Clear();
    }

    /// <summary>
    /// Writes detailed crash dump to disk
    /// </summary>
    private void WriteCrashDump(ErrorReport report)
    {
        try
        {
            var fileName = $"crash_{report.Timestamp:yyyyMMdd_HHmmss}_{report.ProcessId}.json";
            var filePath = Path.Combine(_crashDumpPath, fileName);

            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
            _logger.Log($"Crash dump written to: {filePath}", "ErrorHandling");

            // Keep only last 10 crash dumps
            CleanupOldCrashDumps();
        }
        catch (Exception ex)
        {
            _logger.Log($"Failed to write crash dump: {ex.Message}", "ErrorHandling");
        }
    }

    /// <summary>
    /// Removes old crash dumps
    /// </summary>
    private void CleanupOldCrashDumps()
    {
        try
        {
            var files = Directory.GetFiles(_crashDumpPath, "crash_*.json")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .Skip(10)
                .ToList();

            foreach (var file in files)
            {
                file.Delete();
            }
        }
        catch (Exception ex)
        {
            _logger.Log($"Failed to cleanup crash dumps: {ex.Message}", "ErrorHandling");
        }
    }

    /// <summary>
    /// Global unhandled exception handler
    /// </summary>
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            HandleException(ex, "Unhandled Exception", ErrorSeverity.Critical);
        }
    }

    /// <summary>
    /// Unobserved task exception handler
    /// </summary>
    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleException(e.Exception, "Unobserved Task Exception", ErrorSeverity.Error);
        e.SetObserved(); // Prevent process termination
    }

    /// <summary>
    /// Gets crash dump directory path
    /// </summary>
    public string GetCrashDumpPath() => _crashDumpPath;
}

/// <summary>
/// Error severity levels
/// </summary>
public enum ErrorSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Detailed error report
/// </summary>
public class ErrorReport
{
    public DateTime Timestamp { get; set; }
    public Exception? Exception { get; set; }
    public string Context { get; set; } = string.Empty;
    public ErrorSeverity Severity { get; set; }
    public string StackTrace { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
    public int ProcessId { get; set; }
    public int ThreadId { get; set; }

    public override string ToString()
    {
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Severity}] {Context}: {Exception?.Message}";
    }
}
