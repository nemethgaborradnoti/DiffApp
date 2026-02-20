using DiffApp.Models;
using System.Collections.Generic;

namespace DiffApp.Services
{
    public interface ITextMergeService
    {
        string MergeBlock(string targetText, DiffHunk hunk, MergeDirection direction);
        string MergeLine(string targetText, DiffLine line, int targetLineIndex, MergeDirection direction);
    }
}