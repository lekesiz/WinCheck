using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.Infrastructure.Services;

public class ProcessMonitorService : IProcessMonitorService
{
    private readonly Dictionary<int, ProcessPerformanceData> _performanceCache = new();

    public IObservable<ProcessMetrics> MonitorProcesses()
    {
        return Observable.Interval(TimeSpan.FromSeconds(1))
            .SelectMany(async _ => await GetAllProcessMetricsAsync())
            .SelectMany(metrics => metrics);
    }

    public async Task<IEnumerable<SuspiciousProcess>> DetectSuspiciousProcessesAsync()
    {
        var allProcesses = await GetAllProcessMetricsAsync();
        var suspicious = new List<SuspiciousProcess>();

        foreach (var process in allProcesses)
        {
            var suspicion = AnalyzeProcess(process);
            if (suspicion != null)
            {
                suspicious.Add(suspicion);
            }
        }

        return suspicious;
    }

    public async Task<ProcessMetrics?> GetProcessMetricsAsync(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            return await BuildProcessMetricsAsync(process);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> OptimizeProcessPriorityAsync(int processId, ProcessPriority priority)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.PriorityClass = priority switch
            {
                ProcessPriority.Idle => ProcessPriorityClass.Idle,
                ProcessPriority.BelowNormal => ProcessPriorityClass.BelowNormal,
                ProcessPriority.Normal => ProcessPriorityClass.Normal,
                ProcessPriority.AboveNormal => ProcessPriorityClass.AboveNormal,
                ProcessPriority.High => ProcessPriorityClass.High,
                ProcessPriority.Realtime => ProcessPriorityClass.RealTime,
                _ => ProcessPriorityClass.Normal
            };
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TerminateProcessAsync(int processId, bool force = false)
    {
        try
        {
            var process = Process.GetProcessById(processId);

            if (force)
            {
                process.Kill(true); // Kill process tree
            }
            else
            {
                process.CloseMainWindow();
                if (!process.WaitForExit(5000))
                {
                    process.Kill();
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<SystemResourceUsage> GetSystemResourceUsageAsync()
    {
        var usage = new SystemResourceUsage
        {
            Timestamp = DateTime.Now
        };

        // CPU Usage (total)
        using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        cpuCounter.NextValue(); // First call returns 0
        await Task.Delay(100);
        usage.CpuUsagePercentage = cpuCounter.NextValue();

        // Memory
        var memInfo = new MEMORYSTATUSEX();
        if (GlobalMemoryStatusEx(ref memInfo))
        {
            usage.TotalMemoryBytes = (long)memInfo.ullTotalPhys;
            usage.AvailableMemoryBytes = (long)memInfo.ullAvailPhys;
            usage.UsedMemoryBytes = usage.TotalMemoryBytes - usage.AvailableMemoryBytes;
            usage.MemoryUsagePercentage = ((double)usage.UsedMemoryBytes / usage.TotalMemoryBytes) * 100;
        }

        // Process count
        usage.ActiveProcessCount = Process.GetProcesses().Length;
        usage.TotalThreadCount = Process.GetProcesses().Sum(p =>
        {
            try { return p.Threads.Count; }
            catch { return 0; }
        });

        // Disk usage (drive C:)
        try
        {
            var drive = new System.IO.DriveInfo("C");
            usage.DiskUsages.Add(new DiskUsage
            {
                DriveName = "C:",
                TotalSize = drive.TotalSize,
                FreeSize = drive.AvailableFreeSpace,
                UsedSize = drive.TotalSize - drive.AvailableFreeSpace,
                UsagePercentage = ((double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize) * 100
            });
        }
        catch { }

        return usage;
    }

    private async Task<List<ProcessMetrics>> GetAllProcessMetricsAsync()
    {
        var processes = Process.GetProcesses();
        var metrics = new List<ProcessMetrics>();

        foreach (var process in processes)
        {
            try
            {
                var metric = await BuildProcessMetricsAsync(process);
                if (metric != null)
                {
                    metrics.Add(metric);
                }
            }
            catch
            {
                // Skip processes we can't access
            }
        }

        return metrics;
    }

    private async Task<ProcessMetrics?> BuildProcessMetricsAsync(Process process)
    {
        try
        {
            var metrics = new ProcessMetrics
            {
                ProcessId = process.Id,
                Name = process.ProcessName,
                MemoryUsage = process.WorkingSet64,
                ThreadCount = process.Threads.Count,
                HandleCount = process.HandleCount,
                StartTime = process.StartTime,
                TotalProcessorTime = process.TotalProcessorTime
            };

            // Get executable path
            try
            {
                metrics.ExecutablePath = process.MainModule?.FileName ?? string.Empty;
            }
            catch { }

            // Calculate CPU usage
            metrics.CpuUsage = await GetProcessCpuUsageAsync(process);

            // Get priority
            metrics.Priority = process.PriorityClass switch
            {
                ProcessPriorityClass.Idle => ProcessPriority.Idle,
                ProcessPriorityClass.BelowNormal => ProcessPriority.BelowNormal,
                ProcessPriorityClass.Normal => ProcessPriority.Normal,
                ProcessPriorityClass.AboveNormal => ProcessPriority.AboveNormal,
                ProcessPriorityClass.High => ProcessPriority.High,
                ProcessPriorityClass.RealTime => ProcessPriority.Realtime,
                _ => ProcessPriority.Normal
            };

            // Check if signed
            if (!string.IsNullOrEmpty(metrics.ExecutablePath))
            {
                metrics.IsSigned = IsFileSigned(metrics.ExecutablePath);
                metrics.Publisher = GetFilePublisher(metrics.ExecutablePath);
            }

            return metrics;
        }
        catch
        {
            return null;
        }
    }

    private async Task<double> GetProcessCpuUsageAsync(Process process)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;

            await Task.Delay(100);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;

            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return cpuUsageTotal * 100;
        }
        catch
        {
            return 0;
        }
    }

    private SuspiciousProcess? AnalyzeProcess(ProcessMetrics process)
    {
        var reasons = new List<SuspicionReason>();
        var level = SuspicionLevel.Low;

        // High CPU usage
        if (process.CpuUsage > 80)
        {
            reasons.Add(SuspicionReason.HighCpuUsage);
            level = SuspicionLevel.Medium;
        }

        // High memory usage (> 2GB)
        if (process.MemoryUsage > 2L * 1024 * 1024 * 1024)
        {
            reasons.Add(SuspicionReason.HighMemoryUsage);
            level = SuspicionLevel.Medium;
        }

        // Unsigned executable
        if (!process.IsSigned && !string.IsNullOrEmpty(process.ExecutablePath))
        {
            reasons.Add(SuspicionReason.UnknownPublisher);
            if (level < SuspicionLevel.Medium)
                level = SuspicionLevel.Medium;
        }

        // Suspicious location (Temp, AppData)
        if (process.ExecutablePath.Contains("\\Temp\\") ||
            process.ExecutablePath.Contains("\\AppData\\Local\\Temp\\"))
        {
            reasons.Add(SuspicionReason.SuspiciousLocation);
            level = SuspicionLevel.High;
        }

        // Too many handles (potential leak)
        if (process.HandleCount > 10000)
        {
            reasons.Add(SuspicionReason.SystemResourceAbuse);
        }

        if (reasons.Count == 0)
            return null;

        return new SuspiciousProcess
        {
            Metrics = process,
            Level = level,
            Reasons = reasons,
            Action = level >= SuspicionLevel.High ? RecommendedAction.Terminate : RecommendedAction.Monitor,
            Description = $"Process {process.Name} shows suspicious behavior: {string.Join(", ", reasons)}"
        };
    }

    private bool IsFileSigned(string filePath)
    {
        try
        {
            var cert = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(filePath);
            return cert != null;
        }
        catch
        {
            return false;
        }
    }

    private string? GetFilePublisher(string filePath)
    {
        try
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
            return versionInfo.CompanyName;
        }
        catch
        {
            return null;
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

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

    private class ProcessPerformanceData
    {
        public DateTime LastSample { get; set; }
        public TimeSpan LastCpuTime { get; set; }
    }
}
