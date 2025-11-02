using System.Collections.Generic;
using System.Threading.Tasks;
using WinCheck.Core.Models;

namespace WinCheck.Core.Interfaces;

public interface IOSDetectionService
{
    /// <summary>
    /// Detect comprehensive OS information
    /// </summary>
    Task<OSInfo> DetectOSAsync();

    /// <summary>
    /// Get OS-specific optimizations
    /// </summary>
    Task<List<OSOptimization>> GetRecommendedOptimizationsAsync();

    /// <summary>
    /// Apply an optimization
    /// </summary>
    Task<bool> ApplyOptimizationAsync(OSOptimization optimization);

    /// <summary>
    /// Check for Windows updates
    /// </summary>
    Task<UpdateInfo> CheckForUpdatesAsync();
}
