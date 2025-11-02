using System.Collections.Generic;
using System.Threading.Tasks;
using WinCheck.Core.Models;

namespace WinCheck.Core.Interfaces;

public interface IHardwareDetectionService
{
    Task<HardwareProfile> DetectAllHardwareAsync();
    Task<TestResults> RunHardwareTestsAsync(HardwareTestOptions options);
    Task<HardwareHealth> GetHardwareHealthAsync();
}

public class HardwareProfile
{
    public CpuInfo Cpu { get; set; } = new();
    public MemoryInfo Memory { get; set; } = new();
    public List<StorageInfo> Storage { get; set; } = new();
    public List<GpuInfo> Gpus { get; set; } = new();
    public List<NetworkAdapterInfo> NetworkAdapters { get; set; } = new();
    public MotherboardInfo Motherboard { get; set; } = new();
    public BatteryInfo? Battery { get; set; }
    public List<MonitorInfo> Monitors { get; set; } = new();
}

public class HardwareTestOptions
{
    public bool TestCpu { get; set; } = true;
    public bool TestMemory { get; set; } = true;
    public bool TestStorage { get; set; } = true;
    public bool TestGpu { get; set; } = false;
    public int TestDurationSeconds { get; set; } = 60;
}

public class TestResults
{
    public CpuTestResult? CpuTest { get; set; }
    public MemoryTestResult? MemoryTest { get; set; }
    public List<StorageTestResult> StorageTests { get; set; } = new();
    public GpuTestResult? GpuTest { get; set; }
    public int OverallScore { get; set; }
}

public class HardwareHealth
{
    public int OverallHealthPercentage { get; set; }
    public List<HealthIssue> Issues { get; set; } = new();
    public Dictionary<string, int> ComponentHealth { get; set; } = new();
}
