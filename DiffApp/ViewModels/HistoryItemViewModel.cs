namespace DiffApp.ViewModels
{
    public class HistoryItemViewModel : ViewModelBase
    {
        private readonly DiffHistoryItem _model;

        public Guid Id => _model.Id;
        public string OriginalFull => _model.OriginalText;
        public string ModifiedFull => _model.ModifiedText;
        public DateTime CreatedAt => _model.CreatedAt;

        public string DisplayOriginal => Truncate(_model.OriginalText);
        public string DisplayModified => Truncate(_model.ModifiedText);

        private string _relativeTime;
        public string RelativeTime
        {
            get => _relativeTime;
            set => SetProperty(ref _relativeTime, value);
        }

        public HistoryItemViewModel(DiffHistoryItem model)
        {
            _model = model;
            _relativeTime = GetRelativeTime();
        }

        public void RefreshTime()
        {
            RelativeTime = GetRelativeTime();
        }

        private string Truncate(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var singleLine = text.Replace("\r", " ").Replace("\n", " ");
            if (singleLine.Length > 60)
            {
                return singleLine.Substring(0, 60) + "...";
            }
            return singleLine;
        }

        private string GetRelativeTime()
        {
            var span = DateTime.Now - CreatedAt;

            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} mins ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} days ago";

            return CreatedAt.ToShortDateString();
        }
    }
}