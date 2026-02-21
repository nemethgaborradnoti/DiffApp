using DiffApp.Models;

namespace DiffApp.Services.Interfaces
{
    public interface IMergeService
    {
        string MergeBlock(string targetText, ChangeBlock block, MergeDirection direction);
        string MergeLine(string targetText, ChangeLine line, int targetLineIndex, MergeDirection direction);
    }
}