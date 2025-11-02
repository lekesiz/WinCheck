using System;
using System.Collections.Generic;

namespace WinCheck.Core.Models;

public class OSInfo
{
    public string Name { get; set; } = string.Empty; // Windows 11 Pro
    public string Version { get; set; } = string.Empty; // 23H2
    public int BuildNumber { get; set; } // 22631
    public string Edition { get; set; } = string.Empty; // Home, Pro, Enterprise, Education
    public string Architecture { get; set; } = string.Empty; // x64, x86, ARM64
    public DateTime InstallDate { get; set; }
    public bool IsActivated { get; set; }
    public string ProductKey { get; set; } = string.Empty;
    public int ServicePack { get; set; }
    public string SystemRoot { get; set; } = string.Empty; // C:\Windows
    public long UptimeSeconds { get; set; }
    public WindowsVersion WindowsVersion { get; set; }
    public bool IsInsiderBuild { get; set; }
    public string RegisteredOwner { get; set; } = string.Empty;
    public string ComputerName { get; set; } = string.Empty;
}

public enum WindowsVersion
{
    Unknown,
    Windows7,
    Windows8,
    Windows81,
    Windows10,
    Windows11
}

public class OSOptimization
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Performance, Privacy, Security, UI
    public WindowsVersion MinVersion { get; set; }
    public WindowsVersion MaxVersion { get; set; }
    public OptimizationImpact Impact { get; set; }
    public bool RequiresRestart { get; set; }
    public bool IsReversible { get; set; }
    public string WarningMessage { get; set; } = string.Empty;
    public List<OSOptimizationStep> Steps { get; set; } = new();
}

public enum OptimizationImpact
{
    Low,
    Medium,
    High
}

public class OSOptimizationStep
{
    public OSStepType Type { get; set; }
    public string Target { get; set; } = string.Empty; // Registry path, service name, etc.
    public string Value { get; set; } = string.Empty;
    public string BackupValue { get; set; } = string.Empty;
}

public enum OSStepType
{
    SetRegistryValue,
    DeleteRegistryValue,
    StopService,
    DisableService,
    EnableService,
    RunCommand,
    DeleteFile,
    SetGroupPolicy
}

public class UpdateInfo
{
    public bool UpdatesAvailable { get; set; }
    public List<WindowsUpdate> AvailableUpdates { get; set; } = new();
    public DateTime LastUpdateCheck { get; set; }
    public bool AutoUpdateEnabled { get; set; }
}

public class WindowsUpdate
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string KB { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public bool IsImportant { get; set; }
    public bool IsSecurityUpdate { get; set; }
}
