namespace DiffApp.Models
{
    public class MinimapSegment
    {
        public double OffsetPercentage { get; set; }
        public double HeightPercentage { get; set; }
        public BlockType Type { get; set; }
        public Side Side { get; set; }
        public int TargetLineIndex { get; set; }
        public ChangeBlock Block { get; set; }
    }
}