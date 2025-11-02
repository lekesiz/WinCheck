using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WinCheck.App.ViewModels;

namespace WinCheck.App.Views;

public sealed partial class ServiceOptimizerPage : Page
{
    public ServiceOptimizerViewModel ViewModel { get; }

    public ServiceOptimizerPage()
    {
        ViewModel = App.Services.GetRequiredService<ServiceOptimizerViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
