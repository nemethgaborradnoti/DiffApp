namespace DiffApp.ViewModels
{
    public class ComparisonViewModel : ViewModelBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly IMergeService _mergeService;
        private readonly CompareSettings _settings;
        private readonly Action<Side, string>? _onSourceUpdated;

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

        public string LeftResultText
        {
            get => _leftResultText;
            private set
            {
                if (SetProperty(ref _leftResultText, value))
                {
                    OnPropertyChanged(nameof(LeftLineCount));
                }
            }
        }

        public string RightResultText
        {
            get => _rightResultText;
            private set
            {
                if (SetProperty(ref _rightResultText, value))
                {
                    OnPropertyChanged(nameof(RightLineCount));
                }
            }
        }

        public int LeftLineCount => GetLineCount(_leftResultText);
        public int RightLineCount => GetLineCount(_rightResultText);

        public ICommand MergeBlockCommand { get; }
        public ICommand SelectBlockCommand { get; }
        public ICommand DeselectAllCommand { get; }

        public ComparisonViewModel(
            ComparisonResult initialResult,
            string leftText,
            string rightText,
            CompareSettings settings,
            IComparisonService comparisonService,
            IMergeService mergeService,
            Action<Side, string>? onSourceUpdated = null)
        {
            _comparisonResult = initialResult;
            _leftResultText = leftText;
            _rightResultText = rightText;
            _settings = settings;
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            _onSourceUpdated = onSourceUpdated;

            _unifiedLines = CreateUnifiedLines();

            MergeBlockCommand = new RelayCommand(ExecuteMerge);
            SelectBlockCommand = new RelayCommand(SelectBlock);
            DeselectAllCommand = new RelayCommand(_ => DeselectAll());
        }

        private int GetLineCount(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return text.Replace("\r\n", "\n").Split('\n').Length;
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
                RightResultText = _mergeService.MergeBlock(RightResultText, block, direction);
                _onSourceUpdated?.Invoke(Side.New, RightResultText);
            }
            else
            {
                LeftResultText = _mergeService.MergeBlock(LeftResultText, block, direction);
                _onSourceUpdated?.Invoke(Side.Old, LeftResultText);
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
            var result = _comparisonService.Compare(LeftResultText, RightResultText, _settings);
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