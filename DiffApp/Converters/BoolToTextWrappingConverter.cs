using System.Globalization;

namespace DiffApp.Converters
{
    public class BoolToTextWrappingConverter : IValueConverter
    {
        public static BoolToTextWrappingConverter Instance { get; } = new BoolToTextWrappingConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isWrapped && isWrapped)
            {
                return TextWrapping.Wrap;
            }
            return TextWrapping.NoWrap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}