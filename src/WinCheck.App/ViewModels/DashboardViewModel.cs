using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WinCheck.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private double _healthScore = 82;

    [ObservableProperty]
    private double _cpuUsage = 45;

    [ObservableProperty]
    private double _memoryUsage = 51;

    [ObservableProperty]
    private double _diskUsage = 49;

    [ObservableProperty]
    private string _memoryText = "8.2/16 GB";

    [ObservableProperty]
    private string _diskText = "250/512 GB";

    [RelayCommand]
    private async Task QuickCleanAsync()
    {
        // Quick cleanup logic
        await Task.Delay(100);
    }

    [RelayCommand]
    private async Task DeepScanAsync()
    {
        // Deep scan logic
        await Task.Delay(100);
    }

    [RelayCommand]
    private async Task OptimizeAsync()
    {
        // Optimization logic
        await Task.Delay(100);
    }
}
