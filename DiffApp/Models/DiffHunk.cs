using System.Collections.Generic;

namespace DiffApp.Models
{
    public class DiffHunk
    {
        public HunkKind Kind { get; set; }
        public List<DiffLine> OldLines { get; } = new();
        public List<DiffLine> NewLines { get; } = new();
    }
}

