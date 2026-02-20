using DiffApp.Models;
using System.Collections.Generic;

namespace DiffApp.ViewModels
{
    public class DiffViewModel : ViewModelBase
    {
        private readonly DiffResult _diffResult;

        public DiffResult DiffResult => _diffResult;

        public IReadOnlyList<ChangeLine> UnifiedLines { get; }

        public DiffViewModel(DiffResult diffResult)
        {
            _diffResult = diffResult;
            UnifiedLines = CreateUnifiedLines();
        }

        private List<ChangeLine> CreateUnifiedLines()
        {
            var lines = new List<ChangeLine>();
            foreach (var block in _diffResult.Blocks)
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