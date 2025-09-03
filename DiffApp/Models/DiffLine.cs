using DiffPlex.DiffBuilder.Model;
using System.Collections.Generic;

namespace DiffApp.Models
{
    public class DiffLine
    {
        public IReadOnlyList<DiffPiece> Pieces { get; set; } = new List<DiffPiece>();
        public int? LineNumber { get; set; }
        public ChangeType Kind { get; set; } = ChangeType.Unchanged;
    }
}
