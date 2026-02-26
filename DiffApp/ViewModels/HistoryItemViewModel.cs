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

        public int OriginalLineCount { get; }
        public int ModifiedLineCount { get; }

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

            OriginalLineCount = GetLineCount(_model.OriginalText);
            ModifiedLineCount = GetLineCount(_model.ModifiedText);

            UpdateTimeDisplay();
        }

        public void RefreshTime()
        {
            UpdateTimeDisplay();
        }

        private int GetLineCount(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int count = 1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    count++;
                }
            }
            return count;
        }

        private string GetResourceString(string key, params object[] args)
        {
            if (Application.Current?.Resources[key] is string format)
            {
                return args.Length > 0 ? string.Format(format, args) : format;
            }
            return key;
        }

        private void UpdateTimeDisplay()
        {
            var span = DateTime.Now - CreatedAt;

            if (span.TotalMinutes < 1)
            {
                RelativeTime = GetResourceString("Time_JustNow");
            }
            else if (span.TotalMinutes < 60)
            {
                RelativeTime = GetResourceString("Time_MinsAgo", (int)span.TotalMinutes);
            }
            else if (span.TotalHours < 24)
            {
                RelativeTime = GetResourceString("Time_HoursAgo", (int)span.TotalHours);
            }
            else if (span.TotalDays < 7)
            {
                RelativeTime = GetResourceString("Time_DaysAgo", (int)span.TotalDays);
            }
            else
            {
                RelativeTime = CreatedAt.ToShortDateString();
            }
        }

        private string GeneratePreview(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var lines = text.Replace("\r\n", "\n").Split('\n');
            var previewLines = new List<string>();
            int maxLines = 3;

            for (int i = 0; i < lines.Length && i < maxLines; i++)
            {
                var line = lines[i];
                if (string.IsNullOrEmpty(line))
                {
                    previewLines.Add(" ");
                }
                else
                {
                    previewLines.Add(line);
                }
            }

            return string.Join(Environment.NewLine, previewLines);
        }
    }
}