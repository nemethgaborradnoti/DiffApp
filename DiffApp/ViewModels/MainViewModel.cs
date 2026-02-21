using System.ComponentModel;

namespace DiffApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly IMergeService _mergeService;

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

        public MainViewModel(IComparisonService comparisonService, IMergeService mergeService, InputViewModel inputViewModel)
        {
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            InputViewModel = inputViewModel ?? throw new ArgumentNullException(nameof(inputViewModel));

            InputViewModel.CompareRequested += OnCompareRequested;
            InputViewModel.PropertyChanged += InputViewModel_PropertyChanged;

            CopyTextCommand = new RelayCommand(CopyText);
            ToggleSettingsCommand = new RelayCommand(_ => IsSettingsPanelOpen = !IsSettingsPanelOpen);
            ToggleInputPanelCommand = new RelayCommand(_ => IsInputPanelOpen = !IsInputPanelOpen);
        }

        private void InputViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InputViewModel.ViewMode) && ComparisonViewModel != null)
            {
                ComparisonViewModel.IsUnifiedMode = InputViewModel.ViewMode == ViewMode.Unified;
            }
        }

        private void OnCompareRequested(object? sender, EventArgs e)
        {
            PerformComparison();
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