using System.Text;
using System.Windows.Documents;

namespace DiffApp.Helpers
{
    public static class TextBlockFormatter
    {
        public static readonly DependencyProperty FragmentsProperty =
            DependencyProperty.RegisterAttached(
                "Fragments",
                typeof(IEnumerable<TextFragment>),
                typeof(TextBlockFormatter),
                new PropertyMetadata(null, OnPropertyChanged));

        public static readonly DependencyProperty PrecisionProperty =
            DependencyProperty.RegisterAttached(
                "Precision",
                typeof(PrecisionLevel),
                typeof(TextBlockFormatter),
                new PropertyMetadata(PrecisionLevel.Word, OnPropertyChanged));

        public static IEnumerable<TextFragment> GetFragments(DependencyObject obj)
        {
            return (IEnumerable<TextFragment>)obj.GetValue(FragmentsProperty);
        }

        public static void SetFragments(DependencyObject obj, IEnumerable<TextFragment> value)
        {
            obj.SetValue(FragmentsProperty, value);
        }

        public static PrecisionLevel GetPrecision(DependencyObject obj)
        {
            return (PrecisionLevel)obj.GetValue(PrecisionProperty);
        }

        public static void SetPrecision(DependencyObject obj, PrecisionLevel value)
        {
            obj.SetValue(PrecisionProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                var fragments = GetFragments(textBlock);
                var precision = GetPrecision(textBlock);

                Render(textBlock, fragments, precision);
            }
        }

        private static void Render(TextBlock textBlock, IEnumerable<TextFragment> fragments, PrecisionLevel precision)
        {
            textBlock.Inlines.Clear();

            if (fragments == null) return;

            if (precision == PrecisionLevel.Character)
            {
                RenderCharacterPrecision(textBlock, fragments);
            }
            else
            {
                RenderWordPrecision(textBlock, fragments);
            }
        }

        private static void RenderCharacterPrecision(TextBlock textBlock, IEnumerable<TextFragment> fragments)
        {
            foreach (var fragment in fragments)
            {
                var run = new Run(fragment.Text);
                var brush = GetBrushForChangeType(fragment.Kind);
                if (brush != null)
                {
                    run.Background = brush;
                }
                textBlock.Inlines.Add(run);
            }
        }

        private static void RenderWordPrecision(TextBlock textBlock, IEnumerable<TextFragment> fragments)
        {
            var wordBuffer = new List<(char Char, DiffChangeType Kind)>();

            foreach (var fragment in fragments)
            {
                foreach (char c in fragment.Text)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        FlushWordBuffer(textBlock, wordBuffer);
                        textBlock.Inlines.Add(new Run(c.ToString()));
                    }
                    else
                    {
                        wordBuffer.Add((c, fragment.Kind));
                    }
                }
            }
            FlushWordBuffer(textBlock, wordBuffer);
        }

        private static void FlushWordBuffer(TextBlock textBlock, List<(char Char, DiffChangeType Kind)> buffer)
        {
            if (buffer.Count == 0) return;

            bool isModified = buffer.Any(x => x.Kind != DiffChangeType.Unchanged);

            DiffChangeType dominantKind = DiffChangeType.Unchanged;
            if (isModified)
            {
                var changedItem = buffer.FirstOrDefault(x => x.Kind != DiffChangeType.Unchanged);
                dominantKind = changedItem.Kind;
            }

            var text = new string(buffer.Select(x => x.Char).ToArray());
            var run = new Run(text);

            if (dominantKind != DiffChangeType.Unchanged)
            {
                run.Background = GetBrushForChangeType(dominantKind);
            }

            textBlock.Inlines.Add(run);
            buffer.Clear();
        }

        private static Brush? GetBrushForChangeType(DiffChangeType type)
        {
            string resourceKey = type switch
            {
                DiffChangeType.Inserted => "DiffHighlightAdded",
                DiffChangeType.Deleted => "DiffHighlightRemoved",
                _ => "DiffHighlightUnchanged"
            };

            return Application.Current.TryFindResource(resourceKey) as Brush;
        }
    }
}