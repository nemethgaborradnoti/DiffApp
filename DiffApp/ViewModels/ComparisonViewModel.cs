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
        private IEnumerable<MinimapSegment> _leftMinimapSegments;
        private IEnumerable<MinimapSegment> _rightMinimapSegments;
        private bool _isUnifiedMode;
        private string _leftResultText;
        private string _rightResultText;
        private bool _isBusy;

        public event EventHandler<int>? ScrollRequested;

        public ComparisonResult ComparisonResult
        {
            get => _comparisonResult;
            private set => SetProperty(ref _comparisonResult, value);
        }

        public IList<DiffLineViewModel> DiffLines
        {
            get => _diffLines;
            private set => SetProperty(ref _diffLines, value);
        }

        public IEnumerable<MinimapSegment> LeftMinimapSegments
        {
            get => _leftMinimapSegments;
            private set => SetProperty(ref _leftMinimapSegments, value);
        }

        public IEnumerable<MinimapSegment> RightMinimapSegments
        {
            get => _rightMinimapSegments;
            private set => SetProperty(ref _rightMinimapSegments, value);
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
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            _onSourceUpdated = onSourceUpdated;

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
                // 1. Select the block (visual highlighting + merge controls)
                if (segment.Block != null)
                {
                    SelectBlock(segment.Block);
                }

                // 2. Scroll to the specific line
                ScrollRequested?.Invoke(this, segment.TargetLineIndex);
            }
        }

        private void CalculateMinimap()
        {
            if (_comparisonResult == null)
            {
                LeftMinimapSegments = new List<MinimapSegment>();
                RightMinimapSegments = new List<MinimapSegment>();
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

            var leftSegments = new List<MinimapSegment>();
            var rightSegments = new List<MinimapSegment>();

            double mergeThreshold = 0.005;

            foreach (var block in _comparisonResult.Blocks)
            {
                int height = GetBlockHeight(block);

                if (block.Kind != BlockType.Unchanged)
                {
                    double offsetPct = currentVisualIndex / totalVisualHeight;
                    double heightPct = height / totalVisualHeight;

                    if (heightPct < 0.002) heightPct = 0.002;

                    // Left Side Logic (Red)
                    if (block.Kind == BlockType.Removed || block.Kind == BlockType.Modified)
                    {
                        AddOrMergeSegment(leftSegments, new MinimapSegment
                        {
                            Side = Side.Old,
                            Type = BlockType.Removed,
                            OffsetPercentage = offsetPct,
                            HeightPercentage = heightPct,
                            TargetLineIndex = currentVisualIndex,
                            Block = block
                        }, mergeThreshold);
                    }

                    // Right Side Logic (Green)
                    if (block.Kind == BlockType.Added || block.Kind == BlockType.Modified)
                    {
                        AddOrMergeSegment(rightSegments, new MinimapSegment
                        {
                            Side = Side.New,
                            Type = BlockType.Added,
                            OffsetPercentage = offsetPct,
                            HeightPercentage = heightPct,
                            TargetLineIndex = currentVisualIndex,
                            Block = block
                        }, mergeThreshold);
                    }
                }

                currentVisualIndex += height;
            }

            LeftMinimapSegments = leftSegments;
            RightMinimapSegments = rightSegments;
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

        private void AddOrMergeSegment(List<MinimapSegment> list, MinimapSegment newSegment, double threshold)
        {
            if (list.Count > 0)
            {
                var last = list[list.Count - 1];

                // Only merge if the underlying block is the same or if they are very close.
                // However, for precise clicking, merging different blocks into one segment prevents selecting the correct block.
                // We will disable merging if we want precise block selection, OR strictly merge only identical blocks.
                // Given the visual requirement was for performance on large files, we keep merging but update logic:
                // If we merge, we keep the Block reference of the *start* of the merged segment.
                // But typically, blocks are distinct. The merging logic was for adjacent blocks of same type.

                // NOTE: To support correct selection of individual blocks, strictly we shouldn't merge distinct blocks.
                // But if they are adjacent and same type, they might be effectively one region.
                // For now, we allow merging to keep visual performance, accepting that clicking a merged segment 
                // selects the first block of that segment.

                if (last.Type == newSegment.Type)
                {
                    double gap = newSegment.OffsetPercentage - (last.OffsetPercentage + last.HeightPercentage);

                    if (gap < threshold)
                    {
                        double newEnd = newSegment.OffsetPercentage + newSegment.HeightPercentage;
                        last.HeightPercentage = newEnd - last.OffsetPercentage;
                        // Keep last.Block as the primary target for the merged segment
                        return;
                    }
                }
            }

            list.Add(newSegment);
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