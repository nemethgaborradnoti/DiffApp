using System.Globalization;

namespace DiffApp.Converters
{
    public class SmartBlockBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is not ChangeBlock block || values[1] is not bool ignoreWhitespace)
            {
                return Brushes.Transparent;
            }

            if (ignoreWhitespace && block.IsWhitespaceChange)
            {
                return Brushes.Transparent;
            }

            string side = parameter as string ?? string.Empty;

            if (block.Kind == BlockType.Modified)
            {
                if (side.Equals("Old", StringComparison.OrdinalIgnoreCase))
                {
                    return (Application.Current.TryFindResource("DiffBackgroundRemoved") as Brush) ?? Brushes.Transparent;
                }
                if (side.Equals("New", StringComparison.OrdinalIgnoreCase))
                {
                    return (Application.Current.TryFindResource("DiffBackgroundAdded") as Brush) ?? Brushes.Transparent;
                }
            }

            return block.Kind switch
            {
                BlockType.Added when side.Equals("New", StringComparison.OrdinalIgnoreCase) => (Application.Current.TryFindResource("DiffBackgroundAdded") as Brush) ?? Brushes.Transparent,
                BlockType.Removed when side.Equals("Old", StringComparison.OrdinalIgnoreCase) => (Application.Current.TryFindResource("DiffBackgroundRemoved") as Brush) ?? Brushes.Transparent,
                _ => Brushes.Transparent,
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}