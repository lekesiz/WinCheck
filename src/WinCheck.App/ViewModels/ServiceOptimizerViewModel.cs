using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.App.ViewModels;

public partial class ServiceOptimizerViewModel : ObservableObject
{
    private readonly IServiceOptimizerService _serviceOptimizer;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready to load services";

    [ObservableProperty]
    private int _optimizableCount;

    public ObservableCollection<ServiceOptimization> Services { get; } = new();

    public ServiceOptimizerViewModel(IServiceOptimizerService serviceOptimizer)
    {
        _serviceOptimizer = serviceOptimizer;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading services...";

            // Get optimizable services
            var serviceList = await _serviceOptimizer.GetOptimizableServicesAsync();

            Services.Clear();
            foreach (var svc in serviceList)
            {
                Services.Add(svc);
            }

            OptimizableCount = Services.Count;
            StatusMessage = $"Loaded {OptimizableCount} optimizable services";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task OptimizeAllAsync()
    {
        try
        {
            StatusMessage = "Optimizing all services...";

            int optimized = 0;
            foreach (var service in Services.ToList())
            {
                var success = await _serviceOptimizer.OptimizeServiceAsync(service);
                if (success)
                {
                    optimized++;
                }
            }

            StatusMessage = $"Optimized {optimized} services successfully";
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        try
        {
            StatusMessage = "Creating backup...";
            var success = await _serviceOptimizer.CreateBackupAsync();

            if (success)
            {
                StatusMessage = "Backup created successfully";
            }
            else
            {
                StatusMessage = "Failed to create backup";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RestoreBackupAsync()
    {
        try
        {
            StatusMessage = "Restoring from backup...";
            var success = await _serviceOptimizer.RestoreFromBackupAsync();

            if (success)
            {
                StatusMessage = "Restored from backup successfully";
                await LoadDataAsync();
            }
            else
            {
                StatusMessage = "Failed to restore from backup";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
