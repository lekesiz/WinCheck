using System;
using System.Collections.Generic;

namespace WinCheck.Core.Models;

public class StartupProgram
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public StartupLocation Location { get; set; }
    public bool IsEnabled { get; set; }
    public StartupImpact Impact { get; set; }
    public int EstimatedDelayMs { get; set; }
    public bool IsSigned { get; set; }
    public DateTime? LastModified { get; set; }
}

public enum StartupLocation
{
    RegistryRun,
    RegistryRunOnce,
    RegistryRun64,
    StartupFolder,
    TaskScheduler,
    Services
}

public enum StartupImpact
{
    None,
    Low,
    Medium,
    High,
    VeryHigh
}

public class StartupImpactAnalysis
{
    public int TotalStartupPrograms { get; set; }
    public int EnabledPrograms { get; set; }
    public int DisabledPrograms { get; set; }
    public int EstimatedBootTimeSeconds { get; set; }
    public int PotentialTimeSavingSeconds { get; set; }
    public List<StartupRecommendation> Recommendations { get; set; } = new();
}

public class StartupRecommendation
{
    public StartupProgram Program { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
    public bool RecommendDisable { get; set; }
    public int EstimatedTimeSavingMs { get; set; }
}
