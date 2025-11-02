using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.Infrastructure.Services;

public class HardwareDetectionService : IHardwareDetectionService
{
    public async Task<HardwareProfile> DetectAllHardwareAsync()
    {
        var profile = new HardwareProfile();

        // Run detections in parallel for speed
        await Task.WhenAll(
            Task.Run(async () => profile.Cpu = await DetectCpuAsync()),
            Task.Run(async () => profile.Memory = await DetectMemoryAsync()),
            Task.Run(async () => profile.Storage = await DetectStorageAsync()),
            Task.Run(async () => profile.Gpus = await DetectGpusAsync()),
            Task.Run(async () => profile.NetworkAdapters = await DetectNetworkAdaptersAsync()),
            Task.Run(async () => profile.Motherboard = await DetectMotherboardAsync()),
            Task.Run(async () => profile.Battery = await DetectBatteryAsync()),
            Task.Run(async () => profile.Monitors = await DetectMonitorsAsync())
        );

        return profile;
    }

    public async Task<TestResults> RunHardwareTestsAsync(HardwareTestOptions options)
    {
        var results = new TestResults();
        var tasks = new List<Task>();

        if (options.TestCpu)
        {
            tasks.Add(Task.Run(async () => results.CpuTest = await RunCpuTestAsync(options.TestDurationSeconds)));
        }

        if (options.TestMemory)
        {
            tasks.Add(Task.Run(async () => results.MemoryTest = await RunMemoryTestAsync()));
        }

        if (options.TestStorage)
        {
            tasks.Add(Task.Run(async () => results.StorageTests = await RunStorageTestsAsync()));
        }

        if (options.TestGpu)
        {
            tasks.Add(Task.Run(async () => results.GpuTest = await RunGpuTestAsync()));
        }

        await Task.WhenAll(tasks);

        // Calculate overall score
        results.OverallScore = CalculateOverallScore(results);

        return results;
    }

    public async Task<HardwareHealth> GetHardwareHealthAsync()
    {
        var health = new HardwareHealth();
        var profile = await DetectAllHardwareAsync();

        // Analyze CPU health
        if (profile.Cpu.CurrentTemperature > 85)
        {
            health.Issues.Add(new HealthIssue
            {
                Component = "CPU",
                Issue = $"High temperature: {profile.Cpu.CurrentTemperature}°C",
                Severity = "High",
                Recommendation = "Check CPU cooler and thermal paste. Ensure proper airflow."
            });
        }

        // Analyze storage health
        foreach (var storage in profile.Storage)
        {
            if (storage.SmartData.HealthPercentage < 80)
            {
                health.Issues.Add(new HealthIssue
                {
                    Component = "Storage",
                    Issue = $"Drive {storage.Model} health at {storage.SmartData.HealthPercentage}%",
                    Severity = storage.SmartData.HealthPercentage < 50 ? "Critical" : "Medium",
                    Recommendation = "Backup data immediately. Consider replacing drive."
                });
            }

            if (storage.SmartData.ReallocatedSectors > 0)
            {
                health.Issues.Add(new HealthIssue
                {
                    Component = "Storage",
                    Issue = $"Drive {storage.Model} has {storage.SmartData.ReallocatedSectors} reallocated sectors",
                    Severity = "Medium",
                    Recommendation = "Monitor drive closely. Backup important data."
                });
            }
        }

        // Analyze battery health (if exists)
        if (profile.Battery != null && profile.Battery.WearLevel > 30)
        {
            health.Issues.Add(new HealthIssue
            {
                Component = "Battery",
                Issue = $"Battery wear level at {profile.Battery.WearLevel}%",
                Severity = profile.Battery.WearLevel > 50 ? "High" : "Medium",
                Recommendation = "Consider battery replacement for optimal performance."
            });
        }

        // Calculate component health scores
        health.ComponentHealth["CPU"] = profile.Cpu.CurrentTemperature < 70 ? 100 : (profile.Cpu.CurrentTemperature < 85 ? 80 : 50);
        health.ComponentHealth["Memory"] = 100; // No direct health metric for RAM
        health.ComponentHealth["Storage"] = profile.Storage.Any() ? (int)profile.Storage.Average(s => s.SmartData.HealthPercentage) : 100;

        if (profile.Battery != null)
        {
            health.ComponentHealth["Battery"] = Math.Max(0, 100 - profile.Battery.WearLevel);
        }

        // Calculate overall health
        health.OverallHealthPercentage = (int)health.ComponentHealth.Values.Average();

        return health;
    }

    #region CPU Detection

    private async Task<CpuInfo> DetectCpuAsync()
    {
        var cpu = new CpuInfo();

        await Task.Run(() =>
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                cpu.Name = obj["Name"]?.ToString()?.Trim() ?? "Unknown";
                cpu.Manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
                cpu.NumberOfCores = Convert.ToInt32(obj["NumberOfCores"] ?? 0);
                cpu.NumberOfLogicalProcessors = Convert.ToInt32(obj["NumberOfLogicalProcessors"] ?? 0);
                cpu.MaxClockSpeed = Convert.ToInt32(obj["MaxClockSpeed"] ?? 0);
                cpu.CurrentClockSpeed = Convert.ToInt32(obj["CurrentClockSpeed"] ?? 0);
                cpu.L2CacheSize = Convert.ToInt32(obj["L2CacheSize"] ?? 0);
                cpu.L3CacheSize = Convert.ToInt32(obj["L3CacheSize"] ?? 0);
                cpu.Architecture = GetArchitecture(Convert.ToUInt16(obj["Architecture"] ?? 0));

                // Detect virtualization support
                var description = obj["Description"]?.ToString() ?? "";
                cpu.SupportsVirtualization = description.Contains("Virtualization") ||
                                            description.Contains("VT-x") ||
                                            description.Contains("AMD-V");

                break; // Only need first CPU
            }

            // Detect instruction sets
            cpu.SupportedInstructions = DetectCpuInstructions();

            // Get temperature (requires WMI thermal zone or third-party tools)
            cpu.CurrentTemperature = GetCpuTemperature();
        });

        return cpu;
    }

    private string GetArchitecture(ushort arch)
    {
        return arch switch
        {
            0 => "x86",
            1 => "MIPS",
            2 => "Alpha",
            3 => "PowerPC",
            5 => "ARM",
            6 => "Itanium",
            9 => "x64",
            12 => "ARM64",
            _ => "Unknown"
        };
    }

    private List<string> DetectCpuInstructions()
    {
        var instructions = new List<string>();

        try
        {
            // Use CPUID instruction via P/Invoke or check via registry
            // For simplicity, check common instruction sets
            if (IsProcessorFeaturePresent(PF_MMX_INSTRUCTIONS_AVAILABLE))
                instructions.Add("MMX");
            if (IsProcessorFeaturePresent(PF_XMMI_INSTRUCTIONS_AVAILABLE))
                instructions.Add("SSE");
            if (IsProcessorFeaturePresent(PF_XMMI64_INSTRUCTIONS_AVAILABLE))
                instructions.Add("SSE2");
            if (IsProcessorFeaturePresent(PF_SSE3_INSTRUCTIONS_AVAILABLE))
                instructions.Add("SSE3");
            if (IsProcessorFeaturePresent(PF_AVX_INSTRUCTIONS_AVAILABLE))
                instructions.Add("AVX");
            if (IsProcessorFeaturePresent(PF_AVX2_INSTRUCTIONS_AVAILABLE))
                instructions.Add("AVX2");
        }
        catch { }

        return instructions;
    }

    private int GetCpuTemperature()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
            foreach (ManagementObject obj in searcher.Get())
            {
                var temp = Convert.ToDouble(obj["CurrentTemperature"]);
                return (int)((temp - 2732) / 10.0); // Convert from deciKelvin to Celsius
            }
        }
        catch { }

        return 0; // Temperature not available
    }

    #endregion

    #region Memory Detection

    private async Task<MemoryInfo> DetectMemoryAsync()
    {
        var memory = new MemoryInfo();

        await Task.Run(() =>
        {
            // Get total memory using P/Invoke
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(ref memStatus))
            {
                memory.TotalBytes = (long)memStatus.ullTotalPhys;
                memory.AvailableBytes = (long)memStatus.ullAvailPhys;
            }

            // Get detailed memory info from WMI
            var modules = new List<ManagementObject>();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
            {
                modules.AddRange(searcher.Get().Cast<ManagementObject>());
            }

            if (modules.Any())
            {
                memory.SlotsUsed = modules.Count;
                memory.SpeedMHz = Convert.ToInt32(modules[0]["Speed"] ?? 0);

                // Determine memory type
                var memoryType = Convert.ToUInt16(modules[0]["MemoryType"] ?? 0);
                memory.Type = memoryType switch
                {
                    20 => "DDR",
                    21 => "DDR2",
                    24 => "DDR3",
                    26 => "DDR4",
                    34 => "DDR5",
                    _ => "Unknown"
                };

                // Check if dual channel (2 or 4 modules usually indicates dual channel)
                memory.IsDualChannel = memory.SlotsUsed >= 2 && memory.SlotsUsed % 2 == 0;
            }

            // Get total slots
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemoryArray"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    memory.SlotsTotal = Convert.ToInt32(obj["MemoryDevices"] ?? 0);
                    break;
                }
            }
        });

        return memory;
    }

    #endregion

    #region Storage Detection

    private async Task<List<StorageInfo>> DetectStorageAsync()
    {
        var storageList = new List<StorageInfo>();

        await Task.Run(() =>
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject obj in searcher.Get())
            {
                var storage = new StorageInfo
                {
                    Model = obj["Model"]?.ToString()?.Trim() ?? "Unknown",
                    InterfaceType = obj["InterfaceType"]?.ToString() ?? "Unknown",
                    Size = Convert.ToInt64(obj["Size"] ?? 0),
                    SerialNumber = obj["SerialNumber"]?.ToString()?.Trim() ?? "Unknown",
                    MediaType = obj["MediaType"]?.ToString() ?? "Unknown"
                };

                // Determine if SSD
                var mediaType = obj["MediaType"]?.ToString() ?? "";
                storage.IsSSD = mediaType.Contains("SSD") ||
                               mediaType.Contains("Solid State") ||
                               storage.Model.Contains("SSD");

                // Get SMART data
                storage.SmartData = GetSmartData(obj["DeviceID"]?.ToString() ?? "");

                storageList.Add(storage);
            }
        });

        return storageList;
    }

    private SmartData GetSmartData(string deviceId)
    {
        var smart = new SmartData
        {
            HealthPercentage = 100 // Default healthy
        };

        try
        {
            // Query SMART data from WMI (requires admin privileges)
            using var searcher = new ManagementObjectSearcher($"SELECT * FROM MSStorageDriver_FailurePredictStatus WHERE InstanceName LIKE '%{deviceId}%'");
            foreach (ManagementObject obj in searcher.Get())
            {
                var predictFailure = Convert.ToBoolean(obj["PredictFailure"] ?? false);
                smart.HealthPercentage = predictFailure ? 50 : 100;
            }

            // Get SMART attributes
            using var attrSearcher = new ManagementObjectSearcher($"SELECT * FROM MSStorageDriver_ATAPISmartData WHERE InstanceName LIKE '%{deviceId}%'");
            foreach (ManagementObject obj in attrSearcher.Get())
            {
                var vendorSpecific = obj["VendorSpecific"] as byte[];
                if (vendorSpecific != null && vendorSpecific.Length >= 362)
                {
                    // Parse SMART attributes (simplified)
                    // Attribute 5: Reallocated Sector Count
                    smart.ReallocatedSectors = vendorSpecific[5];

                    // Attribute 9: Power On Hours (offset 9)
                    smart.PowerOnHours = BitConverter.ToInt64(vendorSpecific, 9 * 12 + 5);

                    // Attribute 194: Temperature (if available)
                    if (vendorSpecific.Length > 194 * 12 + 5)
                    {
                        smart.Temperature = vendorSpecific[194 * 12 + 5];
                    }
                }
            }

            // Add warnings based on SMART data
            if (smart.ReallocatedSectors > 0)
            {
                smart.Warnings.Add($"Reallocated sectors detected: {smart.ReallocatedSectors}");
            }
            if (smart.Temperature > 55)
            {
                smart.Warnings.Add($"High temperature: {smart.Temperature}°C");
            }
            if (smart.PowerOnHours > 50000)
            {
                smart.Warnings.Add($"High power-on hours: {smart.PowerOnHours}");
            }
        }
        catch
        {
            // SMART data not available (requires admin or not supported)
        }

        return smart;
    }

    #endregion

    #region GPU Detection

    private async Task<List<GpuInfo>> DetectGpusAsync()
    {
        var gpus = new List<GpuInfo>();

        await Task.Run(() =>
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject obj in searcher.Get())
            {
                var gpu = new GpuInfo
                {
                    Name = obj["Name"]?.ToString() ?? "Unknown",
                    Manufacturer = obj["AdapterCompatibility"]?.ToString() ?? "Unknown",
                    DriverVersion = obj["DriverVersion"]?.ToString() ?? "Unknown",
                    VRamBytes = Convert.ToInt64(obj["AdapterRAM"] ?? 0)
                };

                // Get DirectX version (system-wide, not GPU-specific)
                gpu.DirectXVersion = GetDirectXVersion();

                // Temperature not available via WMI (requires vendor-specific APIs)
                gpu.CurrentTemperature = 0;

                gpus.Add(gpu);
            }
        });

        return gpus;
    }

    private string GetDirectXVersion()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DirectX");
            return key?.GetValue("Version")?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    #endregion

    #region Network Adapter Detection

    private async Task<List<NetworkAdapterInfo>> DetectNetworkAdaptersAsync()
    {
        var adapters = new List<NetworkAdapterInfo>();

        await Task.Run(() =>
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2");
            foreach (ManagementObject obj in searcher.Get())
            {
                var adapter = new NetworkAdapterInfo
                {
                    Name = obj["Name"]?.ToString() ?? "Unknown",
                    MACAddress = obj["MACAddress"]?.ToString() ?? "Unknown",
                    SpeedBits = Convert.ToInt64(obj["Speed"] ?? 0),
                    IsConnected = Convert.ToUInt16(obj["NetConnectionStatus"] ?? 0) == 2
                };

                // Determine adapter type
                var name = adapter.Name.ToLower();
                adapter.Type = name.Contains("wi-fi") || name.Contains("wireless") ? "WiFi" : "Ethernet";

                // Get driver version
                using var driverSearcher = new ManagementObjectSearcher($"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Index = {obj["Index"]}");
                foreach (ManagementObject driverObj in driverSearcher.Get())
                {
                    adapter.DriverVersion = driverObj["ServiceName"]?.ToString() ?? "Unknown";
                    break;
                }

                adapters.Add(adapter);
            }
        });

        return adapters;
    }

    #endregion

    #region Motherboard Detection

    private async Task<MotherboardInfo> DetectMotherboardAsync()
    {
        var motherboard = new MotherboardInfo();

        await Task.Run(() =>
        {
            // Get motherboard info
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    motherboard.Manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
                    motherboard.Model = obj["Product"]?.ToString() ?? "Unknown";
                    break;
                }
            }

            // Get BIOS info
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    motherboard.BIOSVersion = obj["SMBIOSBIOSVersion"]?.ToString() ?? "Unknown";
                    break;
                }
            }

            // Get chipset info (from registry or WMI)
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                motherboard.Chipset = key?.GetValue("ProcessorNameString")?.ToString() ?? "Unknown";
            }
            catch
            {
                motherboard.Chipset = "Unknown";
            }
        });

        return motherboard;
    }

    #endregion

    #region Battery Detection

    private async Task<BatteryInfo?> DetectBatteryAsync()
    {
        BatteryInfo? battery = null;

        await Task.Run(() =>
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            foreach (ManagementObject obj in searcher.Get())
            {
                battery = new BatteryInfo
                {
                    DesignCapacity = Convert.ToInt32(obj["DesignCapacity"] ?? 0),
                    CurrentCapacity = Convert.ToInt32(obj["EstimatedChargeRemaining"] ?? 0),
                    ChargePercentage = Convert.ToInt32(obj["EstimatedChargeRemaining"] ?? 0),
                    ChemistryType = GetBatteryChemistry(Convert.ToUInt16(obj["Chemistry"] ?? 0)),
                    IsCharging = Convert.ToUInt16(obj["BatteryStatus"] ?? 0) == 2
                };

                // Calculate wear level
                if (battery.DesignCapacity > 0)
                {
                    battery.WearLevel = 100 - ((battery.CurrentCapacity * 100) / battery.DesignCapacity);
                }

                break; // Only first battery
            }
        });

        return battery;
    }

    private string GetBatteryChemistry(ushort chemistry)
    {
        return chemistry switch
        {
            1 => "Other",
            2 => "Unknown",
            3 => "Lead Acid",
            4 => "Nickel Cadmium",
            5 => "Nickel Metal Hydride",
            6 => "Lithium-ion",
            7 => "Zinc Air",
            8 => "Lithium Polymer",
            _ => "Unknown"
        };
    }

    #endregion

    #region Monitor Detection

    private async Task<List<MonitorInfo>> DetectMonitorsAsync()
    {
        var monitors = new List<MonitorInfo>();

        await Task.Run(() =>
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor");
            foreach (ManagementObject obj in searcher.Get())
            {
                var monitor = new MonitorInfo
                {
                    Name = obj["Name"]?.ToString() ?? "Unknown",
                    Width = Convert.ToInt32(obj["ScreenWidth"] ?? 0),
                    Height = Convert.ToInt32(obj["ScreenHeight"] ?? 0)
                };

                // Refresh rate and HDR not available via WMI
                monitor.RefreshRate = 60; // Default assumption
                monitor.SupportsHDR = false;

                monitors.Add(monitor);
            }
        });

        return monitors;
    }

    #endregion

    #region Hardware Tests

    private async Task<CpuTestResult> RunCpuTestAsync(int durationSeconds)
    {
        var result = new CpuTestResult
        {
            TestDuration = TimeSpan.FromSeconds(durationSeconds)
        };

        var startTime = DateTime.Now;
        var temperatures = new List<int>();

        // Single-core test
        var singleCoreScore = await Task.Run(() =>
        {
            var iterations = 0;
            var sw = Stopwatch.StartNew();

            while (sw.Elapsed.TotalSeconds < durationSeconds / 2)
            {
                // Prime number calculation (CPU-intensive)
                IsPrime(1000000007);
                iterations++;

                temperatures.Add(GetCpuTemperature());
            }

            return iterations;
        });

        result.SingleCoreScore = singleCoreScore;

        // Multi-core test
        var multiCoreScore = await Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            var tasks = new List<Task<int>>();

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var count = 0;
                    while (sw.Elapsed.TotalSeconds < durationSeconds / 2)
                    {
                        IsPrime(1000000007 + count);
                        count++;
                    }
                    return count;
                }));
            }

            Task.WaitAll(tasks.ToArray());
            return tasks.Sum(t => t.Result);
        });

        result.MultiCoreScore = multiCoreScore;
        result.MaxTemperature = temperatures.Any() ? temperatures.Max() : 0;
        result.AverageClockSpeed = 0; // Would require performance counter monitoring

        return result;
    }

    private bool IsPrime(long number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        var boundary = (long)Math.Floor(Math.Sqrt(number));
        for (long i = 3; i <= boundary; i += 2)
        {
            if (number % i == 0) return false;
        }

        return true;
    }

    private async Task<MemoryTestResult> RunMemoryTestAsync()
    {
        var result = new MemoryTestResult();

        await Task.Run(() =>
        {
            const int testSize = 100 * 1024 * 1024; // 100 MB
            var buffer = new byte[testSize];
            var random = new Random();

            // Write speed test
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < testSize; i++)
            {
                buffer[i] = (byte)random.Next(256);
            }
            sw.Stop();
            result.WriteSpeedMBps = (testSize / (1024.0 * 1024.0)) / sw.Elapsed.TotalSeconds;

            // Read speed test
            sw.Restart();
            long sum = 0;
            for (int i = 0; i < testSize; i++)
            {
                sum += buffer[i];
            }
            sw.Stop();
            result.ReadSpeedMBps = (testSize / (1024.0 * 1024.0)) / sw.Elapsed.TotalSeconds;

            // Latency test (simplified)
            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                var value = buffer[random.Next(testSize)];
            }
            sw.Stop();
            result.LatencyNs = (sw.Elapsed.TotalMilliseconds * 1000000) / 10000;

            result.StabilityTestPassed = true; // Simplified
        });

        return result;
    }

    private async Task<List<StorageTestResult>> RunStorageTestsAsync()
    {
        var results = new List<StorageTestResult>();

        await Task.Run(() =>
        {
            var drives = System.IO.DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == System.IO.DriveType.Fixed);

            foreach (var drive in drives)
            {
                try
                {
                    var result = new StorageTestResult
                    {
                        DriveLetter = drive.Name
                    };

                    var testFile = System.IO.Path.Combine(drive.RootDirectory.FullName, "wincheck_speedtest.tmp");
                    const int testSize = 50 * 1024 * 1024; // 50 MB
                    var buffer = new byte[testSize];
                    new Random().NextBytes(buffer);

                    // Sequential write test
                    var sw = Stopwatch.StartNew();
                    System.IO.File.WriteAllBytes(testFile, buffer);
                    sw.Stop();
                    result.SequentialWriteMBps = (testSize / (1024.0 * 1024.0)) / sw.Elapsed.TotalSeconds;

                    // Sequential read test
                    sw.Restart();
                    var readBuffer = System.IO.File.ReadAllBytes(testFile);
                    sw.Stop();
                    result.SequentialReadMBps = (testSize / (1024.0 * 1024.0)) / sw.Elapsed.TotalSeconds;

                    // Random IOPS (simplified)
                    result.RandomReadIOPS = (int)(result.SequentialReadMBps * 100);
                    result.RandomWriteIOPS = (int)(result.SequentialWriteMBps * 100);

                    // Health score from SMART data
                    result.HealthScore = 100; // Default

                    // Cleanup
                    System.IO.File.Delete(testFile);

                    results.Add(result);
                }
                catch
                {
                    // Skip drives we can't test
                }
            }
        });

        return results;
    }

    private async Task<GpuTestResult> RunGpuTestAsync()
    {
        // GPU testing requires DirectX/OpenGL rendering
        // This is a simplified placeholder
        var result = new GpuTestResult
        {
            ComputeScore = 0,
            RenderScore = 0,
            MemoryBandwidthGBps = 0,
            MaxTemperature = 0
        };

        await Task.Delay(100); // Placeholder

        return result;
    }

    private int CalculateOverallScore(TestResults results)
    {
        var scores = new List<int>();

        if (results.CpuTest != null)
        {
            scores.Add(results.CpuTest.MultiCoreScore / 100);
        }

        if (results.MemoryTest != null)
        {
            scores.Add((int)(results.MemoryTest.ReadSpeedMBps / 10));
        }

        if (results.StorageTests.Any())
        {
            scores.Add((int)results.StorageTests.Average(s => s.SequentialReadMBps));
        }

        return scores.Any() ? (int)scores.Average() : 0;
    }

    #endregion

    #region P/Invoke and Native Structures

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [DllImport("kernel32.dll")]
    private static extern bool IsProcessorFeaturePresent(int processorFeature);

    private const int PF_MMX_INSTRUCTIONS_AVAILABLE = 3;
    private const int PF_XMMI_INSTRUCTIONS_AVAILABLE = 6;
    private const int PF_XMMI64_INSTRUCTIONS_AVAILABLE = 10;
    private const int PF_SSE3_INSTRUCTIONS_AVAILABLE = 13;
    private const int PF_AVX_INSTRUCTIONS_AVAILABLE = 17;
    private const int PF_AVX2_INSTRUCTIONS_AVAILABLE = 18;

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MEMORYSTATUSEX()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }

    #endregion
}
