using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.Infrastructure.Services;

public class RegistryCleanerService : IRegistryCleanerService
{
    public async Task<RegistryScanResult> ScanRegistryAsync()
    {
        var result = new RegistryScanResult
        {
            ScanDate = DateTime.Now
        };

        var sw = Stopwatch.StartNew();

        await Task.Run(() =>
        {
            // Scan for various registry issues (safe scan only)
            result.Issues.AddRange(ScanInvalidFileExtensions());
            result.Issues.AddRange(ScanOrphanedStartupEntries());
            result.Issues.AddRange(ScanInvalidUninstallEntries());
            result.Issues.AddRange(ScanInvalidMUICache());
            result.Issues.AddRange(ScanEmptyRegistryKeys());

            result.TotalIssuesFound = result.Issues.Count;
        });

        sw.Stop();
        result.ScanDuration = sw.Elapsed;

        return result;
    }

    public async Task<RegistryCleanupResult> CleanRegistryAsync(List<RegistryIssue> issuesToFix)
    {
        var result = new RegistryCleanupResult();

        // Create automatic backup first
        var backupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinCheck",
            "Registry Backups",
            $"registry_backup_{DateTime.Now:yyyyMMdd_HHmmss}.reg"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);

        if (await CreateBackupAsync(backupPath))
        {
            result.BackupPath = backupPath;
        }

        await Task.Run(() =>
        {
            foreach (var issue in issuesToFix)
            {
                try
                {
                    if (FixRegistryIssue(issue))
                    {
                        result.IssuesFixed++;
                    }
                    else
                    {
                        result.IssuesFailed++;
                    }
                }
                catch (Exception ex)
                {
                    result.IssuesFailed++;
                    result.Errors.Add($"Error fixing {issue.KeyPath}: {ex.Message}");
                }
            }

            result.Success = result.IssuesFailed == 0;
        });

        return result;
    }

    public async Task<bool> CreateBackupAsync(string backupPath)
    {
        try
        {
            await Task.Run(() =>
            {
                // Export registry sections to .reg file
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "reg",
                    Arguments = $"export HKLM \"{backupPath}\" /y",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                process?.WaitForExit();
            });

            return File.Exists(backupPath);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RestoreBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath)) return false;

            await Task.Run(() =>
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "reg",
                    Arguments = $"import \"{backupPath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas" // Requires admin
                });

                process?.WaitForExit();
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    #region Private Scan Methods

    private List<RegistryIssue> ScanInvalidFileExtensions()
    {
        var issues = new List<RegistryIssue>();

        try
        {
            using var key = Registry.ClassesRoot;
            var subKeyNames = key.GetSubKeyNames().Where(n => n.StartsWith("."));

            foreach (var subKeyName in subKeyNames)
            {
                try
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    if (subKey != null)
                    {
                        var defaultValue = subKey.GetValue("")?.ToString();

                        // Check if the referenced ProgID exists
                        if (!string.IsNullOrEmpty(defaultValue))
                        {
                            using var progIdKey = Registry.ClassesRoot.OpenSubKey(defaultValue);
                            if (progIdKey == null)
                            {
                                issues.Add(new RegistryIssue
                                {
                                    KeyPath = $"HKEY_CLASSES_ROOT\\{subKeyName}",
                                    ValueName = "(Default)",
                                    Type = RegistryIssueType.InvalidFileExtension,
                                    Description = $"File extension {subKeyName} references non-existent ProgID: {defaultValue}",
                                    Severity = RegistryIssueSeverity.Low,
                                    IsFixable = true,
                                    BackupValue = defaultValue
                                });
                            }
                        }
                    }
                }
                catch { }
            }
        }
        catch { }

        return issues;
    }

    private List<RegistryIssue> ScanOrphanedStartupEntries()
    {
        var issues = new List<RegistryIssue>();

        var runKeys = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run"
        };

        foreach (var runKeyPath in runKeys)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(runKeyPath);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var value = key.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            // Extract file path from command
                            var filePath = ExtractFilePath(value);

                            if (!string.IsNullOrEmpty(filePath) && !File.Exists(filePath))
                            {
                                issues.Add(new RegistryIssue
                                {
                                    KeyPath = $"HKEY_LOCAL_MACHINE\\{runKeyPath}",
                                    ValueName = valueName,
                                    Type = RegistryIssueType.OrphanedStartupEntry,
                                    Description = $"Startup entry points to non-existent file: {filePath}",
                                    Severity = RegistryIssueSeverity.Medium,
                                    IsFixable = true,
                                    BackupValue = value
                                });
                            }
                        }
                    }
                }
            }
            catch { }

            // Check Current User as well
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(runKeyPath);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var value = key.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            var filePath = ExtractFilePath(value);

                            if (!string.IsNullOrEmpty(filePath) && !File.Exists(filePath))
                            {
                                issues.Add(new RegistryIssue
                                {
                                    KeyPath = $"HKEY_CURRENT_USER\\{runKeyPath}",
                                    ValueName = valueName,
                                    Type = RegistryIssueType.OrphanedStartupEntry,
                                    Description = $"Startup entry points to non-existent file: {filePath}",
                                    Severity = RegistryIssueSeverity.Medium,
                                    IsFixable = true,
                                    BackupValue = value
                                });
                            }
                        }
                    }
                }
            }
            catch { }
        }

        return issues;
    }

    private List<RegistryIssue> ScanInvalidUninstallEntries()
    {
        var issues = new List<RegistryIssue>();

        var uninstallKeys = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        foreach (var uninstallKeyPath in uninstallKeys)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(uninstallKeyPath);
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using var subKey = key.OpenSubKey(subKeyName);
                            if (subKey != null)
                            {
                                var uninstallString = subKey.GetValue("UninstallString")?.ToString();
                                var displayName = subKey.GetValue("DisplayName")?.ToString();

                                if (!string.IsNullOrEmpty(uninstallString) && !string.IsNullOrEmpty(displayName))
                                {
                                    var filePath = ExtractFilePath(uninstallString);

                                    if (!string.IsNullOrEmpty(filePath) && !File.Exists(filePath))
                                    {
                                        issues.Add(new RegistryIssue
                                        {
                                            KeyPath = $"HKEY_LOCAL_MACHINE\\{uninstallKeyPath}\\{subKeyName}",
                                            ValueName = "UninstallString",
                                            Type = RegistryIssueType.InvalidUninstallEntry,
                                            Description = $"Uninstall entry for '{displayName}' points to non-existent file",
                                            Severity = RegistryIssueSeverity.Low,
                                            IsFixable = true
                                        });
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        return issues;
    }

    private List<RegistryIssue> ScanInvalidMUICache()
    {
        var issues = new List<RegistryIssue>();

        try
        {
            var muiCachePath = @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache";

            using var key = Registry.CurrentUser.OpenSubKey(muiCachePath);
            if (key != null)
            {
                foreach (var valueName in key.GetValueNames())
                {
                    if (!File.Exists(valueName))
                    {
                        issues.Add(new RegistryIssue
                        {
                            KeyPath = $"HKEY_CURRENT_USER\\{muiCachePath}",
                            ValueName = valueName,
                            Type = RegistryIssueType.InvalidMUICache,
                            Description = $"MUI cache entry points to non-existent file",
                            Severity = RegistryIssueSeverity.Low,
                            IsFixable = true
                        });
                    }
                }
            }
        }
        catch { }

        return issues;
    }

    private List<RegistryIssue> ScanEmptyRegistryKeys()
    {
        var issues = new List<RegistryIssue>();

        // Only scan specific safe locations
        var safeLocations = new[]
        {
            (Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts"),
            (Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs")
        };

        foreach (var (hive, path) in safeLocations)
        {
            try
            {
                using var key = hive.OpenSubKey(path);
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using var subKey = key.OpenSubKey(subKeyName);
                            if (subKey != null && subKey.ValueCount == 0 && subKey.SubKeyCount == 0)
                            {
                                issues.Add(new RegistryIssue
                                {
                                    KeyPath = $"{hive.Name}\\{path}\\{subKeyName}",
                                    Type = RegistryIssueType.EmptyRegistryKey,
                                    Description = "Empty registry key with no values or sub keys",
                                    Severity = RegistryIssueSeverity.Low,
                                    IsFixable = true
                                });
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        return issues;
    }

    #endregion

    #region Private Helper Methods

    private bool FixRegistryIssue(RegistryIssue issue)
    {
        try
        {
            var parts = issue.KeyPath.Split('\\');
            if (parts.Length < 2) return false;

            RegistryKey? hive = parts[0] switch
            {
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                _ => null
            };

            if (hive == null) return false;

            var keyPath = string.Join("\\", parts.Skip(1));

            switch (issue.Type)
            {
                case RegistryIssueType.EmptyRegistryKey:
                    // Delete the empty key
                    using (var parentKey = hive.OpenSubKey(Path.GetDirectoryName(keyPath.Replace("\\", "/"))!.Replace("/", "\\"), true))
                    {
                        parentKey?.DeleteSubKey(Path.GetFileName(keyPath.Replace("\\", "/")), false);
                    }
                    return true;

                case RegistryIssueType.InvalidFileExtension:
                case RegistryIssueType.OrphanedStartupEntry:
                case RegistryIssueType.InvalidUninstallEntry:
                case RegistryIssueType.InvalidMUICache:
                    // Delete the invalid value
                    using (var key = hive.OpenSubKey(keyPath, true))
                    {
                        key?.DeleteValue(issue.ValueName, false);
                    }
                    return true;

                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private string ExtractFilePath(string command)
    {
        if (string.IsNullOrEmpty(command)) return string.Empty;

        // Remove quotes
        var path = command.Trim('"');

        // Handle commands with arguments
        if (path.Contains(".exe", StringComparison.OrdinalIgnoreCase))
        {
            var exeIndex = path.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            path = path.Substring(0, exeIndex + 4);
        }

        // Clean up the path
        path = path.Trim('"').Trim();

        // Expand environment variables
        path = Environment.ExpandEnvironmentVariables(path);

        return path;
    }

    #endregion
}
