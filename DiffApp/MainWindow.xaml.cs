namespace DiffApp
{
    public partial class MainWindow : Window
    {
        private readonly IScrollService _scrollService;

        public MainWindow(MainViewModel viewModel, IScrollService scrollService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _scrollService = scrollService;

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _scrollService.RegisterTargets(MainScrollViewer, InputSection);
        }
    }
}