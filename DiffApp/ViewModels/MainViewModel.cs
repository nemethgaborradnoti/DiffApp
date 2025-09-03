using DiffApp.Helpers;
using DiffApp.Services;
using System.Windows.Input;

namespace DiffApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _leftText = string.Empty;
        private string _rightText = string.Empty;
        private bool _isUnifiedMode;
        private DiffViewModel? _diffViewModel;
        private readonly IDiffEngine _diffEngine;

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

        public DiffViewModel? DiffViewModel
        {
            get => _diffViewModel;
            private set => SetProperty(ref _diffViewModel, value);
        }

        public ICommand FindDifferenceCommand { get; }

        public MainViewModel()
        {
            _diffEngine = new DiffEngine();
            FindDifferenceCommand = new RelayCommand(FindDifference, CanFindDifference);
            LoadSampleText();
        }

        private void FindDifference(object? parameter)
        {
            var result = _diffEngine.Compare(LeftText, RightText);
            DiffViewModel = new DiffViewModel(result);
        }

        private bool CanFindDifference(object? parameter)
        {
            return !string.IsNullOrEmpty(LeftText) || !string.IsNullOrEmpty(RightText);
        }

        private void LoadSampleText()
        {
            LeftText = "// Comment\nProgram code\nThis is the original text.\nIt has several lines.\n// Comment here\n// More comment\n// So much comment\nBanana\nBean\nSome lines are unique to this side.\n\n\n.\n \nwill remove this\nwill remove this";
            RightText = "Program code\nThis is the modified text.\nIt has several lines.\nBanana\nBean\nSome lines are unique to the other side.\nAnd an extra line here.\n\nThere\nWill\nBe\nSo\nMuch\nMore\nThan\nThis";
        }
    }
}
