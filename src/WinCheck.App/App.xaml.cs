using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using WinCheck.App.ViewModels;
using WinCheck.App.Views;
using WinCheck.Core.Interfaces;

namespace WinCheck.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Window MainWindow { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // ViewModels
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProcessMonitorViewModel>();
        services.AddTransient<ServiceOptimizerViewModel>();
        services.AddTransient<DiskCleanupViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Services - Infrastructure projeden gelecek
        // services.AddSingleton<IProcessMonitorService, ProcessMonitorService>();
        // services.AddSingleton<IServiceOptimizerService, ServiceOptimizerService>();

        // Logging
        services.AddLogging();

        return services.BuildServiceProvider();
    }
}
