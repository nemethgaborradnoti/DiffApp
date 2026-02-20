using DiffApp.Helpers;
using DiffApp.Models;
using DiffApp.Services;
using DiffApp.Services.Interfaces;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DiffApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _leftText = string.Empty;
        private string _rightText = string.Empty;
        private bool _isUnifiedMode;
        private bool _ignoreWhitespace;
        private bool _isWordWrapEnabled = true; // Default to true
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

        public bool IsWordWrapEnabled
        {
            get => _isWordWrapEnabled;
            set => SetProperty(ref _isWordWrapEnabled, value);
        }

        public bool IgnoreWhitespace
        {
            get => _ignoreWhitespace;
            set
            {
                if (SetProperty(ref _ignoreWhitespace, value))
                {
                    if (CanFindDifference(null)) FindDifference(null);
                }
            }
        }

        public DiffPrecision Precision
        {
            get => _precision;
            set
            {
                if (SetProperty(ref _precision, value))
                {
                    if (CanFindDifference(null)) FindDifference(null);
                }
            }
        }

        public DiffViewModel? DiffViewModel
        {
            get => _diffViewModel;
            private set => SetProperty(ref _diffViewModel, value);
        }

        public ICommand FindDifferenceCommand { get; }
        public ICommand MergeBlockCommand { get; }
        public ICommand CopyTextCommand { get; }
        public ICommand SwapTextsCommand { get; }

        public MainViewModel()
        {
            _diffEngine = new DiffEngine();
            _textMergeService = new TextMergeService();

            FindDifferenceCommand = new RelayCommand(FindDifference, CanFindDifference);
            MergeBlockCommand = new RelayCommand(MergeBlock);
            CopyTextCommand = new RelayCommand(CopyText);
            SwapTextsCommand = new RelayCommand(SwapTexts);

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
        }

        private void CopyText(object? parameter)
        {
            if (parameter is DiffSide side)
            {
                string textToCopy = side == DiffSide.Old ? LeftText : RightText;

                if (!string.IsNullOrEmpty(textToCopy))
                {
                    try
                    {
                        Clipboard.SetText(textToCopy);
                    }
                    catch (Exception)
                    {
                        // Handle clipboard exception if necessary
                    }
                }
            }
        }

        private void SwapTexts(object? parameter)
        {
            string temp = LeftText;
            LeftText = RightText;
            RightText = temp;

            if (CanFindDifference(null))
            {
                FindDifference(null);
            }
        }

        private void PerformMerge(DiffHunk hunk, MergeDirection direction)
        {
            if (direction == MergeDirection.LeftToRight)
            {
                RightText = _textMergeService.MergeBlock(RightText, hunk, direction);
            }
            else
            {
                LeftText = _textMergeService.MergeBlock(LeftText, hunk, direction);
            }

            FindDifference(null);
        }

        private void LoadSampleText()
        {
            LeftText = "// Comment\nProgram code\nThis is the original text.\nIt has several lines.\n// Comment here\n// More comment\n// So much comment\nBanana\nBean\nSome lines are unique to this side.\n\n\n.\n \nwill remove this\nwill remove this";
            RightText = "Program code\nThis is the modified text.\nIt has several lines.\nBanana\nBean\nSome lines are unique to the other side.\nAnd an extra line here.\n\nThere\nWill\nBe\nSo\nMuch\nMore\nThan\nThis";
        }
    }
}