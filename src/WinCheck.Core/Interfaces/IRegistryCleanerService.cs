using System.Collections.Generic;
using System.Threading.Tasks;
using WinCheck.Core.Models;

namespace WinCheck.Core.Interfaces;

public interface IRegistryCleanerService
{
    /// <summary>
    /// Scan registry for issues
    /// </summary>
    Task<RegistryScanResult> ScanRegistryAsync();

    /// <summary>
    /// Fix registry issues
    /// </summary>
    Task<RegistryCleanupResult> CleanRegistryAsync(List<RegistryIssue> issuesToFix);

    /// <summary>
    /// Create registry backup
    /// </summary>
    Task<bool> CreateBackupAsync(string backupPath);

    /// <summary>
    /// Restore registry from backup
    /// </summary>
    Task<bool> RestoreBackupAsync(string backupPath);
}
