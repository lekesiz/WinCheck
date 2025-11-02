using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinCheck.App.Views;

namespace WinCheck.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "WinCheck - System Optimizer";

        // Default navigation
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
        }
        else if (args.SelectedItemContainer?.Tag is string tag)
        {
            var pageType = tag switch
            {
                "Dashboard" => typeof(DashboardPage),
                "ProcessMonitor" => typeof(ProcessMonitorPage),
                "DiskCleanup" => typeof(DiskCleanupPage),
                "ServiceOptimizer" => typeof(ServiceOptimizerPage),
                "StartupManager" => typeof(StartupManagerPage),
                "RegistryCleaner" => typeof(RegistryCleanerPage),
                "Settings" => typeof(SettingsPage),
                _ => typeof(DashboardPage)
            };

            ContentFrame.Navigate(pageType);
        }
    }
}
