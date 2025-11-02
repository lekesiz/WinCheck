using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Microsoft.Win32;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.Infrastructure.Services;

public class StartupManagerService : IStartupManagerService
{
    public async Task<List<StartupProgram>> GetStartupProgramsAsync()
    {
        var programs = new List<StartupProgram>();

        await Task.Run(() =>
        {
            try
            {
                // ONLY scan registry Run keys - most stable
                var runPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

                // Current User
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(runPath);
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            try
                            {
                                var command = key.GetValue(valueName)?.ToString();
                                if (!string.IsNullOrEmpty(command))
                                {
                                    programs.Add(new StartupProgram
                                    {
                                        Name = valueName,
                                        Command = command,
                                        Location = StartupLocation.RegistryRun,
                                        IsEnabled = true,
                                        Publisher = "Unknown",
                                        IsSigned = false,
                                        Impact = StartupImpact.Medium,
                                        EstimatedDelayMs = 1000
                                    });
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                // Local Machine
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(runPath);
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            try
                            {
                                var command = key.GetValue(valueName)?.ToString();
                                if (!string.IsNullOrEmpty(command))
                                {
                                    programs.Add(new StartupProgram
                                    {
                                        Name = valueName,
                                        Command = command,
                                        Location = StartupLocation.RegistryRun,
                                        IsEnabled = true,
                                        Publisher = "Unknown",
                                        IsSigned = false,
                                        Impact = StartupImpact.Medium,
                                        EstimatedDelayMs = 1000
                                    });
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
            catch { }
        });

        return programs;
    }

    public async Task<bool> SetStartupStateAsync(StartupProgram program, bool enabled)
    {
        try
        {
            await Task.Run(() =>
            {
                switch (program.Location)
                {
                    case StartupLocation.RegistryRun:
                    case StartupLocation.RegistryRunOnce:
                    case StartupLocation.RegistryRun64:
                        SetRegistryStartupState(program, enabled);
                        break;

                    case StartupLocation.StartupFolder:
                        SetStartupFolderState(program, enabled);
                        break;

                    case StartupLocation.TaskScheduler:
                        SetTaskSchedulerState(program, enabled);
                        break;
                }
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AddToStartupAsync(string name, string path, StartupLocation location)
    {
        try
        {
            await Task.Run(() =>
            {
                switch (location)
                {
                    case StartupLocation.RegistryRun:
                        using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                        {
                            key?.SetValue(name, path);
                        }
                        break;

                    case StartupLocation.StartupFolder:
                        var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                        var shortcutPath = Path.Combine(startupPath, $"{name}.lnk");
                        CreateShortcut(shortcutPath, path);
                        break;
                }
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveFromStartupAsync(StartupProgram program)
    {
        try
        {
            await Task.Run(() =>
            {
                switch (program.Location)
                {
                    case StartupLocation.RegistryRun:
                    case StartupLocation.RegistryRunOnce:
                    case StartupLocation.RegistryRun64:
                        DeleteRegistryStartup(program);
                        break;

                    case StartupLocation.StartupFolder:
                        DeleteStartupFolderEntry(program);
                        break;

                    case StartupLocation.TaskScheduler:
                        DeleteTaskSchedulerEntry(program);
                        break;
                }
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<StartupImpactAnalysis> AnalyzeBootImpactAsync()
    {
        try
        {
            var programs = await GetStartupProgramsAsync();

            var analysis = new StartupImpactAnalysis
            {
                TotalStartupPrograms = 0,
                EnabledPrograms = 0,
                DisabledPrograms = 0,
                EstimatedBootTimeSeconds = 0,
                PotentialTimeSavingSeconds = 0
            };

            try
            {
                analysis.TotalStartupPrograms = programs?.Count ?? 0;
                analysis.EnabledPrograms = programs?.Count(p => p != null && p.IsEnabled) ?? 0;
                analysis.DisabledPrograms = programs?.Count(p => p != null && !p.IsEnabled) ?? 0;
            }
            catch { }

            try
            {
                // Estimate boot time based on enabled programs
                var enabledPrograms = programs?.Where(p => p != null && p.IsEnabled).ToList() ?? new List<StartupProgram>();
                analysis.EstimatedBootTimeSeconds = enabledPrograms.Sum(p => p.EstimatedDelayMs) / 1000;
            }
            catch { }

            try
            {
                // Calculate potential savings
                var enabledPrograms = programs?.Where(p => p != null && p.IsEnabled).ToList() ?? new List<StartupProgram>();
                var highImpactPrograms = enabledPrograms.Where(p => p.Impact >= StartupImpact.High).ToList();
                analysis.PotentialTimeSavingSeconds = highImpactPrograms.Sum(p => p.EstimatedDelayMs) / 1000;
            }
            catch { }

            try
            {
                // Generate recommendations
                var enabledPrograms = programs?.Where(p => p != null && p.IsEnabled).ToList() ?? new List<StartupProgram>();
                foreach (var program in enabledPrograms)
                {
                    try
                    {
                        if (ShouldRecommendDisabling(program))
                        {
                            analysis.Recommendations.Add(new StartupRecommendation
                            {
                                Program = program,
                                Reason = GetDisableReason(program),
                                RecommendDisable = true,
                                EstimatedTimeSavingMs = program.EstimatedDelayMs
                            });
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return analysis;
        }
        catch
        {
            // Return empty safe analysis on any error
            return new StartupImpactAnalysis
            {
                TotalStartupPrograms = 0,
                EnabledPrograms = 0,
                DisabledPrograms = 0,
                EstimatedBootTimeSeconds = 0,
                PotentialTimeSavingSeconds = 0
            };
        }
    }

    #region Private Methods - Registry

    private List<StartupProgram> GetRegistryStartupPrograms()
    {
        var programs = new List<StartupProgram>();

        var runKeys = new[]
        {
            (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", StartupLocation.RegistryRun),
            (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", StartupLocation.RegistryRunOnce),
            (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", StartupLocation.RegistryRun64),
            (Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", StartupLocation.RegistryRun),
            (Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", StartupLocation.RegistryRunOnce)
        };

        foreach (var (hive, path, location) in runKeys)
        {
            try
            {
                using var key = hive.OpenSubKey(path);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var command = key.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrEmpty(command))
                        {
                            var program = new StartupProgram
                            {
                                Name = valueName,
                                Command = command,
                                Location = location,
                                IsEnabled = true
                            };

                            AnalyzeProgram(program);
                            programs.Add(program);
                        }
                    }
                }
            }
            catch { }
        }

        return programs;
    }

    private void SetRegistryStartupState(StartupProgram program, bool enabled)
    {
        // Windows doesn't have a native "disable" for registry entries
        // We move them to a backup location if disabling
        var runPath = program.Location == StartupLocation.RegistryRun
            ? @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
            : @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce";

        var backupPath = @"SOFTWARE\WinCheck\DisabledStartup";

        if (enabled)
        {
            // Move from backup to Run
            try
            {
                using var backupKey = Registry.CurrentUser.OpenSubKey(backupPath);
                if (backupKey != null)
                {
                    var value = backupKey.GetValue(program.Name)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        using var runKey = Registry.CurrentUser.OpenSubKey(runPath, true);
                        runKey?.SetValue(program.Name, value);

                        using var backupKeyWrite = Registry.CurrentUser.OpenSubKey(backupPath, true);
                        backupKeyWrite?.DeleteValue(program.Name, false);
                    }
                }
            }
            catch { }
        }
        else
        {
            // Move from Run to backup
            try
            {
                using var runKey = Registry.CurrentUser.OpenSubKey(runPath);
                if (runKey != null)
                {
                    var value = runKey.GetValue(program.Name)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        var backupKey = Registry.CurrentUser.CreateSubKey(backupPath, true);
                        backupKey?.SetValue(program.Name, value);

                        using var runKeyWrite = Registry.CurrentUser.OpenSubKey(runPath, true);
                        runKeyWrite?.DeleteValue(program.Name, false);
                    }
                }
            }
            catch { }
        }
    }

    private void DeleteRegistryStartup(StartupProgram program)
    {
        var paths = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run"
        };

        foreach (var path in paths)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(path, true);
                key?.DeleteValue(program.Name, false);

                using var currentUserKey = Registry.CurrentUser.OpenSubKey(path, true);
                currentUserKey?.DeleteValue(program.Name, false);
            }
            catch { }
        }
    }

    #endregion

    #region Private Methods - Startup Folder

    private List<StartupProgram> GetStartupFolderPrograms()
    {
        var programs = new List<StartupProgram>();

        var startupFolders = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup)
        };

        foreach (var folder in startupFolders)
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    var files = Directory.GetFiles(folder);
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        var program = new StartupProgram
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            Command = file,
                            Location = StartupLocation.StartupFolder,
                            IsEnabled = !file.EndsWith(".disabled"),
                            LastModified = fileInfo.LastWriteTime
                        };

                        AnalyzeProgram(program);
                        programs.Add(program);
                    }
                }
            }
            catch { }
        }

        return programs;
    }

    private void SetStartupFolderState(StartupProgram program, bool enabled)
    {
        try
        {
            if (File.Exists(program.Command))
            {
                if (enabled && program.Command.EndsWith(".disabled"))
                {
                    // Rename to remove .disabled
                    var newPath = program.Command.Replace(".disabled", "");
                    File.Move(program.Command, newPath);
                }
                else if (!enabled && !program.Command.EndsWith(".disabled"))
                {
                    // Rename to add .disabled
                    File.Move(program.Command, program.Command + ".disabled");
                }
            }
        }
        catch { }
    }

    private void DeleteStartupFolderEntry(StartupProgram program)
    {
        try
        {
            if (File.Exists(program.Command))
            {
                File.Delete(program.Command);
            }
        }
        catch { }
    }

    #endregion

    #region Private Methods - Task Scheduler

    private List<StartupProgram> GetTaskSchedulerPrograms()
    {
        var programs = new List<StartupProgram>();

        try
        {
            // Use schtasks command instead of WMI for better stability
            var startInfo = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = "/Query /FO CSV /V",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split('\n');
                foreach (var line in lines.Skip(1)) // Skip header
                {
                    try
                    {
                        var parts = line.Split(',');
                        if (parts.Length > 1)
                        {
                            var taskName = parts[0].Trim('"');
                            var status = parts.Length > 3 ? parts[3].Trim('"') : "";

                            // Basic filter for startup tasks
                            if (!string.IsNullOrEmpty(taskName) && taskName.Contains("Startup", StringComparison.OrdinalIgnoreCase))
                            {
                                var program = new StartupProgram
                                {
                                    Name = taskName,
                                    Location = StartupLocation.TaskScheduler,
                                    IsEnabled = status.Contains("Ready", StringComparison.OrdinalIgnoreCase),
                                    Command = "" // Will be populated if needed
                                };

                                programs.Add(program);
                            }
                        }
                    }
                    catch { }
                }
            }
        }
        catch { }

        return programs;
    }

    private void SetTaskSchedulerState(StartupProgram program, bool enabled)
    {
        try
        {
            var action = enabled ? "/Enable" : "/Disable";
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = $"/Change /TN \"{program.Name}\" {action}",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            process?.WaitForExit();
        }
        catch { }
    }

    private void DeleteTaskSchedulerEntry(StartupProgram program)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = $"/Delete /TN \"{program.Name}\" /F",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            process?.WaitForExit();
        }
        catch { }
    }

    #endregion

    #region Private Helper Methods

    private void AnalyzeProgram(StartupProgram program)
    {
        try
        {
            // Extract file path from command
            var filePath = ExtractFilePath(program.Command);

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
                    program.Publisher = versionInfo.CompanyName ?? "Unknown";
                }
                catch
                {
                    program.Publisher = "Unknown";
                }

                // Skip signature checking - it's unreliable and can crash
                program.IsSigned = false;
            }
            else
            {
                program.Publisher = "Unknown";
                program.IsSigned = false;
            }

            // Estimate impact and delay
            program.Impact = EstimateImpact(program);
            program.EstimatedDelayMs = EstimateDelay(program.Impact);
        }
        catch
        {
            // Set safe defaults if anything fails
            program.Publisher = "Unknown";
            program.IsSigned = false;
            program.Impact = StartupImpact.Medium;
            program.EstimatedDelayMs = 1000;
        }
    }

    private string ExtractFilePath(string command)
    {
        if (string.IsNullOrEmpty(command)) return string.Empty;

        var path = command.Trim('"');

        if (path.Contains(".exe", StringComparison.OrdinalIgnoreCase))
        {
            var exeIndex = path.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            path = path.Substring(0, exeIndex + 4);
        }

        path = Environment.ExpandEnvironmentVariables(path.Trim('"').Trim());

        return path;
    }

    private StartupImpact EstimateImpact(StartupProgram program)
    {
        // Known high-impact programs
        var highImpactKeywords = new[] { "adobe", "creative cloud", "steam", "discord", "skype", "dropbox", "onedrive", "google drive" };
        var mediumImpactKeywords = new[] { "antivirus", "security", "defender", "update" };
        var lowImpactKeywords = new[] { "audio", "intel", "nvidia", "amd", "driver" };

        var nameLower = program.Name.ToLower();
        var commandLower = program.Command.ToLower();

        if (highImpactKeywords.Any(k => nameLower.Contains(k) || commandLower.Contains(k)))
            return StartupImpact.High;

        if (mediumImpactKeywords.Any(k => nameLower.Contains(k) || commandLower.Contains(k)))
            return StartupImpact.Medium;

        if (lowImpactKeywords.Any(k => nameLower.Contains(k) || commandLower.Contains(k)))
            return StartupImpact.Low;

        return StartupImpact.Medium;
    }

    private int EstimateDelay(StartupImpact impact)
    {
        return impact switch
        {
            StartupImpact.VeryHigh => 5000,
            StartupImpact.High => 3000,
            StartupImpact.Medium => 1000,
            StartupImpact.Low => 500,
            _ => 200
        };
    }

    private bool ShouldRecommendDisabling(StartupProgram program)
    {
        // Recommend disabling high-impact non-essential programs
        if (program.Impact >= StartupImpact.High && !program.IsSigned)
            return true;

        // Recommend disabling known bloatware
        var bloatwareKeywords = new[] { "adobe creative cloud", "steam", "discord", "skype" };
        var nameLower = program.Name.ToLower();

        return bloatwareKeywords.Any(k => nameLower.Contains(k));
    }

    private string GetDisableReason(StartupProgram program)
    {
        if (program.Impact >= StartupImpact.High)
            return $"High boot time impact ({program.EstimatedDelayMs}ms). Consider disabling if not needed.";

        if (!program.IsSigned)
            return "Unsigned program. May be unnecessary or potentially risky.";

        return "Disabling this program may improve boot time.";
    }

    private void CreateShortcut(string shortcutPath, string targetPath)
    {
        // Creating shortcuts requires COM automation (WScript.Shell)
        // Simplified implementation - in production would use IWshRuntimeLibrary
        try
        {
            var script = $@"
                Set WshShell = WScript.CreateObject(""WScript.Shell"")
                Set Shortcut = WshShell.CreateShortcut(""{shortcutPath}"")
                Shortcut.TargetPath = ""{targetPath}""
                Shortcut.Save
            ";

            var scriptPath = Path.GetTempFileName() + ".vbs";
            File.WriteAllText(scriptPath, script);

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "wscript",
                Arguments = $"\"{scriptPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            process?.WaitForExit();
            File.Delete(scriptPath);
        }
        catch { }
    }

    #endregion
}
