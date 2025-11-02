using System.Collections.Generic;
using System.Threading.Tasks;
using WinCheck.Core.Models;

namespace WinCheck.Core.Interfaces;

public interface IServiceOptimizerService
{
    /// <summary>
    /// Get all Windows services with optimization recommendations
    /// </summary>
    Task<List<WindowsServiceInfo>> GetAllServicesAsync();

    /// <summary>
    /// Get services that are safe to disable
    /// </summary>
    Task<List<ServiceOptimization>> GetOptimizableServicesAsync();

    /// <summary>
    /// Apply service optimization
    /// </summary>
    Task<bool> OptimizeServiceAsync(ServiceOptimization optimization);

    /// <summary>
    /// Restore service to original state
    /// </summary>
    Task<bool> RestoreServiceAsync(string serviceName);

    /// <summary>
    /// Create backup of current service configurations
    /// </summary>
    Task<bool> CreateBackupAsync();

    /// <summary>
    /// Restore from backup
    /// </summary>
    Task<bool> RestoreFromBackupAsync();
}
