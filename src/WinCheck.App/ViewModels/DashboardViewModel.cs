using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;

namespace WinCheck.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IAISystemAnalyzer _aiAnalyzer;
    private readonly IProcessMonitorService _processMonitor;

    [ObservableProperty]
    private bool _isAnalyzing;

    [ObservableProperty]
    private bool _isOptimizing;

    [ObservableProperty]
    private string _statusMessage = "Ready to analyze";

    [ObservableProperty]
    private int _overallHealthScore = 0;

    [ObservableProperty]
    private int _hardwareScore = 0;

    [ObservableProperty]
    private int _softwareScore = 0;

    [ObservableProperty]
    private int _performanceScore = 0;

    [ObservableProperty]
    private int _securityScore = 0;

    [ObservableProperty]
    private double _cpuUsage = 0;

    [ObservableProperty]
    private double _memoryUsage = 0;

    [ObservableProperty]
    private double _diskUsage = 0;

    [ObservableProperty]
    private string _aiExplanation = "Click 'Quick Scan' or 'Deep Scan' to analyze your system";

    public ObservableCollection<string> Recommendations { get; } = new();

    public DashboardViewModel(IAISystemAnalyzer aiAnalyzer, IProcessMonitorService processMonitor)
    {
        _aiAnalyzer = aiAnalyzer;
        _processMonitor = processMonitor;

        // Load initial system status
        _ = LoadInitialStatusAsync();
    }

    private async Task LoadInitialStatusAsync()
    {
        try
        {
            // Small delay to ensure UI is ready
            await Task.Delay(100);

            // Get system resource usage
            var systemUsage = await _processMonitor.GetSystemResourceUsageAsync();
            CpuUsage = Math.Round(systemUsage.CpuUsagePercentage, 1);
            MemoryUsage = Math.Round(systemUsage.MemoryUsagePercentage, 1);
            DiskUsage = systemUsage.DiskUsages.Count > 0
                ? Math.Round(systemUsage.DiskUsages[0].UsagePercentage, 1)
                : 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard LoadInitialStatus error: {ex.Message}");

            // Fallback: Try to get basic metrics directly
            try
            {
                await Task.Run(() =>
                {
                    // Get CPU using PerformanceCounter
                    using var cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total");
                    cpuCounter.NextValue(); // First call returns 0
                    System.Threading.Thread.Sleep(500);
                    CpuUsage = Math.Round(cpuCounter.NextValue(), 1);
                });

                // Get Memory
                var gcMemory = GC.GetTotalMemory(false);
                var process = System.Diagnostics.Process.GetCurrentProcess();
                MemoryUsage = Math.Round((process.WorkingSet64 / 1024.0 / 1024.0), 1); // MB

                // Disk - leave at 0 for now
                DiskUsage = 0;
            }
            catch
            {
                // Final fallback - show placeholder
                CpuUsage = 0;
                MemoryUsage = 0;
                DiskUsage = 0;
            }
        }
    }

    [RelayCommand]
    private async Task QuickScanAsync()
    {
        try
        {
            IsAnalyzing = true;
            StatusMessage = "Running quick AI analysis...";
            Recommendations.Clear();

            // Get AI system explanation
            var explanation = await _aiAnalyzer.ExplainSystemStatusAsync();
            AiExplanation = explanation;

            // Run AI analysis
            var report = await _aiAnalyzer.AnalyzeSystemAsync();

            // Update scores
            OverallHealthScore = report.OverallHealthScore;
            HardwareScore = report.Hardware.HealthScore;
            SoftwareScore = report.Software.HealthScore;
            PerformanceScore = report.Performance.HealthScore;
            SecurityScore = report.Security.HealthScore;

            // Update resource usage
            CpuUsage = report.Performance.CpuUsageAverage;
            MemoryUsage = report.Performance.MemoryUsagePercentage;
            DiskUsage = report.Performance.DiskUsagePercentage;

            // Add recommendations
            foreach (var rec in report.Recommendations)
            {
                Recommendations.Add($"{rec.Priority}: {rec.Description}");
            }

            StatusMessage = $"Quick scan complete - Health: {OverallHealthScore}/100";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            AiExplanation = "Failed to analyze system. Please check AI provider settings.";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task DeepScanAsync()
    {
        try
        {
            IsAnalyzing = true;
            StatusMessage = "Running deep AI analysis...";
            Recommendations.Clear();

            // Run full AI analysis
            var report = await _aiAnalyzer.AnalyzeSystemAsync();

            // Update all metrics
            OverallHealthScore = report.OverallHealthScore;
            HardwareScore = report.Hardware.HealthScore;
            SoftwareScore = report.Software.HealthScore;
            PerformanceScore = report.Performance.HealthScore;
            SecurityScore = report.Security.HealthScore;

            CpuUsage = report.Performance.CpuUsageAverage;
            MemoryUsage = report.Performance.MemoryUsagePercentage;
            DiskUsage = report.Performance.DiskUsagePercentage;

            // Get detailed AI explanation
            var explanation = await _aiAnalyzer.ExplainSystemStatusAsync();
            AiExplanation = explanation;

            // Add all recommendations with details
            foreach (var rec in report.Recommendations)
            {
                Recommendations.Add($"[{rec.Priority}] {rec.Description} - Impact: {rec.EstimatedImpactScore}/100");
            }

            StatusMessage = $"Deep scan complete - Health: {OverallHealthScore}/100";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            AiExplanation = "Failed to analyze system. Please check AI provider settings.";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task OptimizeAsync()
    {
        try
        {
            IsOptimizing = true;
            StatusMessage = "Generating optimization plan...";

            // Generate AI optimization plan
            var plan = await _aiAnalyzer.GenerateOptimizationPlanAsync();

            StatusMessage = $"Executing {plan.Steps.Count} optimization steps...";

            // Execute optimization plan
            var result = await _aiAnalyzer.ExecuteOptimizationPlanAsync(plan);

            // Update UI with results
            OverallHealthScore += result.ActualHealthScoreIncrease;
            AiExplanation = result.AIPostAnalysis;

            Recommendations.Clear();
            Recommendations.Add($"Optimization complete!");
            Recommendations.Add($"Health score increased by: +{result.ActualHealthScoreIncrease}");
            Recommendations.Add($"Steps completed: {result.StepsCompleted}/{plan.Steps.Count}");
            if (result.StepsFailed > 0)
            {
                Recommendations.Add($"Steps failed: {result.StepsFailed}");
            }
            Recommendations.Add($"Duration: {result.Duration.TotalSeconds:F1} seconds");

            StatusMessage = "System optimized successfully!";

            // Rescan to get updated metrics
            await QuickScanAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            Recommendations.Clear();
            Recommendations.Add("Optimization failed. Please check AI provider settings.");
        }
        finally
        {
            IsOptimizing = false;
        }
    }

    [RelayCommand]
    private async Task AskAIQuestionAsync(string question)
    {
        try
        {
            StatusMessage = "Asking AI...";
            var answer = await _aiAnalyzer.AskQuestionAsync(question);
            AiExplanation = answer;
            StatusMessage = "AI answered your question";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            AiExplanation = "Failed to get AI response.";
        }
    }
}
