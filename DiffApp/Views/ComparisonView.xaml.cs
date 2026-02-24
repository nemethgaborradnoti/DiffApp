using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DiffApp.ViewModels;

namespace DiffApp.Views
{
    public partial class ComparisonView : UserControl
    {
        public ComparisonView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ComparisonViewModel oldVm)
            {
                oldVm.ScrollRequested -= OnScrollRequested;
            }

            if (e.NewValue is ComparisonViewModel newVm)
            {
                newVm.ScrollRequested += OnScrollRequested;
            }
        }

        private void OnScrollRequested(object? sender, int index)
        {
            var listView = FindVisualChild<ListView>(this);
            if (listView != null)
            {
                // Fix for virtualization: Use the internal ScrollViewer to scroll by logical item index
                // when CanContentScroll is true (which is default/enabled in XAML).
                var scrollViewer = FindVisualChild<ScrollViewer>(listView);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset(index);
                }
            }
        }

        private void OnRowMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is DiffLineViewModel vm)
            {
                vm.IsBlockHovered = true;
            }
        }

        private void OnRowMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is DiffLineViewModel vm)
            {
                vm.IsBlockHovered = false;
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}