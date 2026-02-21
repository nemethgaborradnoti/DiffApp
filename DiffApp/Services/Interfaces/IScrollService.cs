using System.Windows;
using System.Windows.Controls;

namespace DiffApp.Services.Interfaces
{
    public interface IScrollService
    {
        void ScrollToTop();
        void ScrollToInput();
        void RegisterTargets(ScrollViewer scrollViewer, FrameworkElement inputElement);
    }
}