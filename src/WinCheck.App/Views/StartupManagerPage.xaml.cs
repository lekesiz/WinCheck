using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using WinCheck.App.ViewModels;

namespace WinCheck.App.Views;

public sealed partial class StartupManagerPage : Page
{
    public StartupManagerViewModel ViewModel { get; }

    public StartupManagerPage()
    {
        ViewModel = App.Services.GetRequiredService<StartupManagerViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    private async void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Debug.WriteLine("LoadButton_Click: Starting");

            if (ViewModel?.LoadDataCommand == null)
            {
                Debug.WriteLine("LoadButton_Click: LoadDataCommand is null!");
                ViewModel.StatusMessage = "Error: Command not initialized";
                return;
            }

            Debug.WriteLine("LoadButton_Click: Executing command");
            await ViewModel.LoadDataCommand.ExecuteAsync(null);
            Debug.WriteLine("LoadButton_Click: Command completed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadButton_Click: Exception: {ex.Message}");
            Debug.WriteLine($"LoadButton_Click: Stack trace: {ex.StackTrace}");

            if (ViewModel != null)
            {
                ViewModel.StatusMessage = $"CRASH DETECTED: {ex.Message}";
            }
        }
    }
}
