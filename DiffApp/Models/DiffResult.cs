using System.Collections.Generic;

namespace DiffApp.Models
{
    public class DiffResult
    {
        public IReadOnlyList<DiffHunk> Hunks { get; }

        public DiffResult(IReadOnlyList<DiffHunk> hunks)
        {
            Hunks = hunks;
        }
    }
}
