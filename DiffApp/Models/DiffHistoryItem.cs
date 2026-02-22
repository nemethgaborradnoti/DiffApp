namespace DiffApp.Models
{
    public class DiffHistoryItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OriginalText { get; set; } = string.Empty;
        public string ModifiedText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}