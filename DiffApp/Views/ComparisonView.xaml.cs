using System.Windows.Controls;
using System.Windows.Input;
using DiffApp.ViewModels;

namespace DiffApp.Views
{
    public partial class ComparisonView : UserControl
    {
        public ComparisonView()
        {
            InitializeComponent();
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
    }
}