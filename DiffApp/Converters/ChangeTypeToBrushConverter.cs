using DiffApp.Models;
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
            if (value is not DiffChangeType type)
            {
                return Brushes.Transparent;
            }

            return type switch
            {
                DiffChangeType.Inserted => (Application.Current.TryFindResource("DiffBackgroundAdded") as Brush) ?? Brushes.Transparent,
                DiffChangeType.Deleted => (Application.Current.TryFindResource("DiffBackgroundRemoved") as Brush) ?? Brushes.Transparent,
                DiffChangeType.Modified => (Application.Current.TryFindResource("DiffBackgroundModified") as Brush) ?? Brushes.Transparent,
                DiffChangeType.Imaginary => (Application.Current.TryFindResource("DiffBackgroundImaginary") as Brush) ?? Brushes.Transparent,
                _ => Brushes.Transparent,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}