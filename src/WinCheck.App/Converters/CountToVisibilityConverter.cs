using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace WinCheck.App.Converters;

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var count = value is int intValue ? intValue : 0;
        var inverse = parameter as string == "inverse";

        if (inverse)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        return count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
