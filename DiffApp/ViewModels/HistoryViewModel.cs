using DiffApp.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DiffApp.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        private readonly IHistoryService _historyService;
        private readonly IDialogService _dialogService;
        private ObservableCollection<HistoryItemViewModel> _historyItems = new();
        private bool _isLoading;

        public event EventHandler<DiffHistoryItem>? RestoreRequested;

        public ObservableCollection<HistoryItemViewModel> HistoryItems
        {
            get => _historyItems;
            set
            {
                if (SetProperty(ref _historyItems, value))
                {
                    OnPropertyChanged(nameof(HasItems));
                }
            }
        }

        public bool HasItems => HistoryItems.Count > 0;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoadHistoryCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand DeleteAllCommand { get; }
        public ICommand RestoreItemCommand { get; }

        public HistoryViewModel(IHistoryService historyService, IDialogService dialogService)
        {
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            LoadHistoryCommand = new RelayCommand(async _ => await LoadHistoryAsync());
            DeleteItemCommand = new RelayCommand(async p => await DeleteItemAsync(p));
            DeleteAllCommand = new RelayCommand(async _ => await DeleteAllAsync());
            RestoreItemCommand = new RelayCommand(RestoreItem);
        }

        public async Task LoadHistoryAsync()
        {
            IsLoading = true;
            try
            {
                var items = await _historyService.GetAllAsync();
                var viewModels = items.Select(x => new HistoryItemViewModel(x));

                HistoryItems = new ObservableCollection<HistoryItemViewModel>(viewModels);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteItemAsync(object? parameter)
        {
            var result = _dialogService.ShowDialog(
                "Delete this item?",
                "Delete Item",
                DialogButtons.YesNo,
                DialogImage.Question);

            if (result == DialogResult.Yes)
            {
                if (parameter is HistoryItemViewModel item)
                {
                    await _historyService.DeleteAsync(item.Id);
                    HistoryItems.Remove(item);
                }
                else if (parameter is Guid id)
                {
                    await _historyService.DeleteAsync(id);
                    var itemToRemove = HistoryItems.FirstOrDefault(x => x.Id == id);
                    if (itemToRemove != null)
                    {
                        HistoryItems.Remove(itemToRemove);
                    }
                }
                OnPropertyChanged(nameof(HasItems));
            }
        }

        private async Task DeleteAllAsync()
        {
            if (HistoryItems.Count == 0) return;

            var result = _dialogService.ShowDialog(
                "You are about to delete the whole history database",
                "Delete All History",
                DialogButtons.ConfirmCancel,
                DialogImage.Warning);

            if (result == DialogResult.Confirm)
            {
                await _historyService.ClearAllAsync();
                HistoryItems.Clear();
                OnPropertyChanged(nameof(HasItems));
            }
        }

        private void RestoreItem(object? parameter)
        {
            if (parameter is HistoryItemViewModel item)
            {
                RestoreRequested?.Invoke(this, new DiffHistoryItem
                {
                    Id = item.Id,
                    OriginalText = item.OriginalFull,
                    ModifiedText = item.ModifiedFull,
                    CreatedAt = item.CreatedAt
                });
            }
        }
    }
}