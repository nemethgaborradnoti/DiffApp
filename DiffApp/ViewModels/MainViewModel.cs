using DiffApp.Models;
using DiffApp.Helpers;

namespace DiffApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;

        public EditorViewModel EditorViewModel { get; }
        public HistoryViewModel HistoryViewModel { get; }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand NavigateToEditorCommand { get; }
        public ICommand NavigateToHistoryCommand { get; }
        public ICommand ToggleSettingsCommand { get; }

        public MainViewModel(
            EditorViewModel editorViewModel,
            HistoryViewModel historyViewModel)
        {
            EditorViewModel = editorViewModel ?? throw new ArgumentNullException(nameof(editorViewModel));
            HistoryViewModel = historyViewModel ?? throw new ArgumentNullException(nameof(historyViewModel));

            _currentViewModel = EditorViewModel;

            HistoryViewModel.RestoreRequested += OnHistoryRestoreRequested;

            NavigateToEditorCommand = new RelayCommand(_ => CurrentViewModel = EditorViewModel);

            NavigateToHistoryCommand = new RelayCommand(_ =>
            {
                CurrentViewModel = HistoryViewModel;
                if (HistoryViewModel.LoadHistoryCommand.CanExecute(null))
                {
                    HistoryViewModel.LoadHistoryCommand.Execute(null);
                }
            });

            ToggleSettingsCommand = new RelayCommand(_ =>
            {
                if (CurrentViewModel != EditorViewModel)
                {
                    CurrentViewModel = EditorViewModel;
                }
                EditorViewModel.IsSettingsPanelOpen = !EditorViewModel.IsSettingsPanelOpen;
            });
        }

        private void OnHistoryRestoreRequested(object? sender, DiffHistoryItem item)
        {
            CurrentViewModel = EditorViewModel;
            EditorViewModel.LoadFromHistory(item);
        }
    }
}