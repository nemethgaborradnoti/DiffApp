using DiffApp.Models;

namespace DiffApp.Services.Interfaces
{
    public interface IComparisonService
    {
        ComparisonResult Compare(string oldText, string newText, CompareSettings settings);
    }
}