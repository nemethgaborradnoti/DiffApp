using DiffApp.Models;
using DiffApp.Services.Interfaces;

namespace DiffApp.ViewModels
{
    public class InputViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private string _leftText = string.Empty;
        private string _rightText = string.Empty;
        private bool _ignoreWhitespace;
        private bool _isWordWrapEnabled = true;
        private PrecisionLevel _precision = PrecisionLevel.Word;
        private ViewMode _viewMode = ViewMode.Split;
        private double _fontSize = 14.0;

        public event EventHandler? CompareRequested;
        public event EventHandler<string>? SettingsChanged;

        public string LeftText
        {
            get => _leftText;
            set => SetProperty(ref _leftText, value);
        }

        public string RightText
        {
            get => _rightText;
            set => SetProperty(ref _rightText, value);
        }

        public bool IgnoreWhitespace
        {
            get => _ignoreWhitespace;
            set
            {
                if (SetProperty(ref _ignoreWhitespace, value))
                {
                    SettingsChanged?.Invoke(this, nameof(IgnoreWhitespace));
                }
            }
        }

        public bool IsWordWrapEnabled
        {
            get => _isWordWrapEnabled;
            set => SetProperty(ref _isWordWrapEnabled, value);
        }

        public PrecisionLevel Precision
        {
            get => _precision;
            set
            {
                if (SetProperty(ref _precision, value))
                {
                    SettingsChanged?.Invoke(this, nameof(Precision));
                }
            }
        }

        public ViewMode ViewMode
        {
            get => _viewMode;
            set => SetProperty(ref _viewMode, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public ICommand SwapTextsCommand { get; }
        public ICommand FindDifferenceCommand { get; }

        public InputViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            SwapTextsCommand = new RelayCommand(SwapTexts);
            FindDifferenceCommand = new RelayCommand(OnFindDifference, CanFindDifference);

            ReloadSettings();
            LoadSampleText();
        }

        public void ReloadSettings()
        {
            var settings = _settingsService.LoadSettings();
            IgnoreWhitespace = settings.IgnoreWhitespace;
            IsWordWrapEnabled = settings.IsWordWrapEnabled;
            Precision = settings.Precision;
            ViewMode = settings.ViewMode;
            FontSize = settings.FontSize;
        }

        private void OnFindDifference(object? parameter)
        {
            CompareRequested?.Invoke(this, EventArgs.Empty);
        }

        private bool CanFindDifference(object? parameter)
        {
            return !string.IsNullOrEmpty(LeftText) || !string.IsNullOrEmpty(RightText);
        }

        private void SwapTexts(object? parameter)
        {
            string temp = LeftText;
            LeftText = RightText;
            RightText = temp;

            CompareRequested?.Invoke(this, EventArgs.Empty);
        }

        private void LoadSampleText()
        {
            LeftText = Application.Current?.Resources["InputView_SampleLeft"] as string ?? string.Empty;
            RightText = Application.Current?.Resources["InputView_SampleRight"] as string ?? string.Empty;
        }
    }
}