using DiffPlex.DiffBuilder.Model;

namespace DiffApp.Models
{
    public class TextFragment
    {
        public ChangeType Kind { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}