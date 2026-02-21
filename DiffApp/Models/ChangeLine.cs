using System.Collections.Generic;

namespace DiffApp.Models
{
    public class ChangeLine
    {
        public IReadOnlyList<TextFragment> Fragments { get; set; } = new List<TextFragment>();
        public int? LineNumber { get; set; }
        public DiffChangeType Kind { get; set; } = DiffChangeType.Unchanged;
    }
}