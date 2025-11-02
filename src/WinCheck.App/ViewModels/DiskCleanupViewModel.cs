using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;

namespace WinCheck.App.ViewModels;

public partial class DiskCleanupViewModel : ObservableObject
{
    private readonly IDiskCleanupService _diskCleanup;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private bool _isCleaning;

    [ObservableProperty]
    private string _statusMessage = "Ready to scan";

    [ObservableProperty]
    private string _totalCleanable = "0 MB";

    public ObservableCollection<string> Categories { get; } = new();

    public DiskCleanupViewModel(IDiskCleanupService diskCleanup)
    {
        _diskCleanup = diskCleanup;
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        try
        {
            IsScanning = true;
            StatusMessage = "Scanning disk...";
            Categories.Clear();

            // Analyze disk
            var analysis = await _diskCleanup.AnalyzeDiskAsync();

            long totalBytes = 0;
            foreach (var category in analysis.CleanupCategories)
            {
                Categories.Add($"{category.Name} - {FormatBytes(category.SizeBytes)} ({category.FileCount} files)");
                totalBytes += category.SizeBytes;
            }

            TotalCleanable = FormatBytes(totalBytes);
            StatusMessage = $"Found {TotalCleanable} cleanable in {Categories.Count} categories";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private async Task CleanAsync()
    {
        try
        {
            IsCleaning = true;
            StatusMessage = "Cleaning...";

            long totalCleaned = 0;

            // Clean temporary files
            var tempResult = await _diskCleanup.CleanTemporaryFilesAsync();
            totalCleaned += tempResult.BytesCleaned;

            // Clean browser caches
            var browserResult = await _diskCleanup.CleanBrowserCachesAsync();
            totalCleaned += browserResult.BytesCleaned;

            // Clean Windows Update cache
            var updateResult = await _diskCleanup.CleanWindowsUpdateCacheAsync();
            totalCleaned += updateResult.BytesCleaned;

            // Empty Recycle Bin
            var recycleResult = await _diskCleanup.EmptyRecycleBinAsync();
            totalCleaned += recycleResult.BytesCleaned;

            StatusMessage = $"Cleaned {FormatBytes(totalCleaned)} successfully!";
            TotalCleanable = "0 MB";
            Categories.Clear();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsCleaning = false;
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:F2} {sizes[order]}";
    }
}
