using DiffPlex.DiffBuilder.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DiffApp.Converters
{
    public class ChangeTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ChangeType type)
            {
                return Brushes.Transparent;
            }

            return type switch
            {
                ChangeType.Inserted => Application.Current.TryFindResource("DiffBackgroundAdded") as Brush,
                ChangeType.Deleted => Application.Current.TryFindResource("DiffBackgroundRemoved") as Brush,
                ChangeType.Modified => Application.Current.TryFindResource("DiffBackgroundModified") as Brush,
                ChangeType.Imaginary => Application.Current.TryFindResource("DiffBackgroundImaginary") as Brush,
                _ => Brushes.Transparent,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
