using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WinCheck.App.ViewModels;

namespace WinCheck.App.Views;

public sealed partial class ProcessMonitorPage : Page
{
    public ProcessMonitorViewModel ViewModel { get; }

    public ProcessMonitorPage()
    {
        ViewModel = App.Services.GetRequiredService<ProcessMonitorViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
