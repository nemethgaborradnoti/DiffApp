using DiffApp.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DiffApp.Converters
{
    public class HunkKindToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not HunkKind kind)
            {
                return Brushes.Transparent;
            }

            string side = parameter as string ?? string.Empty;

            return kind switch
            {
                HunkKind.Added when side.Equals("New", StringComparison.OrdinalIgnoreCase) => (Application.Current.TryFindResource("DiffBackgroundAdded") as Brush) ?? Brushes.Transparent,
                HunkKind.Removed when side.Equals("Old", StringComparison.OrdinalIgnoreCase) => (Application.Current.TryFindResource("DiffBackgroundRemoved") as Brush) ?? Brushes.Transparent,
                _ => Brushes.Transparent,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
