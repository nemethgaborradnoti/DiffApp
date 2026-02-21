using DiffApp.Services.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiffApp.Services
{
    public class ScrollService : IScrollService
    {
        private ScrollViewer? _scrollViewer;
        private FrameworkElement? _inputElement;

        public void RegisterTargets(ScrollViewer scrollViewer, FrameworkElement inputElement)
        {
            _scrollViewer = scrollViewer;
            _inputElement = inputElement;
        }

        public void ScrollToTop()
        {
            _scrollViewer?.ScrollToTop();
        }

        public void ScrollToInput()
        {
            if (_scrollViewer != null && _inputElement != null)
            {
                try
                {
                    GeneralTransform transform = _inputElement.TransformToAncestor(_scrollViewer);
                    Point topPosition = transform.Transform(new Point(0, 0));

                    _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + topPosition.Y);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}