using System;
using System.Collections.Generic;

namespace WinCheck.Core.Models;

public class SystemAnalysisReport
{
    public DateTime AnalysisDate { get; set; }
    public int OverallHealthScore { get; set; } // 0-100
    public string Summary { get; set; } = string.Empty;
    public HardwareAnalysis Hardware { get; set; } = new();
    public SoftwareAnalysis Software { get; set; } = new();
    public PerformanceAnalysis Performance { get; set; } = new();
    public SecurityAnalysis Security { get; set; } = new();
    public List<AIRecommendation> Recommendations { get; set; } = new();
    public string AIInsights { get; set; } = string.Empty;
}

public class HardwareAnalysis
{
    public int HealthScore { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public string AIAssessment { get; set; } = string.Empty;
}

public class SoftwareAnalysis
{
    public int HealthScore { get; set; }
    public int StartupProgramCount { get; set; }
    public int OptimizableServicesCount { get; set; }
    public long CleanableSpaceBytes { get; set; }
    public int RegistryIssuesCount { get; set; }
    public string AIAssessment { get; set; } = string.Empty;
}

public class PerformanceAnalysis
{
    public int HealthScore { get; set; }
    public double CpuUsageAverage { get; set; }
    public double MemoryUsagePercentage { get; set; }
    public double DiskUsagePercentage { get; set; }
    public int BottleneckCount { get; set; }
    public List<string> Bottlenecks { get; set; } = new();
    public string AIAssessment { get; set; } = string.Empty;
}

public class SecurityAnalysis
{
    public int HealthScore { get; set; }
    public int ThreatCount { get; set; }
    public int SuspiciousProcessCount { get; set; }
    public int NetworkThreatsCount { get; set; }
    public List<string> SecurityIssues { get; set; } = new();
    public string AIAssessment { get; set; } = string.Empty;
}

public class AIRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationPriority Priority { get; set; }
    public RecommendationCategory Category { get; set; }
    public int EstimatedImpactScore { get; set; } // 0-100
    public string ActionRequired { get; set; } = string.Empty;
    public bool AutomationAvailable { get; set; }
}

public enum RecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum RecommendationCategory
{
    Hardware,
    Performance,
    Security,
    Storage,
    Startup,
    Services,
    Registry,
    Network
}

public class OptimizationPlan
{
    public string PlanId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedDate { get; set; }
    public List<OptimizationStep> Steps { get; set; } = new();
    public int EstimatedTimeSavingMs { get; set; }
    public long EstimatedSpaceSavingBytes { get; set; }
    public int ExpectedHealthScoreIncrease { get; set; }
    public string AIRationale { get; set; } = string.Empty;
}

public class OptimizationStep
{
    public string StepId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OptimizationStepType Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool RequiresUserConfirmation { get; set; }
    public bool RequiresRestart { get; set; }
}

public enum OptimizationStepType
{
    DisableService,
    CleanDisk,
    FixRegistry,
    DisableStartup,
    ApplyOSOptimization,
    TerminateProcess,
    BlockNetwork,
    UpdateDriver
}

public class OptimizationResult
{
    public bool Success { get; set; }
    public int StepsCompleted { get; set; }
    public int StepsFailed { get; set; }
    public List<string> Errors { get; set; } = new();
    public int ActualHealthScoreIncrease { get; set; }
    public TimeSpan Duration { get; set; }
    public string AIPostAnalysis { get; set; } = string.Empty;
}
