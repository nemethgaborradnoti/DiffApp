using DiffApp.Services.Interfaces;
using System.ComponentModel;

namespace DiffApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly IMergeService _mergeService;
        private readonly ISettingsService _settingsService;

        private ComparisonViewModel? _comparisonViewModel;
        private bool _isSettingsPanelOpen = true;
        private bool _isInputPanelOpen = true;

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

        public bool IsInputPanelOpen
        {
            get => _isInputPanelOpen;
            set => SetProperty(ref _isInputPanelOpen, value);
        }

        public ICommand CopyTextCommand { get; }
        public ICommand ToggleSettingsCommand { get; }
        public ICommand ToggleInputPanelCommand { get; }
        public ICommand SwapAllCommand { get; }
        public ICommand ResetDefaultsCommand { get; }

        public MainViewModel(
            IComparisonService comparisonService,
            IMergeService mergeService,
            ISettingsService settingsService,
            InputViewModel inputViewModel)
        {
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            InputViewModel = inputViewModel ?? throw new ArgumentNullException(nameof(inputViewModel));

            InputViewModel.CompareRequested += OnCompareRequested;
            InputViewModel.SettingsChanged += OnSettingsChanged;
            InputViewModel.PropertyChanged += InputViewModel_PropertyChanged;

            CopyTextCommand = new RelayCommand(CopyText);
            ToggleSettingsCommand = new RelayCommand(_ => IsSettingsPanelOpen = !IsSettingsPanelOpen);
            ToggleInputPanelCommand = new RelayCommand(_ => IsInputPanelOpen = !IsInputPanelOpen);
            SwapAllCommand = new RelayCommand(SwapAll);
            ResetDefaultsCommand = new RelayCommand(ResetDefaults);

            // Initial state: Input panel open, Comparison null (handled by UI triggers)
            IsInputPanelOpen = true;
        }

        private void ResetDefaults(object? parameter)
        {
            _settingsService.ResetToDefaults();
            var settings = _settingsService.LoadSettings();

            // Update InputViewModel
            InputViewModel.IsWordWrapEnabled = settings.IsWordWrapEnabled;
            InputViewModel.IgnoreWhitespace = settings.IgnoreWhitespace;
            InputViewModel.Precision = settings.Precision;
            InputViewModel.ViewMode = settings.ViewMode;

            PerformComparison();
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
            // Re-run comparison if settings affecting it (like IgnoreWhitespace) change
            if (ComparisonViewModel != null)
            {
                PerformComparison();
            }
        }

        private void OnCompareRequested(object? sender, EventArgs e)
        {
            PerformComparison();

            // Auto-collapse input panel when comparison is requested
            IsInputPanelOpen = false;
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
                _mergeService
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