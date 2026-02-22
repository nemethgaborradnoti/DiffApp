namespace DiffApp.Models
{
    public class AppSettings
    {
        public bool IsWordWrapEnabled { get; set; } = true;
        public bool IgnoreWhitespace { get; set; } = false;
        public PrecisionLevel Precision { get; set; } = PrecisionLevel.Word;
        public ViewMode ViewMode { get; set; } = ViewMode.Split;

        public double WindowTop { get; set; } = double.NaN;
        public double WindowLeft { get; set; } = double.NaN;
        public double WindowWidth { get; set; } = 1200;
        public double WindowHeight { get; set; } = 800;
        public WindowState WindowState { get; set; } = WindowState.Normal;
    }
}