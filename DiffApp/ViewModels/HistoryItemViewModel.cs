using DiffApp.Models;
using System.Text;

namespace DiffApp.ViewModels
{
    public class HistoryItemViewModel : ViewModelBase
    {
        private readonly DiffHistoryItem _model;

        public Guid Id => _model.Id;
        public string OriginalFull => _model.OriginalText;
        public string ModifiedFull => _model.ModifiedText;
        public DateTime CreatedAt => _model.CreatedAt;

        public string DisplayOriginal => _model.OriginalText;
        public string DisplayModified => _model.ModifiedText;

        public string OriginalPreview => GeneratePreview(OriginalFull);
        public string ModifiedPreview => GeneratePreview(ModifiedFull);

        private string _relativeTime;
        public string RelativeTime
        {
            get => _relativeTime;
            set => SetProperty(ref _relativeTime, value);
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
            _relativeTime = string.Empty;
            UpdateTimeDisplay();
        }

        public void RefreshTime()
        {
            UpdateTimeDisplay();
        }

        private void UpdateTimeDisplay()
        {
            var span = DateTime.Now - CreatedAt;

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

        private string GeneratePreview(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var lines = text.Replace("\r\n", "\n").Split('\n');
            var sb = new StringBuilder();
            int lineCount = 0;

            foreach (var line in lines)
            {
                if (lineCount >= 3)
                    break;

                sb.AppendLine(line);
                lineCount++;
            }

            return sb.ToString().TrimEnd();
        }
    }
}