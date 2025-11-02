using WinCheck.Core.Models;

namespace WinCheck.Core.Interfaces;

/// <summary>
/// Real-time process monitoring ve analiz servisi
/// </summary>
public interface IProcessMonitorService
{
    /// <summary>
    /// Tüm aktif işlemleri real-time izler
    /// </summary>
    IObservable<ProcessMetrics> MonitorProcesses();

    /// <summary>
    /// Gereksiz/zararlı işlemleri tespit eder
    /// </summary>
    Task<IEnumerable<SuspiciousProcess>> DetectSuspiciousProcessesAsync();

    /// <summary>
    /// Belirli bir işlemin metriklerini alır
    /// </summary>
    Task<ProcessMetrics?> GetProcessMetricsAsync(int processId);

    /// <summary>
    /// İşlem önceliğini optimize eder
    /// </summary>
    Task<bool> OptimizeProcessPriorityAsync(int processId, ProcessPriority priority);

    /// <summary>
    /// İşlemi güvenli şekilde sonlandırır
    /// </summary>
    Task<bool> TerminateProcessAsync(int processId, bool force = false);

    /// <summary>
    /// Sistem geneli kaynak kullanımını alır
    /// </summary>
    Task<SystemResourceUsage> GetSystemResourceUsageAsync();
}
