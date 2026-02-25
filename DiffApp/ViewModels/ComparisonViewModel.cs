using System.Threading.Tasks;

namespace DiffApp.ViewModels
{
    public class ComparisonViewModel : ViewModelBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly IMergeService _mergeService;
        private readonly CompareSettings _settings;
        private readonly Action<Side, string>? _onSourceUpdated;

        private ComparisonResult _comparisonResult;
        private IList<DiffLineViewModel> _diffLines;
        private IEnumerable<MinimapSegment> _unifiedMinimapSegments;
        private bool _isUnifiedMode;
        private bool _ignoreWhitespace;
        private string _leftResultText;
        private string _rightResultText;
        private bool _isBusy;
        private int _removalsCount;
        private int _additionsCount;

        public event EventHandler<int>? ScrollRequested;

        public ComparisonResult ComparisonResult
        {
            get => _comparisonResult;
            private set
            {
                if (SetProperty(ref _comparisonResult, value))
                {
                    CalculateStats();
                }
            }
        }

        public IList<DiffLineViewModel> DiffLines
        {
            get => _diffLines;
            private set => SetProperty(ref _diffLines, value);
        }

        public IEnumerable<MinimapSegment> UnifiedMinimapSegments
        {
            get => _unifiedMinimapSegments;
            private set => SetProperty(ref _unifiedMinimapSegments, value);
        }

        public bool IsUnifiedMode
        {
            get => _isUnifiedMode;
            set
            {
                if (SetProperty(ref _isUnifiedMode, value))
                {
                    if (_comparisonResult != null)
                    {
                        DiffLines = new VirtualDiffLineList(_comparisonResult, value);
                        CalculateMinimap();
                    }
                }
            }
        }

        public bool IgnoreWhitespace
        {
            get => _ignoreWhitespace;
            set
            {
                if (SetProperty(ref _ignoreWhitespace, value))
                {
                    CalculateStats();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
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

        public int RemovalsCount
        {
            get => _removalsCount;
            private set => SetProperty(ref _removalsCount, value);
        }

        public int AdditionsCount
        {
            get => _additionsCount;
            private set => SetProperty(ref _additionsCount, value);
        }

        public ICommand MergeBlockCommand { get; }
        public ICommand SelectBlockCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand ScrollMinimapCommand { get; }
        public ICommand NavigateToSegmentCommand { get; }

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
            _ignoreWhitespace = settings.IgnoreWhitespace;
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            _onSourceUpdated = onSourceUpdated;

            CalculateStats();

            DiffLines = new VirtualDiffLineList(_comparisonResult, _isUnifiedMode);
            CalculateMinimap();

            MergeBlockCommand = new RelayCommand(ExecuteMerge);
            SelectBlockCommand = new RelayCommand(SelectBlock);
            DeselectAllCommand = new RelayCommand(_ => DeselectAll());
            ScrollMinimapCommand = new RelayCommand(OnScrollMinimap);
            NavigateToSegmentCommand = new RelayCommand(OnNavigateToSegment);
        }

        private int GetLineCount(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return text.Replace("\r\n", "\n").Split('\n').Length;
        }

        private void CalculateStats()
        {
            if (_comparisonResult?.Blocks == null)
            {
                RemovalsCount = 0;
                AdditionsCount = 0;
                return;
            }

            int removals = 0;
            int additions = 0;

            foreach (var block in _comparisonResult.Blocks)
            {
                if (IgnoreWhitespace && block.IsWhitespaceChange)
                {
                    continue;
                }

                if (block.Kind == BlockType.Removed || block.Kind == BlockType.Modified)
                {
                    removals += block.OldLines.Count;
                }

                if (block.Kind == BlockType.Added || block.Kind == BlockType.Modified)
                {
                    additions += block.NewLines.Count;
                }
            }

            RemovalsCount = removals;
            AdditionsCount = additions;
        }

        private void OnScrollMinimap(object? parameter)
        {
            if (parameter is double relativeOffset && DiffLines != null && DiffLines.Count > 0)
            {
                int targetIndex = (int)(relativeOffset * DiffLines.Count);
                if (targetIndex < 0) targetIndex = 0;
                if (targetIndex >= DiffLines.Count) targetIndex = DiffLines.Count - 1;

                ScrollRequested?.Invoke(this, targetIndex);
            }
        }

        private void OnNavigateToSegment(object? parameter)
        {
            if (parameter is MinimapSegment segment)
            {
                if (IgnoreWhitespace && segment.Block != null && segment.Block.IsWhitespaceChange)
                {
                    return;
                }

                if (segment.Block != null)
                {
                    SelectBlock(segment.Block);
                }

                ScrollRequested?.Invoke(this, segment.TargetLineIndex);
            }
        }

        private void CalculateMinimap()
        {
            if (_comparisonResult == null)
            {
                UnifiedMinimapSegments = new List<MinimapSegment>();
                return;
            }

            int currentVisualIndex = 0;
            double totalVisualHeight = 0;

            foreach (var block in _comparisonResult.Blocks)
            {
                int height = GetBlockHeight(block);
                totalVisualHeight += height;
            }

            if (totalVisualHeight == 0) totalVisualHeight = 1;

            var unifiedSegments = new List<MinimapSegment>();

            foreach (var block in _comparisonResult.Blocks)
            {
                int height = GetBlockHeight(block);

                if (block.Kind != BlockType.Unchanged)
                {
                    double offsetPct = currentVisualIndex / totalVisualHeight;
                    double heightPct = height / totalVisualHeight;

                    if (heightPct < 0.002) heightPct = 0.002;

                    BlockType leftType = BlockType.Unchanged;
                    BlockType rightType = BlockType.Unchanged;

                    if (block.Kind == BlockType.Removed)
                    {
                        leftType = BlockType.Removed;
                        rightType = BlockType.Unchanged;
                    }
                    else if (block.Kind == BlockType.Added)
                    {
                        leftType = BlockType.Unchanged;
                        rightType = BlockType.Added;
                    }
                    else if (block.Kind == BlockType.Modified)
                    {
                        leftType = BlockType.Modified;
                        rightType = BlockType.Modified;
                    }

                    unifiedSegments.Add(new MinimapSegment
                    {
                        LeftType = leftType,
                        RightType = rightType,
                        OffsetPercentage = offsetPct,
                        HeightPercentage = heightPct,
                        TargetLineIndex = currentVisualIndex,
                        Block = block
                    });
                }

                currentVisualIndex += height;
            }

            UnifiedMinimapSegments = unifiedSegments;
        }

        private int GetBlockHeight(ChangeBlock block)
        {
            if (_isUnifiedMode)
            {
                if (block.Kind == BlockType.Modified) return block.OldLines.Count + block.NewLines.Count;
                if (block.Kind == BlockType.Added) return block.NewLines.Count;
                if (block.Kind == BlockType.Removed) return block.OldLines.Count;
                return block.OldLines.Count;
            }
            else
            {
                return Math.Max(block.OldLines.Count, block.NewLines.Count);
            }
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
            ChangeBlock? targetBlock = null;

            if (parameter is DiffLineViewModel lineVm)
            {
                targetBlock = lineVm.ParentBlock;
            }
            else if (parameter is ChangeBlock block)
            {
                targetBlock = block;
            }

            if (targetBlock != null && IgnoreWhitespace && targetBlock.IsWhitespaceChange)
            {
                return;
            }

            if (targetBlock != null && targetBlock.IsMergeable && ComparisonResult?.Blocks != null)
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

        private async void RefreshComparison()
        {
            IsBusy = true;
            try
            {
                _settings.IgnoreWhitespace = IgnoreWhitespace;

                var result = await Task.Run(() => _comparisonService.Compare(LeftResultText, RightResultText, _settings));
                ComparisonResult = result;
                DiffLines = new VirtualDiffLineList(result, IsUnifiedMode);
                CalculateMinimap();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}