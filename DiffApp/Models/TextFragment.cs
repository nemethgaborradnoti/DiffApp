namespace DiffApp.Models
{
    public class TextFragment
    {
        public DiffChangeType Kind { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}