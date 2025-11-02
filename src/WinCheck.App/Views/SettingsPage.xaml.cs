using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WinCheck.App.ViewModels;

namespace WinCheck.App.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
        DataContext = ViewModel;

        // Load settings when page is opened
        _ = ViewModel.LoadSettingsCommand.ExecuteAsync(null);
    }
}
