namespace DiffApp.ViewModels
{
    public class ComparisonViewModel : ViewModelBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly IMergeService _mergeService;
        private readonly CompareSettings _settings;

        private ComparisonResult _comparisonResult;
        private IReadOnlyList<ChangeLine> _unifiedLines;
        private bool _isUnifiedMode;
        private string _leftResultText;
        private string _rightResultText;

        public ComparisonResult ComparisonResult
        {
            get => _comparisonResult;
            private set => SetProperty(ref _comparisonResult, value);
        }

        public IReadOnlyList<ChangeLine> UnifiedLines
        {
            get => _unifiedLines;
            private set => SetProperty(ref _unifiedLines, value);
        }

        public bool IsUnifiedMode
        {
            get => _isUnifiedMode;
            set => SetProperty(ref _isUnifiedMode, value);
        }

        public string LeftResultText => _leftResultText;
        public string RightResultText => _rightResultText;

        public ICommand MergeBlockCommand { get; }
        public ICommand SelectBlockCommand { get; }
        public ICommand DeselectAllCommand { get; }

        public ComparisonViewModel(
            ComparisonResult initialResult,
            string leftText,
            string rightText,
            CompareSettings settings,
            IComparisonService comparisonService,
            IMergeService mergeService)
        {
            _comparisonResult = initialResult;
            _leftResultText = leftText;
            _rightResultText = rightText;
            _settings = settings;
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));

            _unifiedLines = CreateUnifiedLines();

            MergeBlockCommand = new RelayCommand(ExecuteMerge);
            SelectBlockCommand = new RelayCommand(SelectBlock);
            DeselectAllCommand = new RelayCommand(_ => DeselectAll());
        }

        private void ExecuteMerge(object? parameter)
        {
            if (parameter is object[] args && args.Length == 2)
            {
                if (args[0] is ChangeBlock block && args[1] is MergeDirection direction)
                {
                    PerformMerge(block, direction);
                }
            }
        }

        private void PerformMerge(ChangeBlock block, MergeDirection direction)
        {
            if (direction == MergeDirection.LeftToRight)
            {
                _rightResultText = _mergeService.MergeBlock(_rightResultText, block, direction);
            }
            else
            {
                _leftResultText = _mergeService.MergeBlock(_leftResultText, block, direction);
            }

            RefreshComparison();
            DeselectAll();
        }

        private void SelectBlock(object? parameter)
        {
            if (ComparisonResult?.Blocks == null) return;

            if (parameter is ChangeBlock targetBlock && targetBlock.IsMergeable)
            {
                foreach (var block in ComparisonResult.Blocks)
                {
                    block.IsSelected = block == targetBlock;
                }
            }
        }

        private void DeselectAll()
        {
            if (ComparisonResult?.Blocks == null) return;

            foreach (var block in ComparisonResult.Blocks)
            {
                block.IsSelected = false;
            }
        }

        private void RefreshComparison()
        {
            var result = _comparisonService.Compare(_leftResultText, _rightResultText, _settings);
            ComparisonResult = result;
            UnifiedLines = CreateUnifiedLines();
        }

        private List<ChangeLine> CreateUnifiedLines()
        {
            var lines = new List<ChangeLine>();
            if (_comparisonResult?.Blocks == null) return lines;

            foreach (var block in _comparisonResult.Blocks)
            {
                switch (block.Kind)
                {
                    case BlockType.Unchanged:
                    case BlockType.Removed:
                        lines.AddRange(block.OldLines);
                        break;
                    case BlockType.Added:
                        lines.AddRange(block.NewLines);
                        break;
                    case BlockType.Modified:
                        lines.AddRange(block.OldLines);
                        lines.AddRange(block.NewLines);
                        break;
                }
            }
            return lines;
        }
    }
}