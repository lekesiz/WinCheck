using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.Infrastructure.Services;

public class ServiceOptimizerService : IServiceOptimizerService
{
    private const string BackupFileName = "service_backup.json";
    private readonly string _backupPath;

    public ServiceOptimizerService()
    {
        _backupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinCheck",
            BackupFileName
        );

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(_backupPath)!);
    }

    public async Task<List<WindowsServiceInfo>> GetAllServicesAsync()
    {
        var services = new List<WindowsServiceInfo>();

        await Task.Run(() =>
        {
            var allServices = ServiceController.GetServices();

            foreach (var service in allServices)
            {
                try
                {
                    var serviceInfo = new WindowsServiceInfo
                    {
                        Name = service.ServiceName,
                        DisplayName = service.DisplayName,
                        Status = ConvertStatus(service.Status),
                        CanBeStopped = service.CanStop,
                        CanBePaused = service.CanPauseAndContinue
                    };

                    // Get additional info from registry
                    using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{service.ServiceName}");
                    if (key != null)
                    {
                        serviceInfo.Description = key.GetValue("Description")?.ToString() ?? "";
                        serviceInfo.ExecutablePath = key.GetValue("ImagePath")?.ToString() ?? "";
                        serviceInfo.StartMode = ConvertStartMode(key.GetValue("Start"));

                        // Get dependencies
                        var dependOnService = key.GetValue("DependOnService") as string[];
                        if (dependOnService != null)
                        {
                            serviceInfo.Dependencies.AddRange(dependOnService);
                        }
                    }

                    // Get dependent services
                    var dependentServices = service.DependentServices;
                    serviceInfo.DependentServices.AddRange(dependentServices.Select(s => s.ServiceName));

                    services.Add(serviceInfo);
                }
                catch
                {
                    // Skip services we can't access
                }
            }
        });

        return services;
    }

    public async Task<List<ServiceOptimization>> GetOptimizableServicesAsync()
    {
        var allServices = await GetAllServicesAsync();
        var optimizations = new List<ServiceOptimization>();

        // Get service optimization database
        var database = GetServiceOptimizationDatabase();

        foreach (var serviceInfo in allServices)
        {
            if (database.TryGetValue(serviceInfo.Name, out var optimization))
            {
                // Only recommend if current mode is not optimal
                if (serviceInfo.StartMode != optimization.RecommendedStartMode)
                {
                    optimization.CurrentStartMode = serviceInfo.StartMode;
                    optimization.BackupStartMode = serviceInfo.StartMode;
                    optimizations.Add(optimization);
                }
            }
        }

        return optimizations.OrderByDescending(o => o.Safety).ToList();
    }

    public async Task<bool> OptimizeServiceAsync(ServiceOptimization optimization)
    {
        try
        {
            // Backup current state first
            await CreateBackupAsync();

            // Change service startup type
            using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{optimization.ServiceName}", true);
            if (key != null)
            {
                var startValue = optimization.RecommendedStartMode switch
                {
                    WinCheckServiceStartMode.Automatic => 2,
                    WinCheckServiceStartMode.AutomaticDelayed => 2,
                    WinCheckServiceStartMode.Manual => 3,
                    WinCheckServiceStartMode.Disabled => 4,
                    _ => 3
                };

                key.SetValue("Start", startValue);

                // Set delayed auto-start if needed
                if (optimization.RecommendedStartMode == WinCheckServiceStartMode.AutomaticDelayed)
                {
                    key.SetValue("DelayedAutostart", 1);
                }
            }

            // Stop service if recommended mode is Disabled or Manual
            if (optimization.RecommendedStartMode is WinCheckServiceStartMode.Disabled or WinCheckServiceStartMode.Manual)
            {
                try
                {
                    using var service = new ServiceController(optimization.ServiceName);
                    if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running && service.CanStop)
                    {
                        service.Stop();
                        service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }
                catch
                {
                    // Service may not be running or can't be stopped
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RestoreServiceAsync(string serviceName)
    {
        try
        {
            var backup = await LoadBackupAsync();
            if (backup == null) return false;

            var entry = backup.Services.FirstOrDefault(s => s.ServiceName == serviceName);
            if (entry == null) return false;

            using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}", true);
            if (key != null)
            {
                var startValue = entry.StartMode switch
                {
                    WinCheckServiceStartMode.Automatic => 2,
                    WinCheckServiceStartMode.AutomaticDelayed => 2,
                    WinCheckServiceStartMode.Manual => 3,
                    WinCheckServiceStartMode.Disabled => 4,
                    _ => 3
                };

                key.SetValue("Start", startValue);
            }

            // Restore running status if it was running
            if (entry.Status == ServiceStatus.Running)
            {
                try
                {
                    using var service = new ServiceController(serviceName);
                    if (service.Status == System.ServiceProcess.ServiceControllerStatus.Stopped)
                    {
                        service.Start();
                        service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    }
                }
                catch
                {
                    // Service may not start
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CreateBackupAsync()
    {
        try
        {
            var services = await GetAllServicesAsync();

            var backup = new ServiceBackup
            {
                BackupDate = DateTime.Now,
                Services = services.Select(s => new ServiceBackupEntry
                {
                    ServiceName = s.Name,
                    StartMode = s.StartMode,
                    Status = s.Status
                }).ToList()
            };

            var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_backupPath, json);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RestoreFromBackupAsync()
    {
        try
        {
            var backup = await LoadBackupAsync();
            if (backup == null) return false;

            foreach (var entry in backup.Services)
            {
                await RestoreServiceAsync(entry.ServiceName);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    #region Private Helpers

    private async Task<ServiceBackup?> LoadBackupAsync()
    {
        try
        {
            if (!File.Exists(_backupPath)) return null;

            var json = await File.ReadAllTextAsync(_backupPath);
            return JsonSerializer.Deserialize<ServiceBackup>(json);
        }
        catch
        {
            return null;
        }
    }

    private ServiceStatus ConvertStatus(ServiceControllerStatus status)
    {
        return status switch
        {
            ServiceControllerStatus.Stopped => ServiceStatus.Stopped,
            ServiceControllerStatus.StartPending => ServiceStatus.StartPending,
            ServiceControllerStatus.StopPending => ServiceStatus.StopPending,
            ServiceControllerStatus.Running => ServiceStatus.Running,
            ServiceControllerStatus.ContinuePending => ServiceStatus.ContinuePending,
            ServiceControllerStatus.PausePending => ServiceStatus.PausePending,
            ServiceControllerStatus.Paused => ServiceStatus.Paused,
            _ => ServiceStatus.Unknown
        };
    }

    private WinCheckServiceStartMode ConvertStartMode(object? startValue)
    {
        if (startValue == null) return WinCheckServiceStartMode.Manual;

        return Convert.ToInt32(startValue) switch
        {
            0 => WinCheckServiceStartMode.Boot,
            1 => WinCheckServiceStartMode.System,
            2 => WinCheckServiceStartMode.Automatic,
            3 => WinCheckServiceStartMode.Manual,
            4 => WinCheckServiceStartMode.Disabled,
            _ => WinCheckServiceStartMode.Manual
        };
    }

    private Dictionary<string, ServiceOptimization> GetServiceOptimizationDatabase()
    {
        // Comprehensive database of safe-to-disable services
        return new Dictionary<string, ServiceOptimization>
        {
            // SAFE - Telemetry and Data Collection
            ["DiagTrack"] = new()
            {
                ServiceName = "DiagTrack",
                DisplayName = "Connected User Experiences and Telemetry",
                Reason = "Collects diagnostic data. Disable for privacy.",
                RecommendedStartMode = WinCheckServiceStartMode.Disabled,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 10 * 1024 * 1024,
                EstimatedBootTimeSavingMs = 200
            },
            ["dmwappushservice"] = new()
            {
                ServiceName = "dmwappushservice",
                DisplayName = "WAP Push Message Routing Service",
                Reason = "Used for telemetry. Safe to disable.",
                RecommendedStartMode = WinCheckServiceStartMode.Disabled,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 5 * 1024 * 1024
            },

            // SAFE - Windows Search and Indexing
            ["WSearch"] = new()
            {
                ServiceName = "WSearch",
                DisplayName = "Windows Search",
                Reason = "Heavy disk/CPU usage. Disable if you don't use Windows search.",
                RecommendedStartMode = WinCheckServiceStartMode.Disabled,
                Safety = SafetyLevel.MostlySafe,
                EstimatedMemorySavingBytes = 50 * 1024 * 1024,
                EstimatedBootTimeSavingMs = 500
            },

            // SAFE - Superfetch/SysMain
            ["SysMain"] = new()
            {
                ServiceName = "SysMain",
                DisplayName = "SysMain (Superfetch)",
                Reason = "Not needed on SSDs. Can cause high disk usage.",
                RecommendedStartMode = WinCheckServiceStartMode.Disabled,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 30 * 1024 * 1024,
                EstimatedBootTimeSavingMs = 300
            },

            // SAFE - Print Spooler (if not using printer)
            ["Spooler"] = new()
            {
                ServiceName = "Spooler",
                DisplayName = "Print Spooler",
                Reason = "Only needed if you use a printer.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 15 * 1024 * 1024
            },

            // SAFE - Fax
            ["Fax"] = new()
            {
                ServiceName = "Fax",
                DisplayName = "Fax Service",
                Reason = "Rarely used. Safe to disable.",
                RecommendedStartMode = WinCheckServiceStartMode.Disabled,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 10 * 1024 * 1024
            },

            // SAFE - Windows Biometric Service (if not using fingerprint)
            ["WbioSrvc"] = new()
            {
                ServiceName = "WbioSrvc",
                DisplayName = "Windows Biometric Service",
                Reason = "Only needed for fingerprint readers.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 8 * 1024 * 1024
            },

            // SAFE - Remote Registry
            ["RemoteRegistry"] = new()
            {
                ServiceName = "RemoteRegistry",
                DisplayName = "Remote Registry",
                Reason = "Security risk. Disable unless specifically needed.",
                RecommendedStartMode = WinCheckServiceStartMode.Disabled,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 5 * 1024 * 1024
            },

            // SAFE - Xbox Services (if not gaming)
            ["XblAuthManager"] = new()
            {
                ServiceName = "XblAuthManager",
                DisplayName = "Xbox Live Auth Manager",
                Reason = "Only needed for Xbox features.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 12 * 1024 * 1024
            },
            ["XblGameSave"] = new()
            {
                ServiceName = "XblGameSave",
                DisplayName = "Xbox Live Game Save",
                Reason = "Only needed for Xbox features.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 8 * 1024 * 1024
            },
            ["XboxNetApiSvc"] = new()
            {
                ServiceName = "XboxNetApiSvc",
                DisplayName = "Xbox Live Networking Service",
                Reason = "Only needed for Xbox features.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 10 * 1024 * 1024
            },

            // SAFE - Bluetooth (if not using)
            ["bthserv"] = new()
            {
                ServiceName = "bthserv",
                DisplayName = "Bluetooth Support Service",
                Reason = "Only needed if using Bluetooth devices.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 15 * 1024 * 1024
            },

            // SAFE - Tablet Input Service
            ["TabletInputService"] = new()
            {
                ServiceName = "TabletInputService",
                DisplayName = "Touch Keyboard and Handwriting Panel Service",
                Reason = "Only needed for touch screens.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 20 * 1024 * 1024
            },

            // SAFE - Windows Error Reporting
            ["WerSvc"] = new()
            {
                ServiceName = "WerSvc",
                DisplayName = "Windows Error Reporting",
                Reason = "Sends crash reports to Microsoft. Can be disabled.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 8 * 1024 * 1024
            },

            // SAFE - IP Helper (if not using IPv6)
            ["iphlpsvc"] = new()
            {
                ServiceName = "iphlpsvc",
                DisplayName = "IP Helper",
                Reason = "Only needed for IPv6. Can disable if using IPv4 only.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 6 * 1024 * 1024
            },

            // SAFE - Offline Files
            ["CscService"] = new()
            {
                ServiceName = "CscService",
                DisplayName = "Offline Files",
                Reason = "Rarely used feature. Can be disabled.",
                RecommendedStartMode = WinCheckServiceStartMode.Disabled,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 15 * 1024 * 1024
            },

            // SAFE - Program Compatibility Assistant
            ["PcaSvc"] = new()
            {
                ServiceName = "PcaSvc",
                DisplayName = "Program Compatibility Assistant Service",
                Reason = "Not critical. Can be set to manual.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.MostlySafe,
                EstimatedMemorySavingBytes = 10 * 1024 * 1024
            },

            // SAFE - Secondary Logon
            ["seclogon"] = new()
            {
                ServiceName = "seclogon",
                DisplayName = "Secondary Logon",
                Reason = "Only needed if running programs as different user.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 5 * 1024 * 1024
            },

            // SAFE - Smart Card services (if not using smart cards)
            ["SCardSvr"] = new()
            {
                ServiceName = "SCardSvr",
                DisplayName = "Smart Card",
                Reason = "Only needed for smart card readers.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 8 * 1024 * 1024
            },

            // SAFE - Windows Media Player Network Sharing
            ["WMPNetworkSvc"] = new()
            {
                ServiceName = "WMPNetworkSvc",
                DisplayName = "Windows Media Player Network Sharing Service",
                Reason = "Only needed if sharing media with Windows Media Player.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 12 * 1024 * 1024
            },

            // SAFE - Windows Mobile Hotspot Service
            ["icssvc"] = new()
            {
                ServiceName = "icssvc",
                DisplayName = "Windows Mobile Hotspot Service",
                Reason = "Only needed for mobile hotspot feature.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 10 * 1024 * 1024
            },

            // SAFE - Geolocation Service
            ["lfsvc"] = new()
            {
                ServiceName = "lfsvc",
                DisplayName = "Geolocation Service",
                Reason = "Privacy concern. Disable if not using location features.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 7 * 1024 * 1024
            },

            // SAFE - Downloaded Maps Manager
            ["MapsBroker"] = new()
            {
                ServiceName = "MapsBroker",
                DisplayName = "Downloaded Maps Manager",
                Reason = "Only needed for Maps app.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 15 * 1024 * 1024
            },

            // SAFE - Retail Demo Service
            ["RetailDemo"] = new()
            {
                ServiceName = "RetailDemo",
                DisplayName = "Retail Demo Service",
                Reason = "Retail store feature. Safe to disable.",
                RecommendedStartMode = WinCheckServiceStartMode.Disabled,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 5 * 1024 * 1024
            },

            // SAFE - Sensor Services
            ["SensrSvc"] = new()
            {
                ServiceName = "SensrSvc",
                DisplayName = "Sensor Service",
                Reason = "Only needed for devices with sensors.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 8 * 1024 * 1024
            },

            // SAFE - Themes (if using classic theme)
            ["Themes"] = new()
            {
                ServiceName = "Themes",
                DisplayName = "Themes",
                Reason = "Only disable if using classic theme for performance.",
                RecommendedStartMode = WinCheckServiceStartMode.Automatic,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 10 * 1024 * 1024
            },

            // SAFE - Windows Insider Service
            ["wisvc"] = new()
            {
                ServiceName = "wisvc",
                DisplayName = "Windows Insider Service",
                Reason = "Only needed for Windows Insider Program.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Safe,
                EstimatedMemorySavingBytes = 5 * 1024 * 1024
            },

            // SAFE - Parental Controls
            ["WpcMonSvc"] = new()
            {
                ServiceName = "WpcMonSvc",
                DisplayName = "Parental Controls",
                Reason = "Only needed if using parental controls.",
                RecommendedStartMode = WinCheckServiceStartMode.Manual,
                Safety = SafetyLevel.Conditional,
                EstimatedMemorySavingBytes = 12 * 1024 * 1024
            }
        };
    }

    #endregion
}
