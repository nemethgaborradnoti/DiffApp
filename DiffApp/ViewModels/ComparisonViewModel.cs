namespace DiffApp.ViewModels
{
    public class ComparisonViewModel : ViewModelBase
    {
        private readonly ComparisonResult _comparisonResult;
        private bool _isUnifiedMode;

        public event EventHandler<MergeRequestArgs>? MergeRequested;

        public ComparisonResult ComparisonResult => _comparisonResult;

        public IReadOnlyList<ChangeLine> UnifiedLines { get; }

        public bool IsUnifiedMode
        {
            get => _isUnifiedMode;
            set => SetProperty(ref _isUnifiedMode, value);
        }

        public ICommand MergeBlockCommand { get; }

        public ComparisonViewModel(ComparisonResult comparisonResult)
        {
            _comparisonResult = comparisonResult;
            UnifiedLines = CreateUnifiedLines();

            MergeBlockCommand = new RelayCommand(ExecuteMerge);
        }

        private void ExecuteMerge(object? parameter)
        {
            if (parameter is object[] args && args.Length == 2)
            {
                if (args[0] is ChangeBlock block && args[1] is MergeDirection direction)
                {
                    MergeRequested?.Invoke(this, new MergeRequestArgs(block, direction));
                }
            }
        }

        private List<ChangeLine> CreateUnifiedLines()
        {
            var lines = new List<ChangeLine>();
            foreach (var block in _comparisonResult.Blocks)
            {
                switch (block.Kind)
                {
                    case BlockType.Unchanged:
                        lines.AddRange(block.OldLines);
                        break;
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

    public class MergeRequestArgs : EventArgs
    {
        public ChangeBlock Block { get; }
        public MergeDirection Direction { get; }

        public MergeRequestArgs(ChangeBlock block, MergeDirection direction)
        {
            Block = block;
            Direction = direction;
        }
    }
}