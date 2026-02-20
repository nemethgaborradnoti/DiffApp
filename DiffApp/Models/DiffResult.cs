using System.Collections.Generic;

namespace DiffApp.Models
{
    public class DiffResult
    {
        public IReadOnlyList<ChangeBlock> Blocks { get; }

        public DiffResult(IReadOnlyList<ChangeBlock> blocks)
        {
            Blocks = blocks;
        }
    }
}