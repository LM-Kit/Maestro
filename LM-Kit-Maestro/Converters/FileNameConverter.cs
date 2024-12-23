using LMKit.Model;
using System.Globalization;

namespace LMKit.Maestro.Converters;

internal sealed class FileNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null)
        {
            if (value is Uri uri && uri.IsFile)
            {
                return Path.GetFileName(uri.LocalPath);
            }
            else if (value is ModelCard modelCard)
            {
                return Path.GetFileName(modelCard.LocalPath);
            }
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
