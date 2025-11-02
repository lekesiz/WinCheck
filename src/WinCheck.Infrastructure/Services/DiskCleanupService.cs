using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.Infrastructure.Services;

public class DiskCleanupService : IDiskCleanupService
{
    public async Task<DiskAnalysis> AnalyzeDiskAsync(string driveLetter = "C:")
    {
        var analysis = new DiskAnalysis
        {
            DriveLetter = driveLetter
        };

        await Task.Run(() =>
        {
            try
            {
                var drive = new DriveInfo(driveLetter);
                analysis.TotalSizeBytes = drive.TotalSize;
                analysis.FreeSizeBytes = drive.AvailableFreeSpace;
                analysis.UsedSizeBytes = analysis.TotalSizeBytes - analysis.FreeSizeBytes;
                analysis.UsagePercentage = ((double)analysis.UsedSizeBytes / analysis.TotalSizeBytes) * 100;

                // Analyze cleanup categories
                analysis.CleanupCategories.Add(AnalyzeTempFiles());
                analysis.CleanupCategories.Add(AnalyzeBrowserCaches());
                analysis.CleanupCategories.Add(AnalyzeWindowsUpdateCache());
                analysis.CleanupCategories.Add(AnalyzeRecycleBin());
                analysis.CleanupCategories.Add(AnalyzeThumbnails());
                analysis.CleanupCategories.Add(AnalyzeErrorReports());
                analysis.CleanupCategories.Add(AnalyzeLogFiles());

                analysis.TotalCleanableBytes = analysis.CleanupCategories.Sum(c => c.SizeBytes);
            }
            catch { }
        });

        return analysis;
    }

    public async Task<CleanupResult> CleanTemporaryFilesAsync()
    {
        var result = new CleanupResult();
        var sw = Stopwatch.StartNew();

        await Task.Run(() =>
        {
            var tempPaths = new List<string>
            {
                Path.GetTempPath(),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp")
            };

            foreach (var tempPath in tempPaths)
            {
                try
                {
                    if (Directory.Exists(tempPath))
                    {
                        var files = Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            try
                            {
                                var fileInfo = new FileInfo(file);
                                if (fileInfo.Exists && !IsFileLocked(fileInfo))
                                {
                                    var size = fileInfo.Length;
                                    fileInfo.Delete();
                                    result.BytesCleaned += size;
                                    result.FilesDeleted++;
                                    result.DeletedPaths.Add(file);
                                }
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add($"Error deleting {file}: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error accessing {tempPath}: {ex.Message}");
                }
            }

            result.Success = true;
        });

        sw.Stop();
        result.Duration = sw.Elapsed;
        return result;
    }

    public async Task<CleanupResult> CleanBrowserCachesAsync()
    {
        var result = new CleanupResult();
        var sw = Stopwatch.StartNew();

        await Task.Run(() =>
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var browserCachePaths = new Dictionary<string, string>
            {
                // Chrome
                ["Chrome"] = Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default", "Cache"),
                ["Chrome Cache"] = Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default", "Code Cache"),

                // Edge
                ["Edge"] = Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default", "Cache"),
                ["Edge Cache"] = Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default", "Code Cache"),

                // Firefox
                ["Firefox"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles"),

                // Opera
                ["Opera"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera Software", "Opera Stable", "Cache")
            };

            foreach (var browser in browserCachePaths)
            {
                try
                {
                    if (Directory.Exists(browser.Value))
                    {
                        var files = Directory.GetFiles(browser.Value, "*.*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            try
                            {
                                var fileInfo = new FileInfo(file);
                                if (fileInfo.Exists && !IsFileLocked(fileInfo))
                                {
                                    var size = fileInfo.Length;
                                    fileInfo.Delete();
                                    result.BytesCleaned += size;
                                    result.FilesDeleted++;
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error cleaning {browser.Key}: {ex.Message}");
                }
            }

            result.Success = true;
        });

        sw.Stop();
        result.Duration = sw.Elapsed;
        return result;
    }

    public async Task<CleanupResult> CleanWindowsUpdateCacheAsync()
    {
        var result = new CleanupResult();
        var sw = Stopwatch.StartNew();

        await Task.Run(() =>
        {
            try
            {
                // Stop Windows Update service first
                var stopProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = "stop wuauserv",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                stopProcess?.WaitForExit();

                // Clean SoftwareDistribution folder
                var updateCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");

                if (Directory.Exists(updateCachePath))
                {
                    var files = Directory.GetFiles(updateCachePath, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            var size = fileInfo.Length;
                            fileInfo.Delete();
                            result.BytesCleaned += size;
                            result.FilesDeleted++;
                        }
                        catch { }
                    }
                }

                // Restart Windows Update service
                var startProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = "start wuauserv",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                startProcess?.WaitForExit();

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error cleaning Windows Update cache: {ex.Message}");
                result.Success = false;
            }
        });

        sw.Stop();
        result.Duration = sw.Elapsed;
        return result;
    }

    public async Task<List<DuplicateFileGroup>> FindDuplicateFilesAsync(string path)
    {
        var duplicates = new Dictionary<string, List<DuplicateFile>>();

        await Task.Run(() =>
        {
            try
            {
                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

                // Group by size first (quick filter)
                var filesBySize = files
                    .Select(f => new FileInfo(f))
                    .Where(f => f.Length > 0) // Skip empty files
                    .GroupBy(f => f.Length)
                    .Where(g => g.Count() > 1);

                foreach (var sizeGroup in filesBySize)
                {
                    foreach (var file in sizeGroup)
                    {
                        try
                        {
                            var hash = CalculateFileHash(file.FullName);

                            if (!duplicates.ContainsKey(hash))
                            {
                                duplicates[hash] = new List<DuplicateFile>();
                            }

                            duplicates[hash].Add(new DuplicateFile
                            {
                                Path = file.FullName,
                                LastModified = file.LastWriteTime
                            });
                        }
                        catch { }
                    }
                }
            }
            catch { }
        });

        // Convert to DuplicateFileGroup and filter out non-duplicates
        return duplicates
            .Where(kvp => kvp.Value.Count > 1)
            .Select(kvp => new DuplicateFileGroup
            {
                FileHash = kvp.Key,
                FileSize = new FileInfo(kvp.Value[0].Path).Length,
                Files = kvp.Value
            })
            .OrderByDescending(g => g.WastedSpace)
            .ToList();
    }

    public async Task<CleanupResult> EmptyRecycleBinAsync()
    {
        var result = new CleanupResult();
        var sw = Stopwatch.StartNew();

        await Task.Run(() =>
        {
            try
            {
                // Get Recycle Bin size before emptying
                var recycleBinPath = @"C:\$Recycle.Bin";
                if (Directory.Exists(recycleBinPath))
                {
                    var files = Directory.GetFiles(recycleBinPath, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            result.BytesCleaned += fileInfo.Length;
                        }
                        catch { }
                    }
                }

                // Empty Recycle Bin via PowerShell
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                process?.WaitForExit();

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error emptying Recycle Bin: {ex.Message}");
                result.Success = false;
            }
        });

        sw.Stop();
        result.Duration = sw.Elapsed;
        return result;
    }

    public async Task<CleanupResult> RunFullCleanupAsync(CleanupOptions options)
    {
        var result = new CleanupResult();
        var sw = Stopwatch.StartNew();

        var tasks = new List<Task<CleanupResult>>();

        if (options.CleanTempFiles)
            tasks.Add(CleanTemporaryFilesAsync());

        if (options.CleanBrowserCache)
            tasks.Add(CleanBrowserCachesAsync());

        if (options.CleanWindowsUpdate)
            tasks.Add(CleanWindowsUpdateCacheAsync());

        if (options.CleanRecycleBin)
            tasks.Add(EmptyRecycleBinAsync());

        if (options.CleanThumbnails)
            tasks.Add(CleanThumbnailsAsync());

        if (options.CleanErrorReports)
            tasks.Add(CleanErrorReportsAsync());

        if (options.CleanLogFiles)
            tasks.Add(CleanLogFilesAsync());

        var results = await Task.WhenAll(tasks);

        // Aggregate results
        foreach (var r in results)
        {
            result.BytesCleaned += r.BytesCleaned;
            result.FilesDeleted += r.FilesDeleted;
            result.Errors.AddRange(r.Errors);
            result.DeletedPaths.AddRange(r.DeletedPaths);
        }

        result.Success = results.All(r => r.Success);
        sw.Stop();
        result.Duration = sw.Elapsed;

        return result;
    }

    #region Private Helpers

    private CleanupCategory AnalyzeTempFiles()
    {
        var category = new CleanupCategory
        {
            Name = "Temporary Files",
            Description = "Temporary files created by Windows and applications",
            Safety = CleanupSafety.Safe
        };

        try
        {
            var tempPaths = new List<string>
            {
                Path.GetTempPath(),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
            };

            foreach (var path in tempPaths)
            {
                if (Directory.Exists(path))
                {
                    category.Paths.Add(path);
                    var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    category.FileCount += files.Length;
                    category.SizeBytes += files.Sum(f =>
                    {
                        try { return new FileInfo(f).Length; }
                        catch { return 0; }
                    });
                }
            }
        }
        catch { }

        return category;
    }

    private CleanupCategory AnalyzeBrowserCaches()
    {
        var category = new CleanupCategory
        {
            Name = "Browser Caches",
            Description = "Cache files from web browsers",
            Safety = CleanupSafety.MostlySafe
        };

        try
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var cachePaths = new List<string>
            {
                Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default", "Cache"),
                Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default", "Cache")
            };

            foreach (var path in cachePaths)
            {
                if (Directory.Exists(path))
                {
                    category.Paths.Add(path);
                    try
                    {
                        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                        category.FileCount += files.Length;
                        category.SizeBytes += files.Sum(f =>
                        {
                            try { return new FileInfo(f).Length; }
                            catch { return 0; }
                        });
                    }
                    catch { }
                }
            }
        }
        catch { }

        return category;
    }

    private CleanupCategory AnalyzeWindowsUpdateCache()
    {
        var category = new CleanupCategory
        {
            Name = "Windows Update Cache",
            Description = "Downloaded Windows Update files",
            Safety = CleanupSafety.Advanced
        };

        try
        {
            var updatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");

            if (Directory.Exists(updatePath))
            {
                category.Paths.Add(updatePath);
                var files = Directory.GetFiles(updatePath, "*.*", SearchOption.AllDirectories);
                category.FileCount = files.Length;
                category.SizeBytes = files.Sum(f =>
                {
                    try { return new FileInfo(f).Length; }
                    catch { return 0; }
                });
            }
        }
        catch { }

        return category;
    }

    private CleanupCategory AnalyzeRecycleBin()
    {
        var category = new CleanupCategory
        {
            Name = "Recycle Bin",
            Description = "Deleted files in Recycle Bin",
            Safety = CleanupSafety.Careful
        };

        try
        {
            var recycleBinPath = @"C:\$Recycle.Bin";
            if (Directory.Exists(recycleBinPath))
            {
                category.Paths.Add(recycleBinPath);
                var files = Directory.GetFiles(recycleBinPath, "*.*", SearchOption.AllDirectories);
                category.FileCount = files.Length;
                category.SizeBytes = files.Sum(f =>
                {
                    try { return new FileInfo(f).Length; }
                    catch { return 0; }
                });
            }
        }
        catch { }

        return category;
    }

    private CleanupCategory AnalyzeThumbnails()
    {
        var category = new CleanupCategory
        {
            Name = "Thumbnail Cache",
            Description = "Cached image thumbnails",
            Safety = CleanupSafety.Safe
        };

        try
        {
            var thumbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Explorer");

            if (Directory.Exists(thumbPath))
            {
                category.Paths.Add(thumbPath);
                var files = Directory.GetFiles(thumbPath, "thumbcache_*.db", SearchOption.TopDirectoryOnly);
                category.FileCount = files.Length;
                category.SizeBytes = files.Sum(f =>
                {
                    try { return new FileInfo(f).Length; }
                    catch { return 0; }
                });
            }
        }
        catch { }

        return category;
    }

    private CleanupCategory AnalyzeErrorReports()
    {
        var category = new CleanupCategory
        {
            Name = "Error Reports",
            Description = "Windows Error Reporting files",
            Safety = CleanupSafety.Safe
        };

        try
        {
            var errorPaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows", "WER"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "WER")
            };

            foreach (var path in errorPaths)
            {
                if (Directory.Exists(path))
                {
                    category.Paths.Add(path);
                    var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    category.FileCount += files.Length;
                    category.SizeBytes += files.Sum(f =>
                    {
                        try { return new FileInfo(f).Length; }
                        catch { return 0; }
                    });
                }
            }
        }
        catch { }

        return category;
    }

    private CleanupCategory AnalyzeLogFiles()
    {
        var category = new CleanupCategory
        {
            Name = "Log Files",
            Description = "Windows log files",
            Safety = CleanupSafety.Safe
        };

        try
        {
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs");

            if (Directory.Exists(logPath))
            {
                category.Paths.Add(logPath);
                var files = Directory.GetFiles(logPath, "*.log", SearchOption.AllDirectories);
                category.FileCount = files.Length;
                category.SizeBytes = files.Sum(f =>
                {
                    try { return new FileInfo(f).Length; }
                    catch { return 0; }
                });
            }
        }
        catch { }

        return category;
    }

    private async Task<CleanupResult> CleanThumbnailsAsync()
    {
        var result = new CleanupResult();

        await Task.Run(() =>
        {
            try
            {
                var thumbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Explorer");

                if (Directory.Exists(thumbPath))
                {
                    var files = Directory.GetFiles(thumbPath, "thumbcache_*.db");
                    foreach (var file in files)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            var size = fileInfo.Length;
                            fileInfo.Delete();
                            result.BytesCleaned += size;
                            result.FilesDeleted++;
                        }
                        catch { }
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }
        });

        return result;
    }

    private async Task<CleanupResult> CleanErrorReportsAsync()
    {
        var result = new CleanupResult();

        await Task.Run(() =>
        {
            try
            {
                var errorPaths = new List<string>
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows", "WER"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "WER")
                };

                foreach (var path in errorPaths)
                {
                    if (Directory.Exists(path))
                    {
                        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            try
                            {
                                var fileInfo = new FileInfo(file);
                                var size = fileInfo.Length;
                                fileInfo.Delete();
                                result.BytesCleaned += size;
                                result.FilesDeleted++;
                            }
                            catch { }
                        }
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }
        });

        return result;
    }

    private async Task<CleanupResult> CleanLogFilesAsync()
    {
        var result = new CleanupResult();

        await Task.Run(() =>
        {
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs");

                if (Directory.Exists(logPath))
                {
                    var files = Directory.GetFiles(logPath, "*.log", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            // Only delete old log files (> 30 days)
                            var fileInfo = new FileInfo(file);
                            if ((DateTime.Now - fileInfo.LastWriteTime).TotalDays > 30)
                            {
                                var size = fileInfo.Length;
                                fileInfo.Delete();
                                result.BytesCleaned += size;
                                result.FilesDeleted++;
                            }
                        }
                        catch { }
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }
        });

        return result;
    }

    private bool IsFileLocked(FileInfo file)
    {
        try
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch
        {
            return true;
        }
    }

    private string CalculateFileHash(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    #endregion
}
