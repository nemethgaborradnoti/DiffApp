namespace DiffApp.Models
{
    public class AppSettings
    {
        public bool IsWordWrapEnabled { get; set; } = true;
        public bool IgnoreWhitespace { get; set; } = false;
        public PrecisionLevel Precision { get; set; } = PrecisionLevel.Word;
        public ViewMode ViewMode { get; set; } = ViewMode.Split;
    }
}