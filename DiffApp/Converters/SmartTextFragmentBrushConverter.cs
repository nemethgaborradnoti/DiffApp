using DiffApp.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DiffApp.Converters
{
    public class SmartTextFragmentBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is not TextFragment fragment || values[1] is not bool ignoreWhitespace)
            {
                return Brushes.Transparent;
            }

            if (ignoreWhitespace && fragment.IsWhitespaceChange)
            {
                return Brushes.Transparent;
            }

            return fragment.Kind switch
            {
                DiffChangeType.Inserted => (Application.Current.TryFindResource("DiffHighlightAdded") as Brush) ?? Brushes.Transparent,
                DiffChangeType.Deleted => (Application.Current.TryFindResource("DiffHighlightRemoved") as Brush) ?? Brushes.Transparent,
                _ => Brushes.Transparent,
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}