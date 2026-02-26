using System.ComponentModel;

namespace DiffApp
{
    public partial class MainWindow : Window
    {
        private readonly ISettingsService _settingsService;

        public MainWindow(MainViewModel viewModel, ISettingsService settingsService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var settings = _settingsService.LoadSettings();

            if (!double.IsNaN(settings.WindowTop))
            {
                Top = settings.WindowTop;
                Left = settings.WindowLeft;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (settings.WindowWidth > 0 && settings.WindowHeight > 0)
            {
                Width = settings.WindowWidth;
                Height = settings.WindowHeight;
            }

            if (settings.WindowState != WindowState.Minimized)
            {
                WindowState = settings.WindowState;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var settings = _settingsService.LoadSettings();

            settings.WindowState = WindowState;

            if (WindowState == WindowState.Normal)
            {
                settings.WindowTop = Top;
                settings.WindowLeft = Left;
                settings.WindowWidth = Width;
                settings.WindowHeight = Height;
            }

            _settingsService.SaveSettings(settings);
        }
    }
}