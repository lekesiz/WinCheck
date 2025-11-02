using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using WinCheck.App.ViewModels;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Services.AI;
using WinCheck.Infrastructure.AI;
using WinCheck.Infrastructure.Services;

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

        // Core Services
        services.AddSingleton<IProcessMonitorService, ProcessMonitorService>();
        services.AddSingleton<INetworkMonitorService, NetworkMonitorService>();
        services.AddSingleton<IHardwareDetectionService, HardwareDetectionService>();
        services.AddSingleton<IOSDetectionService, OSDetectionService>();
        services.AddSingleton<IServiceOptimizerService, ServiceOptimizerService>();
        services.AddSingleton<IDiskCleanupService, DiskCleanupService>();
        services.AddSingleton<IRegistryCleanerService, RegistryCleanerService>();
        services.AddSingleton<IStartupManagerService, StartupManagerService>();

        // AI Providers (Load from settings - for now use placeholder)
        // In production, these should be loaded from user settings
        services.AddSingleton<IAIProvider>(sp =>
        {
            // Default to OpenAI - user can change in settings
            var apiKey = ""; // Will be loaded from settings
            return new OpenAIProvider(apiKey);
        });

        // AI System Analyzer (Crown Jewel)
        services.AddSingleton<IAISystemAnalyzer, AISystemAnalyzer>();

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddDebug();
            builder.AddConsole();
        });

        return services.BuildServiceProvider();
    }
}
