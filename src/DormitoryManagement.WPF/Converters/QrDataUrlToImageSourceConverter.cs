using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DormitoryManagement.WPF.Converters;

public sealed class QrDataUrlToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string source || string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        if (!source.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            return new BitmapImage(new Uri(source, UriKind.Absolute));
        }

        var commaIndex = source.IndexOf(',');
        if (commaIndex < 0)
        {
            return null;
        }

        var bytes = System.Convert.FromBase64String(source[(commaIndex + 1)..]);
        using var stream = new MemoryStream(bytes);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
