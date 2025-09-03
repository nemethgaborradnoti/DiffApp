using DiffPlex.DiffBuilder.Model;

namespace DiffApp.Models
{
    public class DiffPiece
    {
        public ChangeType Kind { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
