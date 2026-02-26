using System.Globalization;

namespace DiffApp.Converters
{
    public class MinimapBrushConverter : IMultiValueConverter
    {
        private static readonly Brush MinimapGreen = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8fe3c7"));
        private static readonly Brush MinimapRed = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f5a4a4"));

        static MinimapBrushConverter()
        {
            if (MinimapGreen.CanFreeze) MinimapGreen.Freeze();
            if (MinimapRed.CanFreeze) MinimapRed.Freeze();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Brushes.Transparent;

            var segment = values[0] as MinimapSegment;
            if (segment == null)
                return Brushes.Transparent;

            bool ignoreWhitespace = false;
            if (values[1] is bool b)
            {
                ignoreWhitespace = b;
            }

            if (ignoreWhitespace && segment.Block != null && segment.Block.IsWhitespaceChange)
            {
                return Brushes.Transparent;
            }

            string side = parameter as string ?? string.Empty;
            BlockType typeToCheck = BlockType.Unchanged;

            if (string.Equals(side, "Left", StringComparison.OrdinalIgnoreCase))
            {
                typeToCheck = segment.LeftType;
            }
            else if (string.Equals(side, "Right", StringComparison.OrdinalIgnoreCase))
            {
                typeToCheck = segment.RightType;
            }

            return typeToCheck switch
            {
                BlockType.Added => MinimapGreen,
                BlockType.Removed => MinimapRed,
                BlockType.Modified => string.Equals(side, "Left", StringComparison.OrdinalIgnoreCase) ? MinimapRed : MinimapGreen,
                _ => Brushes.Transparent,
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}