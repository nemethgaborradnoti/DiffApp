using DiffApp.Models;
using DiffApp.Services.Interfaces;
using System;
using System.Windows.Input;

namespace DiffApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly IMergeService _mergeService;
        private readonly InputViewModel _inputViewModel;

        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand CopyTextCommand { get; }

        public MainViewModel(IComparisonService comparisonService, IMergeService mergeService, InputViewModel inputViewModel)
        {
            _comparisonService = comparisonService ?? throw new ArgumentNullException(nameof(comparisonService));
            _mergeService = mergeService ?? throw new ArgumentNullException(nameof(mergeService));
            _inputViewModel = inputViewModel ?? throw new ArgumentNullException(nameof(inputViewModel));

            // Setup Navigation
            _inputViewModel.CompareRequested += OnCompareRequested;

            // Start with Input View
            _currentViewModel = _inputViewModel;

            // Global commands (like Copy) can remain here or move to specific VMs. 
            // For now, let's keep Copy here or handle in Views. 
            // Actually, CopyCommand was used in Views via RelativeSource. 
            // We will need to expose it or let views handle it. 
            // For simplicity in Phase 4, we'll let Views handle clipboard directly or re-bind.
            // But to avoid breaking existing bindings in ComparisonView too much:
            CopyTextCommand = new Helpers.RelayCommand(CopyText);
        }

        private void OnCompareRequested(object? sender, EventArgs e)
        {
            PerformComparison();
        }

        private void PerformComparison()
        {
            var settings = new CompareSettings
            {
                IgnoreWhitespace = _inputViewModel.IgnoreWhitespace,
                Precision = _inputViewModel.Precision
            };

            var result = _comparisonService.Compare(_inputViewModel.LeftText, _inputViewModel.RightText, settings);

            var comparisonVM = new ComparisonViewModel(result);
            comparisonVM.BackRequested += (s, args) => CurrentViewModel = _inputViewModel;
            comparisonVM.MergeRequested += OnMergeRequested;

            CurrentViewModel = comparisonVM;
        }

        private void OnMergeRequested(object? sender, MergeRequestArgs e)
        {
            if (e.Direction == MergeDirection.LeftToRight)
            {
                _inputViewModel.RightText = _mergeService.MergeBlock(_inputViewModel.RightText, e.Block, e.Direction);
            }
            else
            {
                _inputViewModel.LeftText = _mergeService.MergeBlock(_inputViewModel.LeftText, e.Block, e.Direction);
            }

            // Re-run comparison to refresh the view
            PerformComparison();
        }

        private void CopyText(object? parameter)
        {
            if (parameter is Side side)
            {
                string textToCopy = side == Side.Old ? _inputViewModel.LeftText : _inputViewModel.RightText;

                if (!string.IsNullOrEmpty(textToCopy))
                {
                    try
                    {
                        System.Windows.Clipboard.SetText(textToCopy);
                    }
                    catch (Exception)
                    {
                        // Ignore clipboard errors
                    }
                }
            }
        }
    }
}