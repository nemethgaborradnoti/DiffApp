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

        public static readonly DependencyProperty AreHighlightsEnabledProperty =
           DependencyProperty.RegisterAttached(
               "AreHighlightsEnabled",
               typeof(bool),
               typeof(TextBlockFormatter),
               new PropertyMetadata(true, OnPropertyChanged));

        public static readonly DependencyProperty IgnoreWhitespaceProperty =
           DependencyProperty.RegisterAttached(
               "IgnoreWhitespace",
               typeof(bool),
               typeof(TextBlockFormatter),
               new PropertyMetadata(false, OnPropertyChanged));

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

        public static bool GetAreHighlightsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(AreHighlightsEnabledProperty);
        }

        public static void SetAreHighlightsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(AreHighlightsEnabledProperty, value);
        }

        public static bool GetIgnoreWhitespace(DependencyObject obj)
        {
            return (bool)obj.GetValue(IgnoreWhitespaceProperty);
        }

        public static void SetIgnoreWhitespace(DependencyObject obj, bool value)
        {
            obj.SetValue(IgnoreWhitespaceProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                var fragments = GetFragments(textBlock);
                var precision = GetPrecision(textBlock);
                var enabled = GetAreHighlightsEnabled(textBlock);
                var ignoreWhitespace = GetIgnoreWhitespace(textBlock);

                Render(textBlock, fragments, precision, enabled, ignoreWhitespace);
            }
        }

        private static void Render(TextBlock textBlock, IEnumerable<TextFragment> fragments, PrecisionLevel precision, bool enabled, bool ignoreWhitespace)
        {
            if (fragments == null)
            {
                textBlock.Text = string.Empty;
                return;
            }

            // OPTIMIZATION: If highlights are disabled or no fragments, use simple Text property.
            if (!enabled || !fragments.Any())
            {
                var sb = new StringBuilder();
                foreach (var f in fragments)
                {
                    sb.Append(f.Text);
                }
                textBlock.Text = sb.ToString();
                return;
            }

            // Clear text before adding inlines
            textBlock.Text = null;
            textBlock.Inlines.Clear();

            if (precision == PrecisionLevel.Character)
            {
                RenderCharacterPrecision(textBlock, fragments, enabled, ignoreWhitespace);
            }
            else
            {
                RenderWordPrecision(textBlock, fragments, enabled, ignoreWhitespace);
            }
        }

        private static void RenderCharacterPrecision(TextBlock textBlock, IEnumerable<TextFragment> fragments, bool enabled, bool ignoreWhitespace)
        {
            foreach (var fragment in fragments)
            {
                var run = new Run(fragment.Text);
                if (enabled)
                {
                    // Apply color only if we are NOT ignoring whitespace, OR if it's NOT a whitespace change
                    if (!ignoreWhitespace || !fragment.IsWhitespaceChange)
                    {
                        var brush = GetBrushForChangeType(fragment.Kind);
                        if (brush != null)
                        {
                            run.Background = brush;
                        }
                    }
                }
                textBlock.Inlines.Add(run);
            }
        }

        private static void RenderWordPrecision(TextBlock textBlock, IEnumerable<TextFragment> fragments, bool enabled, bool ignoreWhitespace)
        {
            var wordBuffer = new List<(char Char, DiffChangeType Kind, bool IsWhitespaceChange)>();

            foreach (var fragment in fragments)
            {
                foreach (char c in fragment.Text)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        FlushWordBuffer(textBlock, wordBuffer, enabled, ignoreWhitespace);
                        textBlock.Inlines.Add(new Run(c.ToString()));
                    }
                    else
                    {
                        wordBuffer.Add((c, fragment.Kind, fragment.IsWhitespaceChange));
                    }
                }
            }
            FlushWordBuffer(textBlock, wordBuffer, enabled, ignoreWhitespace);
        }

        private static void FlushWordBuffer(TextBlock textBlock, List<(char Char, DiffChangeType Kind, bool IsWhitespaceChange)> buffer, bool enabled, bool ignoreWhitespace)
        {
            if (buffer.Count == 0) return;

            bool isModified = buffer.Any(x => x.Kind != DiffChangeType.Unchanged);

            DiffChangeType dominantKind = DiffChangeType.Unchanged;
            bool isEffectiveWhitespaceChange = false;

            if (isModified)
            {
                var changedItem = buffer.FirstOrDefault(x => x.Kind != DiffChangeType.Unchanged);
                dominantKind = changedItem.Kind;
                isEffectiveWhitespaceChange = changedItem.IsWhitespaceChange;
            }

            var text = new string(buffer.Select(x => x.Char).ToArray());
            var run = new Run(text);

            if (enabled && dominantKind != DiffChangeType.Unchanged)
            {
                if (!ignoreWhitespace || !isEffectiveWhitespaceChange)
                {
                    run.Background = GetBrushForChangeType(dominantKind);
                }
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