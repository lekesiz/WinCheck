using System;

namespace WinCheck.Core.Interfaces;

/// <summary>
/// Logging interface for WinCheck application
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs debug information
    /// </summary>
    void LogDebug(string message);

    /// <summary>
    /// Logs informational message
    /// </summary>
    void LogInformation(string message);

    /// <summary>
    /// Logs warning message
    /// </summary>
    void LogWarning(string message);

    /// <summary>
    /// Logs error message
    /// </summary>
    void LogError(string message);

    /// <summary>
    /// Logs error with exception details
    /// </summary>
    void LogError(Exception exception, string message);

    /// <summary>
    /// Logs critical error
    /// </summary>
    void LogCritical(string message);

    /// <summary>
    /// Logs critical error with exception
    /// </summary>
    void LogCritical(Exception exception, string message);
}
