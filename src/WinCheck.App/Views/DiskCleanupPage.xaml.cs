using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WinCheck.App.ViewModels;

namespace WinCheck.App.Views;

public sealed partial class DiskCleanupPage : Page
{
    public DiskCleanupViewModel ViewModel { get; }

    public DiskCleanupPage()
    {
        ViewModel = App.Services.GetRequiredService<DiskCleanupViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
