using System.Globalization;

namespace LMKit.Maestro.Converters
{
    public class LevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float floatValue)
            {
                int h = (int)(120 * Math.Pow(floatValue, 2));
                int s = 100;
                int v = 100;

                return Color.FromHsv(h, s, v);
            }

            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
