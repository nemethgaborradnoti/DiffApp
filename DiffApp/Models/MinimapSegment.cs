namespace DiffApp.Models
{
    public class MinimapSegment
    {
        public double OffsetPercentage { get; set; }
        public double HeightPercentage { get; set; }
        public BlockType LeftType { get; set; }
        public BlockType RightType { get; set; }
        public int TargetLineIndex { get; set; }
        public ChangeBlock Block { get; set; }
    }
}