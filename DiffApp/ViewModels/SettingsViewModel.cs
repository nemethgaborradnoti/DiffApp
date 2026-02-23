namespace DiffApp.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private bool _isWordWrapEnabled;
        private bool _ignoreWhitespace;
        private PrecisionLevel _precision;
        private ViewMode _viewMode;
        private double _fontSize;

        public event EventHandler? SettingsChanged;

        public bool IsWordWrapEnabled
        {
            get => _isWordWrapEnabled;
            set
            {
                if (SetProperty(ref _isWordWrapEnabled, value))
                {
                    SaveSettings();
                    SettingsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IgnoreWhitespace
        {
            get => _ignoreWhitespace;
            set
            {
                if (SetProperty(ref _ignoreWhitespace, value))
                {
                    SaveSettings();
                    SettingsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public PrecisionLevel Precision
        {
            get => _precision;
            set
            {
                if (SetProperty(ref _precision, value))
                {
                    SaveSettings();
                    SettingsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ViewMode ViewMode
        {
            get => _viewMode;
            set
            {
                if (SetProperty(ref _viewMode, value))
                {
                    SaveSettings();
                    SettingsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (SetProperty(ref _fontSize, value))
                {
                    SaveSettings();
                    SettingsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ICommand ResetWindowCommand { get; }
        public ICommand ResetDefaultsCommand { get; }

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            ResetWindowCommand = new RelayCommand(ResetWindow);
            ResetDefaultsCommand = new RelayCommand(ResetDefaults);

            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = _settingsService.LoadSettings();
            _isWordWrapEnabled = settings.IsWordWrapEnabled;
            _ignoreWhitespace = settings.IgnoreWhitespace;
            _precision = settings.Precision;
            _viewMode = settings.ViewMode;
            _fontSize = settings.FontSize;

            OnPropertyChanged(nameof(IsWordWrapEnabled));
            OnPropertyChanged(nameof(IgnoreWhitespace));
            OnPropertyChanged(nameof(Precision));
            OnPropertyChanged(nameof(ViewMode));
            OnPropertyChanged(nameof(FontSize));
        }

        private void SaveSettings()
        {
            var settings = _settingsService.LoadSettings();
            settings.IsWordWrapEnabled = IsWordWrapEnabled;
            settings.IgnoreWhitespace = IgnoreWhitespace;
            settings.Precision = Precision;
            settings.ViewMode = ViewMode;
            settings.FontSize = FontSize;
            _settingsService.SaveSettings(settings);
        }

        private void ResetDefaults(object? parameter)
        {
            _settingsService.ResetToDefaults();
            LoadSettings();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ResetWindow(object? parameter)
        {
            var window = Application.Current.MainWindow;
            if (window != null)
            {
                window.WindowState = WindowState.Normal;
                window.Width = 1200;
                window.Height = 800;

                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                window.Left = (screenWidth - window.Width) / 2;
                window.Top = (screenHeight - window.Height) / 2;
            }
        }
    }
}