using DiffApp.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DiffApp.Converters
{
    public class MinimapBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Expected values:
            // 0: BlockType (enum)
            // 1: IgnoreWhitespace (bool)
            // 2: ChangeBlock (object)

            if (values.Length < 3)
                return Brushes.Transparent;

            if (values[0] is not BlockType kind)
                return Brushes.Transparent;

            bool ignoreWhitespace = values[1] is bool b && b;
            var block = values[2] as ChangeBlock;

            // If filtering is ON and the block is whitespace-only -> Hide it
            if (ignoreWhitespace && block != null && block.IsWhitespaceChange)
            {
                return Brushes.Transparent;
            }

            return kind switch
            {
                BlockType.Added => (Application.Current.TryFindResource("SuccessBrush") as Brush) ?? Brushes.Green,
                BlockType.Removed => (Application.Current.TryFindResource("DangerBrush") as Brush) ?? Brushes.Red,
                // For minimap, Modified is typically split into Added/Removed segments in the ViewModel calculation
                // but if a raw Modified block comes in:
                BlockType.Modified => Brushes.Orange,
                _ => Brushes.Transparent,
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}