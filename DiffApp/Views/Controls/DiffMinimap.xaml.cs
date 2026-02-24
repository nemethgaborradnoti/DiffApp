namespace DiffApp.Views.Controls
{
    public partial class DiffMinimap : UserControl
    {
        public DiffMinimap()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<MinimapSegment>), typeof(DiffMinimap), new PropertyMetadata(null));

        public IEnumerable<MinimapSegment> ItemsSource
        {
            get { return (IEnumerable<MinimapSegment>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty TrackColorProperty =
            DependencyProperty.Register("TrackColor", typeof(Brush), typeof(DiffMinimap), new PropertyMetadata(Brushes.Transparent));

        public Brush TrackColor
        {
            get { return (Brush)GetValue(TrackColorProperty); }
            set { SetValue(TrackColorProperty, value); }
        }

        public static readonly DependencyProperty ScrollCommandProperty =
            DependencyProperty.Register("ScrollCommand", typeof(ICommand), typeof(DiffMinimap), new PropertyMetadata(null));

        public ICommand ScrollCommand
        {
            get { return (ICommand)GetValue(ScrollCommandProperty); }
            set { SetValue(ScrollCommandProperty, value); }
        }

        public static readonly DependencyProperty ItemClickCommandProperty =
            DependencyProperty.Register("ItemClickCommand", typeof(ICommand), typeof(DiffMinimap), new PropertyMetadata(null));

        public ICommand ItemClickCommand
        {
            get { return (ICommand)GetValue(ItemClickCommandProperty); }
            set { SetValue(ItemClickCommandProperty, value); }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Handle general track clicking logic if no specific item was captured
            if (!e.Handled && ScrollCommand != null)
            {
                var position = e.GetPosition(this);
                double relativeY = position.Y / ActualHeight;

                relativeY = Math.Max(0, Math.Min(1, relativeY));

                if (ScrollCommand.CanExecute(relativeY))
                {
                    ScrollCommand.Execute(relativeY);
                }
            }
        }
    }
}