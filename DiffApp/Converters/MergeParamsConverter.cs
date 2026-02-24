using System.Globalization;

namespace DiffApp.Converters
{
    public class MergeParamsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is ChangeBlock block && values[1] is MergeDirection direction)
            {
                return new object[] { block, direction };
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}