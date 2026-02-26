using System.Globalization;

namespace DiffApp.Converters
{
    public class RelativeSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double percentage && values[1] is double totalSize)
            {
                if (double.IsNaN(percentage) || double.IsInfinity(percentage)) return 0.0;
                if (double.IsNaN(totalSize) || double.IsInfinity(totalSize)) return 0.0;

                return percentage * totalSize;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}