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

        public event EventHandler? CompareRequested;
        public event EventHandler? SettingsChanged;

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
                    SaveCurrentSettings();
                    SettingsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsWordWrapEnabled
        {
            get => _isWordWrapEnabled;
            set
            {
                if (SetProperty(ref _isWordWrapEnabled, value))
                {
                    SaveCurrentSettings();
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
                    SaveCurrentSettings();
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
                    SaveCurrentSettings();
                }
            }
        }

        public ICommand SwapTextsCommand { get; }
        public ICommand FindDifferenceCommand { get; }

        public InputViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            SwapTextsCommand = new RelayCommand(SwapTexts);
            FindDifferenceCommand = new RelayCommand(OnFindDifference, CanFindDifference);

            LoadSettings();
            LoadSampleText();
        }

        private void LoadSettings()
        {
            var settings = _settingsService.LoadSettings();
            _ignoreWhitespace = settings.IgnoreWhitespace;
            _isWordWrapEnabled = settings.IsWordWrapEnabled;
            _precision = settings.Precision;
            _viewMode = settings.ViewMode;
        }

        private void SaveCurrentSettings()
        {
            var current = _settingsService.LoadSettings();
            current.IgnoreWhitespace = IgnoreWhitespace;
            current.IsWordWrapEnabled = IsWordWrapEnabled;
            current.Precision = Precision;
            current.ViewMode = ViewMode;
            _settingsService.SaveSettings(current);
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
            LeftText = "// Comment\nProgram code\nThis is the original text.\nIt has several lines.\n// Comment here\n// More comment\n// So much comment\nBanana\nBean\nSome lines are unique to this side.\n\n\n.\n \nwill remove this\nwill remove this";
            RightText = "Program code\nThis is the modified text.\nIt has several lines.\nBanana\nBean\nSome lines are unique to the other side.\nAnd an extra line here.\n\nThere\nWill\nBe\nSo\nMuch\nMore\nThan\nThis";
        }
    }
}