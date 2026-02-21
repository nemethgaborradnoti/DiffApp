using System.Collections.Generic;

namespace DiffApp.Models
{
    public class ComparisonResult
    {
        public IReadOnlyList<ChangeBlock> Blocks { get; }

        public ComparisonResult(IReadOnlyList<ChangeBlock> blocks)
        {
            Blocks = blocks;
        }
    }
}