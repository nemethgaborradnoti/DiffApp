using System.Globalization;

namespace DiffApp.Converters
{
    public class MinimapBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3)
                return Brushes.Transparent;

            if (values[0] is not BlockType kind)
                return Brushes.Transparent;

            bool ignoreWhitespace = values[1] is bool b && b;
            var block = values[2] as ChangeBlock;

            if (ignoreWhitespace && block != null && block.IsWhitespaceChange)
            {
                return Brushes.Transparent;
            }

            return kind switch
            {
                BlockType.Added => (Application.Current.TryFindResource("SuccessBrush") as Brush) ?? Brushes.Green,
                BlockType.Removed => (Application.Current.TryFindResource("DangerBrush") as Brush) ?? Brushes.Red,
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