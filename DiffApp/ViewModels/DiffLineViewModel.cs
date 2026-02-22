using System.ComponentModel;

namespace DiffApp.ViewModels
{
    public class DiffLineViewModel : ViewModelBase
    {
        private readonly ChangeBlock _parentBlock;

        public DiffLineViewModel(ChangeBlock parentBlock, ChangeLine? leftLine, ChangeLine? rightLine, bool isFirstLine, bool isLastLine)
        {
            _parentBlock = parentBlock ?? throw new ArgumentNullException(nameof(parentBlock));
            LeftLine = leftLine;
            RightLine = rightLine;
            IsFirstLine = isFirstLine;
            IsLastLine = isLastLine;

            _parentBlock.PropertyChanged += OnParentBlockPropertyChanged;
        }

        public ChangeLine? LeftLine { get; }
        public ChangeLine? RightLine { get; }
        public bool IsFirstLine { get; }
        public bool IsLastLine { get; }

        public ChangeBlock ParentBlock => _parentBlock;

        public bool IsBlockSelected
        {
            get => _parentBlock.IsSelected;
            set
            {
                if (_parentBlock.IsSelected != value)
                {
                    _parentBlock.IsSelected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowMergeControls));
                }
            }
        }

        public bool IsBlockHovered
        {
            get => _parentBlock.IsHovered;
            set
            {
                if (_parentBlock.IsHovered != value)
                {
                    _parentBlock.IsHovered = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowMergeControls => IsBlockSelected && IsLastLine && IsMergeable;

        public bool IsMergeable => _parentBlock.IsMergeable;
        public BlockType BlockKind => _parentBlock.Kind;

        private void OnParentBlockPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChangeBlock.IsSelected))
            {
                OnPropertyChanged(nameof(IsBlockSelected));
                OnPropertyChanged(nameof(ShowMergeControls));
            }
            else if (e.PropertyName == nameof(ChangeBlock.IsHovered))
            {
                OnPropertyChanged(nameof(IsBlockHovered));
            }
        }
    }
}