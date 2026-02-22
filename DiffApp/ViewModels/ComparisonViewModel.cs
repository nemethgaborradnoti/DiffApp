using System.Collections.ObjectModel;
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
        private ObservableCollection<DiffLineViewModel> _diffLines;
        private bool _isUnifiedMode;
        private string _leftResultText;
        private string _rightResultText;
        private bool _isBusy;

        public ComparisonResult ComparisonResult
        {
            get => _comparisonResult;
            private set => SetProperty(ref _comparisonResult, value);
        }

        public ObservableCollection<DiffLineViewModel> DiffLines
        {
            get => _diffLines;
            private set => SetProperty(ref _diffLines, value);
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
                        if (value)
                        {
                            FlattenUnifiedResults(_comparisonResult);
                        }
                        else
                        {
                            FlattenSideBySideResults(_comparisonResult);
                        }
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

            FlattenSideBySideResults(initialResult);

            MergeBlockCommand = new RelayCommand(ExecuteMerge);
            SelectBlockCommand = new RelayCommand(SelectBlock);
            DeselectAllCommand = new RelayCommand(_ => DeselectAll());
        }

        private int GetLineCount(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return text.Replace("\r\n", "\n").Split('\n').Length;
        }

        private void FlattenSideBySideResults(ComparisonResult result)
        {
            var flatList = new List<DiffLineViewModel>();

            if (result?.Blocks != null)
            {
                foreach (var block in result.Blocks)
                {
                    int oldLinesCount = block.OldLines.Count;
                    int newLinesCount = block.NewLines.Count;
                    int maxCount = Math.Max(oldLinesCount, newLinesCount);

                    for (int i = 0; i < maxCount; i++)
                    {
                        var leftLine = i < oldLinesCount ? block.OldLines[i] : null;
                        var rightLine = i < newLinesCount ? block.NewLines[i] : null;
                        bool isFirst = (i == 0);
                        bool isLast = (i == maxCount - 1);

                        flatList.Add(new DiffLineViewModel(block, leftLine, rightLine, isFirst, isLast));
                    }
                }
            }

            DiffLines = new ObservableCollection<DiffLineViewModel>(flatList);
        }

        private void FlattenUnifiedResults(ComparisonResult result)
        {
            var flatList = new List<DiffLineViewModel>();

            if (result?.Blocks != null)
            {
                foreach (var block in result.Blocks)
                {
                    int totalLinesInBlock = 0;
                    if (block.Kind == BlockType.Modified)
                    {
                        totalLinesInBlock = block.OldLines.Count + block.NewLines.Count;
                        int current = 0;

                        foreach (var line in block.OldLines)
                        {
                            bool isFirst = (current == 0);
                            bool isLast = (current == totalLinesInBlock - 1);
                            flatList.Add(new DiffLineViewModel(block, line, null, isFirst, isLast));
                            current++;
                        }
                        foreach (var line in block.NewLines)
                        {
                            bool isFirst = (current == 0);
                            bool isLast = (current == totalLinesInBlock - 1);
                            flatList.Add(new DiffLineViewModel(block, null, line, isFirst, isLast));
                            current++;
                        }
                    }
                    else if (block.Kind == BlockType.Added)
                    {
                        totalLinesInBlock = block.NewLines.Count;
                        for (int i = 0; i < totalLinesInBlock; i++)
                        {
                            bool isFirst = (i == 0);
                            bool isLast = (i == totalLinesInBlock - 1);
                            flatList.Add(new DiffLineViewModel(block, null, block.NewLines[i], isFirst, isLast));
                        }
                    }
                    else if (block.Kind == BlockType.Removed)
                    {
                        totalLinesInBlock = block.OldLines.Count;
                        for (int i = 0; i < totalLinesInBlock; i++)
                        {
                            bool isFirst = (i == 0);
                            bool isLast = (i == totalLinesInBlock - 1);
                            flatList.Add(new DiffLineViewModel(block, block.OldLines[i], null, isFirst, isLast));
                        }
                    }
                    else
                    {
                        totalLinesInBlock = block.OldLines.Count;
                        for (int i = 0; i < totalLinesInBlock; i++)
                        {
                            bool isFirst = (i == 0);
                            bool isLast = (i == totalLinesInBlock - 1);
                            flatList.Add(new DiffLineViewModel(block, block.OldLines[i], block.NewLines[i], isFirst, isLast));
                        }
                    }
                }
            }

            DiffLines = new ObservableCollection<DiffLineViewModel>(flatList);
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

                if (IsUnifiedMode)
                {
                    FlattenUnifiedResults(result);
                }
                else
                {
                    FlattenSideBySideResults(result);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}