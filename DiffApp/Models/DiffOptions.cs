namespace DiffApp.Models
{
    public enum DiffPrecision
    {
        Word,
        Character
    }

    public class DiffOptions
    {
        public bool IgnoreWhitespace { get; set; }
        public DiffPrecision Precision { get; set; } = DiffPrecision.Word;
    }
}