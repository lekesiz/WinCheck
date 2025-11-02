using System;
using System.Collections.Generic;

namespace WinCheck.Core.Models;

public class DiskAnalysis
{
    public string DriveLetter { get; set; } = string.Empty;
    public long TotalSizeBytes { get; set; }
    public long FreeSizeBytes { get; set; }
    public long UsedSizeBytes { get; set; }
    public double UsagePercentage { get; set; }
    public List<CleanupCategory> CleanupCategories { get; set; } = new();
    public long TotalCleanableBytes { get; set; }
}

public class CleanupCategory
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public int FileCount { get; set; }
    public CleanupSafety Safety { get; set; }
    public List<string> Paths { get; set; } = new();
}

public enum CleanupSafety
{
    Safe,           // 100% safe to delete
    MostlySafe,     // Safe but may clear useful data (cache, downloads)
    Careful,        // Review before deleting
    Advanced        // Only for advanced users
}

public class CleanupOptions
{
    public bool CleanTempFiles { get; set; } = true;
    public bool CleanBrowserCache { get; set; } = true;
    public bool CleanWindowsUpdate { get; set; } = false; // Requires admin
    public bool CleanRecycleBin { get; set; } = true;
    public bool CleanDownloads { get; set; } = false; // User should confirm
    public bool CleanThumbnails { get; set; } = true;
    public bool CleanErrorReports { get; set; } = true;
    public bool CleanLogFiles { get; set; } = true;
}

public class CleanupResult
{
    public bool Success { get; set; }
    public long BytesCleaned { get; set; }
    public int FilesDeleted { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> DeletedPaths { get; set; } = new();
}

public class DuplicateFileGroup
{
    public string FileHash { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public List<DuplicateFile> Files { get; set; } = new();
    public long WastedSpace => FileSize * (Files.Count - 1);
}

public class DuplicateFile
{
    public string Path { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public bool KeepThis { get; set; }
}
