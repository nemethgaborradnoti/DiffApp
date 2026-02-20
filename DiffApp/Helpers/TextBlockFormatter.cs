using DiffApp.Models;
using DiffPlex.DiffBuilder.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DiffApp.Helpers
{
    public static class TextBlockFormatter
    {
        public static readonly DependencyProperty FragmentsProperty =
            DependencyProperty.RegisterAttached(
                "Fragments",
                typeof(IEnumerable<TextFragment>),
                typeof(TextBlockFormatter),
                new PropertyMetadata(null, OnFragmentsChanged));

        public static IEnumerable<TextFragment> GetFragments(DependencyObject obj)
        {
            return (IEnumerable<TextFragment>)obj.GetValue(FragmentsProperty);
        }

        public static void SetFragments(DependencyObject obj, IEnumerable<TextFragment> value)
        {
            obj.SetValue(FragmentsProperty, value);
        }

        private static void OnFragmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock && e.NewValue is IEnumerable<TextFragment> fragments)
            {
                textBlock.Inlines.Clear();

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
        }

        private static Brush? GetBrushForChangeType(ChangeType type)
        {
            string resourceKey = type switch
            {
                ChangeType.Inserted => "DiffHighlightAdded",
                ChangeType.Deleted => "DiffHighlightRemoved",
                _ => "DiffHighlightUnchanged"
            };

            return Application.Current.TryFindResource(resourceKey) as Brush;
        }
    }
}