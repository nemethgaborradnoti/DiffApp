namespace DiffApp.Views
{
    public partial class CustomDialogWindow : Window
    {
        public CustomDialogWindow()
        {
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
    }
}