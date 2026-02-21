using DiffApp.Helpers;
using DiffApp.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace DiffApp.ViewModels
{
    public class InputViewModel : ViewModelBase
    {
        private string _leftText = string.Empty;
        private string _rightText = string.Empty;
        private bool _ignoreWhitespace;
        private bool _isWordWrapEnabled = true;
        private PrecisionLevel _precision = PrecisionLevel.Word;

        public event EventHandler? CompareRequested;

        public string LeftText
        {
            get => _leftText;
            set => SetProperty(ref _leftText, value);
        }

        public string RightText
        {
            get => _rightText;
            set => SetProperty(ref _rightText, value);
        }

        public bool IgnoreWhitespace
        {
            get => _ignoreWhitespace;
            set => SetProperty(ref _ignoreWhitespace, value);
        }

        public bool IsWordWrapEnabled
        {
            get => _isWordWrapEnabled;
            set => SetProperty(ref _isWordWrapEnabled, value);
        }

        public PrecisionLevel Precision
        {
            get => _precision;
            set => SetProperty(ref _precision, value);
        }

        public ICommand SwapTextsCommand { get; }
        public ICommand FindDifferenceCommand { get; }

        public InputViewModel()
        {
            SwapTextsCommand = new RelayCommand(SwapTexts);
            FindDifferenceCommand = new RelayCommand(OnFindDifference, CanFindDifference);

            // Default sample text
            LoadSampleText();
        }

        private void OnFindDifference(object? parameter)
        {
            CompareRequested?.Invoke(this, EventArgs.Empty);
        }

        private bool CanFindDifference(object? parameter)
        {
            return !string.IsNullOrEmpty(LeftText) || !string.IsNullOrEmpty(RightText);
        }

        private void SwapTexts(object? parameter)
        {
            string temp = LeftText;
            LeftText = RightText;
            RightText = temp;
        }

        private void LoadSampleText()
        {
            LeftText = "// Comment\nProgram code\nThis is the original text.\nIt has several lines.\n// Comment here\n// More comment\n// So much comment\nBanana\nBean\nSome lines are unique to this side.\n\n\n.\n \nwill remove this\nwill remove this";
            RightText = "Program code\nThis is the modified text.\nIt has several lines.\nBanana\nBean\nSome lines are unique to the other side.\nAnd an extra line here.\n\nThere\nWill\nBe\nSo\nMuch\nMore\nThan\nThis";
        }
    }
}