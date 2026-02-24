using System.Globalization;

namespace DiffApp.Converters
{
    public class UniversalVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = false;

            if (value is bool booleanValue)
            {
                isVisible = booleanValue;
            }
            else if (value != null)
            {
                isVisible = true;
            }

            if (parameter is string paramString && paramString.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}