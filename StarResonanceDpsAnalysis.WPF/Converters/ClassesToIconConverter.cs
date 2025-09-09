using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StarResonanceDpsAnalysis.WPF.Converters;

internal class ClassesToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return null;

        // First try to find a resource keyed by the enum value itself (we added enum-keyed resources in Classes.xaml)
        var application = Application.Current;
        if (application != null)
        {
            var res = application.TryFindResource(value);
            if (res is ImageSource imageSource) return imageSource;
            if (res is BitmapImage bitmapImage) return bitmapImage;
        }

        // Fallback: try the string key pattern "Classes{EnumName}Icon" to keep compatibility with existing keys
        var key = $"Classes{value}Icon";
        if (application != null)
        {
            var res2 = application.TryFindResource(key);
            if (res2 is ImageSource imageSource2) return imageSource2;
            if (res2 is BitmapImage bitmapImage2) return bitmapImage2;
        }

        // No resource found
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ClassesToIconConverter does not support ConvertBack.");
    }
}