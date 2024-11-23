using System.Globalization;

namespace LMKit.Maestro.Converters;

internal sealed class EqualToZeroConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null && value is int integer)
        {
            return integer == 0;
        }

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}