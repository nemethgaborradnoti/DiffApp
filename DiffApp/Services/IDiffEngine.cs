using DiffApp.Models;

namespace DiffApp.Services
{
    public interface IDiffEngine
    {
        DiffResult Compare(string oldText, string newText, DiffOptions options);
    }
}