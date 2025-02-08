using System.Globalization;
using LMKit.Maestro.Helpers;

namespace LMKit.Maestro.Converters;

internal sealed class FileSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null)
        {
            Type type = value.GetType();

            if (value is long bytes)
            {
                return FileHelpers.FormatFileSize(bytes);
            }
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
