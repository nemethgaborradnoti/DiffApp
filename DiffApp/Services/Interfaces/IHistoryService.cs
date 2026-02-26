using System.Threading.Tasks;

namespace DiffApp.Services.Interfaces
{
    public interface IHistoryService
    {
        Task AddAsync(string original, string modified);
        Task<IEnumerable<DiffHistoryItem>> GetAllAsync();
        Task UpdateBookmarkAsync(Guid id, bool isBookmarked);
        Task DeleteAsync(Guid id);
        Task ClearAllAsync();
    }
}