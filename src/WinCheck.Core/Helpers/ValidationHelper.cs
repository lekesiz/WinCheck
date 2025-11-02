using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WinCheck.Core.Helpers;

/// <summary>
/// Helper class for input validation
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates if a drive letter is valid (A-Z)
    /// </summary>
    public static bool IsValidDriveLetter(string drive)
    {
        if (string.IsNullOrWhiteSpace(drive))
            return false;

        drive = drive.Trim().ToUpper();

        // Must be single letter A-Z
        if (drive.Length != 1 || !char.IsLetter(drive[0]))
            return false;

        return drive[0] >= 'A' && drive[0] <= 'Z';
    }

    /// <summary>
    /// Validates if a file path is safe and valid
    /// </summary>
    public static bool IsValidFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            // Check for invalid characters
            var invalidChars = Path.GetInvalidPathChars();
            if (filePath.IndexOfAny(invalidChars) >= 0)
                return false;

            // Check if path is rooted (absolute)
            if (!Path.IsPathRooted(filePath))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if a directory path is safe and valid
    /// </summary>
    public static bool IsValidDirectoryPath(string directoryPath)
    {
        return IsValidFilePath(directoryPath);
    }

    /// <summary>
    /// Validates if a process ID is valid (>0)
    /// </summary>
    public static bool IsValidProcessId(int processId)
    {
        return processId > 0;
    }

    /// <summary>
    /// Validates if an API key format is reasonable
    /// </summary>
    public static bool IsValidApiKeyFormat(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return false;

        // API keys should be at least 20 characters
        if (apiKey.Length < 20)
            return false;

        // Should only contain alphanumeric and common special chars
        if (!Regex.IsMatch(apiKey, @"^[a-zA-Z0-9\-_\.]+$"))
            return false;

        return true;
    }

    /// <summary>
    /// Validates if a service name is valid
    /// </summary>
    public static bool IsValidServiceName(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return false;

        // Service names should be reasonable length
        if (serviceName.Length > 256)
            return false;

        // Should not contain path separators or dangerous characters
        if (serviceName.Contains("\\") || serviceName.Contains("/"))
            return false;

        return true;
    }

    /// <summary>
    /// Validates if a registry key path is valid
    /// </summary>
    public static bool IsValidRegistryKeyPath(string keyPath)
    {
        if (string.IsNullOrWhiteSpace(keyPath))
            return false;

        // Must start with valid registry hive
        var validHives = new[]
        {
            "HKEY_LOCAL_MACHINE",
            "HKEY_CURRENT_USER",
            "HKEY_CLASSES_ROOT",
            "HKEY_USERS",
            "HKEY_CURRENT_CONFIG",
            "HKLM",
            "HKCU",
            "HKCR",
            "HKU"
        };

        bool startsWithValidHive = false;
        foreach (var hive in validHives)
        {
            if (keyPath.StartsWith(hive, StringComparison.OrdinalIgnoreCase))
            {
                startsWithValidHive = true;
                break;
            }
        }

        return startsWithValidHive;
    }

    /// <summary>
    /// Sanitizes a file name by removing invalid characters
    /// </summary>
    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "unnamed";

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = fileName;

        foreach (var c in invalidChars)
        {
            sanitized = sanitized.Replace(c, '_');
        }

        // Remove leading/trailing dots and spaces
        sanitized = sanitized.Trim('.', ' ');

        // Ensure not empty after sanitization
        if (string.IsNullOrWhiteSpace(sanitized))
            return "unnamed";

        // Limit length
        if (sanitized.Length > 200)
            sanitized = sanitized.Substring(0, 200);

        return sanitized;
    }

    /// <summary>
    /// Validates if a percentage value is valid (0-100)
    /// </summary>
    public static bool IsValidPercentage(double value)
    {
        return value >= 0 && value <= 100;
    }

    /// <summary>
    /// Validates if a file size is reasonable (not negative, not exceeding system limits)
    /// </summary>
    public static bool IsValidFileSize(long sizeBytes)
    {
        // Must be non-negative and less than 1 PB (reasonable upper limit)
        return sizeBytes >= 0 && sizeBytes < 1_000_000_000_000_000L;
    }

    /// <summary>
    /// Validates if a port number is valid (1-65535)
    /// </summary>
    public static bool IsValidPortNumber(int port)
    {
        return port >= 1 && port <= 65535;
    }

    /// <summary>
    /// Validates if an IP address format is valid
    /// </summary>
    public static bool IsValidIPAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        // IPv4 validation
        if (System.Net.IPAddress.TryParse(ipAddress, out var _))
            return true;

        return false;
    }
}
