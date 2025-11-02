using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.Infrastructure.Services;

public class OSDetectionService : IOSDetectionService
{
    public async Task<OSInfo> DetectOSAsync()
    {
        var osInfo = new OSInfo();

        await Task.Run(() =>
        {
            // Get OS version using RtlGetVersion (more accurate than Environment.OSVersion)
            var versionInfo = new OSVERSIONINFOEX();
            RtlGetVersion(ref versionInfo);

            osInfo.BuildNumber = (int)versionInfo.dwBuildNumber;
            osInfo.ServicePack = versionInfo.wServicePackMajor;

            // Determine Windows version
            osInfo.WindowsVersion = DetermineWindowsVersion(versionInfo);
            osInfo.Name = GetWindowsName(osInfo.WindowsVersion, osInfo.BuildNumber);
            osInfo.Version = GetWindowsVersionString(osInfo.WindowsVersion, osInfo.BuildNumber);

            // Get detailed info from WMI
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    osInfo.Edition = obj["Caption"]?.ToString()?.Replace("Microsoft Windows ", "") ?? "Unknown";
                    osInfo.Architecture = obj["OSArchitecture"]?.ToString() ?? "Unknown";
                    osInfo.SystemRoot = obj["SystemDirectory"]?.ToString()?.Replace("\\System32", "") ?? "C:\\Windows";
                    osInfo.RegisteredOwner = obj["RegisteredUser"]?.ToString() ?? "Unknown";

                    // Install date
                    var installDateStr = obj["InstallDate"]?.ToString();
                    if (!string.IsNullOrEmpty(installDateStr) && installDateStr.Length >= 14)
                    {
                        osInfo.InstallDate = ManagementDateTimeConverter.ToDateTime(installDateStr);
                    }

                    // Uptime
                    var lastBootStr = obj["LastBootUpTime"]?.ToString();
                    if (!string.IsNullOrEmpty(lastBootStr) && lastBootStr.Length >= 14)
                    {
                        var lastBoot = ManagementDateTimeConverter.ToDateTime(lastBootStr);
                        osInfo.UptimeSeconds = (long)(DateTime.Now - lastBoot).TotalSeconds;
                    }

                    break;
                }
            }

            // Get computer name
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    osInfo.ComputerName = obj["Name"]?.ToString() ?? "Unknown";
                    break;
                }
            }

            // Check activation status
            osInfo.IsActivated = CheckActivationStatus();

            // Check if Insider build
            osInfo.IsInsiderBuild = CheckInsiderBuild();

            // Get product key (partial, for security)
            osInfo.ProductKey = GetPartialProductKey();
        });

        return osInfo;
    }

    public async Task<List<OSOptimization>> GetRecommendedOptimizationsAsync()
    {
        var osInfo = await DetectOSAsync();
        var optimizations = new List<OSOptimization>();

        // Add optimizations based on Windows version
        optimizations.AddRange(GetPerformanceOptimizations(osInfo.WindowsVersion));
        optimizations.AddRange(GetPrivacyOptimizations(osInfo.WindowsVersion));
        optimizations.AddRange(GetSecurityOptimizations(osInfo.WindowsVersion));
        optimizations.AddRange(GetUIOptimizations(osInfo.WindowsVersion));

        return optimizations;
    }

    public async Task<bool> ApplyOptimizationAsync(OSOptimization optimization)
    {
        try
        {
            foreach (var step in optimization.Steps)
            {
                switch (step.Type)
                {
                    case OSStepType.SetRegistryValue:
                        SetRegistryValue(step.Target, step.Value);
                        break;

                    case OSStepType.DeleteRegistryValue:
                        DeleteRegistryValue(step.Target);
                        break;

                    case OSStepType.StopService:
                        await StopServiceAsync(step.Target);
                        break;

                    case OSStepType.DisableService:
                        await DisableServiceAsync(step.Target);
                        break;

                    case OSStepType.EnableService:
                        await EnableServiceAsync(step.Target);
                        break;

                    case OSStepType.RunCommand:
                        await RunCommandAsync(step.Target);
                        break;

                    case OSStepType.DeleteFile:
                        DeleteFile(step.Target);
                        break;

                    case OSStepType.SetGroupPolicy:
                        SetGroupPolicy(step.Target, step.Value);
                        break;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UpdateInfo> CheckForUpdatesAsync()
    {
        var updateInfo = new UpdateInfo
        {
            LastUpdateCheck = DateTime.Now
        };

        await Task.Run(() =>
        {
            try
            {
                // Check if Windows Update is enabled
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU");
                var noAutoUpdate = key?.GetValue("NoAutoUpdate");
                updateInfo.AutoUpdateEnabled = noAutoUpdate == null || (int)noAutoUpdate == 0;

                // Query Windows Update via WMI (requires admin)
                // This is a simplified check - full implementation would use Windows Update API
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_QuickFixEngineering");
                var updates = searcher.Get();

                // Get last installed update
                DateTime? lastUpdate = null;
                foreach (ManagementObject obj in updates)
                {
                    var installedOnStr = obj["InstalledOn"]?.ToString();
                    if (!string.IsNullOrEmpty(installedOnStr))
                    {
                        if (DateTime.TryParse(installedOnStr, out var installedOn))
                        {
                            if (lastUpdate == null || installedOn > lastUpdate)
                            {
                                lastUpdate = installedOn;
                            }
                        }
                    }
                }

                // If last update is more than 30 days old, suggest checking for updates
                if (lastUpdate != null && (DateTime.Now - lastUpdate.Value).TotalDays > 30)
                {
                    updateInfo.UpdatesAvailable = true;
                }
            }
            catch
            {
                // Windows Update check requires admin privileges
            }
        });

        return updateInfo;
    }

    #region Private Helpers

    private WindowsVersion DetermineWindowsVersion(OSVERSIONINFOEX versionInfo)
    {
        // Windows 11: Build 22000+
        if (versionInfo.dwMajorVersion == 10 && versionInfo.dwMinorVersion == 0 && versionInfo.dwBuildNumber >= 22000)
        {
            return WindowsVersion.Windows11;
        }

        // Windows 10: Build 10240+
        if (versionInfo.dwMajorVersion == 10 && versionInfo.dwMinorVersion == 0)
        {
            return WindowsVersion.Windows10;
        }

        // Windows 8.1
        if (versionInfo.dwMajorVersion == 6 && versionInfo.dwMinorVersion == 3)
        {
            return WindowsVersion.Windows81;
        }

        // Windows 8
        if (versionInfo.dwMajorVersion == 6 && versionInfo.dwMinorVersion == 2)
        {
            return WindowsVersion.Windows8;
        }

        // Windows 7
        if (versionInfo.dwMajorVersion == 6 && versionInfo.dwMinorVersion == 1)
        {
            return WindowsVersion.Windows7;
        }

        return WindowsVersion.Unknown;
    }

    private string GetWindowsName(WindowsVersion version, int buildNumber)
    {
        return version switch
        {
            WindowsVersion.Windows11 => "Windows 11",
            WindowsVersion.Windows10 => "Windows 10",
            WindowsVersion.Windows81 => "Windows 8.1",
            WindowsVersion.Windows8 => "Windows 8",
            WindowsVersion.Windows7 => "Windows 7",
            _ => "Unknown Windows"
        };
    }

    private string GetWindowsVersionString(WindowsVersion version, int buildNumber)
    {
        if (version == WindowsVersion.Windows11)
        {
            return buildNumber switch
            {
                >= 22631 => "23H2",
                >= 22621 => "22H2",
                >= 22000 => "21H2",
                _ => $"Build {buildNumber}"
            };
        }

        if (version == WindowsVersion.Windows10)
        {
            return buildNumber switch
            {
                >= 19045 => "22H2",
                >= 19044 => "21H2",
                >= 19043 => "21H1",
                >= 19042 => "20H2",
                >= 19041 => "2004",
                >= 18363 => "1909",
                >= 18362 => "1903",
                >= 17763 => "1809",
                >= 17134 => "1803",
                >= 16299 => "1709",
                >= 15063 => "1703",
                >= 14393 => "1607",
                >= 10586 => "1511",
                >= 10240 => "1507",
                _ => $"Build {buildNumber}"
            };
        }

        return $"Build {buildNumber}";
    }

    private bool CheckActivationStatus()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM SoftwareLicensingProduct WHERE ApplicationID = '55c92734-d682-4d71-983e-d6ec3f16059f' AND LicenseStatus = 1");
            return searcher.Get().Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private bool CheckInsiderBuild()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsSelfHost\Applicability");
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    private string GetPartialProductKey()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var productId = key?.GetValue("ProductId")?.ToString();

            if (!string.IsNullOrEmpty(productId) && productId.Length > 5)
            {
                return $"*****-{productId.Substring(productId.Length - 5)}";
            }
        }
        catch { }

        return "Unknown";
    }

    #endregion

    #region Optimizations

    private List<OSOptimization> GetPerformanceOptimizations(WindowsVersion version)
    {
        var optimizations = new List<OSOptimization>();

        // Disable Windows Search indexing for better performance
        optimizations.Add(new OSOptimization
        {
            Id = "perf_001",
            Name = "Disable Windows Search Indexing",
            Description = "Reduces background CPU and disk usage by disabling file indexing",
            Category = "Performance",
            MinVersion = WindowsVersion.Windows7,
            MaxVersion = WindowsVersion.Windows11,
            Impact = OptimizationImpact.Medium,
            RequiresRestart = false,
            IsReversible = true,
            Steps = new List<OSOptimizationStep>
            {
                new() { Type = OSStepType.StopService, Target = "WSearch" },
                new() { Type = OSStepType.DisableService, Target = "WSearch" }
            }
        });

        // Disable Superfetch/SysMain (especially good for SSDs)
        optimizations.Add(new OSOptimization
        {
            Id = "perf_002",
            Name = "Disable Superfetch/SysMain",
            Description = "Improves SSD performance by disabling prefetching",
            Category = "Performance",
            MinVersion = WindowsVersion.Windows7,
            MaxVersion = WindowsVersion.Windows11,
            Impact = OptimizationImpact.Low,
            RequiresRestart = false,
            IsReversible = true,
            Steps = new List<OSOptimizationStep>
            {
                new() { Type = OSStepType.StopService, Target = "SysMain" },
                new() { Type = OSStepType.DisableService, Target = "SysMain" }
            }
        });

        // Disable Windows Tips and Suggestions (Win10+)
        if (version >= WindowsVersion.Windows10)
        {
            optimizations.Add(new OSOptimization
            {
                Id = "perf_003",
                Name = "Disable Windows Tips",
                Description = "Disables automatic tips and suggestions",
                Category = "Performance",
                MinVersion = WindowsVersion.Windows10,
                MaxVersion = WindowsVersion.Windows11,
                Impact = OptimizationImpact.Low,
                RequiresRestart = false,
                IsReversible = true,
                Steps = new List<OSOptimizationStep>
                {
                    new()
                    {
                        Type = OSStepType.SetRegistryValue,
                        Target = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SoftLandingEnabled",
                        Value = "0"
                    }
                }
            });
        }

        // Disable Game DVR and Game Bar (Win10+)
        if (version >= WindowsVersion.Windows10)
        {
            optimizations.Add(new OSOptimization
            {
                Id = "perf_004",
                Name = "Disable Game DVR",
                Description = "Improves gaming performance by disabling background recording",
                Category = "Performance",
                MinVersion = WindowsVersion.Windows10,
                MaxVersion = WindowsVersion.Windows11,
                Impact = OptimizationImpact.Medium,
                RequiresRestart = false,
                IsReversible = true,
                Steps = new List<OSOptimizationStep>
                {
                    new()
                    {
                        Type = OSStepType.SetRegistryValue,
                        Target = @"HKEY_CURRENT_USER\System\GameConfigStore\GameDVR_Enabled",
                        Value = "0"
                    },
                    new()
                    {
                        Type = OSStepType.SetRegistryValue,
                        Target = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\GameDVR\AppCaptureEnabled",
                        Value = "0"
                    }
                }
            });
        }

        // Enable Hardware Accelerated GPU Scheduling (Win10 2004+)
        if (version >= WindowsVersion.Windows10)
        {
            optimizations.Add(new OSOptimization
            {
                Id = "perf_005",
                Name = "Enable Hardware GPU Scheduling",
                Description = "Improves graphics performance on supported GPUs",
                Category = "Performance",
                MinVersion = WindowsVersion.Windows10,
                MaxVersion = WindowsVersion.Windows11,
                Impact = OptimizationImpact.Medium,
                RequiresRestart = true,
                IsReversible = true,
                WarningMessage = "Requires compatible GPU driver",
                Steps = new List<OSOptimizationStep>
                {
                    new()
                    {
                        Type = OSStepType.SetRegistryValue,
                        Target = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\HwSchMode",
                        Value = "2"
                    }
                }
            });
        }

        return optimizations;
    }

    private List<OSOptimization> GetPrivacyOptimizations(WindowsVersion version)
    {
        var optimizations = new List<OSOptimization>();

        // Disable Telemetry (Win10+)
        if (version >= WindowsVersion.Windows10)
        {
            optimizations.Add(new OSOptimization
            {
                Id = "priv_001",
                Name = "Disable Telemetry",
                Description = "Reduces data collection by Microsoft",
                Category = "Privacy",
                MinVersion = WindowsVersion.Windows10,
                MaxVersion = WindowsVersion.Windows11,
                Impact = OptimizationImpact.Low,
                RequiresRestart = false,
                IsReversible = true,
                Steps = new List<OSOptimizationStep>
                {
                    new()
                    {
                        Type = OSStepType.SetRegistryValue,
                        Target = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection\AllowTelemetry",
                        Value = "0"
                    },
                    new() { Type = OSStepType.DisableService, Target = "DiagTrack" },
                    new() { Type = OSStepType.DisableService, Target = "dmwappushservice" }
                }
            });
        }

        // Disable Cortana (Win10+)
        if (version >= WindowsVersion.Windows10)
        {
            optimizations.Add(new OSOptimization
            {
                Id = "priv_002",
                Name = "Disable Cortana",
                Description = "Disables Cortana voice assistant",
                Category = "Privacy",
                MinVersion = WindowsVersion.Windows10,
                MaxVersion = WindowsVersion.Windows11,
                Impact = OptimizationImpact.Low,
                RequiresRestart = false,
                IsReversible = true,
                Steps = new List<OSOptimizationStep>
                {
                    new()
                    {
                        Type = OSStepType.SetRegistryValue,
                        Target = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search\AllowCortana",
                        Value = "0"
                    }
                }
            });
        }

        // Disable Location Tracking
        optimizations.Add(new OSOptimization
        {
            Id = "priv_003",
            Name = "Disable Location Tracking",
            Description = "Prevents apps from accessing location",
            Category = "Privacy",
            MinVersion = WindowsVersion.Windows10,
            MaxVersion = WindowsVersion.Windows11,
            Impact = OptimizationImpact.Low,
            RequiresRestart = false,
            IsReversible = true,
            Steps = new List<OSOptimizationStep>
            {
                new()
                {
                    Type = OSStepType.SetRegistryValue,
                    Target = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location\Value",
                    Value = "Deny"
                }
            }
        });

        // Disable Activity History (Win10+)
        if (version >= WindowsVersion.Windows10)
        {
            optimizations.Add(new OSOptimization
            {
                Id = "priv_004",
                Name = "Disable Activity History",
                Description = "Stops Windows from collecting activity history",
                Category = "Privacy",
                MinVersion = WindowsVersion.Windows10,
                MaxVersion = WindowsVersion.Windows11,
                Impact = OptimizationImpact.Low,
                RequiresRestart = false,
                IsReversible = true,
                Steps = new List<OSOptimizationStep>
                {
                    new()
                    {
                        Type = OSStepType.SetRegistryValue,
                        Target = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System\EnableActivityFeed",
                        Value = "0"
                    },
                    new()
                    {
                        Type = OSStepType.SetRegistryValue,
                        Target = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System\PublishUserActivities",
                        Value = "0"
                    }
                }
            });
        }

        return optimizations;
    }

    private List<OSOptimization> GetSecurityOptimizations(WindowsVersion version)
    {
        var optimizations = new List<OSOptimization>();

        // Enable Windows Defender Real-Time Protection
        optimizations.Add(new OSOptimization
        {
            Id = "sec_001",
            Name = "Enable Windows Defender",
            Description = "Ensures real-time protection is enabled",
            Category = "Security",
            MinVersion = WindowsVersion.Windows7,
            MaxVersion = WindowsVersion.Windows11,
            Impact = OptimizationImpact.High,
            RequiresRestart = false,
            IsReversible = true,
            Steps = new List<OSOptimizationStep>
            {
                new()
                {
                    Type = OSStepType.SetRegistryValue,
                    Target = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\DisableAntiSpyware",
                    Value = "0"
                }
            }
        });

        // Enable Firewall
        optimizations.Add(new OSOptimization
        {
            Id = "sec_002",
            Name = "Enable Windows Firewall",
            Description = "Ensures firewall is active on all profiles",
            Category = "Security",
            MinVersion = WindowsVersion.Windows7,
            MaxVersion = WindowsVersion.Windows11,
            Impact = OptimizationImpact.High,
            RequiresRestart = false,
            IsReversible = true,
            Steps = new List<OSOptimizationStep>
            {
                new() { Type = OSStepType.RunCommand, Target = "netsh advfirewall set allprofiles state on" }
            }
        });

        // Disable SMBv1 (security risk)
        if (version >= WindowsVersion.Windows10)
        {
            optimizations.Add(new OSOptimization
            {
                Id = "sec_003",
                Name = "Disable SMBv1 Protocol",
                Description = "Removes vulnerable SMBv1 protocol (WannaCry protection)",
                Category = "Security",
                MinVersion = WindowsVersion.Windows10,
                MaxVersion = WindowsVersion.Windows11,
                Impact = OptimizationImpact.High,
                RequiresRestart = true,
                IsReversible = true,
                WarningMessage = "May affect compatibility with very old network devices",
                Steps = new List<OSOptimizationStep>
                {
                    new() { Type = OSStepType.RunCommand, Target = "dism /online /disable-feature /featurename:SMB1Protocol /norestart" }
                }
            });
        }

        return optimizations;
    }

    private List<OSOptimization> GetUIOptimizations(WindowsVersion version)
    {
        var optimizations = new List<OSOptimization>();

        // Disable Animations
        optimizations.Add(new OSOptimization
        {
            Id = "ui_001",
            Name = "Disable Animations",
            Description = "Makes UI feel snappier by removing animations",
            Category = "UI",
            MinVersion = WindowsVersion.Windows7,
            MaxVersion = WindowsVersion.Windows11,
            Impact = OptimizationImpact.Medium,
            RequiresRestart = false,
            IsReversible = true,
            Steps = new List<OSOptimizationStep>
            {
                new()
                {
                    Type = OSStepType.SetRegistryValue,
                    Target = @"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics\MinAnimate",
                    Value = "0"
                }
            }
        });

        // Show file extensions
        optimizations.Add(new OSOptimization
        {
            Id = "ui_002",
            Name = "Show File Extensions",
            Description = "Shows file extensions in File Explorer",
            Category = "UI",
            MinVersion = WindowsVersion.Windows7,
            MaxVersion = WindowsVersion.Windows11,
            Impact = OptimizationImpact.Low,
            RequiresRestart = false,
            IsReversible = true,
            Steps = new List<OSOptimizationStep>
            {
                new()
                {
                    Type = OSStepType.SetRegistryValue,
                    Target = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\HideFileExt",
                    Value = "0"
                }
            }
        });

        // Disable Transparency Effects (Win10+)
        if (version >= WindowsVersion.Windows10)
        {
            optimizations.Add(new OSOptimization
            {
                Id = "ui_003",
                Name = "Disable Transparency Effects",
                Description = "Improves performance by disabling UI transparency",
                Category = "UI",
                MinVersion = WindowsVersion.Windows10,
                MaxVersion = WindowsVersion.Windows11,
                Impact = OptimizationImpact.Low,
                RequiresRestart = false,
                IsReversible = true,
                Steps = new List<OSOptimizationStep>
                {
                    new()
                    {
                        Type = OSStepType.SetRegistryValue,
                        Target = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\EnableTransparency",
                        Value = "0"
                    }
                }
            });
        }

        return optimizations;
    }

    #endregion

    #region Optimization Actions

    private void SetRegistryValue(string target, string value)
    {
        try
        {
            var parts = target.Split('\\');
            if (parts.Length < 2) return;

            var hive = parts[0] switch
            {
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                "HKEY_USERS" => Registry.Users,
                _ => null
            };

            if (hive == null) return;

            var keyPath = string.Join("\\", parts.Skip(1).Take(parts.Length - 2));
            var valueName = parts[parts.Length - 1];

            using var key = hive.CreateSubKey(keyPath, true);
            if (key != null)
            {
                if (int.TryParse(value, out var intValue))
                {
                    key.SetValue(valueName, intValue, RegistryValueKind.DWord);
                }
                else
                {
                    key.SetValue(valueName, value, RegistryValueKind.String);
                }
            }
        }
        catch { }
    }

    private void DeleteRegistryValue(string target)
    {
        try
        {
            var parts = target.Split('\\');
            if (parts.Length < 2) return;

            var hive = parts[0] switch
            {
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                _ => null
            };

            if (hive == null) return;

            var keyPath = string.Join("\\", parts.Skip(1).Take(parts.Length - 2));
            var valueName = parts[parts.Length - 1];

            using var key = hive.OpenSubKey(keyPath, true);
            key?.DeleteValue(valueName, false);
        }
        catch { }
    }

    private async Task StopServiceAsync(string serviceName)
    {
        try
        {
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sc",
                Arguments = $"stop {serviceName}",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            if (process != null)
            {
                await process.WaitForExitAsync();
            }
        }
        catch { }
    }

    private async Task DisableServiceAsync(string serviceName)
    {
        try
        {
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sc",
                Arguments = $"config {serviceName} start= disabled",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            if (process != null)
            {
                await process.WaitForExitAsync();
            }
        }
        catch { }
    }

    private async Task EnableServiceAsync(string serviceName)
    {
        try
        {
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sc",
                Arguments = $"config {serviceName} start= auto",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            if (process != null)
            {
                await process.WaitForExitAsync();
            }
        }
        catch { }
    }

    private async Task RunCommandAsync(string command)
    {
        try
        {
            var parts = command.Split(' ', 2);
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = parts[0],
                Arguments = parts.Length > 1 ? parts[1] : "",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            if (process != null)
            {
                await process.WaitForExitAsync();
            }
        }
        catch { }
    }

    private void DeleteFile(string filePath)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        catch { }
    }

    private void SetGroupPolicy(string target, string value)
    {
        // Group Policy requires complex COM interactions
        // Simplified to registry equivalent
        SetRegistryValue(target, value);
    }

    #endregion

    #region P/Invoke

    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct OSVERSIONINFOEX
    {
        public uint dwOSVersionInfoSize;
        public uint dwMajorVersion;
        public uint dwMinorVersion;
        public uint dwBuildNumber;
        public uint dwPlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion;
        public ushort wServicePackMajor;
        public ushort wServicePackMinor;
        public ushort wSuiteMask;
        public byte wProductType;
        public byte wReserved;

        public OSVERSIONINFOEX()
        {
            dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(OSVERSIONINFOEX));
            szCSDVersion = "";
        }
    }

    #endregion
}
