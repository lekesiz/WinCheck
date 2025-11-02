using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WinCheck.App.ViewModels;

namespace WinCheck.App.Views;

public sealed partial class RegistryCleanerPage : Page
{
    public RegistryCleanerViewModel ViewModel { get; }

    public RegistryCleanerPage()
    {
        ViewModel = App.Services.GetRequiredService<RegistryCleanerViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
