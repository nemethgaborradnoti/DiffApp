using DiffApp.Models;
using System.Collections.Generic;

namespace DiffApp.ViewModels
{
    public class ComparisonViewModel : ViewModelBase
    {
        private readonly ComparisonResult _comparisonResult;

        public ComparisonResult ComparisonResult => _comparisonResult;

        public IReadOnlyList<ChangeLine> UnifiedLines { get; }

        public ComparisonViewModel(ComparisonResult comparisonResult)
        {
            _comparisonResult = comparisonResult;
            UnifiedLines = CreateUnifiedLines();
        }

        private List<ChangeLine> CreateUnifiedLines()
        {
            var lines = new List<ChangeLine>();
            foreach (var block in _comparisonResult.Blocks)
            {
                switch (block.Kind)
                {
                    case BlockType.Unchanged:
                        lines.AddRange(block.OldLines);
                        break;
                    case BlockType.Removed:
                        lines.AddRange(block.OldLines);
                        break;
                    case BlockType.Added:
                        lines.AddRange(block.NewLines);
                        break;
                    case BlockType.Modified:
                        lines.AddRange(block.OldLines);
                        lines.AddRange(block.NewLines);
                        break;
                }
            }
            return lines;
        }
    }
}