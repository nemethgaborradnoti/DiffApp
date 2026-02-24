using System.Globalization;

namespace DiffApp.Converters
{
    public class MinimapBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not BlockType kind)
            {
                return Brushes.Transparent;
            }

            return kind switch
            {
                BlockType.Added => (Application.Current.TryFindResource("SuccessBrush") as Brush) ?? Brushes.Green,
                BlockType.Removed => (Application.Current.TryFindResource("DangerBrush") as Brush) ?? Brushes.Red,
                // BlockType.Modified should not appear in the minimap anymore as it is split into Added/Removed
                _ => Brushes.Transparent,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}