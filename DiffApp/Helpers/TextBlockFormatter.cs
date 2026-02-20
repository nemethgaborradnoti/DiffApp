using DiffApp.Models;
using DiffPlex.DiffBuilder.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

// Alias létrehozása a névütközés feloldására
// Így a "DiffPiece" ebben a fájlban a mi modellünkre (DiffApp.Models.DiffPiece) utal.
using DiffPiece = DiffApp.Models.DiffPiece;

namespace DiffApp.Helpers
{
    public static class TextBlockFormatter
    {
        public static readonly DependencyProperty PiecesProperty =
            DependencyProperty.RegisterAttached(
                "Pieces",
                typeof(IEnumerable<DiffPiece>),
                typeof(TextBlockFormatter),
                new PropertyMetadata(null, OnPiecesChanged));

        public static IEnumerable<DiffPiece> GetPieces(DependencyObject obj)
        {
            return (IEnumerable<DiffPiece>)obj.GetValue(PiecesProperty);
        }

        public static void SetPieces(DependencyObject obj, IEnumerable<DiffPiece> value)
        {
            obj.SetValue(PiecesProperty, value);
        }

        private static void OnPiecesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock && e.NewValue is IEnumerable<DiffPiece> pieces)
            {
                textBlock.Inlines.Clear();

                foreach (var piece in pieces)
                {
                    var run = new Run(piece.Text);

                    // Apply background color based on ChangeType using resources
                    var brush = GetBrushForChangeType(piece.Kind);
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