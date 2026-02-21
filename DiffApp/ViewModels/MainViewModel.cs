using DiffApp.Helpers;
using DiffApp.Models;
using DiffApp.Services.Interfaces;
using System;
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
        private bool _isWordWrapEnabled = true;
        private PrecisionLevel _precision = PrecisionLevel.Word;
        private ComparisonViewModel? _comparisonViewModel;
        private readonly IComparisonService _comparisonService;
        private readonly IMergeService _mergeService;

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

        public PrecisionLevel Precision
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

        public ComparisonViewModel? ComparisonViewModel
        {
            get => _comparisonViewModel;
            private set => SetProperty(ref _comparisonViewModel, value);
        }

        public ICommand FindDifferenceCommand { get; }
        public ICommand MergeBlockCommand { get; }
        public ICommand CopyTextCommand { get; }
        public ICommand SwapTextsCommand { get; }

        public MainViewModel(IComparisonService comparisonService, IMergeService mergeService)
        {
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));

            FindDifferenceCommand = new RelayCommand(FindDifference, CanFindDifference);
            MergeBlockCommand = new RelayCommand(MergeBlock);
            CopyTextCommand = new RelayCommand(CopyText);
            SwapTextsCommand = new RelayCommand(SwapTexts);

            LoadSampleText();
        }

        private void FindDifference(object? parameter)
        {
            var options = new CompareSettings
            {
                IgnoreWhitespace = IgnoreWhitespace,
                Precision = Precision
            };

            var result = _comparisonService.Compare(LeftText, RightText, options);
            ComparisonViewModel = new ComparisonViewModel(result);
        }

        private bool CanFindDifference(object? parameter)
        {
            return !string.IsNullOrEmpty(LeftText) || !string.IsNullOrEmpty(RightText);
        }

        private void MergeBlock(object? parameter)
        {
            if (parameter is object[] args && args.Length == 2)
            {
                if (args[0] is ChangeBlock block && args[1] is MergeDirection direction)
                {
                    PerformMerge(block, direction);
                }
            }
        }

        private void CopyText(object? parameter)
        {
            if (parameter is Side side)
            {
                string textToCopy = side == Side.Old ? LeftText : RightText;

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

        private void PerformMerge(ChangeBlock block, MergeDirection direction)
        {
            if (direction == MergeDirection.LeftToRight)
            {
                RightText = _mergeService.MergeBlock(RightText, block, direction);
            }
            else
            {
                LeftText = _mergeService.MergeBlock(LeftText, block, direction);
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