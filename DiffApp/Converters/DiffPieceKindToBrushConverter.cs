using DiffPlex.DiffBuilder.Model;
using System.Globalization;

namespace DiffApp.Converters
{
    public class DiffPieceKindToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ChangeType type)
            {
                return (Application.Current.TryFindResource("DiffHighlightUnchanged") as Brush) ?? Brushes.Transparent;
            }

            return type switch
            {
                ChangeType.Inserted => (Application.Current.TryFindResource("DiffHighlightAdded") as Brush) ?? Brushes.Transparent,
                ChangeType.Deleted => (Application.Current.TryFindResource("DiffHighlightRemoved") as Brush) ?? Brushes.Transparent,
                _ => (Application.Current.TryFindResource("DiffHighlightUnchanged") as Brush) ?? Brushes.Transparent,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}