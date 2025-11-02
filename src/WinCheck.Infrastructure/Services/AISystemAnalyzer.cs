using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;
using WinCheck.Core.Services.AI;

namespace WinCheck.Infrastructure.Services;

public class AISystemAnalyzer : IAISystemAnalyzer
{
    private readonly IAIProvider _aiProvider;
    private readonly IHardwareDetectionService _hardwareService;
    private readonly IProcessMonitorService _processService;
    private readonly INetworkMonitorService _networkService;
    private readonly IOSDetectionService _osService;
    private readonly IServiceOptimizerService _serviceOptimizer;
    private readonly IDiskCleanupService _diskCleanup;
    private readonly IRegistryCleanerService _registryCleaner;
    private readonly IStartupManagerService _startupManager;

    public AISystemAnalyzer(
        IAIProvider aiProvider,
        IHardwareDetectionService hardwareService,
        IProcessMonitorService processService,
        INetworkMonitorService networkService,
        IOSDetectionService osService,
        IServiceOptimizerService serviceOptimizer,
        IDiskCleanupService diskCleanup,
        IRegistryCleanerService registryCleaner,
        IStartupManagerService startupManager)
    {
        _aiProvider = aiProvider;
        _hardwareService = hardwareService;
        _processService = processService;
        _networkService = networkService;
        _osService = osService;
        _serviceOptimizer = serviceOptimizer;
        _diskCleanup = diskCleanup;
        _registryCleaner = registryCleaner;
        _startupManager = startupManager;
    }

    public async Task<SystemAnalysisReport> AnalyzeSystemAsync()
    {
        var report = new SystemAnalysisReport
        {
            AnalysisDate = DateTime.Now
        };

        // Gather data from all services in parallel
        var hardwareTask = _hardwareService.DetectAllHardwareAsync();
        var hardwareHealthTask = _hardwareService.GetHardwareHealthAsync();
        var osTask = _osService.DetectOSAsync();
        var resourcesTask = _processService.GetSystemResourceUsageAsync();
        var suspiciousProcessesTask = _processService.DetectSuspiciousProcessesAsync();
        var networkConnectionsTask = _networkService.GetActiveConnectionsAsync();
        var diskAnalysisTask = _diskCleanup.AnalyzeDiskAsync();
        var registryScanTask = _registryCleaner.ScanRegistryAsync();
        var startupProgramsTask = _startupManager.GetStartupProgramsAsync();
        var startupImpactTask = _startupManager.AnalyzeBootImpactAsync();
        var optimizableServicesTask = _serviceOptimizer.GetOptimizableServicesAsync();

        await Task.WhenAll(
            hardwareTask, hardwareHealthTask, osTask, resourcesTask, suspiciousProcessesTask,
            networkConnectionsTask, diskAnalysisTask, registryScanTask,
            startupProgramsTask, startupImpactTask, optimizableServicesTask
        );

        var hardware = await hardwareTask;
        var hardwareHealth = await hardwareHealthTask;
        var os = await osTask;
        var resources = await resourcesTask;
        var suspiciousProcesses = await suspiciousProcessesTask;
        var networkConnections = await networkConnectionsTask;
        var diskAnalysis = await diskAnalysisTask;
        var registryScan = await registryScanTask;
        var startupPrograms = await startupProgramsTask;
        var startupImpact = await startupImpactTask;
        var optimizableServices = await optimizableServicesTask;

        // Analyze hardware
        report.Hardware.HealthScore = hardwareHealth.OverallHealthPercentage;
        report.Hardware.Issues.AddRange(hardwareHealth.Issues.Select(i => i.Issue));

        if (hardware.Cpu.NumberOfCores >= 8)
            report.Hardware.Strengths.Add("Excellent CPU with " + hardware.Cpu.NumberOfCores + " cores");
        if (hardware.Memory.TotalGB >= 16)
            report.Hardware.Strengths.Add("Ample RAM: " + hardware.Memory.TotalGB.ToString("F1") + " GB");
        if (hardware.Storage.Any(s => s.IsSSD))
            report.Hardware.Strengths.Add("Fast SSD storage detected");

        // Analyze software
        report.Software.HealthScore = CalculateSoftwareHealthScore(
            registryScan.TotalIssuesFound,
            startupPrograms.Count(p => p.IsEnabled),
            optimizableServices.Count
        );
        report.Software.StartupProgramCount = startupPrograms.Count(p => p.IsEnabled);
        report.Software.OptimizableServicesCount = optimizableServices.Count;
        report.Software.CleanableSpaceBytes = diskAnalysis.TotalCleanableBytes;
        report.Software.RegistryIssuesCount = registryScan.TotalIssuesFound;

        // Analyze performance
        report.Performance.HealthScore = CalculatePerformanceHealthScore(
            resources.CpuUsagePercentage,
            resources.MemoryUsagePercentage,
            diskAnalysis.UsagePercentage
        );
        report.Performance.CpuUsageAverage = resources.CpuUsagePercentage;
        report.Performance.MemoryUsagePercentage = resources.MemoryUsagePercentage;
        report.Performance.DiskUsagePercentage = diskAnalysis.UsagePercentage;

        if (resources.CpuUsagePercentage > 80)
            report.Performance.Bottlenecks.Add("High CPU usage detected");
        if (resources.MemoryUsagePercentage > 85)
            report.Performance.Bottlenecks.Add("High memory usage - consider adding more RAM");
        if (diskAnalysis.UsagePercentage > 90)
            report.Performance.Bottlenecks.Add("Disk almost full - cleanup recommended");

        report.Performance.BottleneckCount = report.Performance.Bottlenecks.Count;

        // Analyze security
        var networkThreats = 0;
        foreach (var conn in networkConnections)
        {
            var threat = await _networkService.AnalyzeConnectionAsync(conn);
            if (threat.Level >= ThreatLevel.Medium)
                networkThreats++;
        }

        report.Security.HealthScore = CalculateSecurityHealthScore(
            suspiciousProcesses.Count(),
            networkThreats
        );
        report.Security.SuspiciousProcessCount = suspiciousProcesses.Count();
        report.Security.NetworkThreatsCount = networkThreats;
        report.Security.ThreatCount = report.Security.SuspiciousProcessCount + report.Security.NetworkThreatsCount;

        foreach (var proc in suspiciousProcesses)
        {
            report.Security.SecurityIssues.Add($"Suspicious process: {proc.Metrics.Name} ({proc.Level})");
        }

        // Calculate overall health score
        report.OverallHealthScore = (
            report.Hardware.HealthScore +
            report.Software.HealthScore +
            report.Performance.HealthScore +
            report.Security.HealthScore
        ) / 4;

        // Generate AI insights if provider is available
        if (_aiProvider != null && _aiProvider.IsConfigured)
        {
            await GenerateAIInsights(report, hardware, os, resources);
        }

        // Generate recommendations
        report.Recommendations = GenerateRecommendations(
            report, hardwareHealth, diskAnalysis, registryScan,
            startupImpact, optimizableServices, suspiciousProcesses.ToList()
        );

        // Generate summary
        report.Summary = GenerateSummary(report);

        return report;
    }

    public async Task<OptimizationPlan> GenerateOptimizationPlanAsync()
    {
        var report = await AnalyzeSystemAsync();

        var plan = new OptimizationPlan
        {
            CreatedDate = DateTime.Now
        };

        // Add optimization steps based on analysis
        var diskAnalysis = await _diskCleanup.AnalyzeDiskAsync();
        if (diskAnalysis.TotalCleanableBytes > 100 * 1024 * 1024) // > 100 MB
        {
            plan.Steps.Add(new OptimizationStep
            {
                Title = "Clean Disk",
                Description = $"Remove {FormatBytes(diskAnalysis.TotalCleanableBytes)} of temporary files and caches",
                Type = OptimizationStepType.CleanDisk,
                RequiresUserConfirmation = false,
                RequiresRestart = false
            });
            plan.EstimatedSpaceSavingBytes += diskAnalysis.TotalCleanableBytes;
        }

        var registryScan = await _registryCleaner.ScanRegistryAsync();
        if (registryScan.TotalIssuesFound > 0)
        {
            plan.Steps.Add(new OptimizationStep
            {
                Title = "Fix Registry Issues",
                Description = $"Fix {registryScan.TotalIssuesFound} registry issues",
                Type = OptimizationStepType.FixRegistry,
                RequiresUserConfirmation = true,
                RequiresRestart = false
            });
        }

        var startupImpact = await _startupManager.AnalyzeBootImpactAsync();
        if (startupImpact.Recommendations.Any())
        {
            foreach (var rec in startupImpact.Recommendations.Take(5))
            {
                plan.Steps.Add(new OptimizationStep
                {
                    Title = $"Disable Startup: {rec.Program.Name}",
                    Description = rec.Reason,
                    Type = OptimizationStepType.DisableStartup,
                    Parameters = new Dictionary<string, object> { ["ProgramId"] = rec.Program.Id },
                    RequiresUserConfirmation = true,
                    RequiresRestart = false
                });
                plan.EstimatedTimeSavingMs += rec.EstimatedTimeSavingMs;
            }
        }

        var optimizableServices = await _serviceOptimizer.GetOptimizableServicesAsync();
        foreach (var service in optimizableServices.Where(s => s.Safety == SafetyLevel.Safe).Take(5))
        {
            plan.Steps.Add(new OptimizationStep
            {
                Title = $"Optimize Service: {service.DisplayName}",
                Description = service.Reason,
                Type = OptimizationStepType.DisableService,
                Parameters = new Dictionary<string, object> { ["ServiceName"] = service.ServiceName },
                RequiresUserConfirmation = false,
                RequiresRestart = service.RequiresRestart
            });
            plan.EstimatedTimeSavingMs += service.EstimatedBootTimeSavingMs;
        }

        var osOptimizations = await _osService.GetRecommendedOptimizationsAsync();
        foreach (var opt in osOptimizations.Where(o => o.Impact != OptimizationImpact.High).Take(5))
        {
            plan.Steps.Add(new OptimizationStep
            {
                Title = opt.Name,
                Description = opt.Description,
                Type = OptimizationStepType.ApplyOSOptimization,
                Parameters = new Dictionary<string, object> { ["OptimizationId"] = opt.Id },
                RequiresUserConfirmation = opt.Impact == OptimizationImpact.High,
                RequiresRestart = opt.RequiresRestart
            });
        }

        // Calculate expected health score increase
        plan.ExpectedHealthScoreIncrease = Math.Min(30, plan.Steps.Count * 3);

        // Generate AI rationale if available
        if (_aiProvider != null && _aiProvider.IsConfigured)
        {
            plan.AIRationale = await GenerateOptimizationRationale(plan, report);
        }

        return plan;
    }

    public async Task<OptimizationResult> ExecuteOptimizationPlanAsync(OptimizationPlan plan)
    {
        var result = new OptimizationResult();
        var sw = Stopwatch.StartNew();

        var beforeScore = (await AnalyzeSystemAsync()).OverallHealthScore;

        foreach (var step in plan.Steps)
        {
            try
            {
                var success = await ExecuteOptimizationStep(step);

                if (success)
                {
                    result.StepsCompleted++;
                }
                else
                {
                    result.StepsFailed++;
                    result.Errors.Add($"Failed to execute: {step.Title}");
                }
            }
            catch (Exception ex)
            {
                result.StepsFailed++;
                result.Errors.Add($"Error executing {step.Title}: {ex.Message}");
            }
        }

        var afterScore = (await AnalyzeSystemAsync()).OverallHealthScore;
        result.ActualHealthScoreIncrease = afterScore - beforeScore;

        sw.Stop();
        result.Duration = sw.Elapsed;
        result.Success = result.StepsFailed == 0;

        // Generate AI post-analysis
        if (_aiProvider != null && _aiProvider.IsConfigured)
        {
            result.AIPostAnalysis = await GeneratePostOptimizationAnalysis(result, beforeScore, afterScore);
        }

        return result;
    }

    public async Task<string> ExplainSystemStatusAsync()
    {
        var report = await AnalyzeSystemAsync();

        if (_aiProvider == null || !_aiProvider.IsConfigured)
        {
            return GenerateBasicStatusExplanation(report);
        }

        var prompt = $@"Provide a friendly, concise explanation of this Windows system's status for a non-technical user:

Overall Health Score: {report.OverallHealthScore}/100

Hardware:
- Health: {report.Hardware.HealthScore}/100
- Issues: {string.Join(", ", report.Hardware.Issues)}
- Strengths: {string.Join(", ", report.Hardware.Strengths)}

Performance:
- CPU Usage: {report.Performance.CpuUsageAverage:F1}%
- Memory Usage: {report.Performance.MemoryUsagePercentage:F1}%
- Disk Usage: {report.Performance.DiskUsagePercentage:F1}%
- Bottlenecks: {report.Performance.BottleneckCount}

Software:
- Startup Programs: {report.Software.StartupProgramCount}
- Cleanable Space: {FormatBytes(report.Software.CleanableSpaceBytes)}
- Registry Issues: {report.Software.RegistryIssuesCount}

Security:
- Threats: {report.Security.ThreatCount}
- Suspicious Processes: {report.Security.SuspiciousProcessCount}

Provide a 2-3 paragraph explanation that:
1. Summarizes the overall status
2. Highlights any concerns
3. Gives encouraging advice";

        return await _aiProvider.CompleteAsync(prompt, new AICompletionOptions
        {
            MaxTokens = 500,
            Temperature = 0.7
        });
    }

    public async Task<string> AskQuestionAsync(string question)
    {
        var report = await AnalyzeSystemAsync();

        if (_aiProvider == null || !_aiProvider.IsConfigured)
        {
            return "AI provider not configured. Please set up an AI API key in settings.";
        }

        var context = $@"System Context:
OS: Windows {report.OverallHealthScore}/100 health
Hardware Score: {report.Hardware.HealthScore}/100
Performance Score: {report.Performance.HealthScore}/100
Security Score: {report.Security.HealthScore}/100

User Question: {question}

Provide a helpful, accurate answer based on the system context. Be concise and actionable.";

        return await _aiProvider.CompleteAsync(context, new AICompletionOptions
        {
            MaxTokens = 300,
            Temperature = 0.5
        });
    }

    #region Private Helper Methods

    private int CalculateSoftwareHealthScore(int registryIssues, int startupCount, int optimizableServices)
    {
        var score = 100;

        // Deduct for registry issues
        score -= Math.Min(30, registryIssues / 10);

        // Deduct for excessive startup programs
        if (startupCount > 10) score -= Math.Min(20, (startupCount - 10) * 2);

        // Deduct for unoptimized services
        score -= Math.Min(20, optimizableServices / 2);

        return Math.Max(0, score);
    }

    private int CalculatePerformanceHealthScore(double cpuUsage, double memUsage, double diskUsage)
    {
        var score = 100;

        if (cpuUsage > 80) score -= 30;
        else if (cpuUsage > 60) score -= 15;

        if (memUsage > 90) score -= 30;
        else if (memUsage > 75) score -= 15;

        if (diskUsage > 95) score -= 30;
        else if (diskUsage > 85) score -= 15;

        return Math.Max(0, score);
    }

    private int CalculateSecurityHealthScore(int suspiciousProcesses, int networkThreats)
    {
        var score = 100;

        score -= Math.Min(50, suspiciousProcesses * 10);
        score -= Math.Min(40, networkThreats * 15);

        return Math.Max(0, score);
    }

    private async Task GenerateAIInsights(SystemAnalysisReport report, HardwareProfile hardware, OSInfo os, SystemResourceUsage resources)
    {
        var prompt = $@"Analyze this Windows system and provide expert insights:

OS: {os.Name} {os.Version} (Build {os.BuildNumber})
CPU: {hardware.Cpu.Name} ({hardware.Cpu.NumberOfCores} cores @ {hardware.Cpu.MaxClockSpeed} MHz)
RAM: {hardware.Memory.TotalGB:F1} GB {hardware.Memory.Type}
Storage: {string.Join(", ", hardware.Storage.Select(s => $"{s.CapacityGB}GB {(s.IsSSD ? "SSD" : "HDD")}"))}

Current State:
- CPU Usage: {resources.CpuUsagePercentage:F1}%
- Memory Usage: {resources.MemoryUsagePercentage:F1}%
- Active Processes: {resources.ActiveProcessCount}

Health Scores:
- Hardware: {report.Hardware.HealthScore}/100
- Software: {report.Software.HealthScore}/100
- Performance: {report.Performance.HealthScore}/100
- Security: {report.Security.HealthScore}/100

Provide 2-3 key insights about this system's health and performance.";

        report.AIInsights = await _aiProvider.CompleteAsync(prompt, new AICompletionOptions
        {
            MaxTokens = 300,
            Temperature = 0.6
        });

        report.Hardware.AIAssessment = $"Hardware health: {report.Hardware.HealthScore}/100";
        report.Software.AIAssessment = $"Software optimization: {report.Software.HealthScore}/100";
        report.Performance.AIAssessment = $"Performance score: {report.Performance.HealthScore}/100";
        report.Security.AIAssessment = $"Security level: {report.Security.HealthScore}/100";
    }

    private List<AIRecommendation> GenerateRecommendations(
        SystemAnalysisReport report,
        HardwareHealth hardwareHealth,
        DiskAnalysis diskAnalysis,
        RegistryScanResult registryScan,
        StartupImpactAnalysis startupImpact,
        List<ServiceOptimization> optimizableServices,
        List<SuspiciousProcess> suspiciousProcesses)
    {
        var recommendations = new List<AIRecommendation>();

        // Disk cleanup recommendation
        if (diskAnalysis.TotalCleanableBytes > 500 * 1024 * 1024)
        {
            recommendations.Add(new AIRecommendation
            {
                Title = "Clean Up Disk Space",
                Description = $"Free up {FormatBytes(diskAnalysis.TotalCleanableBytes)} by removing temporary files and caches",
                Priority = diskAnalysis.UsagePercentage > 90 ? RecommendationPriority.High : RecommendationPriority.Medium,
                Category = RecommendationCategory.Storage,
                EstimatedImpactScore = 70,
                ActionRequired = "Run disk cleanup",
                AutomationAvailable = true
            });
        }

        // Startup optimization
        if (startupImpact.PotentialTimeSavingSeconds > 5)
        {
            recommendations.Add(new AIRecommendation
            {
                Title = "Optimize Startup Programs",
                Description = $"Improve boot time by ~{startupImpact.PotentialTimeSavingSeconds} seconds by disabling unnecessary startup programs",
                Priority = RecommendationPriority.Medium,
                Category = RecommendationCategory.Startup,
                EstimatedImpactScore = 60,
                ActionRequired = "Review and disable startup programs",
                AutomationAvailable = true
            });
        }

        // Service optimization
        if (optimizableServices.Count > 5)
        {
            recommendations.Add(new AIRecommendation
            {
                Title = "Optimize Windows Services",
                Description = $"Disable {optimizableServices.Count} unnecessary services to improve performance",
                Priority = RecommendationPriority.Medium,
                Category = RecommendationCategory.Services,
                EstimatedImpactScore = 50,
                ActionRequired = "Disable recommended services",
                AutomationAvailable = true
            });
        }

        // Registry cleanup
        if (registryScan.TotalIssuesFound > 10)
        {
            recommendations.Add(new AIRecommendation
            {
                Title = "Fix Registry Issues",
                Description = $"Clean up {registryScan.TotalIssuesFound} registry issues",
                Priority = RecommendationPriority.Low,
                Category = RecommendationCategory.Registry,
                EstimatedImpactScore = 30,
                ActionRequired = "Run registry cleaner",
                AutomationAvailable = true
            });
        }

        // Security recommendations
        if (suspiciousProcesses.Any())
        {
            recommendations.Add(new AIRecommendation
            {
                Title = "Review Suspicious Processes",
                Description = $"Found {suspiciousProcesses.Count} suspicious processes that may need attention",
                Priority = RecommendationPriority.High,
                Category = RecommendationCategory.Security,
                EstimatedImpactScore = 80,
                ActionRequired = "Review and terminate if necessary",
                AutomationAvailable = false
            });
        }

        // Hardware recommendations
        foreach (var issue in hardwareHealth.Issues.Take(3))
        {
            recommendations.Add(new AIRecommendation
            {
                Title = issue.Issue,
                Description = issue.Recommendation,
                Priority = issue.Severity == "Critical" ? RecommendationPriority.Critical : RecommendationPriority.High,
                Category = RecommendationCategory.Hardware,
                EstimatedImpactScore = issue.Severity == "Critical" ? 90 : 70,
                ActionRequired = issue.Recommendation,
                AutomationAvailable = false
            });
        }

        return recommendations.OrderByDescending(r => r.Priority).ToList();
    }

    private string GenerateSummary(SystemAnalysisReport report)
    {
        var sb = new StringBuilder();

        if (report.OverallHealthScore >= 80)
        {
            sb.Append("Your system is in good health. ");
        }
        else if (report.OverallHealthScore >= 60)
        {
            sb.Append("Your system needs some optimization. ");
        }
        else
        {
            sb.Append("Your system requires immediate attention. ");
        }

        if (report.Performance.BottleneckCount > 0)
        {
            sb.Append($"{report.Performance.BottleneckCount} performance bottleneck(s) detected. ");
        }

        if (report.Security.ThreatCount > 0)
        {
            sb.Append($"{report.Security.ThreatCount} security issue(s) found. ");
        }

        if (report.Software.CleanableSpaceBytes > 1024 * 1024 * 1024)
        {
            sb.Append($"{FormatBytes(report.Software.CleanableSpaceBytes)} can be freed. ");
        }

        return sb.ToString().Trim();
    }

    private string GenerateBasicStatusExplanation(SystemAnalysisReport report)
    {
        return $@"Your system health score is {report.OverallHealthScore}/100.

{report.Summary}

Key areas:
- Hardware: {report.Hardware.HealthScore}/100
- Performance: {report.Performance.HealthScore}/100
- Software: {report.Software.HealthScore}/100
- Security: {report.Security.HealthScore}/100

{(report.Recommendations.Any() ? $"Top recommendation: {report.Recommendations.First().Title}" : "No immediate actions required.")}";
    }

    private async Task<string> GenerateOptimizationRationale(OptimizationPlan plan, SystemAnalysisReport report)
    {
        var prompt = $@"Explain why this optimization plan will improve the system:

Current Health Score: {report.OverallHealthScore}/100
Optimization Steps: {plan.Steps.Count}
Expected Improvement: +{plan.ExpectedHealthScoreIncrease} points
Space Savings: {FormatBytes(plan.EstimatedSpaceSavingBytes)}
Time Savings: {plan.EstimatedTimeSavingMs / 1000}s boot time

Provide a brief rationale (2-3 sentences).";

        return await _aiProvider.CompleteAsync(prompt, new AICompletionOptions
        {
            MaxTokens = 150,
            Temperature = 0.6
        });
    }

    private async Task<string> GeneratePostOptimizationAnalysis(OptimizationResult result, int beforeScore, int afterScore)
    {
        var prompt = $@"Analyze the optimization results:

Before: {beforeScore}/100
After: {afterScore}/100
Actual Improvement: +{result.ActualHealthScoreIncrease}
Steps Completed: {result.StepsCompleted}
Steps Failed: {result.StepsFailed}
Duration: {result.Duration.TotalSeconds:F1}s

Provide a brief analysis (2-3 sentences).";

        return await _aiProvider.CompleteAsync(prompt, new AICompletionOptions
        {
            MaxTokens = 150,
            Temperature = 0.6
        });
    }

    private async Task<bool> ExecuteOptimizationStep(OptimizationStep step)
    {
        switch (step.Type)
        {
            case OptimizationStepType.CleanDisk:
                var cleanResult = await _diskCleanup.RunFullCleanupAsync(new CleanupOptions());
                return cleanResult.Success;

            case OptimizationStepType.FixRegistry:
                var scanResult = await _registryCleaner.ScanRegistryAsync();
                var fixResult = await _registryCleaner.CleanRegistryAsync(scanResult.Issues);
                return fixResult.Success;

            case OptimizationStepType.DisableStartup:
                if (step.Parameters.TryGetValue("ProgramId", out var programIdObj))
                {
                    var programs = await _startupManager.GetStartupProgramsAsync();
                    var program = programs.FirstOrDefault(p => p.Id == programIdObj.ToString());
                    if (program != null)
                    {
                        return await _startupManager.SetStartupStateAsync(program, false);
                    }
                }
                return false;

            case OptimizationStepType.DisableService:
                if (step.Parameters.TryGetValue("ServiceName", out var serviceNameObj))
                {
                    var services = await _serviceOptimizer.GetOptimizableServicesAsync();
                    var service = services.FirstOrDefault(s => s.ServiceName == serviceNameObj.ToString());
                    if (service != null)
                    {
                        return await _serviceOptimizer.OptimizeServiceAsync(service);
                    }
                }
                return false;

            case OptimizationStepType.ApplyOSOptimization:
                if (step.Parameters.TryGetValue("OptimizationId", out var optIdObj))
                {
                    var optimizations = await _osService.GetRecommendedOptimizationsAsync();
                    var opt = optimizations.FirstOrDefault(o => o.Id == optIdObj.ToString());
                    if (opt != null)
                    {
                        return await _osService.ApplyOptimizationAsync(opt);
                    }
                }
                return false;

            default:
                return false;
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    #endregion
}
