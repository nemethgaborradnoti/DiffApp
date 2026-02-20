using DiffApp.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DiffApp.Converters
{
    public class MergeParamsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Expected: values[0] = DiffHunk, values[1] = MergeDirection (Enum)
            if (values.Length == 2 && values[0] is DiffHunk hunk && values[1] is MergeDirection direction)
            {
                return new object[] { hunk, direction };
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}