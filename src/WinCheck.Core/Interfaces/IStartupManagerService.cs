using System.Collections.Generic;
using System.Threading.Tasks;
using WinCheck.Core.Models;

namespace WinCheck.Core.Interfaces;

public interface IStartupManagerService
{
    /// <summary>
    /// Get all startup programs
    /// </summary>
    Task<List<StartupProgram>> GetStartupProgramsAsync();

    /// <summary>
    /// Enable/disable startup program
    /// </summary>
    Task<bool> SetStartupStateAsync(StartupProgram program, bool enabled);

    /// <summary>
    /// Add program to startup
    /// </summary>
    Task<bool> AddToStartupAsync(string name, string path, StartupLocation location);

    /// <summary>
    /// Remove program from startup
    /// </summary>
    Task<bool> RemoveFromStartupAsync(StartupProgram program);

    /// <summary>
    /// Get estimated boot time impact
    /// </summary>
    Task<StartupImpactAnalysis> AnalyzeBootImpactAsync();
}
