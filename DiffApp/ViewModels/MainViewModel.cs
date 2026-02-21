namespace DiffApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly IMergeService _mergeService;

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

        public MainViewModel(IComparisonService comparisonService, IMergeService mergeService, InputViewModel inputViewModel)
        {
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            InputViewModel = inputViewModel ?? throw new ArgumentNullException(nameof(inputViewModel));

            InputViewModel.CompareRequested += OnCompareRequested;

            CopyTextCommand = new RelayCommand(CopyText);
            ToggleSettingsCommand = new RelayCommand(_ => IsSettingsPanelOpen = !IsSettingsPanelOpen);
        }

        private void OnCompareRequested(object? sender, EventArgs e)
        {
            PerformComparison();
        }

        private void PerformComparison()
        {
            var settings = new CompareSettings
            {
                IgnoreWhitespace = InputViewModel.IgnoreWhitespace,
                Precision = InputViewModel.Precision
            };

            var result = _comparisonService.Compare(InputViewModel.LeftText, InputViewModel.RightText, settings);

            var comparisonVM = new ComparisonViewModel(result);
            comparisonVM.MergeRequested += OnMergeRequested;

            ComparisonViewModel = comparisonVM;
        }

        private void OnMergeRequested(object? sender, MergeRequestArgs e)
        {
            if (e.Direction == MergeDirection.LeftToRight)
            {
                InputViewModel.RightText = _mergeService.MergeBlock(InputViewModel.RightText, e.Block, e.Direction);
            }
            else
            {
                InputViewModel.LeftText = _mergeService.MergeBlock(InputViewModel.LeftText, e.Block, e.Direction);
            }

            PerformComparison();
        }

        private void CopyText(object? parameter)
        {
            if (parameter is Side side)
            {
                string textToCopy = side == Side.Old ? InputViewModel.LeftText : InputViewModel.RightText;

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