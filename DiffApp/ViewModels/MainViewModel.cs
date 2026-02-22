using System.ComponentModel;

namespace DiffApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly IMergeService _mergeService;
        private readonly ISettingsService _settingsService;
        private readonly IScrollService _scrollService;
        private readonly IHistoryService _historyService;

        private ComparisonViewModel? _comparisonViewModel;
        private bool _isSettingsPanelOpen = true;
        private bool _isInputPanelExpanded = true;
        private bool _isHistoryOpen;

        public InputViewModel InputViewModel { get; }
        public HistoryViewModel HistoryViewModel { get; }

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

        public bool IsInputPanelExpanded
        {
            get => _isInputPanelExpanded;
            set => SetProperty(ref _isInputPanelExpanded, value);
        }

        public bool IsHistoryOpen
        {
            get => _isHistoryOpen;
            set => SetProperty(ref _isHistoryOpen, value);
        }

        public ICommand CopyTextCommand { get; }
        public ICommand ToggleSettingsCommand { get; }
        public ICommand ToggleInputPanelCommand { get; }
        public ICommand SwapAllCommand { get; }
        public ICommand ResetDefaultsCommand { get; }
        public ICommand JumpToTopCommand { get; }
        public ICommand JumpToInputCommand { get; }
        public ICommand ClearContentCommand { get; }
        public ICommand OpenHistoryCommand { get; }
        public ICommand CloseHistoryCommand { get; }
        public ICommand ResetWindowCommand { get; }

        public MainViewModel(
            IComparisonService comparisonService,
            IMergeService mergeService,
            ISettingsService settingsService,
            IScrollService scrollService,
            IHistoryService historyService,
            InputViewModel inputViewModel,
            HistoryViewModel historyViewModel)
        {
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _scrollService = scrollService ?? throw new ArgumentNullException(nameof(scrollService));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));

            InputViewModel = inputViewModel ?? throw new ArgumentNullException(nameof(inputViewModel));
            HistoryViewModel = historyViewModel ?? throw new ArgumentNullException(nameof(historyViewModel));

            InputViewModel.CompareRequested += OnCompareRequested;
            InputViewModel.SettingsChanged += OnSettingsChanged;
            InputViewModel.PropertyChanged += InputViewModel_PropertyChanged;
            HistoryViewModel.RestoreRequested += OnHistoryRestoreRequested;

            CopyTextCommand = new RelayCommand(CopyText);
            ToggleSettingsCommand = new RelayCommand(_ => IsSettingsPanelOpen = !IsSettingsPanelOpen);
            ToggleInputPanelCommand = new RelayCommand(_ => IsInputPanelExpanded = !IsInputPanelExpanded);
            SwapAllCommand = new RelayCommand(SwapAll);
            ResetDefaultsCommand = new RelayCommand(ResetDefaults);
            ClearContentCommand = new RelayCommand(ClearContent);

            JumpToTopCommand = new RelayCommand(_ => _scrollService.ScrollToTop());
            JumpToInputCommand = new RelayCommand(_ => _scrollService.ScrollToInput());

            OpenHistoryCommand = new RelayCommand(OpenHistory);
            CloseHistoryCommand = new RelayCommand(_ => IsHistoryOpen = false);
            ResetWindowCommand = new RelayCommand(ResetWindow);
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

        private void OpenHistory(object? parameter)
        {
            IsHistoryOpen = true;
            IsSettingsPanelOpen = false;
            if (HistoryViewModel.LoadHistoryCommand.CanExecute(null))
            {
                HistoryViewModel.LoadHistoryCommand.Execute(null);
            }
        }

        private void OnHistoryRestoreRequested(object? sender, DiffHistoryItem item)
        {
            IsHistoryOpen = false;

            InputViewModel.LeftText = item.OriginalText;
            InputViewModel.RightText = item.ModifiedText;

            PerformComparison(saveToHistory: false);
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
            IsInputPanelExpanded = true;
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