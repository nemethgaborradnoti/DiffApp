using System.ComponentModel;
using System.Threading.Tasks;

namespace DiffApp.ViewModels
{
    public class EditorViewModel : ViewModelBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly IMergeService _mergeService;
        private readonly IScrollService _scrollService;
        private readonly IHistoryService _historyService;

        private ComparisonViewModel? _comparisonViewModel;
        private bool _isInputPanelExpanded = true;
        private bool _isSettingsPanelOpen;

        private bool _isLeftCopied;
        private bool _isRightCopied;

        public InputViewModel InputViewModel { get; }
        public SettingsViewModel SettingsViewModel { get; }

        public ComparisonViewModel? ComparisonViewModel
        {
            get => _comparisonViewModel;
            private set => SetProperty(ref _comparisonViewModel, value);
        }

        public bool IsInputPanelExpanded
        {
            get => _isInputPanelExpanded;
            set => SetProperty(ref _isInputPanelExpanded, value);
        }

        public bool IsSettingsPanelOpen
        {
            get => _isSettingsPanelOpen;
            set => SetProperty(ref _isSettingsPanelOpen, value);
        }

        public bool IsLeftCopied
        {
            get => _isLeftCopied;
            set => SetProperty(ref _isLeftCopied, value);
        }

        public bool IsRightCopied
        {
            get => _isRightCopied;
            set => SetProperty(ref _isRightCopied, value);
        }

        public ICommand CopyTextCommand { get; }
        public ICommand ToggleInputPanelCommand { get; }
        public ICommand SwapAllCommand { get; }
        public ICommand JumpToTopCommand { get; }
        public ICommand JumpToInputCommand { get; }
        public ICommand ClearContentCommand { get; }
        public ICommand ToggleSettingsPanelCommand { get; }

        public EditorViewModel(
            IComparisonService comparisonService,
            IMergeService mergeService,
            IScrollService scrollService,
            IHistoryService historyService,
            InputViewModel inputViewModel,
            SettingsViewModel settingsViewModel)
        {
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            _scrollService = scrollService ?? throw new ArgumentNullException(nameof(scrollService));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            InputViewModel = inputViewModel ?? throw new ArgumentNullException(nameof(inputViewModel));
            SettingsViewModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));

            InputViewModel.CompareRequested += OnCompareRequested;
            InputViewModel.SettingsChanged += OnSettingsChanged;
            InputViewModel.PropertyChanged += InputViewModel_PropertyChanged;

            SettingsViewModel.SettingsChanged += OnSettingsChanged;

            CopyTextCommand = new RelayCommand(async p => await CopyTextAsync(p), CanCopyText);
            ToggleInputPanelCommand = new RelayCommand(_ => IsInputPanelExpanded = !IsInputPanelExpanded);
            SwapAllCommand = new RelayCommand(SwapAll);
            ClearContentCommand = new RelayCommand(ClearContent);
            JumpToTopCommand = new RelayCommand(_ => _scrollService.ScrollToTop());
            JumpToInputCommand = new RelayCommand(_ => _scrollService.ScrollToInput());
            ToggleSettingsPanelCommand = new RelayCommand(_ => IsSettingsPanelOpen = !IsSettingsPanelOpen);
        }

        public void LoadFromHistory(DiffHistoryItem item)
        {
            InputViewModel.LeftText = item.OriginalText;
            InputViewModel.RightText = item.ModifiedText;
            PerformComparison(saveToHistory: false);
        }

        private void OnSettingsChanged(object? sender, string propertyName)
        {
            InputViewModel.ReloadSettings();

            if (ComparisonViewModel != null)
            {
                if (propertyName == nameof(SettingsViewModel.IgnoreWhitespace))
                {
                    // Sync the setting to ComparisonViewModel directly
                    ComparisonViewModel.IgnoreWhitespace = SettingsViewModel.IgnoreWhitespace;
                    return;
                }

                if (propertyName == nameof(SettingsViewModel.Precision))
                {
                    return;
                }

                if (propertyName == nameof(SettingsViewModel.ViewMode))
                {
                    ComparisonViewModel.IsUnifiedMode = SettingsViewModel.ViewMode == ViewMode.Unified;
                    return;
                }

                if (string.IsNullOrEmpty(propertyName))
                {
                    PerformComparison();
                }
            }
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
            PerformComparison(saveToHistory: true);
            Application.Current.Dispatcher.InvokeAsync(() => _scrollService.ScrollToTop());
        }

        private void PerformComparison(bool saveToHistory = false)
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

            IsInputPanelExpanded = false;

            if (saveToHistory && (!string.IsNullOrEmpty(left) || !string.IsNullOrEmpty(right)))
            {
                _ = _historyService.AddAsync(left, right);
            }
        }

        private void SwapAll(object? parameter)
        {
            string temp = InputViewModel.LeftText;
            InputViewModel.LeftText = InputViewModel.RightText;
            InputViewModel.RightText = temp;

            PerformComparison(saveToHistory: true);
        }

        private void ClearContent(object? parameter)
        {
            InputViewModel.LeftText = string.Empty;
            InputViewModel.RightText = string.Empty;
            ComparisonViewModel = null;
            IsInputPanelExpanded = true;
        }

        private bool CanCopyText(object? parameter)
        {
            if (parameter is Side side)
            {
                return side == Side.Old ? !IsLeftCopied : !IsRightCopied;
            }
            return false;
        }

        private async Task CopyTextAsync(object? parameter)
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

                        if (side == Side.Old)
                        {
                            IsLeftCopied = true;
                            ((RelayCommand)CopyTextCommand).RaiseCanExecuteChanged();
                            await Task.Delay(2000);
                            IsLeftCopied = false;
                            ((RelayCommand)CopyTextCommand).RaiseCanExecuteChanged();
                        }
                        else
                        {
                            IsRightCopied = true;
                            ((RelayCommand)CopyTextCommand).RaiseCanExecuteChanged();
                            await Task.Delay(2000);
                            IsRightCopied = false;
                            ((RelayCommand)CopyTextCommand).RaiseCanExecuteChanged();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}