using System;
using System.Collections.Generic;

namespace DiffApp.Models
{
    public class ChangeBlock
    {
        public Guid Id { get; } = Guid.NewGuid();
        public BlockType Kind { get; set; }
        public List<ChangeLine> OldLines { get; } = new();
        public List<ChangeLine> NewLines { get; } = new();

        public bool IsMergeable => Kind != BlockType.Unchanged;

        public int StartIndexOld { get; set; }
        public int StartIndexNew { get; set; }
    }
}