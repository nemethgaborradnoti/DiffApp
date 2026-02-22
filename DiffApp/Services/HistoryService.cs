using LiteDB;
using System.IO;
using System.Threading.Tasks;

namespace DiffApp.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly string _dbPath;
        private const string CollectionName = "history";

        public HistoryService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiffApp");
            Directory.CreateDirectory(appDataPath);
            _dbPath = Path.Combine(appDataPath, "history.db");
        }

        public Task AddAsync(string original, string modified)
        {
            return Task.Run(() =>
            {
                using var db = new LiteDatabase(_dbPath);
                var col = db.GetCollection<DiffHistoryItem>(CollectionName);

                var item = new DiffHistoryItem
                {
                    OriginalText = original,
                    ModifiedText = modified,
                    CreatedAt = DateTime.Now
                };

                col.Insert(item);
                col.EnsureIndex(x => x.CreatedAt);
            });
        }

        public Task<IEnumerable<DiffHistoryItem>> GetAllAsync()
        {
            return Task.Run(() =>
            {
                using var db = new LiteDatabase(_dbPath);
                var col = db.GetCollection<DiffHistoryItem>(CollectionName);

                // Return explicitly as list to avoid disposing issues with deferred execution
                return (IEnumerable<DiffHistoryItem>)col.FindAll().OrderByDescending(x => x.CreatedAt).ToList();
            });
        }

        public Task DeleteAsync(Guid id)
        {
            return Task.Run(() =>
            {
                using var db = new LiteDatabase(_dbPath);
                var col = db.GetCollection<DiffHistoryItem>(CollectionName);
                col.Delete(id);
            });
        }

        public Task ClearAllAsync()
        {
            return Task.Run(() =>
            {
                using var db = new LiteDatabase(_dbPath);
                db.DropCollection(CollectionName);
            });
        }
    }
}