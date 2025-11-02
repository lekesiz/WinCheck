using System;
using System.Collections.Generic;

namespace WinCheck.Core.Models;

public class CpuInfo
{
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public int NumberOfCores { get; set; }
    public int NumberOfLogicalProcessors { get; set; }
    public int MaxClockSpeed { get; set; } // MHz
    public int CurrentClockSpeed { get; set; } // MHz
    public int L2CacheSize { get; set; } // KB
    public int L3CacheSize { get; set; } // KB
    public string Architecture { get; set; } = string.Empty;
    public List<string> SupportedInstructions { get; set; } = new();
    public bool SupportsVirtualization { get; set; }
    public int CurrentTemperature { get; set; } // Celsius
}

public class MemoryInfo
{
    public long TotalBytes { get; set; }
    public long AvailableBytes { get; set; }
    public double TotalGB => TotalBytes / 1024.0 / 1024.0 / 1024.0;
    public string Type { get; set; } = "Unknown"; // DDR3, DDR4, DDR5
    public int SpeedMHz { get; set; }
    public int SlotsUsed { get; set; }
    public int SlotsTotal { get; set; }
    public bool IsDualChannel { get; set; }
}

public class StorageInfo
{
    public string Model { get; set; } = string.Empty;
    public string InterfaceType { get; set; } = string.Empty;
    public long Size { get; set; }
    public long CapacityGB => Size / 1024 / 1024 / 1024;
    public string MediaType { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public bool IsSSD { get; set; }
    public SmartData SmartData { get; set; } = new();
}

public class SmartData
{
    public int Temperature { get; set; }
    public long PowerOnHours { get; set; }
    public int ReallocatedSectors { get; set; }
    public int HealthPercentage { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class GpuInfo
{
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public long VRamBytes { get; set; }
    public double VRamGB => VRamBytes / 1024.0 / 1024.0 / 1024.0;
    public string DriverVersion { get; set; } = string.Empty;
    public string DirectXVersion { get; set; } = string.Empty;
    public int CurrentTemperature { get; set; }
}

public class NetworkAdapterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Ethernet, WiFi
    public string MACAddress { get; set; } = string.Empty;
    public long SpeedBits { get; set; }
    public string DriverVersion { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
}

public class MotherboardInfo
{
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string BIOSVersion { get; set; } = string.Empty;
    public string Chipset { get; set; } = string.Empty;
}

public class BatteryInfo
{
    public int DesignCapacity { get; set; } // mWh
    public int CurrentCapacity { get; set; } // mWh
    public int ChargePercentage { get; set; }
    public int ChargeCycles { get; set; }
    public int WearLevel { get; set; } // 0-100
    public string ChemistryType { get; set; } = string.Empty;
    public bool IsCharging { get; set; }
}

public class MonitorInfo
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int RefreshRate { get; set; }
    public bool SupportsHDR { get; set; }
}

public class CpuTestResult
{
    public int SingleCoreScore { get; set; }
    public int MultiCoreScore { get; set; }
    public int MaxTemperature { get; set; }
    public double AverageClockSpeed { get; set; }
    public TimeSpan TestDuration { get; set; }
}

public class MemoryTestResult
{
    public double ReadSpeedMBps { get; set; }
    public double WriteSpeedMBps { get; set; }
    public double LatencyNs { get; set; }
    public bool StabilityTestPassed { get; set; }
}

public class StorageTestResult
{
    public string DriveLetter { get; set; } = string.Empty;
    public double SequentialReadMBps { get; set; }
    public double SequentialWriteMBps { get; set; }
    public int RandomReadIOPS { get; set; }
    public int RandomWriteIOPS { get; set; }
    public int HealthScore { get; set; }
}

public class GpuTestResult
{
    public int ComputeScore { get; set; }
    public int RenderScore { get; set; }
    public double MemoryBandwidthGBps { get; set; }
    public int MaxTemperature { get; set; }
}

public class HealthIssue
{
    public string Component { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string Recommendation { get; set; } = string.Empty;
}
