using System.Threading.Tasks;

namespace DiffApp.Services.Interfaces
{
    public interface IHistoryService
    {
        Task AddAsync(string original, string modified);
        Task<IEnumerable<DiffHistoryItem>> GetAllAsync();
        Task DeleteAsync(Guid id);
        Task ClearAllAsync();
    }
}