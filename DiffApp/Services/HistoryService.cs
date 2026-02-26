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

                var existingItem = col.FindOne(x => x.OriginalText == original && x.ModifiedText == modified);

                if (existingItem != null)
                {
                    existingItem.CreatedAt = DateTime.Now;
                    col.Update(existingItem);
                }
                else
                {
                    var item = new DiffHistoryItem
                    {
                        OriginalText = original,
                        ModifiedText = modified,
                        CreatedAt = DateTime.Now,
                        IsBookmarked = false
                    };

                    col.Insert(item);
                }

                col.EnsureIndex(x => x.CreatedAt);
                col.EnsureIndex(x => x.IsBookmarked);
            });
        }

        public Task<IEnumerable<DiffHistoryItem>> GetAllAsync()
        {
            return Task.Run(() =>
            {
                using var db = new LiteDatabase(_dbPath);
                var col = db.GetCollection<DiffHistoryItem>(CollectionName);

                var allItems = col.FindAll();

                return (IEnumerable<DiffHistoryItem>)allItems
                    .OrderByDescending(x => x.IsBookmarked)
                    .ThenByDescending(x => x.CreatedAt)
                    .ToList();
            });
        }

        public Task UpdateBookmarkAsync(Guid id, bool isBookmarked)
        {
            return Task.Run(() =>
            {
                using var db = new LiteDatabase(_dbPath);
                var col = db.GetCollection<DiffHistoryItem>(CollectionName);

                var item = col.FindById(id);
                if (item != null)
                {
                    item.IsBookmarked = isBookmarked;
                    col.Update(item);
                }
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