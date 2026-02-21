using System.ComponentModel;

namespace DiffApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly IMergeService _mergeService;
        private readonly ISettingsService _settingsService;
        private readonly IScrollService _scrollService;

        private ComparisonViewModel? _comparisonViewModel;
        private bool _isSettingsPanelOpen = true;

        public InputViewModel InputViewModel { get; }

        public ComparisonViewModel? ComparisonViewModel
        {
            get => _comparisonViewModel;
            private set => SetProperty(ref _comparisonViewModel, value);
        }

        public bool IsSettingsPanelOpen
        {
            get => _isSettingsPanelOpen;
            set => SetProperty(ref _isSettingsPanelOpen, value);
        }

        public ICommand CopyTextCommand { get; }
        public ICommand ToggleSettingsCommand { get; }
        public ICommand SwapAllCommand { get; }
        public ICommand ResetDefaultsCommand { get; }
        public ICommand JumpToTopCommand { get; }
        public ICommand JumpToInputCommand { get; }
        public ICommand ClearContentCommand { get; }

        public MainViewModel(
            IComparisonService comparisonService,
            IMergeService mergeService,
            ISettingsService settingsService,
            IScrollService scrollService,
            InputViewModel inputViewModel)
        {
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _scrollService = scrollService ?? throw new ArgumentNullException(nameof(scrollService));
            InputViewModel = inputViewModel ?? throw new ArgumentNullException(nameof(inputViewModel));

            InputViewModel.CompareRequested += OnCompareRequested;
            InputViewModel.SettingsChanged += OnSettingsChanged;
            InputViewModel.PropertyChanged += InputViewModel_PropertyChanged;

            CopyTextCommand = new RelayCommand(CopyText);
            ToggleSettingsCommand = new RelayCommand(_ => IsSettingsPanelOpen = !IsSettingsPanelOpen);
            SwapAllCommand = new RelayCommand(SwapAll);
            ResetDefaultsCommand = new RelayCommand(ResetDefaults);
            ClearContentCommand = new RelayCommand(ClearContent);

            JumpToTopCommand = new RelayCommand(_ => _scrollService.ScrollToTop());
            JumpToInputCommand = new RelayCommand(_ => _scrollService.ScrollToInput());
        }

        private void ResetDefaults(object? parameter)
        {
            _settingsService.ResetToDefaults();
            var settings = _settingsService.LoadSettings();

            InputViewModel.IsWordWrapEnabled = settings.IsWordWrapEnabled;
            InputViewModel.IgnoreWhitespace = settings.IgnoreWhitespace;
            InputViewModel.Precision = settings.Precision;
            InputViewModel.ViewMode = settings.ViewMode;

            if (ComparisonViewModel != null)
            {
                PerformComparison();
            }
        }

        private void ClearContent(object? parameter)
        {
            InputViewModel.LeftText = string.Empty;
            InputViewModel.RightText = string.Empty;
            ComparisonViewModel = null;
        }

        private void InputViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InputViewModel.ViewMode) && ComparisonViewModel != null)
            {
                ComparisonViewModel.IsUnifiedMode = InputViewModel.ViewMode == ViewMode.Unified;
            }
        }

        private void OnSettingsChanged(object? sender, EventArgs e)
        {
            if (ComparisonViewModel != null)
            {
                PerformComparison();
            }
        }

        private void OnCompareRequested(object? sender, EventArgs e)
        {
            PerformComparison();
            Application.Current.Dispatcher.InvokeAsync(() => _scrollService.ScrollToTop());
        }

        private void PerformComparison()
        {
            var settings = new CompareSettings
            {
                IgnoreWhitespace = InputViewModel.IgnoreWhitespace,
                Precision = InputViewModel.Precision
            };

            string left = InputViewModel.LeftText;
            string right = InputViewModel.RightText;

            var result = _comparisonService.Compare(left, right, settings);

            ComparisonViewModel = new ComparisonViewModel(
                result,
                left,
                right,
                settings,
                _comparisonService,
                _mergeService,
                (side, text) =>
                {
                    if (side == Side.Old)
                    {
                        InputViewModel.LeftText = text;
                    }
                    else
                    {
                        InputViewModel.RightText = text;
                    }
                }
            );

            ComparisonViewModel.IsUnifiedMode = InputViewModel.ViewMode == ViewMode.Unified;
        }

        private void SwapAll(object? parameter)
        {
            string temp = InputViewModel.LeftText;
            InputViewModel.LeftText = InputViewModel.RightText;
            InputViewModel.RightText = temp;

            PerformComparison();
        }

        private void CopyText(object? parameter)
        {
            if (parameter is Side side)
            {
                string textToCopy = string.Empty;

                if (ComparisonViewModel != null)
                {
                    textToCopy = side == Side.Old ? ComparisonViewModel.LeftResultText : ComparisonViewModel.RightResultText;
                }
                else
                {
                    textToCopy = side == Side.Old ? InputViewModel.LeftText : InputViewModel.RightText;
                }

                if (!string.IsNullOrEmpty(textToCopy))
                {
                    try
                    {
                        Clipboard.SetText(textToCopy);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}