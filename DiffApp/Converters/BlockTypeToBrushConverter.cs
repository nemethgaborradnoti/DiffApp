using System.Globalization;

namespace DiffApp.Converters
{
    public class BlockTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not BlockType kind)
            {
                return Brushes.Transparent;
            }

            string side = parameter as string ?? string.Empty;

            return kind switch
            {
                BlockType.Added when side.Equals("New", StringComparison.OrdinalIgnoreCase) => (Application.Current.TryFindResource("DiffBackgroundAdded") as Brush) ?? Brushes.Transparent,
                BlockType.Removed when side.Equals("Old", StringComparison.OrdinalIgnoreCase) => (Application.Current.TryFindResource("DiffBackgroundRemoved") as Brush) ?? Brushes.Transparent,
                _ => Brushes.Transparent,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}