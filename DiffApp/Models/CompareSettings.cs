namespace DiffApp.Models
{
    public enum PrecisionLevel
    {
        Word,
        Character
    }

    public class CompareSettings
    {
        public bool IgnoreWhitespace { get; set; }
        public PrecisionLevel Precision { get; set; } = PrecisionLevel.Word;
    }
}