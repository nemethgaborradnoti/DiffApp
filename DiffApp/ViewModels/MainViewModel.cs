using DiffApp.Helpers;
using DiffApp.Models;
using DiffApp.Services;
using System;
using System.Windows.Input;

namespace DiffApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _leftText = string.Empty;
        private string _rightText = string.Empty;
        private bool _isUnifiedMode;
        private bool _ignoreWhitespace;
        private DiffPrecision _precision = DiffPrecision.Word;
        private DiffViewModel? _diffViewModel;
        private readonly IDiffEngine _diffEngine;
        private readonly ITextMergeService _textMergeService;

        public string LeftText
        {
            get => _leftText;
            set
            {
                if (SetProperty(ref _leftText, value))
                {
                    (FindDifferenceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string RightText
        {
            get => _rightText;
            set
            {
                if (SetProperty(ref _rightText, value))
                {
                    (FindDifferenceCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsUnifiedMode
        {
            get => _isUnifiedMode;
            set => SetProperty(ref _isUnifiedMode, value);
        }

        public bool IgnoreWhitespace
        {
            get => _ignoreWhitespace;
            set => SetProperty(ref _ignoreWhitespace, value);
        }

        public DiffPrecision Precision
        {
            get => _precision;
            set => SetProperty(ref _precision, value);
        }

        public DiffViewModel? DiffViewModel
        {
            get => _diffViewModel;
            private set => SetProperty(ref _diffViewModel, value);
        }

        public ICommand FindDifferenceCommand { get; }
        public ICommand MergeBlockCommand { get; }

        public MainViewModel()
        {
            _diffEngine = new DiffEngine();
            _textMergeService = new TextMergeService();

            FindDifferenceCommand = new RelayCommand(FindDifference, CanFindDifference);
            MergeBlockCommand = new RelayCommand(MergeBlock);

            LoadSampleText();
        }

        private void FindDifference(object? parameter)
        {
            var options = new DiffOptions
            {
                IgnoreWhitespace = IgnoreWhitespace,
                Precision = Precision
            };

            var result = _diffEngine.Compare(LeftText, RightText, options);
            DiffViewModel = new DiffViewModel(result);
        }

        private bool CanFindDifference(object? parameter)
        {
            return !string.IsNullOrEmpty(LeftText) || !string.IsNullOrEmpty(RightText);
        }

        private void MergeBlock(object? parameter)
        {
            if (parameter is object[] args && args.Length == 2)
            {
                if (args[0] is DiffHunk hunk && args[1] is MergeDirection direction)
                {
                    PerformMerge(hunk, direction);
                }
            }
            // Alternative parameter passing support if needed
        }

        private void PerformMerge(DiffHunk hunk, MergeDirection direction)
        {
            if (direction == MergeDirection.LeftToRight)
            {
                // Modify Right Text based on Left
                RightText = _textMergeService.MergeBlock(RightText, hunk, direction);
            }
            else
            {
                // Modify Left Text based on Right
                LeftText = _textMergeService.MergeBlock(LeftText, hunk, direction);
            }

            // Re-run diff to update UI
            FindDifference(null);
        }

        private void LoadSampleText()
        {
            LeftText = "// Comment\nProgram code\nThis is the original text.\nIt has several lines.\n// Comment here\n// More comment\n// So much comment\nBanana\nBean\nSome lines are unique to this side.\n\n\n.\n \nwill remove this\nwill remove this";
            RightText = "Program code\nThis is the modified text.\nIt has several lines.\nBanana\nBean\nSome lines are unique to the other side.\nAnd an extra line here.\n\nThere\nWill\nBe\nSo\nMuch\nMore\nThan\nThis";
        }
    }
}