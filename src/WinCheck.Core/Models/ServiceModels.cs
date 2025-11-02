using System;
using System.Collections.Generic;

namespace WinCheck.Core.Models;

public class WindowsServiceInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceStatus Status { get; set; }
    public WinCheckServiceStartMode StartMode { get; set; }
    public string ExecutablePath { get; set; } = string.Empty;
    public long MemoryUsageBytes { get; set; }
    public int ProcessId { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public List<string> DependentServices { get; set; } = new();
    public bool CanBeStopped { get; set; }
    public bool CanBePaused { get; set; }
}

public enum ServiceStatus
{
    Stopped,
    StartPending,
    StopPending,
    Running,
    ContinuePending,
    PausePending,
    Paused,
    Unknown
}

public enum WinCheckServiceStartMode
{
    Automatic,
    AutomaticDelayed,
    Manual,
    Disabled,
    Boot,
    System
}

public class ServiceOptimization
{
    public string ServiceName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public WinCheckServiceStartMode CurrentStartMode { get; set; }
    public WinCheckServiceStartMode RecommendedStartMode { get; set; }
    public SafetyLevel Safety { get; set; }
    public long EstimatedMemorySavingBytes { get; set; }
    public int EstimatedBootTimeSavingMs { get; set; }
    public bool RequiresRestart { get; set; }
    public WinCheckServiceStartMode BackupStartMode { get; set; }
}

public enum SafetyLevel
{
    Safe,           // 100% safe to disable
    MostlySafe,     // Safe for most users
    Conditional,    // Safe depending on usage
    Risky,          // May cause issues
    DoNotDisable    // Critical system service
}

public class ServiceBackup
{
    public DateTime BackupDate { get; set; }
    public List<ServiceBackupEntry> Services { get; set; } = new();
}

public class ServiceBackupEntry
{
    public string ServiceName { get; set; } = string.Empty;
    public WinCheckServiceStartMode StartMode { get; set; }
    public ServiceStatus Status { get; set; }
}
