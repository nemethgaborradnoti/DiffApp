using System.Globalization;

namespace DiffApp.Converters
{
    public class DiffStateToBrushConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetBrush(value, false, parameter as string, null);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 1)
                return Brushes.Transparent;

            object item = values[0];
            bool ignoreWhitespace = false;
            ChangeLine? line = null;

            if (values.Length > 1 && values[1] is bool b)
            {
                ignoreWhitespace = b;
            }

            if (values.Length > 2)
            {
                line = values[2] as ChangeLine;
            }

            return GetBrush(item, ignoreWhitespace, parameter as string, line);
        }

        private object GetBrush(object item, bool ignoreWhitespace, string? sideOrContext, ChangeLine? line)
        {
            // 1. Line-level specific overrides (e.g. Imaginary/Gap lines inside a Modified block)
            // If the specific line is null, it means it's a padding line in the VirtualDiffLineList -> Imaginary (Gray).
            // If the specific line exists but is marked Imaginary -> Imaginary (Gray).
            // We only apply this check if 'line' was actually passed (to avoid breaking other bindings).
            if (line != null)
            {
                if (line.Kind == DiffChangeType.Imaginary)
                {
                    return GetResourceBrush("DiffBackgroundImaginary");
                }
            }
            // If line is null but we expected one (implicit in the fact that we are in a block), 
            // the VirtualDiffLineList might pass null for out-of-bounds lines.
            // However, the binding in XAML passes {Binding LeftLine}, which is null.
            // We need to distinguish "Binding passed null" vs "Binding not present".
            // Since we updated the XAML to pass 3 arguments, if the 3rd is null, it's a gap.
            // But checking 'values.Length > 2' handles "Binding present".
            // So if line is null here, and we are in a context where we passed it, it's a gap.
            // CAUTION: The 'item' (Block) check below handles general block colors. 
            // We should only force Imaginary if we are sure it's a gap.
            // In the context of SideBySideRowTemplate, we will pass the line. If it's null, it's a gap.

            // To be safe, we check if the caller passed a 3rd argument (in Convert), but here 'line' is just the value.
            // We'll rely on the line.Kind check above for explicit Imaginary lines.
            // For null lines (gaps), we handle it inside the Block logic if needed, 
            // OR we assume that if the user passed a line binding and it resolved to null, it's a gap.
            // But we can't easily distinguish "null passed" vs "not passed" in this helper method without extra flags.
            // Let's rely on the Block logic + explicit Imaginary lines for now. 
            // *Self-Correction*: If the block is Modified, but the line is null (gap), we want Gray. 
            // If we don't handle it, it returns Block color (Red/Green).
            // We need to know if the binding target is a Line. 
            // Let's assume if the line parameter is provided (even if null), we treat it as significant.
            // But distinguishing null here is hard. 
            // Let's stick to: if (line != null && line.Kind == Imaginary) -> Gray.
            // And ensure ComparisonService creates Imaginary lines for gaps instead of nulls. 
            // (ComparisonService does map DiffPlex Imaginary to DiffChangeType.Imaginary).

            if (item is ChangeBlock block)
            {
                if (ignoreWhitespace && block.IsWhitespaceChange)
                {
                    return Brushes.Transparent;
                }

                if (block.Kind == BlockType.Modified)
                {
                    // Check for gap (null line passed from View)
                    // If we are rendering a line in a Modified block, and the line is missing (null) or Imaginary, return Gray.
                    if (line != null && line.Kind == DiffChangeType.Imaginary)
                    {
                        return GetResourceBrush("DiffBackgroundImaginary");
                    }
                    // Note: If line is null (not just Imaginary kind), we might want Gray too. 
                    // But in strict C# nullable checks, 'line' is null if the binding source is null.

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