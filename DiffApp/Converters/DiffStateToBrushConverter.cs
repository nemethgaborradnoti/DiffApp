using System.Globalization;

namespace DiffApp.Converters
{
    public class DiffStateToBrushConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetBrush(value, false, parameter as string);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 1)
                return Brushes.Transparent;

            object item = values[0];
            bool ignoreWhitespace = false;

            if (values.Length > 1 && values[1] is bool b)
            {
                ignoreWhitespace = b;
            }

            return GetBrush(item, ignoreWhitespace, parameter as string);
        }

        private object GetBrush(object item, bool ignoreWhitespace, string? sideOrContext)
        {
            if (item is ChangeBlock block)
            {
                if (ignoreWhitespace && block.IsWhitespaceChange)
                {
                    return Brushes.Transparent;
                }

                if (block.Kind == BlockType.Modified)
                {
                    if (string.Equals(sideOrContext, "Old", StringComparison.OrdinalIgnoreCase))
                    {
                        return GetResourceBrush("DiffBackgroundRemoved");
                    }
                    if (string.Equals(sideOrContext, "New", StringComparison.OrdinalIgnoreCase))
                    {
                        return GetResourceBrush("DiffBackgroundAdded");
                    }
                    return GetResourceBrush("DiffBackgroundModified");
                }

                if (block.Kind == BlockType.Added)
                {
                    if (string.Equals(sideOrContext, "New", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(sideOrContext))
                    {
                        return GetResourceBrush("DiffBackgroundAdded");
                    }
                    else
                    {
                        return GetResourceBrush("DiffBackgroundImaginary");
                    }
                }

                if (block.Kind == BlockType.Removed)
                {
                    if (string.Equals(sideOrContext, "Old", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(sideOrContext))
                    {
                        return GetResourceBrush("DiffBackgroundRemoved");
                    }
                    else
                    {
                        return GetResourceBrush("DiffBackgroundImaginary");
                    }
                }

                return Brushes.Transparent;
            }

            if (item is TextFragment fragment)
            {
                if (ignoreWhitespace && fragment.IsWhitespaceChange)
                {
                    return Brushes.Transparent;
                }

                return fragment.Kind switch
                {
                    DiffChangeType.Inserted => GetResourceBrush("DiffHighlightAdded"),
                    DiffChangeType.Deleted => GetResourceBrush("DiffHighlightRemoved"),
                    _ => Brushes.Transparent
                };
            }

            if (item is DiffChangeType changeType)
            {
                return changeType switch
                {
                    DiffChangeType.Inserted => GetResourceBrush("DiffBackgroundAdded"),
                    DiffChangeType.Deleted => GetResourceBrush("DiffBackgroundRemoved"),
                    DiffChangeType.Modified => GetResourceBrush("DiffBackgroundModified"),
                    DiffChangeType.Imaginary => GetResourceBrush("DiffBackgroundImaginary"),
                    _ => Brushes.Transparent
                };
            }

            if (item is BlockType blockType)
            {
                return blockType switch
                {
                    BlockType.Added when string.Equals(sideOrContext, "New", StringComparison.OrdinalIgnoreCase) => GetResourceBrush("DiffBackgroundAdded"),
                    BlockType.Removed when string.Equals(sideOrContext, "Old", StringComparison.OrdinalIgnoreCase) => GetResourceBrush("DiffBackgroundRemoved"),
                    _ => Brushes.Transparent
                };
            }

            return Brushes.Transparent;
        }

        private Brush GetResourceBrush(string resourceKey)
        {
            return (Application.Current.TryFindResource(resourceKey) as Brush) ?? Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}