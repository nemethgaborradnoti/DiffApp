namespace DiffApp.ViewModels
{
    public class HistoryItemViewModel : ViewModelBase
    {
        private readonly DiffHistoryItem _model;

        public Guid Id => _model.Id;
        public string OriginalFull => _model.OriginalText;
        public string ModifiedFull => _model.ModifiedText;
        public DateTime CreatedAt => _model.CreatedAt;

        // Removing truncation logic here, View will handle layout
        public string DisplayOriginal => _model.OriginalText;
        public string DisplayModified => _model.ModifiedText;

        private string _relativeTime;
        public string RelativeTime
        {
            get => _relativeTime;
            set => SetProperty(ref _relativeTime, value);
        }

        private FontWeight _timeFontWeight;
        public FontWeight TimeFontWeight
        {
            get => _timeFontWeight;
            set => SetProperty(ref _timeFontWeight, value);
        }

        public bool IsBookmarked
        {
            get => _model.IsBookmarked;
            set
            {
                if (_model.IsBookmarked != value)
                {
                    _model.IsBookmarked = value;
                    OnPropertyChanged();
                }
            }
        }

        public HistoryItemViewModel(DiffHistoryItem model)
        {
            _model = model;
            UpdateTimeDisplay();
        }

        public void RefreshTime()
        {
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            var span = DateTime.Now - CreatedAt;

            // Set Font Weight based on age (< 1 hour = Bold)
            if (span.TotalHours < 1)
            {
                TimeFontWeight = FontWeights.Bold;
            }
            else
            {
                TimeFontWeight = FontWeights.Normal;
            }

            // Set Relative Time Text
            if (span.TotalMinutes < 1)
            {
                RelativeTime = "Just now";
            }
            else if (span.TotalMinutes < 60)
            {
                RelativeTime = $"{(int)span.TotalMinutes} mins ago";
            }
            else if (span.TotalHours < 24)
            {
                RelativeTime = $"{(int)span.TotalHours} hours ago";
            }
            else if (span.TotalDays < 7)
            {
                RelativeTime = $"{(int)span.TotalDays} days ago";
            }
            else
            {
                RelativeTime = CreatedAt.ToShortDateString();
            }
        }
    }
}