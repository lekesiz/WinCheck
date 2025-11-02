using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
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
}
