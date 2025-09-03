using DiffApp.Models;
using System.Collections.Generic;

namespace DiffApp.ViewModels
{
    public class DiffViewModel : ViewModelBase
    {
        private readonly DiffResult _diffResult;

        public DiffResult DiffResult => _diffResult;

        public IReadOnlyList<DiffLine> UnifiedLines { get; }

        public DiffViewModel(DiffResult diffResult)
        {
            _diffResult = diffResult;
            UnifiedLines = CreateUnifiedLines();
        }

        private List<DiffLine> CreateUnifiedLines()
        {
            var lines = new List<DiffLine>();
            foreach (var hunk in _diffResult.Hunks)
            {
                switch (hunk.Kind)
                {
                    case HunkKind.Unchanged:
                        lines.AddRange(hunk.OldLines);
                        break;
                    case HunkKind.Removed:
                        lines.AddRange(hunk.OldLines);
                        break;
                    case HunkKind.Added:
                        lines.AddRange(hunk.NewLines);
                        break;
                    case HunkKind.Modified:
                        lines.AddRange(hunk.OldLines);
                        lines.AddRange(hunk.NewLines);
                        break;
                }
            }
            return lines;
        }
    }
}

