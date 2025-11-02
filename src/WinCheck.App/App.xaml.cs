using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System;
using WinCheck.App.ViewModels;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Services.AI;
using WinCheck.Infrastructure.AI;
using WinCheck.Infrastructure.Logging;
using WinCheck.Infrastructure.Services;

namespace WinCheck.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Window MainWindow { get; private set; } = null!;

    public App()
    {
        InitializeComponent();

        // Clean old logs on startup
        FileLogger.CleanOldLogs();

        Services = ConfigureServices();

        // Log application start
        var logger = Services.GetRequiredService<Core.Interfaces.ILogger>();
        logger.LogInformation("=== WinCheck Application Started ===");
        logger.LogInformation($"Version: 1.0.0");
        logger.LogInformation($"OS: {Environment.OSVersion}");
        logger.LogInformation($".NET: {Environment.Version}");
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // ViewModels - ALL 7 ViewModels registered
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProcessMonitorViewModel>();
        services.AddTransient<ServiceOptimizerViewModel>();
        services.AddTransient<DiskCleanupViewModel>();
        services.AddTransient<StartupManagerViewModel>();
        services.AddTransient<RegistryCleanerViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Core Services - ALL 9 Services registered
        services.AddSingleton<IProcessMonitorService, ProcessMonitorService>();
        services.AddSingleton<INetworkMonitorService, NetworkMonitorService>();
        services.AddSingleton<IHardwareDetectionService, HardwareDetectionService>();
        services.AddSingleton<IOSDetectionService, OSDetectionService>();
        services.AddSingleton<IServiceOptimizerService, ServiceOptimizerService>();
        services.AddSingleton<IDiskCleanupService, DiskCleanupService>();
        services.AddSingleton<IRegistryCleanerService, RegistryCleanerService>();
        services.AddSingleton<IStartupManagerService, StartupManagerService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        // AI Providers - Lazy loading with default
        services.AddSingleton<IAIProvider>(sp =>
        {
            // Start with default OpenAI provider with empty key
            // Will be configured later from settings
            return new OpenAIProvider(string.Empty);
        });

        // AI System Analyzer (Crown Jewel - Orchestrates all services)
        services.AddSingleton<IAISystemAnalyzer, AISystemAnalyzer>();

        // Logging - Custom file logger
        services.AddSingleton<Core.Interfaces.ILogger>(new FileLogger(Infrastructure.Logging.LogLevel.Information));

        return services.BuildServiceProvider();
    }
}
