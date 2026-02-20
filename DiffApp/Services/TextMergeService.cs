using DiffApp.Models;
using DiffApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiffApp.Services
{
    public class TextMergeService : ITextMergeService
    {
        public string MergeBlock(string targetText, DiffHunk hunk, MergeDirection direction)
        {
            var lines = GetLines(targetText);

            if (direction == MergeDirection.LeftToRight)
            {
                if (hunk.Kind == HunkKind.Added)
                {
                    RemoveLines(lines, hunk.NewLines);
                }
                else if (hunk.Kind == HunkKind.Removed)
                {
                    var textToInsert = hunk.OldLines.Select(l => GetText(l)).ToList();
                    InsertLines(lines, hunk.StartIndexNew, textToInsert);
                }
                else if (hunk.Kind == HunkKind.Modified)
                {
                    ReplaceLines(lines, hunk.NewLines, hunk.OldLines);
                }
            }
            else
            {
                if (hunk.Kind == HunkKind.Added)
                {
                    var textToInsert = hunk.NewLines.Select(l => GetText(l)).ToList();
                    InsertLines(lines, hunk.StartIndexOld, textToInsert);
                }
                else if (hunk.Kind == HunkKind.Removed)
                {
                    RemoveLines(lines, hunk.OldLines);
                }
                else if (hunk.Kind == HunkKind.Modified)
                {
                    ReplaceLines(lines, hunk.OldLines, hunk.NewLines);
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        public string MergeLine(string targetText, DiffLine line, int targetLineIndex, MergeDirection direction)
        {
            throw new NotImplementedException("Line-level merge requires strict context management. Block merge is recommended.");
        }

        private List<string> GetLines(string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();
            return text.Replace("\r\n", "\n").Split('\n').ToList();
        }

        private string GetText(DiffLine line)
        {
            return string.Join("", line.Pieces.Select(p => p.Text));
        }

        private void RemoveLines(List<string> textLines, List<DiffLine> linesToRemove)
        {
            var indices = linesToRemove
                .Where(l => l.LineNumber.HasValue)
                .Select(l => l.LineNumber.Value - 1)
                .OrderByDescending(i => i)
                .ToList();

            foreach (var index in indices)
            {
                if (index >= 0 && index < textLines.Count)
                {
                    textLines.RemoveAt(index);
                }
            }
        }

        private void InsertLines(List<string> textLines, int insertIndex, List<string> linesToInsert)
        {
            if (insertIndex > textLines.Count) insertIndex = textLines.Count;
            if (insertIndex < 0) insertIndex = 0;

            textLines.InsertRange(insertIndex, linesToInsert);
        }

        private void ReplaceLines(List<string> textLines, List<DiffLine> targets, List<DiffLine> sources)
        {
            var indices = targets
                .Where(l => l.LineNumber.HasValue)
                .Select(l => l.LineNumber.Value - 1)
                .OrderBy(i => i)
                .ToList();

            if (indices.Count == 0) return;

            int startIndex = indices.First();
            int countToRemove = indices.Count;

            if (startIndex >= 0 && startIndex < textLines.Count)
            {
                int actualRemovable = Math.Min(countToRemove, textLines.Count - startIndex);
                textLines.RemoveRange(startIndex, actualRemovable);

                var newContent = sources.Select(l => GetText(l)).ToList();
                textLines.InsertRange(startIndex, newContent);
            }
        }
    }
}