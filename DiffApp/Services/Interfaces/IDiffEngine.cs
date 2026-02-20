using DiffApp.Models;

namespace DiffApp.Services.Interfaces
{
    public interface IDiffEngine
    {
        DiffResult Compare(string oldText, string newText, DiffOptions options);
    }
}