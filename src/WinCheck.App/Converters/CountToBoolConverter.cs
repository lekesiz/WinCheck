using Microsoft.UI.Xaml.Data;

namespace WinCheck.App.Converters;

public class CountToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var count = value is int intValue ? intValue : 0;
        return count > 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
