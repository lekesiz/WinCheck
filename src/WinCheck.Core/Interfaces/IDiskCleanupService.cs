using System.Collections.Generic;
using System.Threading.Tasks;
using WinCheck.Core.Models;

namespace WinCheck.Core.Interfaces;

public interface IDiskCleanupService
{
    /// <summary>
    /// Analyze disk for cleanable files
    /// </summary>
    Task<DiskAnalysis> AnalyzeDiskAsync(string driveLetter = "C:");

    /// <summary>
    /// Clean temporary files
    /// </summary>
    Task<CleanupResult> CleanTemporaryFilesAsync();

    /// <summary>
    /// Clean browser caches
    /// </summary>
    Task<CleanupResult> CleanBrowserCachesAsync();

    /// <summary>
    /// Clean Windows Update cache
    /// </summary>
    Task<CleanupResult> CleanWindowsUpdateCacheAsync();

    /// <summary>
    /// Find and remove duplicate files
    /// </summary>
    Task<List<DuplicateFileGroup>> FindDuplicateFilesAsync(string path);

    /// <summary>
    /// Empty Recycle Bin
    /// </summary>
    Task<CleanupResult> EmptyRecycleBinAsync();

    /// <summary>
    /// Run full cleanup
    /// </summary>
    Task<CleanupResult> RunFullCleanupAsync(CleanupOptions options);
}
