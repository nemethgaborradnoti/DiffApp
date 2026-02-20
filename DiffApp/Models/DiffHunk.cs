using System;
using System.Collections.Generic;

namespace DiffApp.Models
{
    public class DiffHunk
    {
        public Guid Id { get; } = Guid.NewGuid();
        public HunkKind Kind { get; set; }
        public List<DiffLine> OldLines { get; } = new();
        public List<DiffLine> NewLines { get; } = new();

        // Helpers for Merge Logic
        public bool IsMergeable => Kind != HunkKind.Unchanged;

        // Context indices (0-based) for where this hunk starts in the respective text documents.
        // Used to locate insertion points for imaginary blocks.
        public int StartIndexOld { get; set; }
        public int StartIndexNew { get; set; }
    }
}