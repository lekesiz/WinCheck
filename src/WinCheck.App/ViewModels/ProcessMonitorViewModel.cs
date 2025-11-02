using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.App.ViewModels;

public partial class ProcessMonitorViewModel : ObservableObject
{
    private readonly IProcessMonitorService _processMonitor;
    private IDisposable? _processSubscription;

    [ObservableProperty]
    private bool _isMonitoring;

    [ObservableProperty]
    private string _statusMessage = "Ready to monitor processes";

    [ObservableProperty]
    private double _cpuUsage;

    [ObservableProperty]
    private double _memoryUsage;

    [ObservableProperty]
    private int _processCount;

    public ObservableCollection<ProcessMetrics> Processes { get; } = new();
    public ObservableCollection<SuspiciousProcess> SuspiciousProcesses { get; } = new();

    [ObservableProperty]
    private ProcessMetrics? _selectedProcess;

    public ProcessMonitorViewModel(IProcessMonitorService processMonitor)
    {
        _processMonitor = processMonitor;
    }

    [RelayCommand]
    private async Task StartMonitoringAsync()
    {
        try
        {
            IsMonitoring = true;
            StatusMessage = "Monitoring processes...";

            // Get system resource usage
            var systemUsage = await _processMonitor.GetSystemResourceUsageAsync();
            CpuUsage = systemUsage.CpuUsagePercentage;
            MemoryUsage = systemUsage.MemoryUsagePercentage;

            // Subscribe to real-time process monitoring
            _processSubscription = _processMonitor.MonitorProcesses().Subscribe(
                processMetrics =>
                {
                    // Update or add process in UI
                    var existing = Processes.FirstOrDefault(p => p.ProcessId == processMetrics.ProcessId);
                    if (existing != null)
                    {
                        Processes.Remove(existing);
                    }
                    Processes.Add(processMetrics);

                    // Keep only top 50 processes by CPU usage
                    if (Processes.Count > 50)
                    {
                        var toRemove = Processes.OrderBy(p => p.CpuUsage).First();
                        Processes.Remove(toRemove);
                    }

                    ProcessCount = Processes.Count;
                },
                error =>
                {
                    StatusMessage = $"Monitoring error: {error.Message}";
                }
            );

            // Detect suspicious processes
            var suspicious = await _processMonitor.DetectSuspiciousProcessesAsync();
            SuspiciousProcesses.Clear();
            foreach (var sp in suspicious)
            {
                SuspiciousProcesses.Add(sp);
            }

            if (SuspiciousProcesses.Any())
            {
                StatusMessage = $"Monitoring {ProcessCount} processes - {SuspiciousProcesses.Count} suspicious detected!";
            }
            else
            {
                StatusMessage = $"Monitoring {ProcessCount} processes - All clean";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            IsMonitoring = false;
        }
    }

    [RelayCommand]
    private void StopMonitoring()
    {
        _processSubscription?.Dispose();
        _processSubscription = null;
        IsMonitoring = false;
        Processes.Clear();
        StatusMessage = "Monitoring stopped";
    }

    [RelayCommand]
    private async Task TerminateProcessAsync()
    {
        if (SelectedProcess != null)
        {
            try
            {
                var success = await _processMonitor.TerminateProcessAsync(SelectedProcess.ProcessId);
                if (success)
                {
                    Processes.Remove(SelectedProcess);
                    StatusMessage = $"Process {SelectedProcess.Name} terminated";
                }
                else
                {
                    StatusMessage = $"Failed to terminate {SelectedProcess.Name}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }
    }
}
